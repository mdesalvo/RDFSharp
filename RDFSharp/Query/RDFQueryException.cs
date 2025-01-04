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

using System;
using System.Runtime.Serialization;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryException represents an exception thrown during creation and execution of SPARQL queries
    /// </summary>
    [Serializable]
    public class RDFQueryException : Exception
    {
        #region Ctors
        /// <summary>
        /// Basic ctor to throw an empty RDFQueryException
        /// </summary>
        public RDFQueryException() { }

        /// <summary>
        /// Basic ctor to throw an RDFQueryException with message
        /// </summary>
        public RDFQueryException(string message) : base(message) { }

        /// <summary>
        /// Basic ctor to throw an RDFQueryException with message and inner exception
        /// </summary>
        public RDFQueryException(string message, Exception innerException) : base(message, innerException) { }
        #endregion
    }
}