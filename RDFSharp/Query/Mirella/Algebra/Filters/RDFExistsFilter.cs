/*
   Copyright 2012-2026 Marco De Salvo

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

using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFExistsFilter represents a filter for checking existence of given RDF pattern.
    /// </summary>
    public class RDFExistsFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Pattern to be evaluated
        /// </summary>
        public RDFPattern Pattern { get; internal set; }

        /// <summary>
        /// Results of the pattern evaluation on the RDF data source
        /// </summary>
        internal RDFTable PatternResults { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an Exists filter on the given pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExistsFilter(RDFPattern pattern)
        {
            #region Guards
            if (pattern == null)
                throw new RDFQueryException("Cannot create RDFExistsFilter because given \"pattern\" parameter is null.");
            if (pattern.Variables.Count == 0)
                throw new RDFQueryException("Cannot create RDFExistsFilter because given \"pattern\" parameter is a ground pattern.");
            #endregion

            Pattern = pattern;
            IsEvaluable = true;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( EXISTS { ", Pattern.ToString(prefixes), " } )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the pattern in the given datarow
        /// </summary>
        internal override bool ApplyFilter(RDFTableRow row, bool applyNegation)
        {
            bool keepRow = false;
            string subjectString = Pattern.Subject.ToString();
            string predicateString = Pattern.Predicate.ToString();
            string objectString = Pattern.Object.ToString();

            #region Disjoint Evaluation
            //In case of disjointess between the query and the filter's pattern, all solutions are compatible
            bool disjointSubject = !(Pattern.Subject is RDFVariable) || !row.HasColumn(subjectString);
            bool disjointPredicate = !(Pattern.Predicate is RDFVariable) || !row.HasColumn(predicateString);
            bool disjointObject = !(Pattern.Object is RDFVariable) || !row.HasColumn(objectString);
            if (disjointSubject && disjointPredicate && disjointObject)
                keepRow = true;
            #endregion

            #region Non-Disjoint Evaluation
            else
            {
                IEnumerable<RDFTableRow> patternResultsEnumerable = PatternResults != null ? (IEnumerable<RDFTableRow>)PatternResults.Rows : null;
                if (patternResultsEnumerable?.Any() ?? false)
                {
                    #region Subject
                    bool subjectCompared = false;
                    if (Pattern.Subject is RDFVariable
                         && PatternResults.HasColumn(subjectString)
                         && row.HasColumn(subjectString))
                    {
                        //In case of emptiness the solution is compatible, otherwise proceed with comparison
                        if (!row.IsUnbound(subjectString))
                        {
                            //Get subject filter's value for the given row
                            RDFPatternMember rowMember = RDFQueryUtilities.ParseRDFPatternMember((row[subjectString] ?? string.Empty));

                            //Apply subject filter on the pattern resultset
                            patternResultsEnumerable = patternResultsEnumerable.Where(x => RDFQueryUtilities.ParseRDFPatternMember(x[subjectString]).Equals(rowMember));
                        }
                        subjectCompared = true;
                    }
                    #endregion

                    #region Predicate
                    bool predicateCompared = false;
                    if (Pattern.Predicate is RDFVariable
                         && PatternResults.HasColumn(predicateString)
                         && row.HasColumn(predicateString))
                    {
                        //In case of emptiness the solution is compatible, otherwise proceed with comparison
                        if (!row.IsUnbound(predicateString))
                        {
                            //Get predicate filter's value for the given row
                            RDFPatternMember rowMember = RDFQueryUtilities.ParseRDFPatternMember((row[predicateString] ?? string.Empty));

                            //Apply predicate filter on the pattern resultset
                            patternResultsEnumerable = patternResultsEnumerable.Where(x => RDFQueryUtilities.ParseRDFPatternMember(x[predicateString]).Equals(rowMember));
                        }
                        predicateCompared = true;
                    }
                    #endregion

                    #region Object
                    bool objectCompared = false;
                    if (Pattern.Object is RDFVariable
                         && PatternResults.HasColumn(objectString)
                         && row.HasColumn(objectString))
                    {
                        //In case of emptiness the solution is compatible, otherwise proceed with comparison
                        if (!row.IsUnbound(objectString))
                        {
                            //Get object filter's value for the given row
                            RDFPatternMember rowMember = RDFQueryUtilities.ParseRDFPatternMember((row[objectString] ?? string.Empty));

                            //Apply object filter on the pattern resultset
                            patternResultsEnumerable = patternResultsEnumerable.Where(x => RDFQueryUtilities.ParseRDFPatternMember(x[objectString]).Equals(rowMember));
                        }
                        objectCompared = true;
                    }
                    #endregion

                    //Verify filter's response on the pattern resultset
                    if ((subjectCompared || predicateCompared || objectCompared) && patternResultsEnumerable.Any())
                        keepRow = true;
                }
            }
            #endregion

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}