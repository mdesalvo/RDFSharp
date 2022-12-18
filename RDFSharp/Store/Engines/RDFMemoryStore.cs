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

using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFMemoryStore represents an in-memory RDF store engine.
    /// </summary>
    public class RDFMemoryStore : RDFStore, IEnumerable<RDFQuadruple>, IDisposable
    {
        #region Properties
        /// <summary>
        /// Count of the store's quadruples
        /// </summary>
        public long QuadruplesCount => IndexedQuadruples.Count;

        /// <summary>
        /// Gets the enumerator on the store's quadruples for iteration
        /// </summary>
        public IEnumerator<RDFQuadruple> QuadruplesEnumerator
        {
            get
            {
                foreach (RDFIndexedQuadruple indexedQuadruple in IndexedQuadruples.Values)
                {
                    yield return indexedQuadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                        ? new RDFQuadruple(StoreIndex.ContextsRegister[indexedQuadruple.ContextID], StoreIndex.ResourcesRegister[indexedQuadruple.SubjectID], StoreIndex.ResourcesRegister[indexedQuadruple.PredicateID], StoreIndex.ResourcesRegister[indexedQuadruple.ObjectID])
                        : new RDFQuadruple(StoreIndex.ContextsRegister[indexedQuadruple.ContextID], StoreIndex.ResourcesRegister[indexedQuadruple.SubjectID], StoreIndex.ResourcesRegister[indexedQuadruple.PredicateID], StoreIndex.LiteralsRegister[indexedQuadruple.ObjectID]);
                }
            }
        }

        /// <summary>
        /// Identifier of the memory store
        /// </summary>
        internal string StoreGUID { get; set; }

        /// <summary>
        /// Index on the quadruples of the store
        /// </summary>
        internal RDFStoreIndex StoreIndex { get; set; }

        /// <summary>
        /// Indexed quadruples embedded into the store
        /// </summary>
        internal Dictionary<long, RDFIndexedQuadruple> IndexedQuadruples { get; set; }

        /// <summary>
        /// Flag indicating that the store has already been disposed
        /// </summary>
        internal bool Disposed { get; set; }
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
            => string.Concat(base.ToString(), "|ID=", StoreGUID);

        /// <summary>
        /// Performs the equality comparison between two memory stores
        /// </summary>
        public bool Equals(RDFMemoryStore other)
        {
            if (other == null || QuadruplesCount != other.QuadruplesCount)
                return false;
            if (base.Equals(other))
                return true;

            foreach (RDFQuadruple q in this)
            {
                if (!other.ContainsQuadruple(q))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the store's quadruples
        /// </summary>
        IEnumerator<RDFQuadruple> IEnumerable<RDFQuadruple>.GetEnumerator() => QuadruplesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the store's quadruples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => QuadruplesEnumerator;

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
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                StoreIndex.Dispose();
                StoreIndex = null;

                IndexedQuadruples.Clear();
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
                foreach (RDFTriple t in graph)
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        AddQuadruple(new RDFQuadruple(graphCtx, (RDFResource)t.Subject, (RDFResource)t.Predicate, (RDFResource)t.Object));
                    else
                        AddQuadruple(new RDFQuadruple(graphCtx, (RDFResource)t.Subject, (RDFResource)t.Predicate, (RDFLiteral)t.Object));
                }
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
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubject(RDFResource subjectResource)
        {
            if (subjectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicate(RDFResource predicateResource)
        {
            if (predicateResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given resource as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByObject(RDFResource objectResource)
        {
            if (objectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given literal as object
        /// </summary>
        public override RDFStore RemoveQuadruplesByLiteral(RDFLiteral literalObject)
        {
            if (literalObject != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByLiteral(literalObject))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and subject
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubject(RDFContext contextResource, RDFResource subjectResource)
        {
            if (contextResource != null && subjectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesBySubject(subjectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextPredicate(RDFContext contextResource, RDFResource predicateResource)
        {
            if (contextResource != null && predicateResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextObject(RDFContext contextResource, RDFResource objectResource)
        {
            if (contextResource != null && objectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextLiteral(RDFContext contextResource, RDFLiteral objectLiteral)
        {
            if (contextResource != null && objectLiteral != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, subject and predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubjectPredicate(RDFContext contextResource, RDFResource subjectResource, RDFResource predicateResource)
        {
            if (contextResource != null && subjectResource != null && predicateResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesBySubject(subjectResource)
                                                  .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, subject and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubjectObject(RDFContext contextResource, RDFResource subjectResource, RDFResource objectResource)
        {
            if (contextResource != null && subjectResource != null && objectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesBySubject(subjectResource)
                                                  .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, subject and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextSubjectLiteral(RDFContext contextResource, RDFResource subjectResource, RDFLiteral objectLiteral)
        {
            if (contextResource != null && subjectResource != null && objectLiteral != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesBySubject(subjectResource)
                                                  .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, predicate and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextPredicateObject(RDFContext contextResource, RDFResource predicateResource, RDFResource objectResource)
        {
            if (contextResource != null && predicateResource != null && objectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesByPredicate(predicateResource)
                                                  .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given context, predicate and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByContextPredicateLiteral(RDFContext contextResource, RDFResource predicateResource, RDFLiteral objectLiteral)
        {
            if (contextResource != null && predicateResource != null && objectLiteral != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByContext(contextResource)
                                                .SelectQuadruplesByPredicate(predicateResource)
                                                  .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject and predicate
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubjectPredicate(RDFResource subjectResource, RDFResource predicateResource)
        {
            if (subjectResource != null && predicateResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource)
                                                .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject and object
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubjectObject(RDFResource subjectResource, RDFResource objectResource)
        {
            if (subjectResource != null && objectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource)
                                                .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given subject and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesBySubjectLiteral(RDFResource subjectResource, RDFLiteral objectLiteral)
        {
            if (subjectResource != null && objectLiteral != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesBySubject(subjectResource)
                                                .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate and object
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicateObject(RDFResource predicateResource, RDFResource objectResource)
        {
            if (predicateResource != null && objectResource != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByPredicate(predicateResource)
                                                .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the quadruples with the given predicate and literal
        /// </summary>
        public override RDFStore RemoveQuadruplesByPredicateLiteral(RDFResource predicateResource, RDFLiteral objectLiteral)
        {
            if (predicateResource != null && objectLiteral != null)
            {
                foreach (RDFQuadruple quad in SelectQuadruplesByPredicate(predicateResource)
                                                .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    IndexedQuadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    StoreIndex.RemoveIndex(quad);
                }
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
        /// Gets a store containing quadruples with the specified combination of CSPOL accessors<br/>
        /// (null values are threated as * selectors. Ensure to keep object and literal mutually exclusive!)
        /// </summary>
        public RDFMemoryStore this[RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit]
        {
            get
            {
                if (obj != null && lit != null)
                    throw new RDFStoreException("Cannot access a store when both object and literals are given: they have to be mutually exclusive!");
                return new RDFMemoryStore(RDFStoreUtilities.SelectQuadruples(this, ctx, subj, pred, obj, lit));
            }
        }

        /// <summary>
        /// Gets a store containing quadruples satisfying the given pattern
        /// </summary>
        internal override RDFMemoryStore SelectQuadruples(RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit)
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
                foreach (RDFQuadruple q in store.SelectAllQuadruples())
                    result.AddQuadruple(q);
            }

            return result;
        }

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
        #endregion

        #region Convert

        #region Import
        /// <summary>
        /// Reads a memory store from a file of the given RDF format.
        /// </summary>
        public static RDFMemoryStore FromFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter is null or empty.");
            if (!File.Exists(filepath))
                throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    return RDFNQuads.Deserialize(filepath);
                case RDFStoreEnums.RDFFormats.TriX:
                    return RDFTriX.Deserialize(filepath);
                case RDFStoreEnums.RDFFormats.TriG:
                    return RDFTriG.Deserialize(filepath);
            }
            throw new RDFStoreException("Cannot read RDF memory store from file because given \"rdfFormat\" parameter is not supported.");
        }

        /// <summary>
        /// Asynchronously reads a memory store from a file of the given RDF format.
        /// </summary>
        public static Task<RDFMemoryStore> FromFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
            => Task.Run(() => FromFile(rdfFormat, filepath));

        /// <summary>
        /// Reads a memory store from a stream of the given RDF format.
        /// </summary>
        public static RDFMemoryStore FromStream(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream)
        {
            if (inputStream == null)
                throw new RDFStoreException("Cannot read RDF memory store from stream because given \"inputStream\" parameter is null.");
            
            switch (rdfFormat)
            {
                case RDFStoreEnums.RDFFormats.NQuads:
                    return RDFNQuads.Deserialize(inputStream);
                case RDFStoreEnums.RDFFormats.TriX:
                    return RDFTriX.Deserialize(inputStream);
                case RDFStoreEnums.RDFFormats.TriG:
                    return RDFTriG.Deserialize(inputStream);                    
            }
            throw new RDFStoreException("Cannot read RDF memory store from stream because given \"rdfFormat\" parameter is not supported.");
        }

        /// <summary>
        /// Asynchronously reads a memory store from a stream of the given RDF format.
        /// </summary>
        public static Task<RDFMemoryStore> FromStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream)
            => Task.Run(() => FromStream(rdfFormat, inputStream));

        /// <summary>
        /// Reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        public static RDFMemoryStore FromDataTable(DataTable table)
        {
            if (table == null)
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter is null.");
            if (table.Columns.Count != 4)
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter does not have exactly 4 columns.");
            if (!(table.Columns.Contains("?CONTEXT") && table.Columns.Contains("?SUBJECT") && table.Columns.Contains("?PREDICATE") && table.Columns.Contains("?OBJECT")))
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter does not have the required columns \"?CONTEXT\", \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");

            RDFMemoryStore result = new RDFMemoryStore();

            //Iterate the rows of the datatable
            foreach (DataRow tableRow in table.Rows)
            {
                #region CONTEXT
                if (tableRow.IsNull("?CONTEXT") || string.IsNullOrEmpty(tableRow["?CONTEXT"].ToString()))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?CONTEXT\" column.");
                
                RDFPatternMember rowCtx = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?CONTEXT"].ToString());
                if (!(rowCtx is RDFResource))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?CONTEXT\" column.");
                if (((RDFResource)rowCtx).IsBlank)
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource in the \"?CONTEXT\" column.");
                #endregion

                #region SUBJECT
                if (tableRow.IsNull("?SUBJECT") || string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?SUBJECT\" column.");
                
                RDFPatternMember rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
                if (!(rowSubj is RDFResource))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
                #endregion

                #region PREDICATE
                if (tableRow.IsNull("?PREDICATE") || string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?PREDICATE\" column.");

                RDFPatternMember rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
                if (!(rowPred is RDFResource))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?PREDICATE\" column.");
                if (((RDFResource)rowPred).IsBlank)
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource in the \"?PREDICATE\" column.");
                #endregion

                #region OBJECT
                if (tableRow.IsNull("?OBJECT"))
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL value in the \"?OBJECT\" column.");
                
                RDFPatternMember rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
                if (rowObj is RDFResource)
                    result.AddQuadruple(new RDFQuadruple(new RDFContext(rowCtx.ToString()), (RDFResource)rowSubj, (RDFResource)rowPred, (RDFResource)rowObj));
                else
                    result.AddQuadruple(new RDFQuadruple(new RDFContext(rowCtx.ToString()), (RDFResource)rowSubj, (RDFResource)rowPred, (RDFLiteral)rowObj));
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        public static Task<RDFMemoryStore> FromDataTableAsync(DataTable table)
            => Task.Run(() => FromDataTable(table));

        /// <summary>
        /// Reads a memory store by trying to dereference the given Uri
        /// </summary>
        public static RDFMemoryStore FromUri(Uri uri, int timeoutMilliseconds = 20000)
        {
            if (uri == null)
                throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter is null.");
            if (!uri.IsAbsoluteUri)
                throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter does not represent an absolute Uri.");

            RDFMemoryStore result = new RDFMemoryStore();

            try
            {
                //Grab eventual dereference Uri
                Uri remappedUri = RDFModelUtilities.RemapUriForDereference(uri);

                HttpWebRequest webRequest = WebRequest.CreateHttp(remappedUri);
                webRequest.MaximumAutomaticRedirections = 4;
                webRequest.AllowAutoRedirect = true;
                webRequest.Timeout = timeoutMilliseconds;
                //N-QUADS
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/n-quads");
                //TRIX
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/trix");
                //TRIG
                webRequest.Headers.Add(HttpRequestHeader.Accept, "application/trig");

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
                        result = FromStream(RDFStoreEnums.RDFFormats.NQuads, webResponse.GetResponseStream());

                    //TRIX
                    else if (responseContentType.Contains("application/trix"))
                        result = FromStream(RDFStoreEnums.RDFFormats.TriX, webResponse.GetResponseStream());

                    //TRIG
                    else if (responseContentType.Contains("application/trig"))
                        result = FromStream(RDFStoreEnums.RDFFormats.TriG, webResponse.GetResponseStream());
                }
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot read RDF memory store from Uri because: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously reads a memory store by trying to dereference the given Uri
        /// </summary>
        public static Task<RDFMemoryStore> FromUriAsync(Uri uri, int timeoutMilliseconds = 20000)
            => Task.Run(() => FromUri(uri, timeoutMilliseconds));
        #endregion

        #endregion

        #endregion
    }
}