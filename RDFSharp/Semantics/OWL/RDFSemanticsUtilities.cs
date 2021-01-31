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
using RDFSharp.Query;
using System;
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

        #region Convert
        /// <summary>
        /// Gets an ontology representation of the given graph
        /// </summary>
        internal static RDFOntology FromRDFGraph(RDFGraph ontGraph)
        {
            RDFOntology ontology = null;
            if (ontGraph != null)
            {
                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Graph '{0}' is going to be parsed as Ontology: triples not having supported ontology semantics may be discarded.", ontGraph.Context));

                #region Step 1: Prefetch

                var versionInfo = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.VERSION_INFO);
                var versionIRI = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.VERSION_IRI);
                var comment = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.COMMENT);
                var label = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.LABEL);
                var seeAlso = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SEE_ALSO);
                var isDefinedBy = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.IS_DEFINED_BY);
                var imports = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.IMPORTS);
                var bcwcompWith = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH);
                var incompWith = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INCOMPATIBLE_WITH);
                var priorVersion = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PRIOR_VERSION);

                var rdfType = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE);
                var rdfFirst = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST);
                var rdfRest = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.REST);
                var skosMemberList = ontGraph.SelectTriplesByPredicate(RDFVocabulary.SKOS.MEMBER_LIST); //SKOS
                var subclassOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_CLASS_OF);
                var subpropOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_PROPERTY_OF);
                var domain = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.DOMAIN);
                var range = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.RANGE);
                var equivclassOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_CLASS);
                var disjclassWith = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISJOINT_WITH);
                var alldisjclasses = rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ALL_DISJOINT_CLASSES); //OWL2
                var hasKey = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_KEY); //OWL2
                var propertyChainAxiom = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM); //OWL2
                var equivpropOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_PROPERTY);
                var alldisjprops = rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ALL_DISJOINT_PROPERTIES); //OWL2
                var disjpropWith = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PROPERTY_DISJOINT_WITH); //OWL2
                var inverseOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INVERSE_OF);
                var onProperty = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_PROPERTY);
                var onClass = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_CLASS); //OWL2
                var onDataRange = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_DATARANGE); //OWL2
                var oneOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ONE_OF);
                var unionOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.UNION_OF);
                var disjointUnionOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISJOINT_UNION_OF); //OWL2
                var intersectionOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INTERSECTION_OF);
                var complementOf = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.COMPLEMENT_OF);
                var allvaluesFrom = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ALL_VALUES_FROM);
                var somevaluesFrom = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SOME_VALUES_FROM);
                var hasself = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_SELF); //OWL2
                var hasvalue = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_VALUE);
                var cardinality = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.CARDINALITY);
                var mincardinality = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MIN_CARDINALITY);
                var maxcardinality = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MAX_CARDINALITY);
                var qualifiedCardinality = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.QUALIFIED_CARDINALITY); //OWL2
                var minQualifiedCardinality = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MIN_QUALIFIED_CARDINALITY); //OWL2
                var maxQualifiedCardinality = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MAX_QUALIFIED_CARDINALITY); //OWL2
                var sameAs = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SAME_AS);
                var differentFrom = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DIFFERENT_FROM);
                var alldifferent = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ALL_DIFFERENT); //OWL2
                var members = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MEMBERS);
                var distinctMembers = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISTINCT_MEMBERS);
                var negativeAssertions = rdfType.SelectTriplesByObject(RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION); //OWL2
                var sourceIndividual = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SOURCE_INDIVIDUAL); //OWL2
                var assertionProperty = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ASSERTION_PROPERTY); //OWL2
                var targetValue = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.TARGET_VALUE); //OWL2
                var targetIndividual = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.TARGET_INDIVIDUAL); //OWL2

                var versionInfoAnn = RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty();
                var commentAnn = RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty();
                var labelAnn = RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty();
                var seeAlsoAnn = RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty();
                var isDefinedByAnn = RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty();
                var versionIRIAnn = RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty();
                var priorVersionAnn = RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty();
                var backwardCWAnn = RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty();
                var incompWithAnn = RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty();
                var importsAnn = RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty();
                #endregion

                #region Step 2: Init Ontology
                ontology = new RDFOntology(new RDFResource(ontGraph.Context.ToString())).UnionWith(RDFBASEOntology.Instance);
                ontology.Value = new RDFResource(ontGraph.Context.ToString());
                if (!rdfType.ContainsTriple(new RDFTriple((RDFResource)ontology.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ONTOLOGY)))
                {
                    var ont = rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ONTOLOGY)
                                     .FirstOrDefault();
                    if (ont != null)
                    {
                        ontology.Value = ont.Subject;
                        ontology.PatternMemberID = ontology.Value.PatternMemberID;
                    }
                }
                #endregion

                #region Step 3: Init PropertyModel

                #region Step 3.0: Load RDF:Property
                foreach (var p in rdfType.SelectTriplesByObject(RDFVocabulary.RDF.PROPERTY))
                    ontology.Model.PropertyModel.AddProperty(((RDFResource)p.Subject).ToRDFOntologyProperty());
                #endregion

                #region Step 3.1: Load OWL:AnnotationProperty
                foreach (var ap in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ANNOTATION_PROPERTY))
                {
                    RDFOntologyAnnotationProperty aap = ((RDFResource)ap.Subject).ToRDFOntologyAnnotationProperty();
                    if (!ontology.Model.PropertyModel.Properties.ContainsKey(aap.PatternMemberID))
                        ontology.Model.PropertyModel.AddProperty(aap);
                    else
                        ontology.Model.PropertyModel.Properties[aap.PatternMemberID] = aap;
                }
                #endregion

                #region Step 3.2: Load OWL:DatatypeProperty
                foreach (var dp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.DATATYPE_PROPERTY))
                {
                    RDFOntologyDatatypeProperty dtp = ((RDFResource)dp.Subject).ToRDFOntologyDatatypeProperty();
                    if (!ontology.Model.PropertyModel.Properties.ContainsKey(dtp.PatternMemberID))
                        ontology.Model.PropertyModel.AddProperty(dtp);
                    else
                        ontology.Model.PropertyModel.Properties[dtp.PatternMemberID] = dtp;

                    #region DeprecatedProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)dtp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        dtp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)dtp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        dtp.SetFunctional(true);
                    #endregion

                }
                #endregion

                #region Step 3.3: Load OWL:ObjectProperty
                foreach (var op in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.OBJECT_PROPERTY))
                {
                    RDFOntologyObjectProperty obp = ((RDFResource)op.Subject).ToRDFOntologyObjectProperty();
                    if (!ontology.Model.PropertyModel.Properties.ContainsKey(obp.PatternMemberID))
                        ontology.Model.PropertyModel.AddProperty(obp);
                    else
                        ontology.Model.PropertyModel.Properties[obp.PatternMemberID] = obp;

                    #region DeprecatedProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                        obp.SetDeprecated(true);
                    #endregion

                    #region FunctionalProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                        obp.SetFunctional(true);
                    #endregion

                    #region SymmetricProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.SYMMETRIC_PROPERTY)))
                        obp.SetSymmetric(true);
                    #endregion

                    #region AsymmetricProperty [OWL2]
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ASYMMETRIC_PROPERTY)))
                        obp.SetAsymmetric(true);
                    #endregion

                    #region ReflexiveProperty [OWL2]
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.REFLEXIVE_PROPERTY)))
                        obp.SetReflexive(true);
                    #endregion

                    #region IrreflexiveProperty [OWL2]
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.IRREFLEXIVE_PROPERTY)))
                        obp.SetIrreflexive(true);
                    #endregion

                    #region TransitiveProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.TRANSITIVE_PROPERTY)))
                        obp.SetTransitive(true);
                    #endregion

                    #region InverseFunctionalProperty
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY)))
                        obp.SetInverseFunctional(true);
                    #endregion
                }

                #region SymmetricProperty
                foreach (var sp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.SYMMETRIC_PROPERTY))
                {
                    var syp = ontology.Model.PropertyModel.SelectProperty(sp.Subject.ToString());
                    if (syp == null)
                    {
                        syp = ((RDFResource)sp.Subject).ToRDFOntologyObjectProperty();
                        ontology.Model.PropertyModel.AddProperty(syp);

                        #region DeprecatedProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)syp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                            syp.SetDeprecated(true);
                        #endregion

                        #region FunctionalProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)syp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                            syp.SetFunctional(true);
                        #endregion
                    }
                    ((RDFOntologyObjectProperty)syp).SetSymmetric(true);
                }
                #endregion

                #region AsymmetricProperty [OWL2]
                foreach (var ap in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ASYMMETRIC_PROPERTY))
                {
                    var asyp = ontology.Model.PropertyModel.SelectProperty(ap.Subject.ToString());
                    if (asyp == null)
                    {
                        asyp = ((RDFResource)ap.Subject).ToRDFOntologyObjectProperty();
                        ontology.Model.PropertyModel.AddProperty(asyp);

                        #region DeprecatedProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)asyp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                            asyp.SetDeprecated(true);
                        #endregion

                        #region FunctionalProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)asyp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                            asyp.SetFunctional(true);
                        #endregion
                    }
                    ((RDFOntologyObjectProperty)asyp).SetAsymmetric(true);
                }
                #endregion

                #region ReflexiveProperty [OWL2]
                foreach (var rp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.REFLEXIVE_PROPERTY))
                {
                    var refp = ontology.Model.PropertyModel.SelectProperty(rp.Subject.ToString());
                    if (refp == null)
                    {
                        refp = ((RDFResource)rp.Subject).ToRDFOntologyObjectProperty();
                        ontology.Model.PropertyModel.AddProperty(refp);

                        #region DeprecatedProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)refp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                            refp.SetDeprecated(true);
                        #endregion

                        #region FunctionalProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)refp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                            refp.SetFunctional(true);
                        #endregion
                    }
                    ((RDFOntologyObjectProperty)refp).SetReflexive(true);
                }
                #endregion

                #region IrreflexiveProperty [OWL2]
                foreach (var irp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.IRREFLEXIVE_PROPERTY))
                {
                    var irrefp = ontology.Model.PropertyModel.SelectProperty(irp.Subject.ToString());
                    if (irrefp == null)
                    {
                        irrefp = ((RDFResource)irp.Subject).ToRDFOntologyObjectProperty();
                        ontology.Model.PropertyModel.AddProperty(irrefp);

                        #region DeprecatedProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)irrefp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                            irrefp.SetDeprecated(true);
                        #endregion

                        #region FunctionalProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)irrefp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                            irrefp.SetFunctional(true);
                        #endregion
                    }
                    ((RDFOntologyObjectProperty)irrefp).SetIrreflexive(true);
                }
                #endregion

                #region TransitiveProperty
                foreach (var tp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.TRANSITIVE_PROPERTY))
                {
                    var trp = ontology.Model.PropertyModel.SelectProperty(tp.Subject.ToString());
                    if (trp == null)
                    {
                        trp = ((RDFResource)tp.Subject).ToRDFOntologyObjectProperty();
                        ontology.Model.PropertyModel.AddProperty(trp);

                        #region DeprecatedProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)trp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                            trp.SetDeprecated(true);
                        #endregion

                        #region FunctionalProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)trp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                            trp.SetFunctional(true);
                        #endregion
                    }
                    ((RDFOntologyObjectProperty)trp).SetTransitive(true);
                }
                #endregion

                #region InverseFunctionalProperty
                foreach (var ip in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY))
                {
                    var ifp = ontology.Model.PropertyModel.SelectProperty(ip.Subject.ToString());
                    if (ifp == null)
                    {
                        ifp = ((RDFResource)ip.Subject).ToRDFOntologyObjectProperty();
                        ontology.Model.PropertyModel.AddProperty(ifp);

                        #region DeprecatedProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)ifp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY)))
                            ifp.SetDeprecated(true);
                        #endregion

                        #region FunctionalProperty
                        if (rdfType.ContainsTriple(new RDFTriple((RDFResource)ifp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY)))
                            ifp.SetFunctional(true);
                        #endregion

                    }
                    ((RDFOntologyObjectProperty)ifp).SetInverseFunctional(true);
                }
                #endregion
                #endregion

                #endregion

                #region Step 4: Init ClassModel

                #region Step 4.0: Load RDFS:Class
                foreach (var c in rdfType.SelectTriplesByObject(RDFVocabulary.RDFS.CLASS))
                {
                    var rdfsClass = ((RDFResource)c.Subject).ToRDFOntologyClass(RDFSemanticsEnums.RDFOntologyClassNature.RDFS);
                    ontology.Model.ClassModel.AddClass(rdfsClass);

                    #region DeprecatedClass
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)rdfsClass.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_CLASS)))
                        rdfsClass.SetDeprecated(true);
                    #endregion

                }
                #endregion

                #region Step 4.1: Load OWL:DataRange
                foreach (var dr in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.DATA_RANGE))
                {
                    var datarangeClass = new RDFOntologyDataRangeClass((RDFResource)dr.Subject);
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(datarangeClass.PatternMemberID))
                        ontology.Model.ClassModel.AddClass(datarangeClass);
                    else
                        ontology.Model.ClassModel.Classes[datarangeClass.PatternMemberID] = datarangeClass;
                }
                #endregion

                #region Step 4.2: Load OWL:Class
                foreach (var c in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.CLASS))
                {
                    var owlClass = ((RDFResource)c.Subject).ToRDFOntologyClass();
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(owlClass.PatternMemberID))
                        ontology.Model.ClassModel.AddClass(owlClass);
                    else
                        ontology.Model.ClassModel.Classes[owlClass.PatternMemberID] = owlClass;

                    #region DeprecatedClass
                    if (rdfType.ContainsTriple(new RDFTriple((RDFResource)owlClass.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_CLASS)))
                        owlClass.SetDeprecated(true);
                    #endregion

                }
                #endregion

                #region Step 4.3: Load OWL:DeprecatedClass
                foreach (var dc in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.DEPRECATED_CLASS))
                {
                    var deprecatedClass = ((RDFResource)dc.Subject).ToRDFOntologyClass().SetDeprecated(true);
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(deprecatedClass.PatternMemberID))
                        ontology.Model.ClassModel.AddClass(deprecatedClass);
                    else
                        ontology.Model.ClassModel.Classes[deprecatedClass.PatternMemberID].SetDeprecated(true);
                }
                #endregion

                #region Step 4.4: Load OWL:Restriction
                foreach (var r in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.RESTRICTION))
                {

                    #region OnProperty
                    var op = onProperty.SelectTriplesBySubject((RDFResource)r.Subject).FirstOrDefault();
                    if (op != null)
                    {
                        var onProp = ontology.Model.PropertyModel.SelectProperty(op.Object.ToString());
                        if (onProp != null)
                        {
                            //Ensure to not create a restriction over an annotation property (or a BASE reserved property)
                            if (!onProp.IsAnnotationProperty() && !RDFOntologyChecker.CheckReservedProperty(onProp))
                            {
                                var restr = new RDFOntologyRestriction((RDFResource)r.Subject, onProp);
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
                    #endregion

                }
                #endregion

                #region Step 4.5: Load OWL:[UnionOf|DisjointUnionOf|IntersectionOf|ComplementOf]

                #region Union
                foreach (var u in unionOf)
                {
                    if (u.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        #region Initialize
                        var uc = ontology.Model.ClassModel.SelectClass(u.Subject.ToString());
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
                            var nilFound = false;
                            var itemRest = (RDFResource)u.Object;
                            while (!nilFound)
                            {

                                #region rdf:first
                                var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                    .FirstOrDefault();
                                if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {
                                    var compClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                    if (compClass != null)
                                    {
                                        ontology.Model.ClassModel.AddUnionOfRelation((RDFOntologyUnionClass)uc, new List<RDFOntologyClass>() { compClass });
                                    }
                                    else
                                    {
                                        //Raise warning event to inform the user: union class cannot be completely imported from graph, because definition of its compositing class is not found in the model
                                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("UnionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", u.Subject, first.Object));
                                    }

                                    #region rdf:rest
                                    var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                      .FirstOrDefault();
                                    if (rest != null)
                                    {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        {
                                            nilFound = true;
                                        }
                                        else
                                        {
                                            itemRest = (RDFResource)rest.Object;
                                        }
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
                }
                #endregion

                #region DisjointUnion [OWL2]
                foreach (var du in disjointUnionOf)
                {
                    if (du.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        #region Initialize
                        var duc = ontology.Model.ClassModel.SelectClass(du.Subject.ToString());
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
                        var nilFound = false;
                        var itemRest = (RDFResource)du.Object;
                        var disjointClasses = new List<RDFOntologyClass>();
                        while (!nilFound)
                        {

                            #region rdf:first
                            var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                .FirstOrDefault();
                            if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                var compClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                if (compClass != null)
                                {
                                    ontology.Model.ClassModel.AddUnionOfRelation((RDFOntologyUnionClass)duc, new List<RDFOntologyClass>() { compClass });
                                    //Collect disjoint classes for subsequent elaboration
                                    disjointClasses.Add(compClass);
                                }
                                else
                                {
                                    //Raise warning event to inform the user: union class cannot be completely imported from graph, because definition of its compositing class is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("UnionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", du.Subject, first.Object));
                                }

                                #region rdf:rest
                                var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                    .FirstOrDefault();
                                if (rest != null)
                                {
                                    if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                    {
                                        nilFound = true;
                                    }
                                    else
                                    {
                                        itemRest = (RDFResource)rest.Object;
                                    }
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
                }
                #endregion

                #region Intersection
                foreach (var i in intersectionOf)
                {
                    if (i.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        #region Initialize
                        var ic = ontology.Model.ClassModel.SelectClass(i.Subject.ToString());
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
                        var nilFound = false;
                        var itemRest = (RDFResource)i.Object;
                        while (!nilFound)
                        {

                            #region rdf:first
                            var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                    .FirstOrDefault();
                            if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                var compClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                if (compClass != null)
                                {
                                    ontology.Model.ClassModel.AddIntersectionOfRelation((RDFOntologyIntersectionClass)ic, new List<RDFOntologyClass>() { compClass });
                                }
                                else
                                {
                                    //Raise warning event to inform the user: intersection class cannot be completely imported from graph, because definition of its compositing class is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("IntersectionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", i.Subject, first.Object));
                                }

                                #region rdf:rest
                                var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                            .FirstOrDefault();
                                if (rest != null)
                                {
                                    if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                    {
                                        nilFound = true;
                                    }
                                    else
                                    {
                                        itemRest = (RDFResource)rest.Object;
                                    }
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
                }
                #endregion

                #region Complement
                foreach (var c in complementOf)
                {
                    if (c.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var cc = ontology.Model.ClassModel.SelectClass(c.Subject.ToString());
                        if (cc != null)
                        {
                            var compClass = ontology.Model.ClassModel.SelectClass(c.Object.ToString());
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
                }
                #endregion

                #endregion

                #endregion

                #region Step 5: Init Data
                foreach (var c in ontology.Model.ClassModel.Where(cls => !RDFOntologyChecker.CheckReservedClass(cls)
                                                                            && cls.IsSimpleClass()))
                {
                    foreach (var t in rdfType.SelectTriplesByObject((RDFResource)c.Value))
                    {
                        var f = ontology.Data.SelectFact(t.Subject.ToString());
                        if (f == null)
                        {
                            f = ((RDFResource)t.Subject).ToRDFOntologyFact();
                            ontology.Data.AddFact(f);
                        }
                        ontology.Data.AddClassTypeRelation(f, c);
                    }
                }
                #endregion

                #region Step 6: Finalize

                #region Step 6.1: Finalize OWL:Restriction
                var restrictions = ontology.Model.ClassModel.Where(c => c.IsRestrictionClass()).ToList();
                foreach (var r in restrictions.OfType<RDFOntologyCardinalityRestriction>())
                {

                    #region ExactCardinality
                    int exC = 0;
                    var crEx = cardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crEx != null && crEx.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        if (crEx.Object is RDFPlainLiteral)
                        {
                            if (Regex.IsMatch(crEx.Object.ToString(), @"^[0-9]+$"))
                            {
                                exC = int.Parse(crEx.Object.ToString());
                            }
                        }
                        else
                        {
                            if (((RDFTypedLiteral)crEx.Object).HasDecimalDatatype())
                            {
                                if (Regex.IsMatch(((RDFTypedLiteral)crEx.Object).Value, @"^[0-9]+$"))
                                {
                                    exC = int.Parse(((RDFTypedLiteral)crEx.Object).Value);
                                }
                            }
                        }
                    }

                    if (exC > 0)
                    {
                        var cardRestr = new RDFOntologyCardinalityRestriction((RDFResource)r.Value, r.OnProperty, exC, exC);
                        ontology.Model.ClassModel.Classes[r.PatternMemberID] = cardRestr;
                        continue;
                    }
                    #endregion

                    #region MinMaxCardinality
                    int minC = 0;
                    var crMin = mincardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crMin != null && crMin.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        if (crMin.Object is RDFPlainLiteral)
                        {
                            if (Regex.IsMatch(crMin.Object.ToString(), @"^[0-9]+$"))
                            {
                                minC = int.Parse(crMin.Object.ToString());
                            }
                        }
                        else
                        {
                            if (((RDFTypedLiteral)crMin.Object).HasDecimalDatatype())
                            {
                                if (Regex.IsMatch(((RDFTypedLiteral)crMin.Object).Value, @"^[0-9]+$"))
                                {
                                    minC = int.Parse(((RDFTypedLiteral)crMin.Object).Value);
                                }
                            }
                        }
                    }

                    int maxC = 0;
                    var crMax = maxcardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crMax != null && crMax.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        if (crMax.Object is RDFPlainLiteral)
                        {
                            if (Regex.IsMatch(crMax.Object.ToString(), @"^[0-9]+$"))
                            {
                                maxC = int.Parse(crMax.Object.ToString());
                            }
                        }
                        else
                        {
                            if (((RDFTypedLiteral)crMax.Object).HasDecimalDatatype())
                            {
                                if (Regex.IsMatch(((RDFTypedLiteral)crMax.Object).Value, @"^[0-9]+$"))
                                {
                                    maxC = int.Parse(((RDFTypedLiteral)crMax.Object).Value);
                                }
                            }
                        }
                    }

                    if (minC > 0 || maxC > 0)
                    {
                        var cardRestr = new RDFOntologyCardinalityRestriction((RDFResource)r.Value, r.OnProperty, minC, maxC);
                        ontology.Model.ClassModel.Classes[r.PatternMemberID] = cardRestr;
                    }
                    #endregion

                }
                foreach (var r in restrictions.OfType<RDFOntologyQualifiedCardinalityRestriction>())
                {

                    #region ExactQualifiedCardinality [OWL2]
                    int exQC = 0;
                    var crExQC = qualifiedCardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crExQC != null && crExQC.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        if (crExQC.Object is RDFPlainLiteral)
                        {
                            if (Regex.IsMatch(crExQC.Object.ToString(), @"^[0-9]+$"))
                            {
                                exQC = int.Parse(crExQC.Object.ToString());
                            }
                        }
                        else
                        {
                            if (((RDFTypedLiteral)crExQC.Object).HasDecimalDatatype())
                            {
                                if (Regex.IsMatch(((RDFTypedLiteral)crExQC.Object).Value, @"^[0-9]+$"))
                                {
                                    exQC = int.Parse(((RDFTypedLiteral)crExQC.Object).Value);
                                }
                            }
                        }
                    }

                    if (exQC > 0)
                    {
                        //OnClass
                        var exQCCls = onClass.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                        if (exQCCls != null && exQCCls.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var exQCOnClass = ontology.Model.ClassModel.SelectClass(exQCCls.Object.ToString());
                            if (exQCOnClass != null)
                            {
                                var qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)r.Value, r.OnProperty, exQCOnClass, exQC, exQC);
                                ontology.Model.ClassModel.Classes[r.PatternMemberID] = qualifCardRestr;
                                continue;
                            }
                            else
                            {
                                //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onClass is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onClass '{1}' is not found in the model.", r.Value, exQCCls.Object));
                            }
                        }
                        else
                        {
                            //OnDataRange
                            var exQCDrn = onDataRange.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                            if (exQCDrn != null && exQCDrn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                var exQCOnDataRange = ontology.Model.ClassModel.SelectClass(exQCDrn.Object.ToString());
                                if (exQCOnDataRange != null)
                                {
                                    var qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, exQCOnDataRange, exQC, exQC);
                                    ontology.Model.ClassModel.Classes[r.PatternMemberID] = qualifCardRestr;
                                    continue;
                                }
                                else
                                {
                                    //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onDataRange is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onDataRange '{1}' is not found in the model.", r.Value, exQCDrn.Object));
                                }
                            }
                        }
                    }
                    #endregion

                    #region MinMaxQualifiedCardinality [OWL2]
                    int minQC = 0;
                    var crMinQC = minQualifiedCardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crMinQC != null && crMinQC.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        if (crMinQC.Object is RDFPlainLiteral)
                        {
                            if (Regex.IsMatch(crMinQC.Object.ToString(), @"^[0-9]+$"))
                            {
                                minQC = int.Parse(crMinQC.Object.ToString());
                            }
                        }
                        else
                        {
                            if (((RDFTypedLiteral)crMinQC.Object).HasDecimalDatatype())
                            {
                                if (Regex.IsMatch(((RDFTypedLiteral)crMinQC.Object).Value, @"^[0-9]+$"))
                                {
                                    minQC = int.Parse(((RDFTypedLiteral)crMinQC.Object).Value);
                                }
                            }
                        }
                    }

                    int maxQC = 0;
                    var crMaxQC = maxQualifiedCardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crMaxQC != null && crMaxQC.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        if (crMaxQC.Object is RDFPlainLiteral)
                        {
                            if (Regex.IsMatch(crMaxQC.Object.ToString(), @"^[0-9]+$"))
                            {
                                maxQC = int.Parse(crMaxQC.Object.ToString());
                            }
                        }
                        else
                        {
                            if (((RDFTypedLiteral)crMaxQC.Object).HasDecimalDatatype())
                            {
                                if (Regex.IsMatch(((RDFTypedLiteral)crMaxQC.Object).Value, @"^[0-9]+$"))
                                {
                                    maxQC = int.Parse(((RDFTypedLiteral)crMaxQC.Object).Value);
                                }
                            }
                        }
                    }

                    if (minQC > 0 || maxQC > 0)
                    {
                        //OnClass
                        var minmaxQCCls = onClass.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                        if (minmaxQCCls != null && minmaxQCCls.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var minmaxQCOnClass = ontology.Model.ClassModel.SelectClass(minmaxQCCls.Object.ToString());
                            if (minmaxQCOnClass != null)
                            {
                                var qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)r.Value, r.OnProperty, minmaxQCOnClass, minQC, maxQC);
                                ontology.Model.ClassModel.Classes[r.PatternMemberID] = qualifCardRestr;
                            }
                            else
                            {
                                //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onClass is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onClass '{1}' is not found in the model.", r.Value, minmaxQCCls.Object));
                            }
                        }
                        else
                        {
                            //OnDataRange
                            var minmaxQCDrn = onDataRange.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                            if (minmaxQCDrn != null && minmaxQCDrn.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                var minmaxQCOnDataRange = ontology.Model.ClassModel.SelectClass(minmaxQCDrn.Object.ToString());
                                if (minmaxQCOnDataRange != null)
                                {
                                    var qualifCardRestr = new RDFOntologyQualifiedCardinalityRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, minmaxQCOnDataRange, minQC, maxQC);
                                    ontology.Model.ClassModel.Classes[r.PatternMemberID] = qualifCardRestr;
                                    continue;
                                }
                                else
                                {
                                    //Raise warning event to inform the user: qualified cardinality restriction cannot be imported from graph, because definition of its required onDataRange is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("QualifiedCardinalityRestriction '{0}' cannot be imported from graph, because definition of its required onDataRange '{1}' is not found in the model.", r.Value, minmaxQCDrn.Object));
                                }
                            }
                        }
                    }
                    #endregion

                }
                foreach (var r in restrictions.OfType<RDFOntologyHasSelfRestriction>())
                {

                    #region HasSelf [OWL2]
                    var hsRes = hasself.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (hsRes != null)
                    {
                        if (hsRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL
                                && hsRes.Object.Equals(new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)))
                        {
                            var hasselfRestr = new RDFOntologyHasSelfRestriction((RDFResource)r.Value, r.OnProperty);
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = hasselfRestr;
                        }
                    }
                    #endregion

                }
                foreach (var r in restrictions.OfType<RDFOntologyHasValueRestriction>())
                {

                    #region HasValue
                    var hvRes = hasvalue.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (hvRes != null)
                    {
                        if (hvRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {

                            //Create the fact even if not explicitly classtyped
                            var hvFct = ontology.Data.SelectFact(hvRes.Object.ToString());
                            if (hvFct == null)
                            {
                                hvFct = (new RDFResource(hvRes.Object.ToString())).ToRDFOntologyFact();
                                ontology.Data.AddFact(hvFct);
                            }

                            var hasvalueRestr = new RDFOntologyHasValueRestriction((RDFResource)r.Value, r.OnProperty, hvFct);
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = hasvalueRestr;
                        }
                        else
                        {
                            var hasvalueRestr = new RDFOntologyHasValueRestriction((RDFResource)r.Value, r.OnProperty, ((RDFLiteral)hvRes.Object).ToRDFOntologyLiteral());
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = hasvalueRestr;
                        }
                    }
                    #endregion

                }
                foreach (var r in restrictions.OfType<RDFOntologyAllValuesFromRestriction>())
                {

                    #region AllValuesFrom
                    var avfRes = allvaluesFrom.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (avfRes != null && avfRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var avfCls = ontology.Model.ClassModel.SelectClass(avfRes.Object.ToString());
                        if (avfCls != null)
                        {
                            var allvaluesfromRestr = new RDFOntologyAllValuesFromRestriction((RDFResource)r.Value, r.OnProperty, avfCls);
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = allvaluesfromRestr;
                        }
                        else
                        {
                            //Raise warning event to inform the user: allvaluesfrom restriction cannot be imported from graph, because definition of its required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Restriction '{0}' cannot be imported from graph, because definition of its required class '{1}' is not found in the model.", r.Value, avfRes.Object));
                        }
                    }
                    #endregion

                }
                foreach (var r in restrictions.OfType<RDFOntologySomeValuesFromRestriction>())
                {

                    #region SomeValuesFrom
                    var svfRes = somevaluesFrom.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (svfRes != null && svfRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var svfCls = ontology.Model.ClassModel.SelectClass(svfRes.Object.ToString());
                        if (svfCls != null)
                        {
                            var somevaluesfromRestr = new RDFOntologySomeValuesFromRestriction((RDFResource)r.Value, r.OnProperty, svfCls);
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = somevaluesfromRestr;
                        }
                        else
                        {
                            //Raise warning event to inform the user: somevaluesfrom restriction cannot be imported from graph, because definition of its required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Restriction '{0}' cannot be imported from graph, because definition of its required class '{1}' is not found in the model.", r.Value, svfRes.Object));
                        }
                    }
                    #endregion

                }
                #endregion

                #region Step 6.2: Finalize OWL:OneOf (Enumerate)
                foreach (var e in oneOf)
                {
                    if (e.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var ec = ontology.Model.ClassModel.SelectClass(e.Subject.ToString());
                        if (ec != null && !ec.IsDataRangeClass())
                        {

                            #region ClassToEnumerateClass
                            if (!ec.IsEnumerateClass())
                            {
                                ec = new RDFOntologyEnumerateClass((RDFResource)e.Subject);
                                ontology.Model.ClassModel.Classes[ec.PatternMemberID] = ec;
                            }
                            #endregion

                            #region DeserializeEnumerateCollection
                            var nilFound = false;
                            var itemRest = (RDFResource)e.Object;
                            while (!nilFound)
                            {

                                #region rdf:first
                                var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                     .FirstOrDefault();
                                if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {

                                    //Create the fact even if not explicitly classtyped
                                    var enumMember = ontology.Data.SelectFact(first.Object.ToString());
                                    if (enumMember == null)
                                    {
                                        enumMember = (new RDFResource(first.Object.ToString())).ToRDFOntologyFact();
                                        ontology.Data.AddFact(enumMember);
                                    }
                                    ontology.Model.ClassModel.AddOneOfRelation((RDFOntologyEnumerateClass)ec, new List<RDFOntologyFact>() { enumMember });

                                    #region rdf:rest
                                    var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                              .FirstOrDefault();
                                    if (rest != null)
                                    {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        {
                                            nilFound = true;
                                        }
                                        else
                                        {
                                            itemRest = (RDFResource)rest.Object;
                                        }
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
                        else
                        {
                            if (ec == null)
                            {
                                //Raise warning event to inform the user: enumerate class cannot be imported from graph, because its definition is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EnumerateClass '{0}' cannot be imported from graph, because its definition is not found in the model.", e.Subject));
                            }
                        }
                    }
                }
                #endregion

                #region Step 6.3: Finalize OWL:OneOf (DataRange)
                foreach (var d in oneOf)
                {
                    if (d.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var dr = ontology.Model.ClassModel.SelectClass(d.Subject.ToString());
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
                            var nilFound = false;
                            var itemRest = (RDFResource)d.Object;
                            while (!nilFound)
                            {

                                #region rdf:first
                                var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                     .FirstOrDefault();
                                if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                                {
                                    ontology.Model.ClassModel.AddOneOfRelation((RDFOntologyDataRangeClass)dr, new List<RDFOntologyLiteral>() { ((RDFLiteral)first.Object).ToRDFOntologyLiteral() });
                                    ontology.Data.AddLiteral(((RDFLiteral)first.Object).ToRDFOntologyLiteral());

                                    #region rdf:rest
                                    var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                              .FirstOrDefault();
                                    if (rest != null)
                                    {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        {
                                            nilFound = true;
                                        }
                                        else
                                        {
                                            itemRest = (RDFResource)rest.Object;
                                        }
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
                        else
                        {
                            if (dr == null)
                            {
                                //Raise warning event to inform the user: datarange class cannot be imported from graph, because its definition is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DataRangeClass '{0}' cannot be imported from graph, because its definition is not found in the model.", d.Subject));
                            }
                        }
                    }
                }
                #endregion

                #region Step 6.4: Finalize RDFS:[Domain|Range]
                foreach (var p in ontology.Model.PropertyModel.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop)
                                                                                  && !prop.IsAnnotationProperty()))
                {

                    #region Domain
                    var d = domain.SelectTriplesBySubject((RDFResource)p.Value).FirstOrDefault();
                    if (d != null && d.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var domainClass = ontology.Model.ClassModel.SelectClass(d.Object.ToString());
                        if (domainClass != null)
                        {
                            p.SetDomain(domainClass);
                        }
                        else
                        {
                            //Raise warning event to inform the user: domain constraint cannot be imported from graph because definition of required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Domain constraint on property '{0}' cannot be imported from graph because definition of required class '{1}' is not found in the model.", p.Value, d.Object));
                        }
                    }
                    #endregion

                    #region Range
                    var r = range.SelectTriplesBySubject((RDFResource)p.Value).FirstOrDefault();
                    if (r != null && r.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var rangeClass = ontology.Model.ClassModel.SelectClass(r.Object.ToString());
                        if (rangeClass != null)
                        {
                            p.SetRange(rangeClass);
                        }
                        else
                        {
                            //Raise warning event to inform the user: range constraint cannot be imported from graph because definition of required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Range constraint on property '{0}' cannot be imported from graph because definition of required class '{1}' is not found in the model.", p.Value, r.Object));
                        }
                    }
                    #endregion

                }
                #endregion

                #region Step 6.5: Finalize PropertyModel [RDFS:SubPropertyOf|OWL:EquivalentProperty|OWL:PropertyDisjointWith|OWL:AllDisjointProperties|OWL:PropertyChainAxiom|OWL:InverseOf]
                foreach (var p in ontology.Model.PropertyModel.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop)
                                                                                  && !prop.IsAnnotationProperty()))
                {

                    #region SubPropertyOf
                    foreach (var spof in subpropOf.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (spof.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var superProp = ontology.Model.PropertyModel.SelectProperty(spof.Object.ToString());
                            if (superProp != null)
                            {
                                if (p.IsObjectProperty() && superProp.IsObjectProperty())
                                {
                                    ontology.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)p, (RDFOntologyObjectProperty)superProp);
                                }
                                else if (p.IsDatatypeProperty() && superProp.IsDatatypeProperty())
                                {
                                    ontology.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)p, (RDFOntologyDatatypeProperty)superProp);
                                }
                            }
                            else
                            {
                                //Raise warning event to inform the user: subpropertyof relation cannot be imported from graph because definition of property is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubPropertyOf relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model or represents an annotation property.", p.Value, spof.Object));
                            }
                        }
                    }
                    #endregion

                    #region EquivalentProperty
                    foreach (var eqpr in equivpropOf.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (eqpr.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var equivProp = ontology.Model.PropertyModel.SelectProperty(eqpr.Object.ToString());
                            if (equivProp != null)
                            {
                                if (p.IsObjectProperty() && equivProp.IsObjectProperty())
                                {
                                    ontology.Model.PropertyModel.AddEquivalentPropertyRelation((RDFOntologyObjectProperty)p, (RDFOntologyObjectProperty)equivProp);
                                }
                                else if (p.IsDatatypeProperty() && equivProp.IsDatatypeProperty())
                                {
                                    ontology.Model.PropertyModel.AddEquivalentPropertyRelation((RDFOntologyDatatypeProperty)p, (RDFOntologyDatatypeProperty)equivProp);
                                }
                            }
                            else
                            {
                                //Raise warning event to inform the user: equivalentproperty relation cannot be imported from graph, because definition of property is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentProperty relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model.", p.Value, eqpr.Object));
                            }
                        }
                    }
                    #endregion

                    #region PropertyDisjointWith [OWL2]
                    foreach (var dwpr in disjpropWith.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (dwpr.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var disjProp = ontology.Model.PropertyModel.SelectProperty(dwpr.Object.ToString());
                            if (disjProp != null)
                            {
                                if (p.IsObjectProperty() && disjProp.IsObjectProperty())
                                {
                                    ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyObjectProperty)p, (RDFOntologyObjectProperty)disjProp);
                                }
                                else if (p.IsDatatypeProperty() && disjProp.IsDatatypeProperty())
                                {
                                    ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyDatatypeProperty)p, (RDFOntologyDatatypeProperty)disjProp);
                                }
                            }
                            else
                            {
                                //Raise warning event to inform the user: propertyDisjointWith relation cannot be imported from graph because definition of property is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyDisjointWith relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model.", p.Value, dwpr.Object));
                            }
                        }
                    }
                    #endregion

                    #region AllDisjointProperties [OWL2]
                    foreach (var adjp in alldisjprops)
                    {
                        var allDisjointProperties = new List<RDFOntologyProperty>();
                        foreach (var adjpMembers in members.SelectTriplesBySubject((RDFResource)adjp.Subject))
                        {
                            if (adjpMembers.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                #region DeserializeCollection
                                var nilFound = false;
                                var itemRest = (RDFResource)adjpMembers.Object;
                                while (!nilFound)
                                {

                                    #region rdf:first
                                    var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                        .FirstOrDefault();
                                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                    {
                                        var disjointProperty = ontology.Model.PropertyModel.SelectProperty(first.Object.ToString());
                                        if (disjointProperty != null)
                                        {
                                            allDisjointProperties.Add(disjointProperty);
                                        }
                                        else
                                        {
                                            //Raise warning event to inform the user: all disjoint properties cannot be completely imported from graph because definition of property is not found in the model
                                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllDisjointProperties '{0}' cannot be completely imported from graph because definition of property '{1}' is not found in the model.", adjp.Subject, first.Object));
                                        }

                                        #region rdf:rest
                                        var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                          .FirstOrDefault();
                                        if (rest != null)
                                        {
                                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                            {
                                                nilFound = true;
                                            }
                                            else
                                            {
                                                itemRest = (RDFResource)rest.Object;
                                            }
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
                        }

                        //Replicate algorythm of AddAllDisjointProperties (it requires strong typing of properties)
                        foreach (var outerProperty in allDisjointProperties)
                        {
                            foreach (var innerProperty in allDisjointProperties)
                            {
                                if (outerProperty.IsObjectProperty() && innerProperty.IsObjectProperty())
                                    ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyObjectProperty)outerProperty, (RDFOntologyObjectProperty)innerProperty);
                                else if (outerProperty.IsDatatypeProperty() && innerProperty.IsDatatypeProperty())
                                    ontology.Model.PropertyModel.AddPropertyDisjointWithRelation((RDFOntologyDatatypeProperty)outerProperty, (RDFOntologyDatatypeProperty)innerProperty);
                            }
                        }
                    }
                    #endregion

                    #region PropertyChainAxiom [OWL2]
                    foreach (var pca in propertyChainAxiom.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        var pcaProperty = ontology.Model.PropertyModel.SelectProperty(pca.Subject.ToString());
                        if (pcaProperty != null)
                        {
                            if (pcaProperty is RDFOntologyObjectProperty)
                            {
                                var pcaProperties = new List<RDFOntologyObjectProperty>();
                                if (pca.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {
                                    #region DeserializeCollection
                                    var nilFound = false;
                                    var itemRest = (RDFResource)pca.Object;
                                    while (!nilFound)
                                    {

                                        #region rdf:first
                                        var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                            .FirstOrDefault();
                                        if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                        {
                                            var pcaProp = ontology.Model.PropertyModel.SelectProperty(first.Object.ToString());
                                            if (pcaProp != null)
                                            {
                                                if (pcaProp is RDFOntologyObjectProperty)
                                                {
                                                    pcaProperties.Add((RDFOntologyObjectProperty)pcaProp);
                                                }
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
                                            var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                              .FirstOrDefault();
                                            if (rest != null)
                                            {
                                                if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                                {
                                                    nilFound = true;
                                                }
                                                else
                                                {
                                                    itemRest = (RDFResource)rest.Object;
                                                }
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
                                //Raise warning event to inform the user: PropertyChainAxiom relation cannot be imported from graph, because property is not an object property
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyChainAxiom relation on property '{0}' cannot be imported from graph, because the property is not an object property.", pca.Subject));
                            }
                        }
                        else
                        {
                            //Raise warning event to inform the user: PropertyChainAxiom relation cannot be imported from graph, because definition of property is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PropertyChainAxiom relation on property '{0}' cannot be imported from graph, because definition of the property is not found in the model.", pca.Subject));
                        }
                    }
                    #endregion

                    #region InverseOf
                    if (p.IsObjectProperty())
                    {
                        foreach (var inof in inverseOf.SelectTriplesBySubject((RDFResource)p.Value))
                        {
                            if (inof.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                var invProp = ontology.Model.PropertyModel.SelectProperty(inof.Object.ToString());
                                if (invProp != null && invProp.IsObjectProperty())
                                {
                                    ontology.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)p, (RDFOntologyObjectProperty)invProp);
                                }
                                else
                                {
                                    //Raise warning event to inform the user: inverseof relation cannot be imported from graph because definition of property is not found in the model
                                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("InverseOf relation on property '{0}' cannot be imported from graph because definition of property '{1}' is not found in the model.", p.Value, inof.Object));
                                }
                            }
                        }
                    }
                    #endregion

                }
                #endregion

                #region Step 6.6: Finalize ClassModel [RDFS:SubClassOf|OWL:EquivalentClass|OWL:DisjointWith|OWL:AllDisjointClasses|OWL:HasKey]]
                foreach (var c in ontology.Model.ClassModel.Where(cls => !RDFOntologyChecker.CheckReservedClass(cls)))
                {

                    #region SubClassOf
                    foreach (var scof in subclassOf.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (scof.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var superClass = ontology.Model.ClassModel.SelectClass(scof.Object.ToString());
                            if (superClass != null)
                            {
                                ontology.Model.ClassModel.AddSubClassOfRelation(c, superClass);
                            }
                            else
                            {
                                //Raise warning event to inform the user: subclassof relation cannot be imported from graph, because definition of class is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubClassOf relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", c.Value, scof.Object));
                            }
                        }
                    }
                    #endregion

                    #region EquivalentClass
                    foreach (var eqcl in equivclassOf.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (eqcl.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var equivClass = ontology.Model.ClassModel.SelectClass(eqcl.Object.ToString());
                            if (equivClass != null)
                            {
                                ontology.Model.ClassModel.AddEquivalentClassRelation(c, equivClass);
                            }
                            else
                            {
                                //Raise warning event to inform the user: equivalentclass relation cannot be imported from graph, because definition of class is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentClass relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", c.Value, eqcl.Object));
                            }
                        }
                    }
                    #endregion

                    #region DisjointWith
                    foreach (var djwt in disjclassWith.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (djwt.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            var disjWith = ontology.Model.ClassModel.SelectClass(djwt.Object.ToString());
                            if (disjWith != null)
                            {
                                ontology.Model.ClassModel.AddDisjointWithRelation(c, disjWith);
                            }
                            else
                            {
                                //Raise warning event to inform the user: disjointwith relation cannot be imported from graph, because definition of class is not found in the model
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DisjointWith relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", c.Value, djwt.Object));
                            }
                        }
                    }
                    #endregion

                    #region AllDisjointClasses [OWL2]
                    foreach (var adjc in alldisjclasses)
                    {
                        var allDisjointClasses = new List<RDFOntologyClass>();
                        foreach (var adjcMembers in members.SelectTriplesBySubject((RDFResource)adjc.Subject))
                        {
                            if (adjcMembers.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                #region DeserializeCollection
                                var nilFound = false;
                                var itemRest = (RDFResource)adjcMembers.Object;
                                while (!nilFound)
                                {

                                    #region rdf:first
                                    var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                        .FirstOrDefault();
                                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                    {
                                        var disjointClass = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                        if (disjointClass != null)
                                        {
                                            allDisjointClasses.Add(disjointClass);
                                        }
                                        else
                                        {
                                            //Raise warning event to inform the user: all disjoint classes cannot be completely imported from graph because definition of class is not found in the model
                                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllDisjointClasses '{0}' cannot be completely imported from graph because definition of class '{1}' is not found in the model.", adjc.Subject, first.Object));
                                        }

                                        #region rdf:rest
                                        var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                          .FirstOrDefault();
                                        if (rest != null)
                                        {
                                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                            {
                                                nilFound = true;
                                            }
                                            else
                                            {
                                                itemRest = (RDFResource)rest.Object;
                                            }
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
                        }
                        ontology.Model.ClassModel.AddAllDisjointClassesRelation(allDisjointClasses);
                    }
                    #endregion

                    #region HasKey [OWL2]
                    foreach (var haskey in hasKey.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        var haskeyClass = ontology.Model.ClassModel.SelectClass(haskey.Subject.ToString());
                        if (haskeyClass != null)
                        {
                            var keyProps = new List<RDFOntologyProperty>();
                            if (haskey.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                #region DeserializeCollection
                                var nilFound = false;
                                var itemRest = (RDFResource)haskey.Object;
                                while (!nilFound)
                                {

                                    #region rdf:first
                                    var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                        .FirstOrDefault();
                                    if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                    {
                                        var keyProp = ontology.Model.PropertyModel.SelectProperty(first.Object.ToString());
                                        if (keyProp != null)
                                        {
                                            keyProps.Add(keyProp);
                                        }
                                        else
                                        {
                                            //Raise warning event to inform the user: hasKey cannot be completely imported from graph because definition of property is not found in the model
                                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("HasKey relation '{0}' cannot be completely imported from graph because definition of key property '{1}' is not found in the model.", haskey.Subject, first.Object));
                                        }

                                        #region rdf:rest
                                        var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                          .FirstOrDefault();
                                        if (rest != null)
                                        {
                                            if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                            {
                                                nilFound = true;
                                            }
                                            else
                                            {
                                                itemRest = (RDFResource)rest.Object;
                                            }
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
                #endregion

                #region Step 6.7: Finalize Data [OWL:SameAs|OWL:DifferentFrom|OWL:AllDifferent|SKOS:OrderedCollection|ASSERTIONS|NEGATIVE ASSERTIONS]

                #region SameAs
                foreach (var t in sameAs)
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {

                        //Create the fact even if not explicitly classtyped
                        var subjFct = ontology.Data.SelectFact(t.Subject.ToString());
                        if (subjFct == null)
                        {
                            subjFct = (new RDFResource(t.Subject.ToString())).ToRDFOntologyFact();
                            ontology.Data.AddFact(subjFct);
                        }

                        //Create the fact even if not explicitly classtyped
                        var objFct = ontology.Data.SelectFact(t.Object.ToString());
                        if (objFct == null)
                        {
                            objFct = (new RDFResource(t.Object.ToString())).ToRDFOntologyFact();
                            ontology.Data.AddFact(objFct);
                        }

                        ontology.Data.AddSameAsRelation(subjFct, objFct);
                    }
                }
                #endregion

                #region DifferentFrom
                foreach (var t in differentFrom)
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {

                        //Create the fact even if not explicitly classtyped
                        var subjFct = ontology.Data.SelectFact(t.Subject.ToString());
                        if (subjFct == null)
                        {
                            subjFct = (new RDFResource(t.Subject.ToString())).ToRDFOntologyFact();
                            ontology.Data.AddFact(subjFct);
                        }

                        //Create the fact even if not explicitly classtyped
                        var objFct = ontology.Data.SelectFact(t.Object.ToString());
                        if (objFct == null)
                        {
                            objFct = (new RDFResource(t.Object.ToString())).ToRDFOntologyFact();
                            ontology.Data.AddFact(objFct);
                        }

                        ontology.Data.AddDifferentFromRelation(subjFct, objFct);
                    }
                }
                #endregion

                #region AllDifferent [OWL2]
                foreach (var adif in alldifferent)
                {
                    var allDifferentFacts = new List<RDFOntologyFact>();
                    foreach (var adifMembers in distinctMembers.SelectTriplesBySubject((RDFResource)adif.Subject))
                    {
                        if (adifMembers.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            #region DeserializeCollection
                            var nilFound = false;
                            var itemRest = (RDFResource)adifMembers.Object;
                            while (!nilFound)
                            {

                                #region rdf:first
                                var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                    .FirstOrDefault();
                                if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {
                                    var differentMember = ontology.Data.SelectFact(first.Object.ToString());
                                    if (differentMember != null)
                                    {
                                        allDifferentFacts.Add(differentMember);
                                    }
                                    else
                                    {
                                        //Raise warning event to inform the user: all different cannot be completely imported from graph because definition of fact is not found in the data
                                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("AllDifferent '{0}' cannot be completely imported from graph because definition of fact '{1}' is not found in the data.", adif.Subject, first.Object));
                                    }

                                    #region rdf:rest
                                    var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                      .FirstOrDefault();
                                    if (rest != null)
                                    {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                        {
                                            nilFound = true;
                                        }
                                        else
                                        {
                                            itemRest = (RDFResource)rest.Object;
                                        }
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
                    }
                    ontology.Data.AddAllDifferentRelation(allDifferentFacts);
                }
                #endregion

                #region OrderedCollection [SKOS]
                foreach (var ordCol in skosMemberList)
                {
                    if (ordCol.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        var ordColRepresentFact = new RDFOntologyFact((RDFResource)ordCol.Object);

                        //Representative of skos:OrderedCollection is a fact of type rdf:List
                        ontology.Data.AddFact(ordColRepresentFact);

                        #region DeserializeOrderedCollection
                        var nilFound = false;
                        var itemRest = (RDFResource)ordCol.Object;
                        while (!nilFound)
                        {
                            ontology.Data.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(itemRest.ToRDFOntologyFact(), RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.LIST.ToRDFOntologyClass()));

                            #region rdf:first
                            var first = rdfFirst.SelectTriplesBySubject(itemRest)
                                                           .FirstOrDefault();
                            if (first != null && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {

                                //Items of skos:OrderedCollection are facts of type skos:Concept
                                ontology.Data.AddFact(((RDFResource)first.Object).ToRDFOntologyFact());
                                ontology.Data.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(((RDFResource)first.Object).ToRDFOntologyFact(), RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass()));
                                ontology.Data.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(itemRest.ToRDFOntologyFact(), RDFVocabulary.RDF.FIRST.ToRDFOntologyObjectProperty(), ((RDFResource)first.Object).ToRDFOntologyFact()));

                                #region rdf:rest
                                var rest = rdfRest.SelectTriplesBySubject(itemRest)
                                                          .FirstOrDefault();
                                if (rest != null)
                                {
                                    if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                                    {
                                        ontology.Data.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(itemRest.ToRDFOntologyFact(), RDFVocabulary.RDF.REST.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.NIL.ToRDFOntologyFact()));
                                        nilFound = true;
                                    }
                                    else
                                    {
                                        ontology.Data.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(itemRest.ToRDFOntologyFact(), RDFVocabulary.RDF.REST.ToRDFOntologyObjectProperty(), ((RDFResource)rest.Object).ToRDFOntologyFact()));
                                        itemRest = (RDFResource)rest.Object;
                                    }
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
                }
                #endregion

                #region NegativeAssertions [OWL2]
                foreach (var nAsn in negativeAssertions)
                {
                    #region owl:SourceIndividual
                    RDFPatternMember sIndividual = sourceIndividual.SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                    if (sIndividual == null || sIndividual is RDFLiteral)
                    {
                        //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because owl:SourceIndividual triple is not found in the graph or it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because owl:SourceIndividual triple is not found in the graph or it does not link a resource.", nAsn.Subject));
                        continue;
                    }

                    //Create the source individual fact even if not explicitly classtyped
                    var sIndividualFact = ontology.Data.SelectFact(sIndividual.ToString());
                    if (sIndividualFact == null)
                    {
                        sIndividualFact = ((RDFResource)sIndividual).ToRDFOntologyFact();
                        ontology.Data.AddFact(sIndividualFact);
                    }
                    #endregion

                    #region owl:AssertionProperty
                    RDFPatternMember asnProperty = assertionProperty.SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                    if (asnProperty == null || asnProperty is RDFLiteral)
                    {
                        //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because owl:AssertionProperty triple is not found in the graph or it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because owl:AssertionProperty triple is not found in the graph or it does not link a resource.", nAsn.Subject));
                        continue;
                    }

                    //Check if property exists in the property model
                    var apProperty = ontology.Model.PropertyModel.SelectProperty(asnProperty.ToString());
                    if (apProperty == null)
                    {
                        //Raise warning event to inform the user: negative assertion relation cannot be imported from graph, because owl:AssertionProperty is not a declared property
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation '{0}' cannot be imported from graph, because owl:AssertionProperty '{1}' is not a declared property.", nAsn.Subject, asnProperty));
                        continue;
                    }
                    #endregion

                    #region owl:TargetIndividual
                    RDFPatternMember tIndividual = targetIndividual.SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                    if (tIndividual != null)
                    {
                        //We found owl:TargetIndividual, so we can accept it only if it's a resource
                        //and the negative assertion's property is effectively an object property
                        if (tIndividual is RDFResource && apProperty is RDFOntologyObjectProperty)
                        {
                            //Create the target individual fact even if not explicitly classtyped
                            var tIndividualFact = ontology.Data.SelectFact(tIndividual.ToString());
                            if (tIndividualFact == null)
                            {
                                tIndividualFact = ((RDFResource)tIndividual).ToRDFOntologyFact();
                                ontology.Data.AddFact(tIndividualFact);
                            }

                            //Finally, add negative assertion with all retrieved informations (SPO)
                            ontology.Data.AddNegativeAssertionRelation(sIndividualFact, (RDFOntologyObjectProperty)apProperty, tIndividualFact);
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
                    RDFPatternMember tValue = targetValue.SelectTriplesBySubject((RDFResource)nAsn.Subject).FirstOrDefault()?.Object;
                    if (tValue != null)
                    {
                        //We found owl:TargetValue, so we can accept it only if it's a literal
                        //and the negative assertion's property is effectively a datatype property
                        if (tValue is RDFLiteral && apProperty is RDFOntologyDatatypeProperty)
                        {
                            //Create the target value literal even if not explicitly declared
                            var tValueLit = ontology.Data.SelectLiteral(tValue.ToString());
                            if (tValueLit == null)
                            {
                                tValueLit = ((RDFLiteral)tValue).ToRDFOntologyLiteral();
                                ontology.Data.AddLiteral(tValueLit);
                            }

                            //Finally, add negative assertion with all retrieved informations (SPL)
                            ontology.Data.AddNegativeAssertionRelation(sIndividualFact, (RDFOntologyDatatypeProperty)apProperty, tValueLit);
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
                foreach (var p in ontology.Model.PropertyModel.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop)
                                                                                  && !prop.IsAnnotationProperty()))
                {
                    foreach (var t in ontGraph.SelectTriplesByPredicate((RDFResource)p.Value).Where(triple => !triple.Subject.Equals(ontology)
                                                                                                                 && !ontology.Model.ClassModel.Classes.ContainsKey(triple.Subject.PatternMemberID)
                                                                                                                    && !ontology.Model.PropertyModel.Properties.ContainsKey(triple.Subject.PatternMemberID)))
                    {
                        //Create the fact even if not explicitly classtyped
                        var subjFct = ontology.Data.SelectFact(t.Subject.ToString());
                        if (subjFct == null)
                        {
                            subjFct = (new RDFResource(t.Subject.ToString())).ToRDFOntologyFact();
                            ontology.Data.AddFact(subjFct);
                        }

                        //Check if the property is an owl:ObjectProperty
                        if (p.IsObjectProperty())
                        {
                            if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                //Create the fact even if not explicitly classtyped
                                var objFct = ontology.Data.SelectFact(t.Object.ToString());
                                if (objFct == null)
                                {
                                    objFct = (new RDFResource(t.Object.ToString())).ToRDFOntologyFact();
                                    ontology.Data.AddFact(objFct);
                                }

                                ontology.Data.AddAssertionRelation(subjFct, (RDFOntologyObjectProperty)p, objFct);
                            }
                            else
                            {
                                //Raise warning event to inform the user: assertion relation cannot be imported from graph, because object property links to a literal
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation on fact '{0}' cannot be imported from graph, because object property '{1}' links to a literal.", t.Subject, p));
                            }
                        }

                        //Check if the property is an owl:DatatypeProperty
                        else if (p.IsDatatypeProperty())
                        {
                            if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            {
                                ontology.Data.AddAssertionRelation(subjFct, (RDFOntologyDatatypeProperty)p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                            }
                            else
                            {
                                //Raise warning event to inform the user: assertion relation cannot be imported from graph, because datatype property links to a fact
                                RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation on fact '{0}' cannot be imported from graph, because datatype property '{1}' links to a fact.", t.Subject, p));
                            }
                        }
                    }
                }
                #endregion

                #endregion

                #region Step 6.8: Finalize Annotations

                #region Ontology

                #region VersionInfo
                foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    }
                    else
                    {
                        //Raise warning event to inform the user: versioninfo annotation on ontology cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value));
                    }
                }
                #endregion

                #region VersionIRI
                foreach (var t in versionIRI.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, new RDFOntology((RDFResource)t.Object));
                    }
                    else
                    {
                        //Raise warning event to inform the user: versioniri annotation on ontology cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionIRI annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                    }
                }
                #endregion

                #region Comment
                foreach (var t in comment.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    }
                    else
                    {
                        //Raise warning event to inform the user: comment annotation on ontology cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value));
                    }
                }
                #endregion

                #region Label
                foreach (var t in label.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    }
                    else
                    {
                        //Raise warning event to inform the user: label annotation on ontology cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value));
                    }
                }
                #endregion

                #region SeeAlso
                foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    }
                    else
                    {
                        RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (resource == null)
                        {
                            resource = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (resource == null)
                            {
                                resource = ontology.Data.SelectFact(t.Object.ToString());
                                if (resource == null)
                                {
                                    resource = new RDFOntologyResource();
                                    resource.Value = t.Object;
                                    resource.PatternMemberID = t.Object.PatternMemberID;
                                }
                            }
                        }
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, resource);
                    }
                }
                #endregion

                #region IsDefinedBy
                foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                    }
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
                                {
                                    isDefBy = new RDFOntologyResource();
                                    isDefBy.Value = t.Object;
                                    isDefBy.PatternMemberID = t.Object.PatternMemberID;
                                }
                            }
                        }
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, isDefBy);
                    }
                }
                #endregion

                #region BackwardCompatibleWith
                foreach (var t in bcwcompWith.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, new RDFOntology((RDFResource)t.Object));
                    }
                    else
                    {
                        //Raise warning event to inform the user: backwardcompatiblewith annotation on ontology cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("BackwardCompatibleWith annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                    }
                }
                #endregion

                #region IncompatibleWith
                foreach (var t in incompWith.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, new RDFOntology((RDFResource)t.Object));
                    }
                    else
                    {
                        //Raise warning event to inform the user: incompatiblewith annotation on ontology cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("IncompatibleWith annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                    }
                }
                #endregion

                #region PriorVersion
                foreach (var t in priorVersion.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, new RDFOntology((RDFResource)t.Object));
                    }
                    else
                    {
                        //Raise warning event to inform the user: priorversion annotation on ontology cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("PriorVersion annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                    }
                }
                #endregion

                #region Imports
                foreach (var t in imports.SelectTriplesBySubject((RDFResource)ontology.Value))
                {
                    if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, new RDFOntology((RDFResource)t.Object));
                    }
                    else
                    {
                        //Raise warning event to inform the user: imports annotation on ontology cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Imports annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value));
                    }
                }
                #endregion

                #region CustomAnnotations
                var annotProps = ontology.Model.PropertyModel.OfType<RDFOntologyAnnotationProperty>();
                foreach (var annotProp in annotProps)
                {
                    foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)ontology.Value)
                                              .SelectTriplesByPredicate((RDFResource)annotProp.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.AddCustomAnnotation(annotProp, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
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
                                    {
                                        custAnnValue = new RDFOntologyResource
                                        {
                                            Value = t.Object,
                                            PatternMemberID = t.Object.PatternMemberID
                                        };
                                    }
                                }
                            }
                            ontology.AddCustomAnnotation(annotProp, custAnnValue);
                        }
                    }

                }
                #endregion

                #endregion

                #region Classes
                foreach (var c in ontology.Model.ClassModel)
                {

                    #region VersionInfo
                    foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: versioninfo annotation on class cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value));
                        }
                    }
                    #endregion

                    #region Comment
                    foreach (var t in comment.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: comment annotation on class cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value));
                        }
                    }
                    #endregion

                    #region Label
                    foreach (var t in label.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: label annotation on class cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value));
                        }
                    }
                    #endregion

                    #region SeeAlso
                    foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (resource == null)
                            {
                                resource = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (resource == null)
                                {
                                    resource = ontology.Data.SelectFact(t.Object.ToString());
                                    if (resource == null)
                                    {
                                        resource = new RDFOntologyResource();
                                        resource.Value = t.Object;
                                        resource.PatternMemberID = t.Object.PatternMemberID;
                                    }
                                }
                            }
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, c, resource);
                        }
                    }
                    #endregion

                    #region IsDefinedBy
                    foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)c.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
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
                                    {
                                        isDefBy = new RDFOntologyResource();
                                        isDefBy.Value = t.Object;
                                        isDefBy.PatternMemberID = t.Object.PatternMemberID;
                                    }
                                }
                            }
                            ontology.Model.ClassModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, c, isDefBy);
                        }
                    }
                    #endregion

                    #region CustomAnnotations
                    foreach (var annotProp in annotProps)
                    {
                        foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)c.Value)
                                                  .SelectTriplesByPredicate((RDFResource)annotProp.Value))
                        {
                            if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            {
                                ontology.Model.ClassModel.AddCustomAnnotation(annotProp, c, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                            }
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
                                        {
                                            custAnnValue = new RDFOntologyResource
                                            {
                                                Value = t.Object,
                                                PatternMemberID = t.Object.PatternMemberID
                                            };
                                        }
                                    }
                                }
                                ontology.Model.ClassModel.AddCustomAnnotation(annotProp, c, custAnnValue);
                            }
                        }

                    }
                    #endregion

                }
                #endregion

                #region Properties
                foreach (var p in ontology.Model.PropertyModel)
                {

                    #region VersionInfo
                    foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: versioninfo annotation on property cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value));
                        }
                    }
                    #endregion

                    #region Comment
                    foreach (var t in comment.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: comment annotation on property cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value));
                        }
                    }
                    #endregion

                    #region Label
                    foreach (var t in label.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: label annotation on property cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value));
                        }
                    }
                    #endregion

                    #region SeeAlso
                    foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (resource == null)
                            {
                                resource = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (resource == null)
                                {
                                    resource = ontology.Data.SelectFact(t.Object.ToString());
                                    if (resource == null)
                                    {
                                        resource = new RDFOntologyResource();
                                        resource.Value = t.Object;
                                        resource.PatternMemberID = t.Object.PatternMemberID;
                                    }
                                }
                            }
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, p, resource);
                        }
                    }
                    #endregion

                    #region IsDefinedBy
                    foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)p.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
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
                                    {
                                        isDefBy = new RDFOntologyResource();
                                        isDefBy.Value = t.Object;
                                        isDefBy.PatternMemberID = t.Object.PatternMemberID;
                                    }
                                }
                            }
                            ontology.Model.PropertyModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, p, isDefBy);
                        }
                    }
                    #endregion

                    #region CustomAnnotations
                    foreach (var annotProp in annotProps)
                    {
                        foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)p.Value)
                                                 .SelectTriplesByPredicate((RDFResource)annotProp.Value))
                        {
                            if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            {
                                ontology.Model.PropertyModel.AddCustomAnnotation(annotProp, p, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                            }
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
                                        {
                                            custAnnValue = new RDFOntologyResource
                                            {
                                                Value = t.Object,
                                                PatternMemberID = t.Object.PatternMemberID
                                            };
                                        }
                                    }
                                }
                                ontology.Model.PropertyModel.AddCustomAnnotation(annotProp, p, custAnnValue);
                            }
                        }

                    }
                    #endregion

                }
                #endregion

                #region Facts
                foreach (var f in ontology.Data)
                {

                    #region VersionInfo
                    foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)f.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: versioninfo annotation on fact cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("VersionInfo annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value));
                        }
                    }
                    #endregion

                    #region Comment
                    foreach (var t in comment.SelectTriplesBySubject((RDFResource)f.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: comment annotation on fact cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Comment annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value));
                        }
                    }
                    #endregion

                    #region Label
                    foreach (var t in label.SelectTriplesBySubject((RDFResource)f.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            //Raise warning event to inform the user: label annotation on fact cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Label annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value));
                        }
                    }
                    #endregion

                    #region SeeAlso
                    foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)f.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
                        else
                        {
                            RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (resource == null)
                            {
                                resource = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (resource == null)
                                {
                                    resource = ontology.Data.SelectFact(t.Object.ToString());
                                    if (resource == null)
                                    {
                                        resource = new RDFOntologyResource();
                                        resource.Value = t.Object;
                                        resource.PatternMemberID = t.Object.PatternMemberID;
                                    }
                                }
                            }
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, f, resource);
                        }
                    }
                    #endregion

                    #region IsDefinedBy
                    foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)f.Value))
                    {
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        {
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                        }
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
                                    {
                                        isDefBy = new RDFOntologyResource();
                                        isDefBy.Value = t.Object;
                                        isDefBy.PatternMemberID = t.Object.PatternMemberID;
                                    }
                                }
                            }
                            ontology.Data.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, f, isDefBy);
                        }
                    }
                    #endregion

                    #region CustomAnnotations
                    foreach (var annotProp in annotProps)
                    {
                        foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)f.Value)
                                                  .SelectTriplesByPredicate((RDFResource)annotProp.Value))
                        {
                            if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            {
                                ontology.Data.AddCustomAnnotation(annotProp, f, ((RDFLiteral)t.Object).ToRDFOntologyLiteral());
                            }
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
                                        {
                                            custAnnValue = new RDFOntologyResource
                                            {
                                                Value = t.Object,
                                                PatternMemberID = t.Object.PatternMemberID
                                            };
                                        }
                                    }
                                }
                                ontology.Data.AddCustomAnnotation(annotProp, f, custAnnValue);
                            }
                        }

                    }
                    #endregion

                }
                #endregion

                #endregion

                #region Step 6.9: Finalize Ontology
                ontology = ontology.DifferenceWith(RDFBASEOntology.Instance);
                ontology.Value = new RDFResource(ontGraph.Context.ToString());
                #endregion

                #endregion

                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Graph '{0}' has been parsed as Ontology.", ontGraph.Context));
            }
            return ontology;
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

        #region Extensions

        #region Model Extensions
        /// <summary>
        /// Gets an ontology class of the given nature from the given RDF resource
        /// </summary>
        public static RDFOntologyClass ToRDFOntologyClass(this RDFResource ontResource,
                                                            RDFSemanticsEnums.RDFOntologyClassNature nature = RDFSemanticsEnums.RDFOntologyClassNature.OWL)
        {
            return new RDFOntologyClass(ontResource, nature);
        }

        /// <summary>
        /// Gets an ontology property from the given RDF resource
        /// </summary>
        internal static RDFOntologyProperty ToRDFOntologyProperty(this RDFResource ontResource)
        {
            return new RDFOntologyProperty(ontResource);
        }

        /// <summary>
        /// Gets an ontology object property from the given RDF resource
        /// </summary>
        public static RDFOntologyObjectProperty ToRDFOntologyObjectProperty(this RDFResource ontResource)
        {
            return new RDFOntologyObjectProperty(ontResource);
        }

        /// <summary>
        /// Gets an ontology datatype property from the given RDF resource
        /// </summary>
        public static RDFOntologyDatatypeProperty ToRDFOntologyDatatypeProperty(this RDFResource ontResource)
        {
            return new RDFOntologyDatatypeProperty(ontResource);
        }

        /// <summary>
        /// Gets an ontology annotation property from the given RDF resource
        /// </summary>
        public static RDFOntologyAnnotationProperty ToRDFOntologyAnnotationProperty(this RDFResource ontResource)
        {
            return new RDFOntologyAnnotationProperty(ontResource);
        }

        /// <summary>
        /// Gets an ontology fact from the given RDF resource
        /// </summary>
        public static RDFOntologyFact ToRDFOntologyFact(this RDFResource ontResource)
        {
            return new RDFOntologyFact(ontResource);
        }

        /// <summary>
        /// Gets an ontology literal from the given RDF literal
        /// </summary>
        public static RDFOntologyLiteral ToRDFOntologyLiteral(this RDFLiteral ontLiteral)
        {
            return new RDFOntologyLiteral(ontLiteral);
        }
        #endregion

        #region Query Extensions
        /// <summary>
        /// Applies the given SPARQL SELECT query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFSelectQueryResult ApplyToOntology(this RDFSelectQuery selectQuery,
                                                           RDFOntology ontology,
                                                           RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (selectQuery != null && ontology != null)
                return selectQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFSelectQueryResult();
        }

        /// <summary>
        /// Applies the given SPARQL ASK query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFAskQueryResult ApplyToOntology(this RDFAskQuery askQuery,
                                                        RDFOntology ontology,
                                                        RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (askQuery != null && ontology != null)
                return askQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFAskQueryResult();
        }

        /// <summary>
        /// Applies the given SPARQL CONSTRUCT query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFConstructQueryResult ApplyToOntology(this RDFConstructQuery constructQuery,
                                                              RDFOntology ontology,
                                                              RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (constructQuery != null && ontology != null)
                return constructQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFConstructQueryResult(RDFNamespaceRegister.DefaultNamespace.ToString());
        }

        /// <summary>
        /// Applies the given SPARQL DESCRIBE query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFDescribeQueryResult ApplyToOntology(this RDFDescribeQuery describeQuery,
                                                             RDFOntology ontology,
                                                             RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (describeQuery != null && ontology != null)
                return describeQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFDescribeQueryResult(RDFNamespaceRegister.DefaultNamespace.ToString());
        }
        #endregion

        #region SemanticsExtensions
        /// <summary>
        /// Gets a graph representation of the given taxonomy, exporting inferences according to the selected behavior [OWL2]
        /// </summary>
        internal static RDFGraph ReifyToRDFGraph(this RDFOntologyTaxonomy taxonomy, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior, string taxonomyName)
        {
            RDFGraph result = new RDFGraph();
            switch (taxonomyName)
            {
                //Semantic-based reification
                case nameof(RDFOntologyDataMetadata.NegativeAssertions):
                    result = ReifyNegativeAssertionsTaxonomyToGraph(taxonomy, infexpBehavior);
                    break;

                //List-based reification
                case nameof(RDFOntologyClassModelMetadata.HasKey):
                case nameof(RDFOntologyPropertyModelMetadata.PropertyChainAxiom):
                    result = ReifyListTaxonomyToGraph(taxonomy, taxonomyName, infexpBehavior);
                    break;

                //Triple-based reification
                default:
                    result = ReifyTaxonomyToGraph(taxonomy, infexpBehavior);
                    break;
            }
            return result;
        }
        private static RDFGraph ReifyNegativeAssertionsTaxonomyToGraph(RDFOntologyTaxonomy taxonomy, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();
            foreach (RDFOntologyTaxonomyEntry te in taxonomy)
            {

                //Build reification triples of taxonomy entry
                RDFTriple teTriple = te.ToRDFTriple();

                //Do not export semantic inferences
                if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None)
                {
                    if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                    {
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                        if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                        else
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                    }
                }

                //Export semantic inferences related only to ontology model
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyModel)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model ||
                            taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                        if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                        else
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                            if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                            else
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                        }
                    }
                }

                //Export semantic inferences related only to ontology data
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data ||
                            taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                        if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                        else
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                            if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                            else
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                        }
                    }
                }

                //Export semantic inferences related both to ontology model and data
                else
                {
                    result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                    result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                    result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                    if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                    else
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                }

            }
            return result;
        }
        private static RDFGraph ReifyListTaxonomyToGraph(RDFOntologyTaxonomy taxonomy, string taxonomyName, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();

            RDFResource taxonomyPredicate = taxonomyName.Equals(nameof(RDFOntologyClassModelMetadata.HasKey))
                                                ? RDFVocabulary.OWL.HAS_KEY : RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM;
            foreach (IGrouping<RDFOntologyResource, RDFOntologyTaxonomyEntry> tgroup in taxonomy.GroupBy(t => t.TaxonomySubject))
            {
                //Build collection corresponding to the current subject of the given taxonomy
                RDFCollection tgroupColl = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource, taxonomy.AcceptDuplicates);
                foreach (RDFOntologyTaxonomyEntry tgroupEntry in tgroup.ToList())
                    tgroupColl.AddItem((RDFResource)tgroupEntry.TaxonomyObject.Value);
                result.AddCollection(tgroupColl);

                //Attach collection with taxonomy-specific predicate
                result.AddTriple(new RDFTriple((RDFResource)tgroup.Key.Value, taxonomyPredicate, tgroupColl.ReificationSubject));
            }

            return result;
        }
        private static RDFGraph ReifyTaxonomyToGraph(RDFOntologyTaxonomy taxonomy, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();
            foreach (RDFOntologyTaxonomyEntry te in taxonomy)
            {

                //Do not export semantic inferences
                if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None)
                {
                    if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                    {
                        result.AddTriple(te.ToRDFTriple());
                    }
                }

                //Export semantic inferences related only to ontology model
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyModel)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model
                            || taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(te.ToRDFTriple());
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(te.ToRDFTriple());
                        }
                    }
                }

                //Export semantic inferences related only to ontology data
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data
                            || taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(te.ToRDFTriple());
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(te.ToRDFTriple());
                        }
                    }
                }

                //Export semantic inferences related both to ontology model and data
                else
                {
                    result.AddTriple(te.ToRDFTriple());
                }

            }
            return result;
        }
        #endregion

        #endregion

    }

}
