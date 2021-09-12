/*
   Copyright 2012-2021 Marco De Salvo

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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;

namespace RDFSharp.Test
{
    [TestClass]
    public class RDFModelExceptionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldRaiseEmptyModelException()
        {
            try
            {
                throw new RDFModelException();
            }
            catch (RDFModelException mex)
            {
                Assert.IsTrue(mex.Message.Contains("RDFSharp.Model.RDFModelException", StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestMethod]
        public void ShouldRaiseMessageModelException()
        {
            try
            {
                throw new RDFModelException("This is an exception coming from RDF modeling!");
            }
            catch (RDFModelException mex)
            {
                Assert.IsTrue(mex.Message.Equals("This is an exception coming from RDF modeling!", StringComparison.OrdinalIgnoreCase));
                Assert.IsNull(mex.InnerException);
            }
        }

        [TestMethod]
        public void ShouldRaiseMessageWithInnerModelException()
        {
            try
            {
                throw new RDFModelException("This is an exception coming from RDF modeling!", new Exception("This is the inner exception!"));
            }
            catch (RDFModelException mex)
            {
                Assert.IsTrue(mex.Message.Equals("This is an exception coming from RDF modeling!", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(mex.InnerException);
                Assert.IsTrue(mex.InnerException.Message.Equals("This is the inner exception!"));
            }
        }
        #endregion
    }
}