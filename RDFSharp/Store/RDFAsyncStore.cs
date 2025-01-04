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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFAsyncStore represents an asynchronous wrapper for RDFStore (suitable for working under UI-dependant applications)
    /// </summary>
    public class RDFAsyncStore : RDFDataSource, IEquatable<RDFAsyncStore>
    {
        #region Properties
        /// <summary>
        /// Store wrapped by this asynchronous instance
        /// </summary>
        internal RDFStore WrappedStore { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an asynchronous store wrapping a memory store
        /// </summary>
        public RDFAsyncStore()
            => WrappedStore = new RDFMemoryStore();

        /// <summary>
        /// Builds an asynchronous store wrapping the given store
        /// </summary>
        public RDFAsyncStore(RDFStore store)
            => WrappedStore = store ?? new RDFMemoryStore();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the asynchronous store
        /// </summary>
        public override string ToString()
            => WrappedStore.ToString();

        /// <summary>
        /// Performs the equality comparison between two asynchronous stores
        /// </summary>
        public bool Equals(RDFAsyncStore other)
            => WrappedStore.Equals(other?.WrappedStore);
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given asynchronous graph into the asynchronous store, avoiding duplicate insertions
        /// </summary>
        public Task<RDFAsyncStore> MergeGraphAsync(RDFAsyncGraph asyncGraph)
            => Task.Run(() => new RDFAsyncStore(WrappedStore.MergeGraph(asyncGraph?.WrappedGraph)));

        /// <summary>
        /// Merges the given graph into the asynchronous store, avoiding duplicate insertions
        /// </summary>
        public Task<RDFAsyncStore> MergeGraphAsync(RDFGraph graph)
            => Task.Run(() => new RDFAsyncStore(WrappedStore.MergeGraph(graph)));

        /// <summary>
        /// Adds the given quadruple to the asynchronous store, avoiding duplicate insertions
        /// </summary>
        public Task<RDFAsyncStore> AddQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => { WrappedStore.AddQuadruple(quadruple); return this; });
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => { WrappedStore.RemoveQuadruple(quadruple); return this; });

        /// <summary>
        /// Removes the quadruples with the given context from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextAsync(RDFContext contextResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContext(contextResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given subject from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesBySubjectAsync(RDFResource subjectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesBySubject(subjectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given (non-blank) predicate from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByPredicateAsync(RDFResource predicateResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByPredicate(predicateResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given object from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByObjectAsync(RDFResource objectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByObject(objectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given literal from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByLiteralAsync(RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByLiteral(objectLiteral); return this; });

        /// <summary>
        /// Removes the quadruples with the given context and subject from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextSubjectAsync(RDFContext contextResource, RDFResource subjectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextSubject(contextResource, subjectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given context and predicate from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextPredicateAsync(RDFContext contextResource, RDFResource predicateResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextPredicate(contextResource, predicateResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given context and object from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextObjectAsync(RDFContext contextResource, RDFResource objectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextObject(contextResource, objectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given context and literal from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextLiteralAsync(RDFContext contextResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextLiteral(contextResource, objectLiteral); return this; });

        /// <summary>
        /// Removes the quadruples with the given context, subject and predicate from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextSubjectPredicateAsync(RDFContext contextResource, RDFResource subjectResource, RDFResource predicateResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextSubjectPredicate(contextResource, subjectResource, predicateResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given context, subject and object from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextSubjectObjectAsync(RDFContext contextResource, RDFResource subjectResource, RDFResource objectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextSubjectObject(contextResource, subjectResource, objectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given context, subject and literal from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextSubjectLiteralAsync(RDFContext contextResource, RDFResource subjectResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextSubjectLiteral(contextResource, subjectResource, objectLiteral); return this; });

        /// <summary>
        /// Removes the quadruples with the given context, predicate and object from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextPredicateObjectAsync(RDFContext contextResource, RDFResource predicateResource, RDFResource objectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextPredicateObject(contextResource, predicateResource, objectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given context, predicate and literal from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByContextPredicateLiteralAsync(RDFContext contextResource, RDFResource predicateResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByContextPredicateLiteral(contextResource, predicateResource, objectLiteral); return this; });

        /// <summary>
        /// Removes the quadruples with the given subject and predicate from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesBySubjectPredicateAsync(RDFResource subjectResource, RDFResource predicateResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesBySubjectPredicate(subjectResource, predicateResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given subject and object from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesBySubjectObjectAsync(RDFResource subjectResource, RDFResource objectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesBySubjectObject(subjectResource, objectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given subject and literal from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesBySubjectLiteralAsync(RDFResource subjectResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesBySubjectLiteral(subjectResource, objectLiteral); return this; });

        /// <summary>
        /// Removes the quadruples with the given predicate and object from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByPredicateObjectAsync(RDFResource predicateResource, RDFResource objectResource)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByPredicateObject(predicateResource, objectResource); return this; });

        /// <summary>
        /// Removes the quadruples with the given predicate and literal from the asynchronous store
        /// </summary>
        public Task<RDFAsyncStore> RemoveQuadruplesByPredicateLiteralAsync(RDFResource predicateResource, RDFLiteral objectLiteral)
            => Task.Run(() => { WrappedStore.RemoveQuadruplesByPredicateLiteral(predicateResource, objectLiteral); return this; });

        /// <summary>
        /// Clears the quadruples of the asynchronous store
        /// </summary>
        public Task ClearQuadruplesAsync()
            => Task.Run(() => WrappedStore.ClearQuadruples());

        /// <summary>
        /// Compacts the reified quadruples by removing their 4 standard statements
        /// </summary>
        public Task UnreifyQuadruplesAsync()
            => Task.Run(() => WrappedStore.UnreifyQuadruples());
        #endregion

        #region Select
        /// <summary>
        /// Checks if the asynchronous store contains the given quadruple
        /// </summary>
        public Task<bool> ContainsQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => WrappedStore.ContainsQuadruple(quadruple));

        /// <summary>
        /// Gets a memory store containing all quadruples of the asynchronous store
        /// </summary>
        public Task<RDFMemoryStore> SelectAllQuadruplesAsync()
            => SelectQuadruplesAsync(null, null, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples of the asynchronous store with the specified context
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByContextAsync(RDFContext contextResource)
            => SelectQuadruplesAsync(contextResource, null, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples of the asynchronous store with the specified subject
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesBySubjectAsync(RDFResource subjectResource)
            => SelectQuadruplesAsync(null, subjectResource, null, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples of the asynchronous store with the specified predicate
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByPredicateAsync(RDFResource predicateResource)
            => SelectQuadruplesAsync(null, null, predicateResource, null, null);

        /// <summary>
        /// Gets a memory store containing quadruples of the asynchronous store with the specified object
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByObjectAsync(RDFResource objectResource)
            => SelectQuadruplesAsync(null, null, null, objectResource, null);

        /// <summary>
        /// Gets a memory store containing quadruples of the asynchronous store with the specified literal
        /// </summary>
        public Task<RDFMemoryStore> SelectQuadruplesByLiteralAsync(RDFLiteral objectLiteral)
            => SelectQuadruplesAsync(null, null, null, null, objectLiteral);

        /// <summary>
        /// Gets a memory store containing quadruples of the asynchronous store satisfying the given pattern
        /// </summary>
        internal Task<RDFMemoryStore> SelectQuadruplesAsync(RDFContext contextResource,
                                                            RDFResource subjectResource,
                                                            RDFResource predicateResource,
                                                            RDFResource objectResource,
                                                            RDFLiteral objectLiteral)
            => Task.Run(() => WrappedStore.SelectQuadruples(contextResource,
                                                            subjectResource,
                                                            predicateResource,
                                                            objectResource,
                                                            objectLiteral));

        /// <summary>
        /// Gets a list containing the graphs saved in the asynchronous store
        /// </summary>
        public Task<List<RDFGraph>> ExtractGraphsAsync()
            => Task.Run(() => WrappedStore.ExtractGraphs());

        /// <summary>
        /// Gets a list containing the contexts saved in the asynchronous store
        /// </summary>
        public Task<List<RDFContext>> ExtractContextsAsync()
            => Task.Run(() => WrappedStore.ExtractContexts());
        #endregion

        #region Convert

        #region Export
        /// <summary>
        /// Writes the asynchronous store into a file in the given RDF format
        /// </summary>
        public Task ToFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
            => Task.Run(() => WrappedStore.ToFile(rdfFormat, filepath));

        /// <summary>
        /// Writes the asynchronous store into a stream in the given RDF format
        /// </summary>
        public Task ToStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream)
            => Task.Run(() => WrappedStore.ToStream(rdfFormat, outputStream));

        /// <summary>
        /// Writes the asynchronous store into a datatable with "Context-Subject-Predicate-Object" columns
        /// </summary>
        public Task<DataTable> ToDataTableAsync()
            => Task.Run(() => WrappedStore.ToDataTable());
        #endregion

        #endregion

        #endregion
    }
}