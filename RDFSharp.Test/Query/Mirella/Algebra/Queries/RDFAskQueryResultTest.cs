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
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFAskQueryResultTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateAskQueryResult()
        {
            RDFAskQueryResult result = new RDFAskQueryResult();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldSerializeFalseAskQueryResultToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            askResult.ToSparqlXmlResult(stream);
            byte[] streamData = stream.ToArray();
            
            Assert.IsTrue(streamData.Length > 100);

            RDFAskQueryResult askResult2 = RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(streamData));

            Assert.IsNotNull(askResult2);
            Assert.IsFalse(askResult2.AskResult);
        }

        [TestMethod]
        public void ShouldSerializeTrueAskQueryResultToStream()
        {
            MemoryStream stream = new MemoryStream();
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            askResult.AskResult = true;
            askResult.ToSparqlXmlResult(stream);
            byte[] streamData = stream.ToArray();
            
            Assert.IsTrue(streamData.Length > 100);

            RDFAskQueryResult askResult2 = RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(streamData));

            Assert.IsNotNull(askResult2);
            Assert.IsTrue(askResult2.AskResult);
        }
        #endregion
    }
}