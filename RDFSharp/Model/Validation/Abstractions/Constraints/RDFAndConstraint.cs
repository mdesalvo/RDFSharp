﻿/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Collections.Generic;
using RDFSharp.Query;

namespace RDFSharp.Model;

/// <summary>
/// RDFAndConstraint represents a SHACL constraint requiring all the given shapes for a given RDF term
/// </summary>
public sealed class RDFAndConstraint : RDFConstraint
{
    #region Properties
    /// <summary>
    /// Shapes required for the given RDF term
    /// </summary>
    internal Dictionary<long, RDFResource> AndShapes { get; set; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds an and constraint
    /// </summary>
    public RDFAndConstraint()
        => AndShapes = [];
    #endregion

    #region Methods
    /// <summary>
    /// Adds the given shape to the required shapes of this constraint
    /// </summary>
    public RDFAndConstraint AddShape(RDFResource shapeUri)
    {
        if (shapeUri != null)
            AndShapes.TryAdd(shapeUri.PatternMemberID, shapeUri);
        return this;
    }

    /// <summary>
    /// Evaluates this constraint against the given data graph
    /// </summary>
    internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
    {
        RDFValidationReport report = new RDFValidationReport();
        RDFPropertyShape pShape = shape as RDFPropertyShape;

        //Search for given and shapes
        List<RDFShape> andShapes = [];
        foreach (RDFResource andShapeUri in AndShapes.Values)
        {
            RDFShape andShape = shapesGraph.SelectShape(andShapeUri.ToString());
            if (andShape != null)
                andShapes.Add(andShape);
        }

        //In case no shape messages have been provided, this constraint emits a default one (for usability)
        List<RDFLiteral> shapeMessages = [.. shape.Messages];
        if (shapeMessages.Count == 0)
            shapeMessages.Add(new RDFPlainLiteral("Value does not have all the shapes in sh:and enumeration"));

        #region Evaluation
        foreach (RDFPatternMember valueNode in valueNodes)
        {
            bool valueNodeConforms = true;

            //Iterate required shapes, breaking at the first unsatisfied one
            foreach (RDFShape andShape in andShapes)
            {
                RDFValidationReport andShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, andShape, [valueNode]);
                if (!andShapeReport.Conforms)
                {
                    valueNodeConforms = false;
                    //No need to evaluate remaining shapes, since value has broken sh:and enumeration
                    break;
                }
            }

            if (!valueNodeConforms)
                report.AddResult(new RDFValidationResult(shape,
                    RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT,
                    focusNode,
                    pShape?.Path,
                    valueNode,
                    shapeMessages,
                    shape.Severity));
        }
        #endregion

        return report;
    }

    /// <summary>
    /// Gets a graph representation of this constraint
    /// </summary>
    internal override RDFGraph ToRDFGraph(RDFShape shape)
    {
        RDFGraph result = new RDFGraph();
        if (shape != null)
        {
            //Get collection from andShapes
            RDFCollection andShapes = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource) { InternalReificationSubject = this };
            foreach (RDFResource andShape in AndShapes.Values)
                andShapes.AddItem(andShape);
            result.AddCollection(andShapes);

            //sh:and
            result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.AND, andShapes.ReificationSubject));
        }
        return result;
    }
    #endregion
}