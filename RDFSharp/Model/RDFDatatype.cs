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

using System;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFDatatype represents a custom datatype obtained by faceting the behavior of a base datatype
    /// </summary>
    public class RDFDatatype
    {
        #region Properties
        /// <summary>
        /// Datatype targeted by the facets
        /// </summary>
        public RDFModelEnums.RDFDatatypes TargetDatatype { get; internal set; }

        /// <summary>
        /// Uri of the datatype
        /// </summary>
        public Uri URI { get; internal set; }

        /// <summary>
        /// Indicates that the datatype is supported by RDFModelEnums.RDFDatatypes
        /// </summary>
        public bool IsBuiltIn { get; internal set; }

        /// <summary>
        /// Facets applied on the target datatype
        /// </summary>
        internal List<RDFFacet> Facets { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a datatype having the given URI, targeting the given datatype with the given list of facets
        /// </summary>
        public RDFDatatype(Uri datatypeURI, RDFModelEnums.RDFDatatypes targetDatatype, List<RDFFacet> facets)
        {
          URI = datatypeURI ?? throw new RDFModelException("Cannot create RDFDatatype because given \"datatypeURI\" parameter is null.");
          TargetDatatype = targetDatatype;
          Facets = facets ?? new List<RDFFacet>();
        }
		#endregion

		#region Interfaces
		/// <summary>
		/// Gives the string representation of the datatype
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> URI.ToString();
		#endregion

		#region Methods
		/// <summary>
		/// Gives a graph representation of the datatype
		/// </summary>
		public RDFGraph ToRDFGraph()
        {
			RDFGraph datatypeGraph = new RDFGraph();

			RDFResource datatypeURI = new RDFResource(URI.ToString());
			datatypeGraph.AddTriple(new RDFTriple(datatypeURI, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE));

			if (Facets.Count > 0)
			{
				RDFCollection facetsCollection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
				Facets.ForEach(constraint => facetsCollection.AddItem(constraint.URI));
				datatypeGraph.AddTriple(new RDFTriple(datatypeURI, RDFVocabulary.OWL.ON_DATATYPE, new RDFResource(TargetDatatype.GetDatatypeFromEnum())));
				datatypeGraph.AddTriple(new RDFTriple(datatypeURI, RDFVocabulary.OWL.WITH_RESTRICTIONS, facetsCollection.ReificationSubject));
				datatypeGraph.AddCollection(facetsCollection);
				Facets.ForEach(facet => datatypeGraph = datatypeGraph.UnionWith(facet.ToRDFGraph()));
			}
			else
				datatypeGraph.AddTriple(new RDFTriple(datatypeURI, RDFVocabulary.OWL.EQUIVALENT_CLASS, new RDFResource(TargetDatatype.GetDatatypeFromEnum())));

			return datatypeGraph;
        }

		/// <summary>
		/// Validates the given literal against the datatype
		/// </summary>
		internal (bool,string) Validate(string literalValue)
        {
            //It should validate the target datatype
            (bool,string) validatesTargetDatatype = RDFModelUtilities.ValidateTypedLiteral(literalValue, TargetDatatype);

            //Then the eventual constraining facets
            if (validatesTargetDatatype.Item1 && Facets.Count > 0)
                return (Facets.TrueForAll(facet => facet.Validate(literalValue)), literalValue);
            else
                return validatesTargetDatatype;
        }
        #endregion
    }   
}