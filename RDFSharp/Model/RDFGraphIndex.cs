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
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFGraphIndex represents an automatically managed in-memory index structure for the triples of a graph.
    /// </summary>
    internal class RDFGraphIndex
    {
        #region Properties
        /// <summary>
        /// Register of the graph's resources
        /// </summary>
        internal Dictionary<long, RDFResource> ResourcesRegister { get; set; }

        /// <summary>
        /// Register of the graph's lietarls
        /// </summary>
        internal Dictionary<long, RDFLiteral> LiteralsRegister { get; set; }

        /// <summary>
        /// Index on the subjects of the graph's triples
        /// </summary>
        internal Dictionary<long, HashSet<long>> SubjectsIndex { get; set; }

        /// <summary>
        /// Index on the predicates of the graph's triples
        /// </summary>
        internal Dictionary<long, HashSet<long>> PredicatesIndex { get; set; }

        /// <summary>
        /// Index on the objects of the graph's triples
        /// </summary>
        internal Dictionary<long, HashSet<long>> ObjectsIndex { get; set; }

        /// <summary>
        /// Index on the literals of the graph's triples
        /// </summary>
        internal Dictionary<long, HashSet<long>> LiteralsIndex { get; set; }

        /// <summary>
        /// Empty hashset to be returned in case of index miss
        /// </summary>
        private static readonly HashSet<long> EmptyHashSet = new HashSet<long>();
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor for an empty graph index
        /// </summary>
        internal RDFGraphIndex()
        {
            //Registers
            this.ResourcesRegister = new Dictionary<long, RDFResource>();
            this.LiteralsRegister = new Dictionary<long, RDFLiteral>();
            //Indexes
            this.SubjectsIndex = new Dictionary<long, HashSet<long>>();
            this.PredicatesIndex = new Dictionary<long, HashSet<long>>();
            this.ObjectsIndex = new Dictionary<long, HashSet<long>>();
            this.LiteralsIndex = new Dictionary<long, HashSet<long>>();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given triple to the SPOL index
        /// </summary>
        internal RDFGraphIndex AddIndex(RDFTriple triple)
        {
            if (triple != null)
            {
                //Subject (Register)
                if (!this.ResourcesRegister.ContainsKey(triple.Subject.PatternMemberID))
                    this.ResourcesRegister.Add(triple.Subject.PatternMemberID, (RDFResource)triple.Subject);
                //Subject (Index)
                if (!this.SubjectsIndex.ContainsKey(triple.Subject.PatternMemberID))
                    this.SubjectsIndex.Add(triple.Subject.PatternMemberID, new HashSet<long>() { triple.TripleID });
                else if (!this.SubjectsIndex[triple.Subject.PatternMemberID].Contains(triple.TripleID))
                    this.SubjectsIndex[triple.Subject.PatternMemberID].Add(triple.TripleID);

                //Predicate (Register)
                if (!this.ResourcesRegister.ContainsKey(triple.Predicate.PatternMemberID))
                    this.ResourcesRegister.Add(triple.Predicate.PatternMemberID, (RDFResource)triple.Predicate);
                //Predicate (Index)
                if (!this.PredicatesIndex.ContainsKey(triple.Predicate.PatternMemberID))
                    this.PredicatesIndex.Add(triple.Predicate.PatternMemberID, new HashSet<long>() { triple.TripleID });
                else if (!this.PredicatesIndex[triple.Predicate.PatternMemberID].Contains(triple.TripleID))
                    this.PredicatesIndex[triple.Predicate.PatternMemberID].Add(triple.TripleID);

                //Object
                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    //Register
                    if (!this.ResourcesRegister.ContainsKey(triple.Object.PatternMemberID))
                        this.ResourcesRegister.Add(triple.Object.PatternMemberID, (RDFResource)triple.Object);
                    //Index
                    if (!this.ObjectsIndex.ContainsKey(triple.Object.PatternMemberID))
                        this.ObjectsIndex.Add(triple.Object.PatternMemberID, new HashSet<long>() { triple.TripleID });
                    else if (!this.ObjectsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                        this.ObjectsIndex[triple.Object.PatternMemberID].Add(triple.TripleID);
                }

                //Literal
                else
                {
                    //Register
                    if (!this.LiteralsRegister.ContainsKey(triple.Object.PatternMemberID))
                        this.LiteralsRegister.Add(triple.Object.PatternMemberID, (RDFLiteral)triple.Object);
                    //Index
                    if (!this.LiteralsIndex.ContainsKey(triple.Object.PatternMemberID))
                        this.LiteralsIndex.Add(triple.Object.PatternMemberID, new HashSet<long>() { triple.TripleID });
                    else if (!this.LiteralsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                        this.LiteralsIndex[triple.Object.PatternMemberID].Add(triple.TripleID);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given triple from the SPOL index
        /// </summary>
        internal RDFGraphIndex RemoveIndex(RDFTriple triple)
        {
            if (triple != null)
            {
                //Subject (Index)
                if (this.SubjectsIndex.ContainsKey(triple.Subject.PatternMemberID)
                      && this.SubjectsIndex[triple.Subject.PatternMemberID].Contains(triple.TripleID))
                {
                    this.SubjectsIndex[triple.Subject.PatternMemberID].Remove(triple.TripleID);
                    if (this.SubjectsIndex[triple.Subject.PatternMemberID].Count == 0)
                        this.SubjectsIndex.Remove(triple.Subject.PatternMemberID);
                }

                //Predicate (Index)
                if (this.PredicatesIndex.ContainsKey(triple.Predicate.PatternMemberID)
                      && this.PredicatesIndex[triple.Predicate.PatternMemberID].Contains(triple.TripleID))
                {
                    this.PredicatesIndex[triple.Predicate.PatternMemberID].Remove(triple.TripleID);
                    if (this.PredicatesIndex[triple.Predicate.PatternMemberID].Count == 0)
                        this.PredicatesIndex.Remove(triple.Predicate.PatternMemberID);
                }

                //Object (Index)
                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    if (this.ObjectsIndex.ContainsKey(triple.Object.PatternMemberID)
                          && this.ObjectsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                    {
                        this.ObjectsIndex[triple.Object.PatternMemberID].Remove(triple.TripleID);
                        if (this.ObjectsIndex[triple.Object.PatternMemberID].Count == 0)
                            this.ObjectsIndex.Remove(triple.Object.PatternMemberID);
                    }
                }

                //Literal (Index)
                else
                {
                    if (this.LiteralsIndex.ContainsKey(triple.Object.PatternMemberID)
                          && this.LiteralsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                    {
                        this.LiteralsIndex[triple.Object.PatternMemberID].Remove(triple.TripleID);
                        if (this.LiteralsIndex[triple.Object.PatternMemberID].Count == 0)
                            this.LiteralsIndex.Remove(triple.Object.PatternMemberID);
                    }
                }

                //Subject (Register)
                if (!this.SubjectsIndex.ContainsKey(triple.Subject.PatternMemberID)
                      && !this.PredicatesIndex.ContainsKey(triple.Subject.PatternMemberID)
                        && !this.ObjectsIndex.ContainsKey(triple.Subject.PatternMemberID))
                    this.ResourcesRegister.Remove(triple.Subject.PatternMemberID);

                //Predicate (Register)
                if (!this.SubjectsIndex.ContainsKey(triple.Predicate.PatternMemberID)
                      && !this.PredicatesIndex.ContainsKey(triple.Predicate.PatternMemberID)
                        && !this.ObjectsIndex.ContainsKey(triple.Predicate.PatternMemberID))
                    this.ResourcesRegister.Remove(triple.Predicate.PatternMemberID);

                //Object (Register)
                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    if (!this.SubjectsIndex.ContainsKey(triple.Object.PatternMemberID)
                          && !this.PredicatesIndex.ContainsKey(triple.Object.PatternMemberID)
                            && !this.ObjectsIndex.ContainsKey(triple.Object.PatternMemberID))
                    this.ResourcesRegister.Remove(triple.Object.PatternMemberID);
                }

                //Literal (Register)
                else
                {
                    if (!this.SubjectsIndex.ContainsKey(triple.Object.PatternMemberID)
                          && !this.PredicatesIndex.ContainsKey(triple.Object.PatternMemberID)
                            && !this.ObjectsIndex.ContainsKey(triple.Object.PatternMemberID))
                        this.LiteralsRegister.Remove(triple.Object.PatternMemberID);
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
            this.ResourcesRegister.Clear();
            this.LiteralsRegister.Clear();
            //Indexes
            this.SubjectsIndex.Clear();
            this.PredicatesIndex.Clear();
            this.ObjectsIndex.Clear();
            this.LiteralsIndex.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the triples indexed by the given subject
        /// </summary>
        internal HashSet<long> SelectIndexBySubject(RDFResource subjectResource)
            => subjectResource != null && this.SubjectsIndex.ContainsKey(subjectResource.PatternMemberID)
                ? this.SubjectsIndex[subjectResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the triples indexed by the given predicate
        /// </summary>
        internal HashSet<long> SelectIndexByPredicate(RDFResource predicateResource)
            => predicateResource != null && this.PredicatesIndex.ContainsKey(predicateResource.PatternMemberID)
                ? this.PredicatesIndex[predicateResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the triples indexed by the given object
        /// </summary>
        internal HashSet<long> SelectIndexByObject(RDFResource objectResource)
            => objectResource != null && this.ObjectsIndex.ContainsKey(objectResource.PatternMemberID)
                ? this.ObjectsIndex[objectResource.PatternMemberID] : EmptyHashSet;

        /// <summary>
        /// Selects the triples indexed by the given literal
        /// </summary>
        internal HashSet<long> SelectIndexByLiteral(RDFLiteral objectLiteral)
            => objectLiteral != null && this.LiteralsIndex.ContainsKey(objectLiteral.PatternMemberID)
                ? this.LiteralsIndex[objectLiteral.PatternMemberID] : EmptyHashSet;
        #endregion

        #endregion
    }
}