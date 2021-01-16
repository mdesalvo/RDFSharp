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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyClassModelLens represents a magnifying glass on the knowledge available for a class within an ontology
    /// </summary>
    public class RDFOntologyClassModelLens
    {
        #region Properties
        /// <summary>
        /// Class being observed by the class model lens
        /// </summary>
        public RDFOntologyClass OntologyClass { get; internal set; }

        /// <summary>
        /// Ontology being observed by the class model lens
        /// </summary>
        public RDFOntology Ontology { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a class model lens for the given class on the given ontology
        /// </summary>
        public RDFOntologyClassModelLens(RDFOntologyClass ontologyClass, RDFOntology ontology)
        {
            if (ontologyClass != null)
            {
                if (ontology != null)
                {
                    this.OntologyClass = ontologyClass;
                    this.Ontology = ontology;
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create class model lens because given \"ontology\" parameter is null");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create class model lens because given \"ontologyClass\" parameter is null");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enlists the classes which are directly (or indirectly, if inference is requested) children of the lens class
        /// </summary>
        public List<(bool, RDFOntologyClass)> SubClasses(bool enableInference)
        {
            List<(bool, RDFOntologyClass)> result = new List<(bool, RDFOntologyClass)>();

            //First-level enlisting of subclasses
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesByObject(this.OntologyClass)
                                                                                                       .Where(te => te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
            {
                result.Add((false, (RDFOntologyClass)sf.TaxonomySubject));
            }

            //Inference-enabled discovery of subclasses
            if (enableInference)
            {
                List<RDFOntologyClass> subClasses = RDFOntologyHelper.GetSubClassesOf(this.Ontology.Model.ClassModel, this.OntologyClass).ToList();
                foreach (RDFOntologyClass subClass in subClasses)
                {
                    if (!result.Any(f => f.Item2.Equals(subClass)))
                        result.Add((true, subClass));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the classes which are directly (or indirectly, if inference is requested) parents of the lens class
        /// </summary>
        public List<(bool, RDFOntologyClass)> SuperClasses(bool enableInference)
        {
            List<(bool, RDFOntologyClass)> result = new List<(bool, RDFOntologyClass)>();

            //First-level enlisting of superclasses
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(this.OntologyClass)
                                                                                                       .Where(te => te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
            {
                result.Add((false, (RDFOntologyClass)sf.TaxonomyObject));
            }

            //Inference-enabled discovery of superclasses
            if (enableInference)
            {
                List<RDFOntologyClass> superClasses = RDFOntologyHelper.GetSuperClassesOf(this.Ontology.Model.ClassModel, this.OntologyClass).ToList();
                foreach (RDFOntologyClass superClass in superClasses)
                {
                    if (!result.Any(f => f.Item2.Equals(superClass)))
                        result.Add((true, superClass));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the classes which are directly (or indirectly, if inference is requested) equivalent to the lens class
        /// </summary>
        public List<(bool, RDFOntologyClass)> EquivalentClasses(bool enableInference)
        {
            List<(bool, RDFOntologyClass)> result = new List<(bool, RDFOntologyClass)>();

            //First-level enlisting of equivalent classes
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.EquivalentClass.SelectEntriesBySubject(this.OntologyClass)
                                                                                                            .Where(te => te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
            {
                result.Add((false, (RDFOntologyClass)sf.TaxonomyObject));
            }

            //Inference-enabled discovery of equivalent classes
            if (enableInference)
            {
                List<RDFOntologyClass> equivalentClasses = RDFOntologyHelper.GetEquivalentClassesOf(this.Ontology.Model.ClassModel, this.OntologyClass).ToList();
                foreach (RDFOntologyClass equivalentClass in equivalentClasses)
                {
                    if (!result.Any(f => f.Item2.Equals(equivalentClass)))
                        result.Add((true, equivalentClass));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the classes which are directly (or indirectly, if inference is requested) disjoint from the lens class
        /// </summary>
        public List<(bool, RDFOntologyClass)> DisjointClasses(bool enableInference)
        {
            List<(bool, RDFOntologyClass)> result = new List<(bool, RDFOntologyClass)>();

            //First-level enlisting of disjoint classes
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.DisjointWith.SelectEntriesBySubject(this.OntologyClass)
                                                                                                         .Where(te => te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
            {
                result.Add((false, (RDFOntologyClass)sf.TaxonomyObject));
            }

            //Inference-enabled discovery of disjoint classes
            if (enableInference)
            {
                List<RDFOntologyClass> disjointClasses = RDFOntologyHelper.GetDisjointClassesWith(this.Ontology.Model.ClassModel, this.OntologyClass).ToList();
                foreach (RDFOntologyClass disjointClass in disjointClasses)
                {
                    if (!result.Any(f => f.Item2.Equals(disjointClass)))
                        result.Add((true, disjointClass));
                }
            }

            return result;
        }
        #endregion
    }
}