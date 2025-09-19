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
using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFMemoryStore represents an in-memory RDF store engine.
    /// </summary>
#if NET8_0_OR_GREATER
    public sealed class RDFMemoryStore : RDFStore, IEnumerable<RDFQuadruple>, IAsyncEnumerable<RDFQuadruple>, IDisposable
#else
    public sealed class RDFMemoryStore : RDFStore, IEnumerable<RDFQuadruple>, IDisposable
#endif
    {
        #region Properties
        /// <summary>
        /// Count of the store's quadruples
        /// </summary>
        public override long QuadruplesCount
            => Index.Hashes.Count;

        /// <summary>
        /// Asynchronous count of the store's quadruples
        /// </summary>
        public override Task<long> QuadruplesCountAsync
            => Task.Run(() => QuadruplesCount);

        /// <summary>
        /// Gets the enumerator on the store's quadruples for iteration
        /// </summary>
        public IEnumerator<RDFQuadruple> QuadruplesEnumerator
        {
            get
            {
                foreach (RDFHashedQuadruple hashedQuadruple in Index.Hashes.Values)
                {
                    yield return hashedQuadruple.TripleFlavor == 1 //SPO
                        ? new RDFQuadruple(Index.Contexts[hashedQuadruple.ContextID], Index.Resources[hashedQuadruple.SubjectID], Index.Resources[hashedQuadruple.PredicateID], Index.Resources[hashedQuadruple.ObjectID])
                        : new RDFQuadruple(Index.Contexts[hashedQuadruple.ContextID], Index.Resources[hashedQuadruple.SubjectID], Index.Resources[hashedQuadruple.PredicateID], Index.Literals[hashedQuadruple.ObjectID]);
                }
            }
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Asynchronously gets the enumerator on the store's quadruples for iteration
        /// </summary>
        public IAsyncEnumerable<RDFQuadruple> QuadruplesEnumeratorAsync => GetQuadruplesAsync();
        private async IAsyncEnumerable<RDFQuadruple> GetQuadruplesAsync([EnumeratorCancellation] CancellationToken cancellationToken=default)
        {
            foreach (RDFHashedQuadruple hashedQuadruple in Index.Hashes.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return hashedQuadruple.TripleFlavor == 1 //SPO
                                ? new RDFQuadruple(Index.Contexts[hashedQuadruple.ContextID],
                                                   Index.Resources[hashedQuadruple.SubjectID],
                                                   Index.Resources[hashedQuadruple.PredicateID],
                                                   Index.Resources[hashedQuadruple.ObjectID])
                                : new RDFQuadruple(Index.Contexts[hashedQuadruple.ContextID],
                                                   Index.Resources[hashedQuadruple.SubjectID],
                                                   Index.Resources[hashedQuadruple.PredicateID],
                                                   Index.Literals[hashedQuadruple.ObjectID]);
            }
        }
#endif

        /// <summary>
        /// Index on the quadruples of the store
        /// </summary>
        internal RDFStoreIndex Index { get; set; }

        /// <summary>
        /// Identifier of the store
        /// </summary>
        internal string StoreGUID { get; }

        /// <summary>
        /// Flag indicating that the store has already been disposed
        /// </summary>
        internal bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        ///Builds an empty memory store
        /// </summary>
        public RDFMemoryStore()
        {
            StoreType = "MEMORY";
            StoreGUID = Guid.NewGuid().ToString("N");
            StoreID = RDFModelUtilities.CreateHash(ToString());
            Index = new RDFStoreIndex();
        }

        /// <summary>
        /// Builds a memory store with the given list of quadruples
        /// </summary>
        public RDFMemoryStore(List<RDFQuadruple> quadruples) : this()
            => quadruples?.ForEach(q => AddQuadruple(q));

        /// <summary>
        /// Destroys the memory store instance
        /// </summary>
        ~RDFMemoryStore()
            => Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the memory store
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
            return StoreID == other.StoreID || this.All(other.ContainsQuadruple);
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

#if NET8_0_OR_GREATER
        /// <summary>
        /// Asynchronously exposes a typed enumerator on the store's quadruples
        /// </summary>
        public IAsyncEnumerator<RDFQuadruple> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => GetQuadruplesAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
#endif

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
                Index?.Dispose();
                Index = null;
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
        /// Asynchronously merges the given graph into the store, avoiding duplicate insertions
        /// </summary>
        public override Task<RDFStore> MergeGraphAsync(RDFGraph graph)
            => Task.Run(() => MergeGraph(graph));

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore AddQuadruple(RDFQuadruple quadruple)
        {
            if (quadruple != null)
                Index.Add(quadruple);
            return this;
        }

        /// <summary>
        /// Asynchronously adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public override Task<RDFStore> AddQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => AddQuadruple(quadruple));
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the store
        /// </summary>
        public override RDFStore RemoveQuadruple(RDFQuadruple quadruple)
        {
            if (quadruple != null)
                Index.Remove(quadruple);
            return this;
        }

        /// <summary>
        /// Asynchronously removes the given quadruple from the store
        /// </summary>
        public override Task<RDFStore> RemoveQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => RemoveQuadruple(quadruple));

        /// <summary>
        /// Removes the quadruples which satisfy the given combination of CSPOL accessors<br/>
        /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
        /// </summary>
        public override RDFStore RemoveQuadruples(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
        {
            foreach (RDFQuadruple quadruple in SelectQuadruples(c, s, p, o, l))
                RemoveQuadruple(quadruple);
            return this;
        }

        /// <summary>
        /// Asynchronously removes the quadruples which satisfy the given combination of CSPOL accessors<br/>
        /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
        /// </summary>
        public override Task<RDFStore> RemoveQuadruplesAsync(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
            => Task.Run(() => RemoveQuadruples(c,s,p,o,l));

        /// <summary>
        /// Clears the quadruples of the store
        /// </summary>
        public override void ClearQuadruples()
            => Index.Clear();

        /// <summary>
        /// Asynchronously clears the quadruples of the store
        /// </summary>
        public override Task ClearQuadruplesAsync()
            => Task.Run(ClearQuadruples);
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public override bool ContainsQuadruple(RDFQuadruple quadruple)
            => quadruple != null && Index.Hashes.ContainsKey(quadruple.QuadrupleID);

        /// <summary>
        /// Asynchronously checks if the store contains the given quadruple
        /// </summary>
        public override Task<bool> ContainsQuadrupleAsync(RDFQuadruple quadruple)
            => Task.Run(() => ContainsQuadruple(quadruple));

        /// <summary>
        /// Selects the quadruples which satisfy the given combination of CSPOL accessors<br/>
        /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        public override List<RDFQuadruple> SelectQuadruples(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
        {
            #region Guards
            if (o != null && l != null)
                throw new RDFStoreException("Cannot access a store when both object and literals are given: they must be mutually exclusive!");
            #endregion

            #region Utilities
            void LookupIndex(HashSet<long> lookup, out List<RDFHashedQuadruple> result)
            {
                result = new List<RDFHashedQuadruple>(lookup.Count);
                result.AddRange(lookup.Select(q => Index.Hashes[q]));
            }
            #endregion

            StringBuilder queryFilters = new StringBuilder();
            List<RDFHashedQuadruple> C=null, S=null, P=null, O=null, L=null;

            //Filter by Context
            if (c != null)
            {
                queryFilters.Append('C');
                LookupIndex(Index.LookupIndexByContext(c), out C);
            }

            //Filter by Subject
            if (s != null)
            {
                queryFilters.Append('S');
                LookupIndex(Index.LookupIndexBySubject(s), out S);
            }

            //Filter by Predicate
            if (p != null)
            {
                queryFilters.Append('P');
                LookupIndex(Index.LookupIndexByPredicate(p), out P);
            }

            //Filter by Object
            if (o != null)
            {
                queryFilters.Append('O');
                LookupIndex(Index.LookupIndexByObject(o), out O);
            }

            //Filter by Literal
            if (l != null)
            {
                queryFilters.Append('L');
                LookupIndex(Index.LookupIndexByLiteral(l), out L);
            }

            //Intersect the filters
            switch (queryFilters.ToString())
            {
                case "C":    return C.ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "S":    return S.ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "P":    return P.ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "O":    return O.ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "L":    return L.ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CS":   return C.Intersect(S).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CP":   return C.Intersect(P).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CO":   return C.Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CL":   return C.Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CSP":  return C.Intersect(S).Intersect(P).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CSO":  return C.Intersect(S).Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CSL":  return C.Intersect(S).Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CPO":  return C.Intersect(P).Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CPL":  return C.Intersect(P).Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CSPO": return C.Intersect(S).Intersect(P).Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "CSPL": return C.Intersect(S).Intersect(P).Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "SP":   return S.Intersect(P).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "SO":   return S.Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "SL":   return S.Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "SPO":  return S.Intersect(P).Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "SPL":  return S.Intersect(P).Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "PO":   return P.Intersect(O).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                case "PL":   return P.Intersect(L).ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
                default:     return Index.Hashes.Values.ToList().ConvertAll(hq => new RDFQuadruple(hq, Index));
            }
        }

        /// <summary>
        /// Asynchronously selects the quadruples which satisfy the given combination of CSPOL accessors<br/>
        /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        public override Task<List<RDFQuadruple>> SelectQuadruplesAsync(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
            => Task.Run(() => SelectQuadruples(c,s,p,o,l));
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection store from this store and a given one
        /// </summary>
        public RDFMemoryStore IntersectWith(RDFStore store)
        {
            RDFMemoryStore result = new RDFMemoryStore();
            if (store != null)
            {
                //Add intersection quadruples
                foreach (RDFQuadruple q in this)
                {
                    if (store.ContainsQuadruple(q))
                        result.AddQuadruple(q);
                }
            }
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
            {
                //Add quadruples from the given store
                foreach (RDFQuadruple q in store.SelectQuadruples())
                    result.AddQuadruple(q);
            }

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
                {
                    if (!store.ContainsQuadruple(q))
                        result.AddQuadruple(q);
                }
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
        /// <exception cref="RDFStoreException"></exception>
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
        /// <exception cref="RDFStoreException"></exception>
        public static Task<RDFMemoryStore> FromFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromFile(rdfFormat, filepath, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a memory store from a stream of the given RDF format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
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
        /// <exception cref="RDFStoreException"></exception>
        public static Task<RDFMemoryStore> FromStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromStream(rdfFormat, inputStream, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
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
        /// <exception cref="RDFStoreException"></exception>
        public static Task<RDFMemoryStore> FromDataTableAsync(DataTable table, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromDataTable(table, enableDatatypeDiscovery));

        /// <summary>
        /// Reads a memory store by trying to dereference the given Uri
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
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
                webRequest.MaximumAutomaticRedirections = 3;
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
        /// <exception cref="RDFStoreException"></exception>
        public static Task<RDFMemoryStore> FromUriAsync(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
            => Task.Run(() => FromUri(uri, timeoutMilliseconds, enableDatatypeDiscovery));
        #endregion

        #endregion

        #endregion
    }
}