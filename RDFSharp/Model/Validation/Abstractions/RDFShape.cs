﻿/*
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFShape represents a SHACL shape definition
    /// </summary>
    public class RDFShape: RDFResource, IEnumerable<RDFConstraint> {

        #region Properties
        /// <summary>
        /// Indicates the severity level of this SHACL shape (sh:severity)
        /// </summary>
        public RDFValidationEnums.RDFShapeSeverity Severity { get; internal set; }

        /// <summary>
        /// Indicates that this SHACL shape is ignored (sh:deactivated)
        /// </summary>
        public Boolean Deactivated { get; internal set; }

        /// <summary>
        /// Count of the human-readable messages of this SHACL shape
        /// </summary>
        public Int64 MessagesCount {
            get { return this.Messages.Count; }
        }

        /// <summary>
        /// Gets the enumerator on human-readable messages of this SHACL shape for iteration
        /// </summary>
        public IEnumerator<RDFLiteral> MessagesEnumerator {
            get { return this.Messages.GetEnumerator(); }
        }

        /// <summary>
        /// Count of the SHACL targets of this SHACL shape
        /// </summary>
        public Int64 TargetsCount {
            get { return this.Targets.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the SHACL targets of this SHACL shape for iteration
        /// </summary>
        public IEnumerator<RDFTarget> TargetsEnumerator {
            get { return this.Targets.GetEnumerator(); }
        }

        /// <summary>
        /// Count of the SHACL constraints of this SHACL shape
        /// </summary>
        public Int64 ConstraintsCount {
            get { return this.Constraints.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the SHACL constraints of this SHACL shape for iteration
        /// </summary>
        public IEnumerator<RDFConstraint> ConstraintsEnumerator {
            get { return this.Constraints.GetEnumerator(); }
        }

        /// <summary>
        /// Indicates the human-readable messages of this SHACL shape (sh:message)
        /// </summary>
        internal List<RDFLiteral> Messages { get; set; }

        /// <summary>
        /// Indicats the SHACL targets of this SHACL shape (sh:targetClass,sh:targetNode,sh:targetsSubjectsOf,sh:targetObjectsOf)
        /// </summary>
        internal List<RDFTarget> Targets { get; set; }

        /// <summary>
        /// Indicates the SHACL constraints of this SHACL shape
        /// </summary>
        internal List<RDFConstraint> Constraints { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a generic SHACL shape
        /// </summary>
        internal RDFShape(RDFResource shapeName) : base(shapeName.ToString()) {
            this.Deactivated = false;
            this.Severity = RDFValidationEnums.RDFShapeSeverity.Violation;
            this.Messages = new List<RDFLiteral>();
            this.Targets = new List<RDFTarget>();
            this.Constraints = new List<RDFConstraint>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on this SHACL shape's constraints
        /// </summary>
        IEnumerator<RDFConstraint> IEnumerable<RDFConstraint>.GetEnumerator() {
            return this.ConstraintsEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on this SHACL shape's constraints
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.ConstraintsEnumerator;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Activates this SHACL shape, making it available to the processor
        /// </summary>
        public RDFShape Activate()
        {
            this.Deactivated = false;
            return this;
        }

        /// <summary>
        /// Deactivates this SHACL shape, making it unavailable to the processor
        /// </summary>
        public RDFShape Deactivate()
        {
            this.Deactivated = true;
            return this;
        }

        /// <summary>
        /// Sets the severity level of this SHACL shape
        /// </summary>
        public RDFShape SetSeverity(RDFValidationEnums.RDFShapeSeverity shapeSeverity)
        {
            this.Severity = shapeSeverity;
            return this;
        }

        /// <summary>
        /// Adds the given human-readable message to this SHACL shape
        /// </summary>
        public RDFShape AddMessage(RDFLiteral message) {
            if (message != null) {
 
                //Plain Literal (only one occurrence per language tag is allowed)
                if (message is RDFPlainLiteral) {
                    string languageTag = ((RDFPlainLiteral)message).Language;
                    if (!RDFValidationHelper.CheckLanguageTagInUse(this.Messages, languageTag)) {
                        this.Messages.Add(message);
                    }
                }

                //Typed Literal (only xsd:String datatype is allowed)
                else {
                    if (((RDFTypedLiteral)message).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)) {
                        this.Messages.Add(message);
                    }
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the given SHACL target to this SHACL shape
        /// </summary>
        public RDFShape AddTarget(RDFTarget target)
        {
            if (target != null)
                this.Targets.Add(target);

            return this;
        }

        /// <summary>
        /// Adds the given SHACL constraint to this SHACL shape
        /// </summary>
        public RDFShape AddConstraint(RDFConstraint constraint) {
            if (constraint != null)
                this.Constraints.Add(constraint);

            return this;
        }

        /// <summary>
        /// Gets a graph representation of this SHACL shape
        /// </summary>
        public virtual RDFGraph ToRDFGraph() {
            var result = new RDFGraph();

            //Severity
            switch (this.Severity) {
                case RDFValidationEnums.RDFShapeSeverity.Info:
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.INFO));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Warning:
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.WARNING));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Violation:
                    result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION));
                    break;
            }

            //Deactivated
            if (this.Deactivated)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.DEACTIVATED, new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)));
            else
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.DEACTIVATED, new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)));

            //Messages
            this.Messages.ForEach(message => result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.MESSAGE, message)));

            //Targets
            this.Targets.ForEach(target => result = result.UnionWith(target.ToRDFGraph(this)));

            //Constraints
            this.Constraints.ForEach(constraint => result = result.UnionWith(constraint.ToRDFGraph(this)));

            result.SetContext(this.URI);
            return result;
        }
        #endregion

    }
}