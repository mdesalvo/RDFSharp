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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
        internal DataTable PatternResults { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given pattern
        /// </summary>
        public RDFExistsFilter(RDFPattern pattern)
        {
            if (pattern != null)
            {
                if (pattern.Variables.Any())
                {
                    this.Pattern = pattern;
                    this.IsEvaluable = true;
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFExistsFilter because given \"pattern\" parameter is a ground pattern.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFExistsFilter because given \"pattern\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( EXISTS { ", this.Pattern.ToString(prefixes), " } )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the pattern in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = false;
            EnumerableRowCollection<DataRow> patternResultsEnumerable = this.PatternResults?.AsEnumerable();
            if (patternResultsEnumerable?.Any() ?? false)
            {

                #region Disjoint Evaluation
                //In case of disjointess between the query and the filter's pattern, all solutions are compatible
                bool disjointSubject = this.Pattern.Subject is RDFVariable ?
                                            !row.Table.Columns.Contains(this.Pattern.Subject.ToString()) : true;
                bool disjointPredicate = this.Pattern.Predicate is RDFVariable ?
                                              !row.Table.Columns.Contains(this.Pattern.Predicate.ToString()) : true;
                bool disjointObject = this.Pattern.Object is RDFVariable ?
                                           !row.Table.Columns.Contains(this.Pattern.Object.ToString()) : true;
                if (disjointSubject && disjointPredicate && disjointObject)
                    keepRow = true;
                #endregion

                #region Non-Disjoint Evaluation
                else
                {

                    #region Subject
                    bool subjectCompared = false;
                    if (this.Pattern.Subject is RDFVariable
                            && this.PatternResults.Columns.Contains(this.Pattern.Subject.ToString())
                                && row.Table.Columns.Contains(this.Pattern.Subject.ToString()))
                    {
                        //In case of emptiness the solution is compatible, otherwise proceed with comparison
                        if (!row.IsNull(this.Pattern.Subject.ToString()))
                        {
                            //Get subject filter's value for the given row
                            RDFPatternMember rowMember = RDFQueryUtilities.ParseRDFPatternMember(row[this.Pattern.Subject.ToString()].ToString());

                            //Apply subject filter on the pattern resultset
                            patternResultsEnumerable = patternResultsEnumerable.Where(x => RDFQueryUtilities.ParseRDFPatternMember(x.Field<string>(this.Pattern.Subject.ToString())).Equals(rowMember));
                        }
                        subjectCompared = true;
                    }
                    #endregion

                    #region Predicate
                    bool predicateCompared = false;
                    if (this.Pattern.Predicate is RDFVariable
                            && this.PatternResults.Columns.Contains(this.Pattern.Predicate.ToString())
                                && row.Table.Columns.Contains(this.Pattern.Predicate.ToString()))
                    {
                        //In case of emptiness the solution is compatible, otherwise proceed with comparison
                        if (!row.IsNull(this.Pattern.Predicate.ToString()))
                        {
                            //Get predicate filter's value for the given row
                            RDFPatternMember rowMember = RDFQueryUtilities.ParseRDFPatternMember(row[this.Pattern.Predicate.ToString()].ToString());

                            //Apply predicate filter on the pattern resultset
                            patternResultsEnumerable = patternResultsEnumerable.Where(x => RDFQueryUtilities.ParseRDFPatternMember(x.Field<string>(this.Pattern.Predicate.ToString())).Equals(rowMember));
                        }
                        predicateCompared = true;
                    }
                    #endregion

                    #region Object
                    bool objectCompared = false;
                    if (this.Pattern.Object is RDFVariable
                            && this.PatternResults.Columns.Contains(this.Pattern.Object.ToString())
                                && row.Table.Columns.Contains(this.Pattern.Object.ToString()))
                    {
                        //In case of emptiness the solution is compatible, otherwise proceed with comparison
                        if (!row.IsNull(this.Pattern.Object.ToString()))
                        {
                            //Get object filter's value for the given row
                            RDFPatternMember rowMember = RDFQueryUtilities.ParseRDFPatternMember(row[this.Pattern.Object.ToString()].ToString());

                            //Apply object filter on the pattern resultset
                            patternResultsEnumerable = patternResultsEnumerable.Where(x => RDFQueryUtilities.ParseRDFPatternMember(x.Field<string>(this.Pattern.Object.ToString())).Equals(rowMember));
                        }
                        objectCompared = true;
                    }
                    #endregion

                    #region Decision
                    //Verify filter's response on the pattern resultset
                    if ((subjectCompared || predicateCompared || objectCompared) && patternResultsEnumerable.ToList().Any())
                        keepRow = true;
                    #endregion

                }
                #endregion

            }

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion

    }

}