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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    // RDFQueryEngine (MIRELLA): CONSTRUCT/DESCRIBE template filling and term description.
    internal partial class RDFQueryEngine
    {
        /// <summary>
        /// Fills the given templates with data from the given result table<br/>
        /// (needsContext flag is true only when the caller is a store operation)
        /// </summary>
        internal RDFTable FillTemplates(List<RDFPattern> templates, RDFTable resultTable, bool needsContext)
        {
            //Create the structure of the result table
            RDFTable result = new RDFTable();
            if (needsContext)
                result.AddColumn("?CONTEXT");
            result.AddColumn("?SUBJECT");
            result.AddColumn("?PREDICATE");
            result.AddColumn("?OBJECT");

            //Initialize working variables
            Dictionary<string, string> bindings = new Dictionary<string, string>();
            if (needsContext)
                bindings.Add("?CONTEXT", null);
            bindings.Add("?SUBJECT", null);
            bindings.Add("?PREDICATE", null);
            bindings.Add("?OBJECT", null);

            //Iterate on the templates
            string defaultContext = RDFNamespaceRegister.DefaultNamespace.ToString();
            foreach (RDFPattern template in templates.Where(tp => tp.Variables.Count == 0
                                                                   || tp.Variables.TrueForAll(v => resultTable.HasColumn(v.ToString()))))
            {
                string templateCtx = template.Context?.ToString();
                string templateSubj = template.Subject.ToString();
                string templatePred = template.Predicate.ToString();
                string templateObj = template.Object.ToString();

                #region GROUND TEMPLATE
                if (template.Variables.Count == 0)
                {
                    if (needsContext)
                        bindings["?CONTEXT"] = templateCtx ?? defaultContext;
                    bindings["?SUBJECT"] = templateSubj;
                    bindings["?PREDICATE"] = templatePred;
                    bindings["?OBJECT"] = templateObj;
                    result.AddRow(bindings);
                    continue;
                }
                #endregion

                #region NON-GROUND TEMPLATE
                foreach (RDFTableRow resultRow in resultTable.Rows)
                {
                    #region CONTEXT
                    if (needsContext)
                    {
                        //Context of the template is a variable
                        if (template.Context is RDFVariable)
                        {
                            //Check if the template must be skipped, in order to not produce illegal triples
                            //Row contains an unbound value in position of the variable corresponding to the template context
                            if (resultRow.IsUnbound(templateCtx))
                                continue;

                            RDFPatternMember ctx = ParseRDFPatternMember(resultRow[templateCtx]);
                            //Row contains a literal in position of the variable corresponding to the template context
                            if (ctx is RDFLiteral)
                                continue;
                            //Row contains a resource in position of the variable corresponding to the template context
                            bindings["?CONTEXT"] = ctx.ToString();
                        }
                        //Context of the template is a resource
                        else
                        {
                            bindings["?CONTEXT"] = templateCtx ?? defaultContext;
                        }
                    }
                    #endregion

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (template.Subject is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (resultRow.IsUnbound(templateSubj))
                            continue;

                        RDFPatternMember subj = ParseRDFPatternMember(resultRow[templateSubj]);
                        //Row contains a literal in position of the variable corresponding to the template subject
                        if (subj is RDFLiteral)
                            continue;
                        //Row contains a resource in position of the variable corresponding to the template subject
                        bindings["?SUBJECT"] = subj.ToString();
                    }
                    //Subject of the template is a resource
                    else
                    {
                        bindings["?SUBJECT"] = templateSubj;
                    }
                    #endregion

                    #region PREDICATE
                    //Predicate of the template is a variable
                    if (template.Predicate is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template predicate
                        if (resultRow.IsUnbound(templatePred))
                            continue;

                        RDFPatternMember pred = ParseRDFPatternMember(resultRow[templatePred]);
                        //Row contains a blank resource or a literal in position of the variable corresponding to the template predicate
                        if ((pred is RDFResource predRes && predRes.IsBlank) || pred is RDFLiteral)
                            continue;
                        //Row contains a non-blank resource in position of the variable corresponding to the template predicate
                        bindings["?PREDICATE"] = pred.ToString();
                    }
                    //Predicate of the template is a resource
                    else
                    {
                        bindings["?PREDICATE"] = templatePred;
                    }
                    #endregion

                    #region OBJECT
                    //Object of the template is a variable
                    if (template.Object is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template object
                        if (resultRow.IsUnbound(templateObj))
                            continue;

                        RDFPatternMember obj = ParseRDFPatternMember(resultRow[templateObj]);
                        //Row contains a resource or a literal in position of the variable corresponding to the template object
                        bindings["?OBJECT"] = obj.ToString();
                    }
                    //Object of the template is a resource or a literal
                    else
                    {
                        bindings["?OBJECT"] = templateObj;
                    }
                    #endregion

                    //Insert the triple into the final table
                    result.AddRow(bindings);
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Describes the terms of the given DESCRIBE query with data from the given result table
        /// </summary>
        internal RDFTable DescribeTerms(RDFDescribeQuery describeQuery, RDFDataSource dataSource, RDFTable resultTable)
        {
            //Create the structure of the result table
            RDFTable result = new RDFTable();
            if (dataSource.IsStore())
                result.AddColumn("?CONTEXT");
            result.AddColumn("?SUBJECT");
            result.AddColumn("?PREDICATE");
            result.AddColumn("?OBJECT");

            //In case of "DESCRIBE *" query, all the variables must be considered describe terms
            if (describeQuery.DescribeTerms.Count == 0)
                FetchDescribeVariablesFromQueryMembers(describeQuery, describeQuery.GetEvaluableQueryMembers());

            //Iterate the describe terms of the query
            foreach (RDFPatternMember describeTerm in describeQuery.DescribeTerms)
                switch (describeTerm)
                {
                    case RDFResource describeResource:
                        MergeTable(result, DescribeResourceTerm(describeResource, dataSource, result));
                        break;

                    case RDFVariable describeVariable:
                        MergeTable(result, DescribeVariableTerm(describeVariable, dataSource, result, resultTable));
                        break;
                }

            return result;
        }

        /// <summary>
        /// Describes the given resource term with data from the given result table
        /// </summary>
        internal RDFTable DescribeResourceTerm(RDFResource describeResource, RDFDataSource dataSource, RDFTable describeTemplate)
        {
            #region Utilities
            RDFGraph QueryGraph(RDFGraph dsGraph)
                => describeResource.IsBlank
                   ? dsGraph[s: describeResource]
                      .UnionWith(dsGraph[o: describeResource])
                   : dsGraph[s: describeResource]
                      .UnionWith(dsGraph[p: describeResource])
                      .UnionWith(dsGraph[o: describeResource]);

            RDFMemoryStore QueryStore(RDFStore dsStore)
                => describeResource.IsBlank
                   ? dsStore[s: describeResource]
                      .UnionWith(dsStore[o: describeResource])
                   : dsStore[c: new RDFContext(describeResource.URI)]
                      .UnionWith(dsStore[s: describeResource])
                      .UnionWith(dsStore[p: describeResource])
                      .UnionWith(dsStore[o: describeResource]);

            RDFSelectQuery BuildFederationOrSPARQLEndpointQuery()
                => describeResource.IsBlank
                   ? new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(describeResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeResource)))
                   : new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(describeResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), describeResource, new RDFVariable("?OBJECT")).UnionWithNext())
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeResource)));
            #endregion

            RDFTable result = describeTemplate.Clone();

            switch (dataSource)
            {
                //GRAPH
                case RDFGraph dataSourceGraph:
                    RDFGraph graph = QueryGraph(dataSourceGraph);
                    MergeTable(result, RDFTable.FromDataTable(graph.ToDataTable()));
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    MergeTable(result, RDFTable.FromDataTable(store.ToDataTable()));
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults = dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    MergeTable(result, RDFTable.FromDataTable(queryResults.SelectResults));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Describes the given literal term with data from the given result table
        /// </summary>
        internal RDFTable DescribeLiteralTerm(RDFLiteral describeLiteral, RDFDataSource dataSource, RDFTable describeTemplate)
        {
            #region Utilities
            RDFGraph QueryGraph(RDFGraph dsGraph)
                => dsGraph[l: describeLiteral];

            RDFMemoryStore QueryStore(RDFStore dsStore)
                => dsStore[l: describeLiteral];

            RDFSelectQuery BuildFederationOrSPARQLEndpointQuery()
                => new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeLiteral)));
            #endregion

            RDFTable result = describeTemplate.Clone();

            switch (dataSource)
            {
                //GRAPH
                case RDFGraph dataSourceGraph:
                    RDFGraph graph = QueryGraph(dataSourceGraph);
                    MergeTable(result, RDFTable.FromDataTable(graph.ToDataTable()));
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    MergeTable(result, RDFTable.FromDataTable(store.ToDataTable()));
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults =
                        dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    MergeTable(result, RDFTable.FromDataTable(queryResults.SelectResults));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Describes the given variable term with data from the given result table
        /// </summary>
        internal RDFTable DescribeVariableTerm(RDFVariable describeVariable, RDFDataSource dataSource, RDFTable describeTemplate, RDFTable resultTable)
        {
            RDFTable result = describeTemplate.Clone();

            //In order to be processed this variable must be a column of the results table!
            string describeVariableName = describeVariable.ToString();
            if (!resultTable.HasColumn(describeVariableName))
                return result;

            //Iterate the results table's rows to retrieve terms to be described
            foreach (RDFPatternMember describeVariableValue in
                     from RDFTableRow resultRow in resultTable.Rows
                     where !resultRow.IsUnbound(describeVariableName)
                     select ParseRDFPatternMember(resultRow[describeVariableName]))
            {
                //Execute most appropriate strategy, depending on the type of the variable value
                switch (describeVariableValue)
                {
                    //RESOURCE
                    case RDFResource describeResource:
                        RDFTable describeResourceTable = DescribeResourceTerm(describeResource, dataSource, describeTemplate);
                        MergeTable(result, describeResourceTable);
                        break;

                    //LITERAL
                    case RDFLiteral describeLiteral:
                        RDFTable describeLiteralTable = DescribeLiteralTerm(describeLiteral, dataSource, describeTemplate);
                        MergeTable(result, describeLiteralTable);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches the describe variables from the given collection of query members and propagates them to the given describe query
        /// </summary>
        internal void FetchDescribeVariablesFromQueryMembers(RDFDescribeQuery describeQuery, IEnumerable<RDFQueryMember> evaluableQueryMembers)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
            {
                switch (evaluableQueryMember)
                {
                    //PATTERN GROUP
                    case RDFPatternGroup pgEvaluableQueryMember:
                        pgEvaluableQueryMember.Variables.ForEach(v => describeQuery.AddDescribeTerm(v));
                        break;

                    //SUBQUERY
                    case RDFSelectQuery sqEvaluableQueryMember:
                        FetchDescribeVariablesFromQueryMembers(describeQuery, sqEvaluableQueryMember.GetEvaluableQueryMembers());
                        break;
                }
            }
        }
    }
}