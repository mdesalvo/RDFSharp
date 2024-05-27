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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFDatatypeRegister is a singleton in-memory container for registered RDF datatypes
    /// </summary>
    public sealed class RDFDatatypeRegister : IEnumerable<RDFDatatype>
    {
		#region Statics
		internal static RDFDatatype RDFSLiteral = new RDFDatatype(RDFVocabulary.RDFS.LITERAL.URI, RDFModelEnums.RDFDatatypes.RDFS_LITERAL, null);
		#endregion

        #region Properties
        /// <summary>
        /// Singleton instance of the RDFDatatypeRegister class
        /// </summary>
        public static RDFDatatypeRegister Instance { get; internal set; }

        /// <summary>
        /// List of registered datatypes
        /// </summary>
        internal List<RDFDatatype> Register { get; set; }

        /// <summary>
        /// Count of the register's namespaces
        /// </summary>
        public static int DatatypesCount
            => Instance.Register.Count;

        /// <summary>
        /// Gets the enumerator on the register's namespaces for iteration
        /// </summary>
        public static IEnumerator<RDFDatatype> DatatypesEnumerator
            => Instance.Register.GetEnumerator();
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the register
        /// </summary>
        static RDFDatatypeRegister()
        {
			Instance = new RDFDatatypeRegister { 
				Register = new List<RDFDatatype>() };

			foreach (RDFModelEnums.RDFDatatypes datatype in Enum.GetValues(typeof(RDFModelEnums.RDFDatatypes)).Cast<RDFModelEnums.RDFDatatypes>())
				Instance.Register.Add(new RDFDatatype(new Uri(RDFModelUtilities.GetDatatypeFromEnum(datatype)), datatype, null) { IsBuiltIn = true });
		}
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the register's datatypes
        /// </summary>
        IEnumerator<RDFDatatype> IEnumerable<RDFDatatype>.GetEnumerator()
            => DatatypesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the register's datatypes
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => DatatypesEnumerator;
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given datatype to the register
        /// </summary>
        public static void AddDatatype(RDFDatatype datatype)
        {
            if (datatype != null && GetDatatype(datatype.ToString()) == null)
				Instance.Register.Add(datatype);
        }

        /// <summary>
        /// Retrieves a standard datatype by seeking presence of its Uri
        /// </summary>
        public static RDFDatatype GetDatatype(RDFModelEnums.RDFDatatypes datatype)
            => GetDatatype(datatype.GetDatatypeFromEnum());

        /// <summary>
        /// Retrieves a datatype by seeking presence of its Uri (null if not found)
        /// </summary>
        public static RDFDatatype GetDatatype(string datatypeUri)
			=> Instance.Register.Find(dt => string.Equals(dt.ToString(), datatypeUri?.Trim()));
        #endregion
    }
}