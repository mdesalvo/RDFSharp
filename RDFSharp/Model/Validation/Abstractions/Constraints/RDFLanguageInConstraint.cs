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
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFLanguageInConstraint represents a SHACL constraint on the allowed language tags for a given RDF term
    /// </summary>
    public class RDFLanguageInConstraint : RDFConstraint
    {

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
        public RDFLanguageInConstraint(List<string> languageTags) : base()
        {
            this.LanguageTags = new HashSet<string>();

            //Accept only language tags compatible with langMatches filter
            languageTags?.ForEach(lt =>
            {
                string languageTag = lt?.Trim() ?? string.Empty;
                if (languageTag == string.Empty || languageTag == "*" || RDFPlainLiteral.LangTag.Match(languageTag).Success)
                    this.LanguageTags.Add(languageTag.ToUpperInvariant());
            });
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                switch (valueNode)
                {

                    //PlainLiteral
                    case RDFPlainLiteral valueNodePlainLiteral:
                        bool langMatches = false;
                        var langTagsEnumerator = this.LanguageTags.GetEnumerator();
                        while (langTagsEnumerator.MoveNext() && !langMatches)
                        {

                            //NO language is found in the variable
                            if (langTagsEnumerator.Current == string.Empty)
                                langMatches = !Regex.IsMatch(valueNodePlainLiteral.ToString(), "@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                            //ANY language is found in the variable
                            else if (langTagsEnumerator.Current == "*")
                                langMatches = Regex.IsMatch(valueNodePlainLiteral.ToString(), "@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                            //GIVEN language is found in the variable
                            else
                                langMatches = Regex.IsMatch(valueNodePlainLiteral.ToString(), string.Concat("@", langTagsEnumerator.Current, "(-[a-zA-Z0-9]{1,8})*$"), RegexOptions.IgnoreCase);

                        }
                        if (!langMatches)
                        {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shape.Messages,
                                                                     shape.Severity));
                        }
                        break;

                    //Resource/TypedLiteral
                    default:
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 shape.Severity));
                        break;

                }
            }
            #endregion

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape)
        {
            RDFGraph result = new RDFGraph();
            if (shape != null)
            {

                //Get collection from language tags
                RDFCollection languageTags = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal) { InternalReificationSubject = this };
                foreach (string languageTag in this.LanguageTags)
                    languageTags.AddItem(new RDFPlainLiteral(languageTag));
                result.AddCollection(languageTags);

                //sh:languageIn
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.LANGUAGE_IN, languageTags.ReificationSubject));

            }
            return result;
        }
        #endregion

    }
}