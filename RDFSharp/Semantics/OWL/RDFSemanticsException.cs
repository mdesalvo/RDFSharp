/*
   Copyright 2015-2020 Marco De Salvo

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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFSemanticsException represents an exception thrown during creation and execution of RDF semantics.
    /// </summary>
    [Serializable]
    public class RDFSemanticsException : Exception
    {

        #region Ctors
        /// <summary>
        /// Basic ctor to throw an empty RDFSemanticsException
        /// </summary>
        public RDFSemanticsException() : base() { }

        /// <summary>
        /// Basic ctor to throw an RDFSemanticsException with message
        /// </summary>
        public RDFSemanticsException(string message) : base(message) { }

        /// <summary>
        /// Basic ctor to throw an RDFSemanticsException with message and inner exception
        /// </summary>
        public RDFSemanticsException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Basic ctor to support serialization of a remotely thrown RDFSemanticsException
        /// </summary>
        protected RDFSemanticsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

    }

}