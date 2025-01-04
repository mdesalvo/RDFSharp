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

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFStoreIndex represents an automatically managed in-memory index structure for the quadruples of a store.
    /// </summary>
    internal class RDFStoreIndex : IDisposable
    {
        #region Properties
        /// <summary>
        /// Register of the store's contexts
        /// </summary>
        internal Dictionary<long, RDFContext> ContextsRegister { get; set; }

        /// <summary>
        /// Register of the store's resources
        /// </summary>
        internal Dictionary<long, RDFResource> ResourcesRegister { get; set; }

        /// <summary>
        /// Register of the store's literals
        /// </summary>
        internal Dictionary<long, RDFLiteral> LiteralsRegister { get; set; }

        /// <summary>
        /// Index on the contexts of the store's quadruples
        /// </summary>
        internal Dictionary<long, HashSet<long>> ContextsIndex { get; set; }

        /// <summary>
        /// Index on the subjects of the store's quadruples
        /// </summary>
        internal Dictionary<long, HashSet<long>> SubjectsIndex { get; set; }

        /// <summary>
        /// Index on the predicates of the store's quadruples
        /// </summary>
        internal Dictionary<long, HashSet<long>> PredicatesIndex { get; set; }

        /// <summary>
        /// Index on the objects of the store's quadruples
        /// </summary>
        internal Dictionary<long, HashSet<long>> ObjectsIndex { get; set; }

        /// <summary>
        /// Index on the literals of the store's quadruples
        /// </summary>
        internal Dictionary<long, HashSet<long>> LiteralsIndex { get; set; }

        /// <summary>
        /// Flag indicating that the store index has already been disposed
        /// </summary>
        internal bool Disposed { get; set; }

        /// <summary>
        /// Empty hashset to be returned in case of index miss
        /// </summary>
        private static readonly HashSet<long> EmptyHashSet = new HashSet<long>();
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor for an empty store index
        /// </summary>
        internal RDFStoreIndex()
        {
            //Registers
            ContextsRegister = new Dictionary<long, RDFContext>();
            ResourcesRegister = new Dictionary<long, RDFResource>();
            LiteralsRegister = new Dictionary<long, RDFLiteral>();
            //Indexes
            ContextsIndex = new Dictionary<long, HashSet<long>>();
            SubjectsIndex = new Dictionary<long, HashSet<long>>();
            PredicatesIndex = new Dictionary<long, HashSet<long>>();
            ObjectsIndex = new Dictionary<long, HashSet<long>>();
            LiteralsIndex = new Dictionary<long, HashSet<long>>();
        }

        /// <summary>
        /// Destroys the store index instance
        /// </summary>
        ~RDFStoreIndex() => Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the store index (IDisposable)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the store index 
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                ClearIndex();

                //Registers
                ContextsRegister = null;
                ResourcesRegister = null;
                LiteralsRegister = null;
                //Indexes
                ContextsIndex = null;
                SubjectsIndex = null;
                PredicatesIndex = null;
                ObjectsIndex = null;
                LiteralsIndex = null;
            }

            Disposed = true;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given quadruple to the CSPOL index
        /// </summary>
        internal RDFStoreIndex AddIndex(RDFQuadruple quadruple)
        {
            //Context (Register)
            if (!ContextsRegister.ContainsKey(quadruple.Context.PatternMemberID))
                ContextsRegister.Add(quadruple.Context.PatternMemberID, (RDFContext)quadruple.Context);
            //Context (Index)
            if (!ContextsIndex.ContainsKey(quadruple.Context.PatternMemberID))
                ContextsIndex.Add(quadruple.Context.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
            else if (!ContextsIndex[quadruple.Context.PatternMemberID].Contains(quadruple.QuadrupleID))
                ContextsIndex[quadruple.Context.PatternMemberID].Add(quadruple.QuadrupleID);

            //Subject (Register)
            if (!ResourcesRegister.ContainsKey(quadruple.Subject.PatternMemberID))
                ResourcesRegister.Add(quadruple.Subject.PatternMemberID, (RDFResource)quadruple.Subject);
            //Subject (Index)
            if (!SubjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID))
                SubjectsIndex.Add(quadruple.Subject.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
            else if (!SubjectsIndex[quadruple.Subject.PatternMemberID].Contains(quadruple.QuadrupleID))
                SubjectsIndex[quadruple.Subject.PatternMemberID].Add(quadruple.QuadrupleID);

            //Predicate (Register)
            if (!ResourcesRegister.ContainsKey(quadruple.Predicate.PatternMemberID))
                ResourcesRegister.Add(quadruple.Predicate.PatternMemberID, (RDFResource)quadruple.Predicate);
            //Predicate (Index)
            if (!PredicatesIndex.ContainsKey(quadruple.Predicate.PatternMemberID))
                PredicatesIndex.Add(quadruple.Predicate.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
            else if (!PredicatesIndex[quadruple.Predicate.PatternMemberID].Contains(quadruple.QuadrupleID))
                PredicatesIndex[quadruple.Predicate.PatternMemberID].Add(quadruple.QuadrupleID);

            //Object
            if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                //Register
                if (!ResourcesRegister.ContainsKey(quadruple.Object.PatternMemberID))
                    ResourcesRegister.Add(quadruple.Object.PatternMemberID, (RDFResource)quadruple.Object);
                //Index
                if (!ObjectsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                    ObjectsIndex.Add(quadruple.Object.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                else if (!ObjectsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                    ObjectsIndex[quadruple.Object.PatternMemberID].Add(quadruple.QuadrupleID);
            }

            //Literal
            else
            {
                //Register
                if (!LiteralsRegister.ContainsKey(quadruple.Object.PatternMemberID))
                    LiteralsRegister.Add(quadruple.Object.PatternMemberID, (RDFLiteral)quadruple.Object);
                //Index
                if (!LiteralsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                    LiteralsIndex.Add(quadruple.Object.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                else if (!LiteralsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                    LiteralsIndex[quadruple.Object.PatternMemberID].Add(quadruple.QuadrupleID);
            }

            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the CSPOL index
        /// </summary>
        internal RDFStoreIndex RemoveIndex(RDFQuadruple quadruple)
        {
            //Context
            if (ContextsIndex.ContainsKey(quadruple.Context.PatternMemberID)
                 && ContextsIndex[quadruple.Context.PatternMemberID].Contains(quadruple.QuadrupleID))
            {
                ContextsIndex[quadruple.Context.PatternMemberID].Remove(quadruple.QuadrupleID);
                if (ContextsIndex[quadruple.Context.PatternMemberID].Count == 0)
                    ContextsIndex.Remove(quadruple.Context.PatternMemberID);
            }

            //Subject
            if (SubjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID)
                 && SubjectsIndex[quadruple.Subject.PatternMemberID].Contains(quadruple.QuadrupleID))
            {
                SubjectsIndex[quadruple.Subject.PatternMemberID].Remove(quadruple.QuadrupleID);
                if (SubjectsIndex[quadruple.Subject.PatternMemberID].Count == 0)
                    SubjectsIndex.Remove(quadruple.Subject.PatternMemberID);
            }

            //Predicate
            if (PredicatesIndex.ContainsKey(quadruple.Predicate.PatternMemberID)
                 && PredicatesIndex[quadruple.Predicate.PatternMemberID].Contains(quadruple.QuadrupleID))
            {
                PredicatesIndex[quadruple.Predicate.PatternMemberID].Remove(quadruple.QuadrupleID);
                if (PredicatesIndex[quadruple.Predicate.PatternMemberID].Count == 0)
                    PredicatesIndex.Remove(quadruple.Predicate.PatternMemberID);
            }

            //Object
            if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                if (ObjectsIndex.ContainsKey(quadruple.Object.PatternMemberID)
                     && ObjectsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                {
                    ObjectsIndex[quadruple.Object.PatternMemberID].Remove(quadruple.QuadrupleID);
                    if (ObjectsIndex[quadruple.Object.PatternMemberID].Count == 0)
                        ObjectsIndex.Remove(quadruple.Object.PatternMemberID);
                }
            }

            //Literal
            else
            {
                if (LiteralsIndex.ContainsKey(quadruple.Object.PatternMemberID)
                     && LiteralsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                {
                    LiteralsIndex[quadruple.Object.PatternMemberID].Remove(quadruple.QuadrupleID);
                    if (LiteralsIndex[quadruple.Object.PatternMemberID].Count == 0)
                        LiteralsIndex.Remove(quadruple.Object.PatternMemberID);
                }
            }

            //Context (Register)
            if (!ContextsIndex.ContainsKey(quadruple.Context.PatternMemberID))
                ContextsRegister.Remove(quadruple.Context.PatternMemberID);

            //Subject (Register)
            if (!SubjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID)
                  && !PredicatesIndex.ContainsKey(quadruple.Subject.PatternMemberID)
                    && !ObjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID))
                ResourcesRegister.Remove(quadruple.Subject.PatternMemberID);

            //Predicate (Register)
            if (!SubjectsIndex.ContainsKey(quadruple.Predicate.PatternMemberID)
                  && !PredicatesIndex.ContainsKey(quadruple.Predicate.PatternMemberID)
                    && !ObjectsIndex.ContainsKey(quadruple.Predicate.PatternMemberID))
                ResourcesRegister.Remove(quadruple.Predicate.PatternMemberID);

            //Object (Register)
            if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                if (!SubjectsIndex.ContainsKey(quadruple.Object.PatternMemberID)
                      && !PredicatesIndex.ContainsKey(quadruple.Object.PatternMemberID)
                        && !ObjectsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                    ResourcesRegister.Remove(quadruple.Object.PatternMemberID);
            }

            //Literal (Register)
            else
            {
                if (!SubjectsIndex.ContainsKey(quadruple.Object.PatternMemberID)
                      && !PredicatesIndex.ContainsKey(quadruple.Object.PatternMemberID)
                        && !LiteralsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                    LiteralsRegister.Remove(quadruple.Object.PatternMemberID);
            }

            return this;
        }

        /// <summary>
        /// Clears the index
        /// </summary>
        internal void ClearIndex()
        {
            //Registers
            ContextsRegister.Clear();
            ResourcesRegister.Clear();
            LiteralsRegister.Clear();
            //Indexes
            ContextsIndex.Clear();
            SubjectsIndex.Clear();
            PredicatesIndex.Clear();
            ObjectsIndex.Clear();
            LiteralsIndex.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the quadruples indexed by the given context
        /// </summary>
        internal HashSet<long> SelectIndexByContext(RDFContext contextResource)
            => ContextsIndex.TryGetValue(contextResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given subject
        /// </summary>
        internal HashSet<long> SelectIndexBySubject(RDFResource subjectResource)
            => SubjectsIndex.TryGetValue(subjectResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given predicate
        /// </summary>
        internal HashSet<long> SelectIndexByPredicate(RDFResource predicateResource)
            => PredicatesIndex.TryGetValue(predicateResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given object
        /// </summary>
        internal HashSet<long> SelectIndexByObject(RDFResource objectResource)
            => ObjectsIndex.TryGetValue(objectResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given literal
        /// </summary>
        internal HashSet<long> SelectIndexByLiteral(RDFLiteral objectLiteral)
            => LiteralsIndex.TryGetValue(objectLiteral.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;
        #endregion

        #endregion
    }
}