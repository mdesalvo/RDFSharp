/*
   Copyright 2012-2026 Marco De Salvo

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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTargetSPARQL represents a SHACL-SPARQL target of type "sh:SPARQLTarget" within a shape:
    /// the focus nodes are computed by running a self-contained SELECT query (carrying its own PREFIX
    /// prologue) over the data graph and collecting the resources bound to the mandatory "?this" variable.
    /// </summary>
    public sealed class RDFTargetSPARQL : RDFTarget
    {
        #region Properties
        /// <summary>
        /// The SELECT query whose "?this" projection yields the focus nodes of the shape (sh:select)
        /// </summary>
        public RDFSelectQuery SelectQuery { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a SPARQL target driven by the given SELECT query (its "?this" projection yields the focus nodes)
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        public RDFTargetSPARQL(RDFSelectQuery selectQuery)
        {
            //The carried SELECT query is mandatory: without it there is no way to compute focus nodes
            SelectQuery = selectQuery ?? throw new RDFModelException("Cannot create RDFTargetSPARQL because given \"selectQuery\" parameter is null.");

            //The sh:SPARQLTarget node is an anonymous individual: it carries the query and links back to the shape
            TargetValue = new RDFResource();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph representation of this target
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape)
        {
            RDFGraph result = new RDFGraph();

            if (shape != null)
            {
                //shape sh:target _:t
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.TARGET, TargetValue));

                //_:t rdf:type sh:SPARQLTarget
                result.AddTriple(new RDFTriple(TargetValue, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.SPARQL_TARGET));

                //_:t sh:select "<SelectQuery>"^^xsd:string
                result.AddTriple(new RDFTriple(TargetValue, RDFVocabulary.SHACL.SELECT, new RDFTypedLiteral(SelectQuery.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));
            }

            return result;
        }
        #endregion
    }
}