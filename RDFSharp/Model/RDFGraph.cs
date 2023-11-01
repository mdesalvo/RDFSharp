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

using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFGraph represents an Uri-named collection of triples in the RDF model
    /// </summary>
    public class RDFGraph : RDFDataSource, IEquatable<RDFGraph>, IEnumerable<RDFTriple>, IDisposable
    {
        #region Properties
        /// <summary>
        /// Uri of the graph
        /// </summary>
        public Uri Context { get; internal set; }

        /// <summary>
        /// Count of the graph's triples
        /// </summary>
        public long TriplesCount 
            => IndexedTriples.Count;

        /// <summary>
        /// Gets the enumerator on the graph's triples for iteration
        /// </summary>
        public IEnumerator<RDFTriple> TriplesEnumerator 
        { 
            get 
            {
                foreach (RDFIndexedTriple indexedTriple in IndexedTriples.Values)
                {
                    yield return indexedTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                        ? new RDFTriple(GraphIndex.ResourcesRegister[indexedTriple.SubjectID], GraphIndex.ResourcesRegister[indexedTriple.PredicateID], GraphIndex.ResourcesRegister[indexedTriple.ObjectID]) { TripleMetadata = indexedTriple.TripleMetadata }
                        : new RDFTriple(GraphIndex.ResourcesRegister[indexedTriple.SubjectID], GraphIndex.ResourcesRegister[indexedTriple.PredicateID], GraphIndex.LiteralsRegister[indexedTriple.ObjectID]) { TripleMetadata = indexedTriple.TripleMetadata };
                }
            } 
        }

        /// <summary>
        /// Index on the triples of the graph
        /// </summary>
        internal RDFGraphIndex GraphIndex { get; set; }

        /// <summary>
        /// Indexed triples embedded into the graph
        /// </summary>
        internal Dictionary<long, RDFIndexedTriple> IndexedTriples { get; set; }

        /// <summary>
        /// Flag indicating that the graph has already been disposed
        /// </summary>
        internal bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty graph
        /// </summary>
        public RDFGraph()
        {
            Context = RDFNamespaceRegister.DefaultNamespace.NamespaceUri;
            GraphIndex = new RDFGraphIndex();
            IndexedTriples = new Dictionary<long, RDFIndexedTriple>();
        }

        /// <summary>
        /// Builds a graph with the given list of triples
        /// </summary>
        public RDFGraph(List<RDFTriple> triples) : this()
            => triples?.ForEach(t => AddTriple(t));

        /// <summary>
        /// Destroys the graph instance
        /// </summary>
        ~RDFGraph() 
            => Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the graph
        /// </summary>
        public override string ToString() 
            => Context.ToString();

        /// <summary>
        /// Performs the equality comparison between two graphs
        /// </summary>
        public bool Equals(RDFGraph other)
        {
            if (other == null || TriplesCount != other.TriplesCount)
                return false;

            foreach (RDFTriple t in this)
            {
                if (!other.ContainsTriple(t))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the graph's triples
        /// </summary>
        IEnumerator<RDFTriple> IEnumerable<RDFTriple>.GetEnumerator() 
            => TriplesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the graph's triples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() 
            => TriplesEnumerator;

        /// <summary>
        /// Disposes the graph (IDisposable)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the graph
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                GraphIndex.Dispose();
                GraphIndex = null;

                IndexedTriples.Clear();
                IndexedTriples = null;
            }

            Disposed = true;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Sets the context of the graph to the given absolute Uri
        /// </summary>
        public RDFGraph SetContext(Uri contextUri)
        {
            if (contextUri != null
                  && contextUri.IsAbsoluteUri
                    && !contextUri.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase)
                      && !contextUri.ToString().StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase))
                Context = contextUri;
            return this;
        }

        /// <summary>
        /// Adds the given triple to the graph, avoiding duplicate insertions
        /// </summary>
        public RDFGraph AddTriple(RDFTriple triple)
        {
            if (triple != null && !IndexedTriples.ContainsKey(triple.TripleID))
            {
                //Add triple
                IndexedTriples.Add(triple.TripleID, new RDFIndexedTriple(triple));
                //Add index
                GraphIndex.AddIndex(triple);
            }
            return this;
        }

        /// <summary>
        /// Adds the given container to the graph
        /// </summary>
        public RDFGraph AddContainer(RDFContainer container)
        {
            if (container != null)
            {
                //Reify the container to get its graph representation
                RDFGraph reifCont = container.ReifyContainer();
                //Iterate on the constructed triples
                foreach (RDFTriple t in reifCont)
                    AddTriple(t);
            }
            return this;
        }

        /// <summary>
        /// Adds the given collection to the graph
        /// </summary>
        public RDFGraph AddCollection(RDFCollection collection)
        {
            if (collection != null)
            {
                //Reify the collection to get its graph representation
                RDFGraph reifColl = collection.ReifyCollection();
                //Iterate on the constructed triples
                foreach (RDFTriple t in reifColl)
                    AddTriple(t);
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given triple from the graph
        /// </summary>
        public RDFGraph RemoveTriple(RDFTriple triple)
        {
            if (triple != null)
            {
                //Remove triple
                IndexedTriples.Remove(triple.TripleID);
                //Remove index
                GraphIndex.RemoveIndex(triple);
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given subject
        /// </summary>
        public RDFGraph RemoveTriplesBySubject(RDFResource subjectResource)
        {
            if (subjectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given predicate
        /// </summary>
        public RDFGraph RemoveTriplesByPredicate(RDFResource predicateResource)
        {
            if (predicateResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesByPredicate(predicateResource))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given object
        /// </summary>
        public RDFGraph RemoveTriplesByObject(RDFResource objectResource)
        {
            if (objectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesByObject(objectResource))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given literal
        /// </summary>
        public RDFGraph RemoveTriplesByLiteral(RDFLiteral objectLiteral)
        {
            if (objectLiteral != null)
            {
                foreach (RDFTriple triple in SelectTriplesByLiteral(objectLiteral))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given subject and predicate
        /// </summary>
        public RDFGraph RemoveTriplesBySubjectPredicate(RDFResource subjectResource, RDFResource predicateResource)
        {
            if (subjectResource != null && predicateResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource)
                                               .SelectTriplesByPredicate(predicateResource))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given subject and object
        /// </summary>
        public RDFGraph RemoveTriplesBySubjectObject(RDFResource subjectResource, RDFResource objectResource)
        {
            if (subjectResource != null && objectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource)
                                               .SelectTriplesByObject(objectResource))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given subject and literal
        /// </summary>
        public RDFGraph RemoveTriplesBySubjectLiteral(RDFResource subjectResource, RDFLiteral objectLiteral)
        {
            if (subjectResource != null && objectLiteral != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource)
                                               .SelectTriplesByLiteral(objectLiteral))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given predicate and object
        /// </summary>
        public RDFGraph RemoveTriplesByPredicateObject(RDFResource predicateResource, RDFResource objectResource)
        {
            if (predicateResource != null && objectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesByPredicate(predicateResource)
                                               .SelectTriplesByObject(objectResource))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the triples with the given predicate and literal
        /// </summary>
        public RDFGraph RemoveTriplesByPredicateLiteral(RDFResource predicateResource, RDFLiteral objectLiteral)
        {
            if (predicateResource != null && objectLiteral != null)
            {
                foreach (RDFTriple triple in SelectTriplesByPredicate(predicateResource)
                                               .SelectTriplesByLiteral(objectLiteral))
                {
                    //Remove triple
                    IndexedTriples.Remove(triple.TripleID);
                    //Remove index
                    GraphIndex.RemoveIndex(triple);
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the triples and metadata of the graph
        /// </summary>
        public void ClearTriples()
        {
            //Clear triples
            IndexedTriples.Clear();
            //Clear index
            GraphIndex.ClearIndex();
        }

        /// <summary>
        /// Turns back the reified triples into their compact representation
        /// </summary>
        public void UnreifyTriples()
        {
            //Create SPARQL SELECT query for detecting reified triples
            RDFVariable T = new RDFVariable("T");
            RDFVariable S = new RDFVariable("S");
            RDFVariable P = new RDFVariable("P");
            RDFVariable O = new RDFVariable("O");
            RDFSelectQuery Q = new RDFSelectQuery()
                                    .AddPatternGroup(new RDFPatternGroup()
                                        .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT))
                                        .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.SUBJECT, S))
                                        .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.PREDICATE, P))
                                        .AddPattern(new RDFPattern(T, RDFVocabulary.RDF.OBJECT, O))
                                        .AddFilter(new RDFIsUriFilter(T))
                                        .AddFilter(new RDFIsUriFilter(S))
                                        .AddFilter(new RDFIsUriFilter(P))
                                    );

            //Apply it to the graph
            RDFSelectQueryResult R = Q.ApplyToGraph(this);

            //Iterate results
            IEnumerator reifiedTriples = R.SelectResults.Rows.GetEnumerator();
            while (reifiedTriples.MoveNext())
            {
                //Get reification data (T, S, P, O)
                RDFPatternMember tRepresent = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?T"].ToString());
                RDFPatternMember tSubject = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?S"].ToString());
                RDFPatternMember tPredicate = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?P"].ToString());
                RDFPatternMember tObject = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)reifiedTriples.Current)["?O"].ToString());

                //Cleanup graph from detected reifications
                if (tObject is RDFResource)
                {
                    AddTriple(new RDFTriple((RDFResource)tSubject, (RDFResource)tPredicate, (RDFResource)tObject));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFResource)tObject));
                }
                else
                {
                    AddTriple(new RDFTriple((RDFResource)tSubject, (RDFResource)tPredicate, (RDFLiteral)tObject));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.SUBJECT, (RDFResource)tSubject));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.PREDICATE, (RDFResource)tPredicate));
                    RemoveTriple(new RDFTriple((RDFResource)tRepresent, RDFVocabulary.RDF.OBJECT, (RDFLiteral)tObject));
                }
            }
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the graph contains the given triple
        /// </summary>
        public bool ContainsTriple(RDFTriple triple)
            => triple != null && IndexedTriples.ContainsKey(triple.TripleID);

        /// <summary>
        /// Gets the subgraph containing triples with the specified subject
        /// </summary>
        public RDFGraph SelectTriplesBySubject(RDFResource subjectResource)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, subjectResource, null, null, null));

        /// <summary>
        /// Gets the subgraph containing triples with the specified predicate
        /// </summary>
        public RDFGraph SelectTriplesByPredicate(RDFResource predicateResource)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, null, predicateResource, null, null));

        /// <summary>
        /// Gets the subgraph containing triples with the specified object
        /// </summary>
        public RDFGraph SelectTriplesByObject(RDFResource objectResource)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, null, null, objectResource, null));

        /// <summary>
        /// Gets the subgraph containing triples with the specified literal
        /// </summary>
        public RDFGraph SelectTriplesByLiteral(RDFLiteral objectLiteral)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, null, null, null, objectLiteral));

        /// <summary>
        /// Gets the subgraph containing triples with the specified combination of SPOL accessors<br/>
        /// (null values are threated as * selectors. Ensure to keep object and literal mutually exclusive!)
        /// </summary>
        public RDFGraph this[RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit]
        {
            get
            {
                if (obj != null && lit != null)
                    throw new RDFModelException("Cannot access a graph when both object and literals are given: they have to be mutually exclusive!");
                return new RDFGraph(RDFModelUtilities.SelectTriples(this, subj, pred, obj, lit));
            }
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds an intersection graph from this graph and a given one
        /// </summary>
        public RDFGraph IntersectWith(RDFGraph graph)
        {
            RDFGraph result = new RDFGraph();
            if (graph != null)
            {
                //Add intersection triples
                foreach (RDFTriple t in this)
                {
                    if (graph.IndexedTriples.ContainsKey(t.TripleID))
                        result.AddTriple(t);
                }
            }
            return result;
        }

        /// <summary>
        /// Builds a union graph from this graph and a given one
        /// </summary>
        public RDFGraph UnionWith(RDFGraph graph)
        {
            RDFGraph result = new RDFGraph();

            //Add triples from this graph
            foreach (RDFTriple t in this)
                result.AddTriple(t);
            
            //Manage the given graph
            if (graph != null)
            {
                //Add triples from the given graph
                foreach (RDFTriple t in graph)
                    result.AddTriple(t);
            }

            return result;
        }

        /// <summary>
        /// Builds a difference graph from this graph and a given one
        /// </summary>
        public RDFGraph DifferenceWith(RDFGraph graph)
        {
            RDFGraph result = new RDFGraph();
            if (graph != null)
            {
                //Add difference triples
                foreach (RDFTriple t in this)
                {
                    if (!graph.IndexedTriples.ContainsKey(t.TripleID))
                        result.AddTriple(t);
                }
            }
            else
            {
                //Add triples from this graph
                foreach (RDFTriple t in this)
                    result.AddTriple(t);
            }
            return result;
        }
        #endregion

        #region Convert

        #region Export
        /// <summary>
        /// Writes the graph into a file in the given RDF format.
        /// </summary>
        public void ToFile(RDFModelEnums.RDFFormats rdfFormat, string filepath)
        {
            #region Guards
            if (string.IsNullOrEmpty(filepath))
                throw new RDFModelException("Cannot write RDF graph to file because given \"filepath\" parameter is null or empty.");
            #endregion

            switch (rdfFormat)
            {
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

        /// <summary>
        /// Writes the graph into a stream in the given RDF format (at the end the stream is closed).
        /// </summary>
        public void ToStream(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream)
        {
            #region Guards
            if (outputStream == null)
                throw new RDFModelException("Cannot write RDF graph to stream because given \"outputStream\" parameter is null.");
            #endregion

            switch (rdfFormat)
            {
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

        /// <summary>
        /// Writes the graph into a datatable with "Subject-Predicate-Object" columns
        /// </summary>
        public DataTable ToDataTable()
        {
            //Create the structure of the result datatable
            DataTable result = new DataTable(ToString());
            result.Columns.Add("?SUBJECT", RDFQueryEngine.SystemString);
            result.Columns.Add("?PREDICATE", RDFQueryEngine.SystemString);
            result.Columns.Add("?OBJECT", RDFQueryEngine.SystemString);

            //Iterate the triples of the graph to populate the result datatable
            result.BeginLoadData();
            foreach (RDFTriple t in this)
            {
                DataRow newRow = result.NewRow();
                newRow["?SUBJECT"] = t.Subject.ToString();
                newRow["?PREDICATE"] = t.Predicate.ToString();
                newRow["?OBJECT"] = t.Object.ToString();
                result.Rows.Add(newRow);
            }
            result.EndLoadData();

            return result;
        }
        #endregion

        #region Import
        /// <summary>
        /// Reads a graph from a file of the given RDF format.
        /// </summary>
        public static RDFGraph FromFile(RDFModelEnums.RDFFormats rdfFormat, string filepath)
        {
            #region Guards
            if (string.IsNullOrEmpty(filepath))
                throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter is null or empty.");
            if (!File.Exists(filepath))
                throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            #endregion

            switch (rdfFormat)
            {
                case RDFModelEnums.RDFFormats.RdfXml:
                    return RDFXml.Deserialize(filepath);
                case RDFModelEnums.RDFFormats.Turtle:
                    return RDFTurtle.Deserialize(filepath);
                case RDFModelEnums.RDFFormats.NTriples:
                    return RDFNTriples.Deserialize(filepath);
                case RDFModelEnums.RDFFormats.TriX:
                    return RDFTriX.Deserialize(filepath);
                default:
                    throw new RDFModelException("Cannot read RDF graph from file because given \"rdfFormat\" parameter is not supported.");
            }
        }

        /// <summary>
        /// Reads a graph from a stream of the given RDF format.
        /// </summary>
        public static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream) => FromStream(rdfFormat, inputStream, null);
        internal static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, Uri graphContext)
        {
            #region Guards
            if (inputStream == null)
                throw new RDFModelException("Cannot read RDF graph from stream because given \"inputStream\" parameter is null.");
            #endregion

            switch (rdfFormat)
            {
                case RDFModelEnums.RDFFormats.RdfXml:
                    return RDFXml.Deserialize(inputStream, graphContext);
                case RDFModelEnums.RDFFormats.Turtle:
                    return RDFTurtle.Deserialize(inputStream, graphContext);
                case RDFModelEnums.RDFFormats.NTriples:
                    return RDFNTriples.Deserialize(inputStream, graphContext);
                case RDFModelEnums.RDFFormats.TriX:
                    return RDFTriX.Deserialize(inputStream, graphContext);
                default:
                    throw new RDFModelException("Cannot read RDF graph from stream because given \"rdfFormat\" parameter is not supported.");
            }
        }

        /// <summary>
        /// Reads a graph from a datatable with "Subject-Predicate-Object" columns.
        /// </summary>
        public static RDFGraph FromDataTable(DataTable table)
        {
            #region Guards
            if (table == null)
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter is null.");
            if (table.Columns.Count != 3)
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter does not have exactly 3 columns.");
            if (!(table.Columns.Contains("?SUBJECT") && table.Columns.Contains("?PREDICATE") && table.Columns.Contains("?OBJECT")))
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter does not have the required columns \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");
            #endregion

            RDFGraph result = new RDFGraph();

            #region CONTEXT
            //Parse the name of the datatable for Uri, in order to assign the graph name
            if (Uri.TryCreate(table.TableName, UriKind.Absolute, out Uri graphUri))
                result.SetContext(graphUri);
            #endregion

            //Iterate the rows of the datatable
            foreach (DataRow tableRow in table.Rows)
            {
                #region SUBJECT
                if (tableRow.IsNull("?SUBJECT") || string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"?SUBJECT\" column.");

                RDFPatternMember rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
                if (!(rowSubj is RDFResource))
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
                #endregion

                #region PREDICATE
                if (tableRow.IsNull("?PREDICATE") || string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"?PREDICATE\" column.");

                RDFPatternMember rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
                if (!(rowPred is RDFResource))
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"?PREDICATE\" column.");
                if (((RDFResource)rowPred).IsBlank)
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having a blank resource in the \"?PREDICATE\" column.");
                #endregion

                #region OBJECT
                if (tableRow.IsNull("?OBJECT"))
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null value in the \"?OBJECT\" column.");

                RDFPatternMember rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
                if (rowObj is RDFResource)
                    result.AddTriple(new RDFTriple((RDFResource)rowSubj, (RDFResource)rowPred, (RDFResource)rowObj));
                else
                    result.AddTriple(new RDFTriple((RDFResource)rowSubj, (RDFResource)rowPred, (RDFLiteral)rowObj));
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Reads a graph by trying to dereference the given Uri
        /// </summary>
        public static RDFGraph FromUri(Uri uri, int timeoutMilliseconds = 20000)
        {
            #region Guards
            if (uri == null)
                throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter is null.");
            if (!uri.IsAbsoluteUri)
                throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter does not represent an absolute Uri.");
            #endregion

            RDFGraph result = new RDFGraph();
            try
            {
                //Grab eventual dereference Uri
                Uri remappedUri = RDFModelUtilities.RemapUriForDereference(uri);

                HttpWebRequest webRequest = WebRequest.CreateHttp(remappedUri);
                webRequest.MaximumAutomaticRedirections = 4;
                webRequest.AllowAutoRedirect = true;
                webRequest.Timeout = timeoutMilliseconds;
                //RDF/XML
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/rdf+xml");
                //TURTLE
                webRequest.Headers.Add(HttpRequestHeader.Accept, "text/turtle");
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/x-turtle");
                //N-TRIPLES
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/n-triples");
                //TRIX
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/trix");

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webRequest.HaveResponse)
                {
                    //Cascade detection of ContentType from response
                    string responseContentType = webResponse.ContentType;
                    if (string.IsNullOrWhiteSpace(responseContentType))
                    {
                        responseContentType = webResponse.Headers["ContentType"];
                        if (string.IsNullOrWhiteSpace(responseContentType))
                            responseContentType = "application/rdf+xml"; //Fallback to RDF/XML
                    }

                    //RDF/XML
                    if (responseContentType.Contains("application/rdf+xml"))
                        result = FromStream(RDFModelEnums.RDFFormats.RdfXml, webResponse.GetResponseStream(), webRequest.Address);

                    //TURTLE
                    else if (responseContentType.Contains("text/turtle") ||
                                responseContentType.Contains("application/turtle") ||
                                   responseContentType.Contains("application/x-turtle"))
                        result = FromStream(RDFModelEnums.RDFFormats.Turtle, webResponse.GetResponseStream(), webRequest.Address);

                    //N-TRIPLES
                    else if (responseContentType.Contains("application/n-triples"))
                        result = FromStream(RDFModelEnums.RDFFormats.NTriples, webResponse.GetResponseStream(), webRequest.Address);

                    //TRIX
                    else if (responseContentType.Contains("application/trix"))
                        result = FromStream(RDFModelEnums.RDFFormats.TriX, webResponse.GetResponseStream(), webRequest.Address);
                }
            }
            catch (Exception ex)
            {
                throw new RDFModelException($"Cannot read RDF graph from Uri {uri} because: " + ex.Message);
            }

            return result;
        }
        #endregion

        #endregion

        #endregion
    }
}