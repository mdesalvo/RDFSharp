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
using RDFSharp.Query;
using RDFSharp.Semantics.SKOS;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFSemanticsUtilities is a collector of reusable utility methods for RDF ontology management.
    /// </summary>
    internal static class RDFSemanticsUtilities
    {
        internal static readonly Regex NumberRegex = new Regex(@"^[0-9]+$", RegexOptions.Compiled);
        internal static readonly HashSet<long> StandardAnnotationProperties = new HashSet<long>()
        {
            { RDFVocabulary.OWL.VERSION_INFO.PatternMemberID },
            { RDFVocabulary.OWL.VERSION_IRI.PatternMemberID },
            { RDFVocabulary.RDFS.COMMENT.PatternMemberID },
            { RDFVocabulary.RDFS.LABEL.PatternMemberID },
            { RDFVocabulary.RDFS.SEE_ALSO.PatternMemberID },
            { RDFVocabulary.RDFS.IS_DEFINED_BY.PatternMemberID },
            { RDFVocabulary.OWL.PRIOR_VERSION.PatternMemberID },
            { RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.PatternMemberID },
            { RDFVocabulary.OWL.INCOMPATIBLE_WITH.PatternMemberID },
            { RDFVocabulary.OWL.IMPORTS.PatternMemberID }
        };

        #region Convert
        /// <summary>
        /// Gets an ontology representation of the given graph
        /// </summary>
        internal static RDFOntology FromRDFGraph(RDFGraph ontGraph)
        {
            if (ontGraph == null)
                return null;

            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Graph '{0}' is going to be parsed as Ontology (triples not having supported ontology semantics may be discarded!)", ontGraph.Context));

            //Step 1: start the graph->ontology process
            StartProcess(ontGraph, out Dictionary<string, RDFGraph> prefetchContext);

            //Step 2: initialize ontology (declaration)
            InitializeOntology(ontGraph, prefetchContext, out RDFOntology ontology);

            //Step 3: initialize property model (property declarations)
            InitializePropertyModel(ontology, ontGraph, prefetchContext);

            //Step 4: initialize class model (class declarations)
            InitializeClassModel(ontology, ontGraph, prefetchContext);

            //Step 5: initialize data (fact declarations)
            InitializeData(ontology, ontGraph, prefetchContext);

            //Step 6.1: finalize restrictions (specializations, attributes and relations)
            FinalizeRestrictions(ontology, ontGraph, prefetchContext);

            //Step 6.2: finalize enumerate/datarange classes (specializations, attributes and relations)
            FinalizeEnumeratesAndDataRanges(ontology, ontGraph, prefetchContext);

            //Step 6.3: finalize property model (specializations, attributes and relations)
            FinalizePropertyModel(ontology, ontGraph, prefetchContext);

            //Step 6.4: finalize class model (specializations, attributes and relations)
            FinalizeClassModel(ontology, ontGraph, prefetchContext);

            //Step 6.5: finalize data (attributes and relations)
            FinalizeData(ontology, ontGraph, prefetchContext);

            //Step 6.6: finalize annotations (ontology, class model, property model, data, axioms)
            FinalizeOntologyAnnotations(ontology, ontGraph, prefetchContext);
            FinalizeClassModelAnnotations(ontology, ontGraph, prefetchContext);
            FinalizePropertyModelAnnotations(ontology, ontGraph, prefetchContext);
            FinalizeDataAnnotations(ontology, ontGraph, prefetchContext);
            FinalizeAxiomAnnotations(ontology, ontGraph, prefetchContext);

            //Step 6.7: end the graph->ontology process
            EndProcess(ref ontology);

            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Graph '{0}' has been parsed as Ontology.", ontGraph.Context));
            return ontology;
        }

        /// <summary>
        /// Prefetches the context cache from the given RDF graph
        /// </summary>
        private static void StartProcess(RDFGraph ontGraph, out Dictionary<string, RDFGraph> prefetchContext)
            => prefetchContext = new Dictionary<string, RDFGraph>()
                {
                    { nameof(RDFVocabulary.RDF.TYPE), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE) },
                    { nameof(RDFVocabulary.RDF.FIRST), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST) },
                    { nameof(RDFVocabulary.RDF.REST), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.REST) },
                    { nameof(RDFVocabulary.SKOS.MEMBER), ontGraph.SelectTriplesByPredicate(RDFVocabulary.SKOS.MEMBER) }, //SKOS
                    { nameof(RDFVocabulary.SKOS.MEMBER_LIST), ontGraph.SelectTriplesByPredicate(RDFVocabulary.SKOS.MEMBER_LIST) }, //SKOS
                    { nameof(RDFVocabulary.RDFS.SUB_CLASS_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_CLASS_OF) },
                    { nameof(RDFVocabulary.RDFS.SUB_PROPERTY_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_PROPERTY_OF) },
                    { nameof(RDFVocabulary.RDFS.DOMAIN), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.DOMAIN) },
                    { nameof(RDFVocabulary.RDFS.RANGE), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.RANGE) },
                    { nameof(RDFVocabulary.OWL.EQUIVALENT_CLASS), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_CLASS) },
                    { nameof(RDFVocabulary.OWL.DISJOINT_WITH), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISJOINT_WITH) },
                    { nameof(RDFVocabulary.OWL.ALL_DISJOINT_CLASSES), ontGraph.SelectTriplesByObject(RDFVocabulary.OWL.ALL_DISJOINT_CLASSES).SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)}, //OWL2
                    { nameof(RDFVocabulary.OWL.HAS_KEY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_KEY) }, //OWL2
                    { nameof(RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM) }, //OWL2
                    { nameof(RDFVocabulary.OWL.EQUIVALENT_PROPERTY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_PROPERTY) },
                    { nameof(RDFVocabulary.OWL.PROPERTY_DISJOINT_WITH), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PROPERTY_DISJOINT_WITH) }, //OWL2
                    { nameof(RDFVocabulary.OWL.ALL_DISJOINT_PROPERTIES), ontGraph.SelectTriplesByObject(RDFVocabulary.OWL.ALL_DISJOINT_PROPERTIES).SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)}, //OWL2
                    { nameof(RDFVocabulary.OWL.INVERSE_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INVERSE_OF) },
                    { nameof(RDFVocabulary.OWL.ON_CLASS), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_CLASS) }, //OWL2
                    { nameof(RDFVocabulary.OWL.ON_DATARANGE), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_DATARANGE) }, //OWL2
                    { nameof(RDFVocabulary.OWL.ON_PROPERTY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_PROPERTY) },
                    { nameof(RDFVocabulary.OWL.ONE_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ONE_OF) },
                    { nameof(RDFVocabulary.OWL.UNION_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.UNION_OF) },
                    { nameof(RDFVocabulary.OWL.DISJOINT_UNION_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISJOINT_UNION_OF) }, //OWL2
                    { nameof(RDFVocabulary.OWL.INTERSECTION_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INTERSECTION_OF) },
                    { nameof(RDFVocabulary.OWL.COMPLEMENT_OF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.COMPLEMENT_OF) },
                    { nameof(RDFVocabulary.OWL.ALL_VALUES_FROM), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ALL_VALUES_FROM) },
                    { nameof(RDFVocabulary.OWL.SOME_VALUES_FROM), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SOME_VALUES_FROM) },
                    { nameof(RDFVocabulary.OWL.HAS_SELF), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_SELF) }, //OWL2
                    { nameof(RDFVocabulary.OWL.HAS_VALUE), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_VALUE) },
                    { nameof(RDFVocabulary.OWL.CARDINALITY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.CARDINALITY) },
                    { nameof(RDFVocabulary.OWL.MIN_CARDINALITY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MIN_CARDINALITY) },
                    { nameof(RDFVocabulary.OWL.MAX_CARDINALITY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MAX_CARDINALITY) },
                    { nameof(RDFVocabulary.OWL.QUALIFIED_CARDINALITY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.QUALIFIED_CARDINALITY) }, //OWL2
                    { nameof(RDFVocabulary.OWL.MIN_QUALIFIED_CARDINALITY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MIN_QUALIFIED_CARDINALITY) }, //OWL2
                    { nameof(RDFVocabulary.OWL.MAX_QUALIFIED_CARDINALITY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MAX_QUALIFIED_CARDINALITY) }, //OWL2
                    { nameof(RDFVocabulary.OWL.SAME_AS), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SAME_AS) },
                    { nameof(RDFVocabulary.OWL.DIFFERENT_FROM), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DIFFERENT_FROM) },
                    { nameof(RDFVocabulary.OWL.ALL_DIFFERENT), ontGraph.SelectTriplesByObject(RDFVocabulary.OWL.ALL_DIFFERENT).SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)}, //OWL2
                    { nameof(RDFVocabulary.OWL.MEMBERS), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MEMBERS) },
                    { nameof(RDFVocabulary.OWL.DISTINCT_MEMBERS), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISTINCT_MEMBERS) }, //OWL2
                    { nameof(RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION), ontGraph.SelectTriplesByObject(RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION).SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)}, //OWL2
                    { nameof(RDFVocabulary.OWL.SOURCE_INDIVIDUAL), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SOURCE_INDIVIDUAL) }, //OWL2
                    { nameof(RDFVocabulary.OWL.ASSERTION_PROPERTY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ASSERTION_PROPERTY) }, //OWL2
                    { nameof(RDFVocabulary.OWL.TARGET_VALUE), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.TARGET_VALUE) }, //OWL2
                    { nameof(RDFVocabulary.OWL.TARGET_INDIVIDUAL), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.TARGET_INDIVIDUAL) }, //OWL2
                    { nameof(RDFVocabulary.OWL.AXIOM), ontGraph.SelectTriplesByObject(RDFVocabulary.OWL.AXIOM).SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)}, //OWL2
                    { nameof(RDFVocabulary.OWL.ANNOTATED_SOURCE), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ANNOTATED_SOURCE) }, //OWL2
                    { nameof(RDFVocabulary.OWL.ANNOTATED_PROPERTY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ANNOTATED_PROPERTY) }, //OWL2
                    { nameof(RDFVocabulary.OWL.ANNOTATED_TARGET), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ANNOTATED_TARGET) }, //OWL2
                    { nameof(RDFVocabulary.OWL.NAMED_INDIVIDUAL), ontGraph.SelectTriplesByObject(RDFVocabulary.OWL.NAMED_INDIVIDUAL).SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)}, //OWL2
                    //Annotations
                    { nameof(RDFVocabulary.OWL.VERSION_INFO), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.VERSION_INFO) },
                    { nameof(RDFVocabulary.OWL.VERSION_IRI), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.VERSION_IRI) },
                    { nameof(RDFVocabulary.RDFS.COMMENT), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.COMMENT) },
                    { nameof(RDFVocabulary.RDFS.LABEL), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.LABEL) },
                    { nameof(RDFVocabulary.RDFS.SEE_ALSO), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SEE_ALSO) },
                    { nameof(RDFVocabulary.RDFS.IS_DEFINED_BY), ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.IS_DEFINED_BY) },
                    { nameof(RDFVocabulary.OWL.IMPORTS), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.IMPORTS) },
                    { nameof(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH) },
                    { nameof(RDFVocabulary.OWL.INCOMPATIBLE_WITH), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INCOMPATIBLE_WITH) },
                    { nameof(RDFVocabulary.OWL.PRIOR_VERSION), ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PRIOR_VERSION) }
                };

        /// <summary>
        /// Parses the ontology definition from the given RDF graph
        /// </summary>
        private static void InitializeOntology(RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext, out RDFOntology ontology)
        {
            //Expand with BASE ontology
            ontology = new RDFOntology(new RDFResource(ontGraph.Context.ToString())).UnionWith(RDFBASEOntology.Instance);
            ontology.Value = new RDFResource(ontGraph.Context.ToString());

            if (!prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)ontology.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ONTOLOGY)))
            {
                RDFTriple ontologyTriple = prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.ONTOLOGY).FirstOrDefault();
                if (ontologyTriple != null)
                    ontology.Value = ontologyTriple.Subject;
            }
        }

        /// <summary>
        /// Parses the property model definitions from the given RDF graph
        /// </summary>
        private static void InitializePropertyModel(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            #region Load RDF:Property
            foreach (RDFTriple p in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.RDF.PROPERTY))
                ontology.Model.PropertyModel.AddProperty(((RDFResource)p.Subject).ToRDFOntologyProperty());
            #endregion

            #region Load OWL:AnnotationProperty
            foreach (RDFTriple ap in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.ANNOTATION_PROPERTY))
            {
                RDFOntologyAnnotationProperty aap = ((RDFResource)ap.Subject).ToRDFOntologyAnnotationProperty();
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(aap.PatternMemberID))
                    ontology.Model.PropertyModel.AddProperty(aap);
                else
                    ontology.Model.PropertyModel.Properties[aap.PatternMemberID] = aap;
            }
            #endregion

            #region Load OWL:DatatypeProperty
            foreach (RDFTriple dp in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.DATATYPE_PROPERTY))
            {
                RDFOntologyDatatypeProperty dtp = ((RDFResource)dp.Subject).ToRDFOntologyDatatypeProperty();
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(dtp.PatternMemberID))
                    ontology.Model.PropertyModel.AddProperty(dtp);
                else
                    ontology.Model.PropertyModel.Properties[dtp.PatternMemberID] = dtp;

                #region DeprecatedProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)dtp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                    dtp.SetDeprecated(true);
                #endregion DeprecatedProperty

                #region FunctionalProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)dtp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                    dtp.SetFunctional(true);
                #endregion FunctionalProperty
            }
            #endregion

            #region Load OWL:ObjectProperty
            foreach (RDFTriple op in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.OBJECT_PROPERTY))
            {
                RDFOntologyObjectProperty obp = ((RDFResource)op.Subject).ToRDFOntologyObjectProperty();
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(obp.PatternMemberID))
                    ontology.Model.PropertyModel.AddProperty(obp);
                else
                    ontology.Model.PropertyModel.Properties[obp.PatternMemberID] = obp;

                #region DeprecatedProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                    obp.SetDeprecated(true);
                #endregion

                #region FunctionalProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                    obp.SetFunctional(true);
                #endregion

                #region SymmetricProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.SYMMETRIC_PROPERTY)))
                    obp.SetSymmetric(true);
                #endregion

                #region AsymmetricProperty [OWL2]
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ASYMMETRIC_PROPERTY)))
                    obp.SetAsymmetric(true);
                #endregion

                #region ReflexiveProperty [OWL2]
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.REFLEXIVE_PROPERTY)))
                    obp.SetReflexive(true);
                #endregion

                #region IrreflexiveProperty [OWL2]
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.IRREFLEXIVE_PROPERTY)))
                    obp.SetIrreflexive(true);
                #endregion

                #region TransitiveProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.TRANSITIVE_PROPERTY)))
                    obp.SetTransitive(true);
                #endregion

                #region InverseFunctionalProperty
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY)))
                    obp.SetInverseFunctional(true);
                #endregion
            }

            #region SymmetricProperty
            foreach (RDFTriple sp in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.SYMMETRIC_PROPERTY))
            {
                RDFOntologyProperty syp = ontology.Model.PropertyModel.SelectProperty(sp.Subject.ToString());
                if (syp == null)
                {
                    syp = ((RDFResource)sp.Subject).ToRDFOntologyObjectProperty();
                    ontology.Model.PropertyModel.AddProperty(syp);

                    #region DeprecatedProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)syp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        syp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)syp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        syp.SetFunctional(true);
                    #endregion
                }
                if (syp.IsObjectProperty())
                    ((RDFOntologyObjectProperty)syp).SetSymmetric(true);
            }
            #endregion

            #region AsymmetricProperty [OWL2]
            foreach (RDFTriple ap in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.ASYMMETRIC_PROPERTY))
            {
                RDFOntologyProperty asyp = ontology.Model.PropertyModel.SelectProperty(ap.Subject.ToString());
                if (asyp == null)
                {
                    asyp = ((RDFResource)ap.Subject).ToRDFOntologyObjectProperty();
                    ontology.Model.PropertyModel.AddProperty(asyp);

                    #region DeprecatedProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)asyp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        asyp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)asyp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        asyp.SetFunctional(true);
                    #endregion
                }
                if (asyp.IsObjectProperty())
                    ((RDFOntologyObjectProperty)asyp).SetAsymmetric(true);
            }
            #endregion

            #region ReflexiveProperty [OWL2]
            foreach (RDFTriple rp in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.REFLEXIVE_PROPERTY))
            {
                RDFOntologyProperty refp = ontology.Model.PropertyModel.SelectProperty(rp.Subject.ToString());
                if (refp == null)
                {
                    refp = ((RDFResource)rp.Subject).ToRDFOntologyObjectProperty();
                    ontology.Model.PropertyModel.AddProperty(refp);

                    #region DeprecatedProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)refp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        refp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)refp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        refp.SetFunctional(true);
                    #endregion
                }
                if (refp.IsObjectProperty())
                    ((RDFOntologyObjectProperty)refp).SetReflexive(true);
            }
            #endregion

            #region IrreflexiveProperty [OWL2]
            foreach (RDFTriple irp in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.IRREFLEXIVE_PROPERTY))
            {
                RDFOntologyProperty irrefp = ontology.Model.PropertyModel.SelectProperty(irp.Subject.ToString());
                if (irrefp == null)
                {
                    irrefp = ((RDFResource)irp.Subject).ToRDFOntologyObjectProperty();
                    ontology.Model.PropertyModel.AddProperty(irrefp);

                    #region DeprecatedProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)irrefp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        irrefp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)irrefp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        irrefp.SetFunctional(true);
                    #endregion
                }
                if (irrefp.IsObjectProperty())
                    ((RDFOntologyObjectProperty)irrefp).SetIrreflexive(true);
            }
            #endregion

            #region TransitiveProperty
            foreach (RDFTriple tp in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.TRANSITIVE_PROPERTY))
            {
                RDFOntologyProperty trp = ontology.Model.PropertyModel.SelectProperty(tp.Subject.ToString());
                if (trp == null)
                {
                    trp = ((RDFResource)tp.Subject).ToRDFOntologyObjectProperty();
                    ontology.Model.PropertyModel.AddProperty(trp);

                    #region DeprecatedProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)trp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        trp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)trp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        trp.SetFunctional(true);
                    #endregion
                }
                if (trp.IsObjectProperty())
                    ((RDFOntologyObjectProperty)trp).SetTransitive(true);
            }
            #endregion

            #region InverseFunctionalProperty
            foreach (RDFTriple ip in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY))
            {
                RDFOntologyProperty ifp = ontology.Model.PropertyModel.SelectProperty(ip.Subject.ToString());
                if (ifp == null)
                {
                    ifp = ((RDFResource)ip.Subject).ToRDFOntologyObjectProperty();
                    ontology.Model.PropertyModel.AddProperty(ifp);

                    #region DeprecatedProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)ifp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        ifp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)ifp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        ifp.SetFunctional(true);
                    #endregion
                }
                if (ifp.IsObjectProperty())
                    ((RDFOntologyObjectProperty)ifp).SetInverseFunctional(true);
            }
            #endregion

            #endregion
        }

        /// <summary>
        /// Parses the class model definitions from the given RDF graph
        /// </summary>
        private static void InitializeClassModel(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            #region Load RDFS:Class
            foreach (RDFTriple c in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.RDFS.CLASS))
            {
                RDFOntologyClass rdfsClass = ((RDFResource)c.Subject).ToRDFOntologyClass(RDFSemanticsEnums.RDFOntologyClassNature.RDFS);
                ontology.Model.ClassModel.AddClass(rdfsClass);

                #region DeprecatedClass
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)rdfsClass.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_CLASS)))
                    rdfsClass.SetDeprecated(true);
                #endregion
            }
            #endregion

            #region Load OWL:DataRange
            foreach (RDFTriple dr in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.DATA_RANGE))
            {
                RDFOntologyDataRangeClass datarangeClass = new RDFOntologyDataRangeClass((RDFResource)dr.Subject);
                if (!ontology.Model.ClassModel.Classes.ContainsKey(datarangeClass.PatternMemberID))
                    ontology.Model.ClassModel.AddClass(datarangeClass);
                else
                    ontology.Model.ClassModel.Classes[datarangeClass.PatternMemberID] = datarangeClass;
            }
            #endregion

            #region Load OWL:Class
            foreach (RDFTriple c in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.CLASS))
            {
                RDFOntologyClass owlClass = ((RDFResource)c.Subject).ToRDFOntologyClass();
                if (!ontology.Model.ClassModel.Classes.ContainsKey(owlClass.PatternMemberID))
                    ontology.Model.ClassModel.AddClass(owlClass);
                else
                    ontology.Model.ClassModel.Classes[owlClass.PatternMemberID] = owlClass;

                #region DeprecatedClass
                if (prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].ContainsTriple(new RDFTriple((RDFResource)owlClass.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_CLASS)))
                    owlClass.SetDeprecated(true);
                #endregion
            }
            #endregion

            #region Load OWL:DeprecatedClass
            foreach (RDFTriple dc in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.DEPRECATED_CLASS))
            {
                RDFOntologyClass deprecatedClass = ((RDFResource)dc.Subject).ToRDFOntologyClass().SetDeprecated(true);
                if (!ontology.Model.ClassModel.Classes.ContainsKey(deprecatedClass.PatternMemberID))
                    ontology.Model.ClassModel.AddClass(deprecatedClass);
                else
                    ontology.Model.ClassModel.Classes[deprecatedClass.PatternMemberID].SetDeprecated(true);
            }
            #endregion

            #region Load OWL:Restriction
            foreach (RDFTriple r in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject(RDFVocabulary.OWL.RESTRICTION))
            {
                #region OnProperty
                RDFTriple op = prefetchContext[nameof(RDFVocabulary.OWL.ON_PROPERTY)].SelectTriplesBySubject((RDFResource)r.Subject).FirstOrDefault();
                if (op != null)
                {
                    RDFOntologyProperty onProp = ontology.Model.PropertyModel.SelectProperty(op.Object.ToString());
                    if (onProp != null)
                    {
                        //Ensure to not create a restriction over an annotation property (or a BASE reserved property)
                        if (!onProp.IsAnnotationProperty() && !RDFOntologyChecker.CheckReservedProperty(onProp))
                        {
                            RDFOntologyRestriction restr = new RDFOntologyRestriction((RDFResource)r.Subject, onProp);
                            if (!ontology.Model.ClassModel.Classes.ContainsKey(restr.PatternMemberID))
                                ontology.Model.ClassModel.AddClass(restr);
                            else
                                ontology.Model.ClassModel.Classes[restr.PatternMemberID] = restr;
                        }
                        else
                        {
                            //Raise warning event to inform the user: restriction cannot be imported from graph, because its applied property is reserved and cannot be restricted
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Restriction '{0}' cannot be imported from graph, because its applied property '{1}' represents an annotation property or is a reserved BASE property.", r.Subject, op.Object));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: restriction cannot be imported from graph, because definition of its applied property is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Restriction '{0}' cannot be imported from graph, because definition of its applied property '{1}' is not found in the model.", r.Subject, op.Object));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: restriction cannot be imported from graph, because owl:OnProperty triple is not found in the graph
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Restriction '{0}' cannot be imported from graph, because owl:OnProperty triple is not found in the graph.", r.Subject));
                }
                #endregion
            }
            #endregion

            #region Load OWL:[UnionOf|DisjointUnionOf|IntersectionOf|ComplementOf]

            #region Union
            foreach (RDFTriple u in prefetchContext[nameof(RDFVocabulary.OWL.UNION_OF)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                #region Initialize
                RDFOntologyClass uc = ontology.Model.ClassModel.SelectClass(u.Subject.ToString());
                if (uc == null)
                {
                    uc = new RDFOntologyClass(new RDFResource(u.Subject.ToString()));
                    ontology.Model.ClassModel.AddClass(uc);
                }
                #endregion

                #region ClassToUnionClass
                if (!(uc is RDFOntologyUnionClass))
                {
                    uc = new RDFOntologyUnionClass((RDFResource)u.Subject);
                    ontology.Model.ClassModel.Classes[uc.PatternMemberID] = uc;
                }
                #endregion

                #region DeserializeUnionCollection
                bool nilFound = false;
                RDFResource itemRest = (RDFResource)u.Object;
                HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                while (!nilFound)
                {
                    #region rdf:first
                    RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        RDFOntologyClass compClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                        if (compClass != null)
                            ontology.Model.ClassModel.AddUnionOfRelation((RDFOntologyUnionClass)uc, new List<RDFOntologyClass>() { compClass });
                        else
                        {
                            //Raise warning event to inform the user: union class cannot be completely imported from graph, because definition of its compositing class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("UnionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", u.Subject, first.Object));
                        }

                        #region rdf:rest
                        RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (rest != null)
                        {
                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                nilFound = true;
                            else
                            {
                                itemRest = (RDFResource)rest.Object;
                                //Avoid bad-formed cyclic lists to generate infinite loops
                                if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                    itemRestVisitCache.Add(itemRest.PatternMemberID);
                                else
                                    nilFound = true;
                            }
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    else
                    {
                        nilFound = true;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region DisjointUnion [OWL2]
            foreach (RDFTriple du in prefetchContext[nameof(RDFVocabulary.OWL.DISJOINT_UNION_OF)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                #region Initialize
                RDFOntologyClass duc = ontology.Model.ClassModel.SelectClass(du.Subject.ToString());
                if (duc == null)
                {
                    duc = new RDFOntologyClass(new RDFResource(du.Subject.ToString()));
                    ontology.Model.ClassModel.AddClass(duc);
                }
                #endregion

                #region ClassToUnionClass
                if (!(duc is RDFOntologyUnionClass))
                {
                    duc = new RDFOntologyUnionClass((RDFResource)du.Subject);
                    ontology.Model.ClassModel.Classes[duc.PatternMemberID] = duc;
                }
                #endregion

                #region DeserializeUnionCollection
                bool nilFound = false;
                RDFResource itemRest = (RDFResource)du.Object;
                HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                List<RDFOntologyClass> disjointClasses = new List<RDFOntologyClass>();
                while (!nilFound)
                {
                    #region rdf:first
                    RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        RDFOntologyClass compClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                        if (compClass != null)
                        {
                            ontology.Model.ClassModel.AddUnionOfRelation((RDFOntologyUnionClass)duc, new List<RDFOntologyClass>() { compClass });
                            disjointClasses.Add(compClass);
                        }
                        else
                        {
                            //Raise warning event to inform the user: union class cannot be completely imported from graph, because definition of its compositing class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("UnionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", du.Subject, first.Object));
                        }

                        #region rdf:rest
                        RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (rest != null)
                        {
                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                nilFound = true;
                            else
                            {
                                itemRest = (RDFResource)rest.Object;
                                //Avoid bad-formed cyclic lists to generate infinite loops
                                if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                    itemRestVisitCache.Add(itemRest.PatternMemberID);
                                else
                                    nilFound = true;
                            }
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    else
                    {
                        nilFound = true;
                    }
                    #endregion
                }
                #endregion

                #region DisjointClasses
                disjointClasses.ForEach(outerClass =>
                    disjointClasses.ForEach(innerClass => ontology.Model.ClassModel.AddDisjointWithRelation(outerClass, innerClass)));
                #endregion
            }
            #endregion

            #region Intersection
            foreach (RDFTriple i in prefetchContext[nameof(RDFVocabulary.OWL.INTERSECTION_OF)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                #region Initialize
                RDFOntologyClass ic = ontology.Model.ClassModel.SelectClass(i.Subject.ToString());
                if (ic == null)
                {
                    ic = new RDFOntologyClass(new RDFResource(i.Subject.ToString()));
                    ontology.Model.ClassModel.AddClass(ic);
                }
                #endregion

                #region ClassToIntersectionClass
                if (!(ic is RDFOntologyIntersectionClass))
                {
                    ic = new RDFOntologyIntersectionClass((RDFResource)i.Subject);
                    ontology.Model.ClassModel.Classes[ic.PatternMemberID] = ic;
                }
                #endregion

                #region DeserializeIntersectionCollection
                bool nilFound = false;
                RDFResource itemRest = (RDFResource)i.Object;
                HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                while (!nilFound)
                {
                    #region rdf:first
                    RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        RDFOntologyClass compClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                        if (compClass != null)
                            ontology.Model.ClassModel.AddIntersectionOfRelation((RDFOntologyIntersectionClass)ic, new List<RDFOntologyClass>() { compClass });
                        else
                        {
                            //Raise warning event to inform the user: intersection class cannot be completely imported from graph, because definition of its compositing class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("IntersectionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", i.Subject, first.Object));
                        }

                        #region rdf:rest
                        RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (rest != null)
                        {
                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                nilFound = true;
                            else
                            {
                                itemRest = (RDFResource)rest.Object;
                                //Avoid bad-formed cyclic lists to generate infinite loops
                                if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                    itemRestVisitCache.Add(itemRest.PatternMemberID);
                                else
                                    nilFound = true;
                            }
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    else
                    {
                        nilFound = true;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region Complement
            foreach (RDFTriple c in prefetchContext[nameof(RDFVocabulary.OWL.COMPLEMENT_OF)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFOntologyClass cc = ontology.Model.ClassModel.SelectClass(c.Subject.ToString());
                if (cc != null)
                {
                    RDFOntologyClass compClass = ontology.Model.ClassModel.SelectClass(c.Object.ToString());
                    if (compClass != null)
                    {
                        cc = new RDFOntologyComplementClass((RDFResource)c.Subject, compClass);
                        ontology.Model.ClassModel.Classes[cc.PatternMemberID] = cc;
                    }
                    else
                    {
                        //Raise warning event to inform the user: complement class cannot be imported from graph, because definition of its complemented class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Class '{0}' cannot be imported from graph, because definition of its complement class '{1}' is not found in the model.", c.Subject, c.Object));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: complement class cannot be imported from graph, because its definition is not found in the model
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Class '{0}' cannot be imported from graph, because its definition is not found in the model.", c.Subject));
                }
            }
            #endregion

            #endregion
        }

        /// <summary>
        /// Parses the data definitions from the given RDF graph
        /// </summary>
        private static void InitializeData(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            RDFOntologyObjectProperty rdfTypeProperty = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();

            //Detect named individuals [OWL2]
            RDFOntologyClass namedIndividualClass = RDFVocabulary.OWL.NAMED_INDIVIDUAL.ToRDFOntologyClass();
            foreach (RDFTriple namedIndividual in prefetchContext[nameof(RDFVocabulary.OWL.NAMED_INDIVIDUAL)].Where(ni => !((RDFResource)ni.Subject).IsBlank))
            {
                RDFOntologyFact fact = ontology.Data.SelectFact(namedIndividual.Subject.ToString());
                if (fact == null)
                {
                    fact = ((RDFResource)namedIndividual.Subject).ToRDFOntologyFact();
                    ontology.Data.AddFact(fact);
                }
                ontology.Data.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(fact, rdfTypeProperty, namedIndividualClass));
            }

            //Detect classtyped individuals
            List<RDFOntologyClass> simpleClasses = ontology.Model.ClassModel.Where(cls => !RDFOntologyChecker.CheckReservedClass(cls) && cls.IsSimpleClass()).ToList();
            foreach (RDFOntologyClass simpleClass in simpleClasses)
            {
                foreach (RDFTriple classtypeTriple in prefetchContext[nameof(RDFVocabulary.RDF.TYPE)].SelectTriplesByObject((RDFResource)simpleClass.Value))
                {
                    RDFOntologyFact fact = ontology.Data.SelectFact(classtypeTriple.Subject.ToString());
                    if (fact == null)
                        fact = ((RDFResource)classtypeTriple.Subject).ToRDFOntologyFact();

                    ontology.Data.AddClassTypeRelation(fact, simpleClass);
                }
            }
        }

        /// <summary>
        /// Finalizes the definition of the ontology restrictions previously detected
        /// </summary>
        private static void FinalizeRestrictions(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            foreach (RDFOntologyClass restriction in ontology.Model.ClassModel.Where(rst => rst.IsRestrictionClass()).ToList())
            {
                #region Cardinality
                int exC = 0;
                RDFTriple crEx = prefetchContext[nameof(RDFVocabulary.OWL.CARDINALITY)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (crEx != null && crEx.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                {
                    if (crEx.Object is RDFPlainLiteral)
                    {
                        if (NumberRegex.IsMatch(crEx.Object.ToString()))
                            exC = int.Parse(crEx.Object.ToString());
                    }
                    else
                    {
                        if (((RDFTypedLiteral)crEx.Object).HasDecimalDatatype())
                        {
                            if (NumberRegex.IsMatch(((RDFTypedLiteral)crEx.Object).Value))
                                exC = int.Parse(((RDFTypedLiteral)crEx.Object).Value);
                        }
                    }
                }

                if (exC > 0)
                {
                    RDFOntologyCardinalityRestriction cardRestr = new RDFOntologyCardinalityRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, exC, exC);
                    ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = cardRestr;
                    continue; //Restriction has been successfully typed
                }
                #endregion

                #region MinMaxCardinality
                int minC = 0;
                RDFTriple crMin = prefetchContext[nameof(RDFVocabulary.OWL.MIN_CARDINALITY)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (crMin != null && crMin.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                {
                    if (crMin.Object is RDFPlainLiteral)
                    {
                        if (NumberRegex.IsMatch(crMin.Object.ToString()))
                            minC = int.Parse(crMin.Object.ToString());
                    }
                    else
                    {
                        if (((RDFTypedLiteral)crMin.Object).HasDecimalDatatype())
                        {
                            if (NumberRegex.IsMatch(((RDFTypedLiteral)crMin.Object).Value))
                                minC = int.Parse(((RDFTypedLiteral)crMin.Object).Value);
                        }
                    }
                }

                int maxC = 0;
                RDFTriple crMax = prefetchContext[nameof(RDFVocabulary.OWL.MAX_CARDINALITY)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (crMax != null && crMax.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                {
                    if (crMax.Object is RDFPlainLiteral)
                    {
                        if (NumberRegex.IsMatch(crMax.Object.ToString()))
                            maxC = int.Parse(crMax.Object.ToString());
                    }
                    else
                    {
                        if (((RDFTypedLiteral)crMax.Object).HasDecimalDatatype())
                        {
                            if (NumberRegex.IsMatch(((RDFTypedLiteral)crMax.Object).Value))
                                maxC = int.Parse(((RDFTypedLiteral)crMax.Object).Value);
                        }
                    }
                }

                if (minC > 0 || maxC > 0)
                {
                    RDFOntologyCardinalityRestriction cardRestr = new RDFOntologyCardinalityRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, minC, maxC);
                    ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = cardRestr;
                    continue; //Restriction has been successfully typed
                }
                #endregion

                #region QualifiedCardinality [OWL2]
                int exQC = 0;
                RDFTriple crExQC = prefetchContext[nameof(RDFVocabulary.OWL.QUALIFIED_CARDINALITY)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (crExQC != null && crExQC.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                {
                    if (crExQC.Object is RDFPlainLiteral)
                    {
                        if (NumberRegex.IsMatch(crExQC.Object.ToString()))
                            exQC = int.Parse(crExQC.Object.ToString());
                    }
                    else
                    {
                        if (((RDFTypedLiteral)crExQC.Object).HasDecimalDatatype())
                        {
                            if (NumberRegex.IsMatch(((RDFTypedLiteral)crExQC.Object).Value))
                                exQC = int.Parse(((RDFTypedLiteral)crExQC.Object).Value);
                        }
                    }
                }

                if (exQC > 0)
                {
                    //OnClass
                    RDFTriple exQCCls = prefetchContext[nameof(RDFVocabulary.OWL.ON_CLASS)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                    if (exQCCls != null && exQCCls.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        RDFOntologyClass exQCOnClass = ontology.Model.ClassModel.SelectClass(exQCCls.Object.ToString());
                        if (exQCOnClass != null)
                        {
                            RDFOntologyQualifiedCardinalityRestriction qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, exQCOnClass, exQC, exQC);
                            ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = qualifCardRestr;
                            continue; //Restriction has been successfully typed
                        }
                        else
                        {
                            //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onClass is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onClass '{1}' is not found in the model.", restriction.Value, exQCCls.Object));
                        }
                    }
                    else
                    {
                        //OnDataRange
                        RDFTriple exQCDrn = prefetchContext[nameof(RDFVocabulary.OWL.ON_DATARANGE)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                        if (exQCDrn != null && exQCDrn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            RDFOntologyClass exQCOnDataRange = ontology.Model.ClassModel.SelectClass(exQCDrn.Object.ToString());
                            if (exQCOnDataRange != null)
                            {
                                RDFOntologyQualifiedCardinalityRestriction qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, exQCOnDataRange, exQC, exQC);
                                ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = qualifCardRestr;
                                continue; //Restriction has been successfully typed
                            }
                            else
                            {
                                //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onDataRange is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onDataRange '{1}' is not found in the model.", restriction.Value, exQCDrn.Object));
                            }
                        }
                    }
                }
                #endregion

                #region MinMaxQualifiedCardinality [OWL2]
                int minQC = 0;
                RDFTriple crMinQC = prefetchContext[nameof(RDFVocabulary.OWL.MIN_QUALIFIED_CARDINALITY)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (crMinQC != null && crMinQC.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                {
                    if (crMinQC.Object is RDFPlainLiteral)
                    {
                        if (NumberRegex.IsMatch(crMinQC.Object.ToString()))
                            minQC = int.Parse(crMinQC.Object.ToString());
                    }
                    else
                    {
                        if (((RDFTypedLiteral)crMinQC.Object).HasDecimalDatatype())
                        {
                            if (NumberRegex.IsMatch(((RDFTypedLiteral)crMinQC.Object).Value))
                                minQC = int.Parse(((RDFTypedLiteral)crMinQC.Object).Value);
                        }
                    }
                }

                int maxQC = 0;
                RDFTriple crMaxQC = prefetchContext[nameof(RDFVocabulary.OWL.MAX_QUALIFIED_CARDINALITY)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (crMaxQC != null && crMaxQC.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                {
                    if (crMaxQC.Object is RDFPlainLiteral)
                    {
                        if (NumberRegex.IsMatch(crMaxQC.Object.ToString()))
                            maxQC = int.Parse(crMaxQC.Object.ToString());
                    }
                    else
                    {
                        if (((RDFTypedLiteral)crMaxQC.Object).HasDecimalDatatype())
                        {
                            if (NumberRegex.IsMatch(((RDFTypedLiteral)crMaxQC.Object).Value))
                                maxQC = int.Parse(((RDFTypedLiteral)crMaxQC.Object).Value);
                        }
                    }
                }

                if (minQC > 0 || maxQC > 0)
                {
                    //OnClass
                    RDFTriple minmaxQCCls = prefetchContext[nameof(RDFVocabulary.OWL.ON_CLASS)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                    if (minmaxQCCls != null && minmaxQCCls.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        RDFOntologyClass minmaxQCOnClass = ontology.Model.ClassModel.SelectClass(minmaxQCCls.Object.ToString());
                        if (minmaxQCOnClass != null)
                        {
                            RDFOntologyQualifiedCardinalityRestriction qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, minmaxQCOnClass, minQC, maxQC);
                            ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = qualifCardRestr;
                            continue; //Restriction has been successfully typed
                        }
                        else
                        {
                            //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onClass is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onClass '{1}' is not found in the model.", restriction.Value, minmaxQCCls.Object));
                        }
                    }
                    else
                    {
                        //OnDataRange
                        RDFTriple minmaxQCDrn = prefetchContext[nameof(RDFVocabulary.OWL.ON_DATARANGE)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                        if (minmaxQCDrn != null && minmaxQCDrn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            RDFOntologyClass minmaxQCOnDataRange = ontology.Model.ClassModel.SelectClass(minmaxQCDrn.Object.ToString());
                            if (minmaxQCOnDataRange != null)
                            {
                                RDFOntologyQualifiedCardinalityRestriction qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, minmaxQCOnDataRange, minQC, maxQC);
                                ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = qualifCardRestr;
                                continue; //Restriction has been successfully typed
                            }
                            else
                            {
                                //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onDataRange is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onDataRange '{1}' is not found in the model.", restriction.Value, minmaxQCDrn.Object));
                            }
                        }
                    }
                }
                #endregion

                #region HasSelf [OWL2]
                RDFTriple hsRes = prefetchContext[nameof(RDFVocabulary.OWL.HAS_SELF)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (hsRes?.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL
                        && hsRes.Object.Equals(RDFTypedLiteral.True))
                {
                    RDFOntologyHasSelfRestriction hasselfRestr = new RDFOntologyHasSelfRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty);
                    ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = hasselfRestr;
                    continue; //Restriction has been successfully typed
                }
                #endregion

                #region HasValue
                RDFTriple hvRes = prefetchContext[nameof(RDFVocabulary.OWL.HAS_VALUE)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (hvRes != null)
                {
                    if (hvRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        RDFOntologyHasValueRestriction hasvalueRestr = new RDFOntologyHasValueRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, ((RDFResource)hvRes.Object).ToRDFOntologyFact());
                        ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = hasvalueRestr;
                        continue; //Restriction has been successfully typed
                    }
                    else
                    {
                        RDFOntologyHasValueRestriction hasvalueRestr = new RDFOntologyHasValueRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, ((RDFLiteral)hvRes.Object).ToRDFOntologyLiteral());
                        ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = hasvalueRestr;
                        continue; //Restriction has been successfully typed
                    }
                }
                #endregion

                #region AllValuesFrom
                RDFTriple avfRes = prefetchContext[nameof(RDFVocabulary.OWL.ALL_VALUES_FROM)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (avfRes?.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    RDFOntologyClass avfCls = ontology.Model.ClassModel.SelectClass(avfRes.Object.ToString());
                    if (avfCls != null)
                    {
                        RDFOntologyAllValuesFromRestriction allvaluesfromRestr = new RDFOntologyAllValuesFromRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, avfCls);
                        ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = allvaluesfromRestr;
                        continue; //Restriction has been successfully typed
                    }
                    else
                    {
                        //Raise warning event to inform the user: allvaluesfrom restriction cannot be imported from graph, because definition of its required class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllValuesFromRestriction '{0}' cannot be imported from graph, because definition of its required class '{1}' is not found in the model.", restriction.Value, avfRes.Object));
                    }
                }
                #endregion

                #region SomeValuesFrom
                RDFTriple svfRes = prefetchContext[nameof(RDFVocabulary.OWL.SOME_VALUES_FROM)].SelectTriplesBySubject((RDFResource)restriction.Value).FirstOrDefault();
                if (svfRes?.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    RDFOntologyClass svfCls = ontology.Model.ClassModel.SelectClass(svfRes.Object.ToString());
                    if (svfCls != null)
                    {
                        RDFOntologySomeValuesFromRestriction somevaluesfromRestr = new RDFOntologySomeValuesFromRestriction((RDFResource)restriction.Value, ((RDFOntologyRestriction)restriction).OnProperty, svfCls);
                        ontology.Model.ClassModel.Classes[restriction.PatternMemberID] = somevaluesfromRestr;
                        continue; //Restriction has been successfully typed
                    }
                    else
                    {
                        //Raise warning event to inform the user: somevaluesfrom restriction cannot be imported from graph, because definition of its required class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SomeValuesFromRestriction '{0}' cannot be imported from graph, because definition of its required class '{1}' is not found in the model.", restriction.Value, svfRes.Object));
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Finalizes the definition of the ontology enumerate/datarange classes previously detected
        /// </summary>
        private static void FinalizeEnumeratesAndDataRanges(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            #region OWL:OneOf (Enumerate)
            foreach (RDFTriple e in prefetchContext[nameof(RDFVocabulary.OWL.ONE_OF)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFOntologyClass ec = ontology.Model.ClassModel.SelectClass(e.Subject.ToString());
                if (ec != null && !ec.IsDataRangeClass())
                {
                    #region ClassToEnumerateClass
                    if (!ec.IsEnumerateClass())
                    {
                        ec = new RDFOntologyEnumerateClass((RDFResource)e.Subject);
                        ontology.Model.ClassModel.Classes[ec.PatternMemberID] = ec;
                    }
                    #endregion ClassToEnumerateClass

                    #region DeserializeEnumerateCollection
                    bool nilFound = false;
                    RDFResource itemRest = (RDFResource)e.Object;
                    HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                    while (!nilFound)
                    {
                        #region rdf:first
                        RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            ontology.Model.ClassModel.AddOneOfRelation((RDFOntologyEnumerateClass)ec, new List<RDFOntologyFact>() { ((RDFResource)first.Object).ToRDFOntologyFact() });

                            #region rdf:rest
                            RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                            if (rest != null)
                            {
                                if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                    nilFound = true;
                                else
                                {
                                    itemRest = (RDFResource)rest.Object;
                                    //Avoid bad-formed cyclic lists to generate infinite loops
                                    if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                        itemRestVisitCache.Add(itemRest.PatternMemberID);
                                    else
                                        nilFound = true;
                                }
                            }
                            else
                            {
                                nilFound = true;
                            }
                            #endregion
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (ec == null)
                {
                    //Raise warning event to inform the user: enumerate class cannot be imported from graph, because its definition is not found in the model
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EnumerateClass '{0}' cannot be imported from graph, because its definition is not found in the model.", e.Subject));
                }
            }
            #endregion

            #region OWL:OneOf (DataRange)
            foreach (RDFTriple d in prefetchContext[nameof(RDFVocabulary.OWL.ONE_OF)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFOntologyClass dr = ontology.Model.ClassModel.SelectClass(d.Subject.ToString());
                if (dr != null && !dr.IsEnumerateClass())
                {
                    #region ClassToDataRangeClass
                    if (!dr.IsDataRangeClass())
                    {
                        dr = new RDFOntologyDataRangeClass((RDFResource)d.Subject);
                        ontology.Model.ClassModel.Classes[dr.PatternMemberID] = dr;
                    }
                    #endregion

                    #region DeserializeDataRangeCollection
                    bool nilFound = false;
                    RDFResource itemRest = (RDFResource)d.Object;
                    HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                    while (!nilFound)
                    {
                        #region rdf:first
                        RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.ClassModel.AddOneOfRelation((RDFOntologyDataRangeClass)dr, new List<RDFOntologyLiteral>() { ((RDFLiteral)first.Object).ToRDFOntologyLiteral() });

                            #region rdf:rest
                            RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                            if (rest != null)
                            {
                                if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                    nilFound = true;
                                else
                                {
                                    itemRest = (RDFResource)rest.Object;
                                    //Avoid bad-formed cyclic lists to generate infinite loops
                                    if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                        itemRestVisitCache.Add(itemRest.PatternMemberID);
                                    else
                                        nilFound = true;
                                }
                            }
                            else
                            {
                                nilFound = true;
                            }
                            #endregion
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    #endregion
                }
                else if (dr == null)
                {
                    //Raise warning event to inform the user: datarange class cannot be imported from graph, because its definition is not found in the model
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DataRangeClass '{0}' cannot be imported from graph, because its definition is not found in the model.", d.Subject));
                }
            }
            #endregion
        }

        /// <summary>
        /// Finalizes the property model definitions
        /// </summary>
        private static void FinalizePropertyModel(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            foreach (RDFOntologyProperty evp in ontology.Model.PropertyModel.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop) && !prop.IsAnnotationProperty()).ToList())
            {
                #region Domain
                RDFTriple d = prefetchContext[nameof(RDFVocabulary.RDFS.DOMAIN)].SelectTriplesBySubject((RDFResource)evp.Value).FirstOrDefault();
                if (d != null && d.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    RDFOntologyClass domainClass = ontology.Model.ClassModel.SelectClass(d.Object.ToString());
                    if (domainClass != null)
                        evp.SetDomain(domainClass);
                    else
                    {
                        //Raise warning event to inform the user: domain constraint cannot be imported from graph because definition of required class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Domain constraint on property '{0}' cannot be imported from graph because definition of required class '{1}' is not found in the model.", evp.Value, d.Object));
                    }
                }
                #endregion

                #region Range
                RDFTriple r = prefetchContext[nameof(RDFVocabulary.RDFS.RANGE)].SelectTriplesBySubject((RDFResource)evp.Value).FirstOrDefault();
                if (r != null && r.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    RDFOntologyClass rangeClass = ontology.Model.ClassModel.SelectClass(r.Object.ToString());
                    if (rangeClass != null)
                        evp.SetRange(rangeClass);
                    else
                    {
                        //Raise warning event to inform the user: range constraint cannot be imported from graph because definition of required class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Range constraint on property '{0}' cannot be imported from graph because definition of required class '{1}' is not found in the model.", evp.Value, r.Object));
                    }
                }
                #endregion

                #region SubPropertyOf
                foreach (RDFTriple spof in prefetchContext[nameof(RDFVocabulary.RDFS.SUB_PROPERTY_OF)].SelectTriplesBySubject((RDFResource)evp.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyProperty superProp = ontology.Model.PropertyModel.SelectProperty(spof.Object.ToString());
                    if (superProp != null)
                    {
                        if (evp.IsObjectProperty() && superProp.IsObjectProperty())
                            ontology.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)evp, (RDFOntologyObjectProperty)superProp);
                        else if (evp.IsDatatypeProperty() && superProp.IsDatatypeProperty())
                            ontology.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)evp, (RDFOntologyDatatypeProperty)superProp);
                        else
                        {
                            //Raise warning event to inform the user: subpropertyof relation cannot be imported from graph because both properties must be explicitly typed as object or datatype properties
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubPropertyOf relation between properties '{0}' and '{1}' cannot be imported from graph because both properties must be explicitly typed as object or datatype properties.", evp.Value, spof.Object));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: subpropertyof relation cannot be imported from graph because definition of property is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubPropertyOf relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model or represents an annotation property.", evp.Value, spof.Object));
                    }
                }
                #endregion

                #region EquivalentProperty
                foreach (RDFTriple eqpr in prefetchContext[nameof(RDFVocabulary.OWL.EQUIVALENT_PROPERTY)].SelectTriplesBySubject((RDFResource)evp.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyProperty equivProp = ontology.Model.PropertyModel.SelectProperty(eqpr.Object.ToString());
                    if (equivProp != null)
                    {
                        if (evp.IsObjectProperty() && equivProp.IsObjectProperty())
                            ontology.Model.PropertyModel.AddEquivalentPropertyRelation((RDFOntologyObjectProperty)evp, (RDFOntologyObjectProperty)equivProp);
                        else if (evp.IsDatatypeProperty() && equivProp.IsDatatypeProperty())
                            ontology.Model.PropertyModel.AddEquivalentPropertyRelation((RDFOntologyDatatypeProperty)evp, (RDFOntologyDatatypeProperty)equivProp);
                        else
                        {
                            //Raise warning event to inform the user: equivalentproperty relation cannot be imported from graph because both properties must be explicitly typed as object or datatype properties
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentProperty relation between properties '{0}' and '{1}' cannot be imported from graph because both properties must be explicitly typed as object or datatype properties.", evp.Value, eqpr.Object));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: equivalentproperty relation cannot be imported from graph, because definition of property is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentProperty relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model.", evp.Value, eqpr.Object));
                    }
                }
                #endregion

                #region InverseOf
                if (evp.IsObjectProperty())
                {
                    foreach (RDFTriple inof in prefetchContext[nameof(RDFVocabulary.OWL.INVERSE_OF)].SelectTriplesBySubject((RDFResource)evp.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                    {
                        RDFOntologyProperty invProp = ontology.Model.PropertyModel.SelectProperty(inof.Object.ToString());
                        if (invProp != null && invProp.IsObjectProperty())
                            ontology.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)evp, (RDFOntologyObjectProperty)invProp);
                        else
                        {
                            //Raise warning event to inform the user: inverseof relation cannot be imported from graph because definition of property is not found in the model, or it does not represent an object property
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("InverseOf relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model, or it does not represent an object property.", evp.Value, inof.Object));
                        }
                    }
                }
                #endregion

                #region PropertyDisjointWith [OWL2]
                foreach (RDFTriple dwpr in prefetchContext[nameof(RDFVocabulary.OWL.PROPERTY_DISJOINT_WITH)].SelectTriplesBySubject((RDFResource)evp.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyProperty disjProp = ontology.Model.PropertyModel.SelectProperty(dwpr.Object.ToString());
                    if (disjProp != null)
                    {
                        if (evp.IsObjectProperty() && disjProp.IsObjectProperty())
                            ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyObjectProperty)evp, (RDFOntologyObjectProperty)disjProp);
                        else if (evp.IsDatatypeProperty() && disjProp.IsDatatypeProperty())
                            ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyDatatypeProperty)evp, (RDFOntologyDatatypeProperty)disjProp);
                        else
                        {
                            //Raise warning event to inform the user: propertyDisjointWith relation cannot be imported from graph because both properties must be explicitly typed as object or datatype properties
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyDisjointWith relation between properties '{0}' and '{1}' cannot be imported from graph because both properties must be explicitly typed as object or datatype properties.", evp.Value, dwpr.Object));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: propertyDisjointWith relation cannot be imported from graph because definition of property is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyDisjointWith relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model.", evp.Value, dwpr.Object));
                    }
                }
                #endregion

                #region AllDisjointProperties [OWL2]
                foreach (RDFTriple adjp in prefetchContext[nameof(RDFVocabulary.OWL.ALL_DISJOINT_PROPERTIES)])
                {
                    List<RDFOntologyProperty> allDisjointProperties = new List<RDFOntologyProperty>();
                    foreach (RDFTriple adjpMembers in prefetchContext[nameof(RDFVocabulary.OWL.MEMBERS)].SelectTriplesBySubject((RDFResource)adjp.Subject).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                    {
                        #region DeserializeCollection
                        bool nilFound = false;
                        RDFResource itemRest = (RDFResource)adjpMembers.Object;
                        HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                        while (!nilFound)
                        {
                            #region rdf:first
                            RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                            if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                RDFOntologyProperty disjointProperty = ontology.Model.PropertyModel.SelectProperty(first.Object.ToString());
                                if (disjointProperty != null)
                                    allDisjointProperties.Add(disjointProperty);
                                else
                                {
                                    //Raise warning event to inform the user: all disjoint properties cannot be completely imported from graph because definition of property is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllDisjointProperties '{0}' cannot be completely imported from graph because definition of property '{1}' is not found in the model.", adjp.Subject, first.Object));
                                }

                                #region rdf:rest
                                RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                                if (rest != null)
                                {
                                    if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        nilFound = true;
                                    else
                                    {
                                        itemRest = (RDFResource)rest.Object;
                                        //Avoid bad-formed cyclic lists to generate infinite loops
                                        if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                            itemRestVisitCache.Add(itemRest.PatternMemberID);
                                        else
                                            nilFound = true;
                                    }
                                }
                                else
                                {
                                    nilFound = true;
                                }
                                #endregion
                            }
                            else
                            {
                                nilFound = true;
                            }
                            #endregion
                        }
                        #endregion
                    }

                    //Add pairs of disjointPropertyWith relations
                    foreach (RDFOntologyProperty outerProperty in allDisjointProperties)
                    {
                        foreach (RDFOntologyProperty innerProperty in allDisjointProperties)
                        {
                            if (outerProperty.IsObjectProperty() && innerProperty.IsObjectProperty())
                                ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyObjectProperty)outerProperty, (RDFOntologyObjectProperty)innerProperty);
                            else if (outerProperty.IsDatatypeProperty() && innerProperty.IsDatatypeProperty())
                                ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyDatatypeProperty)outerProperty, (RDFOntologyDatatypeProperty)innerProperty);
                            else
                            {
                                //Raise warning event to inform the user: propertyDisjointWith relation cannot be imported from graph because both properties must be explicitly typed as object or datatype properties
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyDisjointWith relation between properties '{0}' and '{1}' cannot be imported from graph because both properties must be explicitly typed as object or datatype properties.", outerProperty, innerProperty));
                            }
                        }
                    }
                }
                #endregion

                #region PropertyChainAxiom [OWL2]
                foreach (RDFTriple pca in prefetchContext[nameof(RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM)].SelectTriplesBySubject((RDFResource)evp.Value))
                {
                    RDFOntologyProperty pcaProperty = ontology.Model.PropertyModel.SelectProperty(pca.Subject.ToString());
                    if (pcaProperty != null && pcaProperty is RDFOntologyObjectProperty)
                    {
                        List<RDFOntologyObjectProperty> pcaProperties = new List<RDFOntologyObjectProperty>();
                        if (pca.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            #region DeserializeCollection
                            bool nilFound = false;
                            RDFResource itemRest = (RDFResource)pca.Object;
                            HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                            while (!nilFound)
                            {
                                #region rdf:first
                                RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                                if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {
                                    RDFOntologyProperty pcaProp = ontology.Model.PropertyModel.SelectProperty(first.Object.ToString());
                                    if (pcaProp != null)
                                    {
                                        if (pcaProp is RDFOntologyObjectProperty)
                                            pcaProperties.Add((RDFOntologyObjectProperty)pcaProp);
                                        else
                                        {
                                            //Raise warning event to inform the user: PropertyChainAxiom cannot be completely imported from graph, because chain property is not an object property
                                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyChainAxiom relation '{0}' cannot be completely imported from graph, because chain property '{1}' is not an object property.", pca, first.Object));
                                        }
                                    }
                                    else
                                    {
                                        //Raise warning event to inform the user: PropertyChainAxiom cannot be completely imported from graph, because definition of chain property is not found in the model
                                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyChainAxiom relation '{0}' cannot be completely imported from graph, because definition of chain property '{1}' is not found in the model.", pca, first.Object));
                                    }

                                    #region rdf:rest
                                    RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                                    if (rest != null)
                                    {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                            nilFound = true;
                                        else
                                        {
                                            itemRest = (RDFResource)rest.Object;
                                            //Avoid bad-formed cyclic lists to generate infinite loops
                                            if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                                itemRestVisitCache.Add(itemRest.PatternMemberID);
                                            else
                                                nilFound = true;
                                        }
                                    }
                                    else
                                    {
                                        nilFound = true;
                                    }
                                    #endregion
                                }
                                else
                                {
                                    nilFound = true;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        ontology.Model.PropertyModel.AddPropertyChainAxiomRelation((RDFOntologyObjectProperty)pcaProperty, pcaProperties);
                    }
                    else
                    {
                        //Raise warning event to inform the user: PropertyChainAxiom relation cannot be imported from graph, because definition of property is not found in the model, or it does not represent an object property
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyChainAxiom relation on property '{0}' cannot be imported from graph, because definition of the property is not found in the model, or it does not represent an object property.", pca.Subject));
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Finalizes the class model definitions
        /// </summary>
        private static void FinalizeClassModel(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            List<RDFOntologyClass> simpleClasses = ontology.Model.ClassModel.Where(cls => !RDFOntologyChecker.CheckReservedClass(cls) && cls.IsSimpleClass()).ToList();
            foreach (RDFOntologyClass evc in simpleClasses)
            {
                #region SubClassOf
                foreach (RDFTriple scof in prefetchContext[nameof(RDFVocabulary.RDFS.SUB_CLASS_OF)].SelectTriplesBySubject((RDFResource)evc.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyClass superClass = ontology.Model.ClassModel.SelectClass(scof.Object.ToString());
                    if (superClass != null)
                        ontology.Model.ClassModel.AddSubClassOfRelation(evc, superClass);
                    else
                    {
                        //Raise warning event to inform the user: subclassof relation cannot be imported from graph, because definition of class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubClassOf relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", evc.Value, scof.Object));
                    }
                }
                #endregion

                #region EquivalentClass
                foreach (RDFTriple eqcl in prefetchContext[nameof(RDFVocabulary.OWL.EQUIVALENT_CLASS)].SelectTriplesBySubject((RDFResource)evc.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyClass equivClass = ontology.Model.ClassModel.SelectClass(eqcl.Object.ToString());
                    if (equivClass != null)
                        ontology.Model.ClassModel.AddEquivalentClassRelation(evc, equivClass);
                    else
                    {
                        //Raise warning event to inform the user: equivalentclass relation cannot be imported from graph, because definition of class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentClass relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", evc.Value, eqcl.Object));
                    }
                }
                #endregion

                #region DisjointWith
                foreach (RDFTriple djwt in prefetchContext[nameof(RDFVocabulary.OWL.DISJOINT_WITH)].SelectTriplesBySubject((RDFResource)evc.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyClass disjWith = ontology.Model.ClassModel.SelectClass(djwt.Object.ToString());
                    if (disjWith != null)
                        ontology.Model.ClassModel.AddDisjointWithRelation(evc, disjWith);
                    else
                    {
                        //Raise warning event to inform the user: disjointwith relation cannot be imported from graph, because definition of class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DisjointWith relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", evc.Value, djwt.Object));
                    }
                }
                #endregion

                #region AllDisjointClasses [OWL2]
                foreach (RDFTriple adjc in prefetchContext[nameof(RDFVocabulary.OWL.ALL_DISJOINT_CLASSES)])
                {
                    List<RDFOntologyClass> allDisjointClasses = new List<RDFOntologyClass>();
                    foreach (RDFTriple adjcMembers in prefetchContext[nameof(RDFVocabulary.OWL.MEMBERS)].SelectTriplesBySubject((RDFResource)adjc.Subject).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                    {
                        #region DeserializeCollection
                        bool nilFound = false;
                        RDFResource itemRest = (RDFResource)adjcMembers.Object;
                        HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                        while (!nilFound)
                        {
                            #region rdf:first
                            RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                            if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                RDFOntologyClass disjointClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                if (disjointClass != null)
                                    allDisjointClasses.Add(disjointClass);
                                else
                                {
                                    //Raise warning event to inform the user: all disjoint classes cannot be completely imported from graph because definition of class is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllDisjointClasses '{0}' cannot be completely imported from graph because definition of class '{1}' is not found in the model.", adjc.Subject, first.Object));
                                }

                                #region rdf:rest
                                RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                                if (rest != null)
                                {
                                    if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        nilFound = true;
                                    else
                                    {
                                        itemRest = (RDFResource)rest.Object;
                                        //Avoid bad-formed cyclic lists to generate infinite loops
                                        if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                            itemRestVisitCache.Add(itemRest.PatternMemberID);
                                        else
                                            nilFound = true;
                                    }
                                }
                                else
                                {
                                    nilFound = true;
                                }
                                #endregion rdf:rest
                            }
                            else
                            {
                                nilFound = true;
                            }
                            #endregion
                        }
                        #endregion
                    }
                    ontology.Model.ClassModel.AddAllDisjointClassesRelation(allDisjointClasses);
                }
                #endregion

                #region HasKey [OWL2]
                foreach (RDFTriple haskey in prefetchContext[nameof(RDFVocabulary.OWL.HAS_KEY)].SelectTriplesBySubject((RDFResource)evc.Value).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    RDFOntologyClass haskeyClass = ontology.Model.ClassModel.SelectClass(haskey.Subject.ToString());
                    if (haskeyClass != null)
                    {
                        List<RDFOntologyProperty> keyProps = new List<RDFOntologyProperty>();

                        #region DeserializeCollection
                        bool nilFound = false;
                        RDFResource itemRest = (RDFResource)haskey.Object;
                        HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                        while (!nilFound)
                        {
                            #region rdf:first
                            RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                            if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                RDFOntologyProperty keyProp = ontology.Model.PropertyModel.SelectProperty(first.Object.ToString());
                                if (keyProp != null)
                                    keyProps.Add(keyProp);
                                else
                                {
                                    //Raise warning event to inform the user: hasKey cannot be completely imported from graph because definition of property is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("HasKey relation '{0}' cannot be completely imported from graph because definition of key property '{1}' is not found in the model.", haskey.Subject, first.Object));
                                }

                                #region rdf:rest
                                RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                                if (rest != null)
                                {
                                    if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        nilFound = true;
                                    else
                                    {
                                        itemRest = (RDFResource)rest.Object;
                                        //Avoid bad-formed cyclic lists to generate infinite loops
                                        if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                            itemRestVisitCache.Add(itemRest.PatternMemberID);
                                        else
                                            nilFound = true;
                                    }
                                }
                                else
                                {
                                    nilFound = true;
                                }
                                #endregion
                            }
                            else
                            {
                                nilFound = true;
                            }
                            #endregion
                        }
                        #endregion

                        ontology.Model.ClassModel.AddHasKeyRelation(haskeyClass, keyProps);
                    }
                    else
                    {
                        //Raise warning event to inform the user: hasKey relation cannot be imported from graph, because definition of class is not found in the model
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("HasKey relation on class '{0}' cannot be imported from graph, because definition of the class is not found in the model.", haskey.Subject));
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Finalizes the data definitions
        /// </summary>
        private static void FinalizeData(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            #region SameAs
            foreach (RDFTriple sa in prefetchContext[nameof(RDFVocabulary.OWL.SAME_AS)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                ontology.Data.AddSameAsRelation(((RDFResource)sa.Subject).ToRDFOntologyFact(), ((RDFResource)sa.Object).ToRDFOntologyFact());
            #endregion

            #region DifferentFrom
            foreach (RDFTriple df in prefetchContext[nameof(RDFVocabulary.OWL.DIFFERENT_FROM)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                ontology.Data.AddDifferentFromRelation(((RDFResource)df.Subject).ToRDFOntologyFact(), ((RDFResource)df.Object).ToRDFOntologyFact());
            #endregion

            #region AllDifferent [OWL2]
            foreach (RDFTriple adif in prefetchContext[nameof(RDFVocabulary.OWL.ALL_DIFFERENT)])
            {
                List<RDFOntologyFact> allDifferentFacts = new List<RDFOntologyFact>();
                foreach (RDFTriple adifMembers in prefetchContext[nameof(RDFVocabulary.OWL.DISTINCT_MEMBERS)].SelectTriplesBySubject((RDFResource)adif.Subject).Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                {
                    #region DeserializeCollection
                    bool nilFound = false;
                    RDFResource itemRest = (RDFResource)adifMembers.Object;
                    HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                    while (!nilFound)
                    {
                        #region rdf:first
                        RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            RDFOntologyFact differentMember = ontology.Data.SelectFact(first.Object.ToString());
                            if (differentMember != null)
                                allDifferentFacts.Add(differentMember);
                            else
                            {
                                //Raise warning event to inform the user: all different cannot be completely imported from graph because definition of fact is not found in the data
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllDifferent '{0}' cannot be completely imported from graph because definition of fact '{1}' is not found in the data.", adif.Subject, first.Object));
                            }

                            #region rdf:rest
                            RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                            if (rest != null)
                            {
                                if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                    nilFound = true;
                                else
                                {
                                    itemRest = (RDFResource)rest.Object;
                                    //Avoid bad-formed cyclic lists to generate infinite loops
                                    if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                        itemRestVisitCache.Add(itemRest.PatternMemberID);
                                    else
                                        nilFound = true;
                                }
                            }
                            else
                            {
                                nilFound = true;
                            }
                            #endregion
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    #endregion
                }
                ontology.Data.AddAllDifferentRelation(allDifferentFacts);
            }
            #endregion

            #region Member [SKOS]
            foreach (RDFTriple col in prefetchContext[nameof(RDFVocabulary.SKOS.MEMBER)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                //skos:Collection
                RDFOntologyFact skosCollection = new RDFOntologyFact((RDFResource)col.Subject);
                ontology.Data.AddFact(skosCollection);
                ontology.Data.AddClassTypeRelation(skosCollection, RDFVocabulary.SKOS.COLLECTION.ToRDFOntologyClass());

                //skos:Collection -> skos:member -> [skos:Concept|skos:Collection]
                ontology.Data.AddMemberRelation(skosCollection, new RDFOntologyFact((RDFResource)col.Object));
            }
            #endregion

            #region MemberList [SKOS]
            foreach (RDFTriple ordCol in prefetchContext[nameof(RDFVocabulary.SKOS.MEMBER_LIST)].Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                //skos:OrderedCollection
                RDFOntologyFact skosOrderedCollection = new RDFOntologyFact((RDFResource)ordCol.Subject);
                ontology.Data.AddFact(skosOrderedCollection);
                ontology.Data.AddClassTypeRelation(skosOrderedCollection, RDFVocabulary.SKOS.ORDERED_COLLECTION.ToRDFOntologyClass());

                #region DeserializeOrderedCollection
                bool nilFound = false;
                RDFResource itemRest = (RDFResource)ordCol.Object;
                HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
                while (!nilFound)
                {
                    #region rdf:first
                    RDFTriple first = prefetchContext[nameof(RDFVocabulary.RDF.FIRST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        //skos:OrderedCollection -> skos:memberList -> skos:Concept
                        ontology.Data.AddFact(((RDFResource)first.Object).ToRDFOntologyFact());
                        ontology.Data.AddClassTypeRelation(((RDFResource)first.Object).ToRDFOntologyFact(), RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass());
                        ontology.Data.AddMemberListRelation(skosOrderedCollection, ((RDFResource)first.Object).ToRDFOntologyFact());

                        #region rdf:rest
                        RDFTriple rest = prefetchContext[nameof(RDFVocabulary.RDF.REST)].SelectTriplesBySubject(itemRest).FirstOrDefault();
                        if (rest != null)
                        {
                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                nilFound = true;
                            else
                            {
                                itemRest = (RDFResource)rest.Object;
                                //Avoid bad-formed cyclic lists to generate infinite loops
                                if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                    itemRestVisitCache.Add(itemRest.PatternMemberID);
                                else
                                    nilFound = true;
                            }
                        }
                        else
                        {
                            nilFound = true;
                        }
                        #endregion
                    }
                    else
                    {
                        nilFound = true;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

            #region NegativeAssertions [OWL2]
            foreach (RDFTriple nAsn in prefetchContext[nameof(RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION)])
            {
                #region owl:SourceIndividual
                RDFPatternMember sIndividual = prefetchContext[nameof(RDFVocabulary.OWL.SOURCE_INDIVIDUAL)].SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                if (sIndividual == null || sIndividual is RDFLiteral)
                {
                    //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because owl:SourceIndividual triple is not found in the graph or it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because owl:SourceIndividual triple is not found in the graph or it does not link a resource.", nAsn.Subject));
                    continue;
                }
                #endregion

                #region owl:AssertionProperty
                RDFPatternMember asnProperty = prefetchContext[nameof(RDFVocabulary.OWL.ASSERTION_PROPERTY)].SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                if (asnProperty == null || asnProperty is RDFLiteral)
                {
                    //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because owl:AssertionProperty triple is not found in the graph or it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because owl:AssertionProperty triple is not found in the graph or it does not link a resource.", nAsn.Subject));
                    continue;
                }

                //Check if property exists in the property model
                RDFOntologyProperty apProperty = ontology.Model.PropertyModel.SelectProperty(asnProperty.ToString());
                if (apProperty == null)
                {
                    //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because owl:AssertionProperty is not a declared property
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because owl:AssertionProperty '{1}' is not a declared property.", nAsn.Subject, asnProperty));
                    continue;
                }
                #endregion

                #region owl:TargetIndividual
                RDFPatternMember tIndividual = prefetchContext[nameof(RDFVocabulary.OWL.TARGET_INDIVIDUAL)].SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                if (tIndividual != null)
                {
                    //We found owl:TargetIndividual, so we can accept it only if it's a resource
                    //and the negative assertion's property is effectively an object property
                    if (tIndividual is RDFResource && apProperty is RDFOntologyObjectProperty)
                    {
                        ontology.Data.AddNegativeAssertionRelation(((RDFResource)sIndividual).ToRDFOntologyFact(), (RDFOntologyObjectProperty)apProperty, ((RDFResource)tIndividual).ToRDFOntologyFact());
                        continue;
                    }
                    else
                    {
                        //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because use of target individual is not correct
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because use of owl:TargetIndividual is not correct.", nAsn.Subject));
                        continue;
                    }
                }
                #endregion

                #region owl:TargetValue
                RDFPatternMember tValue = prefetchContext[nameof(RDFVocabulary.OWL.TARGET_VALUE)].SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                if (tValue != null)
                {
                    //We found owl:TargetValue, so we can accept it only if it's a literal
                    //and the negative assertion's property is effectively a datatype property
                    if (tValue is RDFLiteral && apProperty is RDFOntologyDatatypeProperty)
                    {
                        ontology.Data.AddNegativeAssertionRelation(((RDFResource)sIndividual).ToRDFOntologyFact(), (RDFOntologyDatatypeProperty)apProperty, ((RDFLiteral)tValue).ToRDFOntologyLiteral());
                        continue;
                    }
                    else
                    {
                        //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because use of target value is not correct
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because use of owl:TargetValue is not correct.", nAsn.Subject));
                        continue;
                    }
                }
                #endregion

                //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because neither owl:TargetIndividual or owl:TargetValue triples are found in the graph
                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because neither owl:TargetIndividual or owl:TargetValue triples are found in the graph.", nAsn.Subject));
            }
            #endregion

            #region Assertions
            foreach (RDFOntologyProperty p in ontology.Model.PropertyModel.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop) && !prop.IsAnnotationProperty()).ToList())
            {
                foreach (RDFTriple asn in ontGraph.SelectTriplesByPredicate((RDFResource)p.Value).Where(triple => !triple.Subject.Equals(ontology)
                                                                                                                     && !ontology.Model.ClassModel.Classes.ContainsKey(triple.Subject.PatternMemberID)
                                                                                                                        && !ontology.Model.PropertyModel.Properties.ContainsKey(triple.Subject.PatternMemberID)))
                {
                    //Check if the property is an owl:ObjectProperty
                    if (p.IsObjectProperty())
                    {
                        if (asn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            ontology.Data.AddAssertionRelation(((RDFResource)asn.Subject).ToRDFOntologyFact(), (RDFOntologyObjectProperty)p, ((RDFResource)asn.Object).ToRDFOntologyFact());
                        else
                        {
                            //Raise warning event to inform the user: assertion relation cannot be imported from graph, because object property links to a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation on fact '{0}' cannot be imported from graph, because object property '{1}' links to a literal.", asn.Subject, p));
                        }
                    }

                    //Check if the property is an owl:DatatypeProperty
                    else if (p.IsDatatypeProperty())
                    {
                        if (asn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            ontology.Data.AddAssertionRelation(((RDFResource)asn.Subject).ToRDFOntologyFact(), (RDFOntologyDatatypeProperty)p, ((RDFLiteral)asn.Object).ToRDFOntologyLiteral());
                        else
                        {
                            //Raise warning event to inform the user: assertion relation cannot be imported from graph, because datatype property links to a fact
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation on fact '{0}' cannot be imported from graph, because datatype property '{1}' links to a fact.", asn.Subject, p));
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Finalizes the ontology annotations
        /// </summary>
        private static void FinalizeOntologyAnnotations(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            #region VersionInfo
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.VERSION_INFO)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                else
                {
                    //Raise warning event to inform the user: versioninfo annotation on ontology cannot be imported from graph, because it does not link a literal
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value));
                }
            }
            #endregion

            #region VersionIRI
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.VERSION_IRI)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, new RDFOntology((RDFResource)t.Object));
                else
                {
                    //Raise warning event to inform the user: versioniri annotation on ontology cannot be imported from graph, because it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionIRI annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                }
            }
            #endregion

            #region Comment
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.COMMENT)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                else
                {
                    //Raise warning event to inform the user: comment annotation on ontology cannot be imported from graph, because it does not link a literal
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value));
                }
            }
            #endregion

            #region Label
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.LABEL)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                else
                {
                    //Raise warning event to inform the user: label annotation on ontology cannot be imported from graph, because it does not link a literal
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value));
                }
            }
            #endregion

            #region SeeAlso
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.SEE_ALSO)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                else
                {
                    RDFOntologyResource seeAlso = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                    if (seeAlso == null)
                    {
                        seeAlso = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                        if (seeAlso == null)
                        {
                            seeAlso = ontology.Data.SelectFact(t.Object.ToString());
                            if (seeAlso == null)
                                seeAlso = new RDFOntologyResource { Value = t.Object };
                        }
                    }
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, seeAlso);
                }
            }
            #endregion

            #region IsDefinedBy
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.IS_DEFINED_BY)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                else
                {
                    RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                    if (isDefBy == null)
                    {
                        isDefBy = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                        if (isDefBy == null)
                        {
                            isDefBy = ontology.Data.SelectFact(t.Object.ToString());
                            if (isDefBy == null)
                                isDefBy = new RDFOntologyResource { Value = t.Object };
                        }
                    }
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, isDefBy);
                }
            }
            #endregion

            #region BackwardCompatibleWith
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, new RDFOntology((RDFResource)t.Object));
                else
                {
                    //Raise warning event to inform the user: backwardcompatiblewith annotation on ontology cannot be imported from graph, because it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("BackwardCompatibleWith annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                }
            }
            #endregion

            #region IncompatibleWith
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.INCOMPATIBLE_WITH)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, new RDFOntology((RDFResource)t.Object));
                else
                {
                    //Raise warning event to inform the user: incompatiblewith annotation on ontology cannot be imported from graph, because it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("IncompatibleWith annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                }
            }
            #endregion

            #region PriorVersion
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.PRIOR_VERSION)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, new RDFOntology((RDFResource)t.Object));
                else
                {
                    //Raise warning event to inform the user: priorversion annotation on ontology cannot be imported from graph, because it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PriorVersion annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                }
            }
            #endregion

            #region Imports
            foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.IMPORTS)].SelectTriplesBySubject((RDFResource)ontology.Value))
            {
                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, new RDFOntology((RDFResource)t.Object));
                else
                {
                    //Raise warning event to inform the user: imports annotation on ontology cannot be imported from graph, because it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Imports annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                }
            }
            #endregion

            #region CustomAnnotations
            foreach (RDFOntologyProperty annotProp in ontology.Model.PropertyModel.Where(ap => ap.IsAnnotationProperty() && !StandardAnnotationProperties.Contains(ap.PatternMemberID)).ToList())
            {
                foreach (RDFTriple t in ontGraph.SelectTriplesBySubject((RDFResource)ontology.Value).SelectTriplesByPredicate((RDFResource)annotProp.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource custAnnValue = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (custAnnValue == null)
                        {
                            custAnnValue = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (custAnnValue == null)
                            {
                                custAnnValue = ontology.Data.SelectFact(t.Object.ToString());
                                if (custAnnValue == null)
                                    custAnnValue = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, custAnnValue);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Finalizes the class model annotations
        /// </summary>
        private static void FinalizeClassModelAnnotations(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            List<RDFOntologyClass> evaluableClasses = ontology.Model.ClassModel.Where(cls => !RDFOntologyChecker.CheckReservedClass(cls)).ToList();
            foreach (RDFOntologyClass c in evaluableClasses)
            {
                #region VersionInfo
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.VERSION_INFO)].SelectTriplesBySubject((RDFResource)c.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: versioninfo annotation on class cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value));
                    }
                }
                #endregion

                #region Comment
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.COMMENT)].SelectTriplesBySubject((RDFResource)c.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: comment annotation on class cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value));
                    }
                }
                #endregion

                #region Label
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.LABEL)].SelectTriplesBySubject((RDFResource)c.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: label annotation on class cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value));
                    }
                }
                #endregion

                #region SeeAlso
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.SEE_ALSO)].SelectTriplesBySubject((RDFResource)c.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource seeAlso = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (seeAlso == null)
                        {
                            seeAlso = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (seeAlso == null)
                            {
                                seeAlso = ontology.Data.SelectFact(t.Object.ToString());
                                if (seeAlso == null)
                                    seeAlso = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, c, seeAlso);
                    }
                }
                #endregion

                #region IsDefinedBy
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.IS_DEFINED_BY)].SelectTriplesBySubject((RDFResource)c.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (isDefBy == null)
                        {
                            isDefBy = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (isDefBy == null)
                            {
                                isDefBy = ontology.Data.SelectFact(t.Object.ToString());
                                if (isDefBy == null)
                                    isDefBy = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, c, isDefBy);
                    }
                }
                #endregion

                #region CustomAnnotations
                RDFGraph classGraph = ontGraph.SelectTriplesBySubject((RDFResource)c.Value);
                foreach (RDFOntologyProperty annotProp in ontology.Model.PropertyModel.Where(ap => ap.IsAnnotationProperty() && !StandardAnnotationProperties.Contains(ap.PatternMemberID)).ToList())
                {
                    foreach (RDFTriple t in classGraph.SelectTriplesByPredicate((RDFResource)annotProp.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            ontology.Model.ClassModel.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        else
                        {
                            RDFOntologyResource custAnnValue = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (custAnnValue == null)
                            {
                                custAnnValue = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (custAnnValue == null)
                                {
                                    custAnnValue = ontology.Data.SelectFact(t.Object.ToString());
                                    if (custAnnValue == null)
                                        custAnnValue = new RDFOntologyResource { Value = t.Object };
                                }
                            }
                            ontology.Model.ClassModel.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, c, custAnnValue);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Finalizes the property model annotations
        /// </summary>
        private static void FinalizePropertyModelAnnotations(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            List<RDFOntologyProperty> evaluableProperties = ontology.Model.PropertyModel.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop)).ToList();
            foreach (RDFOntologyProperty p in evaluableProperties)
            {
                #region VersionInfo
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.VERSION_INFO)].SelectTriplesBySubject((RDFResource)p.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: versioninfo annotation on property cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value));
                    }
                }
                #endregion

                #region Comment
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.COMMENT)].SelectTriplesBySubject((RDFResource)p.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: comment annotation on property cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value));
                    }
                }
                #endregion

                #region Label
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.LABEL)].SelectTriplesBySubject((RDFResource)p.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: label annotation on property cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value));
                    }
                }
                #endregion

                #region SeeAlso
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.SEE_ALSO)].SelectTriplesBySubject((RDFResource)p.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource seeAlso = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (seeAlso == null)
                        {
                            seeAlso = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (seeAlso == null)
                            {
                                seeAlso = ontology.Data.SelectFact(t.Object.ToString());
                                if (seeAlso == null)
                                    seeAlso = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, p, seeAlso);
                    }
                }
                #endregion

                #region IsDefinedBy
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.IS_DEFINED_BY)].SelectTriplesBySubject((RDFResource)p.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (isDefBy == null)
                        {
                            isDefBy = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (isDefBy == null)
                            {
                                isDefBy = ontology.Data.SelectFact(t.Object.ToString());
                                if (isDefBy == null)
                                    isDefBy = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, p, isDefBy);
                    }
                }
                #endregion

                #region CustomAnnotations
                RDFGraph propertyGraph = ontGraph.SelectTriplesBySubject((RDFResource)p.Value);
                foreach (RDFOntologyProperty annotProp in ontology.Model.PropertyModel.Where(ap => ap.IsAnnotationProperty() && !StandardAnnotationProperties.Contains(ap.PatternMemberID)).ToList())
                {
                    foreach (RDFTriple t in propertyGraph.SelectTriplesByPredicate((RDFResource)annotProp.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            ontology.Model.PropertyModel.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        else
                        {
                            RDFOntologyResource custAnnValue = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (custAnnValue == null)
                            {
                                custAnnValue = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (custAnnValue == null)
                                {
                                    custAnnValue = ontology.Data.SelectFact(t.Object.ToString());
                                    if (custAnnValue == null)
                                        custAnnValue = new RDFOntologyResource { Value = t.Object };
                                }
                            }
                            ontology.Model.PropertyModel.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, p, custAnnValue);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Finalizes the data annotations
        /// </summary>
        private static void FinalizeDataAnnotations(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            foreach (RDFOntologyFact f in ontology.Data)
            {
                #region VersionInfo
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.OWL.VERSION_INFO)].SelectTriplesBySubject((RDFResource)f.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: versioninfo annotation on fact cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value));
                    }
                }
                #endregion

                #region Comment
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.COMMENT)].SelectTriplesBySubject((RDFResource)f.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: comment annotation on fact cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value));
                    }
                }
                #endregion

                #region Label
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.LABEL)].SelectTriplesBySubject((RDFResource)f.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        //Raise warning event to inform the user: label annotation on fact cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value));
                    }
                }
                #endregion

                #region SeeAlso
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.SEE_ALSO)].SelectTriplesBySubject((RDFResource)f.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource seeAlso = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (seeAlso == null)
                        {
                            seeAlso = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (seeAlso == null)
                            {
                                seeAlso = ontology.Data.SelectFact(t.Object.ToString());
                                if (seeAlso == null)
                                    seeAlso = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, f, seeAlso);
                    }
                }
                #endregion

                #region IsDefinedBy
                foreach (RDFTriple t in prefetchContext[nameof(RDFVocabulary.RDFS.IS_DEFINED_BY)].SelectTriplesBySubject((RDFResource)f.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    else
                    {
                        RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (isDefBy == null)
                        {
                            isDefBy = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (isDefBy == null)
                            {
                                isDefBy = ontology.Data.SelectFact(t.Object.ToString());
                                if (isDefBy == null)
                                    isDefBy = new RDFOntologyResource { Value = t.Object };
                            }
                        }
                        ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, f, isDefBy);
                    }
                }
                #endregion

                #region CustomAnnotations
                RDFGraph factGraph = ontGraph.SelectTriplesBySubject((RDFResource)f.Value);
                foreach (RDFOntologyProperty annotProp in ontology.Model.PropertyModel.Where(ap => ap.IsAnnotationProperty() && !StandardAnnotationProperties.Contains(ap.PatternMemberID)).ToList())
                {
                    foreach (RDFTriple t in factGraph.SelectTriplesByPredicate((RDFResource)annotProp.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            ontology.Data.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        else
                        {
                            RDFOntologyResource custAnnValue = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (custAnnValue == null)
                            {
                                custAnnValue = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (custAnnValue == null)
                                {
                                    custAnnValue = ontology.Data.SelectFact(t.Object.ToString());
                                    if (custAnnValue == null)
                                        custAnnValue = new RDFOntologyResource { Value = t.Object };
                                }
                            }
                            ontology.Data.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProp, f, custAnnValue);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Finalizes the axiom annotations
        /// </summary>
        private static void FinalizeAxiomAnnotations(RDFOntology ontology, RDFGraph ontGraph, Dictionary<string, RDFGraph> prefetchContext)
        {
            #region AxiomAnnotations [OWL2]
            foreach (RDFTriple axAsn in prefetchContext[nameof(RDFVocabulary.OWL.AXIOM)])
            {
                #region owl:annotatedSource
                RDFPatternMember annotatedSource = prefetchContext[nameof(RDFVocabulary.OWL.ANNOTATED_SOURCE)].SelectTriplesBySubject((RDFResource)axAsn.Subject).FirstOrDefault()?.Object;
                if (annotatedSource == null || annotatedSource is RDFLiteral)
                {
                    //Raise warning event to inform the user: axiom annotation cannot be imported from graph, because owl:annotatedSource triple is not found in the graph or it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Axiom annotation '{0}' cannot be imported from graph, because owl:annotatedSource triple is not found in the graph or it does not link a resource.", axAsn.Subject));
                    continue;
                }
                #endregion

                #region owl:annotatedProperty
                RDFPatternMember annotatedProperty = prefetchContext[nameof(RDFVocabulary.OWL.ANNOTATED_PROPERTY)].SelectTriplesBySubject((RDFResource)axAsn.Subject).FirstOrDefault()?.Object;
                if (annotatedProperty == null || annotatedProperty is RDFLiteral)
                {
                    //Raise warning event to inform the user: axiom annotation cannot be imported from graph, because owl:annotatedProperty triple is not found in the graph or it does not link a resource
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Axiom annotation '{0}' cannot be imported from graph, because owl:annotatedProperty triple is not found in the graph or it does not link a resource.", axAsn.Subject));
                    continue;
                }
                #endregion

                #region owl:annotatedTarget
                RDFPatternMember annotatedTarget = prefetchContext[nameof(RDFVocabulary.OWL.ANNOTATED_TARGET)].SelectTriplesBySubject((RDFResource)axAsn.Subject).FirstOrDefault()?.Object;
                if (annotatedTarget == null)
                { 
                    //Raise warning event to inform the user: axiom annotation cannot be imported from graph, because owl:annotatedTarget triple is not found in the graph
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Axiom annotation '{0}' cannot be imported from graph, because owl:annotatedTarget triple is not found in the graph.", axAsn.Subject));
                    continue;
                }
                #endregion

                //Fetch annotations owned by this axiom
                RDFGraph axiomAnnTriples = ontGraph.SelectTriplesBySubject((RDFResource)axAsn.Subject)
                                                    .RemoveTriplesByPredicateObject(RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.AXIOM)
                                                    .RemoveTriplesByPredicate(RDFVocabulary.OWL.ANNOTATED_SOURCE)
                                                    .RemoveTriplesByPredicate(RDFVocabulary.OWL.ANNOTATED_PROPERTY)
                                                    .RemoveTriplesByPredicate(RDFVocabulary.OWL.ANNOTATED_TARGET);

                //Assign fetched annotations to proper taxonomy (recognition driven by owl:AnnotatedProperty)
                bool annotatedTargetIsResource = annotatedTarget is RDFResource;
                foreach (RDFTriple axiomAnnTriple in axiomAnnTriples.Where(axn => axn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                {
                    RDFOntologyAxiomAnnotation axiomAnnotation = new RDFOntologyAxiomAnnotation(((RDFResource)axiomAnnTriple.Predicate).ToRDFOntologyProperty(), ((RDFLiteral)axiomAnnTriple.Object).ToRDFOntologyLiteral());

                    #region ClassModel(SubClassOf)
                    if (annotatedProperty.Equals(RDFVocabulary.RDFS.SUB_CLASS_OF))
                        ontology.Model.ClassModel.AddSubClassOfRelation(((RDFResource)annotatedSource).ToRDFOntologyClass(), ((RDFResource)annotatedTarget).ToRDFOntologyClass(), axiomAnnotation);
                    #endregion

                    #region ClassModel(EquivalentClass)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.EQUIVALENT_CLASS))
                        ontology.Model.ClassModel.AddEquivalentClassRelation(((RDFResource)annotatedSource).ToRDFOntologyClass(), ((RDFResource)annotatedTarget).ToRDFOntologyClass(), axiomAnnotation);
                    #endregion

                    #region ClassModel(DisjointWith)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.DISJOINT_WITH))
                        ontology.Model.ClassModel.AddDisjointWithRelation(((RDFResource)annotatedSource).ToRDFOntologyClass(), ((RDFResource)annotatedTarget).ToRDFOntologyClass(), axiomAnnotation);
                    #endregion

                    #region PropertyModel(SubPropertyOf)
                    else if (annotatedProperty.Equals(RDFVocabulary.RDFS.SUB_PROPERTY_OF))
                        ontology.Model.PropertyModel.AddSubPropertyOfRelation(((RDFResource)annotatedSource).ToRDFOntologyObjectProperty(), ((RDFResource)annotatedTarget).ToRDFOntologyObjectProperty(), axiomAnnotation);
                    #endregion

                    #region PropertyModel(EquivalentProperty)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.EQUIVALENT_PROPERTY))
                        ontology.Model.PropertyModel.AddEquivalentPropertyRelation(((RDFResource)annotatedSource).ToRDFOntologyObjectProperty(), ((RDFResource)annotatedTarget).ToRDFOntologyObjectProperty(), axiomAnnotation);
                    #endregion

                    #region PropertyModel(PropertyDisjointWith)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.PROPERTY_DISJOINT_WITH))
                        ontology.Model.PropertyModel.AddPropertyDisjointWithRelation(((RDFResource)annotatedSource).ToRDFOntologyObjectProperty(), ((RDFResource)annotatedTarget).ToRDFOntologyObjectProperty(), axiomAnnotation);
                    #endregion

                    #region PropertyModel(InverseOf)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.INVERSE_OF))
                        ontology.Model.PropertyModel.AddInverseOfRelation(((RDFResource)annotatedSource).ToRDFOntologyObjectProperty(), ((RDFResource)annotatedTarget).ToRDFOntologyObjectProperty(), axiomAnnotation);
                    #endregion

                    #region Data(ClassType)
                    else if (annotatedProperty.Equals(RDFVocabulary.RDF.TYPE))
                        ontology.Data.AddClassTypeRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedTarget).ToRDFOntologyClass(), axiomAnnotation);
                    #endregion

                    #region Data(SameAs)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.SAME_AS))
                        ontology.Data.AddSameAsRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedTarget).ToRDFOntologyFact(), axiomAnnotation);
                    #endregion

                    #region Data(DifferentFrom)
                    else if (annotatedProperty.Equals(RDFVocabulary.OWL.DIFFERENT_FROM))
                        ontology.Data.AddDifferentFromRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedTarget).ToRDFOntologyFact(), axiomAnnotation);
                    #endregion

                    #region Data(Member)
                    else if (annotatedProperty.Equals(RDFVocabulary.SKOS.MEMBER))
                        ontology.Data.AddMemberRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedTarget).ToRDFOntologyFact(), axiomAnnotation);
                    #endregion

                    #region Data(MemberList)
                    else if (annotatedProperty.Equals(RDFVocabulary.SKOS.MEMBER_LIST))
                        ontology.Data.AddMemberListRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedTarget).ToRDFOntologyFact(), axiomAnnotation);
                    #endregion

                    #region Data(Assertions)
                    else
                    {
                        if (annotatedTargetIsResource)
                            ontology.Data.AddAssertionRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedProperty).ToRDFOntologyObjectProperty(), ((RDFResource)annotatedTarget).ToRDFOntologyFact(), axiomAnnotation);
                        else
                            ontology.Data.AddAssertionRelation(((RDFResource)annotatedSource).ToRDFOntologyFact(), ((RDFResource)annotatedProperty).ToRDFOntologyDatatypeProperty(), ((RDFLiteral)annotatedTarget).ToRDFOntologyLiteral(), axiomAnnotation);
                    }
                    #endregion
                }
            }
            #endregion
        }

        /// <summary>
        /// Ends the graph->ontology process
        /// </summary>
        private static void EndProcess(ref RDFOntology ontology)
        {
            //Unexpand from BASE ontology
            RDFPatternMember ontologyValue = ontology.Value;
            ontology = ontology.DifferenceWith(RDFBASEOntology.Instance);
            ontology.Value = ontologyValue;
        }

        /// <summary>
        /// Gets a graph representation of the given ontology, exporting inferences according to the selected behavior
        /// </summary>
        internal static RDFGraph ToRDFGraph(RDFOntology ontology, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            var result = new RDFGraph();
            if (ontology != null)
            {

                #region Step 1: Export ontology
                result.AddTriple(new RDFTriple((RDFResource)ontology.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ONTOLOGY));
                result = result.UnionWith(ontology.Annotations.VersionInfo.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.VersionInfo)))
                               .UnionWith(ontology.Annotations.VersionIRI.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.VersionIRI)))
                               .UnionWith(ontology.Annotations.Comment.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.Comment)))
                               .UnionWith(ontology.Annotations.Label.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.Label)))
                               .UnionWith(ontology.Annotations.SeeAlso.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.SeeAlso)))
                               .UnionWith(ontology.Annotations.IsDefinedBy.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.IsDefinedBy)))
                               .UnionWith(ontology.Annotations.BackwardCompatibleWith.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.BackwardCompatibleWith)))
                               .UnionWith(ontology.Annotations.IncompatibleWith.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.IncompatibleWith)))
                               .UnionWith(ontology.Annotations.PriorVersion.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.PriorVersion)))
                               .UnionWith(ontology.Annotations.Imports.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.Imports)))
                               .UnionWith(ontology.Annotations.CustomAnnotations.ReifyToRDFGraph(infexpBehavior, nameof(ontology.Annotations.CustomAnnotations)));
                #endregion

                #region Step 2: Export model
                result = result.UnionWith(ontology.Model.ToRDFGraph(infexpBehavior));
                #endregion

                #region Step 3: Export data
                result = result.UnionWith(ontology.Data.ToRDFGraph(infexpBehavior));
                #endregion

                #region Step 4: Finalize
                result.SetContext(((RDFResource)ontology.Value).URI);
                #endregion

            }

            return result;
        }
        #endregion
    }

}