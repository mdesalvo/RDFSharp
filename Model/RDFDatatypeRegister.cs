/*
   Copyright 2012-2016 Marco De Salvo

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

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFDatatypeRegister is a singleton container for registered RDF datatypes.
    /// </summary>
    public sealed class RDFDatatypeRegister: IEnumerable<RDFDatatype> {

        #region Properties
        /// <summary>
        /// Singleton instance of the RDFDatatypeRegister class
        /// </summary>
        internal static RDFDatatypeRegister Instance { get; set; }

        /// <summary>
        /// List of registered datatypes
        /// </summary>
        internal List<RDFDatatype> Register { get; set; }

        /// <summary>
        /// Count of the register's datatypes
        /// </summary>
        public static Int32 DatatypesCount {
            get { return Instance.Register.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the register's datatypes for iteration
        /// </summary>
        public static IEnumerator<RDFDatatype> DatatypesEnumerator {
            get { return Instance.Register.GetEnumerator(); }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the register
        /// </summary>
        static RDFDatatypeRegister() {
            Instance          = new RDFDatatypeRegister();
            Instance.Register = new List<RDFDatatype>();

            #region Datatypes
            AddDatatype(new RDFDatatype(RDFVocabulary.RDFS.PREFIX, RDFVocabulary.RDFS.BASE_URI, RDFVocabulary.RDFS.LITERAL.ToString().Replace(RDFVocabulary.RDFS.BASE_URI, String.Empty),            RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.RDF.PREFIX,  RDFVocabulary.RDF.BASE_URI,  RDFVocabulary.RDF.XML_LITERAL.ToString().Replace(RDFVocabulary.RDF.BASE_URI, String.Empty),          RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.RDF.PREFIX,  RDFVocabulary.RDF.BASE_URI,  RDFVocabulary.RDF.HTML.ToString().Replace(RDFVocabulary.RDF.BASE_URI, String.Empty),                 RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.BOOLEAN.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),              RDFModelEnums.RDFDatatypeCategory.Boolean));
			AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.DATETIME.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),             RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.DATE.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                 RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.TIME.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                 RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.G_YEAR_MONTH.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),         RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.G_MONTH_DAY.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),          RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.G_YEAR.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),               RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.G_MONTH.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),              RDFModelEnums.RDFDatatypeCategory.DateTime));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.G_DAY.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                RDFModelEnums.RDFDatatypeCategory.DateTime));
			AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.DURATION.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),             RDFModelEnums.RDFDatatypeCategory.TimeSpan));
			AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.STRING.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),               RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.ANY_URI.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),              RDFModelEnums.RDFDatatypeCategory.String));
			AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.HEX_BINARY.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),           RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.BASE64_BINARY.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),        RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.LANGUAGE.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),             RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NORMALIZED_STRING.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),    RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.TOKEN.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NMTOKEN.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),              RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NAME.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                 RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NCNAME.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),               RDFModelEnums.RDFDatatypeCategory.String));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NOTATION.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),             RDFModelEnums.RDFDatatypeCategory.String));
			AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.DECIMAL.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),              RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.FLOAT.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.DOUBLE.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),               RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.INTEGER.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),              RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.POSITIVE_INTEGER.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),     RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),     RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty), RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty), RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.LONG.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                 RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.INT.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                  RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.SHORT.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.BYTE.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),                 RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.UNSIGNED_LONG.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),        RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.UNSIGNED_INT.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),         RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.UNSIGNED_SHORT.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),       RDFModelEnums.RDFDatatypeCategory.Numeric));
            AddDatatype(new RDFDatatype(RDFVocabulary.XSD.PREFIX,  RDFVocabulary.XSD.BASE_URI,  RDFVocabulary.XSD.UNSIGNED_BYTE.ToString().Replace(RDFVocabulary.XSD.BASE_URI, String.Empty),        RDFModelEnums.RDFDatatypeCategory.Numeric));            
            #endregion
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the register's datatypes
        /// </summary>
        IEnumerator<RDFDatatype> IEnumerable<RDFDatatype>.GetEnumerator() {
            return Instance.Register.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the register's datatypes
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return Instance.Register.GetEnumerator();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given datatype to the register.
        /// </summary>
        public static void AddDatatype(RDFDatatype datatype) {
            if (datatype != null) {
                if (!ContainsDatatype(datatype)) {
                     Instance.Register.Add(datatype);
                     //Also add the namespace to the namespace register.
                     RDFNamespaceRegister.AddNamespace(new RDFNamespace(datatype.Prefix, datatype.Namespace));
                }
            }
        }

        /// <summary>
        /// Removes the given datatype from the register
        /// </summary>
        public static void RemoveDatatype(RDFDatatype datatype) {
            if (datatype != null) {
                Instance.Register.RemoveAll(dt => (dt.Prefix.Equals(datatype.Prefix, StringComparison.Ordinal) || dt.Namespace.Equals(datatype.Namespace))
                                                   && dt.Datatype.Equals(datatype.Datatype, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Checks for existence of the given datatype in the register by seeking presence of its prefix or its uri and its datatype
        /// </summary>
        public static Boolean ContainsDatatype(RDFDatatype datatype) {
            if (datatype != null) {
                return Instance.Register.Exists(dt => (dt.Prefix.Equals(datatype.Prefix, StringComparison.Ordinal) || dt.Namespace.Equals(datatype.Namespace)) 
                                                   &&  dt.Datatype.Equals(datatype.Datatype, StringComparison.Ordinal));
            }
            return false;
        }

        /// <summary>
        /// Retrieves a datatype from the register by seeking presence of its namespace and datatype
        /// </summary>
        public static RDFDatatype GetByNamespaceAndDatatype(String nSpace, String datatype) {
            if (nSpace        != null && nSpace.Trim()   != String.Empty) {
                if(datatype   != null && datatype.Trim() != String.Empty) {
                    Uri tempNS = RDFModelUtilities.GetUriFromString(nSpace);
                    if(tempNS != null){
                        return Instance.Register.Find(dt => dt.Namespace.Equals(tempNS) && dt.Datatype.Equals(datatype, StringComparison.Ordinal));
                    }
                    throw new RDFModelException("Cannot retrieve RDFDatatype because given \"nSpace\" parameter (" + nSpace + ") does not represent a valid Uri.");
                }
                throw new RDFModelException("Cannot retrieve RDFDatatype because given \"datatype\" parameter is null or empty.");
            }
            throw new RDFModelException("Cannot retrieve RDFDatatype because given \"nSpace\" parameter is null or empty.");
        }

        /// <summary>
        /// Retrieves a datatype from the register by seeking presence of its prefix and datatype
        /// </summary>
        public static RDFDatatype GetByPrefixAndDatatype(String prefix, String datatype) {
            if (prefix       != null && prefix.Trim()   != String.Empty) {
                if (datatype != null && datatype.Trim() != String.Empty) {
                    return Instance.Register.Find(dt => dt.Prefix.Equals(prefix, StringComparison.Ordinal) && dt.Datatype.Equals(datatype, StringComparison.Ordinal));
                }
                throw new RDFModelException("Cannot retrieve RDFDatatype because given \"datatype\" parameter is null or empty.");
            }
            throw new RDFModelException("Cannot retrieve RDFDatatype because given \"prefix\" parameter is null or empty.");
        }
        #endregion

    }

}