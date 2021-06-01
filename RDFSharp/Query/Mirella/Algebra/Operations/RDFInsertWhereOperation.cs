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
using RDFSharp.Store;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFInsertWhereOperation is the SPARQL "INSERT WHERE" operation implementation
    /// </summary>
    public class RDFInsertWhereOperation : RDFOperation
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the INSERT WHERE operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintInsertWhereOperation(this);
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty INSERT WHERE operation
        /// </summary>
        public RDFInsertWhereOperation() : base()
            => this.IsInsertWhere = true;
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the templates of the operation
        /// </summary>
        public RDFInsertWhereOperation AddInsertTemplate(RDFPattern template)
        {
            if (template != null)
            {
                if (!this.InsertTemplates.Any(tp => tp.Equals(template)))
                {
                    this.InsertTemplates.Add(template);

                    //Context
                    if (template.Context != null && template.Context is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Context)))
                            this.Variables.Add((RDFVariable)template.Context);
                    }

                    //Subject
                    if (template.Subject is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Subject)))
                            this.Variables.Add((RDFVariable)template.Subject);
                    }

                    //Predicate
                    if (template.Predicate is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Predicate)))
                            this.Variables.Add((RDFVariable)template.Predicate);
                    }

                    //Object
                    if (template.Object is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Object)))
                            this.Variables.Add((RDFVariable)template.Object);
                    }
                }
            }            
            return this;
        }

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        public RDFInsertWhereOperation AddPatternGroup(RDFPatternGroup patternGroup)
        {
            if (patternGroup != null)
            {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup)))
                    this.QueryMembers.Add(patternGroup);
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        public RDFInsertWhereOperation AddModifier(RDFDistinctModifier modifier)
        {
            if (modifier != null)
            {
                if (!this.GetModifiers().Any(m => m is RDFDistinctModifier))
                    this.QueryMembers.Add(modifier);
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFInsertWhereOperation AddPrefix(RDFNamespace prefix)
        {
            if (prefix != null)
            {
                if (!this.Prefixes.Any(p => p.Equals(prefix)))
                    this.Prefixes.Add(prefix);
            }
            return this;
        }

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        public RDFInsertWhereOperation AddSubQuery(RDFSelectQuery subQuery)
        {
            if (subQuery != null)
            {
                if (!this.GetSubQueries().Any(q => q.Equals(subQuery)))
                    this.QueryMembers.Add(subQuery.SubQuery());
            }
            return this;
        }

        /// <summary>
        /// Applies the operation to the given graph
        /// </summary>
        public override RDFOperationResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFOperationEngine().EvaluateInsertWhereOperation(this, graph)
                             : new RDFOperationResult();

        /// <summary>
        /// Asynchronously applies the operation to the given graph
        /// </summary>
        public override Task<RDFOperationResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => ApplyToGraph(graph));

        /// <summary>
        /// Applies the operation to the given store
        /// </summary>
        public override RDFOperationResult ApplyToStore(RDFStore store)
            => store != null ? new RDFOperationEngine().EvaluateInsertWhereOperation(this, store)
                             : new RDFOperationResult();

        /// <summary>
        /// Asynchronously applies the operation to the given store
        /// </summary>
        public override Task<RDFOperationResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => ApplyToStore(store));
        #endregion
    }
}