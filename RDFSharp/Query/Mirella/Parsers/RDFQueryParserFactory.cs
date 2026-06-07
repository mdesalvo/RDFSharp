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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryParserFactory is the public entry point for turning a SPARQL 1.1 query string into the matching
    /// RDFQuery object-model instance. It reads the query's prologue and form keyword and returns the concrete
    /// query type (RDFSelectQuery, RDFAskQuery, RDFConstructQuery, RDFDescribeQuery) as an RDFQuery. The strongly
    /// typed <c>FromString</c> helpers on the concrete query classes delegate here and then validate the form.
    /// </summary>
    public static class RDFQueryParserFactory
    {
        #region Methods
        /// <summary>
        /// Parses the given SPARQL 1.1 query string into its RDFQuery object-model representation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the query string is null/empty or syntactically invalid.</exception>
        public static RDFQuery ParseQuery(string queryString)
            => RDFQueryParser.ParseQuery(queryString);
        #endregion
    }
}