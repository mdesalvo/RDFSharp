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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Store
{
    
    /// <summary>
    /// RDFMemoryStore represents an in-memory RDF store engine.
    /// </summary>
    public class RDFMemoryStore: RDFStore, IEnumerable<RDFQuadruple> {

        #region Properties
        /// <summary>
        /// Count of the store's quadruples
        /// </summary>
        public Int64 QuadruplesCount {
            get { return this.Quadruples.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the store's quadruples for iteration
        /// </summary>
        public IEnumerator<RDFQuadruple> QuadruplesEnumerator {
            get { return this.Quadruples.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Identifier of the memory store
        /// </summary>
        internal String StoreGUID { get; set; }

        /// <summary>
        /// Index on the quadruples of the store
        /// </summary>
        internal RDFStoreIndex StoreIndex { get; set; }

        /// <summary>
        /// List of quadruples embedded into the store
        /// </summary>
        internal Dictionary<Int64, RDFQuadruple> Quadruples { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty memory store
        /// </summary>
        public RDFMemoryStore() {
            this.StoreType  = "MEMORY";
            this.StoreGUID  = Guid.NewGuid().ToString();
            this.StoreIndex = new RDFStoreIndex();
            this.StoreID    = RDFModelUtilities.CreateHash(this.ToString());
            this.Quadruples = new Dictionary<Int64, RDFQuadruple>();
        }

        /// <summary>
        /// List-based ctor to build a memory store with the given list of quadruples
        /// </summary>
        internal RDFMemoryStore(List<RDFQuadruple> quadruples): this() {
            if (quadruples != null) {
                quadruples.ForEach(q => this.AddQuadruple(q));
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Memory store 
        /// </summary>
        public override String ToString() {
            return base.ToString() + "|ID=" + this.StoreGUID;
        }

        /// <summary>
        /// Performs the equality comparison between two memory stores
        /// </summary>
        public Boolean Equals(RDFMemoryStore other) {
            if (other == null || this.QuadruplesCount != other.QuadruplesCount) {
                return false;
            }
            foreach(RDFQuadruple q in this) {
                if (!other.ContainsQuadruple(q)) {
                     return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the store's quadruples
        /// </summary>
        IEnumerator<RDFQuadruple> IEnumerable<RDFQuadruple>.GetEnumerator() {
            return this.Quadruples.Values.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the store's quadruples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Quadruples.Values.GetEnumerator();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore MergeGraph(RDFGraph graph) {
            if (graph != null) {
                var graphCtx             = new RDFContext(graph.Context);
                foreach (var t          in graph) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         this.AddQuadruple(new RDFQuadruple(graphCtx, (RDFResource)t.Subject, (RDFResource)t.Predicate, (RDFResource)t.Object));
                    }
                    else {
                         this.AddQuadruple(new RDFQuadruple(graphCtx, (RDFResource)t.Subject, (RDFResource)t.Predicate, (RDFLiteral)t.Object));
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore AddQuadruple(RDFQuadruple quadruple) {
            if (quadruple != null) {
                if (!this.Quadruples.ContainsKey(quadruple.QuadrupleID)) {
                     this.Quadruples.Add(quadruple.QuadrupleID, quadruple);
                     this.StoreIndex.AddIndex(quadruple);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the store
        /// </summary>
        public override RDFStore RemoveQuadruple(RDFQuadruple quadruple) {
            if (this.ContainsQuadruple(quadruple)) {
                this.Quadruples.Remove(quadruple.QuadrupleID);
                this.StoreIndex.RemoveIndex(quadruple);
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context
        /// </summary>
        public override RDFStore RemoveQuadruplesByContext(RDFContext contextResource) {
            if (contextResource        != null) {
                foreach (var quadruple in this.SelectQuadruplesByContext(contextResource)) {
                    this.Quadruples.Remove(quadruple.QuadrupleID);
                    this.StoreIndex.RemoveIndex(quadruple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubject(RDFResource subjectResource) {
            if (subjectResource        != null) {
                foreach (var quadruple in this.SelectQuadruplesBySubject(subjectResource)) {
                    this.Quadruples.Remove(quadruple.QuadrupleID);
                    this.StoreIndex.RemoveIndex(quadruple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given (non-blank) predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicate(RDFResource predicateResource) {
            if (predicateResource      != null && !predicateResource.IsBlank) {
                foreach (var quadruple in this.SelectQuadruplesByPredicate(predicateResource)) {
                    this.Quadruples.Remove(quadruple.QuadrupleID);
                    this.StoreIndex.RemoveIndex(quadruple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByObject(RDFResource objectResource) {
            if (objectResource         != null) {
                foreach (var quadruple in this.SelectQuadruplesByObject(objectResource)) {
                    this.Quadruples.Remove(quadruple.QuadrupleID);
                    this.StoreIndex.RemoveIndex(quadruple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByLiteral(RDFLiteral literalObject) {
            if (literalObject          != null) {
                foreach (var quadruple in this.SelectQuadruplesByLiteral(literalObject)) {
                    this.Quadruples.Remove(quadruple.QuadrupleID);
                    this.StoreIndex.RemoveIndex(quadruple);
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public override void ClearQuadruples() {
            this.Quadruples.Clear();
            this.StoreIndex.ClearIndex();
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public override Boolean ContainsQuadruple(RDFQuadruple quadruple) {
            return (quadruple != null && this.Quadruples.ContainsKey(quadruple.QuadrupleID));
        }

        /// <summary>
        /// Gets a store containing quadruples satisfying the given pattern
        /// </summary>
        internal override RDFMemoryStore SelectQuadruples(RDFContext contextResource, 
                                                          RDFResource subjectResource, 
                                                          RDFResource predicateResource, 
                                                          RDFResource objectResource, 
                                                          RDFLiteral  objectLiteral) {
            return (new RDFMemoryStore(RDFStoreUtilities.SelectQuadruples(this, contextResource, subjectResource, predicateResource, objectResource, objectLiteral)));
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection store from this store and a given one
        /// </summary>
        public RDFMemoryStore IntersectWith(RDFStore store) {
            var result = new RDFMemoryStore();
            if (store != null) {

                //Add intersection quadruples
                foreach (var q in this) {
                    if (store.ContainsQuadruple(q)) {
                        result.AddQuadruple(q);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Builds a new union store from this store and a given one
        /// </summary>
        public RDFMemoryStore UnionWith(RDFStore store) {
            var result = new RDFMemoryStore();

            //Add quadruples from this store
            foreach (var q in this) {
                result.AddQuadruple(q);
            }

            //Manage the given store
            if (store != null) {

                //Add quadruples from the given store
                foreach (var q in store.SelectAllQuadruples()) {
                    result.AddQuadruple(q);
                }

            }

            return result;
        }

        /// <summary>
        /// Builds a new difference store from this store and a given one
        /// </summary>
        public RDFMemoryStore DifferenceWith(RDFStore store) {
            var result = new RDFMemoryStore();
            if (store != null) {

                //Add difference quadruples
                foreach (var q in this) {
                    if (!store.ContainsQuadruple(q)) {
                         result.AddQuadruple(q);
                    }
                }

            }
            else {

                //Add quadruples from this store
                foreach (var q in this) {
                    result.AddQuadruple(q);
                }

            }
            return result;
        }
        #endregion

        #region Convert

        #region Importers
        /// <summary>
        /// Creates a memory store from a file of the given RDF format. 
        /// </summary>
        public static RDFMemoryStore FromFile(RDFStoreEnums.RDFFormats rdfFormat, String filepath) {
            if (filepath != null) {
                if (File.Exists(filepath)) {
                    switch  (rdfFormat) {
                        case RDFStoreEnums.RDFFormats.TriX:
                             return RDFTriX.Deserialize(filepath);
                        case RDFStoreEnums.RDFFormats.NQuads:
                             return RDFNQuads.Deserialize(filepath);
                    }
                }
                throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            }
            throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter is null or empty.");
        }

        /// <summary>
        /// Creates a memory store from a stream of the given RDF format. 
        /// </summary>
        public static RDFMemoryStore FromStream(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream) {
            if (inputStream != null) {
                switch  (rdfFormat) {
                    case RDFStoreEnums.RDFFormats.TriX:
                         return RDFTriX.Deserialize(inputStream);
                    case RDFStoreEnums.RDFFormats.NQuads:
                         return RDFNQuads.Deserialize(inputStream);
                }
            }
            throw new RDFStoreException("Cannot read RDF memory store from stream because given \"inputStream\" parameter is null.");
        }

        /// <summary>
        /// Creates a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        public static RDFMemoryStore FromDataTable(DataTable table) {
            var result = new RDFMemoryStore();

            //Check the structure of the datatable for consistency against the "C-S-P-O" RDF model
            if (table != null && table.Columns.Count == 4) {
                if (table.Columns.Contains("CONTEXT") && table.Columns.Contains("SUBJECT") && table.Columns.Contains("PREDICATE") && table.Columns.Contains("OBJECT")) {

                    //Iterate the rows of the datatable
                    foreach (DataRow tableRow in table.Rows) {

                        #region CONTEXT
                        //Parse the quadruple context
                        if(!tableRow.IsNull("CONTEXT") && tableRow["CONTEXT"].ToString() != String.Empty) {
                            var rowCont = RDFQueryUtilities.ParseRDFPatternMember(tableRow["CONTEXT"].ToString());
                            if (rowCont is RDFResource && !((RDFResource)rowCont).IsBlank) {

                                #region SUBJECT
                                //Parse the quadruple subject
                                if(!tableRow.IsNull("SUBJECT") && tableRow["SUBJECT"].ToString() != String.Empty) {
                                    var rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["SUBJECT"].ToString());
                                    if (rowSubj is RDFResource) {

                                        #region PREDICATE
                                        //Parse the quadruple predicate
                                        if(!tableRow.IsNull("PREDICATE") && tableRow["PREDICATE"].ToString() != String.Empty) {
                                            var rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["PREDICATE"].ToString());
                                            if (rowPred is RDFResource && !((RDFResource)rowPred).IsBlank) {

                                                #region OBJECT
                                                //Parse the quadruple object
                                                if(!tableRow.IsNull("OBJECT")) {
                                                    var rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["OBJECT"].ToString());
                                                    if (rowObj is RDFResource) {
                                                        result.AddQuadruple(new RDFQuadruple(new RDFContext(rowCont.ToString()), (RDFResource)rowSubj, (RDFResource)rowPred, (RDFResource)rowObj));
                                                    }
                                                    else {
                                                        result.AddQuadruple(new RDFQuadruple(new RDFContext(rowCont.ToString()), (RDFResource)rowSubj, (RDFResource)rowPred, (RDFLiteral)rowObj));
                                                    }
                                                }
                                                else {
                                                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL value in the \"OBJECT\" column.");
                                                }
                                                #endregion

                                            }
                                            else {
                                                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource or a literal in the \"PREDICATE\" column.");
                                            }
                                        }
                                        else {
                                            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL or empty value in the \"PREDICATE\" column.");
                                        }
                                        #endregion

                                    }
                                    else {
                                        throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"SUBJECT\" column.");
                                    }
                                }
                                else {
                                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL or empty value in the \"SUBJECT\" column.");
                                }
                                #endregion

                            }
                            else {
                                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource or a literal in the \"CONTEXT\" column.");
                            }
                        }
                        else {
                            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL or empty value in the \"CONTEXT\" column.");
                        }
                        #endregion

                    }

                }
                else {
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter does not have the required columns \"CONTEXT\", \"SUBJECT\", \"PREDICATE\", \"OBJECT\".");
                }
            }
            else {
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter is null, or it does not have exactly 4 columns.");
            }

            return result;
        }
        #endregion

        #endregion

        #endregion

    }

}