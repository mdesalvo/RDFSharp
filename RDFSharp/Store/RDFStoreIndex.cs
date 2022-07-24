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
using System.Collections.Generic;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFStoreIndex represents an automatically managed in-memory index structure for the quadruples of a store.
    /// </summary>
    internal class RDFStoreIndex
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
            this.ContextsRegister = new Dictionary<long, RDFContext>();
            this.ResourcesRegister = new Dictionary<long, RDFResource>();
            this.LiteralsRegister = new Dictionary<long, RDFLiteral>();
            //Indexes
            this.ContextsIndex = new Dictionary<long, HashSet<long>>();
            this.SubjectsIndex = new Dictionary<long, HashSet<long>>();
            this.PredicatesIndex = new Dictionary<long, HashSet<long>>();
            this.ObjectsIndex = new Dictionary<long, HashSet<long>>();
            this.LiteralsIndex = new Dictionary<long, HashSet<long>>();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given quadruple to the CSPOL index
        /// </summary>
        internal RDFStoreIndex AddIndex(RDFQuadruple quadruple)
        {
            if (quadruple != null)
            {
                //Context (Register)
                if (!this.ContextsRegister.ContainsKey(quadruple.Context.PatternMemberID))
                    this.ContextsRegister.Add(quadruple.Context.PatternMemberID, (RDFContext)quadruple.Context);
                //Context (Index)
                if (!this.ContextsIndex.ContainsKey(quadruple.Context.PatternMemberID))
                    this.ContextsIndex.Add(quadruple.Context.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                else if (!this.ContextsIndex[quadruple.Context.PatternMemberID].Contains(quadruple.QuadrupleID))
                    this.ContextsIndex[quadruple.Context.PatternMemberID].Add(quadruple.QuadrupleID);

                //Subject (Register)
                if (!this.ResourcesRegister.ContainsKey(quadruple.Subject.PatternMemberID))
                    this.ResourcesRegister.Add(quadruple.Subject.PatternMemberID, (RDFResource)quadruple.Subject);
                //Subject (Index)
                if (!this.SubjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID))
                    this.SubjectsIndex.Add(quadruple.Subject.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                else if (!this.SubjectsIndex[quadruple.Subject.PatternMemberID].Contains(quadruple.QuadrupleID))
                    this.SubjectsIndex[quadruple.Subject.PatternMemberID].Add(quadruple.QuadrupleID);

                //Predicate (Register)
                if (!this.ResourcesRegister.ContainsKey(quadruple.Predicate.PatternMemberID))
                    this.ResourcesRegister.Add(quadruple.Predicate.PatternMemberID, (RDFResource)quadruple.Predicate);
                //Predicate (Index)
                if (!this.PredicatesIndex.ContainsKey(quadruple.Predicate.PatternMemberID))
                    this.PredicatesIndex.Add(quadruple.Predicate.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                else if (!this.PredicatesIndex[quadruple.Predicate.PatternMemberID].Contains(quadruple.QuadrupleID))
                    this.PredicatesIndex[quadruple.Predicate.PatternMemberID].Add(quadruple.QuadrupleID);

                //Object
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    //Register
                    if (!this.ResourcesRegister.ContainsKey(quadruple.Object.PatternMemberID))
                        this.ResourcesRegister.Add(quadruple.Object.PatternMemberID, (RDFResource)quadruple.Object);
                    //Index
                    if (!this.ObjectsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                        this.ObjectsIndex.Add(quadruple.Object.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                    else if (!this.ObjectsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                        this.ObjectsIndex[quadruple.Object.PatternMemberID].Add(quadruple.QuadrupleID);
                }

                //Literal
                else
                {
                    //Register
                    if (!this.LiteralsRegister.ContainsKey(quadruple.Object.PatternMemberID))
                        this.LiteralsRegister.Add(quadruple.Object.PatternMemberID, (RDFLiteral)quadruple.Object);
                    //Index
                    if (!this.LiteralsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                        this.LiteralsIndex.Add(quadruple.Object.PatternMemberID, new HashSet<long>() { quadruple.QuadrupleID });
                    else if (!this.LiteralsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                        this.LiteralsIndex[quadruple.Object.PatternMemberID].Add(quadruple.QuadrupleID);
                }
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
            if (quadruple != null)
            {
                //Context
                if (this.ContextsIndex.ContainsKey(quadruple.Context.PatternMemberID))
                {
                    if (this.ContextsIndex[quadruple.Context.PatternMemberID].Contains(quadruple.QuadrupleID))
                    {
                        this.ContextsIndex[quadruple.Context.PatternMemberID].Remove(quadruple.QuadrupleID);
                        if (this.ContextsIndex[quadruple.Context.PatternMemberID].Count == 0)
                            this.ContextsIndex.Remove(quadruple.Context.PatternMemberID);
                    }
                }

                //Subject
                if (this.SubjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID))
                {
                    if (this.SubjectsIndex[quadruple.Subject.PatternMemberID].Contains(quadruple.QuadrupleID))
                    {
                        this.SubjectsIndex[quadruple.Subject.PatternMemberID].Remove(quadruple.QuadrupleID);
                        if (this.SubjectsIndex[quadruple.Subject.PatternMemberID].Count == 0)
                            this.SubjectsIndex.Remove(quadruple.Subject.PatternMemberID);
                    }
                }

                //Predicate
                if (this.PredicatesIndex.ContainsKey(quadruple.Predicate.PatternMemberID))
                {
                    if (this.PredicatesIndex[quadruple.Predicate.PatternMemberID].Contains(quadruple.QuadrupleID))
                    {
                        this.PredicatesIndex[quadruple.Predicate.PatternMemberID].Remove(quadruple.QuadrupleID);
                        if (this.PredicatesIndex[quadruple.Predicate.PatternMemberID].Count == 0)
                            this.PredicatesIndex.Remove(quadruple.Predicate.PatternMemberID);
                    }
                }

                //Object
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    if (this.ObjectsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                    {
                        if (this.ObjectsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                        {
                            this.ObjectsIndex[quadruple.Object.PatternMemberID].Remove(quadruple.QuadrupleID);
                            if (this.ObjectsIndex[quadruple.Object.PatternMemberID].Count == 0)
                                this.ObjectsIndex.Remove(quadruple.Object.PatternMemberID);
                        }
                    }
                }

                //Literal
                else
                {
                    if (this.LiteralsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                    {
                        if (this.LiteralsIndex[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID))
                        {
                            this.LiteralsIndex[quadruple.Object.PatternMemberID].Remove(quadruple.QuadrupleID);
                            if (this.LiteralsIndex[quadruple.Object.PatternMemberID].Count == 0)
                                this.LiteralsIndex.Remove(quadruple.Object.PatternMemberID);
                        }
                    }
                }

                //Context (Register)
                if (!this.ContextsIndex.ContainsKey(quadruple.Context.PatternMemberID))
                    this.ContextsRegister.Remove(quadruple.Context.PatternMemberID);

                //Subject (Register)
                if (!this.SubjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID)
                      && !this.PredicatesIndex.ContainsKey(quadruple.Subject.PatternMemberID)
                        && !this.ObjectsIndex.ContainsKey(quadruple.Subject.PatternMemberID))
                    this.ResourcesRegister.Remove(quadruple.Subject.PatternMemberID);

                //Predicate (Register)
                if (!this.SubjectsIndex.ContainsKey(quadruple.Predicate.PatternMemberID)
                      && !this.PredicatesIndex.ContainsKey(quadruple.Predicate.PatternMemberID)
                        && !this.ObjectsIndex.ContainsKey(quadruple.Predicate.PatternMemberID))
                    this.ResourcesRegister.Remove(quadruple.Predicate.PatternMemberID);

                //Object (Register)
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    if (!this.SubjectsIndex.ContainsKey(quadruple.Object.PatternMemberID)
                          && !this.PredicatesIndex.ContainsKey(quadruple.Object.PatternMemberID)
                            && !this.ObjectsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                        this.ResourcesRegister.Remove(quadruple.Object.PatternMemberID);
                }

                //Literal (Register)
                else
                {
                    if (!this.SubjectsIndex.ContainsKey(quadruple.Object.PatternMemberID)
                          && !this.PredicatesIndex.ContainsKey(quadruple.Object.PatternMemberID)
                            && !this.LiteralsIndex.ContainsKey(quadruple.Object.PatternMemberID))
                        this.LiteralsRegister.Remove(quadruple.Object.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Clears the index
        /// </summary>
        internal void ClearIndex()
        {
            //Registers
            this.ContextsRegister.Clear();
            this.ResourcesRegister.Clear();
            this.LiteralsRegister.Clear();
            //Indexes
            this.ContextsIndex.Clear();
            this.SubjectsIndex.Clear();
            this.PredicatesIndex.Clear();
            this.ObjectsIndex.Clear();
            this.LiteralsIndex.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the quadruples indexed by the given context
        /// </summary>
        internal HashSet<long> SelectIndexByContext(RDFContext contextResource)
            => contextResource != null && this.ContextsIndex.ContainsKey(contextResource.PatternMemberID)
                ? this.ContextsIndex[contextResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given subject
        /// </summary>
        internal HashSet<long> SelectIndexBySubject(RDFResource subjectResource)
            => subjectResource != null && this.SubjectsIndex.ContainsKey(subjectResource.PatternMemberID)
                ? this.SubjectsIndex[subjectResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given predicate
        /// </summary>
        internal HashSet<long> SelectIndexByPredicate(RDFResource predicateResource)
            => predicateResource != null && this.PredicatesIndex.ContainsKey(predicateResource.PatternMemberID)
                ? this.PredicatesIndex[predicateResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given object
        /// </summary>
        internal HashSet<long> SelectIndexByObject(RDFResource objectResource)
            => objectResource != null && this.ObjectsIndex.ContainsKey(objectResource.PatternMemberID)
                ? this.ObjectsIndex[objectResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the quadruples indexed by the given literal
        /// </summary>
        internal HashSet<long> SelectIndexByLiteral(RDFLiteral objectLiteral)
            => objectLiteral != null && this.LiteralsIndex.ContainsKey(objectLiteral.PatternMemberID)
                ? this.LiteralsIndex[objectLiteral.PatternMemberID] : EmptyHashSet;
        #endregion

        #endregion
    }
}