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

using RDFSharp.Query;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    ///  RDFValidationResult represents an evidence reported by a shape's validation.
    /// </summary>
    public class RDFValidationResult : RDFResource
    {
        #region Properties
        /// <summary>
        /// Indicates the severity level of this validation result (sh:resultSeverity)
        /// </summary>
        public RDFValidationEnums.RDFShapeSeverity Severity { get; internal set; }

        /// <summary>
        /// Indicates the shape which caused the validation result (sh:sourceShape)
        /// </summary>
        public RDFResource SourceShape { get; internal set; }

        /// <summary>
        /// Indicates the constraint component which caused the validation result (sh:sourceConstraintComponent)
        /// </summary>
        public RDFResource SourceConstraintComponent { get; internal set; }

        /// <summary>
        /// Indicates the node which caused the validation result (sh:focusNode)
        /// </summary>
        public RDFPatternMember FocusNode { get; internal set; }

        /// <summary>
        /// Indicates the property which caused the validation result (sh:resultPath)
        /// </summary>
        public RDFResource ResultPath { get; internal set; }

        /// <summary>
        /// Indicates the value which caused the validation result (sh:value)
        /// </summary>
        public RDFPatternMember ResultValue { get; internal set; }

        /// <summary>
        /// Indicates the human-readable messages of this validation result (sh:resultMessage)
        /// </summary>
        public List<RDFLiteral> ResultMessages { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a validation result with the given properties
        /// </summary>
        internal RDFValidationResult(RDFResource sourceShape,
                                     RDFResource sourceConstraintComponent,
                                     RDFPatternMember focusNode,
                                     RDFResource resultPath,
                                     RDFPatternMember resultValue,
                                     List<RDFLiteral> resultMessages,
                                     RDFValidationEnums.RDFShapeSeverity severity = RDFValidationEnums.RDFShapeSeverity.Violation)
        {
            SourceShape = sourceShape;
            SourceConstraintComponent = sourceConstraintComponent;
            FocusNode = focusNode;
            ResultPath = resultPath;
            ResultValue = resultValue;
            ResultMessages = resultMessages ?? new List<RDFLiteral>();
            Severity = severity;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph representation of this validation result
        /// </summary>
        public RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();

            //ValidationResult
            result.AddTriple(new RDFTriple(this, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT));

            //Severity
            switch (Severity)
            {
                case RDFValidationEnums.RDFShapeSeverity.Info:
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.INFO));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Warning:
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.WARNING));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Violation:
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION));
                    break;
            }

            //SourceShape
            if (SourceShape != null)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.SOURCE_SHAPE, SourceShape));

            //SourceConstraintComponent
            if (SourceConstraintComponent != null)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, SourceConstraintComponent));

            //FocusNode
            if (FocusNode is RDFResource focusNodeResource)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.FOCUS_NODE, focusNodeResource));

            //ResultPath
            if (ResultPath != null)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT_PATH, ResultPath));

            //Value
            if (ResultValue != null)
            {
                if (ResultValue is RDFLiteral resvalLit)
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.VALUE, resvalLit));
                else
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.VALUE, (RDFResource)ResultValue));
            }

            //Messages
            ResultMessages.ForEach(message => result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.RESULT_MESSAGE, message)));

            result.SetContext(URI);
            return result;
        }
        #endregion
    }
}