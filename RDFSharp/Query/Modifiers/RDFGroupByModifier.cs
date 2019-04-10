/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Text;
using System.Linq;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFGroupByModifier represents an aggregator modifier to be applied on a query results table.
    /// </summary>
    public class RDFGroupByModifier : RDFModifier
    {

        #region Properties
        /// <summary>
        /// List of variables on which query results are grouped
        /// </summary>
        internal List<RDFVariable> GroupByVariables { get; set; }

        /// <summary>
        /// List of functions applied on the result groups
        /// </summary>
        internal List<RDFAggregatorFunction> AggregatorFunctions { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a GroupBy modifier on the given variables
        /// </summary>
        internal RDFGroupByModifier(List<RDFVariable> groupByVariables)
        {
            if (groupByVariables != null && groupByVariables.Any())
            {
                groupByVariables.ForEach(gv1 => {
                    if (!this.GroupByVariables.Any(gv2 => gv2.Equals(gv1)))
                    {
                        this.GroupByVariables.Add(gv1);
                    }
                });
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFGroupByModifier because given \"groupByVariables\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override String ToString()
        {
            var result = new StringBuilder();
            result.Append(String.Format("GROUP BY {0}", String.Join(" ", this.GroupByVariables)));
            if (this.AggregatorFunctions.Any())
            {
                result.Append("??");
                result.Append(String.Join(" ", this.AggregatorFunctions.Select(v => v.ToString())));
            }            
            return result.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given aggregator function to the modifier
        /// </summary>
        public RDFGroupByModifier AddAggregatorFunction(RDFAggregatorFunction aggregatorFunction)
        {
            if (aggregatorFunction != null)
            {
                this.AggregatorFunctions.Add(aggregatorFunction);
            }
            return this;
        }

        /// <summary>
        /// Applies the modifier on the given datatable 
        /// </summary>
        internal override DataTable ApplyModifier(DataTable tableToFilter)
        {
            throw new NotImplementedException();
        }
        #endregion

    }

    /// <summary>
    /// RDFAggregatorFunction represents an aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFAggregatorFunction
    {

        #region Properties
        /// <summary>
        /// Flag indicating that the aggregator function considers distinct values
        /// </summary>
        public Boolean IsDistinct { get; internal set; }

        /// <summary>
        /// Type of aggregator function applied on the in-scope variable
        /// </summary>
        public RDFQueryEnums.RDFAggregatorFunctionTypes FunctionType { get; internal set; }

        /// <summary>
        /// Variable on which the aggregator function is applied
        /// </summary>
        public RDFVariable AggregatorVariable { get; internal set; }

        /// <summary>
        /// Variable used for projection of aggregator function results
        /// </summary>
        public RDFVariable ProjectionVariable { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an aggregator function of the given type on the given variable and with the given projection name
        /// </summary>
        public RDFAggregatorFunction(RDFQueryEnums.RDFAggregatorFunctionTypes functionType, RDFVariable aggregatorVariable, RDFVariable projectionVariable)
        {
            if (aggregatorVariable != null)
            {
                if (projectionVariable != null)
                {
                    this.FunctionType = functionType;
                    this.AggregatorVariable = aggregatorVariable;
                    this.ProjectionVariable = projectionVariable;
                    this.IsDistinct = false;
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFAggregatorFunction because given \"projectionVariable\" parameter is null.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFAggregatorFunction because given \"aggregatorVariable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the aggregator function 
        /// </summary>
        public override String ToString()
        {
            var result = new StringBuilder();
            var aggregationToken = (this.IsDistinct ? String.Format("DISTINCT({0})", this.AggregatorVariable) : String.Format("{0}", this.AggregatorVariable));
            switch (this.FunctionType)
            {
                case RDFQueryEnums.RDFAggregatorFunctionTypes.AVG:
                    result.Append(String.Format("(AVG({0}) AS {1})", aggregationToken, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.COUNT:
                    result.Append(String.Format("(COUNT({0}) AS {1})", aggregationToken, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.MAX:
                    result.Append(String.Format("(MAX({0}) AS {1})", aggregationToken, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.MIN:
                    result.Append(String.Format("(MIN({0}) AS {1})", aggregationToken, this.ProjectionVariable));
                    break;
                case RDFQueryEnums.RDFAggregatorFunctionTypes.SUM:
                    result.Append(String.Format("(SUM({0}) AS {1})", aggregationToken, this.ProjectionVariable));
                    break;
            }
            return result.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the aggregator function to consider distinct values on the in-scope variable
        /// </summary>
        public RDFAggregatorFunction Distinct()
        {
            this.IsDistinct = true;
            return this;
        }
        #endregion

    }
}