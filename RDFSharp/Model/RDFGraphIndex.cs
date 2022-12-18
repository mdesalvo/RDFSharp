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
    internal class RDFGraphIndex : IDisposable
    {
        #region Properties
        /// <summary>
        /// Register of the graph's resources
        /// </summary>
        internal Dictionary<long, RDFResource> ResourcesRegister { get; set; }

        /// <summary>
        /// Register of the graph's literals
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
        /// Flag indicating that the graph index has already been disposed
        /// </summary>
        internal bool Disposed { get; set; }

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
            ResourcesRegister = new Dictionary<long, RDFResource>();
            LiteralsRegister = new Dictionary<long, RDFLiteral>();
            //Indexes
            SubjectsIndex = new Dictionary<long, HashSet<long>>();
            PredicatesIndex = new Dictionary<long, HashSet<long>>();
            ObjectsIndex = new Dictionary<long, HashSet<long>>();
            LiteralsIndex = new Dictionary<long, HashSet<long>>();
        }

        /// <summary>
        /// Destroys the graph index instance
        /// </summary>
        ~RDFGraphIndex() => Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the graph index (IDisposable)
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the graph index 
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                ClearIndex();

                //Registers
                ResourcesRegister = null;
                LiteralsRegister = null;
                //Indexes
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
        /// Adds the given triple to the SPOL index
        /// </summary>
        internal RDFGraphIndex AddIndex(RDFTriple triple)
        {
            //Subject (Register)
            if (!ResourcesRegister.ContainsKey(triple.Subject.PatternMemberID))
                ResourcesRegister.Add(triple.Subject.PatternMemberID, (RDFResource)triple.Subject);
            //Subject (Index)
            if (!SubjectsIndex.ContainsKey(triple.Subject.PatternMemberID))
                SubjectsIndex.Add(triple.Subject.PatternMemberID, new HashSet<long>() { triple.TripleID });
            else if (!SubjectsIndex[triple.Subject.PatternMemberID].Contains(triple.TripleID))
                SubjectsIndex[triple.Subject.PatternMemberID].Add(triple.TripleID);

            //Predicate (Register)
            if (!ResourcesRegister.ContainsKey(triple.Predicate.PatternMemberID))
                ResourcesRegister.Add(triple.Predicate.PatternMemberID, (RDFResource)triple.Predicate);
            //Predicate (Index)
            if (!PredicatesIndex.ContainsKey(triple.Predicate.PatternMemberID))
                PredicatesIndex.Add(triple.Predicate.PatternMemberID, new HashSet<long>() { triple.TripleID });
            else if (!PredicatesIndex[triple.Predicate.PatternMemberID].Contains(triple.TripleID))
                PredicatesIndex[triple.Predicate.PatternMemberID].Add(triple.TripleID);

            //Object
            if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                //Register
                if (!ResourcesRegister.ContainsKey(triple.Object.PatternMemberID))
                    ResourcesRegister.Add(triple.Object.PatternMemberID, (RDFResource)triple.Object);
                //Index
                if (!ObjectsIndex.ContainsKey(triple.Object.PatternMemberID))
                    ObjectsIndex.Add(triple.Object.PatternMemberID, new HashSet<long>() { triple.TripleID });
                else if (!ObjectsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                    ObjectsIndex[triple.Object.PatternMemberID].Add(triple.TripleID);
            }

            //Literal
            else
            {
                //Register
                if (!LiteralsRegister.ContainsKey(triple.Object.PatternMemberID))
                    LiteralsRegister.Add(triple.Object.PatternMemberID, (RDFLiteral)triple.Object);
                //Index
                if (!LiteralsIndex.ContainsKey(triple.Object.PatternMemberID))
                    LiteralsIndex.Add(triple.Object.PatternMemberID, new HashSet<long>() { triple.TripleID });
                else if (!LiteralsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                    LiteralsIndex[triple.Object.PatternMemberID].Add(triple.TripleID);
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
            //Subject (Index)
            if (SubjectsIndex.ContainsKey(triple.Subject.PatternMemberID)
                 && SubjectsIndex[triple.Subject.PatternMemberID].Contains(triple.TripleID))
            {
                SubjectsIndex[triple.Subject.PatternMemberID].Remove(triple.TripleID);
                if (SubjectsIndex[triple.Subject.PatternMemberID].Count == 0)
                    SubjectsIndex.Remove(triple.Subject.PatternMemberID);
            }

            //Predicate (Index)
            if (PredicatesIndex.ContainsKey(triple.Predicate.PatternMemberID)
                 && PredicatesIndex[triple.Predicate.PatternMemberID].Contains(triple.TripleID))
            {
                PredicatesIndex[triple.Predicate.PatternMemberID].Remove(triple.TripleID);
                if (PredicatesIndex[triple.Predicate.PatternMemberID].Count == 0)
                    PredicatesIndex.Remove(triple.Predicate.PatternMemberID);
            }

            //Object (Index)
            if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                if (ObjectsIndex.ContainsKey(triple.Object.PatternMemberID)
                     && ObjectsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                {
                    ObjectsIndex[triple.Object.PatternMemberID].Remove(triple.TripleID);
                    if (ObjectsIndex[triple.Object.PatternMemberID].Count == 0)
                        ObjectsIndex.Remove(triple.Object.PatternMemberID);
                }
            }

            //Literal (Index)
            else
            {
                if (LiteralsIndex.ContainsKey(triple.Object.PatternMemberID)
                     && LiteralsIndex[triple.Object.PatternMemberID].Contains(triple.TripleID))
                {
                    LiteralsIndex[triple.Object.PatternMemberID].Remove(triple.TripleID);
                    if (LiteralsIndex[triple.Object.PatternMemberID].Count == 0)
                        LiteralsIndex.Remove(triple.Object.PatternMemberID);
                }
            }

            //Subject (Register)
            if (!SubjectsIndex.ContainsKey(triple.Subject.PatternMemberID)
                  && !PredicatesIndex.ContainsKey(triple.Subject.PatternMemberID)
                    && !ObjectsIndex.ContainsKey(triple.Subject.PatternMemberID))
                ResourcesRegister.Remove(triple.Subject.PatternMemberID);

            //Predicate (Register)
            if (!SubjectsIndex.ContainsKey(triple.Predicate.PatternMemberID)
                  && !PredicatesIndex.ContainsKey(triple.Predicate.PatternMemberID)
                    && !ObjectsIndex.ContainsKey(triple.Predicate.PatternMemberID))
                ResourcesRegister.Remove(triple.Predicate.PatternMemberID);

            //Object (Register)
            if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                if (!SubjectsIndex.ContainsKey(triple.Object.PatternMemberID)
                      && !PredicatesIndex.ContainsKey(triple.Object.PatternMemberID)
                        && !ObjectsIndex.ContainsKey(triple.Object.PatternMemberID))
                ResourcesRegister.Remove(triple.Object.PatternMemberID);
            }

            //Literal (Register)
            else
            {
                if (!SubjectsIndex.ContainsKey(triple.Object.PatternMemberID)
                      && !PredicatesIndex.ContainsKey(triple.Object.PatternMemberID)
                        && !LiteralsIndex.ContainsKey(triple.Object.PatternMemberID))
                    LiteralsRegister.Remove(triple.Object.PatternMemberID);
            }

            return this;
        }

        /// <summary>
        /// Clears the index
        /// </summary>
        internal void ClearIndex()
        {
            //Registers
            ResourcesRegister.Clear();
            LiteralsRegister.Clear();
            //Indexes
            SubjectsIndex.Clear();
            PredicatesIndex.Clear();
            ObjectsIndex.Clear();
            LiteralsIndex.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the triples indexed by the given subject
        /// </summary>
        internal HashSet<long> SelectIndexBySubject(RDFResource subjectResource)
            => SubjectsIndex.TryGetValue(subjectResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the triples indexed by the given predicate
        /// </summary>
        internal HashSet<long> SelectIndexByPredicate(RDFResource predicateResource)
            => PredicatesIndex.TryGetValue(predicateResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the triples indexed by the given object
        /// </summary>
        internal HashSet<long> SelectIndexByObject(RDFResource objectResource)
            => ObjectsIndex.TryGetValue(objectResource.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;

        /// <summary>
        /// Selects the triples indexed by the given literal
        /// </summary>
        internal HashSet<long> SelectIndexByLiteral(RDFLiteral objectLiteral)
            => LiteralsIndex.TryGetValue(objectLiteral.PatternMemberID, out HashSet<long> index) ? index : EmptyHashSet;
        #endregion

        #endregion
    }
}