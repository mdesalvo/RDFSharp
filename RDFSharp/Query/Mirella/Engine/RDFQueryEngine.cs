/*
   Copyright 2012-2026 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryEngine is the engine for execution of SPARQL queries (MIRELLA).
    /// Split across partial files by responsibility; this file holds the shared evaluation
    /// state and the top-level query lifecycle (SELECT/ASK/CONSTRUCT/DESCRIBE orchestration).
    /// </summary>
    internal partial class RDFQueryEngine
    {
        #region Properties
        /// <summary>
        /// Dictionary of result tables produced by evaluation of patternGroup members
        /// </summary>
        internal Dictionary<long, List<RDFTable>> PatternGroupMemberResultTables { get; set; }

        /// <summary>
        /// Dictionary of result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<long, RDFTable> QueryMemberResultTables { get; set; }

        /// <summary>
        /// Attribute denoting an optional pattern/patternGroup/query
        /// </summary>
        internal const string IsOptional = nameof(IsOptional);

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as union
        /// </summary>
        internal const string JoinAsUnion = nameof(JoinAsUnion);

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as minus
        /// </summary>
        internal const string JoinAsMinus = nameof(JoinAsMinus);
        #endregion

        #region Ctors
        /// <summary>
        /// Initializes a query engine instance
        /// </summary>
        internal RDFQueryEngine()
        {
            PatternGroupMemberResultTables = new Dictionary<long, List<RDFTable>>();
            QueryMemberResultTables = new Dictionary<long, RDFTable>();
        }
        #endregion

        /// <summary>
        /// Evaluates the given SPARQL query on the given RDF datasource
        /// </summary>
        private RDFTable EvaluateQuery(RDFQuery query, RDFDataSource datasource)
        {
            RDFTable queryResultTable = new RDFTable();

            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Evaluate the active members of the query
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Combine intermediate results into final table
                queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList());
            }

            return queryResultTable;
        }

        /// <summary>
        /// Evaluates the given SPARQL SELECT query on the given RDF datasource
        /// </summary>
        internal RDFSelectQueryResult EvaluateSelectQuery(RDFSelectQuery selectQuery, RDFDataSource datasource)
        {
            RDFTable queryResultTable = EvaluateQuery(selectQuery, datasource);
            RDFTable finalTable = ApplyModifiers(selectQuery, queryResultTable);

            //Export to the public DataTable result, carrying the join flags onto its ExtendedProperties
            //so that consumers can read them back off the result
            DataTable selectResults = finalTable.ToDataTable();
            selectResults.ExtendedProperties[IsOptional] = finalTable.IsOptional;
            selectResults.ExtendedProperties[JoinAsUnion] = finalTable.JoinAsUnion;
            selectResults.ExtendedProperties[JoinAsMinus] = finalTable.JoinAsMinus;
            return new RDFSelectQueryResult
            {
                SelectResults = selectResults
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL DESCRIBE query on the given RDF datasource
        /// </summary>
        internal RDFDescribeQueryResult EvaluateDescribeQuery(RDFDescribeQuery describeQuery, RDFDataSource datasource)
        {
            #region Utilities
            RDFTable FillDescribeTerms(RDFTable qResultTable)
            {
                RDFTable resultTable = new RDFTable();
                if (datasource.IsFederation())
                {
                    foreach (RDFDataSource fedDataSource in ((RDFFederation)datasource).Where(fedDataSource => !fedDataSource.IsFederation() || ((RDFFederation)fedDataSource).DataSourcesCount != 0))
                        MergeTable(resultTable, DescribeTerms(describeQuery, fedDataSource, qResultTable));
                }
                else
                {
                    resultTable = DescribeTerms(describeQuery, datasource, qResultTable);
                }
                return resultTable;
            }
            #endregion

            RDFTable queryResultTable = EvaluateQuery(describeQuery, datasource);
            return new RDFDescribeQueryResult
            {
                DescribeResults = ApplyModifiers(describeQuery, FillDescribeTerms(queryResultTable)).ToDataTable()
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL CONSTRUCT query on the given RDF datasource
        /// </summary>
        internal RDFConstructQueryResult EvaluateConstructQuery(RDFConstructQuery constructQuery, RDFDataSource datasource)
        {
            RDFTable queryResultTable = EvaluateQuery(constructQuery, datasource);
            return new RDFConstructQueryResult
            {
                ConstructResults = ApplyModifiers(constructQuery, FillTemplates(constructQuery.Templates, queryResultTable, false)).ToDataTable()
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL ASK query on the given RDF datasource
        /// </summary>
        internal RDFAskQueryResult EvaluateAskQuery(RDFAskQuery askQuery, RDFDataSource datasource)
        {
            RDFTable queryResultTable = EvaluateQuery(askQuery, datasource);
            return new RDFAskQueryResult
            {
                 AskResult = queryResultTable.RowsCount > 0
            };
        }

        /// <summary>
        /// Evaluates the given list of query members against the given datasource
        /// </summary>
        internal void EvaluateQueryMembers(List<RDFQueryMember> evaluableQueryMembers, RDFDataSource datasource)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                switch (evaluableQueryMember)
                {
                    case RDFPatternGroup patternGroup:
                        //Get the intermediate result tables of the pattern group
                        EvaluatePatternGroup(patternGroup, datasource);

                        //Get the result table of the pattern group
                        FinalizePatternGroup(patternGroup);

                        //Apply the filters of the pattern group to its result table
                        ApplyFilters(patternGroup);
                        break;

                    case RDFSelectQuery subQuery:
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = subQuery.ApplyToDataSource(datasource);

                        //Make it the correct format
                        RDFTable subQueryTable = RDFTable.FromDataTable(subQueryResult.SelectResults);
                        subQueryTable.IsOptional = subQuery.IsOptional || subQueryResult.SelectResults.ExtendedProperties[IsOptional] is true;
                        subQueryTable.JoinAsUnion = subQuery.JoinAsUnion;
                        subQueryTable.JoinAsMinus = subQuery.JoinAsMinus;

                        //Save updates
                        QueryMemberResultTables[subQuery.QueryMemberID] = subQueryTable;
                        break;
                }
        }
    }
}