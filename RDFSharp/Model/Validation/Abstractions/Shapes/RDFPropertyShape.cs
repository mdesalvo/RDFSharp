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

using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFPropertyShape represents a SHACL property shape definition
    /// </summary>
    public class RDFPropertyShape : RDFShape
    {
        #region Properties
        /// <summary>
        /// Indicates the property on which this property shape is applied (sh:path)
        /// </summary>
        public RDFResource Path { get; internal set; }

        /// <summary>
        /// Indicates the human-readable descriptions of this property shape's path (sh:description)
        /// </summary>
        public List<RDFLiteral> Descriptions { get; internal set; }

        /// <summary>
        /// Indicates the human-readable labels of this property shape's path (sh:name)
        /// </summary>
        public List<RDFLiteral> Names { get; internal set; }

        /// <summary>
        /// Indicates the relative order of this property shape compared to its siblings (sh:order)
        /// </summary>
        public RDFLiteral Order { get; internal set; }

        /// <summary>
        /// Indicates the group of property shapes to which this property shape belongs (sh:group)
        /// </summary>
        public RDFResource Group { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named property shape on the given property
        /// </summary>
        public RDFPropertyShape(RDFResource propertyShapeName, RDFResource path) : base(propertyShapeName)
        {
            if (path == null)
                throw new RDFModelException("Cannot create RDFPropertyShape because given \"path\" parameter is null.");
            
            this.Path = path;
            this.Descriptions = new List<RDFLiteral>();
            this.Names = new List<RDFLiteral>();
        }

        /// <summary>
        /// Default-ctor to build a blank property shape on the given property
        /// </summary>
        public RDFPropertyShape(RDFResource path) : this(new RDFResource(), path) { }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given human-readable description to this property shape's path
        /// </summary>
        public RDFPropertyShape AddDescription(RDFLiteral description)
        {
            if (description != null)
            {
                //Plain Literal
                if (description is RDFPlainLiteral)
                    this.Descriptions.Add(description);

                //Typed Literal
                else
                    if (((RDFTypedLiteral)description).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))
                    this.Descriptions.Add(description);
            }
            return this;
        }

        /// <summary>
        /// Adds the given human-readable label to this property shape's path
        /// </summary>
        public RDFPropertyShape AddName(RDFLiteral name)
        {
            if (name != null)
            {
                //Plain Literal
                if (name is RDFPlainLiteral)
                    this.Names.Add(name);

                //Typed Literal
                else
                    if (((RDFTypedLiteral)name).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))
                    this.Names.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Sets the relative order of this property shape compared to its siblings
        /// </summary>
        public RDFPropertyShape SetOrder(int order)
        {
            this.Order = new RDFTypedLiteral(order.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
            return this;
        }

        /// <summary>
        /// Sets the group of property shapes to which this SHACL property shape belongs
        /// </summary>
        public RDFPropertyShape SetGroup(RDFResource group)
        {
            this.Group = group;
            return this;
        }

        /// <summary>
        /// Gets a graph representation of this property shape
        /// </summary>
        public override RDFGraph ToRDFGraph()
        {
            RDFGraph result = base.ToRDFGraph();

            //PropertyShape
            result.AddTriple(new RDFTriple(this, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE));

            //Path
            result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.PATH, this.Path));

            //Descriptions
            this.Descriptions.ForEach(description => result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.DESCRIPTION, description)));

            //Names
            this.Names.ForEach(name => result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.NAME, name)));

            //Order
            if (this.Order != null)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.ORDER, this.Order));

            //Group
            if (this.Group != null)
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.GROUP, this.Group));

            return result;
        }
        #endregion
    }
}