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

        #region Methods

        #region Modeling
        /// <summary>
        /// Gets the focus nodes of the given shape within the given data graph
        /// </summary>
        internal static List<RDFResource> GetFocusNodesOf(this RDFGraph dataGraph,
                                                               RDFShape shape) {
            var result = new List<RDFResource>();
            if (shape != null && dataGraph != null) {
                foreach (var target in shape.Targets) {
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
        /// Gets the value nodes of the given shape within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetValueNodesOf(this RDFGraph dataGraph,
                                                               RDFShape shape,
                                                               RDFResource focusNode) {
            var result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null) {
                switch (shape) {

                    //sh:NodeShape
                    case RDFNodeShape nodeShape:
                        result.Add(focusNode);
                        break;

                    //sh:PropertyShape
                    case RDFPropertyShape propertyShape:
                        foreach (var triple in dataGraph.SelectTriplesBySubject(focusNode)
                                                        .SelectTriplesByPredicate(((RDFPropertyShape)shape).Path))
                            result.Add(triple.Object);
                        break;

                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the direct (rdf:type) and indirect (rdfs:subClassOf) instances of the given class within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetInstancesOfClass(this RDFGraph dataGraph,
                                                                   RDFResource className, 
                                                                   HashSet<Int64> visitContext = null) {
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

        /// <summary>
        /// Checks if the given language tag is used within the given collection of literals
        /// </summary>
        internal static Boolean CheckLanguageTagInUse(List<RDFLiteral> literals, 
                                                      String languageTag) {
            return literals.Any(lit => lit is RDFPlainLiteral
                                            && ((RDFPlainLiteral)lit).Language.Equals(languageTag, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Gets a shapes graph representation of the given graph
        /// </summary>
        internal static RDFShapesGraph FromRDFGraph(RDFGraph graph) {
            if (graph != null) {
                RDFShapesGraph result = new RDFShapesGraph(new RDFResource(graph.Context.ToString()));
                RDFSelectQueryResult shapesResult = GetShapeQuery().ApplyToGraph(graph);
                foreach (DataRow shapesRow in shapesResult.SelectResults.AsEnumerable()) {
                    RDFShape shape = ParseShapeType(shapesRow, shapesResult);
                    if (shape == null)
                        continue;

                    //Targets
                    ParseShapeTargets(shapesRow, shapesResult, shape);

                    //Attributes
                    ParseShapeAttributes(shapesRow, shapesResult, shape);

                    //Constraints
                    ParseShapeConstraints(shapesRow, shapesResult, graph, shape);

                    result.AddShape(shape);
                }
                return result;
            }
            return null;
        }
        private static RDFSelectQuery GetShapeQuery()
        {
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
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.CLASS, new RDFVariable("CLASS")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.DATATYPE, new RDFVariable("DATATYPE")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.LANGUAGE_IN, new RDFVariable("LANGUAGEIN")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MAX_LENGTH, new RDFVariable("MAXLENGTH")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.MIN_LENGTH, new RDFVariable("MINLENGTH")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.NODE_KIND, new RDFVariable("NODEKIND")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.PATTERN, new RDFVariable("PATTERN")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("NSHAPE"), RDFVocabulary.SHACL.FLAGS, new RDFVariable("FLAGS")).Optional())
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
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.CLASS, new RDFVariable("CLASS")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.DATATYPE, new RDFVariable("DATATYPE")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.LANGUAGE_IN, new RDFVariable("LANGUAGEIN")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MAX_LENGTH, new RDFVariable("MAXLENGTH")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.MIN_LENGTH, new RDFVariable("MINLENGTH")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.NODE_KIND, new RDFVariable("NODEKIND")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.PATTERN, new RDFVariable("PATTERN")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("PSHAPE"), RDFVocabulary.SHACL.FLAGS, new RDFVariable("FLAGS")).Optional())
                );
        }
        private static RDFShape ParseShapeType(DataRow shapesRow, RDFSelectQueryResult shapesResult) {
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
                    if (order is RDFTypedLiteral && ((RDFTypedLiteral)order).Datatype.Equals(RDFVocabulary.XSD.INTEGER))
                        ((RDFPropertyShape)shape).SetOrder(int.Parse(((RDFTypedLiteral)order).Value));
                }
            }

            return shape;
        }
        private static void ParseShapeTargets(DataRow shapesRow, RDFSelectQueryResult shapesResult, RDFShape shape) {

            //sh:targetClass
            if (!shapesRow.IsNull("?TARGETCLASS")) { 
                shape.AddTarget(new RDFTargetClass(new RDFResource(shapesRow.Field<string>("?TARGETCLASS"))));
            }

            //sh:targetNode
            if (!shapesRow.IsNull("?TARGETNODE")) { 
                shape.AddTarget(new RDFTargetNode(new RDFResource(shapesRow.Field<string>("?TARGETNODE"))));
            }

            //sh:targetSubjectsOf
            if (!shapesRow.IsNull("?TARGETSUBJECTSOF")) { 
                shape.AddTarget(new RDFTargetSubjectsOf(new RDFResource(shapesRow.Field<string>("?TARGETSUBJECTSOF"))));
            }

            //sh:targetObjectsOf
            if (!shapesRow.IsNull("?TARGETOBJECTSOF")) { 
                shape.AddTarget(new RDFTargetObjectsOf(new RDFResource(shapesRow.Field<string>("?TARGETOBJECTSOF"))));
            }

        }
        private static void ParseShapeAttributes(DataRow shapesRow, RDFSelectQueryResult shapesResult, RDFShape shape) {

            //sh:severity
            if (!shapesRow.IsNull("?SEVERITY")) {
                if (shapesRow.Field<string>("?SEVERITY").Equals(RDFVocabulary.SHACL.INFO.ToString()))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
                else if (shapesRow.Field<string>("?SEVERITY").Equals(RDFVocabulary.SHACL.WARNING.ToString()))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);
            }

            //sh:deactivated
            if (!shapesRow.IsNull("?DEACTIVATED")) {
                if (shapesRow.Field<bool>("?DEACTIVATED"))
                    shape.Deactivate();
            }

            //sh:message
            if (!shapesRow.IsNull("?MESSAGE")) {
                RDFPatternMember message = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MESSAGE"));
                if (message is RDFLiteral)
                    shape.AddMessage((RDFLiteral)message);
            }

        }
        private static void ParseShapeConstraints(DataRow shapesRow, RDFSelectQueryResult shapesResult, RDFGraph graph, RDFShape shape) {

            //sh:class
            if (!shapesRow.IsNull("?CLASS")) { 
                shape.AddConstraint(new RDFClassConstraint(new RDFResource(shapesRow.Field<string>("?CLASS"))));
            }

            //sh:datatype
            if (!shapesRow.IsNull("?DATATYPE")) { 
                shape.AddConstraint(new RDFDatatypeConstraint(RDFModelUtilities.GetDatatypeFromString(shapesRow.Field<string>("?DATATYPE"))));
            }

            //sh:languageIn
            if (!shapesRow.IsNull("?LANGUAGEIN")) {
                RDFPatternMember reifSubj = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?LANGUAGEIN"));
                if (reifSubj is RDFResource) {
                    RDFCollection langTagsColl = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)reifSubj, RDFModelEnums.RDFTripleFlavors.SPL);
                    shape.AddConstraint(new RDFLanguageInConstraint(langTagsColl.Select(x => x.ToString()).ToList()));
                }
            }

            //sh:maxLength
            if (!shapesRow.IsNull("?MAXLENGTH")) {
                RDFPatternMember maxLength = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MAXLENGTH"));
                if (maxLength is RDFTypedLiteral && ((RDFTypedLiteral)maxLength).Datatype.Equals(RDFVocabulary.XSD.INTEGER))
                    shape.AddConstraint(new RDFMaxLengthConstraint(int.Parse(((RDFTypedLiteral)maxLength).Value)));
            }

            //sh:minLength
            if (!shapesRow.IsNull("?MINLENGTH")) {
                RDFPatternMember minLength = RDFQueryUtilities.ParseRDFPatternMember(shapesRow.Field<string>("?MINLENGTH"));
                if (minLength is RDFTypedLiteral && ((RDFTypedLiteral)minLength).Datatype.Equals(RDFVocabulary.XSD.INTEGER))
                    shape.AddConstraint(new RDFMinLengthConstraint(int.Parse(((RDFTypedLiteral)minLength).Value)));
            }

            //sh:nodeKind
            if (!shapesRow.IsNull("?NODEKIND")) {
                if (shapesRow.Field<string>("?NODEKIND").Equals(RDFVocabulary.SHACL.BLANK_NODE.ToString()))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNode));
                else if (shapesRow.Field<string>("?NODEKIND").Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI.ToString()))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI));
                else if (shapesRow.Field<string>("?NODEKIND").Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL.ToString()))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral));
                else if (shapesRow.Field<string>("?NODEKIND").Equals(RDFVocabulary.SHACL.IRI.ToString()))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRI));
                else if (shapesRow.Field<string>("?NODEKIND").Equals(RDFVocabulary.SHACL.IRI_OR_LITERAL.ToString()))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRIOrLiteral));
                else if (shapesRow.Field<string>("?NODEKIND").Equals(RDFVocabulary.SHACL.LITERAL.ToString()))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.Literal));
            }

            //sh:pattern
            if (!shapesRow.IsNull("?PATTERN")) {
                RegexOptions regexOptions = RegexOptions.None;
                if (!shapesRow.IsNull("?FLAGS")) {
                    string regexFlags = shapesRow.Field<string>("?FLAGS");
                    if (regexFlags.Contains("i"))
                        regexOptions |= RegexOptions.IgnoreCase;
                    if (regexFlags.Contains("s"))
                        regexOptions |= RegexOptions.Singleline;
                    if (regexFlags.Contains("m"))
                        regexOptions |= RegexOptions.Multiline;
                    if (regexFlags.Contains("x"))
                        regexOptions |= RegexOptions.IgnorePatternWhitespace;
                }
                shape.AddConstraint(new RDFPatternConstraint(new Regex(shapesRow.Field<string>("?PATTERN"), regexOptions)));
            }

        }
        #endregion

        #endregion

    }
}