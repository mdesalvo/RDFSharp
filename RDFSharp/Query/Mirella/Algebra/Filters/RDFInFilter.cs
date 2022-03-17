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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFInFilter represents a filter checking if a RDF term is found in a given set of RDF terms.
    /// </summary>
    public class RDFInFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// RDF term to be searched
        /// </summary>
        public RDFPatternMember TermToSearch { get; internal set; }

        /// <summary>
        /// List of RDF terms in which the term should be searched
        /// </summary>
        internal List<RDFPatternMember> InTerms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given term and given search list
        /// </summary>
        public RDFInFilter(RDFPatternMember termToSearch, List<RDFPatternMember> inTerms)
        {
            if (termToSearch == null)
                throw new RDFQueryException("Cannot create RDFInFilter because given \"termToSearch\" parameter is null.");

            this.TermToSearch = termToSearch;
            this.InTerms = inTerms ?? new List<RDFPatternMember>();
            //Do not accept null values in input list
            this.InTerms.RemoveAll(t => t == null);
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat(
                "FILTER ( ",
                RDFQueryPrinter.PrintPatternMember(this.TermToSearch, prefixes),
                " IN (",
                string.Join(", ", this.InTerms.Select(t => RDFQueryPrinter.PrintPatternMember(t, prefixes))),
                ") )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = false;

            //IN filter is equivalent to an OR-chain of equality comparison filters
            IEnumerator<RDFPatternMember> inTermsEnumerator = this.InTerms.GetEnumerator();
            while (inTermsEnumerator.MoveNext())
            {
                RDFComparisonFilter compFilter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.EqualTo, this.TermToSearch, inTermsEnumerator.Current);
                keepRow = compFilter.ApplyFilter(row, false);
                if (keepRow)
                    break;
            }

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}