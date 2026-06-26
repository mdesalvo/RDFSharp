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

using System;
using System.Collections.Generic;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFServiceEngine evaluates SPARQL <c>SERVICE</c> (federated query) members: it builds the remote query out
    /// of a SERVICE inner pattern, sends it to the proper endpoint, and imports the returned solutions as a result
    /// table. It is the dedicated companion of <see cref="RDFQueryEngine"/>, which keeps owning the orchestration
    /// (when each SERVICE member is evaluated, and the two-phase deferring of variable-endpoint SERVICE members).
    /// </summary>
    internal static class RDFServiceEngine
    {
        #region Methods
        /// <summary>
        /// Evaluates a SERVICE with a concrete (IRI) endpoint: wraps its inner pattern in a <c>SELECT *</c> and
        /// sends it to the remote endpoint, importing the returned solutions as a result table.
        /// </summary>
        internal static RDFTable EvaluateConcreteService(RDFService service)
        {
            RDFSelectQueryResult serviceResult = BuildServiceQuery(service.InnerPattern)
                                                    .ApplyToSPARQLEndpoint(service.Endpoint, service.QueryOptions);
            RDFTable serviceTable = RDFTable.FromDataTable(serviceResult.SelectResults);
            serviceTable.IsOptional = service.IsOptional;
            return serviceTable;
        }

        /// <summary>
        /// Evaluates a SERVICE with a variable endpoint (<c>SERVICE ?ep {…}</c>): collects the distinct endpoint
        /// IRIs bound to the variable by the sibling members (from the given context table), queries each of them
        /// reusing the single endpoint instance (its address is overwritten per binding), binds the endpoint
        /// variable on every returned row, and unions the per-endpoint results into a single table.
        /// </summary>
        /// <exception cref="RDFQueryException">When the endpoint variable is not bound by any sibling member and the SERVICE is not SILENT.</exception>
        internal static RDFTable EvaluateVariableService(RDFService service, RDFTable contextTable)
        {
            string endpointColumn = service.EndpointVariable.ToString();

            //Collect the distinct IRI values bound to the endpoint variable by the sibling members
            List<RDFResource> endpointIRIs = CollectDistinctEndpointIRIs(contextTable, endpointColumn);

            //An unbound endpoint variable: SILENT yields an empty result, otherwise it is an evaluation error
            if (endpointIRIs.Count == 0)
            {
                if (service.IsSilent)
                    return new RDFTable { IsOptional = service.IsOptional };
                throw new RDFQueryException("Cannot evaluate SPARQL SERVICE clause: its endpoint variable '" + endpointColumn + "' is not bound by any sibling pattern.");
            }

            //Build the SELECT * once and reuse it (and the single endpoint) across all the bound endpoints
            RDFSelectQuery serviceQuery = BuildServiceQuery(service.InnerPattern);
            RDFTable serviceTable = new RDFTable();
            foreach (RDFResource endpointIRI in endpointIRIs)
            {
                //Reuse the single endpoint by overwriting its address with the current bound IRI
                service.Endpoint.BaseAddress = new Uri(endpointIRI.ToString());
                RDFTable endpointTable = RDFTable.FromDataTable(serviceQuery.ApplyToSPARQLEndpoint(service.Endpoint, service.QueryOptions).SelectResults);

                //Bind the endpoint variable to the IRI on every returned row (cross-join with a single-row constant table)
                RDFTable endpointBinding = new RDFTable();
                endpointBinding.AddColumn(endpointColumn);
                endpointBinding.AddRow(new Dictionary<string, string> { { endpointColumn, endpointIRI.ToString() } });

                //UNION (multiset) the per-endpoint results
                RDFTableEngine.MergeTable(serviceTable, RDFTableEngine.CombineTables(new List<RDFTable> { endpointTable, endpointBinding }));
            }
            serviceTable.IsOptional = service.IsOptional;
            return serviceTable;
        }

        /// <summary>
        /// Collects, in order of first appearance, the distinct IRI values bound to the given endpoint column in the
        /// given context table. Literals, blank nodes and unbound cells are skipped (they cannot name an endpoint).
        /// </summary>
        private static List<RDFResource> CollectDistinctEndpointIRIs(RDFTable contextTable, string endpointColumn)
        {
            List<RDFResource> endpointIRIs = new List<RDFResource>();

            //The endpoint variable may be entirely absent from the context (no sibling member bound it)
            if (contextTable.OrdinalOf(endpointColumn) == -1)
                return endpointIRIs;

            HashSet<string> seenEndpoints = new HashSet<string>();
            foreach (RDFTableRow contextRow in contextTable.Rows)
            {
                string endpointValue = contextRow[endpointColumn];
                if (endpointValue != null
                     && seenEndpoints.Add(endpointValue)
                     && RDFQueryUtilities.ParseRDFPatternMember(endpointValue) is RDFResource endpointResource
                     && !endpointResource.IsBlank)
                {
                    endpointIRIs.Add(endpointResource);
                }
            }
            return endpointIRIs;
        }

        /// <summary>
        /// Wraps the given inner group graph pattern in a fresh <c>SELECT *</c> query, ready to be sent to a remote
        /// SPARQL endpoint. The inner pattern may be of any shape (pattern group, sub-select, binary tree, or a
        /// nested SERVICE), so it is routed to the matching <c>Add…</c> method.
        /// </summary>
        private static RDFSelectQuery BuildServiceQuery(RDFQueryMember innerPattern)
        {
            RDFSelectQuery serviceQuery = new RDFSelectQuery();
            switch (innerPattern)
            {
                case RDFPatternGroup patternGroup:
                    serviceQuery.AddPatternGroup(patternGroup);
                    break;

                case RDFSelectQuery subQuery:
                    serviceQuery.AddSubQuery(subQuery);
                    break;

                case RDFBinaryQueryMember binaryMember:
                    serviceQuery.AddBinaryQueryMember(binaryMember);
                    break;

                case RDFService nestedService:
                    serviceQuery.AddService(nestedService);
                    break;
            }
            return serviceQuery;
        }
        #endregion
    }
}