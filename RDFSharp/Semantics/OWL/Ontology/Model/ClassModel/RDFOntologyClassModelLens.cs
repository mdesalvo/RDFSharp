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
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesByObject(this.OntologyClass).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyClass)sf.TaxonomySubject));

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
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(this.OntologyClass).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyClass)sf.TaxonomyObject));

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
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.EquivalentClass.SelectEntriesBySubject(this.OntologyClass).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyClass)sf.TaxonomyObject));

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
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Relations.DisjointWith.SelectEntriesBySubject(this.OntologyClass).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyClass)sf.TaxonomyObject));

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

        /// <summary>
        /// Enlists the facts/literals which are directly (or indirectly, if inference is requested) members of the lens class
        /// </summary>
        public List<(bool, RDFOntologyResource)> Members(bool enableInference)
        {
            List<(bool, RDFOntologyResource)> result = new List<(bool, RDFOntologyResource)>();

            //First-level enlisting of members (datatype class)
            if (RDFOntologyHelper.CheckIsLiteralCompatibleClass(this.Ontology.Model.ClassModel, this.OntologyClass))
            {
                IEnumerable<RDFOntologyLiteral> ontlits = this.Ontology.Data.Literals.Values.Where(ol => ol.Value is RDFTypedLiteral);
                foreach (RDFOntologyLiteral ontlit in ontlits.Where(ol => RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)ol.Value).Datatype).Equals(this.OntologyClass)))
                {
                    if (!result.Any(ol => ol.Equals(ontlit)))
                        result.Add((false, ontlit));
                }
            }
            //First-level enlisting of members (object class)
            else
            {
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.ClassType.SelectEntriesByObject(this.OntologyClass).Where(te => !te.IsInference()))
                    result.Add((false, sf.TaxonomySubject));
            }

            //Inference-enabled discovery of members
            if (enableInference)
            {
                RDFOntologyData members = RDFOntologyHelper.GetMembersOf(this.Ontology, this.OntologyClass);
                foreach (RDFOntologyFact fact in members)
                {
                    if (!result.Any(f => f.Item2.Equals(fact)))
                        result.Add((true, fact));
                }
                foreach (RDFOntologyLiteral literal in members.Literals.Values)
                {
                    if (!result.Any(f => f.Item2.Equals(literal)))
                        result.Add((true, literal));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the object annotations which are assigned to the lens class
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyResource)> ObjectAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyResource)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyResource)>();

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.SeeAlso.SelectEntriesBySubject(this.OntologyClass).Where(te => !te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.IsDefinedBy.SelectEntriesBySubject(this.OntologyClass).Where(te => !te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(this.OntologyClass).Where(te => !te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Enlists the literal annotations which are assigned to the lens class
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> DataAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>();

            //VersionInfo
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.VersionInfo.SelectEntriesBySubject(this.OntologyClass).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Comment
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.Comment.SelectEntriesBySubject(this.OntologyClass).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Label
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.Label.SelectEntriesBySubject(this.OntologyClass).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.SeeAlso.SelectEntriesBySubject(this.OntologyClass).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.IsDefinedBy.SelectEntriesBySubject(this.OntologyClass).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(this.OntologyClass).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            return result;
        }
        #endregion
    }
}