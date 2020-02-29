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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
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
        /// Default-ctor to build a languageIn constraint with the given list of language tags
        /// </summary>
        public RDFLanguageInConstraint(List<string> languageTags) : base() {
            this.LanguageTags = new HashSet<string>();

            //Accept only language tags compatible with langMatches filter
            languageTags?.ForEach(lt => {
                string languageTag = lt?.Trim() ?? string.Empty;
                if (languageTag == string.Empty || languageTag == "*" || Regex.IsMatch(languageTag, "^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$")) {
                    this.LanguageTags.Add(languageTag.ToUpperInvariant());
                }
            });
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            switch (validationContext.ValueNode) {

                //Resource/TypedLiteral
                case RDFResource valueNodeResource:
                case RDFTypedLiteral valueNodeTypedLiteral:
                    report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                             RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                             validationContext.FocusNode,
                                                             validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                             validationContext.ValueNode,
                                                             validationContext.Shape.Messages,
                                                             new RDFResource(),
                                                             validationContext.Shape.Severity));
                    break;

                //PlainLiteral
                case RDFPlainLiteral valueNodePlainLiteral:
                    using (DataTable table = new DataTable(this.ToString())) {

                        //Create langMatches table
                        RDFQueryEngine.AddColumn(table, "?valueNode");
                        RDFQueryEngine.AddRow(table, new Dictionary<string, string>() {
                            { "?valueNode", valueNodePlainLiteral.ToString() }
                        });

                        //Execute langMatches filter
                        bool langMatches = false;
                        var languageTagsEnumerator = this.LanguageTags.GetEnumerator();
                        while (languageTagsEnumerator.MoveNext() && !langMatches) {
                            RDFLangMatchesFilter langMatchesFilter = new RDFLangMatchesFilter(new RDFVariable("valueNode"), languageTagsEnumerator.Current);
                            if (langMatchesFilter.ApplyFilter(table.Rows[0], false)) 
                                langMatches = true;
                        }

                        //Report langMatches violation
                        if (!langMatches) { 
                            report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                                     RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                                     validationContext.FocusNode,
                                                                     validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                                     validationContext.ValueNode,
                                                                     validationContext.Shape.Messages,
                                                                     new RDFResource(),
                                                                     validationContext.Shape.Severity));
                        }
                    }
                    break;

            }
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //Get collection from language tags
                RDFCollection languageTags = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal) { InternalReificationSubject = this };
                this.LanguageTags.ToList().ForEach(lt => languageTags.AddItem(new RDFPlainLiteral(lt)));
                result.AddCollection(languageTags);

                //sh:languageIn
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.LANGUAGE_IN, languageTags.ReificationSubject));

            }
            return result;
        }
        #endregion

    }
}