/*
   Copyright 2012-2020 Marco De Salvo

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

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFMemoryStore represents an in-memory RDF store engine.
    /// </summary>
    public sealed class RDFMemoryStore : RDFStore, IEnumerable<RDFQuadruple>
    {

        #region Properties
        /// <summary>
        /// Count of the store's quadruples
        /// </summary>
        public long QuadruplesCount => this.Quadruples.Count;

        /// <summary>
        /// Gets the enumerator on the store's quadruples for iteration
        /// </summary>
        public IEnumerator<RDFQuadruple> QuadruplesEnumerator => this.Quadruples.Values.GetEnumerator();

        /// <summary>
        /// Identifier of the memory store
        /// </summary>
        internal string StoreGUID { get; set; }

        /// <summary>
        /// Index on the quadruples of the store
        /// </summary>
        internal RDFStoreIndex StoreIndex { get; set; }

        /// <summary>
        /// List of quadruples embedded into the store
        /// </summary>
        internal Dictionary<long, RDFQuadruple> Quadruples { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty memory store
        /// </summary>
        public RDFMemoryStore()
        {
            this.StoreType = "MEMORY";
            this.StoreGUID = Guid.NewGuid().ToString();
            this.StoreIndex = new RDFStoreIndex();
            this.StoreID = RDFModelUtilities.CreateHash(this.ToString());
            this.Quadruples = new Dictionary<long, RDFQuadruple>();
        }

        /// <summary>
        /// List-based ctor to build a memory store with the given list of quadruples
        /// </summary>
        public RDFMemoryStore(List<RDFQuadruple> quadruples) : this()
            => quadruples?.ForEach(q => this.AddQuadruple(q));
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the Memory store
        /// </summary>
        public override string ToString()
            => string.Concat(base.ToString(), "|ID=", this.StoreGUID);

        /// <summary>
        /// Performs the equality comparison between two memory stores
        /// </summary>
        public bool Equals(RDFMemoryStore other)
        {
            if (other == null || this.QuadruplesCount != other.QuadruplesCount)
            {
                return false;
            }
            foreach (RDFQuadruple q in this)
            {
                if (!other.ContainsQuadruple(q))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the store's quadruples
        /// </summary>
        IEnumerator<RDFQuadruple> IEnumerable<RDFQuadruple>.GetEnumerator() => this.QuadruplesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the store's quadruples
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.QuadruplesEnumerator;
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
                var graphCtx = new RDFContext(graph.Context);
                foreach (var t in graph)
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        this.AddQuadruple(new RDFQuadruple(graphCtx, (RDFResource)t.Subject, (RDFResource)t.Predicate, (RDFResource)t.Object));
                    }
                    else
                    {
                        this.AddQuadruple(new RDFQuadruple(graphCtx, (RDFResource)t.Subject, (RDFResource)t.Predicate, (RDFLiteral)t.Object));
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given quadruple to the store, avoiding duplicate insertions
        /// </summary>
        public override RDFStore AddQuadruple(RDFQuadruple quadruple)
        {
            if (quadruple != null)
            {
                if (!this.Quadruples.ContainsKey(quadruple.QuadrupleID))
                {
                    //Add quadruple
                    this.Quadruples.Add(quadruple.QuadrupleID, quadruple);
                    //Add index
                    this.StoreIndex.AddIndex(quadruple);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleAdded(string.Format("Quadruple '{0}' has been added to the Store '{1}'.", quadruple, this));
                }
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
            if (this.ContainsQuadruple(quadruple))
            {
                //Remove quadruple
                this.Quadruples.Remove(quadruple.QuadrupleID);
                //Remove index
                this.StoreIndex.RemoveIndex(quadruple);
                //Raise event
                RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quadruple, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesBySubject(subjectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByLiteral(literalObject))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesBySubject(subjectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesBySubject(subjectResource)
                                         .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesBySubject(subjectResource)
                                         .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesBySubject(subjectResource)
                                         .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesByPredicate(predicateResource)
                                         .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByContext(contextResource)
                                         .SelectQuadruplesByPredicate(predicateResource)
                                         .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesBySubject(subjectResource)
                                         .SelectQuadruplesByPredicate(predicateResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesBySubject(subjectResource)
                                         .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesBySubject(subjectResource)
                                         .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByPredicate(predicateResource)
                                         .SelectQuadruplesByObject(objectResource))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
                foreach (var quad in this.SelectQuadruplesByPredicate(predicateResource)
                                         .SelectQuadruplesByLiteral(objectLiteral))
                {
                    //Remove quadruple
                    this.Quadruples.Remove(quad.QuadrupleID);
                    //Remove index
                    this.StoreIndex.RemoveIndex(quad);
                    //Raise event
                    RDFStoreEvents.RaiseOnQuadrupleRemoved(string.Format("Quadruple '{0}' has been removed from the Store '{1}'.", quad, this));
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
            this.Quadruples.Clear();
            //Clear index
            this.StoreIndex.ClearIndex();
            //Raise event
            RDFStoreEvents.RaiseOnStoreCleared(string.Format("Store '{0}' has been cleared.", this));
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the store contains the given quadruple
        /// </summary>
        public override bool ContainsQuadruple(RDFQuadruple quadruple)
        {
            return (quadruple != null && this.Quadruples.ContainsKey(quadruple.QuadrupleID));
        }

        /// <summary>
        /// Gets a store containing quadruples satisfying the given pattern
        /// </summary>
        internal override RDFMemoryStore SelectQuadruples(RDFContext contextResource,
                                                          RDFResource subjectResource,
                                                          RDFResource predicateResource,
                                                          RDFResource objectResource,
                                                          RDFLiteral objectLiteral)
        {
            return (new RDFMemoryStore(RDFStoreUtilities.SelectQuadruples(this, contextResource, subjectResource, predicateResource, objectResource, objectLiteral)));
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection store from this store and a given one
        /// </summary>
        public RDFMemoryStore IntersectWith(RDFStore store)
        {
            var result = new RDFMemoryStore();
            if (store != null)
            {

                //Add intersection quadruples
                foreach (var q in this)
                {
                    if (store.ContainsQuadruple(q))
                    {
                        result.AddQuadruple(q);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Builds a new union store from this store and a given one
        /// </summary>
        public RDFMemoryStore UnionWith(RDFStore store)
        {
            var result = new RDFMemoryStore();

            //Add quadruples from this store
            foreach (var q in this)
            {
                result.AddQuadruple(q);
            }

            //Manage the given store
            if (store != null)
            {

                //Add quadruples from the given store
                foreach (var q in store.SelectAllQuadruples())
                {
                    result.AddQuadruple(q);
                }

            }

            return result;
        }

        /// <summary>
        /// Builds a new difference store from this store and a given one
        /// </summary>
        public RDFMemoryStore DifferenceWith(RDFStore store)
        {
            var result = new RDFMemoryStore();
            if (store != null)
            {

                //Add difference quadruples
                foreach (var q in this)
                {
                    if (!store.ContainsQuadruple(q))
                    {
                        result.AddQuadruple(q);
                    }
                }

            }
            else
            {

                //Add quadruples from this store
                foreach (var q in this)
                {
                    result.AddQuadruple(q);
                }

            }
            return result;
        }
        #endregion

        #region Convert

        #region Import
        /// <summary>
        /// Creates a memory store from a file of the given RDF format.
        /// </summary>
        public static RDFMemoryStore FromFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                if (File.Exists(filepath))
                {
                    switch (rdfFormat)
                    {
                        case RDFStoreEnums.RDFFormats.NQuads:
                            return RDFNQuads.Deserialize(filepath);
                        case RDFStoreEnums.RDFFormats.TriX:
                            return RDFTriX.Deserialize(filepath);
                    }
                }
                throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            }
            throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter is null or empty.");
        }

        /// <summary>
        /// Creates a memory store from a stream of the given RDF format.
        /// </summary>
        public static RDFMemoryStore FromStream(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream)
        {
            if (inputStream != null)
            {
                switch (rdfFormat)
                {
                    case RDFStoreEnums.RDFFormats.NQuads:
                        return RDFNQuads.Deserialize(inputStream);
                    case RDFStoreEnums.RDFFormats.TriX:
                        return RDFTriX.Deserialize(inputStream);
                }
            }
            throw new RDFStoreException("Cannot read RDF memory store from stream because given \"inputStream\" parameter is null.");
        }

        /// <summary>
        /// Creates a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
        /// </summary>
        public static RDFMemoryStore FromDataTable(DataTable table)
        {
            var result = new RDFMemoryStore();

            //Check the structure of the datatable for consistency against the "C-S-P-O" RDF model
            if (table != null && table.Columns.Count == 4)
            {
                if (table.Columns.Contains("?CONTEXT")
                        && table.Columns.Contains("?SUBJECT")
                            && table.Columns.Contains("?PREDICATE")
                                && table.Columns.Contains("?OBJECT"))
                {

                    //Iterate the rows of the datatable
                    foreach (DataRow tableRow in table.Rows)
                    {

                        #region CONTEXT
                        //Parse the quadruple context
                        if (!tableRow.IsNull("?CONTEXT") && !string.IsNullOrEmpty(tableRow["?CONTEXT"].ToString()))
                        {
                            var rowCont = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?CONTEXT"].ToString());
                            if (rowCont is RDFResource && !((RDFResource)rowCont).IsBlank)
                            {

                                #region SUBJECT
                                //Parse the quadruple subject
                                if (!tableRow.IsNull("?SUBJECT") && !string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                                {
                                    var rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
                                    if (rowSubj is RDFResource)
                                    {

                                        #region PREDICATE
                                        //Parse the quadruple predicate
                                        if (!tableRow.IsNull("?PREDICATE") && !string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                                        {
                                            var rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
                                            if (rowPred is RDFResource && !((RDFResource)rowPred).IsBlank)
                                            {

                                                #region OBJECT
                                                //Parse the quadruple object
                                                if (!tableRow.IsNull("?OBJECT"))
                                                {
                                                    var rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
                                                    if (rowObj is RDFResource)
                                                    {
                                                        result.AddQuadruple(new RDFQuadruple(new RDFContext(rowCont.ToString()), (RDFResource)rowSubj, (RDFResource)rowPred, (RDFResource)rowObj));
                                                    }
                                                    else
                                                    {
                                                        result.AddQuadruple(new RDFQuadruple(new RDFContext(rowCont.ToString()), (RDFResource)rowSubj, (RDFResource)rowPred, (RDFLiteral)rowObj));
                                                    }
                                                }
                                                else
                                                {
                                                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL value in the \"?OBJECT\" column.");
                                                }
                                                #endregion

                                            }
                                            else
                                            {
                                                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource or a literal in the \"?PREDICATE\" column.");
                                            }
                                        }
                                        else
                                        {
                                            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL or empty value in the \"?PREDICATE\" column.");
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
                                    }
                                }
                                else
                                {
                                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL or empty value in the \"?SUBJECT\" column.");
                                }
                                #endregion

                            }
                            else
                            {
                                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource or a literal in the \"?CONTEXT\" column.");
                            }
                        }
                        else
                        {
                            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL or empty value in the \"?CONTEXT\" column.");
                        }
                        #endregion

                    }

                }
                else
                {
                    throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter does not have the required columns \"?CONTEXT\", \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");
                }
            }
            else
            {
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter is null, or it does not have exactly 4 columns.");
            }

            return result;
        }

        /// <summary>
        /// Creates a memory store by trying to dereference the given Uri
        /// </summary>
        public static RDFMemoryStore FromUri(Uri uri, int timeoutMilliseconds = 20000)
        {
            var result = new RDFMemoryStore();

            if (uri != null && uri.IsAbsoluteUri)
            {
                uri = RDFModelUtilities.RemapUriForDereference(uri);
                try
                {
                    HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
                    webRequest.MaximumAutomaticRedirections = 3;
                    webRequest.AllowAutoRedirect = true;
                    webRequest.Timeout = timeoutMilliseconds;
                    //N-QUADS
                    webRequest.Headers.Add(HttpRequestHeader.Accept, "application/n-quads");
                    //TRIX
                    webRequest.Headers.Add(HttpRequestHeader.Accept, "application/trix");

                    HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                    if (webRequest.HaveResponse)
                    {
                        //N-QUADS
                        if (string.IsNullOrEmpty(webResponse.ContentType) ||
                                webResponse.ContentType.Contains("application/n-quads"))
                            result = FromStream(RDFStoreEnums.RDFFormats.NQuads, webResponse.GetResponseStream());

                        //TRIX
                        else if (webResponse.ContentType.Contains("application/trix"))
                            result = FromStream(RDFStoreEnums.RDFFormats.TriX, webResponse.GetResponseStream());
                    }
                }
                catch (Exception ex)
                {
                    throw new RDFStoreException("Cannot read RDF memory store from Uri because: " + ex.Message);
                }
            }
            else
            {
                throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter is null, or it does not represent an absolute Uri.");
            }

            return result;
        }
        #endregion

        #endregion

        #endregion

    }

}
