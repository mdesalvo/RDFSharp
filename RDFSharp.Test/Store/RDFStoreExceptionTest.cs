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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Store;
using System;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFStoreExceptionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldRaiseEmptyStoreException()
        {
            try
            {
                throw new RDFStoreException();
            }
            catch (RDFStoreException mex)
            {
                Assert.IsTrue(mex.Message.Contains("RDFSharp.Store.RDFStoreException", StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestMethod]
        public void ShouldRaiseMessageStoreException()
        {
            try
            {
                throw new RDFStoreException("This is an exception coming from RDF Store!");
            }
            catch (RDFStoreException mex)
            {
                Assert.IsTrue(mex.Message.Equals("This is an exception coming from RDF Store!", StringComparison.OrdinalIgnoreCase));
                Assert.IsNull(mex.InnerException);
            }
        }

        [TestMethod]
        public void ShouldRaiseMessageWithInnerStoreException()
        {
            try
            {
                throw new RDFStoreException("This is an exception coming from RDF Store!", new Exception("This is the inner exception!"));
            }
            catch (RDFStoreException mex)
            {
                Assert.IsTrue(mex.Message.Equals("This is an exception coming from RDF Store!", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(mex.InnerException);
                Assert.IsTrue(mex.InnerException.Message.Equals("This is the inner exception!"));
            }
        }
        #endregion
    }
}