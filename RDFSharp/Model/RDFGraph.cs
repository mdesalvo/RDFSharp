/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFGraph represents an Uri-named collection of triples in the RDF model
    /// </summary>
    public sealed class RDFGraph : RDFDataSource, IEquatable<RDFGraph>, IEnumerable<RDFTriple>, IDisposable
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
                        ? new RDFTriple(GraphIndex.ResourcesRegister[indexedTriple.SubjectID], GraphIndex.ResourcesRegister[indexedTriple.PredicateID], GraphIndex.ResourcesRegister[indexedTriple.ObjectID])
                        : new RDFTriple(GraphIndex.ResourcesRegister[indexedTriple.SubjectID], GraphIndex.ResourcesRegister[indexedTriple.PredicateID], GraphIndex.LiteralsRegister[indexedTriple.ObjectID]);
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
            {
                return false;
            }

            return this.All(other.ContainsTriple);
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
        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                GraphIndex?.Dispose();
                GraphIndex = null;

                IndexedTriples?.Clear();
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
            if (contextUri?.IsAbsoluteUri == true
                && !contextUri.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase)
                && !contextUri.ToString().StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase))
            {
                Context = contextUri;
            }
            return this;
        }

        /// <summary>
        /// Asynchronously sets the context of the graph to the given absolute Uri
        /// </summary>
        public Task<RDFGraph> SetContextAsync(Uri contextUri)
            => Task.Run(() => SetContext(contextUri));

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
        /// Asynchronously adds the given triple to the graph, avoiding duplicate insertions
        /// </summary>
        public Task<RDFGraph> AddTripleAsync(RDFTriple triple)
            => Task.Run(() => AddTriple(triple));

        /// <summary>
        /// Adds the given container to the graph
        /// </summary>
        public RDFGraph AddContainer(RDFContainer container)
        {
            if (container != null)
            {
                //Reify the container to get its graph representation
                foreach (RDFTriple t in container.ReifyContainer())
                    AddTriple(t);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously adds the given container to the graph
        /// </summary>
        public Task<RDFGraph> AddContainerAsync(RDFContainer container)
            => Task.Run(() => AddContainer(container));

        /// <summary>
        /// Adds the given collection to the graph
        /// </summary>
        public RDFGraph AddCollection(RDFCollection collection)
        {
            if (collection != null)
            {
                //Reify the collection to get its graph representation
                foreach (RDFTriple t in collection.ReifyCollection())
                    AddTriple(t);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously adds the given collection to the graph
        /// </summary>
        public Task<RDFGraph> AddCollectionAsync(RDFCollection collection)
            => Task.Run(() => AddCollection(collection));

        /// <summary>
        /// Adds the given datatype to the graph
        /// </summary>
        public RDFGraph AddDatatype(RDFDatatype datatype)
        {
            if (datatype != null)
            {
                //Reify the datatype to get its graph representation
                foreach (RDFTriple t in datatype.ToRDFGraph())
                    AddTriple(t);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously adds the given datatype to the graph
        /// </summary>
        public Task<RDFGraph> AddDatatypeAsync(RDFDatatype datatype)
            => Task.Run(() => AddDatatype(datatype));
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
        /// Asynchronously removes the given triple from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTripleAsync(RDFTriple triple)
            => Task.Run(() => RemoveTriple(triple));

        /// <summary>
        /// Removes the triples with the given subject
        /// </summary>
        public RDFGraph RemoveTriplesBySubject(RDFResource subjectResource)
        {
            if (subjectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given subject from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesBySubjectAsync(RDFResource subjectResource)
            => Task.Run(() => RemoveTriplesBySubject(subjectResource));

        /// <summary>
        /// Removes the triples with the given predicate
        /// </summary>
        public RDFGraph RemoveTriplesByPredicate(RDFResource predicateResource)
        {
            if (predicateResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesByPredicate(predicateResource))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given predicate from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesByPredicateAsync(RDFResource predicateResource)
            => Task.Run(() => RemoveTriplesByPredicate(predicateResource));

        /// <summary>
        /// Removes the triples with the given object
        /// </summary>
        public RDFGraph RemoveTriplesByObject(RDFResource objectResource)
        {
            if (objectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesByObject(objectResource))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given object from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesByObjectAsync(RDFResource objectResource)
            => Task.Run(() => RemoveTriplesByObject(objectResource));

        /// <summary>
        /// Removes the triples with the given literal
        /// </summary>
        public RDFGraph RemoveTriplesByLiteral(RDFLiteral literal)
        {
            if (literal != null)
            {
                foreach (RDFTriple triple in SelectTriplesByLiteral(literal))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given literal from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesByLiteralAsync(RDFLiteral literal)
            => Task.Run(() => RemoveTriplesByLiteral(literal));

        /// <summary>
        /// Removes the triples with the given subject and predicate
        /// </summary>
        public RDFGraph RemoveTriplesBySubjectPredicate(RDFResource subjectResource, RDFResource predicateResource)
        {
            if (subjectResource != null && predicateResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource).SelectTriplesByPredicate(predicateResource))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given subject and predicate from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesBySubjectPredicateAsync(RDFResource subjectResource, RDFResource predicateResource)
            => Task.Run(() => RemoveTriplesBySubjectPredicate(subjectResource, predicateResource));

        /// <summary>
        /// Removes the triples with the given subject and object
        /// </summary>
        public RDFGraph RemoveTriplesBySubjectObject(RDFResource subjectResource, RDFResource objectResource)
        {
            if (subjectResource != null && objectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource).SelectTriplesByObject(objectResource))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given subject and object from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesBySubjectObjectAsync(RDFResource subjectResource, RDFResource objectResource)
            => Task.Run(() => RemoveTriplesBySubjectObject(subjectResource, objectResource));

        /// <summary>
        /// Removes the triples with the given subject and literal
        /// </summary>
        public RDFGraph RemoveTriplesBySubjectLiteral(RDFResource subjectResource, RDFLiteral literal)
        {
            if (subjectResource != null && literal != null)
            {
                foreach (RDFTriple triple in SelectTriplesBySubject(subjectResource).SelectTriplesByLiteral(literal))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given subject and literal from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesBySubjectLiteralAsync(RDFResource subjectResource, RDFLiteral literal)
            => Task.Run(() => RemoveTriplesBySubjectLiteral(subjectResource, literal));

        /// <summary>
        /// Removes the triples with the given predicate and object
        /// </summary>
        public RDFGraph RemoveTriplesByPredicateObject(RDFResource predicateResource, RDFResource objectResource)
        {
            if (predicateResource != null && objectResource != null)
            {
                foreach (RDFTriple triple in SelectTriplesByPredicate(predicateResource).SelectTriplesByObject(objectResource))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given predicate and object from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesByPredicateObjectAsync(RDFResource predicateResource, RDFResource objectResource)
            => Task.Run(() => RemoveTriplesByPredicateObject(predicateResource, objectResource));

        /// <summary>
        /// Removes the triples with the given predicate and literal
        /// </summary>
        public RDFGraph RemoveTriplesByPredicateLiteral(RDFResource predicateResource, RDFLiteral objectLiteral)
        {
            if (predicateResource != null && objectLiteral != null)
            {
                foreach (RDFTriple triple in SelectTriplesByPredicate(predicateResource).SelectTriplesByLiteral(objectLiteral))
                    RemoveTriple(triple);
            }
            return this;
        }

        /// <summary>
        /// Asynchronously removes the triples with the given predicate and literal from the graph
        /// </summary>
        public Task<RDFGraph> RemoveTriplesByPredicateLiteralAsync(RDFResource predicateResource, RDFLiteral literal)
            => Task.Run(() => RemoveTriplesByPredicateLiteral(predicateResource, literal));

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
        /// Asynchronously clears the triples and metadata of the graph
        /// </summary>
        public Task ClearTriplesAsync()
            => Task.Run(ClearTriples);
        #endregion

        #region Select
        /// <summary>
        /// Checks if the graph contains the given triple
        /// </summary>
        public bool ContainsTriple(RDFTriple triple)
            => triple != null && IndexedTriples.ContainsKey(triple.TripleID);

        /// <summary>
        /// Asynchronously checks if the graph contains the given triple
        /// </summary>
        public Task<bool> ContainsTripleAsync(RDFTriple triple)
            => Task.Run(() => ContainsTriple(triple));

        /// <summary>
        /// Gets the subgraph containing triples with the specified subject
        /// </summary>
        public RDFGraph SelectTriplesBySubject(RDFResource subjectResource)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, subjectResource, null, null, null));

        /// <summary>
        /// Asynchronously gets the subgraph containing triples with the specified subject
        /// </summary>
        public Task<RDFGraph> SelectTriplesBySubjectAsync(RDFResource subjectResource)
            => Task.Run(() => SelectTriplesBySubject(subjectResource));

        /// <summary>
        /// Gets the subgraph containing triples with the specified predicate
        /// </summary>
        public RDFGraph SelectTriplesByPredicate(RDFResource predicateResource)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, null, predicateResource, null, null));

        /// <summary>
        /// Asynchronously gets the subgraph containing triples with the specified predicate
        /// </summary>
        public Task<RDFGraph> SelectTriplesByPredicateAsync(RDFResource predicateResource)
            => Task.Run(() => SelectTriplesByPredicate(predicateResource));

        /// <summary>
        /// Gets the subgraph containing triples with the specified object
        /// </summary>
        public RDFGraph SelectTriplesByObject(RDFResource objectResource)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, null, null, objectResource, null));

        /// <summary>
        /// Asynchronously gets the subgraph containing triples with the specified object
        /// </summary>
        public Task<RDFGraph> SelectTriplesByObjectAsync(RDFResource objectResource)
            => Task.Run(() => SelectTriplesByObject(objectResource));

        /// <summary>
        /// Gets the subgraph containing triples with the specified literal
        /// </summary>
        public RDFGraph SelectTriplesByLiteral(RDFLiteral objectLiteral)
            => new RDFGraph(RDFModelUtilities.SelectTriples(this, null, null, null, objectLiteral));

        /// <summary>
        /// Asynchronously gets the subgraph containing triples with the specified literal
        /// </summary>
        public Task<RDFGraph> SelectTriplesByLiteralAsync(RDFLiteral literal)
            => Task.Run(() => SelectTriplesByLiteral(literal));

        /// <summary>
        /// Gets the subgraph containing triples with the specified combination of SPOL accessors<br/>
        /// (null values are handled as * selectors. Ensure to keep object and literal mutually exclusive!)
        /// </summary>
        public RDFGraph this[RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit]
            => obj != null && lit != null ? throw new RDFModelException("Cannot access a graph when both object and literals are given: they must be mutually exclusive!")
                                          : new RDFGraph(RDFModelUtilities.SelectTriples(this, subj, pred, obj, lit));
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
                    {
                        result.AddTriple(t);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously builds an intersection graph from this graph and a given one
        /// </summary>
        public Task<RDFGraph> IntersectWithAsync(RDFGraph graph)
            => Task.Run(() => IntersectWith(graph));

        /// <summary>
        /// Builds a union graph from this graph and a given one
        /// </summary>
        public RDFGraph UnionWith(RDFGraph graph)
        {
            RDFGraph result = new RDFGraph();

            //Add triples from this graph
            foreach (RDFTriple t in this)
            {
                result.AddTriple(t);
            }

            //Manage the given graph
            if (graph != null)
            {
                //Add triples from the given graph
                foreach (RDFTriple t in graph)
                {
                    result.AddTriple(t);
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously builds a union graph from this graph and a given one
        /// </summary>
        public Task<RDFGraph> UnionWithAsync(RDFGraph graph)
            => Task.Run(() => UnionWith(graph));

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
                    {
                        result.AddTriple(t);
                    }
                }
            }
            else
            {
                //Add triples from this graph
                foreach (RDFTriple t in this)
                {
                    result.AddTriple(t);
                }
            }
            return result;
        }

        /// <summary>
        /// Asynchronously builds a difference graph from this graph and a given one
        /// </summary>
        public Task<RDFGraph> DifferenceWithAsync(RDFGraph graph)
            => Task.Run(() => DifferenceWith(graph));
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
            {
                throw new RDFModelException("Cannot write RDF graph to file because given \"filepath\" parameter is null or empty.");
            }
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
        /// Asynchronously writes the graph into a file in the given RDF format
        /// </summary>
        public Task ToFileAsync(RDFModelEnums.RDFFormats rdfFormat, string filepath)
            => Task.Run(() => ToFile(rdfFormat, filepath));

        /// <summary>
        /// Writes the graph into a stream in the given RDF format (at the end the stream is closed)
        /// </summary>
        public void ToStream(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream)
        {
            #region Guards
            if (outputStream == null)
            {
                throw new RDFModelException("Cannot write RDF graph to stream because given \"outputStream\" parameter is null.");
            }
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
        /// Asynchronously writes the graph into a stream in the given RDF format (at the end the stream is closed)
        /// </summary>
        public Task ToStreamAsync(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream)
            => Task.Run(() => ToStream(rdfFormat, outputStream));

        /// <summary>
        /// Writes the graph into a datatable with "Subject-Predicate-Object" columns
        /// </summary>
        public DataTable ToDataTable()
        {
            //Create the structure of the result datatable
            DataTable result = new DataTable(ToString());
            result.Columns.Add("?SUBJECT", typeof(string));
            result.Columns.Add("?PREDICATE", typeof(string));
            result.Columns.Add("?OBJECT", typeof(string));

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

        /// <summary>
        /// Asynchronously writes the graph into a datatable with "Subject-Predicate-Object" columns
        /// </summary>
        public Task<DataTable> ToDataTableAsync()
            => Task.Run(ToDataTable);
        #endregion

        #region Import
        /// <summary>
        /// Reads a graph from a file of the given RDF format.
        /// </summary>
        public static RDFGraph FromFile(RDFModelEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (string.IsNullOrEmpty(filepath))
            {
                throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter is null or empty.");
            }

            if (!File.Exists(filepath))
            {
                throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            }
            #endregion

            RDFGraph graph = null;
            switch (rdfFormat)
            {
                case RDFModelEnums.RDFFormats.RdfXml:
                    graph = RDFXml.Deserialize(filepath);
                    break;
                case RDFModelEnums.RDFFormats.Turtle:
                    graph =  RDFTurtle.Deserialize(filepath);
                    break;
                case RDFModelEnums.RDFFormats.NTriples:
                    graph =  RDFNTriples.Deserialize(filepath);
                    break;
                case RDFModelEnums.RDFFormats.TriX:
                    graph =  RDFTriX.Deserialize(filepath);
                    break;
            }

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            #endregion

            return graph;
        }

        /// <summary>
        /// Asynchronously reads a graph from a file of the given RDF format
        /// </summary>
        public static Task<RDFGraph> FromFileAsync(RDFModelEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery = false)
            => Task.Run(() => FromFile(rdfFormat, filepath, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a graph from a stream of the given RDF format.
        /// </summary>
        public static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
            => FromStream(rdfFormat, inputStream, null, enableDatatypeDiscovery);
        internal static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, Uri graphContext, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (inputStream == null)
            {
                throw new RDFModelException("Cannot read RDF graph from stream because given \"inputStream\" parameter is null.");
            }
            #endregion

            RDFGraph graph = null;
            switch (rdfFormat)
            {
                case RDFModelEnums.RDFFormats.RdfXml:
                    graph = RDFXml.Deserialize(inputStream, graphContext);
                    break;
                case RDFModelEnums.RDFFormats.Turtle:
                    graph =  RDFTurtle.Deserialize(inputStream, graphContext);
                    break;
                case RDFModelEnums.RDFFormats.NTriples:
                    graph =  RDFNTriples.Deserialize(inputStream, graphContext);
                    break;
                case RDFModelEnums.RDFFormats.TriX:
                    graph =  RDFTriX.Deserialize(inputStream, graphContext);
                    break;
            }

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            #endregion

            return graph;
        }

        /// <summary>
        /// Asynchronously reads a graph from a stream of the given RDF format
        /// </summary>
        public static Task<RDFGraph> FromStreamAsync(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery = false)
            => Task.Run(() => FromStream(rdfFormat, inputStream, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a graph from a datatable with "Subject-Predicate-Object" columns.
        /// </summary>
        public static RDFGraph FromDataTable(DataTable table, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (table == null)
            {
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter is null.");
            }

            if (!(table.Columns.Contains("?SUBJECT") && table.Columns.Contains("?PREDICATE") && table.Columns.Contains("?OBJECT")))
            {
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter does not have the required columns \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");
            }
            #endregion

            RDFGraph graph = new RDFGraph();

            #region Parse Table

            #region CONTEXT
            //Parse the name of the datatable for Uri, in order to assign the graph name
            if (Uri.TryCreate(table.TableName, UriKind.Absolute, out Uri graphUri))
            {
                graph.SetContext(graphUri);
            }
            #endregion

            #region SUBJECT-PREDICATE-OBJECT
            foreach (DataRow tableRow in table.Rows)
            {
                #region SUBJECT
                if (tableRow.IsNull("?SUBJECT") || string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"?SUBJECT\" column.");
                }

                RDFPatternMember rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
                if (!(rowSubj is RDFResource subj))
                {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
                }
                #endregion

                #region PREDICATE
                if (tableRow.IsNull("?PREDICATE") || string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"?PREDICATE\" column.");
                }

                RDFPatternMember rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
                if (!(rowPred is RDFResource pred))
                {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"?PREDICATE\" column.");
                }

                if (pred.IsBlank)
                {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having a blank resource in the \"?PREDICATE\" column.");
                }
                #endregion

                #region OBJECT
                if (tableRow.IsNull("?OBJECT"))
                {
                    throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null value in the \"?OBJECT\" column.");
                }

                RDFPatternMember rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
                if (rowObj is RDFResource rowObjRes)
                {
                    graph.AddTriple(new RDFTriple(subj, pred, rowObjRes));
                }
                else
                {
                    graph.AddTriple(new RDFTriple(subj, pred, (RDFLiteral)rowObj));
                }
                #endregion
            }
            #endregion

            #endregion

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            #endregion

            return graph;
        }

        /// <summary>
        /// Asynchronously reads a graph from a datatable with "Subject-Predicate-Object" columns
        /// </summary>
        public static Task<RDFGraph> FromDataTableAsync(DataTable table, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromDataTable(table, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a graph by trying to dereference the given Uri
        /// </summary>
        public static RDFGraph FromUri(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (uri == null)
            {
                throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter is null.");
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter does not represent an absolute Uri.");
            }
            #endregion

            RDFGraph graph = new RDFGraph();
            try
            {
                //Grab eventual dereference Uri
                Uri remappedUri = RDFModelUtilities.RemapUriForDereference(uri);

                HttpWebRequest webRequest = WebRequest.CreateHttp(remappedUri);
                webRequest.MaximumAutomaticRedirections = 4;
                webRequest.AllowAutoRedirect = true;
                webRequest.Timeout = timeoutMilliseconds;
                webRequest.Accept = "application/rdf+xml,text/turtle,application/turtle,application/x-turtle,application/n-triples,application/trix";

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webRequest.HaveResponse)
                {
                    //Cascade detection of ContentType from response
                    string responseContentType = webResponse.ContentType;
                    if (string.IsNullOrWhiteSpace(responseContentType))
                    {
                        responseContentType = webResponse.Headers["ContentType"];
                        if (string.IsNullOrWhiteSpace(responseContentType))
                        {
                            responseContentType = "application/rdf+xml"; //Fallback to RDF/XML
                        }
                    }

                    //RDF/XML
                    if (responseContentType.Contains("application/rdf+xml"))
                    {
                        graph = FromStream(RDFModelEnums.RDFFormats.RdfXml, webResponse.GetResponseStream(), remappedUri);
                    }

                    //TURTLE
                    else if (responseContentType.Contains("text/turtle")
                             || responseContentType.Contains("application/turtle")
                             || responseContentType.Contains("application/x-turtle"))
                    {
                        graph = FromStream(RDFModelEnums.RDFFormats.Turtle, webResponse.GetResponseStream(), remappedUri);
                    }

                    //N-TRIPLES
                    else if (responseContentType.Contains("application/n-triples"))
                    {
                        graph = FromStream(RDFModelEnums.RDFFormats.NTriples, webResponse.GetResponseStream(), remappedUri);
                    }

                    //TRIX
                    else if (responseContentType.Contains("application/trix"))
                    {
                        graph = FromStream(RDFModelEnums.RDFFormats.TriX, webResponse.GetResponseStream(), remappedUri);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RDFModelException($"Cannot read RDF graph from Uri {uri} because: " + ex.Message);
            }

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            #endregion

            return graph;
        }

        /// <summary>
        /// Asynchronously reads a graph by trying to dereference the given Uri
        /// </summary>
        public static Task<RDFGraph> FromUriAsync(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromUri(uri, timeoutMilliseconds, enableDatatypeDiscovery));
        #endregion

        #endregion

        #endregion
    }
}