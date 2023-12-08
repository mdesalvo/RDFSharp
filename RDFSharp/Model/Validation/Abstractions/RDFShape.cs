/*
   Copyright 2012-2024 Marco De Salvo

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

using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFShape represents a generic SHACL shape definition
    /// </summary>
    public class RDFShape : RDFResource, IEnumerable<RDFConstraint>
    {
        #region Properties
        /// <summary>
        /// Indicates the severity level of this shape (sh:severity)
        /// </summary>
        public RDFValidationEnums.RDFShapeSeverity Severity { get; internal set; }

        /// <summary>
        /// Indicates that this shape is ignored (sh:deactivated)
        /// </summary>
        public bool Deactivated { get; internal set; }

        /// <summary>
        /// Count of the human-readable messages of this shape
        /// </summary>
        public long MessagesCount => Messages.Count;

        /// <summary>
        /// Gets the enumerator on human-readable messages of this shape for iteration
        /// </summary>
        public IEnumerator<RDFLiteral> MessagesEnumerator => Messages.GetEnumerator();

        /// <summary>
        /// Count of the targets of this shape
        /// </summary>
        public long TargetsCount => Targets.Count;

        /// <summary>
        /// Gets the enumerator on the targets of this shape for iteration
        /// </summary>
        public IEnumerator<RDFTarget> TargetsEnumerator => Targets.GetEnumerator();

        /// <summary>
        /// Count of the constraints of this shape
        /// </summary>
        public long ConstraintsCount => Constraints.Count;

        /// <summary>
        /// Gets the enumerator on the constraints of this shape for iteration
        /// </summary>
        public IEnumerator<RDFConstraint> ConstraintsEnumerator => Constraints.GetEnumerator();

        /// <summary>
        /// Indicates the human-readable messages of this shape (sh:message)
        /// </summary>
        internal List<RDFLiteral> Messages { get; set; }

        /// <summary>
        /// Indicats the targets of this shape (sh:targetClass,sh:targetNode,sh:targetsSubjectsOf,sh:targetObjectsOf)
        /// </summary>
        internal List<RDFTarget> Targets { get; set; }

        /// <summary>
        /// Indicates the constraints of this shape
        /// </summary>
        internal List<RDFConstraint> Constraints { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a generic shape
        /// </summary>
        internal RDFShape(RDFResource shapeName) : base(shapeName.ToString())
        {
            Deactivated = false;
            Severity = RDFValidationEnums.RDFShapeSeverity.Violation;
            Messages = new List<RDFLiteral>();
            Targets = new List<RDFTarget>();
            Constraints = new List<RDFConstraint>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on this shape's constraints
        /// </summary>
        IEnumerator<RDFConstraint> IEnumerable<RDFConstraint>.GetEnumerator() => ConstraintsEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on this shape's constraints
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => ConstraintsEnumerator;
        #endregion

        #region Methods
        /// <summary>
        /// Activates this shape, making it available to the processor
        /// </summary>
        public RDFShape Activate()
        {
            Deactivated = false;
            return this;
        }

        /// <summary>
        /// Deactivates this shape, making it unavailable to the processor
        /// </summary>
        public RDFShape Deactivate()
        {
            Deactivated = true;
            return this;
        }

        /// <summary>
        /// Sets the severity level of this shape
        /// </summary>
        public RDFShape SetSeverity(RDFValidationEnums.RDFShapeSeverity shapeSeverity)
        {
            Severity = shapeSeverity;
            return this;
        }

        /// <summary>
        /// Adds the given human-readable message to this shape
        /// </summary>
        public RDFShape AddMessage(RDFLiteral message)
        {
            if (message != null)
            {
                //Plain Literal
                if (message is RDFPlainLiteral)
                    Messages.Add(message);

                //Typed Literal
                else
                    if (((RDFTypedLiteral)message).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))
                        Messages.Add(message);
            }
            return this;
        }

        /// <summary>
        /// Adds the given target to this shape
        /// </summary>
        public RDFShape AddTarget(RDFTarget target)
        {
            if (target != null)
                Targets.Add(target);

            return this;
        }

        /// <summary>
        /// Adds the given constraint to this shape
        /// </summary>
        public RDFShape AddConstraint(RDFConstraint constraint)
        {
            if (constraint != null)
                Constraints.Add(constraint);

            return this;
        }

        /// <summary>
        /// Gets a graph representation of this shape
        /// </summary>
        public virtual RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();

            //Severity
            switch (Severity)
            {
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
            result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.DEACTIVATED, Deactivated ? RDFTypedLiteral.True : RDFTypedLiteral.False));

            //Messages
            Messages.ForEach(message => result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.MESSAGE, message)));

            //Targets
            Targets.ForEach(target => result = result.UnionWith(target.ToRDFGraph(this)));

            //Constraints
            Constraints.ForEach(constraint => result = result.UnionWith(constraint.ToRDFGraph(this)));

            result.SetContext(URI);
            return result;
        }
        #endregion
    }
}