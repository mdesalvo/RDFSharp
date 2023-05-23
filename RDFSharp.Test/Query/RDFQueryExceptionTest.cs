/*
   Copyright 2012-2023 Marco De Salvo

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
using RDFSharp.Query;
using System;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFQueryExceptionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldRaiseEmptyQueryException()
        {
            try
            {
                throw new RDFQueryException();
            }
            catch (RDFQueryException mex)
            {
                Assert.IsTrue(mex.Message.Contains("RDFSharp.Query.RDFQueryException", StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestMethod]
        public void ShouldRaiseMessageQueryException()
        {
            try
            {
                throw new RDFQueryException("This is an exception coming from RDF Query!");
            }
            catch (RDFQueryException mex)
            {
                Assert.IsTrue(mex.Message.Equals("This is an exception coming from RDF Query!", StringComparison.OrdinalIgnoreCase));
                Assert.IsNull(mex.InnerException);
            }
        }

        [TestMethod]
        public void ShouldRaiseMessageWithInnerQueryException()
        {
            try
            {
                throw new RDFQueryException("This is an exception coming from RDF Query!", new Exception("This is the inner exception!"));
            }
            catch (RDFQueryException mex)
            {
                Assert.IsTrue(mex.Message.Equals("This is an exception coming from RDF Query!", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(mex.InnerException);
                Assert.IsTrue(mex.InnerException.Message.Equals("This is the inner exception!"));
            }
        }
        #endregion
    }
}