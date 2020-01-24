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

using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFLanguageInConstraint represents a SHACL constraint on the allowed language tags for a given RDF term
    /// </summary>
    public class RDFLanguageInConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Language Tags allowed on the given RDF term
        /// </summary>
        internal HashSet<string> LanguageTags { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named languageIn constraint
        /// </summary>
        public RDFLanguageInConstraint(RDFResource constraintName, List<string> languageTags) : base(constraintName) {
            this.LanguageTags = new HashSet<string>();

            //Accept only language tags compatible with langMatches filter
            languageTags?.ForEach(lt => {
                string languageTag = lt?.Trim() ?? string.Empty;
                if (languageTag == string.Empty || languageTag == "*" || Regex.IsMatch(languageTag, "^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$")) {
                    this.LanguageTags.Add(languageTag.ToUpperInvariant());
                }
            });
        }

        /// <summary>
        /// Default-ctor to build a blank languageIn constraint
        /// </summary>
        public RDFLanguageInConstraint(List<string> languageTags) : this(new RDFResource(), languageTags) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFShape shape,
                                                                 RDFGraph dataGraph,
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode) {
            var report = new RDFValidationReport(new RDFResource());
            switch (valueNode) {

                //Resource/TypedLiteral
                case RDFResource valueNodeResource:
                case RDFTypedLiteral valueNodeTypedLiteral:
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                             valueNode,
                                                             shape.Messages,
                                                             new RDFResource(),
                                                             shape.Severity));
                    break;

                //PlainLiteral
                case RDFPlainLiteral valueNodePlainLiteral:
                    using (DataTable table = new DataTable(this.ToString())) {

                        //Create langMatches table
                        RDFQueryEngine.AddColumn(table, "languageTag");
                        RDFQueryEngine.AddRow(table, new Dictionary<string, string>() {
                            { "languageTag", valueNodePlainLiteral.Language }
                        });

                        //Execute langMatches filter
                        bool langMatches = false;
                        var languageTagsEnumerator = this.LanguageTags.GetEnumerator();
                        while (languageTagsEnumerator.MoveNext() && !langMatches) {
                            RDFLangMatchesFilter langMatchesFilter = new RDFLangMatchesFilter(new RDFVariable("languageTag"), languageTagsEnumerator.Current);
                            if (langMatchesFilter.ApplyFilter(table.Rows[0], false)) 
                                langMatches = true;
                        }

                        //Report langMatches violation
                        if (!langMatches) { 
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shape.Messages,
                                                                     new RDFResource(),
                                                                     shape.Severity));
                        }
                    }
                    break;

            }
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        public override RDFGraph ToRDFGraph(RDFShape shape) {
            var result = new RDFGraph();
            if (shape != null) {

                //TODO

            }
            return result;
        }
        #endregion

    }
}