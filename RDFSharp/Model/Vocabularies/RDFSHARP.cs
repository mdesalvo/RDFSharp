/*
   Copyright 2012-2022 Marco De Salvo

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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies.
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region RDFSHARP
        /// <summary>
        /// RDFSHARP represents the vocabulary of this library.
        /// </summary>
        public static class RDFSHARP
        {

            #region Properties
            /// <summary>
            /// rdfsharp
            /// </summary>
            public static readonly string PREFIX = "rdfsharp";

            /// <summary>
            /// https://rdfsharp.codeplex.com/
            /// </summary>
            public static readonly string BASE_URI = "https://rdfsharp.codeplex.com/";

            /// <summary>
            /// https://rdfsharp.codeplex.com/
            /// </summary>
            public static readonly string DEREFERENCE_URI = "https://rdfsharp.codeplex.com/";
            #endregion

        }
        #endregion
    }
}