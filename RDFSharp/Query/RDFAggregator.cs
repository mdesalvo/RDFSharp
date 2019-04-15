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
using System.Data;
using System.Text;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAggregator represents an aggregation function applied by a GroupBy modifier
    /// </summary>
    public abstract class RDFAggregator
    {

        #region Properties
        /// <summary>
        /// Variable on which the aggregator is applied
        /// </summary>
        public RDFVariable AggregatorVariable { get; internal set; }

        /// <summary>
        /// Variable used for projection of aggregator results
        /// </summary>
        public RDFVariable ProjectionVariable { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an aggregator on the given variable and with the given projection name
        /// </summary>
        internal RDFAggregator(RDFVariable aggregatorVariable, RDFVariable projectionVariable)
        {
            if (aggregatorVariable != null)
            {
                if (projectionVariable != null)
                {
                    this.AggregatorVariable = aggregatorVariable;
                    this.ProjectionVariable = projectionVariable;
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFAggregator because given \"projectionVariable\" parameter is null.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFAggregator because given \"aggregatorVariable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the aggregator function 
        /// </summary>
        public override String ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the aggregator function on the given tablerow
        /// </summary>
        internal abstract void ExecuteAggregatorFunction(Dictionary<String, DataTable> groupingRegistry, String groupingKey, DataRow tableRow);
        #endregion

    }

}