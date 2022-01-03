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

using RDFSharp.Model;
using System;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyQualifiedCardinalityRestriction represents an "owl:QualifiedCardinality" restriction class definition within an ontology model [OWL2].
    /// </summary>
    public class RDFOntologyQualifiedCardinalityRestriction : RDFOntologyRestriction
    {

        #region Properties
        /// <summary>
        /// Minimum accepted qualified cardinality for the restriction to be satisfied
        /// </summary>
        public int MinQualifiedCardinality { get; internal set; }

        /// <summary>
        /// Maximum accepted qualified cardinality for the restriction to be satisfied
        /// </summary>
        public int MaxQualifiedCardinality { get; internal set; }

        /// <summary>
        /// Ontology class on which the ontology qualified restriction is applied
        /// </summary>
        public RDFOntologyClass OnClass { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology qualified cardinality restriction with the given name on the given property and given class
        /// </summary>
        public RDFOntologyQualifiedCardinalityRestriction(RDFResource restrictionName,
                                                          RDFOntologyProperty onProperty,
                                                          RDFOntologyClass onClass,
                                                          int minQualifiedCardinality,
                                                          int maxQualifiedCardinality) : base(restrictionName, onProperty)
        {

            //OnClass
            if (onClass != null)
            {
                this.OnClass = onClass;
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyQualifiedCardinalityRestriction because given \"onClass\" parameter is null.");
            }

            //MinQualifiedCardinality
            if (minQualifiedCardinality > 0)
            {
                if (maxQualifiedCardinality > 0)
                {
                    if (minQualifiedCardinality <= maxQualifiedCardinality)
                    {
                        this.MinQualifiedCardinality = minQualifiedCardinality;
                    }
                    else
                    {
                        throw new RDFSemanticsException("Cannot create RDFOntologyQualifiedCardinalityRestriction because given \"minQualifiedCardinality\" parameter (" + minQualifiedCardinality + ") must be less or equal than given \"maxQualifiedCardinality\" parameter (" + maxQualifiedCardinality + ")");
                    }
                }
                else
                {
                    this.MinQualifiedCardinality = minQualifiedCardinality;
                }
            }

            //MaxQualifiedCardinality
            if (maxQualifiedCardinality > 0)
            {
                if (minQualifiedCardinality > 0)
                {
                    if (maxQualifiedCardinality >= minQualifiedCardinality)
                    {
                        this.MaxQualifiedCardinality = maxQualifiedCardinality;
                    }
                    else
                    {
                        throw new RDFSemanticsException("Cannot create RDFOntologyQualifiedCardinalityRestriction because given \"maxQualifiedCardinality\" parameter (" + maxQualifiedCardinality + ") must be greater or equal than given \"minQualifiedCardinality\" parameter (" + minQualifiedCardinality + ")");
                    }
                }
                else
                {
                    this.MaxQualifiedCardinality = maxQualifiedCardinality;
                }
            }

            if (this.MinQualifiedCardinality == 0 && this.MaxQualifiedCardinality == 0)
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyQualifiedCardinalityRestriction because at least one of the given \"minQualifiedCardinality\" and \"maxQualifiedCardinality\" parameters must be greater than 0.");
            }

        }
        #endregion

    }
}