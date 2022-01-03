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
        #region SWRL
        /// <summary>
        /// SWRL represents the W3C Semantic Web Rule Language vocabulary.
        /// </summary>
        public static class SWRL
        {

            #region Properties
            /// <summary>
            /// swrl
            /// </summary>
            public static readonly string PREFIX = "swrl";

            /// <summary>
            /// https://www.w3.org/2003/11/swrl
            /// </summary>
            public static readonly string BASE_URI = "https://www.w3.org/2003/11/swrl";

            /// <summary>
            /// https://www.w3.org/2003/11/swrl
            /// </summary>
            public static readonly string DEREFERENCE_URI = "https://www.w3.org/2003/11/swrl";
            #endregion

            #region Extended Properties
            /// <summary>
            /// SWRLB represents the W3C Semantic Web Rule Language - BuiltIns vocabulary.
            /// </summary>
            public static class SWRLB
            {

                #region Properties
                /// <summary>
                /// swrlb
                /// </summary>
                public static readonly string PREFIX = "swrlb";

                /// <summary>
                /// https://www.w3.org/2003/11/swrlb
                /// </summary>
                public static readonly string BASE_URI = "https://www.w3.org/2003/11/swrlb";

                /// <summary>
                /// https://www.w3.org/2003/11/swrlb
                /// </summary>
                public static readonly string DEREFERENCE_URI = "https://www.w3.org/2003/11/swrlb";
                #endregion

            }
            #endregion

        }
        #endregion
    }
}