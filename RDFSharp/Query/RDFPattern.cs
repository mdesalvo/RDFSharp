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
using RDFSharp.Store;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFPattern represents a search pattern over a collection of RDF data.
    /// </summary>
    public class RDFPattern: IEquatable<RDFPattern> {

        #region Properties
        /// <summary>
        /// Unique representation of the pattern
        /// </summary>
        internal Int64 PatternID { get; set; }

        /// <summary>
        /// Member acting as context token of the pattern
        /// </summary>
        public RDFPatternMember Context { get; internal set; }

        /// <summary>
        /// Member acting as subject token of the pattern
        /// </summary>
        public RDFPatternMember Subject { get; internal set; }

        /// <summary>
        /// Member acting as predicate token of the pattern
        /// </summary>
        public RDFPatternMember Predicate { get; internal set; }

        /// <summary>
        /// Member acting as object token of the pattern
        /// </summary>
        public RDFPatternMember Object { get; internal set; }

        #region Internals
        /// <summary>
        /// Flag indicating the pattern as Optional
        /// </summary>
        internal Boolean IsOptional { get; set; }

        /// <summary>
        /// Flag indicating the pattern to be joined as union
        /// </summary>
        internal Boolean JoinAsUnion { get; set; }

        /// <summary>
        /// List of variables carried by the pattern
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor for SPO pattern
        /// </summary>
        public RDFPattern(RDFPatternMember subject, RDFPatternMember predicate, RDFPatternMember objLit) {
            this.Variables   = new List<RDFVariable>();
            this.IsOptional  = false;
            this.JoinAsUnion = false;

            //Subject
            if (subject != null) {
                if (subject is RDFResource || subject is RDFVariable) {
                    this.Subject = subject;
                    if (subject is RDFVariable) {
                        if (!this.Variables.Exists(v => v.Equals(subject))) {
                             this.Variables.Add((RDFVariable)subject);
                        }
                    }
                }
                else {
                    throw new RDFQueryException("Cannot create RDFPattern because given \"subject\" parameter (" + subject + ") is neither a resource or a variable");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFPattern because given \"subject\" parameter is null");
            }

            //Predicate
            if (predicate != null) {
                if (predicate is RDFResource || predicate is RDFVariable) {
                    if (predicate is RDFResource && ((RDFResource)predicate).IsBlank) {
                        throw new RDFQueryException("Cannot create RDFPattern because given \"predicate\" parameter is a blank resource");
                    }
                    this.Predicate = predicate;
                    if (predicate is RDFVariable) {
                        if (!this.Variables.Exists(v => v.Equals(predicate))) {
                             this.Variables.Add((RDFVariable)predicate);
                        }
                    }
                }
                else {
                    throw new RDFQueryException("Cannot create RDFPattern because given \"predicate\" parameter (" + predicate + ") is neither a resource or a variable");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFPattern because given \"predicate\" parameter is null");
            }

            //Object/Literal
            if (objLit != null) {
                if (objLit is RDFResource || objLit is RDFLiteral || objLit is RDFVariable) {
                    this.Object    = objLit;
                    if (objLit is RDFVariable) {
                        if (!this.Variables.Exists(v => v.Equals(objLit))) {
                             this.Variables.Add((RDFVariable)objLit);
                        }
                    }
                }
                else {
                    throw new RDFQueryException("Cannot create RDFPattern because given \"objLit\" parameter (" + objLit + ") is neither a resource, or a literal or a variable");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFPattern because given \"objLit\" parameter is null");
            }

            //PatternID
            this.PatternID  = RDFModelUtilities.CreateHash(this.ToString());
        }

        /// <summary>
        /// Default ctor for CSPO pattern
        /// </summary>
        public RDFPattern(RDFPatternMember context, RDFPatternMember subject, RDFPatternMember predicate, RDFPatternMember objLit): this(subject, predicate, objLit) {
            //Context
            if (context != null) {
                if (context is RDFContext || context is RDFVariable) {
                    this.Context = context;
                    if (context is RDFVariable) {
                        if (!this.Variables.Exists(v => v.Equals(context))) {
                             this.Variables.Add((RDFVariable)context);
                        }
                    }
                }
                else {
                    throw new RDFQueryException("Cannot create RDFPattern because given \"context\" parameter (" + context + ") is neither a context or a variable");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFPattern because given \"context\" parameter is null");
            }

            //PatternID
            this.PatternID = RDFModelUtilities.CreateHash(this.ToString());
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the pattern
        /// </summary>
        public override String ToString() {
            String subj       = RDFQueryUtilities.PrintRDFPatternMember(this.Subject);
            String pred       = RDFQueryUtilities.PrintRDFPatternMember(this.Predicate);
            String obj        = RDFQueryUtilities.PrintRDFPatternMember(this.Object);

            //CSPO pattern
            if (this.Context != null) {
                String ctx    = RDFQueryUtilities.PrintRDFPatternMember(this.Context);
                if (this.IsOptional) {
                    return "OPTIONAL { GRAPH " + ctx + " { " + subj + " " + pred + " " + obj + " } }";
                }
                return "GRAPH " + ctx + " { " + subj + " " + pred + " " + obj + " }";
            }

            //SPO pattern
            if (this.IsOptional) {
                return "OPTIONAL { " + subj + " " + pred + " " + obj + " }";
            }
            return subj + " " + pred + " " + obj;
        }

        /// <summary>
        /// Performs the equality comparison between two patterns
        /// </summary>
        public Boolean Equals(RDFPattern other) {
            return (other != null && this.PatternID.Equals(other.PatternID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the pattern as Optional
        /// </summary>
        public RDFPattern Optional() {
            this.IsOptional  = true;
            this.PatternID   = RDFModelUtilities.CreateHash(this.ToString());
            return this;
        }

        /// <summary>
        /// Sets the pattern to be joined as Union with the next pattern
        /// </summary>
        public RDFPattern UnionWithNext() {
            this.JoinAsUnion = true;
            this.PatternID   = RDFModelUtilities.CreateHash(this.ToString());
            return this;
        }
        #endregion

    }

}