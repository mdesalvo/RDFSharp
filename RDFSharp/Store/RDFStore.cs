/*
   Copyright 2012-2017 Marco De Salvo

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
using System.Data;
using System.IO;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFStore represents an abstract RDF store, baseline for Memory or SQL-based implementations.
    /// </summary>
    public abstract class RDFStore: RDFDataSource, IEquatable<RDFStore> {

        #region Properties
        /// <summary>
        /// Unique representation of the store
        /// </summary>
        public Int64 StoreID { get; set; }

        /// <summary>
        /// Type of the store
        /// </summary>
        public String StoreType { get; set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the store
        /// </summary>
        public override String ToString() {
            return this.StoreType;
        }

        /// <summary>
        /// Performs the equality comparison between two stores
        /// </summary>
        public Boolean Equals(RDFStore other) {
            return (other != null && this.StoreID.Equals(other.StoreID));
        }
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
        /// Removes the given quadruples from the store
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
        /// Clears the quadruples of the store
        /// </summary>
        public abstract void ClearQuadruples();

        /// <summary>
        /// Compacts the reified quadruples by removing their 4 standard statements 
        /// </summary>
        public void UnreifyQuadruples() {

            //Create SPARQL SELECT query for detecting reified quadruples
            var T = new RDFVariable("T");
            var C = new RDFVariable("C");
            var S = new RDFVariable("S");
            var P = new RDFVariable("P");
            var O = new RDFVariable("O");
            var Q = new RDFSelectQuery()
                            .AddPatternGroup(new RDFPatternGroup("UnreifyQuadruples")
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
            var R = Q.ApplyToStore(this);

            //Iterate results
            var reifiedQuadruples = R.SelectResults.Rows.GetEnumerator();
            while (reifiedQuadruples.MoveNext()) {

                //Get reification data (T, C, S, P, O)
                var tRepresent = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?T"].ToString());
                var tContext   = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?C"].ToString());
                var tSubject   = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?S"].ToString());
                var tPredicate = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?P"].ToString());
                var tObject    = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedQuadruples.Current)["?O"].ToString());

                //Cleanup store from detected reifications
                if (tObject is RDFResource) {
                    this.AddQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tSubject, (RDFResource)tPredicate, (RDFResource)tObject));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFResource)tObject));
                }
                else {
                    this.AddQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tSubject, (RDFResource)tPredicate, (RDFLiteral)tObject));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    this.RemoveQuadruple(new RDFQuadruple(new RDFContext(((RDFResource)tContext).URI), (RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFLiteral)tObject));
                }

            }

        }
        #endregion

        #region Select
        /// <summary>
        /// Gets a list containing the graphs saved in the store
        /// </summary>
        public List<RDFGraph> ExtractGraphs() {
            var graphs      = new Dictionary<Int64, RDFGraph>();
            foreach (var q in (this is RDFMemoryStore ? (RDFMemoryStore)this : this.SelectAllQuadruples())) {

                // Step 1: Cache-Update
                if (!graphs.ContainsKey(q.Context.PatternMemberID)) {
                     graphs.Add(q.Context.PatternMemberID, new RDFGraph());
                     graphs[q.Context.PatternMemberID].SetContext(((RDFContext)q.Context).Context);
                }

                // Step 2: Result-Update
                if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    graphs[q.Context.PatternMemberID].AddTriple(new RDFTriple((RDFResource)q.Subject,
                                                                              (RDFResource)q.Predicate,
                                                                              (RDFResource)q.Object));
                }
                else {
                    graphs[q.Context.PatternMemberID].AddTriple(new RDFTriple((RDFResource)q.Subject,
                                                                              (RDFResource)q.Predicate,
                                                                              (RDFLiteral)q.Object));
                }

            }
            return graphs.Values.ToList();
        }

        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public virtual Boolean ContainsQuadruple(RDFQuadruple quadruple) {
            if (quadruple   != null) {
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    return (this.SelectQuadruples((RDFContext)quadruple.Context,
                                                  (RDFResource)quadruple.Subject,
                                                  (RDFResource)quadruple.Predicate,
                                                  (RDFResource)quadruple.Object,
                                                  null)).QuadruplesCount > 0;
                }
                else {
                    return (this.SelectQuadruples((RDFContext)quadruple.Context,
                                                  (RDFResource)quadruple.Subject,
                                                  (RDFResource)quadruple.Predicate,
                                                  null,
                                                  (RDFLiteral)quadruple.Object)).QuadruplesCount > 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a store containing all quadruples
        /// </summary>
        public RDFMemoryStore SelectAllQuadruples() {
            return this.SelectQuadruples(null, null, null, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified context
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByContext(RDFContext contextResource) {
            return this.SelectQuadruples(contextResource, null, null, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified subject
        /// </summary>
        public RDFMemoryStore SelectQuadruplesBySubject(RDFResource subjectResource) {
            return this.SelectQuadruples(null, subjectResource, null, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified predicate
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByPredicate(RDFResource predicateResource) {
            return this.SelectQuadruples(null, null, predicateResource, null, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified object
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByObject(RDFResource objectResource) {
            return this.SelectQuadruples(null, null, null, objectResource, null);
        }

        /// <summary>
        /// Gets a memory store containing quadruples with the specified literal
        /// </summary>
        public RDFMemoryStore SelectQuadruplesByLiteral(RDFLiteral objectLiteral) {
            return this.SelectQuadruples(null, null, null, null, objectLiteral);
        }

        /// <summary>
        /// Gets a store containing quadruples satisfying the given pattern
        /// </summary>
        internal abstract RDFMemoryStore SelectQuadruples(RDFContext  contextResource,
                                                          RDFResource subjectResource,
                                                          RDFResource predicateResource,
                                                          RDFResource objectResource,
                                                          RDFLiteral  objectLiteral);
        #endregion

        #region Convert

        #region Exporters
        /// <summary>
        /// Writes the store into a file in the given RDF format. 
        /// </summary>
        public void ToFile(RDFStoreEnums.RDFFormats rdfFormat, String filepath) {
            if (filepath != null) {
                switch  (rdfFormat) {
                    case RDFStoreEnums.RDFFormats.NQuads:
                         RDFNQuads.Serialize(this, filepath);
                         break;
                    case RDFStoreEnums.RDFFormats.TriX:
                         RDFTriX.Serialize(this, filepath);
                         break;
                }
            }
            else {
                throw new RDFStoreException("Cannot write RDF store to file because given \"filepath\" parameter is null or empty.");
            }
        }

        /// <summary>
        /// Writes the store into a stream in the given RDF format. 
        /// </summary>
        public void ToStream(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream) {
            if (outputStream != null) {
                switch  (rdfFormat) {
                    case RDFStoreEnums.RDFFormats.NQuads:
                         RDFNQuads.Serialize(this, outputStream);
                         break;
                    case RDFStoreEnums.RDFFormats.TriX:
                         RDFTriX.Serialize(this, outputStream);
                         break;
                }
            }
            else {
                throw new RDFStoreException("Cannot write RDF store to stream because given \"outputStream\" parameter is null.");
            }
        }

        /// <summary>
        /// Writes the store into a datatable with "Context-Subject-Predicate-Object" columns
        /// </summary>
        public DataTable ToDataTable() {

            //Create the structure of the result datatable
            DataTable result                = new DataTable(this.ToString());
            result.Columns.Add("CONTEXT",   Type.GetType("System.String"));
            result.Columns.Add("SUBJECT",   Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT",    Type.GetType("System.String"));
            result.AcceptChanges();

            //Iterate the quadruples of the store to populate the result datatable
            result.BeginLoadData();
            foreach (var q in this.SelectAllQuadruples()) {
                DataRow newRow              = result.NewRow();
                newRow["CONTEXT"]           = q.Context.ToString();
                newRow["SUBJECT"]           = q.Subject.ToString();
                newRow["PREDICATE"]         = q.Predicate.ToString();
                newRow["OBJECT"]            = q.Object.ToString();
                newRow.AcceptChanges();
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