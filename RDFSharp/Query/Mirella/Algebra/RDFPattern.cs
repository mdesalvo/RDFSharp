﻿/*
   Copyright 2012-2023 Marco De Salvo

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
using RDFSharp.Store;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFPattern represents a search pattern over a collection of RDF data.
    /// </summary>
    public class RDFPattern : RDFPatternGroupMember
    {
        #region Properties
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

        /// <summary>
        /// Flag indicating the pattern as optional
        /// </summary>
        internal bool IsOptional { get; set; }

        /// <summary>
        /// Flag indicating the pattern to be joined as union
        /// </summary>
        internal bool JoinAsUnion { get; set; }

        /// <summary>
        /// List of variables carried by the pattern
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor for SPO pattern
        /// </summary>
        public RDFPattern(RDFPatternMember subject, RDFPatternMember predicate, RDFPatternMember objLit)
        {
            #region Guards
            if (subject == null)
                throw new RDFQueryException("Cannot create RDFPattern because given \"subject\" parameter is null");
            if (!(subject is RDFResource || subject is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPattern because given \"subject\" parameter (" + subject + ") is neither a resource or a variable");
            if (predicate == null)
                throw new RDFQueryException("Cannot create RDFPattern because given \"predicate\" parameter is null");
            if (!(predicate is RDFResource || predicate is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPattern because given \"predicate\" parameter (" + predicate + ") is neither a resource or a variable");
            if (predicate is RDFResource && ((RDFResource)predicate).IsBlank)
                throw new RDFQueryException("Cannot create RDFPattern because given \"predicate\" parameter is a blank resource");
            if (objLit == null)
                throw new RDFQueryException("Cannot create RDFPattern because given \"objLit\" parameter is null");
            if (!(objLit is RDFResource || objLit is RDFLiteral || objLit is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPattern because given \"objLit\" parameter (" + objLit + ") is neither a resource, or a literal or a variable");
            #endregion

            Variables = new List<RDFVariable>();
            IsEvaluable = true;
            IsOptional = false;
            JoinAsUnion = false;
            
            //Subject
            Subject = subject;
            if (subject is RDFVariable subjVar && !Variables.Any(v => v.Equals(subjVar)))
                Variables.Add(subjVar);

            //Predicate
            Predicate = predicate;
            if (predicate is RDFVariable predVar && !Variables.Any(v => v.Equals(predVar)))
                Variables.Add(predVar);

            //Object/Literal
            Object = objLit;
            if (objLit is RDFVariable objVar && !Variables.Any(v => v.Equals(objVar)))
                Variables.Add(objVar);
        }

        /// <summary>
        /// Default ctor for CSPO pattern
        /// </summary>
        public RDFPattern(RDFPatternMember context, RDFPatternMember subject, RDFPatternMember predicate, RDFPatternMember objLit) : this(subject, predicate, objLit)
        {
            #region Guards
            if (context == null)
                throw new RDFQueryException("Cannot create RDFPattern because given \"context\" parameter is null");
            if (!(context is RDFContext || context is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPattern because given \"context\" parameter (" + context + ") is neither a context or a variable");
            #endregion

            //Context
            Context = context;
            if (context is RDFVariable ctxVar && !Variables.Any(v => v.Equals(ctxVar)))
                Variables.Add(ctxVar);
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the pattern
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintPattern(this, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Sets the pattern to be joined as optional with the previous pattern
        /// </summary>
        public RDFPattern Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the pattern to be joined as union with the next pattern
        /// </summary>
        public RDFPattern UnionWithNext()
        {
            JoinAsUnion = true;
            return this;
        }
        #endregion
    }
}