/*
   Copyright 2012-2023 Marco De Salvo

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

using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFStore represents an abstract store in the RDF model.
    /// </summary>
    public abstract class RDFStore : RDFDataSource, IEquatable<RDFStore>
    {
        #region Properties
        /// <summary>
        /// Unique representation of the store
        /// </summary>
        public long StoreID { get; set; }

        /// <summary>
        /// Type of the store
        /// </summary>
        public string StoreType { get; set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the store
        /// </summary>
        public override string ToString()
            => StoreType;

        /// <summary>
        /// Performs the equality comparison between two stores
        /// </summary>
        public bool Equals(RDFStore other)
            => other != null && StoreID.Equals(other.StoreID);
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public abstract RDFStore MergeGraph(RDFGraph graph);

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public abstract RDFStore AddQuadruple(RDFQuadruple quadruple);
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the store
        /// </summary>
        public abstract RDFStore RemoveQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Removes the quadruples with the given context
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContext(RDFContext contextResource);

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubject(RDFResource subjectResource);

        /// <summary>
        /// Removes the quadruples with the given (non-blank) predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicate(RDFResource predicateResource);

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByObject(RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByLiteral(RDFLiteral objectLiteral);

        /// <summary>
        /// Removes the quadruples with the given context and subject
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubject(RDFContext contextResource, RDFResource subjectResource);

        /// <summary>
        /// Removes the quadruples with the given context and predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextPredicate(RDFContext contextResource, RDFResource predicateResource);

        /// <summary>
        /// Removes the quadruples with the given context and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextObject(RDFContext contextResource, RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given context and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextLiteral(RDFContext contextResource, RDFLiteral objectLiteral);

        /// <summary>
        /// Removes the quadruples with the given context, subject and predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubjectPredicate(RDFContext contextResource, RDFResource subjectResource, RDFResource predicateResource);

        /// <summary>
        /// Removes the quadruples with the given context, subject and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubjectObject(RDFContext contextResource, RDFResource subjectResource, RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given context, subject and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextSubjectLiteral(RDFContext contextResource, RDFResource subjectResource, RDFLiteral objectLiteral);

        /// <summary>
        /// Removes the quadruples with the given context, predicate and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextPredicateObject(RDFContext contextResource, RDFResource predicateResource, RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given context, predicate and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByContextPredicateLiteral(RDFContext contextResource, RDFResource predicateResource, RDFLiteral objectLiteral);

        /// <summary>
        /// Removes the quadruples with the given subject and predicate
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubjectPredicate(RDFResource subjectResource, RDFResource predicateResource);

        /// <summary>
        /// Removes the quadruples with the given subject and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubjectObject(RDFResource subjectResource, RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given subject and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesBySubjectLiteral(RDFResource subjectResource, RDFLiteral objectLiteral);

        /// <summary>
        /// Removes the quadruples with the given predicate and object
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicateObject(RDFResource predicateResource, RDFResource objectResource);

        /// <summary>
        /// Removes the quadruples with the given predicate and literal
        /// </summary>
        public abstract RDFStore RemoveQuadruplesByPredicateLiteral(RDFResource predicateResource, RDFLiteral objectLiteral);

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public abstract void ClearQuadruples();

        /// <summary>
        /// Compacts the reified quadruples by removing their 4 standard statements
        /// </summary>
        public void UnreifyQuadruples()
        {
            //Create SPARQL SELECT query for detecting reified quadruples
            RDFVariable T = new RDFVariable("T");
            RDFVariable C = new RDFVariable("C");
            RDFVariable S = new RDFVariable("S");
            RDFVariable P = new RDFVariable("P");
            RDFVariable O = new RDFVariable("O");
            RDFSelectQuery Q = new RDFSelectQuery()
                                .AddPatternGroup(new RDFPatternGroup()
                                    .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT))
                                    .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.SUBJECT, S))
                                    .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.PREDICATE, P))
                                    .AddPattern(new RDFPattern(C, T, RDFVocabulary.RDF.OBJECT, O))
                                    .AddFilter(new RDFIsUriFilter(C))
                                    .AddFilter(new RDFIsUriFilter(T))
                                    .AddFilter(new RDFIsUriFilter(S))
                                    .AddFilter(new RDFIsUriFilter(P))
                                );

            //Apply it to the store
            RDFSelectQueryResult R = Q.ApplyToStore(this);

            //Iterate results
            IEnumerator reifiedQuadruples = R.SelectResults.Rows.GetEnumerator();
            while (reifiedQuadruples.MoveNext())
            {
                //Get reification data (T, C, S, P, O)
                RDFPatternMember tRepresent = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?T"].ToString());
                RDFPatternMember tContext = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?C"].ToString());
                RDFPatternMember tSubject = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?S"].ToString());
                RDFPatternMember tPredicate = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?P"].ToString());
                RDFPatternMember tObject = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?O"].ToString());

                //Cleanup store from detected reifications
                if (tObject is RDFResource)
                {
                    AddQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tSubject, (RDFResource)tPredicate, (RDFResource)tObject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFResource)tObject));
                }
                else
                {
                    AddQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tSubject, (RDFResource)tPredicate, (RDFLiteral)tObject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFLiteral)tObject));
                }
            }
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public abstract bool ContainsQuadruple(RDFQuadruple quadruple);

        /// <summary>
        /// Gets a store containing all quadruples
        /// </summary>
        public RDFMemoryStore SelectAllQuadruples()
            => SelectQuadruples(null, null, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified context
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByContext(RDFContext contextResource)
            => SelectQuadruples(contextResource, null, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified subject
        /// </summary>
        public RDFMemoryStore SelectQuadruplesBySubject(RDFResource subjectResource)
            => SelectQuadruples(null, subjectResource, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified predicate
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByPredicate(RDFResource predicateResource)
            => SelectQuadruples(null, null, predicateResource, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified object
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByObject(RDFResource objectResource)
            => SelectQuadruples(null, null, null, objectResource, null);

        /// <summary>
        /// Gets a memory store containing quadruples with the specified literal
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByLiteral(RDFLiteral objectLiteral)
            => SelectQuadruples(null, null, null, null, objectLiteral);

        /// <summary>
        /// Gets a memory store containing quadruples satisfying the given pattern
        /// </summary>
        internal abstract RDFMemoryStore SelectQuadruples(RDFContext contextResource,
                                                          RDFResource subjectResource,
                                                          RDFResource predicateResource,
                                                          RDFResource objectResource,
                                                          RDFLiteral objectLiteral);

        /// <summary>
        /// Gets a list containing the graphs saved in the store
        /// </summary>
        public List<RDFGraph> ExtractGraphs()
        {
            Dictionary<long, RDFGraph> graphs = new Dictionary<long, RDFGraph>();
            foreach (RDFQuadruple q in (this is RDFMemoryStore ? (RDFMemoryStore)this : SelectAllQuadruples()))
            {
                // Step 1: Cache-Update
                if (!graphs.ContainsKey(q.Context.PatternMemberID))
                {
                    graphs.Add(q.Context.PatternMemberID, new RDFGraph());
                    graphs[q.Context.PatternMemberID].SetContext(((RDFContext)q.Context).Context);
                }

                // Step 2: Result-Update
                if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    graphs[q.Context.PatternMemberID].AddTriple(new RDFTriple((RDFResource)q.Subject, (RDFResource)q.Predicate, (RDFResource)q.Object));
                else
                    graphs[q.Context.PatternMemberID].AddTriple(new RDFTriple((RDFResource)q.Subject, (RDFResource)q.Predicate, (RDFLiteral)q.Object));
            }
            return graphs.Values.ToList();
        }

        /// <summary>
        /// Gets a list containing the contexts saved in the store
        /// </summary>
        public List<RDFContext> ExtractContexts()
        {
            Dictionary<long, RDFPatternMember> contexts = new Dictionary<long, RDFPatternMember>();
            foreach (RDFQuadruple q in (this is RDFMemoryStore ? (RDFMemoryStore)this : SelectAllQuadruples()))
            {
                if (!contexts.ContainsKey(q.Context.PatternMemberID))
                    contexts.Add(q.Context.PatternMemberID, q.Context);
            }
            return contexts.Values.OfType<RDFContext>().ToList();
        }
        #endregion

        #region Convert

        #region Export
        /// <summary>
        /// Writes the store into a file in the given RDF format
        /// </summary>
        public void ToFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                throw new RDFStoreException("Cannot write RDF store to file because given \"filepath\" parameter is null or empty.");
            
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    RDFNQuads.Serialize(this, filepath);
                    break;
                case RDFStoreEnums.RDFFormats.TriX:
                    RDFTriX.Serialize(this, filepath);
                    break;
                case RDFStoreEnums.RDFFormats.TriG:
                    RDFTriG.Serialize(this, filepath);
                    break;
            }
        }

        /// <summary>
        /// Writes the store into a stream in the given RDF format
        /// </summary>
        public void ToStream(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream)
        {
            if (outputStream == null)
                throw new RDFStoreException("Cannot write RDF store to stream because given \"outputStream\" parameter is null.");
            
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    RDFNQuads.Serialize(this, outputStream);
                    break;
                case RDFStoreEnums.RDFFormats.TriX:
                    RDFTriX.Serialize(this, outputStream);
                    break;
                case RDFStoreEnums.RDFFormats.TriG:
                    RDFTriG.Serialize(this, outputStream);
                    break;
            }
        }

        /// <summary>
        /// Writes the store into a datatable with "Context-Subject-Predicate-Object" columns
        /// </summary>
        public DataTable ToDataTable()
        {
            //Create the structure of the result datatable
            DataTable result = new DataTable(ToString());
            result.Columns.Add("?CONTEXT", RDFQueryEngine.SystemString);
            result.Columns.Add("?SUBJECT", RDFQueryEngine.SystemString);
            result.Columns.Add("?PREDICATE", RDFQueryEngine.SystemString);
            result.Columns.Add("?OBJECT", RDFQueryEngine.SystemString);

            //Iterate the quadruples of the store to populate the result datatable
            result.BeginLoadData();
            foreach (RDFQuadruple q in SelectAllQuadruples())
            {
                DataRow newRow = result.NewRow();
                newRow["?CONTEXT"] = q.Context.ToString();
                newRow["?SUBJECT"] = q.Subject.ToString();
                newRow["?PREDICATE"] = q.Predicate.ToString();
                newRow["?OBJECT"] = q.Object.ToString();
                result.Rows.Add(newRow);
            }
            result.EndLoadData();

            return result;
        }
        #endregion

        #endregion

        #endregion
    }
}