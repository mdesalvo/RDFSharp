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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFMemoryStore represents an in-memory RDF store engine.
    /// </summary>
    public sealed class RDFMemoryStore : RDFStore, IEnumerable<RDFQuadruple>, IDisposable
    {
        #region Properties
        /// <summary>
        /// Count of the store's quadruples
        /// </summary>
        public override long QuadruplesCount
            => IndexedQuadruples.Count;

        /// <summary>
        /// Gets the enumerator on the store's quadruples for iteration
        /// </summary>
        public IEnumerator<RDFQuadruple> QuadruplesEnumerator
            => IndexedQuadruples.Values.Select(indexedQuadruple =>
                indexedQuadruple.TripleFlavor == 1 //SPO
                    ? new RDFQuadruple(StoreIndex.ContextsRegister[indexedQuadruple.ContextID],
                        StoreIndex.ResourcesRegister[indexedQuadruple.SubjectID],
                        StoreIndex.ResourcesRegister[indexedQuadruple.PredicateID],
                        StoreIndex.ResourcesRegister[indexedQuadruple.ObjectID])
                    : new RDFQuadruple(StoreIndex.ContextsRegister[indexedQuadruple.ContextID],
                        StoreIndex.ResourcesRegister[indexedQuadruple.SubjectID],
                        StoreIndex.ResourcesRegister[indexedQuadruple.PredicateID],
                        StoreIndex.LiteralsRegister[indexedQuadruple.ObjectID])).GetEnumerator();

        /// <summary>
        /// Identifier of the memory store
        /// </summary>
        internal string StoreGUID { get; }

        /// <summary>
        /// Index on the quadruples of the store
        /// </summary>
        internal RDFStoreIndex StoreIndex { get; private set; }

        /// <summary>
        /// Indexed quadruples embedded into the store
        /// </summary>
        internal Dictionary<long, RDFIndexedQuadruple> IndexedQuadruples { get; private set; }

        /// <summary>
        /// Flag indicating that the store has already been disposed
        /// </summary>
        internal bool Disposed { get; private set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty memory store
        /// </summary>
        public RDFMemoryStore()
        {
            StoreType = "MEMORY";
            StoreGUID = Guid.NewGuid().ToString("N");
            StoreIndex = new RDFStoreIndex();
            StoreID = RDFModelUtilities.CreateHash(ToString());
            IndexedQuadruples = new Dictionary<long, RDFIndexedQuadruple>();
        }

        /// <summary>
        /// List-based ctor to build a memory store with the given list of quadruples
        /// </summary>
        public RDFMemoryStore(List<RDFQuadruple> quadruples) : this()
            => quadruples?.ForEach(q => AddQuadruple(q));

        /// <summary>
        /// Destroys the memory store instance
        /// </summary>
        ~RDFMemoryStore() => Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Memory store
        /// </summary>
        public override string ToString()
            => $"{base.ToString()}|ID={StoreGUID}";

        /// <summary>
        /// Performs the equality comparison between two memory stores
        /// </summary>
        public bool Equals(RDFMemoryStore other)
        {
            if (other == null || QuadruplesCount != other.QuadruplesCount)
                return false;

            return base.Equals(other) || this.All(other.ContainsQuadruple);
        }

        /// <summary>
        /// Exposes a typed enumerator on the store's quadruples
        /// </summary>
        IEnumerator<RDFQuadruple> IEnumerable<RDFQuadruple>.GetEnumerator()
            => QuadruplesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the store's quadruples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => QuadruplesEnumerator;

        /// <summary>
        /// Disposes the memory store (IDisposable)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the memory store
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                StoreIndex?.Dispose();
                StoreIndex = null;

                IndexedQuadruples?.Clear();
                IndexedQuadruples = null;
            }

            Disposed = true;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore MergeGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                RDFContext graphCtx = new RDFContext(graph.Context);
                foreach (RDFTriple triple in graph)
                    AddQuadruple(new RDFQuadruple(graphCtx, triple));
            }
            return this;
        }

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore AddQuadruple(RDFQuadruple quadruple)
        {
            if (quadruple != null && !IndexedQuadruples.ContainsKey(quadruple.QuadrupleID))
            {
                //Add quadruple
                IndexedQuadruples.Add(quadruple.QuadrupleID, new RDFIndexedQuadruple(quadruple));
                //Add index
                StoreIndex.AddIndex(quadruple);
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the store
        /// </summary>
        public override RDFStore RemoveQuadruple(RDFQuadruple quadruple)
        {
            if (quadruple != null)
            {
                //Remove quadruple
                IndexedQuadruples.Remove(quadruple.QuadrupleID);
                //Remove index
                StoreIndex.RemoveIndex(quadruple);
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context
        /// </summary>
        public override RDFStore RemoveQuadruplesByContext(RDFContext contextResource)
        {
            if (contextResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubject(RDFResource subjectResource)
        {
            if (subjectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicate(RDFResource predicateResource)
        {
            if (predicateResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByObject(RDFResource objectResource)
        {
            if (objectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByLiteral(RDFLiteral literal)
        {
            if (literal != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByLiteral(literal))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and subject
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubject(RDFContext contextResource, RDFResource subjectResource)
        {
            if (contextResource != null && subjectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesBySubject(subjectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextPredicate(RDFContext contextResource, RDFResource predicateResource)
        {
            if (contextResource != null && predicateResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextObject(RDFContext contextResource, RDFResource objectResource)
        {
            if (contextResource != null && objectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextLiteral(RDFContext contextResource, RDFLiteral literal)
        {
            if (contextResource != null && literal != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesByLiteral(literal))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, subject and predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubjectPredicate(RDFContext contextResource, RDFResource subjectResource, RDFResource predicateResource)
        {
            if (contextResource != null && subjectResource != null && predicateResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesBySubject(subjectResource)
                             .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, subject and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubjectObject(RDFContext contextResource, RDFResource subjectResource, RDFResource objectResource)
        {
            if (contextResource != null && subjectResource != null && objectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesBySubject(subjectResource)
                             .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, subject and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubjectLiteral(RDFContext contextResource, RDFResource subjectResource, RDFLiteral literal)
        {
            if (contextResource != null && subjectResource != null && literal != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesBySubject(subjectResource)
                             .SelectQuadruplesByLiteral(literal))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, predicate and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextPredicateObject(RDFContext contextResource, RDFResource predicateResource, RDFResource objectResource)
        {
            if (contextResource != null && predicateResource != null && objectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesByPredicate(predicateResource)
                             .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, predicate and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextPredicateLiteral(RDFContext contextResource, RDFResource predicateResource, RDFLiteral literal)
        {
            if (contextResource != null && predicateResource != null && literal != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                             .SelectQuadruplesByPredicate(predicateResource)
                             .SelectQuadruplesByLiteral(literal))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject and predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubjectPredicate(RDFResource subjectResource, RDFResource predicateResource)
        {
            if (subjectResource != null && predicateResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource)
                             .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject and object
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubjectObject(RDFResource subjectResource, RDFResource objectResource)
        {
            if (subjectResource != null && objectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource)
                             .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubjectLiteral(RDFResource subjectResource, RDFLiteral literal)
        {
            if (subjectResource != null && literal != null)
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource)
                             .SelectQuadruplesByLiteral(literal))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicateObject(RDFResource predicateResource, RDFResource objectResource)
        {
            if (predicateResource != null && objectResource != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByPredicate(predicateResource)
                             .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicateLiteral(RDFResource predicateResource, RDFLiteral literal)
        {
            if (predicateResource != null && literal != null)
                foreach (RDFQuadruple quad in SelectQuadruplesByPredicate(predicateResource)
                             .SelectQuadruplesByLiteral(literal))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }

            return this;
        }

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public override void ClearQuadruples()
        {
            //Clear quadruples
            IndexedQuadruples.Clear();
            //Clear index
            StoreIndex.ClearIndex();
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public override bool ContainsQuadruple(RDFQuadruple quadruple)
            => quadruple != null && IndexedQuadruples.ContainsKey(quadruple.QuadrupleID);

        /// <summary>
        /// Gets a store containing quadruples satisfying the given pattern
        /// </summary>
        public override RDFMemoryStore SelectQuadruples(RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit)
            => new RDFMemoryStore(RDFStoreUtilities.SelectQuadruples(this, ctx, subj, pred, obj, lit));
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection store from this store and a given one
        /// </summary>
        public RDFMemoryStore IntersectWith(RDFStore store)
        {
            RDFMemoryStore result = new RDFMemoryStore();

            if (store != null)
                //Add intersection quadruples
                foreach (RDFQuadruple q in this)
                    if (store.ContainsQuadruple(q))
                        result.AddQuadruple(q);

            return result;
        }

        /// <summary>
        /// Asynchronously builds a new intersection store from this store and a given one
        /// </summary>
        public Task<RDFMemoryStore> IntersectWithAsync(RDFStore store)
            => Task.Run(() => IntersectWith(store));

        /// <summary>
        /// Builds a new union store from this store and a given one
        /// </summary>
        public RDFMemoryStore UnionWith(RDFStore store)
        {
            RDFMemoryStore result = new RDFMemoryStore();

            //Add quadruples from this store
            foreach (RDFQuadruple q in this)
                result.AddQuadruple(q);

            //Manage the given store
            if (store != null)
                //Add quadruples from the given store
                foreach (RDFQuadruple q in store.SelectAllQuadruples())
                    result.AddQuadruple(q);

            return result;
        }

        /// <summary>
        /// Asynchronously builds a new union store from this store and a given one
        /// </summary>
        public Task<RDFMemoryStore> UnionWithAsync(RDFStore store)
            => Task.Run(() => UnionWith(store));

        /// <summary>
        /// Builds a new difference store from this store and a given one
        /// </summary>
        public RDFMemoryStore DifferenceWith(RDFStore store)
        {
            RDFMemoryStore result = new RDFMemoryStore();

            if (store != null)
            {
                //Add difference quadruples
                foreach (RDFQuadruple q in this)
                    if (!store.ContainsQuadruple(q))
                        result.AddQuadruple(q);
            }
            else
            {
                //Add quadruples from this store
                foreach (RDFQuadruple q in this)
                    result.AddQuadruple(q);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously builds a new difference store from this store and a given one
        /// </summary>
        public Task<RDFMemoryStore> DifferenceWithAsync(RDFStore store)
            => Task.Run(() => DifferenceWith(store));
        #endregion

        #region Convert

        #region Import
        /// <summary>
        /// Reads a memory store from a file of the given RDF format.
        /// </summary>
        public static RDFMemoryStore FromFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (string.IsNullOrWhiteSpace(filepath))
                throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter is null or empty.");
            if (!File.Exists(filepath))
                throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            #endregion

            RDFMemoryStore memStore = null;
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    memStore = RDFNQuads.Deserialize(filepath);
                    break;
                case RDFStoreEnums.RDFFormats.TriX:
                    memStore =  RDFTriX.Deserialize(filepath);
                    break;
                case RDFStoreEnums.RDFFormats.TriG:
                    memStore =  RDFTriG.Deserialize(filepath);
                    break;
            }

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
            {
                foreach (RDFGraph graph in memStore.ExtractGraphs())
                    RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            }
            #endregion

            return memStore;
        }

        /// <summary>
        /// Asynchronously reads a memory store from a file of the given RDF format.
        /// </summary>
        public static Task<RDFMemoryStore> FromFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromFile(rdfFormat, filepath, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a memory store from a stream of the given RDF format.
        /// </summary>
        public static RDFMemoryStore FromStream(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (inputStream == null)
                throw new RDFStoreException("Cannot read RDF memory store from stream because given \"inputStream\" parameter is null.");
            #endregion

            RDFMemoryStore memStore = null;
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    memStore = RDFNQuads.Deserialize(inputStream);
                    break;
                case RDFStoreEnums.RDFFormats.TriX:
                    memStore =  RDFTriX.Deserialize(inputStream);
                    break;
                case RDFStoreEnums.RDFFormats.TriG:
                    memStore =  RDFTriG.Deserialize(inputStream);
                    break;
            }

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
            {
                foreach (RDFGraph graph in memStore.ExtractGraphs())
                    RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            }
            #endregion

            return memStore;
        }

        /// <summary>
        /// Asynchronously reads a memory store from a stream of the given RDF format.
        /// </summary>
        public static Task<RDFMemoryStore> FromStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromStream(rdfFormat, inputStream, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        public static RDFMemoryStore FromDataTable(DataTable table, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (table == null)
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter is null.");
            if (!(table.Columns.Contains("?SUBJECT") && table.Columns.Contains("?PREDICATE") && table.Columns.Contains("?OBJECT")))
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter does not have the required columns \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");
            #endregion

            RDFMemoryStore memStore = new RDFMemoryStore();
            RDFContext defaultContext = new RDFContext();
            bool hasContextColumn = table.Columns.Contains("?CONTEXT");

            #region Parse Table
            foreach (DataRow tableRow in table.Rows)
            {
                #region CONTEXT
                RDFPatternMember rowContext;
                if (hasContextColumn)
                {
                    if (tableRow.IsNull("?CONTEXT") || string.IsNullOrEmpty(tableRow["?CONTEXT"].ToString()))
                    {
                        rowContext = defaultContext;
                    }
                    else
                    {
                        rowContext = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?CONTEXT"].ToString());
                        if (!(rowContext is RDFResource resource))
                            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?CONTEXT\" column.");
                        if (resource.IsBlank)
                            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource in the \"?CONTEXT\" column.");
                    }
                }
                else
                {
                    rowContext = defaultContext;
                }
                #endregion

                #region SUBJECT
                if (tableRow.IsNull("?SUBJECT") || string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?SUBJECT\" column.");

                RDFPatternMember rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
                if (!(rowSubj is RDFResource subj))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
                #endregion

                #region PREDICATE
                if (tableRow.IsNull("?PREDICATE") || string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?PREDICATE\" column.");

                RDFPatternMember rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
                if (!(rowPred is RDFResource pred))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?PREDICATE\" column.");
                if (pred.IsBlank)
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource in the \"?PREDICATE\" column.");
                #endregion

                #region OBJECT
                if (tableRow.IsNull("?OBJECT"))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL value in the \"?OBJECT\" column.");

                RDFPatternMember rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
                if (rowObj is RDFResource objRes)
                    memStore.AddQuadruple(new RDFQuadruple(new RDFContext(rowContext.ToString()), subj, pred, objRes));
                else
                    memStore.AddQuadruple(new RDFQuadruple(new RDFContext(rowContext.ToString()), subj, pred, (RDFLiteral)rowObj));
                #endregion
            }
            #endregion

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
            {
                foreach (RDFGraph graph in memStore.ExtractGraphs())
                    RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            }
            #endregion

            return memStore;
        }

        /// <summary>
        /// Asynchronously reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        public static Task<RDFMemoryStore> FromDataTableAsync(DataTable table, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromDataTable(table, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a memory store by trying to dereference the given Uri
        /// </summary>
        public static RDFMemoryStore FromUri(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
        {
            #region Guards
            if (uri == null)
                throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter is null.");
            if (!uri.IsAbsoluteUri)
                throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter does not represent an absolute Uri.");
            #endregion

            RDFMemoryStore memStore = new RDFMemoryStore();

            try
            {
                //Grab eventual dereference Uri
                Uri remappedUri = RDFModelUtilities.RemapUriForDereference(uri);

                HttpWebRequest webRequest = WebRequest.CreateHttp(remappedUri);
                webRequest.MaximumAutomaticRedirections = 4;
                webRequest.AllowAutoRedirect = true;
                webRequest.Timeout = timeoutMilliseconds;
                webRequest.Accept = "application/n-quads,application/trix,application/trig";

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webRequest.HaveResponse)
                {
                    //Cascade detection of ContentType from response
                    string responseContentType = webResponse.ContentType;
                    if (string.IsNullOrWhiteSpace(responseContentType))
                    {
                        responseContentType = webResponse.Headers["ContentType"];
                        if (string.IsNullOrWhiteSpace(responseContentType))
                            responseContentType = "application/n-quads"; //Fallback to N-QUADS
                    }

                    //N-QUADS
                    if (responseContentType.Contains("application/n-quads"))
                        memStore = FromStream(RDFStoreEnums.RDFFormats.NQuads, webResponse.GetResponseStream());

                    //TRIX
                    else if (responseContentType.Contains("application/trix"))
                        memStore = FromStream(RDFStoreEnums.RDFFormats.TriX, webResponse.GetResponseStream());

                    //TRIG
                    else if (responseContentType.Contains("application/trig"))
                        memStore = FromStream(RDFStoreEnums.RDFFormats.TriG, webResponse.GetResponseStream());
                }
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot read RDF memory store from Uri because: " + ex.Message);
            }

            #region Datatype Discovery
            if (enableDatatypeDiscovery)
            {
                foreach (RDFGraph graph in memStore.ExtractGraphs())
                    RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
            }
            #endregion

            return memStore;
        }

        /// <summary>
        /// Asynchronously reads a memory store by trying to dereference the given Uri
        /// </summary>
        public static Task<RDFMemoryStore> FromUriAsync(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromUri(uri, timeoutMilliseconds, enableDatatypeDiscovery));
        #endregion

        #endregion

        #endregion
    }
}