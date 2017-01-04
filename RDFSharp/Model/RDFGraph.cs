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
using RDFSharp.Query;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFGraph represents a graph in the RDF model.
    /// </summary>
    public class RDFGraph: RDFDataSource, IEquatable<RDFGraph>, IEnumerable<RDFTriple> {

        #region Properties
        /// <summary>
        /// Uri of the graph
        /// </summary>
        public Uri Context { get; internal set; }

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
        /// Builds an empty graph
        /// </summary>
        public RDFGraph() {
            this.Context       = RDFNamespaceRegister.DefaultNamespace.NamespaceUri;
            this.GraphMetadata = new RDFGraphMetadata();
            this.GraphIndex    = new RDFGraphIndex();
            this.Triples       = new Dictionary<Int64, RDFTriple>();
        }

        /// <summary>
        /// Builds a graph with the given list of triples
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
            foreach (var t in this) {
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
        /// Sets the context of the graph to the given Uri (null or blank-node Uris are not accepted)
        /// </summary>
        public RDFGraph SetContext(Uri contextUri) {
            if (contextUri  != null && !contextUri.ToString().ToUpperInvariant().StartsWith("BNODE:")) {
                this.Context = contextUri;
            }
            return this;
        }

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
        public void ClearTriples() {
            this.Triples.Clear();
            this.GraphIndex.ClearIndex();
            this.GraphMetadata.ClearMetadata();
        }

        /// <summary>
        /// Compacts the reified triples by removing their 4 standard statements
        /// </summary>
        public void UnreifyTriples() {

            //Create SPARQL SELECT query for detecting reified triples
            var T = new RDFVariable("T");
            var S = new RDFVariable("S");
            var P = new RDFVariable("P");
            var O = new RDFVariable("O");
            var Q = new RDFSelectQuery()
                            .AddPatternGroup(new RDFPatternGroup("UnreifyTriples")
                                .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT))
                                .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.SUBJECT, S))
                                .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.PREDICATE, P))
                                .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.OBJECT, O))
                                .AddFilter(new RDFIsUriFilter(T))
                                .AddFilter(new RDFIsUriFilter(S))
                                .AddFilter(new RDFIsUriFilter(P))
                             );

            //Apply it to the graph
            var R = Q.ApplyToGraph(this);

            //Iterate results
            var reifiedTriples = R.SelectResults.Rows.GetEnumerator();
            while (reifiedTriples.MoveNext()) {

                //Get reification data (T, S, P, O)
                var tRepresent = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?T"].ToString());
                var tSubject   = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?S"].ToString());
                var tPredicate = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?P"].ToString());
                var tObject    = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?O"].ToString());

                //Cleanup graph from detected reifications
                if (tObject   is RDFResource) {
                    this.AddTriple(new RDFTriple((RDFResource)tSubject, (RDFResource)tPredicate, (RDFResource)tObject));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFResource)tObject));
                }
                else {
                    this.AddTriple(new RDFTriple((RDFResource)tSubject, (RDFResource)tPredicate, (RDFLiteral)tObject));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    this.RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFLiteral)tObject));
                }

            }

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

        #region Exporters
        /// <summary>
        /// Writes the graph into a file in the given RDF format. 
        /// </summary>
        public void ToFile(RDFModelEnums.RDFFormats rdfFormat, String filepath) {
            if (filepath != null && filepath.Trim() != String.Empty) {
                switch  (rdfFormat) {
                    case RDFModelEnums.RDFFormats.NTriples:
                         RDFNTriples.Serialize(this, filepath);
                         break;
                    case RDFModelEnums.RDFFormats.RdfXml:
                         RDFXml.Serialize(this, filepath);
                         break;
                    case RDFModelEnums.RDFFormats.TriX:
                         RDFTriX.Serialize(this, filepath);
                         break;
                    case RDFModelEnums.RDFFormats.Turtle:
                         RDFTurtle.Serialize(this, filepath);
                         break;
                }
            }
            else {
                throw new RDFModelException("Cannot write RDF graph to file because given \"filepath\" parameter is null or empty.");
            }
        }

        /// <summary>
        /// Writes the graph into a stream in the given RDF format. 
        /// </summary>
        public void ToStream(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream) {
            if (outputStream != null) {
                switch  (rdfFormat) {
                    case RDFModelEnums.RDFFormats.NTriples:
                         RDFNTriples.Serialize(this, outputStream);
                         break;
                    case RDFModelEnums.RDFFormats.RdfXml:
                         RDFXml.Serialize(this, outputStream);
                         break;
                    case RDFModelEnums.RDFFormats.TriX:
                         RDFTriX.Serialize(this, outputStream);
                         break;
                    case RDFModelEnums.RDFFormats.Turtle:
                         RDFTurtle.Serialize(this, outputStream);
                         break;
                }
            }
            else {
                throw new RDFModelException("Cannot write RDF graph to stream because given \"outputStream\" parameter is null.");
            }
        }

        /// <summary>
        /// Writes the graph into a datatable with "Subject-Predicate-Object" columns
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
        #endregion

        #region Importers
        /// <summary>
        /// Creates a graph from a file of the given RDF format. 
        /// </summary>
        public static RDFGraph FromFile(RDFModelEnums.RDFFormats rdfFormat, String filepath) {
            if (filepath != null) {
                if (File.Exists(filepath)) {
                    switch  (rdfFormat) {
                        case RDFModelEnums.RDFFormats.NTriples:
                             return RDFNTriples.Deserialize(filepath);
                        case RDFModelEnums.RDFFormats.RdfXml:
                             return RDFXml.Deserialize(filepath);
                        case RDFModelEnums.RDFFormats.TriX:
                             return RDFTriX.Deserialize(filepath);
                        case RDFModelEnums.RDFFormats.Turtle:
                             throw new RDFModelException("Cannot read RDF graph from file because Turtle format is not supported. What about joining the project to contribute it?");
                    }
                }
                throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            }
            throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter is null or empty.");
        }

        /// <summary>
        /// Creates a graph from a stream of the given RDF format. 
        /// </summary>
        public static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream) {
            if (inputStream != null) {
                switch  (rdfFormat) {
                    case RDFModelEnums.RDFFormats.NTriples:
                         return RDFNTriples.Deserialize(inputStream);
                    case RDFModelEnums.RDFFormats.RdfXml:
                         return RDFXml.Deserialize(inputStream);
                    case RDFModelEnums.RDFFormats.TriX:
                         return RDFTriX.Deserialize(inputStream);
                    case RDFModelEnums.RDFFormats.Turtle:
                         throw new RDFModelException("Cannot read RDF graph from stream because Turtle format is not supported. What about joining the project to contribute it?");
                }
            }
            throw new RDFModelException("Cannot read RDF graph from stream because given \"filepath\" parameter is null.");
        }

        /// <summary>
        /// Creates a graph from a datatable with "Subject-Predicate-Object" columns.
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
                        result.SetContext(graphUri);
                    }
                    #endregion

                    //Iterate the rows of the datatable
                    foreach (DataRow tableRow in table.Rows) {

                        #region SUBJECT
                        //Parse the triple subject
                        if(!tableRow.IsNull("SUBJECT") && tableRow["SUBJECT"].ToString() != String.Empty) {
                            var rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["SUBJECT"].ToString());
                            if (rowSubj is RDFResource) {

                                #region PREDICATE
                                //Parse the triple predicate
                                if(!tableRow.IsNull("PREDICATE")  && tableRow["PREDICATE"].ToString() != String.Empty) {
                                    var rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["PREDICATE"].ToString());
                                    if (rowPred is RDFResource && !((RDFResource)rowPred).IsBlank) {

                                        #region OBJECT
                                        //Parse the triple object
                                        if(!tableRow.IsNull("OBJECT")) {
                                            var rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["OBJECT"].ToString());
                                            if (rowObj is RDFResource) {
                                                result.AddTriple(new RDFTriple((RDFResource)rowSubj, (RDFResource)rowPred, (RDFResource)rowObj));
                                            }
                                            else {
                                                result.AddTriple(new RDFTriple((RDFResource)rowSubj, (RDFResource)rowPred, (RDFLiteral)rowObj));
                                            }
                                        }
                                        else {
                                            throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having NULL value in the \"OBJECT\" column.");
                                        }
                                        #endregion

                                    }
                                    else {
                                        throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having a blank resource or a literal in the \"PREDICATE\" column.");
                                    }
                                }
                                else {
                                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"PREDICATE\" column.");
                                }
                                #endregion

                            }
                            else {
                                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"SUBJECT\" column.");
                            }
                        }
                        else {
                            throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"SUBJECT\" column.");
                        }
                        #endregion

                    }

                }
                else {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter does not have the required columns \"SUBJECT\", \"PREDICATE\", \"OBJECT\".");
                }
            }
            else {
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter is null, or it does not have exactly 3 columns.");
            }

            return result;
        }
        #endregion

        #endregion

        #endregion

    }

}