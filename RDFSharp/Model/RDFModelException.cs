﻿/*
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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFModelException represents an exception thrown during manipulation of RDF models
    /// </summary>
    [Serializable]
    public sealed class RDFModelException : Exception
    {
        #region Ctors
        /// <summary>
        /// Basic ctor to throw an empty RDFModelException
        /// </summary>
        public RDFModelException() { }

        /// <summary>
        /// Basic ctor to throw an RDFModelException with message
        /// </summary>
        public RDFModelException(string message) : base(message) { }

        /// <summary>
        /// Basic ctor to throw an RDFModelException with message and inner exception
        /// </summary>
        public RDFModelException(string message, Exception innerException) : base(message, innerException) { }
        #endregion
    }
}