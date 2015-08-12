/*
   Copyright 2012-2015 Marco De Salvo

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
using System.Linq;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFGraph represents a graph in the RDF model.
    /// </summary>
    public class RDFGraph: IEquatable<RDFGraph>, IEnumerable<RDFTriple> {

        #region Data
        private Uri context;
        #endregion

        #region Properties
        /// <summary>
        /// Uri of the graph
        /// </summary>
        public Uri Context {
            get {
                return this.context;
            } 
            set {
                this.context = (value ?? RDFNamespaceRegister.DefaultNamespace.Namespace);
            } 
        }

        /// <summary>
        /// Count of the graph's triples
        /// </summary>
        public Int64 TriplesCount {
            get { return this.Triples.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the graph's triples for iteration
        /// </summary>
        public IEnumerator<RDFTriple> TriplesEnumerator  {
            get { return this.Triples.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Metadata of the graph
        /// </summary>
        internal RDFGraphMetadata GraphMetadata { get; set; }

        /// <summary>
        /// Index on the triples of the graph
        /// </summary>
        internal RDFGraphIndex GraphIndex { get; set; }

        /// <summary>
        /// List of triples embedded into the graph
        /// </summary>
        internal Dictionary<Int64, RDFTriple>  Triples { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty graph
        /// </summary>
        public RDFGraph() {
            this.Context       = RDFNamespaceRegister.DefaultNamespace.Namespace;
            this.GraphMetadata = new RDFGraphMetadata();
            this.GraphIndex    = new RDFGraphIndex();
            this.Triples       = new Dictionary<Int64, RDFTriple>();
        }

        /// <summary>
        /// List-based ctor to build a graph with the given list of triples
        /// </summary>
        public RDFGraph(List<RDFTriple> triples): this() {
            if (triples != null) {
                triples.ForEach(t => this.AddTriple(t));
            }
        }
        #endregion
        
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the graph
        /// </summary>
        public override String ToString() {
            return this.Context.ToString();
        }

        /// <summary>
        /// Performs the equality comparison between two graphs
        /// </summary>
        public Boolean Equals(RDFGraph other) {
            if (other == null || this.TriplesCount != other.TriplesCount) {
                return false;
            }
            foreach (RDFTriple t in this) {
                if (!other.ContainsTriple(t)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the graph's triples
        /// </summary>
        IEnumerator<RDFTriple> IEnumerable<RDFTriple>.GetEnumerator() {
            return this.Triples.Values.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the graph's triples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Triples.Values.GetEnumerator();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given triple to the graph, avoiding duplicate insertions
        /// </summary>
        public RDFGraph AddTriple(RDFTriple triple) {
            if (triple != null) {
                if (!this.Triples.ContainsKey(triple.TripleID)) {
                     this.Triples.Add(triple.TripleID, triple);
                     this.GraphIndex.AddIndex(triple);
                     this.GraphMetadata.UpdateMetadata(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given container to the graph
        /// </summary>
        public RDFGraph AddContainer(RDFContainer container) {
            if (container     != null) {
                //Reify the container to get its graph representation
                var reifCont   = container.ReifyContainer();
                //Iterate on the constructed triples
                foreach(var t in reifCont) {
                    this.AddTriple(t);
                }                
            }
            return this;
        }

        /// <summary>
        /// Adds the given collection to the graph
        /// </summary>
        public RDFGraph AddCollection(RDFCollection collection) {
            if (collection    != null) {
                //Reify the collection to get its graph representation
                var reifColl   = collection.ReifyCollection();
                //Iterate on the constructed triples
                foreach(var t in reifColl) {
                    this.AddTriple(t);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given triple from the graph
        /// </summary>
        public RDFGraph RemoveTriple(RDFTriple triple) {
            if (this.ContainsTriple(triple)) {
                this.Triples.Remove(triple.TripleID);
                RDFModelUtilities.RebuildGraph(this);
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given subject
        /// </summary>
        public RDFGraph RemoveTriplesBySubject(RDFResource subjectResource) {
            if (subjectResource != null) {
                var tripleFound  = false;
                foreach (var triple in this.SelectTriplesBySubject(subjectResource)) {
                    this.Triples.Remove(triple.TripleID);
                    tripleFound  = true;
                }
                if (tripleFound) {
                    RDFModelUtilities.RebuildGraph(this);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given (non-blank) predicate
        /// </summary>
        public RDFGraph RemoveTriplesByPredicate(RDFResource predicateResource) {
            if (predicateResource  != null && !predicateResource.IsBlank) {
                var tripleFound     = false;
                foreach (var triple in this.SelectTriplesByPredicate(predicateResource)) {
                    this.Triples.Remove(triple.TripleID);
                    tripleFound     = true;
                }
                if (tripleFound) {
                    RDFModelUtilities.RebuildGraph(this);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given resource as object
        /// </summary>
        public RDFGraph RemoveTriplesByObject(RDFResource objectResource) {
            if (objectResource  != null) {
                var tripleFound  = false;
                foreach (var triple in this.SelectTriplesByObject(objectResource)) {
                    this.Triples.Remove(triple.TripleID);
                    tripleFound  = true;
                }
                if (tripleFound) {
                    RDFModelUtilities.RebuildGraph(this);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given literal as object
        /// </summary>
        public RDFGraph RemoveTriplesByLiteral(RDFLiteral objectLiteral) {
            if (objectLiteral  != null) {
                var tripleFound = false;
                foreach (var triple in this.SelectTriplesByLiteral(objectLiteral)) {
                    this.Triples.Remove(triple.TripleID);
                    tripleFound = true;
                }
                if (tripleFound) {
                    RDFModelUtilities.RebuildGraph(this);
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the triples and metadata of the graph
        /// </summary>
        public RDFGraph ClearTriples() {
            this.Triples.Clear();
            this.GraphIndex.ClearIndex();
            this.GraphMetadata.ClearMetadata();
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the graph contains the given triple
        /// </summary>
        public Boolean ContainsTriple(RDFTriple triple) {
            return (triple != null && this.Triples.ContainsKey(triple.TripleID));
        }

        /// <summary>
        /// Gets the subgraph containing triples with the specified resource as subject 
        /// </summary>
        public RDFGraph SelectTriplesBySubject(RDFResource subjectResource) {
            return (new RDFGraph(RDFModelUtilities.SelectTriples(this, subjectResource, null, null, null)));
        }

        /// <summary>
        /// Gets the subgraph containing triples with the specified resource as predicate
        /// </summary>
        public RDFGraph SelectTriplesByPredicate(RDFResource predicateResource) {
            return (new RDFGraph(RDFModelUtilities.SelectTriples(this, null, predicateResource, null, null)));
        }

        /// <summary>
        /// Gets the subgraph containing triples with the specified resource as object 
        /// </summary>
        public RDFGraph SelectTriplesByObject(RDFResource objectResource) {
            return (new RDFGraph(RDFModelUtilities.SelectTriples(this, null, null, objectResource, null)));
        }

        /// <summary>
        /// Gets the subgraph containing triples with the specified literal as object 
        /// </summary>
        public RDFGraph SelectTriplesByLiteral(RDFLiteral objectLiteral) {
            return (new RDFGraph(RDFModelUtilities.SelectTriples(this, null, null, null, objectLiteral)));
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection graph from this graph and a given one
        /// </summary>
        public RDFGraph IntersectWith(RDFGraph graph) {
            var result = new RDFGraph();
            if (graph != null) {

                //Add intersection triples
                foreach(var t in this) {
                    if (graph.ContainsTriple(t)) {
                        result.AddTriple(t);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Builds a new union graph from this graph and a given one
        /// </summary>
        public RDFGraph UnionWith(RDFGraph graph) {
            var result = new RDFGraph();

            //Add triples from this graph
            foreach (var t in this) {
                result.AddTriple(t);
            }

            //Manage the given graph
            if (graph != null) {

                //Add triples from the given graph
                foreach(var t in graph) {
                    result.AddTriple(t);
                }

            }

            return result;
        }

        /// <summary>
        /// Builds a new difference graph from this graph and a given one
        /// </summary>
        public RDFGraph DifferenceWith(RDFGraph graph) {
            var result = new RDFGraph();
            if (graph != null) {

                //Add difference triples
                foreach(var t in this) {
                    if (!graph.ContainsTriple(t)) {
                        result.AddTriple(t);
                    }
                }

            }
            else {

                //Add triples from this graph
                foreach (var t in this) {
                    result.AddTriple(t);
                }

            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Converts the graph into a "Subject-Predicate-Object" datatable 
        /// </summary>
        public DataTable ToDataTable() {

            //Create the structure of the result datatable
            var result = new DataTable(this.ToString());
            result.Columns.Add("SUBJECT",   Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT",    Type.GetType("System.String"));
            result.AcceptChanges();

            //Iterate the triples of the graph to populate the result datatable
            result.BeginLoadData();
            foreach (var t in this) {
                var newRow          = result.NewRow();
                newRow["SUBJECT"]   = t.Subject.ToString();
                newRow["PREDICATE"] = t.Predicate.ToString();
                newRow["OBJECT"]    = t.Object.ToString();
                newRow.AcceptChanges();
                result.Rows.Add(newRow);
            }
            result.EndLoadData();

            return result;
        }

        /// <summary>
        /// Creates a graph from a "Subject-Predicate-Object" datatable 
        /// </summary>
        public static RDFGraph FromDataTable(DataTable table) {
            var result = new RDFGraph();

            //Check the structure of the datatable for consistency against the "S-P-O" RDF model
            if (table != null && table.Columns.Count == 3) {
                if (table.Columns.Contains("SUBJECT") && table.Columns.Contains("PREDICATE") && table.Columns.Contains("OBJECT")) {

                    #region CONTEXT
                    //Parse the name of the datatable for Uri, in order to assign the graph name
                    Uri graphUri;
                    if (Uri.TryCreate(table.TableName, UriKind.Absolute, out graphUri)) {
                        result.Context = graphUri;
                    }
                    #endregion

                    //Iterate the rows of the datatable
                    foreach (DataRow tableRow in table.Rows) {

                        #region SUBJECT
                        //Parse the triple subject
                        if (!tableRow.IsNull("SUBJECT") && tableRow["SUBJECT"].ToString() != String.Empty) {
                            var rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["SUBJECT"].ToString());
                            if (rowSubj is RDFResource) {

                                #region PREDICATE
                                //Parse the triple predicate
                                if (!tableRow.IsNull("PREDICATE")  && tableRow["PREDICATE"].ToString() != String.Empty) {
                                    var rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["PREDICATE"].ToString());
                                    if (rowPred is RDFResource && !((RDFResource)rowPred).IsBlank) {

                                        #region OBJECT
                                        //Parse the triple object
                                        if (!tableRow.IsNull("OBJECT")) {
                                            var rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["OBJECT"].ToString());
                                            if (rowObj is RDFResource) {
                                                result.AddTriple(new RDFTriple((RDFResource)rowSubj, (RDFResource)rowPred, (RDFResource)rowObj));
                                            }
                                            else {
                                                result.AddTriple(new RDFTriple((RDFResource)rowSubj, (RDFResource)rowPred, (RDFLiteral)rowObj));
                                            }
                                        }
                                        else {
                                            throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter contains a row having NULL value in the \"OBJECT\" column.");
                                        }
                                        #endregion

                                    }
                                    else {
                                        throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter contains a row not having a resource, or having a blank resource, in the \"PREDICATE\" column.");
                                    }
                                }
                                else {
                                    throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter contains a row having null or empty value in the \"PREDICATE\" column.");
                                }
                                #endregion

                            }
                            else {
                                throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter contains a row not having a resource in the \"SUBJECT\" column.");
                            }
                        }
                        else {
                            throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter contains a row having null or empty value in the \"SUBJECT\" column.");
                        }
                        #endregion

                    }

                }
                else {
                    throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter does not have the required 3 columns: \"SUBJECT\", \"PREDICATE\", \"OBJECT\".");
                }
            }
            else {
                throw new RDFModelException("Cannot create RDFGraph because given \"table\" parameter is null, or it does not have exactly 3 columns.");
            }

            return result;
        }
        #endregion

        #endregion

    }

}