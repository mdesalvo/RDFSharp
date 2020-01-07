/*
   Copyright 2015-2019 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFShapesGraph represents a SHACL shapes graph definition
    /// </summary>
    public class RDFShapesGraph: RDFResource, IEnumerable<RDFShape> {

        #region Properties
        /// <summary>
        /// Count of the SHACL shapes composing this SHACL shapes graph
        /// </summary>
        public Int64 ShapesCount {
            get { return this.Shapes.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the SHACL shapes of this SHACL shapes graph for iteration
        /// </summary>
        public IEnumerator<RDFShape> ShapesEnumerator {
            get { return this.Shapes.Values.GetEnumerator(); }
        }

        /// <summary>
        /// SHACL shapes contained in this SHACL shapes graph
        /// </summary>
        internal Dictionary<Int64, RDFShape> Shapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build SHACL shapes graph with the given name
        /// </summary>
        public RDFShapesGraph(RDFResource shapesGraphName): base(shapesGraphName.ToString()) {
            this.Shapes = new Dictionary<Int64, RDFShape>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the SHACL shapes of this SHACL shapes graph
        /// </summary>
        IEnumerator<RDFShape> IEnumerable<RDFShape>.GetEnumerator() {
            return this.ShapesEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the SHACL shapes of this SHACL shapes graph
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.ShapesEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given SHACL shape to this SHACL shapes graph
        /// </summary>
        public RDFShapesGraph AddShape(RDFShape shape) {
            if (shape != null) {
                if (!this.Shapes.ContainsKey(shape.PatternMemberID)) {
                    this.Shapes.Add(shape.PatternMemberID, shape);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given SHACL shape from this SHACL shapes graph
        /// </summary>
        public RDFShapesGraph RemoveShape(RDFShape shape) {
            if (shape != null) {
                if (this.Shapes.ContainsKey(shape.PatternMemberID)) {
                    this.Shapes.Remove(shape.PatternMemberID);
                }
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if this shapes graph contains the given SHACL shape
        /// </summary>
        public Boolean ContainsShape(RDFShape shape) {
            return (shape != null && this.Shapes.ContainsKey(shape.PatternMemberID));
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection SHACL shapes graph from this SHACL shapes graph and a given one
        /// </summary>
        public RDFShapesGraph IntersectWith(RDFShapesGraph shapesGraph) {
            var result = new RDFShapesGraph(new RDFResource());
            if (shapesGraph != null) {

                //Add intersection shapes
                foreach (var s in this) {
                    if (shapesGraph.ContainsShape(s)) {
                        result.AddShape(s);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Builds a new union SHACL shapes graph from this SHACL shapes graph and the given one
        /// </summary>
        public RDFShapesGraph UnionWith(RDFShapesGraph shapesGraph) {
            var result = new RDFShapesGraph(new RDFResource());

            //Add shapes from this shapes graph
            foreach (var s in this) {
                result.AddShape(s);
            }

            //Manage the given shapes graph
            if (shapesGraph != null) {

                //Add triples from the given graph
                foreach (var s in shapesGraph) {
                    result.AddShape(s);
                }

            }

            return result;
        }

        /// <summary>
        /// Builds a new difference SHACL shapes graph from this SHACL shapes graph and a given one
        /// </summary>
        public RDFShapesGraph DifferenceWith(RDFShapesGraph shapesGraph) {
            var result = new RDFShapesGraph(new RDFResource());
            if (shapesGraph != null) {

                //Add difference shapes
                foreach (var s in this) {
                    if (!shapesGraph.ContainsShape(s)) {
                        result.AddShape(s);
                    }
                }

            }
            else {

                //Add shapes from this shape
                foreach (var s in this) {
                    result.AddShape(s);
                }

            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this shapes graph
        /// </summary>
        public RDFGraph ToRDFGraph() {
            var result = new RDFGraph();

            foreach (var shape in this)
                result = result.UnionWith(shape.ToRDFGraph());

            result.SetContext(this.URI);
            return result;
        }
        #endregion

        #endregion

    }
}