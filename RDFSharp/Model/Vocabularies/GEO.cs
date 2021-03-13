/*
   Copyright 2012-2020 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License"));
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
        #region GEO
        /// <summary>
        /// GEO represents the W3C GEO vocabulary.
        /// </summary>
        public static class GEO
        {

            #region Properties
            /// <summary>
            /// geo
            /// </summary>
            public static readonly string PREFIX = "geo";

            /// <summary>
            /// http://www.w3.org/2003/01/geo/wgs84_pos#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2003/01/geo/wgs84_pos#";

            /// <summary>
            /// http://www.w3.org/2003/01/geo/wgs84_pos#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/01/geo/wgs84_pos#";

            /// <summary>
            /// geo:lat
            /// </summary>
            public static readonly RDFResource LAT = new RDFResource(string.Concat(GEO.BASE_URI, "lat"));

            /// <summary>
            /// geo:long
            /// </summary>
            public static readonly RDFResource LONG = new RDFResource(string.Concat(GEO.BASE_URI, "long"));

            /// <summary>
            /// geo:lat_long
            /// </summary>
            public static readonly RDFResource LAT_LONG = new RDFResource(string.Concat(GEO.BASE_URI, "lat_long"));

            /// <summary>
            /// geo:alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource(string.Concat(GEO.BASE_URI, "alt"));

            /// <summary>
            /// geo:Point
            /// </summary>
            public static readonly RDFResource POINT = new RDFResource(string.Concat(GEO.BASE_URI, "Point"));

            /// <summary>
            /// geo:SpatialThing
            /// </summary>
            public static readonly RDFResource SPATIAL_THING = new RDFResource(string.Concat(GEO.BASE_URI, "SpatialThing"));

            /// <summary>
            /// geo:location
            /// </summary>
            public static readonly RDFResource LOCATION = new RDFResource(string.Concat(GEO.BASE_URI, "location"));
            #endregion

        }
        #endregion
    }
}