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
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyModel represents the model component (T-BOX) of an ontology.
    /// </summary>
    public class RDFOntologyModel
    {
        #region Properties
        /// <summary>
        /// Submodel containing the ontology classes
        /// </summary>
        public RDFOntologyClassModel ClassModel { get; set; }

        /// <summary>
        /// Submodel containing the ontology properties
        /// </summary>
        public RDFOntologyPropertyModel PropertyModel { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology model
        /// </summary>
        public RDFOntologyModel()
        {
            this.ClassModel = new RDFOntologyClassModel();
            this.PropertyModel = new RDFOntologyPropertyModel();
        }
        #endregion

        #region Methods

        #region Set
        /// <summary>
        /// Builds a new intersection model from this model and a given one
        /// </summary>
        public RDFOntologyModel IntersectWith(RDFOntologyModel model)
        {
            RDFOntologyModel result = new RDFOntologyModel();

            if (model != null)
            {
                //Intersect the class models
                result.ClassModel = this.ClassModel.IntersectWith(model.ClassModel);

                //Intersect the property models
                result.PropertyModel = this.PropertyModel.IntersectWith(model.PropertyModel);
            }

            return result;
        }

        /// <summary>
        /// Builds a new union model from this model and a given one
        /// </summary>
        public RDFOntologyModel UnionWith(RDFOntologyModel model)
        {
            RDFOntologyModel result = new RDFOntologyModel();

            //Use this class model
            result.ClassModel = result.ClassModel.UnionWith(this.ClassModel);

            //Use this property model
            result.PropertyModel = result.PropertyModel.UnionWith(this.PropertyModel);

            //Manage the given model
            if (model != null)
            {
                //Union with the given class model
                result.ClassModel = result.ClassModel.UnionWith(model.ClassModel);

                //Union with the given property model
                result.PropertyModel = result.PropertyModel.UnionWith(model.PropertyModel);
            }
            return result;
        }

        /// <summary>
        /// Builds a new difference model from this model and a given one
        /// </summary>
        public RDFOntologyModel DifferenceWith(RDFOntologyModel model)
        {
            RDFOntologyModel result = new RDFOntologyModel();

            //Use this class model
            result.ClassModel = result.ClassModel.UnionWith(this.ClassModel);

            //Use this property model
            result.PropertyModel = result.PropertyModel.UnionWith(this.PropertyModel);

            //Manage the given model
            if (model != null)
            {
                //Difference with the given class model
                result.ClassModel = result.ClassModel.DifferenceWith(model.ClassModel);

                //Difference with the given property model
                result.PropertyModel = result.PropertyModel.DifferenceWith(model.PropertyModel);
            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this ontology model, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
            => this.ClassModel.ToRDFGraph(infexpBehavior)
                              .UnionWith(this.PropertyModel.ToRDFGraph(infexpBehavior));

        /// <summary>
        /// Asynchronously gts a graph representation of this ontology model, exporting inferences according to the selected behavior
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
            => Task.Run(() => ToRDFGraph(infexpBehavior));
        #endregion

        #endregion
    }
}