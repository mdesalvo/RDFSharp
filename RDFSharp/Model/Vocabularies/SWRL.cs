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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies
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
            /// http://www.w3.org/2003/11/swrl#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2003/11/swrl#";

            /// <summary>
            /// http://www.w3.org/2003/11/swrl#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/11/swrl#";

            /// <summary>
            /// swrl:Imp
            /// </summary>
            public static readonly RDFResource IMP = new RDFResource(string.Concat(BASE_URI,"Imp"));

            /// <summary>
            /// swrl:head
            /// </summary>
            public static readonly RDFResource HEAD = new RDFResource(string.Concat(BASE_URI,"head"));

            /// <summary>
            /// swrl:body
            /// </summary>
            public static readonly RDFResource BODY = new RDFResource(string.Concat(BASE_URI,"body"));

            /// <summary>
            /// swrl:Variable
            /// </summary>
            public static readonly RDFResource VARIABLE = new RDFResource(string.Concat(BASE_URI,"Variable"));

            /// <summary>
            /// swrl:Atom
            /// </summary>
            public static readonly RDFResource ATOM = new RDFResource(string.Concat(BASE_URI,"Atom"));

            /// <summary>
            /// swrl:AtomList
            /// </summary>
            public static readonly RDFResource ATOMLIST = new RDFResource(string.Concat(BASE_URI,"AtomList"));

            /// <summary>
            /// swrl:Builtin
            /// </summary>
            public static readonly RDFResource BUILTIN_CLS = new RDFResource(string.Concat(BASE_URI,"Builtin"));

            /// <summary>
            /// swrl:argument1
            /// </summary>
            public static readonly RDFResource ARGUMENT1 = new RDFResource(string.Concat(BASE_URI,"argument1"));

            /// <summary>
            /// swrl:argument2
            /// </summary>
            public static readonly RDFResource ARGUMENT2 = new RDFResource(string.Concat(BASE_URI,"argument2"));

            /// <summary>
            /// swrl:arguments
            /// </summary>
            public static readonly RDFResource ARGUMENTS = new RDFResource(string.Concat(BASE_URI,"arguments"));

            /// <summary>
            /// swrl:classPredicate
            /// </summary>
            public static readonly RDFResource CLASS_PREDICATE = new RDFResource(string.Concat(BASE_URI,"classPredicate"));

            /// <summary>
            /// swrl:propertyPredicate
            /// </summary>
            public static readonly RDFResource PROPERTY_PREDICATE = new RDFResource(string.Concat(BASE_URI,"propertyPredicate"));

            /// <summary>
            /// swrl:dataRange
            /// </summary>
            public static readonly RDFResource DATARANGE = new RDFResource(string.Concat(BASE_URI,"dataRange"));

            /// <summary>
            /// swrl:builtin
            /// </summary>
            public static readonly RDFResource BUILTIN_PROP = new RDFResource(string.Concat(BASE_URI,"builtin"));

            /// <summary>
            /// swrl:ClassAtom
            /// </summary>
            public static readonly RDFResource CLASS_ATOM = new RDFResource(string.Concat(BASE_URI,"ClassAtom"));

            /// <summary>
            /// swrl:IndividualPropertyAtom
            /// </summary>
            public static readonly RDFResource INDIVIDUAL_PROPERTY_ATOM = new RDFResource(string.Concat(BASE_URI,"IndividualPropertyAtom"));

            /// <summary>
            /// swrl:DatavaluedPropertyAtom
            /// </summary>
            public static readonly RDFResource DATAVALUED_PROPERTY_ATOM = new RDFResource(string.Concat(BASE_URI,"DatavaluedPropertyAtom"));

            /// <summary>
            /// swrl:SameIndividualAtom
            /// </summary>
            public static readonly RDFResource SAME_INDIVIDUAL_ATOM = new RDFResource(string.Concat(BASE_URI,"SameIndividualAtom"));

            /// <summary>
            /// swrl:DifferentIndividualsAtom
            /// </summary>
            public static readonly RDFResource DIFFERENT_INDIVIDUALS_ATOM = new RDFResource(string.Concat(BASE_URI,"DifferentIndividualsAtom"));

            /// <summary>
            /// swrl:DataRangeAtom
            /// </summary>
            public static readonly RDFResource DATARANGE_ATOM = new RDFResource(string.Concat(BASE_URI,"DataRangeAtom"));

            /// <summary>
            /// swrl:BuiltinAtom
            /// </summary>
            public static readonly RDFResource BUILTIN_ATOM = new RDFResource(string.Concat(BASE_URI,"BuiltinAtom"));
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
                /// http://www.w3.org/2003/11/swrlb#
                /// </summary>
                public static readonly string BASE_URI = "http://www.w3.org/2003/11/swrlb#";

                /// <summary>
                /// http://www.w3.org/2003/11/swrlb#
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/11/swrlb#";
                #endregion
            }
            #endregion
        }
        #endregion
    }
}