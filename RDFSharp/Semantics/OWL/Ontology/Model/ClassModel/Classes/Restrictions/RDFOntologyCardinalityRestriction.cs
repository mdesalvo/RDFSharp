/*
   Copyright 2015-2020 Marco De Salvo

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
    /// RDFOntologyCardinalityRestriction represents an "owl:Cardinality" restriction class definition within an ontology model.
    /// </summary>
    public class RDFOntologyCardinalityRestriction : RDFOntologyRestriction
    {

        #region Properties
        /// <summary>
        /// Minimum accepted cardinality for the restriction to be satisfied
        /// </summary>
        public int MinCardinality { get; internal set; }

        /// <summary>
        /// Maximum accepted cardinality for the restriction to be satisfied
        /// </summary>
        public int MaxCardinality { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology cardinality restriction with the given name on the given property
        /// </summary>
        public RDFOntologyCardinalityRestriction(RDFResource restrictionName,
                                                 RDFOntologyProperty onProperty,
                                                 int minCardinality,
                                                 int maxCardinality) : base(restrictionName, onProperty)
        {

            //MinCardinality
            if (minCardinality > 0)
            {
                if (maxCardinality > 0)
                {
                    if (minCardinality <= maxCardinality)
                    {
                        this.MinCardinality = minCardinality;
                    }
                    else
                    {
                        throw new RDFSemanticsException("Cannot create RDFOntologyCardinalityRestriction because given \"minCardinality\" parameter (" + minCardinality + ") must be less or equal than given \"maxCardinality\" parameter (" + maxCardinality + ")");
                    }
                }
                else
                {
                    this.MinCardinality = minCardinality;
                }
            }

            //MaxCardinality
            if (maxCardinality > 0)
            {
                if (minCardinality > 0)
                {
                    if (maxCardinality >= minCardinality)
                    {
                        this.MaxCardinality = maxCardinality;
                    }
                    else
                    {
                        throw new RDFSemanticsException("Cannot create RDFOntologyCardinalityRestriction because given \"maxCardinality\" parameter (" + maxCardinality + ") must be greater or equal than given \"minCardinality\" parameter (" + minCardinality + ")");
                    }
                }
                else
                {
                    this.MaxCardinality = maxCardinality;
                }
            }

            if (this.MinCardinality == 0 && this.MaxCardinality == 0)
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyCardinalityRestriction because at least one of the given \"minCardinality\" and \"maxCardinality\" parameters must be greater than 0.");
            }
        }
        #endregion

    }

}