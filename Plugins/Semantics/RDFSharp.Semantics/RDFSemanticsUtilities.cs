/*
   Copyright 2012-2016 Marco De Salvo

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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFSemanticsUtilities is a collector of reusable utility methods for RDF ontology management.
    /// </summary>
    internal static class RDFSemanticsUtilities {

        #region Initialize
        /// <summary>
        /// Initializes the given ontology
        /// </summary>
        internal static void InitializeOntology(RDFOntology ontology) {

            //Classes
            ontology.Model.ClassModel.AddClass(RDFOntologyVocabulary.Classes.THING);
            ontology.Model.ClassModel.AddClass(RDFOntologyVocabulary.Classes.NOTHING);
            
            //Datatypes
            foreach (var dType  in RDFDatatypeRegister.Instance.Value) {
                var  dTypeCls    = new RDFOntologyClass(new RDFResource(dType.ToString()));
                ontology.Model.ClassModel.AddClass(dTypeCls);
            }

            //Taxonomies - Primitive Datatypes
            var rdfsLiteralCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.RDFS.LITERAL.ToString());
            var rdfXmlLitCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.RDF.XML_LITERAL.ToString());
            var rdfHtmlCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.RDF.HTML.ToString());
            var xsdStringCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString());
            var xsdBooleanCls    = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString());
            var xsdBase64Cls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BASE64_BINARY.ToString());
            var xsdHexBinCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.HEX_BINARY.ToString());
            var xsdFloatCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.FLOAT.ToString());
            var xsdDecimalCls    = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.DECIMAL.ToString());
            var xsdDoubleCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.DOUBLE.ToString());
            var xsdAnyUriCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.ANY_URI.ToString());
            var xsdQNameCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.QNAME.ToString());
            var xsdNotationCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NOTATION.ToString());
            var xsdDurationCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.DURATION.ToString());
            var xsdDateTimeCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.DATETIME.ToString());
            var xsdTimeCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.TIME.ToString());
            var xsdDateCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.DATE.ToString());            
            var xsdGYearMonthCls = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.G_YEAR_MONTH.ToString());
            var xsdGYearCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.G_YEAR.ToString());
            var xsdGMonthDayCls  = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.G_MONTH_DAY.ToString());
            var xsdGDayCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.G_DAY.ToString());
            var xsdGMonthCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.G_MONTH.ToString());
            ontology.Model.ClassModel.AddSubClassOfRelation(rdfXmlLitCls,     rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(rdfHtmlCls,       rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdStringCls,     rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdBooleanCls,    rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdBase64Cls,     rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdHexBinCls,     rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdFloatCls,      rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdDecimalCls,    rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdDoubleCls,     rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdAnyUriCls,     rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdQNameCls,      rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNotationCls,   rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdDurationCls,   rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdDateTimeCls,   rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdTimeCls,       rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdDateCls,       rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdGYearMonthCls, rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdGYearCls,      rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdGMonthDayCls,  rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdGDayCls,       rdfsLiteralCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdGMonthCls,     rdfsLiteralCls);

            //Taxonomies - Derived Datatypes
            var xsdNormStringCls = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NORMALIZED_STRING.ToString());
            var xsdTokenCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.TOKEN.ToString());
            var xsdLanguageCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.LANGUAGE.ToString());
            var xsdNameCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NAME.ToString());
            var xsdNMTokenCls    = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NMTOKEN.ToString());
            var xsdNCNameCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NCNAME.ToString());
            var xsdIntegerCls    = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString());
            var xsdNPIntegerCls  = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString());
            var xsdNIntegerCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString());
            var xsdNNIntegerCls  = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString());
            var xsdPIntegerCls   = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.POSITIVE_INTEGER.ToString());
            var xsdLongCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.LONG.ToString());
            var xsdIntCls        = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INT.ToString());
            var xsdShortCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.SHORT.ToString());
            var xsdByteCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BYTE.ToString());
            var xsdULongCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.UNSIGNED_LONG.ToString());
            var xsdUIntCls       = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.UNSIGNED_INT.ToString());
            var xsdUShortCls     = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.UNSIGNED_SHORT.ToString());
            var xsdUByteCls      = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.UNSIGNED_BYTE.ToString());
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNormStringCls, xsdStringCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdTokenCls,      xsdNormStringCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdLanguageCls,   xsdTokenCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNameCls,       xsdTokenCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNMTokenCls,    xsdTokenCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNCNameCls,     xsdNameCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdIntegerCls,    xsdDecimalCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNPIntegerCls,  xsdIntegerCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNNIntegerCls,  xsdIntegerCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdLongCls,       xsdIntegerCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdNIntegerCls,   xsdNPIntegerCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdIntCls,        xsdLongCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdShortCls,      xsdIntCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdByteCls,       xsdShortCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdPIntegerCls,   xsdNNIntegerCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdULongCls,      xsdNNIntegerCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdUIntCls,       xsdULongCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdUShortCls,     xsdUIntCls);
            ontology.Model.ClassModel.AddSubClassOfRelation(xsdUByteCls,      xsdUShortCls);

            //Taxonomy - Custom Datatypes
            foreach (var customDType  in RDFDatatypeRegister.Instance.Value.Where(dt =>
                                             !dt.Prefix.Equals(RDFVocabulary.XSD.PREFIX,  StringComparison.Ordinal)  &&
                                             !dt.Prefix.Equals(RDFVocabulary.RDF.PREFIX,  StringComparison.Ordinal)  &&
                                             !dt.Prefix.Equals(RDFVocabulary.RDFS.PREFIX, StringComparison.Ordinal))) {

                var customDTypeCls     = ontology.Model.ClassModel.SelectClass(customDType.ToString());
                if (customDTypeCls    != null) {
                    switch (customDType.Category) {
                        case RDFModelEnums.RDFDatatypeCategory.Boolean:
                            ontology.Model.ClassModel.AddSubClassOfRelation(customDTypeCls, xsdBooleanCls);
                            break;
                        case RDFModelEnums.RDFDatatypeCategory.DateTime:
                            ontology.Model.ClassModel.AddSubClassOfRelation(customDTypeCls, xsdDateTimeCls);
                            break;
                        case RDFModelEnums.RDFDatatypeCategory.Numeric:
                            ontology.Model.ClassModel.AddSubClassOfRelation(customDTypeCls, xsdDecimalCls);
                            break;
                        case RDFModelEnums.RDFDatatypeCategory.String:
                            ontology.Model.ClassModel.AddSubClassOfRelation(customDTypeCls, xsdStringCls);
                            break;
                        case RDFModelEnums.RDFDatatypeCategory.TimeSpan:
                            ontology.Model.ClassModel.AddSubClassOfRelation(customDTypeCls, xsdDurationCls);
                            break;
                    }
                }
            }

        }
        #endregion

        #region Enlist

        #region Model

        #region ClassModel

        #region SubClassOf
        /// <summary>
        /// Subsumes the "rdfs:subClassOf" taxonomy to discover direct and indirect subClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel EnlistSubClassesOf(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result  = new RDFOntologyClassModel();

            // Transitivity of "rdfs:subClassOf" taxonomy: ((A SUBCLASSOF B)  &&  (B SUBCLASSOF C))  =>  (A SUBCLASSOF C)
            foreach(var sc in classModel.Relations.SubClassOf.SelectEntriesByObject(ontClass)) {
                result.AddClass((RDFOntologyClass)sc.TaxonomySubject);
                result  = result.UnionWith(RDFSemanticsUtilities.EnlistSubClassesOf((RDFOntologyClass)sc.TaxonomySubject, classModel));
            }

            return result;
        }
        internal static RDFOntologyClassModel EnlistSubClassesOf_Core(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result1 = new RDFOntologyClassModel();
            var result2 = new RDFOntologyClassModel();            

            // Step 1: Direct subsumption of "rdfs:subClassOf" taxonomy
            result1     = RDFSemanticsUtilities.EnlistSubClassesOf(ontClass, classModel);

            // Step 2: Enlist equivalent classes of subclasses
            result2     = result2.UnionWith(result1);
            foreach(var sc in result1) {
                result2 = result2.UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(sc, classModel)
                                                .UnionWith(RDFOntologyReasoningHelper.EnlistSubClassesOf(sc, classModel)));
            }
            
            return result2;
        }
        #endregion

        #region SuperClassOf
        /// <summary>
        /// Subsumes the "rdfs:subClassOf" taxonomy to discover direct and indirect superClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel EnlistSuperClassesOf(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result  = new RDFOntologyClassModel();

            // Transitivity of "rdfs:subClassOf" taxonomy: ((A SUPERCLASSOF B)  &&  (B SUPERCLASSOF C))  =>  (A SUPERCLASSOF C)
            foreach(var sc in classModel.Relations.SubClassOf.SelectEntriesBySubject(ontClass)) {
                result.AddClass((RDFOntologyClass)sc.TaxonomyObject);
                result  = result.UnionWith(RDFSemanticsUtilities.EnlistSuperClassesOf((RDFOntologyClass)sc.TaxonomyObject, classModel));
            }

            return result;
        }
        internal static RDFOntologyClassModel EnlistSuperClassesOf_Core(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result1 = new RDFOntologyClassModel();
            var result2 = new RDFOntologyClassModel();

            // Step 1: Direct subsumption of "rdfs:subClassOf" taxonomy
            result1     = RDFSemanticsUtilities.EnlistSuperClassesOf(ontClass, classModel);

            // Step 2: Enlist equivalent classes of superclasses
            result2     = result2.UnionWith(result1);
            foreach(var sc in result1) {
                result2 = result2.UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(sc, classModel)
                                                .UnionWith(RDFOntologyReasoningHelper.EnlistSuperClassesOf(sc, classModel)));
            }

            return result2;
        }
        #endregion

        #region EquivalentClass
        /// <summary>
        /// Subsumes the "owl:equivalentClass" taxonomy to discover direct and indirect equivalentClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel EnlistEquivalentClassesOf_Core(RDFOntologyClass ontClass, 
                                                                             RDFOntologyClassModel classModel, 
                                                                             Dictionary<Int64, RDFOntologyClass> visitContext) {
            var result        = new RDFOntologyClassModel();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyClass>() { { ontClass.PatternMemberID, ontClass } };
            }
            else {
                if (!visitContext.ContainsKey(ontClass.PatternMemberID)) {
                     visitContext.Add(ontClass.PatternMemberID, ontClass);
                }
                else {
                    return result;
                }
            }
            #endregion

            // Transitivity of "owl:equivalentClass" taxonomy: ((A EQUIVALENTCLASSOF B)  &&  (B EQUIVALENTCLASS C))  =>  (A EQUIVALENTCLASS C)
            foreach (var      ec in classModel.Relations.EquivalentClass.SelectEntriesBySubject(ontClass)) {
                result.AddClass((RDFOntologyClass)ec.TaxonomyObject);
                result        = result.UnionWith(RDFSemanticsUtilities.EnlistEquivalentClassesOf_Core((RDFOntologyClass)ec.TaxonomyObject, classModel, visitContext));
            }

            return result;
        }
        #endregion

        #region DisjointWith
        internal static RDFOntologyClassModel EnlistDisjointClassesWith_Core(RDFOntologyClass ontClass, RDFOntologyClassModel classModel, Dictionary<Int64, RDFOntologyClass> visitContext) {
            var result1       = new RDFOntologyClassModel();
            var result2       = new RDFOntologyClassModel();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyClass>() { { ontClass.PatternMemberID, ontClass } };
            }
            else {
                if (!visitContext.ContainsKey(ontClass.PatternMemberID)) {
                     visitContext.Add(ontClass.PatternMemberID, ontClass);
                }
                else {
                    return result1;
                }
            }
            #endregion

            // Inference: ((A DISJOINTWITH B)   &&  (B EQUIVALENTCLASS C))  =>  (A DISJOINTWITH C)
            foreach (var      dw in classModel.Relations.DisjointWith.SelectEntriesBySubject(ontClass)) {
                result1.AddClass((RDFOntologyClass)dw.TaxonomyObject);
                result1       = result1.UnionWith(RDFSemanticsUtilities.EnlistEquivalentClassesOf_Core((RDFOntologyClass)dw.TaxonomyObject, classModel, visitContext));
            }

            // Inference: ((A DISJOINTWITH B)   &&  (B SUPERCLASS C))  =>  (A DISJOINTWITH C)
            result2           = result2.UnionWith(result1);
            foreach (var      c in result1) {
                result2       = result2.UnionWith(RDFSemanticsUtilities.EnlistSubClassesOf_Core(c, classModel));
            }
            result1           = result1.UnionWith(result2);

            // Inference: ((A EQUIVALENTCLASS B || A SUBCLASSOF B)  &&  (B DISJOINTWITH C))     =>  (A DISJOINTWITH C)
            var compatibleCls = RDFOntologyReasoningHelper.EnlistSuperClassesOf(ontClass, classModel)
                                        .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(ontClass, classModel));
            foreach (var      ec in compatibleCls) {
                result1       = result1.UnionWith(RDFSemanticsUtilities.EnlistDisjointClassesWith_Core(ec, classModel, visitContext));
            }

            return result1;
        }
        #endregion

        #endregion

        #region PropertyModel

        #region SubPropertyOf
        /// <summary>
        /// Subsumes the "rdfs:subPropertyOf" taxonomy to discover direct and indirect subProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel EnlistSubPropertiesOf(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result  = new RDFOntologyPropertyModel();

            // Transitivity of "rdfs:subPropertyOf" taxonomy: ((A SUBPROPERTYOF B)  &&  (B SUBPROPERTYOF C))  =>  (A SUBPROPERTYOF C)
            foreach(var sp in propertyModel.Relations.SubPropertyOf.SelectEntriesByObject(ontProperty)) {
                result.AddProperty((RDFOntologyProperty)sp.TaxonomySubject);
                result  = result.UnionWith(RDFSemanticsUtilities.EnlistSubPropertiesOf((RDFOntologyProperty)sp.TaxonomySubject, propertyModel));
            }

            return result;
        }
        internal static RDFOntologyPropertyModel EnlistSubPropertiesOf_Core(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result1 = new RDFOntologyPropertyModel();
            var result2 = new RDFOntologyPropertyModel();

            // Step 1: Direct subsumption of "rdfs:subPropertyOf" taxonomy
            result1     = RDFSemanticsUtilities.EnlistSubPropertiesOf(ontProperty, propertyModel);

            // Step 2: Enlist equivalent properties of subproperties
            result2     = result2.UnionWith(result1);
            foreach(var sp in result1) {
                result2 = result2.UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(sp, propertyModel)
                                                .UnionWith(RDFOntologyReasoningHelper.EnlistSubPropertiesOf(sp, propertyModel)));
            }

            return result2;
        }
        #endregion

        #region SuperPropertyOf
        /// <summary>
        /// Subsumes the "rdfs:subPropertyOf" taxonomy to discover direct and indirect superProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel EnlistSuperPropertiesOf(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result  = new RDFOntologyPropertyModel();

            // Transitivity of "rdfs:subPropertyOf" taxonomy: ((A SUPERPROPERTYOF B)  &&  (B SUPERPROPERTYOF C))  =>  (A SUPERPROPERTYOF C)
            foreach(var sp in propertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(ontProperty)) {
                result.AddProperty((RDFOntologyProperty)sp.TaxonomyObject);
                result  = result.UnionWith(RDFSemanticsUtilities.EnlistSuperPropertiesOf((RDFOntologyProperty)sp.TaxonomyObject, propertyModel));
            }

            return result;
        }
        internal static RDFOntologyPropertyModel EnlistSuperPropertiesOf_Core(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result1 = new RDFOntologyPropertyModel();
            var result2 = new RDFOntologyPropertyModel();

            // Step 1: Direct subsumption of "rdfs:subPropertyOf" taxonomy
            result1     = RDFSemanticsUtilities.EnlistSuperPropertiesOf(ontProperty, propertyModel);

            // Step 2: Enlist equivalent properties of subproperties
            result2     = result2.UnionWith(result1);
            foreach(var sp in result1) {
                result2 = result2.UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(sp, propertyModel)
                                                .UnionWith(RDFOntologyReasoningHelper.EnlistSuperPropertiesOf(sp, propertyModel)));
            }

            return result2;
        }
        #endregion

        #region EquivalentProperty
        /// <summary>
        /// Subsumes the "owl:equivalentProperty" taxonomy to discover direct and indirect equivalentProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel EnlistEquivalentPropertiesOf_Core(RDFOntologyProperty ontProperty,
                                                                                   RDFOntologyPropertyModel propertyModel,
                                                                                   Dictionary<Int64, RDFOntologyProperty> visitContext) {
            var result        = new RDFOntologyPropertyModel();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyProperty>() { { ontProperty.PatternMemberID, ontProperty } };
            }
            else {
                if (!visitContext.ContainsKey(ontProperty.PatternMemberID)) {
                     visitContext.Add(ontProperty.PatternMemberID, ontProperty);
                }
                else {
                    return result;
                }
            }
            #endregion

            // Transitivity of "owl:equivalentProperty" taxonomy: ((A EQUIVALENTPROPERTY B)  &&  (B EQUIVALENTPROPERTY C))  =>  (A EQUIVALENTPROPERTY C)
            foreach (var      ep in propertyModel.Relations.EquivalentProperty.SelectEntriesBySubject(ontProperty)) {
                result.AddProperty((RDFOntologyProperty)ep.TaxonomyObject);
                result        = result.UnionWith(RDFSemanticsUtilities.EnlistEquivalentPropertiesOf_Core((RDFOntologyProperty)ep.TaxonomyObject, propertyModel, visitContext));
            }

            return result;
        }
        #endregion

        #endregion

        #endregion

        #region Data

        #region SameAs
        /// <summary>
        /// Subsumes the "owl:sameAs" taxonomy to discover direct and indirect samefacts of the given facts
        /// </summary>
        internal static RDFOntologyData EnlistSameFactsAs_Core(RDFOntologyFact ontFact,
                                                               RDFOntologyData data,
                                                               Dictionary<Int64, RDFOntologyFact> visitContext) {
            var result        = new RDFOntologyData();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID)) {
                     visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else {
                    return result;
                }
            }
            #endregion

            // Transitivity of "owl:sameAs" taxonomy: ((A SAMEAS B)  &&  (B SAMEAS C))  =>  (A SAMEAS C)
            foreach (var      sf in data.Relations.SameAs.SelectEntriesBySubject(ontFact)) {
                result.AddFact((RDFOntologyFact)sf.TaxonomyObject);
                result        = result.UnionWith(RDFSemanticsUtilities.EnlistSameFactsAs_Core((RDFOntologyFact)sf.TaxonomyObject, data, visitContext));
            }

            return result;
        }
        #endregion

        #region DifferentFrom
        /// <summary>
        /// Subsumes the "owl:differentFrom" taxonomy to discover direct and indirect differentFacts of the given facts
        /// </summary>
        internal static RDFOntologyData EnlistDifferentFactsFrom_Core(RDFOntologyFact ontFact,
                                                                      RDFOntologyData data,
                                                                      Dictionary<Int64, RDFOntologyFact> visitContext) {
            var result        = new RDFOntologyData();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID)) {
                     visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else {
                    return result;
                }
            }
            #endregion

            // Inference: (A DIFFERENTFROM B  &&  B SAMEAS C         =>  A DIFFERENTFROM C)
            foreach (var      df in data.Relations.DifferentFrom.SelectEntriesBySubject(ontFact)) {
                result.AddFact((RDFOntologyFact)df.TaxonomyObject);
                result        = result.UnionWith(RDFSemanticsUtilities.EnlistSameFactsAs_Core((RDFOntologyFact)df.TaxonomyObject, data, visitContext));
            }

            // Inference: (A SAMEAS B         &&  B DIFFERENTFROM C  =>  A DIFFERENTFROM C)
            foreach (var     sa in RDFOntologyReasoningHelper.EnlistSameFactsAs(ontFact, data)) {
                result        = result.UnionWith(RDFSemanticsUtilities.EnlistDifferentFactsFrom_Core(sa, data, visitContext));
            }

            return result;
        }
        #endregion

        #region TransitiveProperty
        /// <summary>
        /// Enlists the transitive assertions of the given fact and the given property within the given data
        /// </summary>
        internal static RDFOntologyData EnlistTransitiveAssertionsOf_Core(RDFOntologyFact ontFact,
                                                                          RDFOntologyObjectProperty ontProp,
                                                                          RDFOntologyData data,
                                                                          Dictionary<Int64, RDFOntologyFact> visitContext) {
            var result        = new RDFOntologyData();

            #region visitContext
            if(visitContext  == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else {
                if(!visitContext.ContainsKey(ontFact.PatternMemberID)) {
                    visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else {
                    return result;
                }
            }
            #endregion

            // ((F1 P F2)    &&  (F2 P F3))  =>  (F1 P F3)
            foreach(var ta   in data.Relations.Assertions.SelectEntriesBySubject(ontFact)
                                                         .SelectEntriesByPredicate(ontProp)) {
                result.AddFact((RDFOntologyFact)ta.TaxonomyObject);
                result        = result.UnionWith(RDFSemanticsUtilities.EnlistTransitiveAssertionsOf_Core((RDFOntologyFact)ta.TaxonomyObject, ontProp, data, visitContext));
            }

            return result;
        }
        #endregion

        #region MembersOf
        /// <summary>
        /// Enlists the facts which are members of the given restriction within the given ontology
        /// </summary>
        internal static RDFOntologyData EnlistMembersOfRestriction(RDFOntologyRestriction ontRestriction, RDFOntology ontology) {
            var result     = new RDFOntologyData();

            //Enlist the properties which are compatible with the restriction's "OnProperty"
            var compProps  = RDFOntologyReasoningHelper.EnlistSubPropertiesOf(ontRestriction.OnProperty, ontology.Model.PropertyModel)
                                 .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(ontRestriction.OnProperty, ontology.Model.PropertyModel))
                                    .AddProperty(ontRestriction.OnProperty);
            
            //Filter assertions made with enlisted compatible properties
            var fTaxonomy  = new RDFOntologyTaxonomy();
            foreach (var   p in compProps) {
                fTaxonomy  = fTaxonomy.UnionWith(ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p));
            }

            #region Cardinality
            if (ontRestriction is RDFOntologyCardinalityRestriction) {

                //Item2 is a counter for occurrences of the restricted property within the subject fact
                var fCount = new Dictionary<Int64, Tuple<RDFOntologyFact, Int64>>();
                
                //Iterate the compatible assertions
                foreach (var tEntry in fTaxonomy) {
                    if  (!fCount.ContainsKey(tEntry.TaxonomySubject.PatternMemberID)) {
                          fCount.Add(tEntry.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, 1));
                    }
                    else {
                        var occurrencyCounter                          = fCount[tEntry.TaxonomySubject.PatternMemberID].Item2;
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, occurrencyCounter + 1);
                    }
                }

                //Apply the cardinality restriction on the tracked facts
                var fCountEnum   = fCount.Values.GetEnumerator();
                while (fCountEnum.MoveNext()) {
                    var passesMinCardinality     = true;
                    var passesMaxCardinality     = true;

                    //MinCardinality: signal tracked facts having "#occurrences < MinCardinality"
                    if (((RDFOntologyCardinalityRestriction)ontRestriction).MinCardinality > 0) {
                        if (fCountEnum.Current.Item2 < ((RDFOntologyCardinalityRestriction)ontRestriction).MinCardinality) {
                            passesMinCardinality = false;
                        }
                    }

                    //MaxCardinality: signal tracked facts having "#occurrences > MaxCardinality"
                    if (((RDFOntologyCardinalityRestriction)ontRestriction).MaxCardinality > 0) {
                        if (fCountEnum.Current.Item2 > ((RDFOntologyCardinalityRestriction)ontRestriction).MaxCardinality) {
                            passesMaxCardinality = false;
                        }
                    }

                    //Save the candidate fact if it passes cardinality restrictions
                    if (passesMinCardinality    && passesMaxCardinality) {
                        result.AddFact(fCountEnum.Current.Item1);
                    }
                }

            }
            #endregion

            #region AllValuesFrom
            else if (ontRestriction is RDFOntologyAllValuesFromRestriction) {

                //Item2 is a counter for occurrences of the restricted property with a range member of the restricted "FromClass"
                //Item3 is a counter for occurrences of the restricted property with a range member not of the restricted "FromClass"
                var fCount      = new Dictionary<Int64, Tuple<RDFOntologyFact, Int64, Int64>>();

                //Enlist the classes which are compatible with the restricted "FromClass"
                var compClasses = RDFOntologyReasoningHelper.EnlistSubClassesOf(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass, ontology.Model.ClassModel)
                                      .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass, ontology.Model.ClassModel))
                                         .AddClass(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass);

                //Iterate the compatible assertions
                foreach (var tEntry in fTaxonomy) {

                    //Initialize the occurrence counters of the subject fact
                    if (!fCount.ContainsKey(tEntry.TaxonomySubject.PatternMemberID)) {
                         fCount.Add(tEntry.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, 0, 0));
                    }

                    //Iterate the class types of the object fact, checking presence of the restricted "FromClass"
                    var fromClassFound            = false;
                    var objFactClassTypes         = ontology.Data.Relations.ClassType.SelectEntriesBySubject(tEntry.TaxonomyObject);
                    foreach (var objFactClassType in objFactClassTypes) {
                        var compObjFactClassTypes = RDFOntologyReasoningHelper.EnlistSubClassesOf((RDFOntologyClass)objFactClassType.TaxonomyObject, ontology.Model.ClassModel)
                                                        .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf((RDFOntologyClass)objFactClassType.TaxonomyObject, ontology.Model.ClassModel))
                                                            .AddClass((RDFOntologyClass)objFactClassType.TaxonomyObject);
                        if  (compObjFactClassTypes.IntersectWith(compClasses).ClassesCount > 0) {
                             fromClassFound       = true;
                             break;
                        }
                    }

                    //Update the occurrence counters of the subject fact
                    var equalityCounter           = fCount[tEntry.TaxonomySubject.PatternMemberID].Item2;
                    var differenceCounter         = fCount[tEntry.TaxonomySubject.PatternMemberID].Item3;
                    if (fromClassFound) {
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, equalityCounter + 1, differenceCounter);
                    }
                    else {
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, equalityCounter, differenceCounter + 1);
                    }

                }

                //Apply the restriction on the subject facts
                var fCountEnum    = fCount.Values.GetEnumerator();
                while  (fCountEnum.MoveNext()) {
                    if (fCountEnum.Current.Item2 >= 1 && fCountEnum.Current.Item3 == 0) {
                        result.AddFact(fCountEnum.Current.Item1);
                    }
                }

            }
            #endregion

            #region SomeValuesFrom
            else if (ontRestriction is RDFOntologySomeValuesFromRestriction) {

                //Item2 is a counter for occurrences of the restricted property with a range member of the restricted "FromClass"
                //Item3 is a counter for occurrences of the restricted property with a range member not of the restricted "FromClass"
                var fCount      = new Dictionary<Int64, Tuple<RDFOntologyFact, Int64, Int64>>();

                //Enlist the classes which are compatible with the restricted "FromClass"
                var compClasses = RDFOntologyReasoningHelper.EnlistSubClassesOf(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass, ontology.Model.ClassModel)
                                      .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass, ontology.Model.ClassModel))
                                         .AddClass(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass);

                //Iterate the compatible assertions
                foreach (var tEntry in fTaxonomy) {

                    //Initialize the occurrence counters of the subject fact
                    if (!fCount.ContainsKey(tEntry.TaxonomySubject.PatternMemberID)) {
                         fCount.Add(tEntry.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, 0, 0));
                    }

                    //Iterate the class types of the object fact, checking presence of the restricted "FromClass"
                    var fromClassFound            = false;
                    var objFactClassTypes         = ontology.Data.Relations.ClassType.SelectEntriesBySubject(tEntry.TaxonomyObject);
                    foreach (var objFactClassType in objFactClassTypes) {
                        var compObjFactClassTypes = RDFOntologyReasoningHelper.EnlistSubClassesOf((RDFOntologyClass)objFactClassType.TaxonomyObject, ontology.Model.ClassModel)
                                                        .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf((RDFOntologyClass)objFactClassType.TaxonomyObject, ontology.Model.ClassModel))
                                                            .AddClass((RDFOntologyClass)objFactClassType.TaxonomyObject);
                        if  (compObjFactClassTypes.IntersectWith(compClasses).ClassesCount > 0) {
                             fromClassFound       = true;
                             break;
                        }
                    }

                    //Update the occurrence counters of the subject fact
                    var equalityCounter           = fCount[tEntry.TaxonomySubject.PatternMemberID].Item2;
                    var differenceCounter         = fCount[tEntry.TaxonomySubject.PatternMemberID].Item3;
                    if (fromClassFound) {
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, equalityCounter + 1, differenceCounter);
                    }
                    else {
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, equalityCounter, differenceCounter + 1);
                    }

                }

                //Apply the restriction on the subject facts
                var fCountEnum    = fCount.Values.GetEnumerator();
                while  (fCountEnum.MoveNext()) {
                    if (fCountEnum.Current.Item2 >= 1) {
                        result.AddFact(fCountEnum.Current.Item1);
                    }
                }

            }
            #endregion

            #region HasValue
            else if (ontRestriction is RDFOntologyHasValueRestriction) {                
                if (((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue.IsFact()) {

                    //Enlist the same facts of the restriction's "RequiredValue"
                    var compFacts = RDFOntologyReasoningHelper.EnlistSameFactsAs((RDFOntologyFact)((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue, ontology.Data)
                                        .AddFact((RDFOntologyFact)((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue);

                    //Iterate the compatible assertions
                    foreach (var  tEntry in fTaxonomy) {
                        if  (tEntry.TaxonomyObject.IsFact()) {
                             if (compFacts.SelectFact(tEntry.TaxonomyObject.ToString()) != null) {
                                 result.AddFact((RDFOntologyFact)tEntry.TaxonomySubject);
                             }
                        }
                    }

                }
                else if (((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue.IsLiteral()) {

                    //Iterate the compatible assertions and track the occurrence informations
                    foreach (var  tEntry in fTaxonomy) {
                        if  (tEntry.TaxonomyObject.IsLiteral()) {
                             try {
                                 var semanticLiteralsCompare  = RDFQueryUtilities.CompareRDFPatternMembers(((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue.Value, tEntry.TaxonomyObject.Value);
                                 if (semanticLiteralsCompare == 0) {
                                     result.AddFact((RDFOntologyFact)tEntry.TaxonomySubject);
                                 }
                             }
                             finally { }
                        }
                    }

                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given enumeration within the given ontology
        /// </summary>
        internal static RDFOntologyData EnlistMembersOfEnumerate(RDFOntologyEnumerateClass ontEnumClass, RDFOntology ontology) {
            var result     = new RDFOntologyData();

            //Filter "oneOf" relations made with the given enumerate class
            var enTaxonomy = ontology.Model.ClassModel.Relations.OneOf.SelectEntriesBySubject(ontEnumClass);
            foreach (var   tEntry in enTaxonomy) {
                
                //Add the fact and its synonyms
                if  (tEntry.TaxonomySubject.IsEnumerateClass() && tEntry.TaxonomyObject.IsFact()) {
                     result= result.UnionWith(RDFOntologyReasoningHelper.EnlistSameFactsAs((RDFOntologyFact)tEntry.TaxonomyObject, ontology.Data))
                                      .AddFact((RDFOntologyFact)tEntry.TaxonomyObject);
                }

            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given composition within the given ontology
        /// </summary>
        internal static RDFOntologyData EnlistMembersOfComposite(RDFOntologyClass ontCompClass, RDFOntology ontology) {
            var result            = new RDFOntologyData();

            //Intersection
            if (ontCompClass      is RDFOntologyIntersectionClass) {

                //Filter "intersectionOf" relations made with the given intersection class
                var firstIter     = true;
                var iTaxonomy     = ontology.Model.ClassModel.Relations.IntersectionOf.SelectEntriesBySubject(ontCompClass);
                foreach (var      tEntry in iTaxonomy) {
                    if  (firstIter) {
                        result    = RDFOntologyReasoningHelper.EnlistMembersOf((RDFOntologyClass)tEntry.TaxonomyObject, ontology);
                        firstIter = false;
                    }
                    else {
                        result    = result.IntersectWith(RDFOntologyReasoningHelper.EnlistMembersOf((RDFOntologyClass)tEntry.TaxonomyObject, ontology));
                    }
                }

            }

            //Union
            else if (ontCompClass is RDFOntologyUnionClass) {

                //Filter "unionOf" relations made with the given union class
                var uTaxonomy     = ontology.Model.ClassModel.Relations.UnionOf.SelectEntriesBySubject(ontCompClass);
                foreach (var      tEntry in uTaxonomy) {
                    result        = result.UnionWith(RDFOntologyReasoningHelper.EnlistMembersOf((RDFOntologyClass)tEntry.TaxonomyObject, ontology));
                }

            }

            //Complement
            else if (ontCompClass is RDFOntologyComplementClass) {
                result            = ontology.Data.DifferenceWith(RDFOntologyReasoningHelper.EnlistMembersOf(ontCompClass, ontology));
            }

            return result;
        }

        /// <summary>
        /// Enlists the literals which are members of the given literal-compatible class within the given ontology
        /// </summary>
        internal static RDFOntologyData EnlistMembersOfLiteralCompatibleClass(RDFOntologyClass ontClass, RDFOntology ontology) {
            var result         = new RDFOntologyData();

            //DataRange
            if(ontClass.IsDataRangeClass()) {

                //Filter "oneOf" relations made with the given datarange class
                var drTaxonomy = ontology.Model.ClassModel.Relations.OneOf.SelectEntriesBySubject(ontClass);
                foreach(var tEntry in drTaxonomy) {

                    //Add the literal
                    if(tEntry.TaxonomySubject.IsDataRangeClass() && tEntry.TaxonomyObject.IsLiteral()) {
                        result.AddLiteral((RDFOntologyLiteral)tEntry.TaxonomyObject);
                    }

                }

            }

            //Pure Literal
            else if(ontClass.Equals(RDFOntologyVocabulary.Classes.LITERAL) || RDFOntologyReasoningHelper.IsEquivalentClassOf(ontClass, RDFOntologyVocabulary.Classes.LITERAL, ontology.Model.ClassModel)) {
                foreach(var ontLit in ontology.Data.Literals.Values) {
                    result.AddLiteral(ontLit);
                }
            }

            //Derived Literal
            else {

                //String-Literals
                var xsdStringClass          = ontology.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString());
                if(ontClass.Equals(xsdStringClass) || RDFOntologyReasoningHelper.IsEquivalentClassOf(ontClass, xsdStringClass, ontology.Model.ClassModel)) {
                    foreach(var ontLit in ontology.Data.Literals.Values) {
                        if(ontLit.Value is RDFPlainLiteral) {
                            result.AddLiteral(ontLit);
                        }
                        else {
                            var dTypeClass  = ontology.Model.ClassModel.SelectClass(((RDFTypedLiteral)ontLit.Value).Datatype.ToString());
                            if(dTypeClass != null) {
                                if(dTypeClass.Equals(ontClass) || RDFOntologyReasoningHelper.IsSubClassOf(dTypeClass, ontClass, ontology.Model.ClassModel) 
                                                               || RDFOntologyReasoningHelper.IsEquivalentClassOf(dTypeClass, ontClass, ontology.Model.ClassModel)) {
                                    result.AddLiteral(ontLit);
                                }
                            }
                            else {
                                if(dTypeClass.Equals(ontClass)) {
                                    result.AddLiteral(ontLit);
                                }
                            }
                        }
                    }
                }

                //Other Literals
                else {
                    foreach(var ontLit in ontology.Data.Literals.Values.Where(l => l.Value is RDFTypedLiteral)) {
                        var  dTypeClass   = ontology.Model.ClassModel.SelectClass(((RDFTypedLiteral)ontLit.Value).Datatype.ToString());
                        if(dTypeClass != null) {
                            if(dTypeClass.Equals(ontClass) || RDFOntologyReasoningHelper.IsSubClassOf(dTypeClass, ontClass, ontology.Model.ClassModel) 
                                                           || RDFOntologyReasoningHelper.IsEquivalentClassOf(dTypeClass, ontClass, ontology.Model.ClassModel)) {
                                result.AddLiteral(ontLit);
                            }
                        }
                        else {
                            if(dTypeClass.Equals(ontClass)) {
                                result.AddLiteral(ontLit);
                            }
                        }
                    }
                }

            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given class within the given ontology
        /// </summary>
        internal static RDFOntologyData EnlistMembersOfClass(RDFOntologyClass ontClass, RDFOntology ontology) {
            var result     = new RDFOntologyData();

            //Get the compatible classes
            var compCls    = RDFOntologyReasoningHelper.EnlistSubClassesOf(ontClass, ontology.Model.ClassModel)
                                    .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(ontClass, ontology.Model.ClassModel))
                                    .AddClass(ontClass);

            //Filter "classType" relations made with compatible classes
            var fTaxonomy  = new RDFOntologyTaxonomy();
            foreach (var   c in compCls) {
                fTaxonomy  = fTaxonomy.UnionWith(ontology.Data.Relations.ClassType.SelectEntriesByObject(c));
            }
            foreach (var   tEntry in fTaxonomy) {

                //Add the fact and its synonyms
                if (tEntry.TaxonomySubject.IsFact()) {
                    result = result.UnionWith(RDFOntologyReasoningHelper.EnlistSameFactsAs((RDFOntologyFact)tEntry.TaxonomySubject, ontology.Data))
                                        .AddFact((RDFOntologyFact)tEntry.TaxonomySubject);
                }

            }
 
            return result;
        }
        #endregion

        #endregion

        #endregion

        #region Convert
        /// <summary>
        /// Gets an ontology representation of the given graph
        /// </summary>
        internal static RDFOntology FromRDFGraph(RDFGraph ontGraph) {
            RDFOntology ontology  = null;
            if (ontGraph         != null) {
                ontology          = new RDFOntology(new RDFResource(ontGraph.Context));

                #region Prefetch
                var versionInfo   = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.VERSION_INFO);
                var comment       = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.COMMENT);
                var label         = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.LABEL);
                var seeAlso       = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SEE_ALSO);
                var isDefinedBy   = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.IS_DEFINED_BY);
                var imports       = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.IMPORTS);
                var bcwcompWith   = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH);
                var incompWith    = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INCOMPATIBLE_WITH);
                var priorVersion  = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.PRIOR_VERSION);

                var rdfType       = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE);
                var subclassOf    = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_CLASS_OF);
                var subpropOf     = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_PROPERTY_OF);
                var equivclassOf  = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_CLASS);
                var equivpropOf   = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_PROPERTY);
                var disjclassWith = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DISJOINT_WITH);
                var domain        = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.DOMAIN);
                var range         = ontGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.RANGE);
                var onProperty    = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ON_PROPERTY);
                var oneOf         = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ONE_OF);
                var unionOf       = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.UNION_OF);
                var intersectionOf= ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INTERSECTION_OF);
                var complementOf  = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.COMPLEMENT_OF);
                var inverseOf     = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.INVERSE_OF);
                var allvaluesFrom = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.ALL_VALUES_FROM);
                var somevaluesFrom= ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SOME_VALUES_FROM);
                var hasvalue      = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.HAS_VALUE);
                var cardinality   = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.CARDINALITY);
                var mincardinality= ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MIN_CARDINALITY);
                var maxcardinality= ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.MAX_CARDINALITY);
                var sameAs        = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.SAME_AS);
                var differentFrom = ontGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.DIFFERENT_FROM);
                #endregion

                #region Load

                #region Ontology
                if (!rdfType.ContainsTriple(new RDFTriple((RDFResource)ontology.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ONTOLOGY))) {
                     var ont     = rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ONTOLOGY).FirstOrDefault();
                     if (ont    != null) {
                         ontology.Value           = ont.Subject;
                         ontology.PatternMemberID = ontology.Value.PatternMemberID;
                     }
                }
                #endregion

                #region OntologyModel

                #region PropertyModel

                #region AnnotationProperty
                foreach (var ap in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.ANNOTATION_PROPERTY)) {
                    ontology.Model.PropertyModel.AddProperty(new RDFOntologyAnnotationProperty((RDFResource)ap.Subject));
                }
                #endregion

                #region DatatypeProperty
                foreach (var dp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.DATATYPE_PROPERTY)) {
                    var dtp   = new RDFOntologyDatatypeProperty((RDFResource)dp.Subject);
                    ontology.Model.PropertyModel.AddProperty(dtp);

                    #region DeprecatedProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)dtp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY))) {
                        dtp.SetDeprecated(true);
                    }
                    #endregion

                    #region FunctionalProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)dtp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY))) {
                        dtp.SetFunctional(true);
                    }
                    #endregion

                }
                #endregion

                #region ObjectProperty
                foreach (var op in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.OBJECT_PROPERTY)) {
                    var obp  = new RDFOntologyObjectProperty((RDFResource)op.Subject);
                    ontology.Model.PropertyModel.AddProperty(obp);

                    #region DeprecatedProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY))) {
                        obp.SetDeprecated(true);
                    }
                    #endregion

                    #region FunctionalProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY))) {
                        obp.SetFunctional(true);
                    }
                    #endregion

                    #region SymmetricProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.SYMMETRIC_PROPERTY))) {
                        obp.SetSymmetric(true);
                    }
                    #endregion

                    #region TransitiveProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.TRANSITIVE_PROPERTY))) {
                        obp.SetTransitive(true);
                    }
                    #endregion

                    #region InverseFunctionalProperty
                    if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)obp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY))) {
                        obp.SetInverseFunctional(true);
                    }
                    #endregion

                }

                #region SymmetricProperty
                foreach (var sp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.SYMMETRIC_PROPERTY)) {
                    var syp  = ontology.Model.PropertyModel.SelectProperty(sp.Subject.ToString());
                    if (syp == null) {
                        syp  = new RDFOntologyObjectProperty((RDFResource)sp.Subject);
                        ontology.Model.PropertyModel.AddProperty(syp);

                        #region DeprecatedProperty
                        if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)syp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY))) {
                            syp.SetDeprecated(true);
                        }
                        #endregion

                        #region FunctionalProperty
                        if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)syp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY))) {
                            syp.SetFunctional(true);
                        }
                        #endregion

                    }
                    ((RDFOntologyObjectProperty)syp).SetSymmetric(true);
                }
                #endregion

                #region TransitiveProperty
                foreach (var tp in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.TRANSITIVE_PROPERTY)) {
                    var trp  = ontology.Model.PropertyModel.SelectProperty(tp.Subject.ToString());
                    if (trp == null) {
                        trp  = new RDFOntologyObjectProperty((RDFResource)tp.Subject);
                        ontology.Model.PropertyModel.AddProperty(trp);

                        #region DeprecatedProperty
                        if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)trp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY))) {
                            trp.SetDeprecated(true);
                        }
                        #endregion

                        #region FunctionalProperty
                        if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)trp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY))) {
                            trp.SetFunctional(true);
                        }
                        #endregion

                    }
                    ((RDFOntologyObjectProperty)trp).SetTransitive(true);
                }
                #endregion

                #region InverseFunctionalProperty
                foreach (var ip in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY)) {
                    var ifp  = ontology.Model.PropertyModel.SelectProperty(ip.Subject.ToString());
                    if (ifp == null) {
                        ifp  = new RDFOntologyObjectProperty((RDFResource)ip.Subject);
                        ontology.Model.PropertyModel.AddProperty(ifp);

                        #region DeprecatedProperty
                        if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)ifp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY))) {
                            ifp.SetDeprecated(true);
                        }
                        #endregion

                        #region FunctionalProperty
                        if (ontGraph.ContainsTriple(new RDFTriple((RDFResource)ifp.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY))) {
                            ifp.SetFunctional(true);
                        }
                        #endregion

                    }
                    ((RDFOntologyObjectProperty)ifp).SetInverseFunctional(true);
                }
                #endregion
                #endregion

                #endregion

                #region ClassModel

                #region Class
                foreach (var c   in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.CLASS)) {
                    var ontClass  = new RDFOntologyClass((RDFResource)c.Subject);
                    ontology.Model.ClassModel.AddClass(ontClass);
                    if   (ontGraph.ContainsTriple(new RDFTriple((RDFResource)ontClass.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_CLASS))) {
                          ontClass.SetDeprecated(true);
                    }
                }
                #endregion

                #region DeprecatedClass
                foreach (var dc  in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.DEPRECATED_CLASS)) {
                    var ontClass  = new RDFOntologyClass((RDFResource)dc.Subject);
                    ontClass.SetDeprecated(true);
                    ontology.Model.ClassModel.AddClass(ontClass);
                }
                #endregion

                #region Restriction
                foreach (var r in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.RESTRICTION)) {

                    #region OnProperty
                    var op   = onProperty.SelectTriplesBySubject((RDFResource)r.Subject).FirstOrDefault();
                    if (op  != null) {
                        var onProp     = ontology.Model.PropertyModel.SelectProperty(op.Object.ToString());
                        if (onProp    != null) {
                            var restr  = new RDFOntologyRestriction((RDFResource)r.Subject, onProp);
                            ontology.Model.ClassModel.AddClass(restr);
                        }
                        else {

                            //Raise warning event to inform the user: restriction cannot be imported from 
                            //graph, because definition of its applied property is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Restriction '{0}' cannot be imported from graph, because definition of its applied property '{1}' is not found in the model.", r.Subject, op.Object));

                        }
                    }
                    #endregion

                }
                #endregion

                #region DataRange
                foreach (var dr  in rdfType.SelectTriplesByObject(RDFVocabulary.OWL.DATA_RANGE)) {
                    ontology.Model.ClassModel.AddClass(new RDFOntologyDataRangeClass((RDFResource)dr.Subject));
                }
                #endregion

                #region Composite

                #region Union
                foreach (var u in unionOf) {
                    if  (u.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var uc          = ontology.Model.ClassModel.SelectClass(u.Subject.ToString());
                         if (uc         != null) {

                            #region ClassToUnionClass
                            if (!(uc    is RDFOntologyUnionClass)) {
                                  uc     = new RDFOntologyUnionClass((RDFResource)u.Subject);
                                  ontology.Model.ClassModel.Classes[uc.PatternMemberID] = uc;
                            }
                            #endregion

                            #region DeserializeUnionCollection
                            var nilFound = false;
                            var itemRest = (RDFResource)u.Object;
                            while (!nilFound) {

                                #region rdf:first
                                var first  = ontGraph.SelectTriplesBySubject(itemRest)
                                                     .SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                                                     .FirstOrDefault();
                                if (first != null   && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                                    var compClass    = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                    if (compClass   != null) {
                                        ontology.Model.ClassModel.AddUnionOfRelation((RDFOntologyUnionClass)uc, compClass);
                                    }
                                    else {

                                        //Raise warning event to inform the user: union class cannot be completely imported
                                        //from graph, because definition of its compositing class is not found in the model
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("UnionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", u.Subject, first.Object));
                                    
                                    }

                                    #region rdf:rest
                                    var rest         = ontGraph.SelectTriplesBySubject(itemRest)
                                                               .SelectTriplesByPredicate(RDFVocabulary.RDF.REST)
                                                               .FirstOrDefault();
                                    if (rest        != null) {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL)) {
                                            nilFound = true;
                                        }
                                        else {
                                            itemRest = (RDFResource)rest.Object;
                                        }
                                    }
                                    #endregion

                                }
                                else {
                                    nilFound = true;
                                }
                                #endregion

                            }
                            #endregion

                         }
                    }
                }
                #endregion

                #region Intersection
                foreach (var i in intersectionOf) {
                    if  (i.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var ic          = ontology.Model.ClassModel.SelectClass(i.Subject.ToString());
                         if (ic         != null) {

                            #region ClassToIntersectionClass
                            if (!(ic    is RDFOntologyIntersectionClass)) {
                                  ic     = new RDFOntologyIntersectionClass((RDFResource)i.Subject);
                                  ontology.Model.ClassModel.Classes[ic.PatternMemberID] = ic;
                            }
                            #endregion

                            #region DeserializeIntersectionCollection
                            var nilFound   = false;
                            var itemRest   = (RDFResource)i.Object;
                            while (!nilFound) {

                                #region rdf:first
                                var first  = ontGraph.SelectTriplesBySubject(itemRest)
                                                     .SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                                                     .FirstOrDefault();
                                if (first != null   && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                                    var compClass    = ontology.Model.ClassModel.SelectClass(first.Object.ToString());
                                    if (compClass   != null) {
                                        ontology.Model.ClassModel.AddIntersectionOfRelation((RDFOntologyIntersectionClass)ic, compClass);
                                    }
                                    else {

                                        //Raise warning event to inform the user: intersection class cannot be completely imported
                                        //from graph, because definition of its compositing class is not found in the model
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("IntersectionClass '{0}' cannot be completely imported from graph, because definition of its compositing class '{1}' is not found in the model.", i.Subject, first.Object));
                                    
                                    }


                                    #region rdf:rest
                                    var rest         = ontGraph.SelectTriplesBySubject(itemRest)
                                                               .SelectTriplesByPredicate(RDFVocabulary.RDF.REST)
                                                               .FirstOrDefault();
                                    if (rest        != null) {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL)) {
                                            nilFound = true;
                                        }
                                        else {
                                            itemRest = (RDFResource)rest.Object;
                                        }
                                    }
                                    #endregion

                                }
                                else {
                                    nilFound = true;
                                }
                                #endregion

                            }
                            #endregion

                         }
                    }
                }
                #endregion

                #region Complement
                foreach (var c in complementOf) {
                    if  (c.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var cc  = ontology.Model.ClassModel.SelectClass(c.Subject.ToString());
                         if (cc != null) {
                             var compClass  = ontology.Model.ClassModel.SelectClass(c.Object.ToString());
                             if (compClass != null) {
                                 cc         = new RDFOntologyComplementClass((RDFResource)c.Subject, compClass);
                                 ontology.Model.ClassModel.Classes[cc.PatternMemberID] = cc;
                             }
                             else {

                                 //Raise warning event to inform the user: complement class cannot be imported
                                 //from graph, because definition of its complemented class is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Class '{0}' cannot be imported from graph, because definition of its complement class '{1}' is not found in the model.", c.Subject, c.Object));

                             }
                         }
                         else {

                             //Raise warning event to inform the user: complement class cannot be imported 
                             //from graph, because its definition is not found in the model
                             RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Class '{0}' cannot be imported from graph, because its definition is not found in the model.", c.Subject));

                         }
                    }
                }
                #endregion

                #endregion

                #endregion

                #endregion

                #region OntologyData

                #region Fact
                foreach (var c  in ontology.Model.ClassModel) {
                    foreach (var t in rdfType.SelectTriplesByObject((RDFResource)c.Value)) {
                        var f    = ontology.Data.SelectFact(t.Subject.ToString());
                        if (f   == null) {
                            f    = new RDFOntologyFact((RDFResource)t.Subject);
                            ontology.Data.AddFact(f);
                        }
                        ontology.Data.AddClassTypeRelation(f, c);
                    }
                }
                #endregion

                #endregion

                #region Finalization

                #region Restriction
                var restrictions = ontology.Model.ClassModel.Where(c => c.IsRestrictionClass()).ToList();
                foreach (var     r in restrictions) {

                    #region Cardinality
                    Int32 exC    = 0;
                    var  crEx    = cardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crEx    != null   && crEx.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                        if (crEx.Object   is RDFPlainLiteral) {
                            if (Regex.IsMatch(crEx.Object.ToString(), @"^[0-9]+$")) {
                                exC        = Int32.Parse(crEx.Object.ToString());
                            }
                        }
                        else {
                            if (((RDFTypedLiteral)crEx.Object).Datatype.Category == RDFModelEnums.RDFDatatypeCategory.Numeric) {
                                if (Regex.IsMatch(((RDFTypedLiteral)crEx.Object).Value, @"^[0-9]+$")) {
                                    exC    = Int32.Parse(((RDFTypedLiteral)crEx.Object).Value);
                                }
                            }
                        }
                    }
                    if (exC > 0) {
                        var cardRestr      = new RDFOntologyCardinalityRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, exC, exC);
                        ontology.Model.ClassModel.Classes[r.PatternMemberID] = cardRestr;
                        continue;
                    }

                    Int32 minC   = 0;
                    var  crMin   = mincardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crMin   != null   && crMin.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                        if (crMin.Object  is RDFPlainLiteral) {
                            if (Regex.IsMatch(crMin.Object.ToString(), @"^[0-9]+$")) {
                                minC       = Int32.Parse(crMin.Object.ToString());
                            }
                        }
                        else {
                            if (((RDFTypedLiteral)crMin.Object).Datatype.Category == RDFModelEnums.RDFDatatypeCategory.Numeric) {
                                if (Regex.IsMatch(((RDFTypedLiteral)crMin.Object).Value, @"^[0-9]+$")) {
                                    minC   = Int32.Parse(((RDFTypedLiteral)crMin.Object).Value);
                                }
                            }
                        }
                    }

                    Int32 maxC   = 0;
                    var  crMax   = maxcardinality.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (crMax   != null   && crMax.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                        if (crMax.Object  is RDFPlainLiteral) {
                            if (Regex.IsMatch(crMax.Object.ToString(), @"^[0-9]+$")) {
                                maxC       = Int32.Parse(crMax.Object.ToString());
                            }
                        }
                        else {
                            if (((RDFTypedLiteral)crMax.Object).Datatype.Category == RDFModelEnums.RDFDatatypeCategory.Numeric) {
                                if (Regex.IsMatch(((RDFTypedLiteral)crMax.Object).Value, @"^[0-9]+$")) {
                                    maxC   = Int32.Parse(((RDFTypedLiteral)crMax.Object).Value);
                                }
                            }
                        }
                    }
                    if (minC > 0  ||  maxC > 0) {
                        var cardRestr      = new RDFOntologyCardinalityRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, minC, maxC);
                        ontology.Model.ClassModel.Classes[r.PatternMemberID] = cardRestr;
                        continue;
                    }
                    #endregion

                    #region HasValue
                    var hvRes    = hasvalue.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (hvRes   != null) {
                        if (hvRes.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                            var hvFct             = ontology.Data.SelectFact(hvRes.Object.ToString());
                            if (hvFct            != null) {
                                var hasvalueRestr = new RDFOntologyHasValueRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, hvFct);
                                ontology.Model.ClassModel.Classes[r.PatternMemberID] = hasvalueRestr;
                                continue;
                            }
                            else {

                                //Raise warning event to inform the user: hasvalue restriction cannot be imported
                                //from graph, because definition of its required fact is not found in the data
                                RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Restriction '{0}' cannot be imported from graph, because definition of its required fact '{1}' is not found in the data.", r.Value, hvRes.Object));

                            }
                        }
                        else {
                            var hasvalueRestr     = new RDFOntologyHasValueRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, new RDFOntologyLiteral((RDFLiteral)hvRes.Object));
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = hasvalueRestr;
                            continue;
                        }
                    }
                    #endregion

                    #region AllValuesFrom
                    var avfRes       = allvaluesFrom.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (avfRes      != null   && avfRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                        var avfCls   = ontology.Model.ClassModel.SelectClass(avfRes.Object.ToString());
                        if (avfCls  != null) {
                            var allvaluesfromRestr = new RDFOntologyAllValuesFromRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, avfCls);
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = allvaluesfromRestr;
                            continue;
                        }
                        else {

                            //Raise warning event to inform the user: allvaluesfrom restriction cannot be imported
                            //from graph, because definition of its required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Restriction '{0}' cannot be imported from graph, because definition of its required class '{1}' is not found in the model.", r.Value, avfRes.Object));

                        }
                    }
                    #endregion

                    #region SomeValuesFrom
                    var svfRes      = somevaluesFrom.SelectTriplesBySubject((RDFResource)r.Value).FirstOrDefault();
                    if (svfRes     != null   && svfRes.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                        var svfCls  = ontology.Model.ClassModel.SelectClass(svfRes.Object.ToString());
                        if (svfCls != null) {
                            var somevaluesfromRestr = new RDFOntologySomeValuesFromRestriction((RDFResource)r.Value, ((RDFOntologyRestriction)r).OnProperty, svfCls);
                            ontology.Model.ClassModel.Classes[r.PatternMemberID] = somevaluesfromRestr;
                            continue;
                        }
                        else {

                            //Raise warning event to inform the user: somevaluesfrom restriction cannot be imported
                            //from graph, because definition of its required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Restriction '{0}' cannot be imported from graph, because definition of its required class '{1}' is not found in the model.", r.Value, svfRes.Object));

                        }
                    }
                    #endregion

                }
                #endregion

                #region Enumerate
                foreach (var e  in  oneOf) {
                    if  (e.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var ec          = ontology.Model.ClassModel.SelectClass(e.Subject.ToString());
                         if (ec         != null && !ec.IsDataRangeClass()) {

                            #region ClassToEnumerateClass
                            if (!ec.IsEnumerateClass()) {
                                 ec      = new RDFOntologyEnumerateClass((RDFResource)e.Subject);
                                 ontology.Model.ClassModel.Classes[ec.PatternMemberID] = ec;
                            }
                            #endregion

                            #region DeserializeEnumerateCollection
                            var nilFound   = false;
                            var itemRest   = (RDFResource)e.Object;
                            while (!nilFound) {

                                #region rdf:first
                                var first  = ontGraph.SelectTriplesBySubject(itemRest)
                                                     .SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                                                     .FirstOrDefault();
                                if (first != null   && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                                    var enumMember   = ontology.Data.SelectFact(first.Object.ToString());
                                    if (enumMember  != null) {
                                        ontology.Model.ClassModel.AddOneOfRelation((RDFOntologyEnumerateClass)ec, enumMember);
                                    }
									else {

                                        //Raise warning event to inform the user: enumerate class cannot be completely imported
                                        //from graph, because definition of its fact member is not found in the data
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EnumerateClass '{0}' cannot be completely imported from graph, because definition of its fact member '{1}' is not found in the data.", e.Subject, first.Object));

			                        }

                                    #region rdf:rest
                                    var rest         = ontGraph.SelectTriplesBySubject(itemRest)
                                                               .SelectTriplesByPredicate(RDFVocabulary.RDF.REST)
                                                               .FirstOrDefault();
                                    if (rest        != null) {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL)) {
                                            nilFound = true;
                                        }
                                        else {
                                            itemRest = (RDFResource)rest.Object;
                                        }
                                    }
                                    #endregion

                                }
                                else {
                                    nilFound = true;
                                }
                                #endregion

                            }
                            #endregion

                         }
                         else {
                             if (ec     == null) {

                                 //Raise warning event to inform the user: enumerate class cannot be imported
                                 //from graph, because its definition is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EnumerateClass '{0}' cannot be imported from graph, because its definition is not found in the model.", e.Subject));

                             }
                         }
                    }
                }
                #endregion

                #region DataRange
                foreach (var d in oneOf) {
                    if  (d.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var dr          = ontology.Model.ClassModel.SelectClass(d.Subject.ToString());
                         if (dr         != null && !dr.IsEnumerateClass()) {

                            #region ClassToDataRangeClass
                            if (!dr.IsDataRangeClass()) {
                                 dr      = new RDFOntologyDataRangeClass((RDFResource)d.Subject);
                                 ontology.Model.ClassModel.Classes[dr.PatternMemberID] = dr;
                            }
                            #endregion

                            #region DeserializeDataRangeCollection
                            var nilFound   = false;
                            var itemRest   = (RDFResource)d.Object;
                            while (!nilFound) {

                                #region rdf:first
                                var first  = ontGraph.SelectTriplesBySubject(itemRest)
                                                     .SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                                                     .FirstOrDefault();
                                if (first != null   && first.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                                    ontology.Model.ClassModel.AddOneOfRelation((RDFOntologyDataRangeClass)dr, new RDFOntologyLiteral((RDFLiteral)first.Object));

                                    #region rdf:rest
                                    var rest         = ontGraph.SelectTriplesBySubject(itemRest)
                                                               .SelectTriplesByPredicate(RDFVocabulary.RDF.REST)
                                                               .FirstOrDefault();
                                    if (rest        != null) {
                                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL)) {
                                            nilFound = true;
                                        }
                                        else {
                                            itemRest = (RDFResource)rest.Object;
                                        }
                                    }
                                    #endregion

                                }
                                else {
                                    nilFound = true;
                                }
                                #endregion

                            }
                            #endregion

                         }
                         else {
                             if (dr     == null) {

                                 //Raise warning event to inform the user: datarange class cannot be imported from
                                 //graph, because its definition is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("DataRangeClass '{0}' cannot be imported from graph, because its definition is not found in the model.", d.Subject));

                             }
                         }
                    }
                }
                #endregion

                #region Domain/Range
                foreach (var p in ontology.Model.PropertyModel) {
                    
                    #region Domain
                    var d   = domain.SelectTriplesBySubject((RDFResource)p.Value).FirstOrDefault();
                    if (d  != null   && d.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                        var domainClass  = ontology.Model.ClassModel.SelectClass(d.Object.ToString());
                        if (domainClass != null) {
                            p.SetDomain(domainClass);
                        }
                        else {

                            //Raise warning event to inform the user: domain constraint cannot be imported from graph, 
                            //because definition of required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Domain constraint on property '{0}' cannot be imported from graph, because definition of required class '{1}' is not found in the model.", p.Value, d.Object));

                        }
                    }
                    #endregion

                    #region Range
                    var r   = range.SelectTriplesBySubject((RDFResource)p.Value).FirstOrDefault();
                    if (r  != null  && r.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                        var rangeClass  = ontology.Model.ClassModel.SelectClass(r.Object.ToString());
                        if (rangeClass != null) {
                            p.SetRange(rangeClass);
                        }
                        else {

                            //Raise warning event to inform the user: range constraint cannot be imported from graph, 
                            //because definition of required class is not found in the model
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Range constraint on property '{0}' cannot be imported from graph, because definition of required class '{1}' is not found in the model.", p.Value, r.Object));

                        }
                    }
                    #endregion

                }
                #endregion

                #region PropertyModel Relations
                foreach (var p in ontology.Model.PropertyModel) {
                    
                    #region SubPropertyOf
                    foreach (var spof in subpropOf.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (spof.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                             var superProp        = ontology.Model.PropertyModel.SelectProperty(spof.Object.ToString());
                             if (superProp       != null) {
                                 if (p.IsObjectProperty()        && superProp.IsObjectProperty()) {
                                     ontology.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)p,   (RDFOntologyObjectProperty)superProp);
                                 }
                                 else if (p.IsDatatypeProperty() && superProp.IsDatatypeProperty()) {
                                     ontology.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)p, (RDFOntologyDatatypeProperty)superProp);
                                 }
                             }
                             else {

                                 //Raise warning event to inform the user: subpropertyof relation cannot be imported
                                 //from graph, because definition of property is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SubPropertyOf relation on property '{0}' cannot be imported from graph, because definition of property '{1}' is not found in the model.", p.Value, spof.Object));

                             }
                        }
                    }
                    #endregion

                    #region EquivalentProperty
                    foreach (var eqpr in equivpropOf.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (eqpr.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                             var equivProp        = ontology.Model.PropertyModel.SelectProperty(eqpr.Object.ToString());
                             if (equivProp       != null) {
                                 if (p.IsObjectProperty()        && equivProp.IsObjectProperty()) {
                                     ontology.Model.PropertyModel.AddEquivalentPropertyRelation((RDFOntologyObjectProperty)p,   (RDFOntologyObjectProperty)equivProp);
                                 }
                                 else if (p.IsDatatypeProperty() && equivProp.IsDatatypeProperty()) {
                                     ontology.Model.PropertyModel.AddEquivalentPropertyRelation((RDFOntologyDatatypeProperty)p, (RDFOntologyDatatypeProperty)equivProp);
                                 }
                             }
                             else {

                                 //Raise warning event to inform the user: equivalentproperty relation cannot be imported
                                 //from graph, because definition of property is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EquivalentProperty relation on property '{0}' cannot be imported from graph, because definition of property '{1}' is not found in the model.", p.Value, eqpr.Object));

                             }
                        }
                    }
                    #endregion

                    #region InverseOf
                    if (p.IsObjectProperty()) {
                        foreach (var inof in inverseOf.SelectTriplesBySubject((RDFResource)p.Value)) {
                            if  (inof.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                                 var invProp          = ontology.Model.PropertyModel.SelectProperty(inof.Object.ToString());
                                 if (invProp != null && invProp.IsObjectProperty()) {
                                     ontology.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)p, (RDFOntologyObjectProperty)invProp);
                                 }
                                 else {

                                     //Raise warning event to inform the user: inverseof relation cannot be imported
                                     //from graph, because definition of property is not found in the model
                                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("InverseOf relation on property '{0}' cannot be imported from graph, because definition of property '{1}' is not found in the model.", p.Value, inof.Object));

                                 }
                            }
                        }
                    }
                    #endregion

                }
                #endregion

                #region ClassModel Relations
                foreach (var c in ontology.Model.ClassModel) {

                    #region SubClassOf
                    foreach (var scof in subclassOf.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (scof.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                             var superClass       = ontology.Model.ClassModel.SelectClass(scof.Object.ToString());
                             if (superClass      != null) {
                                 ontology.Model.ClassModel.AddSubClassOfRelation(c, superClass);
                             }
                             else {

                                 //Raise warning event to inform the user: subclassof relation cannot be imported
                                 //from graph, because definition of class is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SubClassOf relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", c.Value, scof.Object));

                             }
                        }
                    }
                    #endregion

                    #region EquivalentClass
                    foreach (var eqcl in equivclassOf.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (eqcl.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                             var equivClass       = ontology.Model.ClassModel.SelectClass(eqcl.Object.ToString());
                             if (equivClass      != null) {
                                 ontology.Model.ClassModel.AddEquivalentClassRelation(c, equivClass);
                             }
                             else {

                                 //Raise warning event to inform the user: equivalentclass relation cannot be imported
                                 //from graph, because definition of class is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EquivalentClass relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", c.Value, eqcl.Object));

                             }
                        }
                    }
                    #endregion

                    #region DisjointWith
                    foreach (var djwt in disjclassWith.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (djwt.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                             var disjWith         = ontology.Model.ClassModel.SelectClass(djwt.Object.ToString());
                             if (disjWith        != null) {
                                 ontology.Model.ClassModel.AddDisjointWithRelation(c, disjWith);
                             }
                             else {

                                 //Raise warning event to inform the user: disjointwith relation cannot be imported
                                 //from graph, because definition of class is not found in the model
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("DisjointWith relation on class '{0}' cannot be imported from graph, because definition of class '{1}' is not found in the model.", c.Value, djwt.Object));

                             }
                        }
                    }
                    #endregion

                }
                #endregion

                #region Data Relations

                #region SameAs
                foreach (var t in sameAs) {
                    if  (t.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var subjFct       = ontology.Data.SelectFact(t.Subject.ToString());
                         if (subjFct      != null) {
                             var objFct    = ontology.Data.SelectFact(t.Object.ToString());
                             if (objFct   != null) {
                                 ontology.Data.AddSameAsRelation(subjFct, objFct);
                             }
                             else {

                                 //Raise warning event to inform the user: sameas relation cannot be imported
                                 //from graph, because definition of fact is not found in the data
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SameAs relation on fact '{0}' cannot be imported from graph, because definition of fact '{1}' is not found in the data.", t.Subject, t.Object));

                             }
                         }
                         else {

                             //Raise warning event to inform the user: sameas relation cannot be imported
                             //from graph, because definition of fact is not found in the data
                             RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SameAs relation on fact '{0}' cannot be imported from graph, because its definition is not found in the data.", t.Subject));

                         }
                    }
                }
                #endregion

                #region DifferentFrom
                foreach (var t in differentFrom) {
                    if  (t.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                         var subjFct       = ontology.Data.SelectFact(t.Subject.ToString());
                         if (subjFct      != null) {
                             var objFct    = ontology.Data.SelectFact(t.Object.ToString());
                             if (objFct   != null) {
                                 ontology.Data.AddDifferentFromRelation(subjFct, objFct);
                             }
                             else {

                                 //Raise warning event to inform the user: differentfrom relation cannot be imported
                                 //from graph, because definition of fact is not found in the data
                                 RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("DifferentFrom relation on fact '{0}' cannot be imported from graph, because definition of fact '{1}' is not found in the data.", t.Subject, t.Object));

                             }
                         }
                         else {

                             //Raise warning event to inform the user: differentfrom relation cannot be imported
                             //from graph, because its definition is not found in the data
                             RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("DifferentFrom relation on fact '{0}' cannot be imported from graph, because its definition is not found in the data.", t.Subject));

                         }
                    }
                }
                #endregion

                #region Assertion
                foreach (var p      in ontology.Model.PropertyModel) {
                    foreach (var t  in ontGraph.SelectTriplesByPredicate((RDFResource)p.Value)) {
                        var subjFct  = ontology.Data.SelectFact(t.Subject.ToString());
                        if (subjFct != null) {
                            if (p.IsObjectProperty()) {
                                if (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPO) {
                                    var objFct       = ontology.Data.SelectFact(t.Object.ToString());
                                    if (objFct      != null) {
                                        ontology.Data.AddAssertionRelation(subjFct, (RDFOntologyObjectProperty)p, objFct);
                                    }
                                    else {

                                        //Raise warning event to inform the user: assertion relation cannot be imported
                                        //from graph, because definition of fact is not found in the data
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation on fact '{0}' cannot be imported from graph, because definition of fact '{1}' is not found in the data.", t.Subject, t.Object));

                                    }
                                }
                                else {

                                    //Raise warning event to inform the user: assertion relation cannot be imported
                                    //from graph, because object property links to a literal
                                    RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation on fact '{0}' cannot be imported from graph, because object property '{1}' links to a literal.", t.Subject, p));

                                }
                            }
                            else if (p.IsDatatypeProperty()) {
                                 if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                                     ontology.Data.AddAssertionRelation(subjFct, (RDFOntologyDatatypeProperty)p, new RDFOntologyLiteral((RDFLiteral)t.Object));
                                 }
                                 else {

                                     //Raise warning event to inform the user: assertion relation cannot be imported
                                     //from graph, because datatype property links to a fact
                                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation on fact '{0}' cannot be imported from graph, because datatype property '{1}' links to a fact.", t.Subject, p));

                                 }
                            }
                        }
                        else {

                            //Raise warning event to inform the user: assertion relation cannot be imported
                            //from graph, because definition of fact is not found in the data
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation on fact '{0}' cannot be imported from graph, because definition of the fact is not found in the data. Ensure its classtype relation is specified.", t.Subject));

                        }
                    }
                }
                #endregion

                #endregion

                #region Annotations

                #region Ontology

                #region VersionInfo
                foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                         ontology.AddVersionInfoAnnotation(new RDFOntologyLiteral((RDFLiteral)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: versioninfo annotation on ontology 
                        //cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("VersionInfo annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region Comment
                foreach (var t in comment.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                         ontology.AddCommentAnnotation(new RDFOntologyLiteral((RDFLiteral)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: comment annotation on ontology 
                        //cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Comment annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region Label
                foreach (var t in label.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                         ontology.AddLabelAnnotation(new RDFOntologyLiteral((RDFLiteral)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: label annotation on ontology 
                        //cannot be imported from graph, because it does not link a literal
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Label annotation on ontology '{0}' cannot be imported from graph, because it does not link a literal.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region SeeAlso
                foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                         ontology.AddSeeAlsoAnnotation(new RDFOntologyLiteral((RDFLiteral)t.Object));
                    }
                    else {
                        RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (resource         == null) {
                            resource          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (resource     == null) {
                                resource      = ontology.Data.SelectFact(t.Object.ToString());
                                if (resource == null) {
                                    resource  = new RDFOntologyResource();
                                    resource.Value           = t.Object;
                                    resource.PatternMemberID = t.Object.PatternMemberID;

                                    //Raise warning event to inform the user: seealso annotation on ontology 
                                    //has been imported from graph, but linking an unknown generic resource
                                    RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SeeAlso annotation on ontology '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", ontology.Value, t.Object));

                                }
                            }
                        }
                        ontology.AddSeeAlsoAnnotation(resource);
                    }
                }
                #endregion

                #region IsDefinedBy
                foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                         ontology.AddIsDefinedByAnnotation(new RDFOntologyLiteral((RDFLiteral)t.Object));
                    }
                    else {
                        RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                        if (isDefBy         == null) {
                            isDefBy          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                            if (isDefBy     == null) {
                                isDefBy      = ontology.Data.SelectFact(t.Object.ToString());
                                if (isDefBy == null) {
                                    isDefBy  = new RDFOntologyResource();
                                    isDefBy.Value           = t.Object;
                                    isDefBy.PatternMemberID = t.Object.PatternMemberID;

                                    //Raise warning event to inform the user: isdefinedby annotation on ontology 
                                    //has been imported from graph, but linking an unknown generic resource
                                    RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("IsDefinedBy annotation on ontology '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", ontology.Value, t.Object));

                                }
                            }
                        }
                        ontology.AddIsDefinedByAnnotation(isDefBy);
                    }
                }
                #endregion

                #region BackwardCompatibleWith
                foreach (var t in bcwcompWith.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         ontology.AddBackwardCompatibleWithAnnotation(new RDFOntology((RDFResource)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: backwardcompatiblewith annotation on ontology 
                        //cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("BackwardCompatibleWith annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region IncompatibleWith
                foreach (var t in incompWith.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         ontology.AddIncompatibleWithAnnotation(new RDFOntology((RDFResource)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: incompatiblewith annotation on ontology 
                        //cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("IncompatibleWith annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region PriorVersion
                foreach (var t in priorVersion.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                         ontology.AddPriorVersionAnnotation(new RDFOntology((RDFResource)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: priorversion annotation on ontology 
                        //cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("PriorVersion annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region Imports
                foreach(var t in imports.SelectTriplesBySubject((RDFResource)ontology.Value)) {
                    if (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPO) {
                        ontology.AddImportsAnnotation(new RDFOntology((RDFResource)t.Object));
                    }
                    else {

                        //Raise warning event to inform the user: imports annotation on ontology 
                        //cannot be imported from graph, because it does not link a resource
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Imports annotation on ontology '{0}' cannot be imported from graph, because it does not link a resource.", ontology.Value, t.Object));

                    }
                }
                #endregion

                #region CustomAnnotations
                var annotProps   = ontology.Model.PropertyModel.AnnotationPropertiesEnumerator;
                while (annotProps.MoveNext()) {
                    foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)ontology.Value)
                                              .SelectTriplesByPredicate((RDFResource)annotProps.Current.Value)) {
                        if  (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProps.Current, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource custAnn = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (custAnn         == null) {
                                custAnn          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (custAnn     == null) {
                                    custAnn      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (custAnn == null) {
                                        custAnn  = new RDFOntologyResource();
                                        custAnn.Value           = t.Object;
                                        custAnn.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: custom annotation on ontology 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Custom annotation '{0}' on ontology '{1}' has been imported from graph, but linking an unknown generic resource '{2}'.", annotProps.Current.Value, ontology.Value, t.Object));

                                    }
                                }
                            }
                            ontology.AddCustomAnnotation((RDFOntologyAnnotationProperty)annotProps.Current, custAnn);
                        }

                    }
                }
                #endregion

                #endregion

                #region Classes
                foreach (var c in ontology.Model.ClassModel) {

                    #region VersionInfo
                    foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.ClassModel.AddVersionInfoAnnotation(c, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: versioninfo annotation on class 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("VersionInfo annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value, t.Object));

                        }
                    }
                    #endregion

                    #region Comment
                    foreach (var t in comment.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.ClassModel.AddCommentAnnotation(c, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: comment annotation on class 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Comment annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value, t.Object));

                        }
                    }
                    #endregion

                    #region Label
                    foreach (var t in label.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.ClassModel.AddLabelAnnotation(c, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: label annotation on class 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Label annotation on class '{0}' cannot be imported from graph, because it does not link a literal.", c.Value, t.Object));

                        }
                    }
                    #endregion

                    #region SeeAlso
                    foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.ClassModel.AddSeeAlsoAnnotation(c, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (resource         == null) {
                                resource          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (resource     == null) {
                                    resource      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (resource == null) {
                                        resource  = new RDFOntologyResource();
                                        resource.Value           = t.Object;
                                        resource.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: seealso annotation on class 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SeeAlso annotation on class '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", c.Value, t.Object));

                                    }
                                }
                            }
                            ontology.Model.ClassModel.AddSeeAlsoAnnotation(c, resource);
                        }
                    }
                    #endregion

                    #region IsDefinedBy
                    foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)c.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.ClassModel.AddIsDefinedByAnnotation(c, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (isDefBy         == null) {
                                isDefBy          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (isDefBy     == null) {
                                    isDefBy      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (isDefBy == null) {
                                        isDefBy  = new RDFOntologyResource();
                                        isDefBy.Value           = t.Object;
                                        isDefBy.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: isdefinedby annotation on class 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("IsDefinedBy annotation on class '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", c.Value, t.Object));

                                    }
                                }
                            }
                            ontology.Model.ClassModel.AddIsDefinedByAnnotation(c, isDefBy);
                        }
                    }
                    #endregion

                    #region CustomAnnotations
                    annotProps       = ontology.Model.PropertyModel.AnnotationPropertiesEnumerator;
                    while (annotProps.MoveNext()) {
                        foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)c.Value)
                                                  .SelectTriplesByPredicate((RDFResource)annotProps.Current.Value)) {
                            if  (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPL) {
                                 ontology.Model.ClassModel.AddCustomAnnotation(c, (RDFOntologyAnnotationProperty)annotProps.Current, new RDFOntologyLiteral((RDFLiteral)t.Object));
                            }
                            else {
                                RDFOntologyResource custAnn = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                                if (custAnn         == null) {
                                    custAnn          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                    if (custAnn     == null) {
                                        custAnn      = ontology.Data.SelectFact(t.Object.ToString());
                                        if (custAnn == null) {
                                            custAnn  = new RDFOntologyResource();
                                            custAnn.Value           = t.Object;
                                            custAnn.PatternMemberID = t.Object.PatternMemberID;

                                            //Raise warning event to inform the user: custom annotation on class 
                                            //has been imported from graph, but linking an unknown generic resource
                                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Custom annotation '{0}' on class '{1}' has been imported from graph, but linking an unknown generic resource '{2}'.", annotProps.Current.Value, c.Value, t.Object));

                                        }
                                    }
                                }
                                ontology.Model.ClassModel.AddCustomAnnotation(c, (RDFOntologyAnnotationProperty)annotProps.Current, custAnn);
                            }

                        }
                    }
                    #endregion

                }
                #endregion

                #region Properties
                foreach (var p in ontology.Model.PropertyModel) {

                    #region VersionInfo
                    foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.PropertyModel.AddVersionInfoAnnotation(p, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: versioninfo annotation on property 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("VersionInfo annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value, t.Object));

                        }
                    }
                    #endregion

                    #region Comment
                    foreach (var t in comment.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.PropertyModel.AddCommentAnnotation(p, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: comment annotation on property 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Comment annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value, t.Object));

                        }
                    }
                    #endregion

                    #region Label
                    foreach (var t in label.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.PropertyModel.AddLabelAnnotation(p, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: label annotation on property 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Label annotation on property '{0}' cannot be imported from graph, because it does not link a literal.", p.Value, t.Object));

                        }
                    }
                    #endregion

                    #region SeeAlso
                    foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.PropertyModel.AddSeeAlsoAnnotation(p, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (resource         == null) {
                                resource          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (resource     == null) {
                                    resource      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (resource == null) {
                                        resource  = new RDFOntologyResource();
                                        resource.Value           = t.Object;
                                        resource.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: seealso annotation on property 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SeeAlso annotation on property '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", p.Value, t.Object));

                                    }
                                }
                            }
                            ontology.Model.PropertyModel.AddSeeAlsoAnnotation(p, resource);
                        }
                    }
                    #endregion

                    #region IsDefinedBy
                    foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)p.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Model.PropertyModel.AddIsDefinedByAnnotation(p, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (isDefBy         == null) {
                                isDefBy          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (isDefBy     == null) {
                                    isDefBy      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (isDefBy == null) {
                                        isDefBy  = new RDFOntologyResource();
                                        isDefBy.Value           = t.Object;
                                        isDefBy.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: isdefinedby annotation on property 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("IsDefinedBy annotation on property '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", p.Value, t.Object));

                                    }
                                }
                            }
                            ontology.Model.PropertyModel.AddIsDefinedByAnnotation(p, isDefBy);
                        }
                    }
                    #endregion

                    #region CustomAnnotations
                    annotProps       = ontology.Model.PropertyModel.AnnotationPropertiesEnumerator;
                    while (annotProps.MoveNext()) {
                        foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)p.Value)
                                                  .SelectTriplesByPredicate((RDFResource)annotProps.Current.Value)) {
                            if  (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPL) {
                                 ontology.Model.PropertyModel.AddCustomAnnotation(p, (RDFOntologyAnnotationProperty)annotProps.Current, new RDFOntologyLiteral((RDFLiteral)t.Object));
                            }
                            else {
                                RDFOntologyResource custAnn = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                                if (custAnn         == null) {
                                    custAnn          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                    if (custAnn     == null) {
                                        custAnn      = ontology.Data.SelectFact(t.Object.ToString());
                                        if (custAnn == null) {
                                            custAnn  = new RDFOntologyResource();
                                            custAnn.Value           = t.Object;
                                            custAnn.PatternMemberID = t.Object.PatternMemberID;

                                            //Raise warning event to inform the user: custom annotation on property 
                                            //has been imported from graph, but linking an unknown generic resource
                                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Custom annotation '{0}' on property '{1}' has been imported from graph, but linking an unknown generic resource '{2}'.", annotProps.Current.Value, p.Value, t.Object));

                                        }
                                    }
                                }
                                ontology.Model.PropertyModel.AddCustomAnnotation(p, (RDFOntologyAnnotationProperty)annotProps.Current, custAnn);
                            }

                        }
                    }
                    #endregion

                }
                #endregion

                #region Facts
                foreach (var f in ontology.Data) {

                    #region VersionInfo
                    foreach (var t in versionInfo.SelectTriplesBySubject((RDFResource)f.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Data.AddVersionInfoAnnotation(f, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: versioninfo annotation on fact 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("VersionInfo annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value, t.Object));

                        }
                    }
                    #endregion

                    #region Comment
                    foreach (var t in comment.SelectTriplesBySubject((RDFResource)f.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Data.AddCommentAnnotation(f, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: comment annotation on fact 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Comment annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value, t.Object));

                        }
                    }
                    #endregion

                    #region Label
                    foreach (var t in label.SelectTriplesBySubject((RDFResource)f.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Data.AddLabelAnnotation(f, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {

                            //Raise warning event to inform the user: label annotation on fact 
                            //cannot be imported from graph, because it does not link a literal
                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Label annotation on fact '{0}' cannot be imported from graph, because it does not link a literal.", f.Value, t.Object));

                        }
                    }
                    #endregion

                    #region SeeAlso
                    foreach (var t in seeAlso.SelectTriplesBySubject((RDFResource)f.Value)) {
                        if  (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Data.AddSeeAlsoAnnotation(f, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource resource = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (resource         == null) {
                                resource          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (resource     == null) {
                                    resource      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (resource == null) {
                                        resource  = new RDFOntologyResource();
                                        resource.Value           = t.Object;
                                        resource.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: seealso annotation on fact 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SeeAlso annotation on fact '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", f.Value, t.Object));

                                    }
                                }
                            }
                            ontology.Data.AddSeeAlsoAnnotation(f, resource);
                        }
                    }
                    #endregion

                    #region IsDefinedBy
                    foreach (var t in isDefinedBy.SelectTriplesBySubject((RDFResource)f.Value)) {
                        if  (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL) {
                             ontology.Data.AddIsDefinedByAnnotation(f, new RDFOntologyLiteral((RDFLiteral)t.Object));
                        }
                        else {
                            RDFOntologyResource isDefBy = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                            if (isDefBy         == null) {
                                isDefBy          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                if (isDefBy     == null) {
                                    isDefBy      = ontology.Data.SelectFact(t.Object.ToString());
                                    if (isDefBy == null) {
                                        isDefBy  = new RDFOntologyResource();
                                        isDefBy.Value           = t.Object;
                                        isDefBy.PatternMemberID = t.Object.PatternMemberID;

                                        //Raise warning event to inform the user: isdefinedby annotation on fact 
                                        //has been imported from graph, but linking an unknown generic resource
                                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("IsDefinedBy annotation on fact '{0}' has been imported from graph, but linking an unknown generic resource '{1}'.", f.Value, t.Object));

                                    }
                                }
                            }
                            ontology.Data.AddIsDefinedByAnnotation(f, isDefBy);
                        }
                    }
                    #endregion

                    #region CustomAnnotations
                    annotProps       = ontology.Model.PropertyModel.AnnotationPropertiesEnumerator;
                    while (annotProps.MoveNext()) {
                        foreach (var t in ontGraph.SelectTriplesBySubject((RDFResource)f.Value)
                                                  .SelectTriplesByPredicate((RDFResource)annotProps.Current.Value)) {
                            if  (t.TripleFlavor  == RDFModelEnums.RDFTripleFlavors.SPL) {
                                 ontology.Data.AddCustomAnnotation(f, (RDFOntologyAnnotationProperty)annotProps.Current, new RDFOntologyLiteral((RDFLiteral)t.Object));
                            }
                            else {
                                RDFOntologyResource custAnn = ontology.Model.ClassModel.SelectClass(t.Object.ToString());
                                if (custAnn         == null) {
                                    custAnn          = ontology.Model.PropertyModel.SelectProperty(t.Object.ToString());
                                    if (custAnn     == null) {
                                        custAnn      = ontology.Data.SelectFact(t.Object.ToString());
                                        if (custAnn == null) {
                                            custAnn  = new RDFOntologyResource();
                                            custAnn.Value           = t.Object;
                                            custAnn.PatternMemberID = t.Object.PatternMemberID;

                                            //Raise warning event to inform the user: custom annotation on fact 
                                            //has been imported from graph, but linking an unknown generic resource
                                            RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Custom annotation '{0}' on fact '{1}' has been imported from graph, but linking an unknown generic resource '{2}'.", annotProps.Current.Value, f.Value, t.Object));

                                        }
                                    }
                                }
                                ontology.Data.AddCustomAnnotation(f, (RDFOntologyAnnotationProperty)annotProps.Current, custAnn);
                            }

                        }
                    }
                    #endregion

                }
                #endregion

                #endregion

                #endregion

                #endregion

            }
            return ontology;
        }

        /// <summary>
        /// Gets a graph representation of the given ontology, eventually including inferences
        /// </summary>
        internal static RDFGraph ToRDFGraph(RDFOntology ontology, Boolean includeInferences) {
            var result    = new RDFGraph();
            if (ontology != null) {

                //Ontology
                result.AddTriple(new RDFTriple((RDFResource)ontology.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ONTOLOGY));
                result    = result.UnionWith(ontology.Annotations.VersionInfo.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.Comment.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.Label.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.SeeAlso.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.IsDefinedBy.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.BackwardCompatibleWith.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.IncompatibleWith.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.PriorVersion.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.Imports.ToRDFGraph(includeInferences))
                                  .UnionWith(ontology.Annotations.CustomAnnotations.ToRDFGraph(includeInferences));

                //Model
                result    = result.UnionWith(ontology.Model.ToRDFGraph(includeInferences));

                //Data
                result    = result.UnionWith(ontology.Data.ToRDFGraph(includeInferences));

                //Ontology Name
                result.SetContext(((RDFResource)ontology.Value).URI);
            }

            return result;
        }
        #endregion

        #region Reason
        /// <summary>
        /// Triggers the execution of the given rule on the given ontology. 
        /// Returns a boolean indicating if new evidences have been found.
        /// </summary>
        internal static Boolean TriggerRule(String ruleName, RDFOntologyReasoner reasoner, RDFOntology ontology, RDFOntologyReasoningReport report) {
            var reasonerRule  = reasoner.SelectRule(ruleName);
            if (reasonerRule != null) {

                //Raise launching signal
                RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Launching execution of reasoning rule '{0}'", ruleName));

                //Launch the reasoning rule
                var oldCnt    = report.EvidencesCount;
                reasonerRule.ExecuteRule(ontology, report);
                var newCnt    = report.EvidencesCount - oldCnt;

                //Raise termination signal
                RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Completed execution of reasoning rule '{0}': found {1} new evidences", ruleName, newCnt));

                return newCnt > 0;
            }
            return false;
        }
        #endregion

    }

}