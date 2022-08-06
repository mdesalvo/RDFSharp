/*
   Copyright 2012-2022 Marco De Salvo

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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        [TestMethod]
        public void ShouldSerializeQueryException()
        {
            byte[] SerializeToBytes(RDFQueryException e)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, e);
                    return stream.GetBuffer();
                }   
            }

            RDFQueryException DeserializeFromBytes(byte[] data)
            {
                using (MemoryStream stream = new MemoryStream(data))
                    return (RDFQueryException)new BinaryFormatter().Deserialize(stream);
            }

            RDFQueryException mex = new RDFQueryException("RDFQueryException is serializable");
            byte[] bytes = SerializeToBytes(mex);
            Assert.IsTrue(bytes.Length > 0);

            RDFQueryException result = DeserializeFromBytes(bytes);
            Assert.IsTrue(result.Message.Equals("RDFQueryException is serializable"));
            Assert.IsNull(result.InnerException);
        }
        #endregion
    }
}