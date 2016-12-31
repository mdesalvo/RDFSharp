/*
   Copyright 2012-2017 Marco De Salvo

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

namespace RDFSharp.Model {

    /// <summary>
    /// RDFGraphIndex represents an automatically managed in-memory index structure for the triples of a graph.
    /// </summary>
    internal class RDFGraphIndex {

        #region Properties
        /// <summary>
        /// Index on the subjects of the graph's triples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Subjects { get; set; }

        /// <summary>
        /// Index on the predicates of the graph's triples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Predicates { get; set; }

        /// <summary>
        /// Index on the objects of the graph's triples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Objects { get; set; }

        /// <summary>
        /// Index on the literals of the graph's triples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Literals { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor for an empty graph index
        /// </summary>
        internal RDFGraphIndex() {
            this.Subjects   = new Dictionary<Int64, HashSet<Int64>>();
            this.Predicates = new Dictionary<Int64, HashSet<Int64>>();
            this.Objects    = new Dictionary<Int64, HashSet<Int64>>();
            this.Literals   = new Dictionary<Int64, HashSet<Int64>>();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given triple to the SPOL index
        /// </summary>
        internal RDFGraphIndex AddIndex(RDFTriple triple) {
            if (triple != null) {

                //Subject
                if (!this.Subjects.ContainsKey(triple.Subject.PatternMemberID)) {
                     this.Subjects.Add(triple.Subject.PatternMemberID, new HashSet<Int64>() { triple.TripleID });
                }
                else {
                     if (!this.Subjects[triple.Subject.PatternMemberID].Contains(triple.TripleID)) {
                          this.Subjects[triple.Subject.PatternMemberID].Add(triple.TripleID);
                     }
                }

                //Predicate
                if (!this.Predicates.ContainsKey(triple.Predicate.PatternMemberID)) {
                     this.Predicates.Add(triple.Predicate.PatternMemberID, new HashSet<Int64>() { triple.TripleID });
                }
                else {
                     if (!this.Predicates[triple.Predicate.PatternMemberID].Contains(triple.TripleID)) {
                          this.Predicates[triple.Predicate.PatternMemberID].Add(triple.TripleID);
                     }
                }

                //Object
                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    if (!this.Objects.ContainsKey(triple.Object.PatternMemberID)) {
                         this.Objects.Add(triple.Object.PatternMemberID, new HashSet<Int64>() { triple.TripleID });
                    }
                    else {
                         if (!this.Objects[triple.Object.PatternMemberID].Contains(triple.TripleID)) {
                              this.Objects[triple.Object.PatternMemberID].Add(triple.TripleID);
                         }
                    }
                }

                //Literal
                else {
                     if (!this.Literals.ContainsKey(triple.Object.PatternMemberID)) {
                          this.Literals.Add(triple.Object.PatternMemberID, new HashSet<Int64>() { triple.TripleID });
                     }
                     else {
                          if (!this.Literals[triple.Object.PatternMemberID].Contains(triple.TripleID)) {
                               this.Literals[triple.Object.PatternMemberID].Add(triple.TripleID);
                          }
                     }
                }

            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given triple from the SPOL index
        /// </summary>
        internal RDFGraphIndex RemoveIndex(RDFTriple triple) {
            if (triple != null) {

                //Subject
                if (this.Subjects.ContainsKey(triple.Subject.PatternMemberID)) {
                    if (this.Subjects[triple.Subject.PatternMemberID].Contains(triple.TripleID)) {
                        this.Subjects[triple.Subject.PatternMemberID].Remove(triple.TripleID);
                        if (this.Subjects[triple.Subject.PatternMemberID].Count == 0) {
                            this.Subjects.Remove(triple.Subject.PatternMemberID);
                        }
                    }
                }

                //Predicate
                if (this.Predicates.ContainsKey(triple.Predicate.PatternMemberID)) {
                    if (this.Predicates[triple.Predicate.PatternMemberID].Contains(triple.TripleID)) {
                        this.Predicates[triple.Predicate.PatternMemberID].Remove(triple.TripleID);
                        if (this.Predicates[triple.Predicate.PatternMemberID].Count == 0) {
                            this.Predicates.Remove(triple.Predicate.PatternMemberID);
                        }
                    }
                }

                //Object
                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    if (this.Objects.ContainsKey(triple.Object.PatternMemberID)) {
                        if (this.Objects[triple.Object.PatternMemberID].Contains(triple.TripleID)) {
                            this.Objects[triple.Object.PatternMemberID].Remove(triple.TripleID);
                            if (this.Objects[triple.Object.PatternMemberID].Count == 0) {
                                this.Objects.Remove(triple.Object.PatternMemberID);
                            }
                        }
                    }
                }

                //Literal
                else {
                     if (this.Literals.ContainsKey(triple.Object.PatternMemberID)) {
                         if (this.Literals[triple.Object.PatternMemberID].Contains(triple.TripleID)) {
                             this.Literals[triple.Object.PatternMemberID].Remove(triple.TripleID);
                             if (this.Literals[triple.Object.PatternMemberID].Count == 0) {
                                 this.Literals.Remove(triple.Object.PatternMemberID);
                             }
                         }
                     }
                }

            }
            return this;
        }

        /// <summary>
        /// Clears the index
        /// </summary>
        internal void ClearIndex() {
            this.Subjects.Clear();
            this.Predicates.Clear();
            this.Objects.Clear();
            this.Literals.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the triples indexed by the given subject
        /// </summary>
        internal HashSet<Int64> SelectIndexBySubject(RDFResource subjectResource) {
            if (subjectResource != null) {
                if (this.Subjects.ContainsKey(subjectResource.PatternMemberID)) {
                    return this.Subjects[subjectResource.PatternMemberID];
                }
            }
            return new HashSet<Int64>();
        }

        /// <summary>
        /// Selects the triples indexed by the given predicate
        /// </summary>
        internal HashSet<Int64> SelectIndexByPredicate(RDFResource predicateResource) {
            if (predicateResource != null) {
                if (this.Predicates.ContainsKey(predicateResource.PatternMemberID)) {
                    return this.Predicates[predicateResource.PatternMemberID];
                }
            }
            return new HashSet<Int64>();
        }

        /// <summary>
        /// Selects the triples indexed by the given object
        /// </summary>
        internal HashSet<Int64> SelectIndexByObject(RDFResource objectResource) {
            if (objectResource != null) {
                if (this.Objects.ContainsKey(objectResource.PatternMemberID)) {
                    return this.Objects[objectResource.PatternMemberID];
                }
            }
            return new HashSet<Int64>();
        }

        /// <summary>
        /// Selects the triples indexed by the given literal
        /// </summary>
        internal HashSet<Int64> SelectIndexByLiteral(RDFLiteral objectLiteral) {
            if (objectLiteral   != null) {
                if (this.Literals.ContainsKey(objectLiteral.PatternMemberID)) {
                    return this.Literals[objectLiteral.PatternMemberID];
                }
            }
            return new HashSet<Int64>();
        }
        #endregion

        #endregion

    }

}