/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Text.RegularExpressions;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFLanguageInConstraint represents a SHACL constraint on the allowed language tags for a given RDF term
    /// </summary>
    public sealed class RDFLanguageInConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Language Tags allowed on the given RDF term
        /// </summary>
        internal HashSet<string> LanguageTags { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a languageIn constraint with the given list of language tags
        /// </summary>
        public RDFLanguageInConstraint(List<string> languageTags)
        {
            LanguageTags = [];

            //Accept only language tags compatible with langMatches filter
            languageTags?.ForEach(lt =>
            {
                string languageTag = lt?.Trim() ?? string.Empty;
                if (languageTag.Length == 0 || languageTag == "*" || RDFRegex.LangTagRegex().IsMatch(languageTag))
                    LanguageTags.Add(languageTag.ToUpperInvariant());
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
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = [.. shape.Messages];
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral("Not a language from the sh:languageIn enumeration"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
                switch (valueNode)
                {
                    //PlainLiteral
                    case RDFPlainLiteral valueNodePLit:
                        string valueNodePLitString = valueNodePLit.ToString();
                        bool langMatches = false;
                        HashSet<string>.Enumerator langTagsEnumerator = LanguageTags.GetEnumerator();
                        while (langTagsEnumerator.MoveNext() && !langMatches)
                            switch (langTagsEnumerator.Current)
                            {
                                //NO language is found in the variable
                                case "":
                                    langMatches = !RDFRegex.EndingLangTagRegex().IsMatch(valueNodePLitString);
                                    break;

                                //ANY language is found in the variable
                                case "*":
                                    langMatches = RDFRegex.EndingLangTagRegex().IsMatch(valueNodePLitString);
                                    break;

                                //GIVEN language is found in the variable
                                default:
                                    langMatches = Regex.IsMatch(valueNodePLitString, $"@{langTagsEnumerator.Current}{RDFRegex.LangTagSubMask}$", RegexOptions.IgnoreCase);
                                    break;
                            }

                        if (!langMatches)
                        {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     pShape?.Path,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        }
                        break;

                    //Resource/TypedLiteral
                    default:
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 pShape?.Path,
                                                                 valueNode,
                                                                 shapeMessages,
                                                                 shape.Severity));
                        break;
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
                foreach (string languageTag in LanguageTags)
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