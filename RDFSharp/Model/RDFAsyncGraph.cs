/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Threading.Tasks;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFAsyncGraph represents an asynchronous wrapper for RDFGraph
    /// </summary>
    public class RDFAsyncGraph : RDFDataSource, IEquatable<RDFAsyncGraph>, IEnumerable<RDFTriple>, IDisposable
    {
        #region Properties
        /// <summary>
        /// Graph wrapped by this asynchronous instance
        /// </summary>
        internal RDFGraph WrappedGraph { get; set; }

        /// <summary>
        /// Uri of the asynchronous asyncGraph
        /// </summary>
        public Uri Context => WrappedGraph.Context;

        /// <summary>
        /// Count of the asynchronous asyncGraph's triples
        /// </summary>
        public long TriplesCount => WrappedGraph.TriplesCount;

        /// <summary>
        /// Gets the enumerator on the asynchronous asyncGraph's triples for iteration
        /// </summary>
        public IEnumerator<RDFTriple> TriplesEnumerator => WrappedGraph.TriplesEnumerator;

        /// <summary>
        /// Flag indicating that the asynchronous asyncGraph has already been disposed
        /// </summary>
        internal bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty asynchronous asyncGraph
        /// </summary>
        public RDFAsyncGraph()
            => WrappedGraph = new RDFGraph();

        /// <summary>
        /// Builds an asynchronous asyncGraph with the given list of triples
        /// </summary>
        public RDFAsyncGraph(List<RDFTriple> triples)
            => WrappedGraph = new RDFGraph(triples);

        /// <summary>
        /// Builds an asynchronous asyncGraph wrapping the given asyncGraph
        /// </summary>
        public RDFAsyncGraph(RDFGraph graph)
            => WrappedGraph = graph ?? new RDFGraph();

        /// <summary>
        /// Destroys the asynchronous asyncGraph instance
        /// </summary>
        ~RDFAsyncGraph() => Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the asynchronous asyncGraph
        /// </summary>
        public override string ToString() 
            => WrappedGraph.ToString();

        /// <summary>
        /// Performs the equality comparison between two asynchronous graphs
        /// </summary>
        public bool Equals(RDFAsyncGraph other)
            => WrappedGraph.Equals(other?.WrappedGraph);

        /// <summary>
        /// Exposes a typed enumerator on the asynchronousgraph's triples
        /// </summary>
        IEnumerator<RDFTriple> IEnumerable<RDFTriple>.GetEnumerator() 
            => TriplesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the asynchronous asyncGraph's triples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() 
            => TriplesEnumerator;

        /// <summary>
        /// Disposes the gasynchronousraph (IDisposable)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the asynchronous asyncGraph
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                WrappedGraph.Dispose();
                WrappedGraph = null;
            }   

            Disposed = true;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Sets the context of the asynchronous asyncGraph to the given absolute Uri
        /// </summary>
        public Task<RDFAsyncGraph> SetContextAsync(Uri contextUri)
            => Task.Run(() => { WrappedGraph.SetContext(contextUri); return this; });

        /// <summary>
        /// Adds the given triple to the asynchronous asyncGraph, avoiding duplicate insertions
        /// </summary>
        public Task<RDFAsyncGraph> AddTripleAsync(RDFTriple triple)
            => Task.Run(() => { WrappedGraph.AddTriple(triple); return this; });

        /// <summary>
        /// Adds the given container to the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> AddContainerAsync(RDFContainer container)
            => Task.Run(() => { WrappedGraph.AddContainer(container); return this; });

        /// <summary>
        /// Adds the given collection to the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> AddCollectionAsync(RDFCollection collection)
            => Task.Run(() => { WrappedGraph.AddCollection(collection); return this; });
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given triple from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTripleAsync(RDFTriple triple)
            => Task.Run(() => { WrappedGraph.RemoveTriple(triple); return this; });

        /// <summary>
        /// Removes the triples with the given subject from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesBySubjectAsync(RDFResource subjectResource)
            => Task.Run(() => { WrappedGraph.RemoveTriplesBySubject(subjectResource); return this; });

        /// <summary>
        /// Removes the triples with the given predicate from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesByPredicateAsync(RDFResource predicateResource)
            => Task.Run(() => { WrappedGraph.RemoveTriplesByPredicate(predicateResource); return this; });

        /// <summary>
        /// Removes the triples with the given object from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesByObjectAsync(RDFResource objectResource)
            => Task.Run(() => { WrappedGraph.RemoveTriplesByObject(objectResource); return this; });

        /// <summary>
        /// Removes the triples with the given literal from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesByLiteralAsync(RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedGraph.RemoveTriplesByLiteral(objectLiteral); return this; });

        /// <summary>
        /// Removes the triples with the given subject and predicate from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesBySubjectPredicateAsync(RDFResource subjectResource, RDFResource predicateResource)
            => Task.Run(() => { WrappedGraph.RemoveTriplesBySubjectPredicate(subjectResource, predicateResource); return this; });

        /// <summary>
        /// Removes the triples with the given subject and object from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesBySubjectObjectAsync(RDFResource subjectResource, RDFResource objectResource)
            => Task.Run(() => { WrappedGraph.RemoveTriplesBySubjectObject(subjectResource, objectResource); return this; });

        /// <summary>
        /// Removes the triples with the given subject and literal from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesBySubjectLiteralAsync(RDFResource subjectResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedGraph.RemoveTriplesBySubjectLiteral(subjectResource, objectLiteral); return this; });

        /// <summary>
        /// Removes the triples with the given predicate and object from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesByPredicateObjectAsync(RDFResource predicateResource, RDFResource objectResource)
            => Task.Run(() => { WrappedGraph.RemoveTriplesByPredicateObject(predicateResource, objectResource); return this; });

        /// <summary>
        /// Removes the triples with the given predicate and literal from the asynchronous asyncGraph
        /// </summary>
        public Task<RDFAsyncGraph> RemoveTriplesByPredicateLiteralAsync(RDFResource predicateResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedGraph.RemoveTriplesByPredicateLiteral(predicateResource, objectLiteral); return this; });

        /// <summary>
        /// Clears the triples and metadata of the asynchronous asyncGraph
        /// </summary>
        public Task ClearTriplesAsync()
            => Task.Run(() => WrappedGraph.ClearTriples());

        /// <summary>
        /// Turns back the reified triples of the asynchronous asyncGraph into their compact representation
        /// </summary>
        public Task UnreifyTriplesAsync()
            => Task.Run(() => WrappedGraph.UnreifyTriples());
        #endregion

        #region Select
        /// <summary>
        /// Checks if the asynchronous asyncGraph contains the given triple
        /// </summary>
        public Task<bool> ContainsTripleAsync(RDFTriple triple)
            => Task.Run(() => WrappedGraph.ContainsTriple(triple));

        /// <summary>
        /// Gets the subgraph containing triples with the specified subject
        /// </summary>
        public Task<RDFAsyncGraph> SelectTriplesBySubjectAsync(RDFResource subjectResource)
            => Task.Run(() => { WrappedGraph.SelectTriplesBySubject(subjectResource); return this; });

        /// <summary>
        /// Gets the subgraph containing triples with the specified predicate
        /// </summary>
        public Task<RDFAsyncGraph> SelectTriplesByPredicateAsync(RDFResource predicateResource)
            => Task.Run(() => { WrappedGraph.SelectTriplesByPredicate(predicateResource); return this; });

        /// <summary>
        /// Gets the subgraph containing triples with the specified object
        /// </summary>
        public Task<RDFAsyncGraph> SelectTriplesByObjectAsync(RDFResource objectResource)
            => Task.Run(() => { WrappedGraph.SelectTriplesByObject(objectResource); return this; });

        /// <summary>
        /// Gets the subgraph containing triples with the specified literal
        /// </summary>
        public Task<RDFAsyncGraph> SelectTriplesByLiteralAsync(RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedGraph.SelectTriplesByLiteral(objectLiteral); return this; });

        /// <summary>
        /// Gets the subgraph containing triples with the specified combination of SPOL accessors<br/>
        /// (null values are threated as * selectors. Ensure to keep object and literal mutually exclusive!)
        /// </summary>
        public Task<RDFAsyncGraph> this[RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit]
            => Task.Run(() => 
               {
                   if (obj != null && lit != null)
                       throw new RDFModelException("Cannot access an asynchronous graph when both object and literals are given: they have to be mutually exclusive!");
                   return new RDFAsyncGraph(RDFModelUtilities.SelectTriples(WrappedGraph, subj, pred, obj, lit));
               });
        #endregion

        #region Set
        /// <summary>
        /// Builds an intersection asynchronous graph from this asynchronous graph and a given one
        /// </summary>
        public Task<RDFAsyncGraph> IntersectWithAsync(RDFAsyncGraph asyncGraph)
            => Task.Run(() => new RDFAsyncGraph(WrappedGraph.IntersectWith(asyncGraph?.WrappedGraph)));

        /// <summary>
        /// Builds a union asynchronous graph from this asynchronous graph and a given one
        /// </summary>
        public Task<RDFAsyncGraph> UnionWithAsync(RDFAsyncGraph asyncGraph)
            => Task.Run(() => new RDFAsyncGraph(WrappedGraph.UnionWith(asyncGraph?.WrappedGraph)));

        /// <summary>
        /// Builds a difference asynchronous graph from this asynchronous graph and a given one
        /// </summary>
        public Task<RDFAsyncGraph> DifferenceWithAsync(RDFAsyncGraph asyncGraph)
            => Task.Run(() => new RDFAsyncGraph(WrappedGraph.DifferenceWith(asyncGraph?.WrappedGraph)));
        #endregion

        #region Convert

        #region Export
        /// <summary>
        /// Writes the asynchronous graph into a file in the given RDF format
        /// </summary>
        public Task ToFileAsync(RDFModelEnums.RDFFormats rdfFormat, string filepath)
            => Task.Run(() => WrappedGraph.ToFile(rdfFormat, filepath));

        /// <summary>
        /// Writes the asynchronous graph into a stream in the given RDF format (at the end the stream is closed)
        /// </summary>
        public Task ToStreamAsync(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream)
            => Task.Run(() => WrappedGraph.ToStream(rdfFormat, outputStream));

        /// <summary>
        /// Writes the asynchronous graph into a datatable with "Subject-Predicate-Object" columns
        /// </summary>
        public Task<DataTable> ToDataTableAsync()
            => Task.Run(() => WrappedGraph.ToDataTable());
        #endregion

        #region Import
        /// <summary>
        /// Reads an asynchronous graph from a file of the given RDF format
        /// </summary>
        public static Task<RDFAsyncGraph> FromFileAsync(RDFModelEnums.RDFFormats rdfFormat, string filepath)
            => Task.Run(() => new RDFAsyncGraph(RDFGraph.FromFile(rdfFormat, filepath)));

        /// <summary>
        /// Reads an asynchronous graph from a stream of the given RDF format
        /// </summary>
        public static Task<RDFAsyncGraph> FromStreamAsync(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream)
            => Task.Run(() => new RDFAsyncGraph(RDFGraph.FromStream(rdfFormat, inputStream)));

        /// <summary>
        /// Reads an asynchronous graph from a datatable with "Subject-Predicate-Object" columns
        /// </summary>
        public static Task<RDFAsyncGraph> FromDataTableAsync(DataTable table)
            => Task.Run(() => new RDFAsyncGraph(RDFGraph.FromDataTable(table)));

        /// <summary>
        /// Reads an asynchronous graph by trying to dereference the given Uri
        /// </summary>
        public static Task<RDFAsyncGraph> FromUriAsync(Uri uri, int timeoutMilliseconds = 20000)
            => Task.Run(() => new RDFAsyncGraph(RDFGraph.FromUri(uri, timeoutMilliseconds)));
        #endregion

        #endregion

        #endregion
    }
}