/*
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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryEnums represents a collector for all the enumerations used by the "RDFSharp.Query" namespace
    /// </summary>
    public static class RDFQueryEnums
    {
        /// <summary>
        /// RDFPatternHoles represents an enumeration for possible positions of holes in a pattern.
        /// </summary>
        internal enum RDFPatternHoles { C, S, P, O, CS, CP, CO, CSP, CSO, CPO, CSPO, SP, SO, PO, SPO }

        /// <summary>
        /// RDFOrderByFlavors represents an enumeration for possible directions of query results ordering on a given variable.
        /// </summary>
        public enum RDFOrderByFlavors
        {
            /// <summary>
            /// Orders SPARQL results in ascending mode on the selected variable
            /// </summary>
            ASC = 1,
            /// <summary>
            /// Orders SPARQL results in descending mode on the selected variable
            /// </summary>
            DESC = 2
        }

        /// <summary>
        /// RDFLanguageDirections represents an enumeration for possible directions of a plainliteral's language tag.
        /// </summary>
        public enum RDFLanguageDirections
        {
            /// <summary>
            /// Left-to-Right
            /// </summary>
            LTR = 1,
            /// <summary>
            /// Right-to-Left
            /// </summary>
            RTL = 2
        }

        /// <summary>
        /// RDFComparisonFlavors represents an enumeration for possible comparison modes between two patten members.
        /// </summary>
        public enum RDFComparisonFlavors
        {
            /// <summary>
            /// Represents the less-or-equal comparison operator
            /// </summary>
            LessOrEqualThan = 1,
            /// <summary>
            /// Represents the less comparison operator
            /// </summary>
            LessThan = 2,
            /// <summary>
            /// Represents the equal comparison operator
            /// </summary>
            EqualTo = 3,
            /// <summary>
            /// Represents the not-equal comparison operator
            /// </summary>
            NotEqualTo = 4,
            /// <summary>
            /// Represents the greater comparison operator
            /// </summary>
            GreaterThan = 5,
            /// <summary>
            /// Represents the greater-or-equal comparison operator
            /// </summary>
            GreaterOrEqualThan = 6
        }

        /// <summary>
        /// RDFPropertyPathStepFlavors represents an enumeration for possible connection types within a property path.
        /// </summary>
        public enum RDFPropertyPathStepFlavors
        {
            /// <summary>
            /// Steps within a property path are connected with AND semantic
            /// </summary>
            Sequence = '/',
            /// <summary>
            /// Steps within a property path are connected with OR semantic
            /// </summary>
            Alternative = '|'
        }

        /// <summary>
        /// RDFMinMaxAggregatorFlavors represents an enumeration for supported flavors of MIN/MAX aggregators
        /// </summary>
        public enum RDFMinMaxAggregatorFlavors
        {
            /// <summary>
            /// MIN/MAX aggregator suitable for working on columns with numeric typedliteral values
            /// </summary>
            Numeric = 1,
            /// <summary>
            /// MIN/MAX aggregator suitable for working on columns with generic values
            /// </summary>
            String = 2
        }

        /// <summary>
        /// RDFSPARQLEndpointQueryErrorBehaviors represents an enumeration for possible handling behaviors of SPARQL endpoint query errors
        /// </summary>
        public enum RDFSPARQLEndpointQueryErrorBehaviors
        {
            /// <summary>
            /// An exception will be delivered to the application
            /// </summary>
            ThrowException = 1,
            /// <summary>
            /// An empty SPARQL result will be delivered to the application
            /// </summary>
            GiveEmptyResult = 2
        }

        /// <summary>
        /// RDFSPARQLEndpointAuthorizationTypes represents an enumeration for possible authorization header types to be sent to a SPARQL endpoint
        /// </summary>
        public enum RDFSPARQLEndpointAuthorizationTypes
        {
            /// <summary>
            /// No authorization header will be sent to the SPARQL endpoint
            /// </summary>
            None = 0,
            /// <summary>
            /// Basic authorization header will be sent to the SPARQL endpoint
            /// </summary>
            Basic = 1,
            /// <summary>
            /// Bearer authorization header will be sent to the SPARQL endpoint
            /// </summary>
            Bearer = 2
        }

        /// <summary>
        /// RDFSPARQLEndpointQueryMethods represents an enumeration for possible HTTP methods of SPARQL endpoint query
        /// </summary>
        public enum RDFSPARQLEndpointQueryMethods
        {
            /// <summary>
            /// SPARQL endpoint will be contacted with HTTP/GET
            /// </summary>
            Get = 1,
            /// <summary>
            /// SPARQL endpoint will be contacted with HTTP/POST
            /// </summary>
            Post = 2
        }

        /// <summary>
        /// RDFSPARQLEndpointOperationContentTypes represents an enumeration for supported Content-Type headers to be sent for SPARQL UPDATE operations
        /// </summary>
        public enum RDFSPARQLEndpointOperationContentTypes
        {
            /// <summary>
            /// Posts the SPARQL UPDATE operation with "application/sparql-update" Content-Type header
            /// </summary>
            Sparql_Update = 1,
            /// <summary>
            /// Posts the SPARQL UPDATE operation with "application/x-www-form-urlencoded" Content-Type header
            /// </summary>
            X_WWW_FormUrlencoded = 2
        }

        /// <summary>
        /// RDFClearOperationFlavor represents an enumeration for supported flavors of implicit SPARQL CLEAR operation
        /// </summary>
        public enum RDFClearOperationFlavor
        {
            /// <summary>
            /// Indicates to remove only triples belonging to the default graph
            /// </summary>
            DEFAULT = 0,
            /// <summary>
            /// Indicates to remove only triples belonging to the named graphs
            /// </summary>
            NAMED = 1,
            /// <summary>
            /// Indicates to remove only triples belonging to the both default and named graphs
            /// </summary>
            ALL = 2
        }

        /// <summary>
        /// Represents an enumeration for the set of built-in Egenhofer spatial relations
        /// </summary>
        public enum RDFGeoEgenhoferRelations
        {
            /// <summary>
            /// geof:ehEquals
            /// </summary>
            Equals = 1,
            /// <summary>
            /// geof:ehDisjoint
            /// </summary>
            Disjoint = 2,
            /// <summary>
            /// geof:ehMeet
            /// </summary>
            Meet = 3,
            /// <summary>
            /// geof:ehOverlap
            /// </summary>
            Overlap = 4,
            /// <summary>
            /// geof:ehCovers
            /// </summary>
            Covers = 5,
            /// <summary>
            /// geof:ehCoveredBy
            /// </summary>
            CoveredBy = 6,
            /// <summary>
            /// geof:ehInside
            /// </summary>
            Inside = 7,
            /// <summary>
            /// geof:ehContains
            /// </summary>
            Contains = 8
        }

        /// <summary>
        /// Represents an enumeration for the set of built-in Region-Connected-Calculus-8 spatial relations
        /// </summary>
        public enum RDFGeoRCC8Relations
        {
            /// <summary>
            /// geosf:rcc8dc    -> geof:relate("FFTFFTTTT")
            /// </summary>
            RCC8DC = 1,
            /// <summary>
            /// geosf:rcc8dc    -> geof:relate("FFTFTTTTT")
            /// </summary>
            RCC8EC = 2,
            /// <summary>
            /// geosf:rcc8po    -> geof:relate("TTTTTTTTT")
            /// </summary>
            RCC8PO = 3,
            /// <summary>
            /// geosf:rcc8tppi  -> geof:relate("TTTFTTFFT")
            /// </summary>
            RCC8TPPI = 4,
            /// <summary>
            /// geosf:rcc8tpp   -> geof:relate("TFFTTFTTT")
            /// </summary>
            RCC8TPP = 5,
            /// <summary>
            /// geosf:rcc8ntpp  -> geof:relate("TFFTFFTTT")
            /// </summary>
            RCC8NTPP = 6,
            /// <summary>
            /// geosf:rcc8ntppi -> geof:relate("TTTFFTFFT")
            /// </summary>
            RCC8NTPPI = 7,
            /// <summary>
            /// geosf:rcc8eq    -> geof:relate("TFFFTFFFT")
            /// </summary>
            RCC8EQ = 8
        }
    }
}