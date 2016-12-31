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
using RDFSharp.Model;

namespace RDFSharp.Store {

    /// <summary>
    /// RDFStoreIndex represents an automatically managed in-memory index structure for the quadruples of a store.
    /// </summary>
    internal class RDFStoreIndex {

        #region Properties
        /// <summary>
        /// Index on the contexts of the store's quadruples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Contexts { get; set; }

        /// <summary>
        /// Index on the subjects of the store's quadruples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Subjects { get; set; }

        /// <summary>
        /// Index on the predicates of the store's quadruples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Predicates { get; set; }

        /// <summary>
        /// Index on the objects of the store's quadruples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Objects { get; set; }

        /// <summary>
        /// Index on the literals of the store's quadruples
        /// </summary>
        internal Dictionary<Int64, HashSet<Int64>> Literals { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor for an empty store index
        /// </summary>
        internal RDFStoreIndex() {
            this.Contexts   = new Dictionary<Int64, HashSet<Int64>>();
            this.Subjects   = new Dictionary<Int64, HashSet<Int64>>();
            this.Predicates = new Dictionary<Int64, HashSet<Int64>>();
            this.Objects    = new Dictionary<Int64, HashSet<Int64>>();
            this.Literals   = new Dictionary<Int64, HashSet<Int64>>();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given quadruple to the CSPOL index
        /// </summary>
        internal RDFStoreIndex AddIndex(RDFQuadruple quadruple) {
            if (quadruple != null) {

                //Context
                if (!this.Contexts.ContainsKey(quadruple.Context.PatternMemberID)) {
                     this.Contexts.Add(quadruple.Context.PatternMemberID, new HashSet<Int64>() { quadruple.QuadrupleID });
                }
                else {
                     if (!this.Contexts[quadruple.Context.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                          this.Contexts[quadruple.Context.PatternMemberID].Add(quadruple.QuadrupleID);
                     }
                }

                //Subject
                if (!this.Subjects.ContainsKey(quadruple.Subject.PatternMemberID)) {
                     this.Subjects.Add(quadruple.Subject.PatternMemberID, new HashSet<Int64>() { quadruple.QuadrupleID });
                }
                else {
                     if (!this.Subjects[quadruple.Subject.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                          this.Subjects[quadruple.Subject.PatternMemberID].Add(quadruple.QuadrupleID);
                     }
                }

                //Predicate
                if (!this.Predicates.ContainsKey(quadruple.Predicate.PatternMemberID)) {
                     this.Predicates.Add(quadruple.Predicate.PatternMemberID, new HashSet<Int64>() { quadruple.QuadrupleID });
                }
                else {
                     if (!this.Predicates[quadruple.Predicate.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                          this.Predicates[quadruple.Predicate.PatternMemberID].Add(quadruple.QuadrupleID);
                     }
                }

                //Object
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    if (!this.Objects.ContainsKey(quadruple.Object.PatternMemberID)) {
                         this.Objects.Add(quadruple.Object.PatternMemberID, new HashSet<Int64>() { quadruple.QuadrupleID });
                    }
                    else {
                         if (!this.Objects[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                              this.Objects[quadruple.Object.PatternMemberID].Add(quadruple.QuadrupleID);
                         }
                    }
                }

                //Literal
                else {
                    if (!this.Literals.ContainsKey(quadruple.Object.PatternMemberID)) {
                         this.Literals.Add(quadruple.Object.PatternMemberID, new HashSet<Int64>() { quadruple.QuadrupleID });
                    }
                    else {
                         if (!this.Literals[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                              this.Literals[quadruple.Object.PatternMemberID].Add(quadruple.QuadrupleID);
                         }
                    }
                }

            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given quadruple from the CSPOL index
        /// </summary>
        internal RDFStoreIndex RemoveIndex(RDFQuadruple quadruple) {
            if (quadruple != null) {

                //Context
                if (this.Contexts.ContainsKey(quadruple.Context.PatternMemberID)) {
                    if (this.Contexts[quadruple.Context.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                        this.Contexts[quadruple.Context.PatternMemberID].Remove(quadruple.QuadrupleID);
                        if (this.Contexts[quadruple.Context.PatternMemberID].Count == 0) {
                            this.Contexts.Remove(quadruple.Context.PatternMemberID);
                        }
                    }
                }

                //Subject
                if (this.Subjects.ContainsKey(quadruple.Subject.PatternMemberID)) {
                    if (this.Subjects[quadruple.Subject.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                        this.Subjects[quadruple.Subject.PatternMemberID].Remove(quadruple.QuadrupleID);
                        if (this.Subjects[quadruple.Subject.PatternMemberID].Count == 0) {
                            this.Subjects.Remove(quadruple.Subject.PatternMemberID);
                        }
                    }
                }

                //Predicate
                if (this.Predicates.ContainsKey(quadruple.Predicate.PatternMemberID)) {
                    if (this.Predicates[quadruple.Predicate.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                        this.Predicates[quadruple.Predicate.PatternMemberID].Remove(quadruple.QuadrupleID);
                        if (this.Predicates[quadruple.Predicate.PatternMemberID].Count == 0) {
                            this.Predicates.Remove(quadruple.Predicate.PatternMemberID);
                        }
                    }
                }

                //Object
                if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                    if (this.Objects.ContainsKey(quadruple.Object.PatternMemberID)) {
                        if (this.Objects[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                            this.Objects[quadruple.Object.PatternMemberID].Remove(quadruple.QuadrupleID);
                            if (this.Objects[quadruple.Object.PatternMemberID].Count == 0) {
                                this.Objects.Remove(quadruple.Object.PatternMemberID);
                            }
                        }
                    }
                }

                //Literal
                else {
                    if (this.Literals.ContainsKey(quadruple.Object.PatternMemberID)) {
                        if (this.Literals[quadruple.Object.PatternMemberID].Contains(quadruple.QuadrupleID)) {
                            this.Literals[quadruple.Object.PatternMemberID].Remove(quadruple.QuadrupleID);
                            if (this.Literals[quadruple.Object.PatternMemberID].Count == 0) {
                                this.Literals.Remove(quadruple.Object.PatternMemberID);
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
            this.Contexts.Clear();
            this.Subjects.Clear();
            this.Predicates.Clear();
            this.Objects.Clear();
            this.Literals.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the quadruples indexed by the given context
        /// </summary>
        internal HashSet<Int64> SelectIndexByContext(RDFContext contextResource) {
            if (contextResource != null) {
                if (this.Contexts.ContainsKey(contextResource.PatternMemberID)) {
                    return this.Contexts[contextResource.PatternMemberID];
                }
            }
            return new HashSet<Int64>();
        }
        
        /// <summary>
        /// Selects the quadruples indexed by the given subject
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
        /// Selects the quadruples indexed by the given predicate
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
        /// Selects the quadruples indexed by the given object
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
        /// Selects the quadruples indexed by the given literal
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