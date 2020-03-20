﻿/*
   Copyright 2012-2020 Marco De Salvo

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

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFModelException represents an exception thrown during manipulation of RDF data models.
    /// </summary>
    [Serializable]
    public class RDFModelException : Exception
    {

        #region Ctors
        /// <summary>
        /// Basic ctor to throw an empty RDFModelException
        /// </summary>
        public RDFModelException() : base() { }

        /// <summary>
        /// Basic ctor to throw an RDFModelException with message
        /// </summary>
        public RDFModelException(String message) : base(message) { }

        /// <summary>
        /// Basic ctor to throw an RDFModelException with message and inner exception
        /// </summary>
        public RDFModelException(String message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Basic ctor to support serialization of a remotely thrown RDFModelException
        /// </summary>
        protected RDFModelException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

    }

}