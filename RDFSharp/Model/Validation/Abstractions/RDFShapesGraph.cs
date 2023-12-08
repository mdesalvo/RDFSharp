/*
   Copyright 2012-2024 Marco De Salvo

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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFShapesGraph represents a SHACL shapes graph definition
    /// </summary>
    public class RDFShapesGraph : RDFResource, IEnumerable<RDFShape>
    {
        #region Properties
        /// <summary>
        /// Count of the shapes composing this shapes graph
        /// </summary>
        public long ShapesCount => Shapes.Count;

        /// <summary>
        /// Gets the enumerator on the shapes of this shapes graph for iteration
        /// </summary>
        public IEnumerator<RDFShape> ShapesEnumerator => Shapes.Values.GetEnumerator();

        /// <summary>
        /// SHACL shapes contained in this shapes graph
        /// </summary>
        internal Dictionary<long, RDFShape> Shapes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named shapes graph
        /// </summary>
        public RDFShapesGraph(RDFResource shapesGraphName) : base(shapesGraphName.ToString())
            => Shapes = new Dictionary<long, RDFShape>();

        /// <summary>
        /// Default-ctor to build a blank shapes graph
        /// </summary>
        public RDFShapesGraph() : this(new RDFResource()) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the shapes of this shapes graph
        /// </summary>
        IEnumerator<RDFShape> IEnumerable<RDFShape>.GetEnumerator() => ShapesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the shapes of this shapes graph
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => ShapesEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given shape to this shapes graph
        /// </summary>
        public RDFShapesGraph AddShape(RDFShape shape)
        {
            if (shape != null && !Shapes.ContainsKey(shape.PatternMemberID))
                Shapes.Add(shape.PatternMemberID, shape);
            return this;
        }

        /// <summary>
        /// Merges the shapes of the given shapes graph to this shapes graph
        /// </summary>
        public RDFShapesGraph MergeShapes(RDFShapesGraph shapesGraph)
        {
            if (shapesGraph != null)
            {
                foreach (RDFShape shape in shapesGraph)
                    if (!Shapes.ContainsKey(shape.PatternMemberID))
                        Shapes.Add(shape.PatternMemberID, shape);
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given shape from this shapes graph
        /// </summary>
        public RDFShapesGraph RemoveShape(RDFShape shape)
        {
            if (shape != null)
                Shapes.Remove(shape.PatternMemberID);
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the shape represented by the given string from this shapes graph
        /// </summary>
        public RDFShape SelectShape(string shapeName)
        {
            if (shapeName != null)
            {
                long shapeID = RDFModelUtilities.CreateHash(shapeName);
                if (Shapes.ContainsKey(shapeID))
                    return Shapes[shapeID];
            }
            return null;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this shapes graph
        /// </summary>
        public RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();

            foreach (RDFShape shape in this)
                result = result.UnionWith(shape.ToRDFGraph());

            result.SetContext(URI);
            return result;
        }

        /// <summary>
        /// Gets an asynchronous graph representation of this shapes graph
        /// </summary>
        public Task<RDFAsyncGraph> ToRDFGraphAsync()
            => Task.Run(() => new RDFAsyncGraph(ToRDFGraph()));

        /// <summary>
        /// Gets a shapes graph representation of the given graph
        /// </summary>
        public static RDFShapesGraph FromRDFGraph(RDFGraph graph)
            => RDFValidationHelper.FromRDFGraph(graph);

        /// <summary>
        /// Gets a shapes graph representation of the given asynchronous graph
        /// </summary>
        public static Task<RDFShapesGraph> FromRDFGraphAsync(RDFAsyncGraph asyncGraph)
            => Task.Run(() => FromRDFGraph(asyncGraph?.WrappedGraph));
        #endregion

        #endregion
    }
}