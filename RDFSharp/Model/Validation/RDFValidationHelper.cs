/*
   Copyright 2012-2020 Marco De Salvo
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

using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{
    /// <summary>
    ///  RDFValidationHelper contains utility methods supporting SHACL modeling and validation
    /// </summary>
    internal static class RDFValidationHelper {

        #region Modeling
        /// <summary>
        /// Gets the focus nodes of the given shape
        /// </summary>
        internal static List<RDFPatternMember> GetFocusNodesOf(this RDFGraph dataGraph, RDFShape shape) {
            List<RDFPatternMember> result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null) {
                foreach (RDFTarget target in shape.Targets) {
                    switch (target) {

                        //sh:targetClass
                        case RDFTargetClass targetClass:
                            result.AddRange(dataGraph.GetInstancesOfClass(target.TargetValue)
                                                     .OfType<RDFResource>());
                            break;

                        //sh:targetNode
                        case RDFTargetNode targetNode:
                            result.Add(target.TargetValue);
                            break;

                        //sh:targetSubjectsOf
                        case RDFTargetSubjectsOf targetSubjectsOf:
                            result.AddRange(dataGraph.SelectTriplesByPredicate(target.TargetValue)
                                                     .Select(x => x.Subject)
                                                     .OfType<RDFResource>());
                            break;

                        //sh:targetObjectsOf
                        case RDFTargetObjectsOf targetObjectsOf:
                            result.AddRange(dataGraph.SelectTriplesByPredicate(target.TargetValue)
                                                     .Select(x => x.Object)
                                                     .OfType<RDFResource>());
                            break;

                    }
                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the value nodes of the given focus node
        /// </summary>
        internal static List<RDFPatternMember> GetValueNodesOf(this RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode) {
            List<RDFPatternMember> result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null) {
                switch (shape) {

                    //sh:NodeShape
                    case RDFNodeShape nodeShape:
                        result.Add(focusNode);
                        break;

                    //sh:PropertyShape
                    case RDFPropertyShape propertyShape:
                        if (focusNode is RDFResource) { 
                            foreach (var triple in dataGraph.SelectTriplesBySubject((RDFResource)focusNode)
                                                            .SelectTriplesByPredicate(((RDFPropertyShape)shape).Path)) { 
                                result.Add(triple.Object);
                            }
                        }
                        break;

                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the direct (rdf:type) and indirect (rdfs:subClassOf) instances of the given class within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetInstancesOfClass(this RDFGraph dataGraph, RDFResource className, HashSet<Int64> visitContext = null) {
            var result = new List<RDFPatternMember>();
            if (className != null && dataGraph != null) {

                #region visitContext
                if (visitContext == null) {
                    visitContext = new HashSet<Int64>() { { className.PatternMemberID } };
                }
                else {
                    if (!visitContext.Contains(className.PatternMemberID)) {
                        visitContext.Add(className.PatternMemberID);
                    }
                    else {
                        return result;
                    }
                }
                #endregion

                //rdf:type
                foreach (var triple in dataGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                                                .SelectTriplesByObject(className))
                    result.Add(triple.Subject);

                //rdfs:subClassOf
                foreach (var triple in dataGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_CLASS_OF)
                                                .SelectTriplesByObject(className))
                    result.AddRange(dataGraph.GetInstancesOfClass((RDFResource)triple.Subject, visitContext));

            }
            return result;
        }
        #endregion

        #region Conversion
        internal static RDFShapesGraph FromRDFGraph(RDFGraph graph) {
            if (graph != null) {
                RDFShapesGraph result = new RDFShapesGraph(new RDFResource(graph.Context.ToString()));
                RDFSelectQueryResult shapesResult = GetShapeQuery().ApplyToGraph(graph);
                foreach (DataRow shapesRow in shapesResult.SelectResults.AsEnumerable()) {
                    RDFShape shape = ParseShapeType(shapesRow);
                    if (shape == null)
                        continue;

                    //Targets
                    ParseShapeTargets(shapesRow, shape);

                    //Attributes
                    ParseShapeAttributes(shapesRow, shape);

                    //Constraints
                    ParseShapeConstraints(shapesRow, graph, shape);

                    //Merge
                    MergeShape(result, shape);
                }
                return result;
            }
            return null;
        }
        private static RDFSelectQuery GetShapeQuery() {
            return
             new RDFSelectQuery()
                 .AddPatternGroup(new RDFPatternGroup("NODESHAPES")
                     //Definition
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE))
                     //Targets
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.TARGET_CLASS, new RDFVariable("TARGETCLASS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.TARGET_NODE, new RDFVariable("TARGETNODE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, new RDFVariable("TARGETSUBJECTSOF")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.TARGET_OBJECTS_OF, new RDFVariable("TARGETOBJECTSOF")).Optional())
                     //Attributes
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.SEVERITY_PROPERTY, new RDFVariable("SEVERITY")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.DEACTIVATED, new RDFVariable("DEACTIVATED")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MESSAGE, new RDFVariable("MESSAGE")).Optional())
                     //Constraints
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.AND, new RDFVariable("AND")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.CLASS, new RDFVariable("CLASS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.CLOSED, new RDFVariable("CLOSED")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.IGNORED_PROPERTIES, new RDFVariable("IGNOREDPROPERTIES")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.DATATYPE, new RDFVariable("DATATYPE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.DISJOINT, new RDFVariable("DISJOINT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.EQUALS, new RDFVariable("EQUALS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.HAS_VALUE, new RDFVariable("HASVALUE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.IN, new RDFVariable("IN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.LANGUAGE_IN, new RDFVariable("LANGUAGEIN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.LESS_THAN, new RDFVariable("LESSTHAN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS, new RDFVariable("LESSTHANOREQUALS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MAX_COUNT, new RDFVariable("MAXCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MAX_EXCLUSIVE, new RDFVariable("MAXEXCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MAX_INCLUSIVE, new RDFVariable("MAXINCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MAX_LENGTH, new RDFVariable("MAXLENGTH")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MIN_COUNT, new RDFVariable("MINCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MIN_EXCLUSIVE, new RDFVariable("MINEXCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MIN_INCLUSIVE, new RDFVariable("MININCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MIN_LENGTH, new RDFVariable("MINLENGTH")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.NODE, new RDFVariable("NODE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.NODE_KIND, new RDFVariable("NODEKIND")).Optional())
					 .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.OR, new RDFVariable("OR")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.PATTERN, new RDFVariable("PATTERN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.FLAGS, new RDFVariable("FLAGS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.PROPERTY, new RDFVariable("PROPERTY")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE, new RDFVariable("QUALIFIEDVALUESHAPE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT, new RDFVariable("QUALIFIEDMINCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT, new RDFVariable("QUALIFIEDMAXCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.UNIQUE_LANG, new RDFVariable("UNIQUELANG")).Optional())
                     .UnionWithNext()
                 )
                 .AddPatternGroup(new RDFPatternGroup("PROPERTYSHAPES")
                     //Definition
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE))
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.PATH, new RDFVariable("PATH")))
                     //Targets
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.TARGET_CLASS, new RDFVariable("TARGETCLASS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.TARGET_NODE, new RDFVariable("TARGETNODE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, new RDFVariable("TARGETSUBJECTSOF")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.TARGET_OBJECTS_OF, new RDFVariable("TARGETOBJECTSOF")).Optional())
                     //Attributes
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.SEVERITY_PROPERTY, new RDFVariable("SEVERITY")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.DEACTIVATED, new RDFVariable("DEACTIVATED")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MESSAGE, new RDFVariable("MESSAGE")).Optional())
                     //NonValidating
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.DESCRIPTION, new RDFVariable("DESCRIPTION")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.NAME, new RDFVariable("NAME")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.GROUP, new RDFVariable("GROUP")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.ORDER, new RDFVariable("ORDER")).Optional())
                     //Constraints
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.AND, new RDFVariable("AND")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.CLASS, new RDFVariable("CLASS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.CLOSED, new RDFVariable("CLOSED")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.IGNORED_PROPERTIES, new RDFVariable("IGNOREDPROPERTIES")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.DATATYPE, new RDFVariable("DATATYPE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.DISJOINT, new RDFVariable("DISJOINT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.EQUALS, new RDFVariable("EQUALS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.HAS_VALUE, new RDFVariable("HASVALUE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.IN, new RDFVariable("IN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.LANGUAGE_IN, new RDFVariable("LANGUAGEIN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.LESS_THAN, new RDFVariable("LESSTHAN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS, new RDFVariable("LESSTHANOREQUALS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MAX_COUNT, new RDFVariable("MAXCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MAX_EXCLUSIVE, new RDFVariable("MAXEXCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MAX_INCLUSIVE, new RDFVariable("MAXINCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MAX_LENGTH, new RDFVariable("MAXLENGTH")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MIN_COUNT, new RDFVariable("MINCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MIN_EXCLUSIVE, new RDFVariable("MINEXCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MIN_INCLUSIVE, new RDFVariable("MININCLUSIVE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MIN_LENGTH, new RDFVariable("MINLENGTH")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.NODE, new RDFVariable("NODE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.NODE_KIND, new RDFVariable("NODEKIND")).Optional())
					 .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.OR, new RDFVariable("OR")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.PATTERN, new RDFVariable("PATTERN")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.FLAGS, new RDFVariable("FLAGS")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.PROPERTY, new RDFVariable("PROPERTY")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE, new RDFVariable("QUALIFIEDVALUESHAPE")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT, new RDFVariable("QUALIFIEDMINCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT, new RDFVariable("QUALIFIEDMAXCOUNT")).Optional())
                     .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.UNIQUE_LANG, new RDFVariable("UNIQUELANG")).Optional())
                 );
        }
        private static RDFShape ParseShapeType(DataRow shapesRow) {
            RDFShape shape = null;

            //sh:NodeShape
            if (!shapesRow.IsNull("?NSHAPE")) {
                shape = new RDFNodeShape(new RDFResource(shapesRow.Field<string>("?NSHAPE")));
            }

            //sh:PropertyShape
            else if (!shapesRow.IsNull("?PSHAPE") && !shapesRow.IsNull("?PATH")) {
                shape = new RDFPropertyShape(new RDFResource(shapesRow.Field<string>("?PSHAPE")), new RDFResource(shapesRow.Field<string>("?PATH")));

                //sh:description
                if (!shapesRow.IsNull("?DESCRIPTION")) {
                    RDFPatternMember description = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?DESCRIPTION"));
                    if (description is RDFLiteral)
                        ((RDFPropertyShape)shape).AddDescription((RDFLiteral)description);
                }

                //sh:name
                if (!shapesRow.IsNull("?NAME")) {
                    RDFPatternMember name = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?NAME"));
                    if (name is RDFLiteral)
                        ((RDFPropertyShape)shape).AddName((RDFLiteral)name);
                }

                //sh:group
                if (!shapesRow.IsNull("?GROUP")) {
                    RDFPatternMember group = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?GROUP"));
                    if (group is RDFResource)
                        ((RDFPropertyShape)shape).SetGroup((RDFResource)group);
                }

                //sh:order
                if (!shapesRow.IsNull("?ORDER")) {
                    RDFPatternMember order = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?ORDER"));
                    if (order is RDFTypedLiteral && ((RDFTypedLiteral)order).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                        ((RDFPropertyShape)shape).SetOrder(int.Parse(((RDFTypedLiteral)order).Value));
                }
            }

            return shape;
        }
        private static void ParseShapeTargets(DataRow shapesRow, RDFShape shape) {

            //sh:targetClass
            if (!shapesRow.IsNull("?TARGETCLASS"))
                shape.AddTarget(new RDFTargetClass(new RDFResource(shapesRow.Field<string>("?TARGETCLASS"))));
            
            //sh:targetNode
            if (!shapesRow.IsNull("?TARGETNODE"))
                shape.AddTarget(new RDFTargetNode(new RDFResource(shapesRow.Field<string>("?TARGETNODE"))));
            
            //sh:targetSubjectsOf
            if (!shapesRow.IsNull("?TARGETSUBJECTSOF"))
                shape.AddTarget(new RDFTargetSubjectsOf(new RDFResource(shapesRow.Field<string>("?TARGETSUBJECTSOF"))));
            
            //sh:targetObjectsOf
            if (!shapesRow.IsNull("?TARGETOBJECTSOF"))
                shape.AddTarget(new RDFTargetObjectsOf(new RDFResource(shapesRow.Field<string>("?TARGETOBJECTSOF"))));

        }
        private static void ParseShapeAttributes(DataRow shapesRow, RDFShape shape) {

            //sh:severity
            if (!shapesRow.IsNull("?SEVERITY")) {
                if (shapesRow.Field<string>("?SEVERITY").Equals(RDFVocabulary.SHACL.INFO.ToString()))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
                else if (shapesRow.Field<string>("?SEVERITY").Equals(RDFVocabulary.SHACL.WARNING.ToString()))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);
            }

            //sh:deactivated
            if (!shapesRow.IsNull("?DEACTIVATED")) {
                RDFPatternMember deactivated = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?DEACTIVATED"));
                if (deactivated is RDFTypedLiteral
                        && ((RDFTypedLiteral)deactivated).HasBooleanDatatype()
                            && Boolean.Parse(((RDFTypedLiteral)deactivated).Value))
                    shape.Deactivate();
            }

            //sh:message
            if (!shapesRow.IsNull("?MESSAGE")) {
                RDFPatternMember message = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MESSAGE"));
                if (message is RDFLiteral)
                    shape.AddMessage((RDFLiteral)message);
            }

        }
        private static void ParseShapeConstraints(DataRow shapesRow, RDFGraph graph, RDFShape shape) {

            //sh:and
            if (!shapesRow.IsNull("?AND")) {
                RDFPatternMember reifSubj = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?AND"));
                if (reifSubj is RDFResource) {
                    RDFAndConstraint andConstraint = new RDFAndConstraint();
                    RDFCollection andColl = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)reifSubj, RDFModelEnums.RDFTripleFlavors.SPO);
                    andColl.Items.ForEach(item => {
                        andConstraint.AddShape((RDFResource)item);
                    });
                    shape.AddConstraint(andConstraint);
                }
            }

            //sh:class
            if (!shapesRow.IsNull("?CLASS")) {
                RDFPatternMember cls = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?CLASS"));
                if (cls is RDFResource)
                    shape.AddConstraint(new RDFClassConstraint((RDFResource)cls));
            }

            //sh:closed
            if (!shapesRow.IsNull("?CLOSED")) {
                RDFPatternMember closed = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?CLOSED"));
                if (closed is RDFTypedLiteral
                        && ((RDFTypedLiteral)closed).HasBooleanDatatype()) {
                    RDFClosedConstraint closedConstraint = new RDFClosedConstraint(Boolean.Parse(((RDFTypedLiteral)closed).Value));

                    //sh:ignoredProperties
                    RDFPatternMember reifSubj = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?IGNOREDPROPERTIES"));
                    if (reifSubj is RDFResource) {
                        RDFCollection ignoredPropsColl = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)reifSubj, RDFModelEnums.RDFTripleFlavors.SPO);
                        ignoredPropsColl.Items.ForEach(item => {
                            closedConstraint.AddIgnoredProperty((RDFResource)item);
                        });
                    }

                    shape.AddConstraint(closedConstraint);
                }
            }

            //sh:datatype
            if (!shapesRow.IsNull("?DATATYPE")) {
                RDFPatternMember datatype = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?DATATYPE"));
                if (datatype is RDFResource)
                    shape.AddConstraint(new RDFDatatypeConstraint(RDFModelUtilities.GetDatatypeFromString(datatype.ToString())));
            }

            //sh:disjoint
            if (!shapesRow.IsNull("?DISJOINT")) {
                RDFPatternMember disjpred = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?DISJOINT"));
                if (disjpred is RDFResource)
                    shape.AddConstraint(new RDFDisjointConstraint((RDFResource)disjpred));
            }

            //sh:equals
            if (!shapesRow.IsNull("?EQUALS")) {
                RDFPatternMember eqpred = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?EQUALS"));
                if (eqpred is RDFResource)
                    shape.AddConstraint(new RDFEqualsConstraint((RDFResource)eqpred));
            }

            //sh:hasValue
            if (!shapesRow.IsNull("?HASVALUE")) {
                RDFPatternMember value = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?HASVALUE"));
                if (value is RDFResource)
                    shape.AddConstraint(new RDFHasValueConstraint((RDFResource)value));
                else if (value is RDFLiteral)
                    shape.AddConstraint(new RDFHasValueConstraint((RDFLiteral)value));
            }

            //sh:in
            if (!shapesRow.IsNull("?IN")) {
                RDFPatternMember reifSubj = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?IN"));
                if (reifSubj is RDFResource) {
                    RDFModelEnums.RDFTripleFlavors inCollFlavor = RDFModelUtilities.DetectCollectionFlavorFromGraph(graph, (RDFResource)reifSubj);
                    RDFCollection inColl = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)reifSubj, inCollFlavor);
                    RDFInConstraint inConstraint = new RDFInConstraint(inColl.ItemType);
                    inColl.Items.ForEach(item => {
                        if (inColl.ItemType == RDFModelEnums.RDFItemTypes.Literal)
                            inConstraint.AddValue((RDFLiteral)item);
                        else
                            inConstraint.AddValue((RDFResource)item);
                    });
                    shape.AddConstraint(inConstraint);
                }
            }

            //sh:languageIn
            if (!shapesRow.IsNull("?LANGUAGEIN")) {
                RDFPatternMember reifSubj = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?LANGUAGEIN"));
                if (reifSubj is RDFResource) {
                    RDFCollection langTagsColl = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)reifSubj, RDFModelEnums.RDFTripleFlavors.SPL);
                    shape.AddConstraint(new RDFLanguageInConstraint(langTagsColl.Select(x => x.ToString()).ToList()));
                }
            }

            //sh:lessThan
            if (!shapesRow.IsNull("?LESSTHAN")) {
                RDFPatternMember ltpred = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?LESSTHAN"));
                if (ltpred is RDFResource)
                    shape.AddConstraint(new RDFLessThanConstraint((RDFResource)ltpred));
            }

            //sh:lessThanOrEquals
            if (!shapesRow.IsNull("?LESSTHANOREQUALS")) {
                RDFPatternMember lteqpred = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?LESSTHANOREQUALS"));
                if (lteqpred is RDFResource)
                    shape.AddConstraint(new RDFLessThanOrEqualsConstraint((RDFResource)lteqpred));
            }

            //sh:maxCount
            if (!shapesRow.IsNull("?MAXCOUNT")) {
                RDFPatternMember maxCount = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MAXCOUNT"));
                if (maxCount is RDFTypedLiteral && ((RDFTypedLiteral)maxCount).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMaxCountConstraint(int.Parse(((RDFTypedLiteral)maxCount).Value)));
            }

            //sh:maxExclusive
            if (!shapesRow.IsNull("?MAXEXCLUSIVE")) {
                RDFPatternMember value = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MAXEXCLUSIVE"));
                if (value is RDFResource)
                    shape.AddConstraint(new RDFMaxExclusiveConstraint((RDFResource)value));
                else if (value is RDFLiteral)
                    shape.AddConstraint(new RDFMaxExclusiveConstraint((RDFLiteral)value));
            }

            //sh:maxInclusive
            if (!shapesRow.IsNull("?MAXINCLUSIVE")) {
                RDFPatternMember value = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MAXINCLUSIVE"));
                if (value is RDFResource)
                    shape.AddConstraint(new RDFMaxInclusiveConstraint((RDFResource)value));
                else if (value is RDFLiteral)
                    shape.AddConstraint(new RDFMaxInclusiveConstraint((RDFLiteral)value));
            }

            //sh:maxLength
            if (!shapesRow.IsNull("?MAXLENGTH")) {
                RDFPatternMember maxLength = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MAXLENGTH"));
                if (maxLength is RDFTypedLiteral && ((RDFTypedLiteral)maxLength).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMaxLengthConstraint(int.Parse(((RDFTypedLiteral)maxLength).Value)));
            }

            //sh:minCount
            if (!shapesRow.IsNull("?MINCOUNT")) {
                RDFPatternMember minCount = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MINCOUNT"));
                if (minCount is RDFTypedLiteral && ((RDFTypedLiteral)minCount).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMinCountConstraint(int.Parse(((RDFTypedLiteral)minCount).Value)));
            }

            //sh:minExclusive
            if (!shapesRow.IsNull("?MINEXCLUSIVE")) {
                RDFPatternMember value = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MINEXCLUSIVE"));
                if (value is RDFResource)
                    shape.AddConstraint(new RDFMinExclusiveConstraint((RDFResource)value));
                else if (value is RDFLiteral)
                    shape.AddConstraint(new RDFMinExclusiveConstraint((RDFLiteral)value));
            }

            //sh:minInclusive
            if (!shapesRow.IsNull("?MININCLUSIVE")) {
                RDFPatternMember value = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MININCLUSIVE"));
                if (value is RDFResource)
                    shape.AddConstraint(new RDFMinInclusiveConstraint((RDFResource)value));
                else if (value is RDFLiteral)
                    shape.AddConstraint(new RDFMinInclusiveConstraint((RDFLiteral)value));
            }

            //sh:minLength
            if (!shapesRow.IsNull("?MINLENGTH")) {
                RDFPatternMember minLength = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MINLENGTH"));
                if (minLength is RDFTypedLiteral && ((RDFTypedLiteral)minLength).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMinLengthConstraint(int.Parse(((RDFTypedLiteral)minLength).Value)));
            }

            //sh:node
            if (!shapesRow.IsNull("?NODE")) {
                RDFPatternMember nodeshapeUri = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?NODE"));
                if (nodeshapeUri is RDFResource)
                    shape.AddConstraint(new RDFNodeConstraint((RDFResource)nodeshapeUri));
            }

            //sh:nodeKind
            if (!shapesRow.IsNull("?NODEKIND")) {
                RDFPatternMember nodeKind = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?NODEKIND"));
                if (nodeKind is RDFResource) {
                    if (nodeKind.Equals(RDFVocabulary.SHACL.BLANK_NODE))
                        shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNode));
                    else if (nodeKind.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI))
                        shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI));
                    else if (nodeKind.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL))
                        shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral));
                    else if (nodeKind.Equals(RDFVocabulary.SHACL.IRI))
                        shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRI));
                    else if (nodeKind.Equals(RDFVocabulary.SHACL.IRI_OR_LITERAL))
                        shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRIOrLiteral));
                    else if (nodeKind.Equals(RDFVocabulary.SHACL.LITERAL))
                        shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.Literal));
                }
            }

            //sh:or
            if (!shapesRow.IsNull("?OR")) {
                RDFPatternMember reifSubj = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?OR"));
                if (reifSubj is RDFResource) {
                    RDFOrConstraint orConstraint = new RDFOrConstraint();
                    RDFCollection orColl = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)reifSubj, RDFModelEnums.RDFTripleFlavors.SPO);
                    orColl.Items.ForEach(item => {
                        orConstraint.AddShape((RDFResource)item);
                    });
                    shape.AddConstraint(orConstraint);
                }
            }

            //sh:pattern
            if (!shapesRow.IsNull("?PATTERN")) {
                RDFPatternMember regexPattern = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?PATTERN"));
                if (regexPattern is RDFTypedLiteral && ((RDFTypedLiteral)regexPattern).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)) {
                    //sh:flags
                    RegexOptions regexOptions = RegexOptions.None;
                    if (!shapesRow.IsNull("?FLAGS")) {
                        RDFPatternMember regexFlags = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?FLAGS"));
                        if (regexFlags is RDFTypedLiteral && ((RDFTypedLiteral)regexFlags).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)) {
                            if (((RDFTypedLiteral)regexFlags).Value.Contains("i"))
                                regexOptions |= RegexOptions.IgnoreCase;
                            if (((RDFTypedLiteral)regexFlags).Value.Contains("s"))
                                regexOptions |= RegexOptions.Singleline;
                            if (((RDFTypedLiteral)regexFlags).Value.Contains("m"))
                                regexOptions |= RegexOptions.Multiline;
                            if (((RDFTypedLiteral)regexFlags).Value.Contains("x"))
                                regexOptions |= RegexOptions.IgnorePatternWhitespace;
                        }
                    }
                    shape.AddConstraint(new RDFPatternConstraint(new Regex(((RDFTypedLiteral)regexPattern).Value, regexOptions)));
                }
            }

            //sh:property
            if (!shapesRow.IsNull("?PROPERTY")) {
                RDFPatternMember propertyshapeUri = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?PROPERTY"));
                if (propertyshapeUri is RDFResource)
                    shape.AddConstraint(new RDFPropertyConstraint((RDFResource)propertyshapeUri));
            }

            //sh:qualifiedValueShape
            if (!shapesRow.IsNull("?QUALIFIEDVALUESHAPE")) {
                RDFPatternMember qualifiedValueShapeUri = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?QUALIFIEDVALUESHAPE"));
                if (qualifiedValueShapeUri is RDFResource) {
                    //sh:qualifiedMinCount
                    int? qualifiedMinCountValue = null;
                    if (!shapesRow.IsNull("?QUALIFIEDMINCOUNT")) {
                        RDFPatternMember qualifiedMinCount = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?QUALIFIEDMINCOUNT"));
                        if (qualifiedMinCount is RDFTypedLiteral && ((RDFTypedLiteral)qualifiedMinCount).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                            qualifiedMinCountValue = int.Parse(((RDFTypedLiteral)qualifiedMinCount).Value);
                    }
                    //sh:qualifiedMaxCount
                    int? qualifiedMaxCountValue = null;
                    if (!shapesRow.IsNull("?QUALIFIEDMAXCOUNT")) {
                        RDFPatternMember qualifiedMaxCount = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?QUALIFIEDMAXCOUNT"));
                        if (qualifiedMaxCount is RDFTypedLiteral && ((RDFTypedLiteral)qualifiedMaxCount).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                            qualifiedMaxCountValue = int.Parse(((RDFTypedLiteral)qualifiedMaxCount).Value);
                    }
                    shape.AddConstraint(new RDFQualifiedValueShapeConstraint((RDFResource)qualifiedValueShapeUri, qualifiedMinCountValue, qualifiedMaxCountValue));
                }
            }

            //sh:uniqueLang
            if (!shapesRow.IsNull("?UNIQUELANG")) {
                RDFPatternMember uniqueLang = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?UNIQUELANG"));
                if (uniqueLang is RDFTypedLiteral
                        && ((RDFTypedLiteral)uniqueLang).HasBooleanDatatype())
                    shape.AddConstraint(new RDFUniqueLangConstraint(Boolean.Parse(((RDFTypedLiteral)uniqueLang).Value)));
            }

        }
        private static void MergeShape(RDFShapesGraph result, RDFShape shape) {
            RDFShape existingShape = result.SelectShape(shape.ToString());
            if (existingShape == null) {
                result.AddShape(shape);
            }
            else {
                existingShape.Targets.AddRange(shape.Targets);
                existingShape.Constraints.AddRange(shape.Constraints);
                existingShape.Messages.AddRange(shape.Messages);
                if (existingShape is RDFPropertyShape && shape is RDFPropertyShape) {
                    ((RDFPropertyShape)existingShape).Descriptions.AddRange(((RDFPropertyShape)shape).Descriptions);
                    ((RDFPropertyShape)existingShape).Names.AddRange(((RDFPropertyShape)shape).Names);
                }
                result.RemoveShape(existingShape);
                result.AddShape(existingShape);
            }
        }
        #endregion

    }
}