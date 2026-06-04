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
using System.Text;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    // RDFQueryEngine (MIRELLA): pattern matching against graph, store and federation datasources.
    internal partial class RDFQueryEngine
    {
        /// <summary>
        /// Applies the given pattern to the given data source
        /// </summary>
        internal RDFTable ApplyPattern(RDFPattern pattern, RDFDataSource dataSource)
        {
            switch (dataSource)
            {
                case RDFGraph graph:
                    return ApplyPatternToGraph(pattern, graph);

                case RDFStore store:
                    return ApplyPatternToStore(pattern, store);

                case RDFFederation federation:
                    return ApplyPatternToFederation(pattern, federation);
            }
            return new RDFTable();
        }

        /// <summary>
        /// Applies the given pattern to the given graph
        /// </summary>
        internal RDFTable ApplyPatternToGraph(RDFPattern pattern, RDFGraph graph)
        {
            RDFTable patternResultTable = new RDFTable();
            StringBuilder templateHoleDetector = new StringBuilder();

            //Analyze subject of the pattern
            if (pattern.Subject is RDFVariable)
            {
                templateHoleDetector.Append('S');
                patternResultTable.AddColumn(pattern.Subject.ToString());
            }

            //Analyze predicate of the pattern
            if (pattern.Predicate is RDFVariable)
            {
                templateHoleDetector.Append('P');
                patternResultTable.AddColumn(pattern.Predicate.ToString());
            }

            //Analyze object of the pattern
            bool pObjRes = pattern.Object is RDFResource;
            bool pObjLit = pattern.Object is RDFLiteral;
            if (pattern.Object is RDFVariable)
            {
                templateHoleDetector.Append('O');
                patternResultTable.AddColumn(pattern.Object.ToString());
            }

            //Analyze templateHoleDetector to refine the set of matching triples
            List<RDFTriple> matchingTriples = null;
            switch (templateHoleDetector.ToString())
            {
                case "S":
                    matchingTriples = graph.SelectTriples(p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "P":
                    matchingTriples = graph.SelectTriples(s: (RDFResource)pattern.Subject, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "O":
                    matchingTriples = graph.SelectTriples(s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate);
                    break;

                case "SP":
                    matchingTriples = graph.SelectTriples(o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                    break;

                case "SO":
                    matchingTriples = graph.SelectTriples(p: (RDFResource)pattern.Predicate);
                    //In case of same S and O variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                    break;

                case "PO":
                    matchingTriples = graph.SelectTriples(s: (RDFResource)pattern.Subject);
                    //In case of same P and O variable, must refine matching triples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                    break;

                case "SPO":
                    matchingTriples = graph.SelectTriples();
                    //In case of same S and P variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                    //In case of same S and O variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                    //In case of same P and O variable, must refine matching triples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                    break;
            }

            //Fully-bound patterns (no holes) match no switch case and leave the table empty
            if (matchingTriples != null)
                PopulateTable(pattern, matchingTriples, patternResultTable);

            return patternResultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given store
        /// </summary>
        internal RDFTable ApplyPatternToStore(RDFPattern pattern, RDFStore store)
        {
            RDFTable patternResultTable = new RDFTable();
            StringBuilder templateHoleDetector = new StringBuilder();

            //Analyze context of the pattern
            bool hasContext = pattern.Context != null;
            if (hasContext && pattern.Context is RDFVariable)
            {
                templateHoleDetector.Append('C');
                patternResultTable.AddColumn(pattern.Context.ToString());
            }

            //Analyze subject of the pattern
            if (pattern.Subject is RDFVariable)
            {
                templateHoleDetector.Append('S');
                patternResultTable.AddColumn(pattern.Subject.ToString());
            }

            //Analyze predicate of the pattern
            if (pattern.Predicate is RDFVariable)
            {
                templateHoleDetector.Append('P');
                patternResultTable.AddColumn(pattern.Predicate.ToString());
            }

            //Analyze object of the pattern
            bool pObjRes = pattern.Object is RDFResource;
            bool pObjLit = pattern.Object is RDFLiteral;
            if (pattern.Object is RDFVariable)
            {
                templateHoleDetector.Append('O');
                patternResultTable.AddColumn(pattern.Object.ToString());
            }

            //Analyze templateHoleDetector to refine the set of matching quadruples
            List<RDFQuadruple> matchingQuadruples = null;
            switch (templateHoleDetector.ToString())
            {
                case "C":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "S":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "P":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, s: (RDFResource)pattern.Subject, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "O":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate);
                    break;

                case "CS":
                    matchingQuadruples = store.SelectQuadruples(p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    break;

                case "CP":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    break;

                case "CO":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate);
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    break;

                case "SP":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    break;

                case "SO":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, p: (RDFResource)pattern.Predicate);
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    break;

                case "PO":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, s: (RDFResource)pattern.Subject);
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;

                case "CSP":
                    matchingQuadruples = store.SelectQuadruples(o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    break;

                case "CSO":
                    matchingQuadruples = store.SelectQuadruples(p: (RDFResource)pattern.Predicate);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    break;

                case "CPO":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;

                case "SPO":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;

                case "CSPO":
                    matchingQuadruples = store.SelectQuadruples();
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;
            }

            //Fully-bound patterns (no holes) match no switch case and leave the table empty
            if (matchingQuadruples != null)
                PopulateTable(pattern, matchingQuadruples, patternResultTable);

            return patternResultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given federation
        /// </summary>
        internal RDFTable ApplyPatternToFederation(RDFPattern pattern, RDFFederation federation)
        {
            RDFTable resultTable = new RDFTable();

            //Iterate data sources of the federation
            foreach (RDFDataSource dataSource in federation)
            {
                switch (dataSource)
                {
                    case RDFGraph dataSourceGraph:
                        RDFTable graphTable = ApplyPatternToGraph(pattern, dataSourceGraph);
                        MergeTable(resultTable, graphTable);
                        break;

                    case RDFStore dataSourceStore:
                        RDFTable storeTable = ApplyPatternToStore(pattern, dataSourceStore);
                        MergeTable(resultTable, storeTable);
                        break;

                    case RDFFederation dataSourceFederation:
                        RDFTable federationTable = ApplyPatternToFederation(pattern, dataSourceFederation);
                        MergeTable(resultTable, federationTable);
                        break;

                    case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                        //Pattern is transformed into an equivalent "SELECT *" query which is sent to the SPARQL endpoint.
                        //SPARQL endpoint options are eventually retrieved directly from the federation.
                        federation.EndpointDataSourcesQueryOptions.TryGetValue(dataSourceSparqlEndpoint.ToString(), out RDFSPARQLEndpointQueryOptions dataSourceSparqlEndpointOptions);
                        RDFSelectQuery sparqlEndpointQuery =  new RDFSelectQuery().AddPatternGroup(new RDFPatternGroup().AddPattern(pattern));
                        RDFSelectQueryResult sparqlEndpointTable = sparqlEndpointQuery.ApplyToSPARQLEndpoint(dataSourceSparqlEndpoint, dataSourceSparqlEndpointOptions);
                        MergeTable(resultTable, RDFTable.FromDataTable(sparqlEndpointTable.SelectResults));
                        break;
                }
            }

            return resultTable;
        }
    }
}
