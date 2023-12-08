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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
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
        public async Task ShouldSerializeFalseAskQueryResultToStreamAsync()
        {
            MemoryStream stream = new MemoryStream();
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            await askResult.ToSparqlXmlResultAsync(stream);
            byte[] streamData = stream.ToArray();
            
            Assert.IsTrue(streamData.Length > 100);

            RDFAskQueryResult askResult2 = await RDFAskQueryResult.FromSparqlXmlResultAsync(new MemoryStream(streamData));

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

        [TestMethod]
        public async Task ShouldSerializeTrueAskQueryResultToStreamAsync()
        {
            MemoryStream stream = new MemoryStream();
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            askResult.AskResult = true;
            await askResult.ToSparqlXmlResultAsync(stream);
            byte[] streamData = stream.ToArray();
            
            Assert.IsTrue(streamData.Length > 100);

            RDFAskQueryResult askResult2 = await RDFAskQueryResult.FromSparqlXmlResultAsync(new MemoryStream(streamData));

            Assert.IsNotNull(askResult2);
            Assert.IsTrue(askResult2.AskResult);
        }

        [TestMethod]
        public void ShouldSerializeFalseAskQueryResultToFile()
        {
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            askResult.ToSparqlXmlResult(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeFalseAskQueryResultToFile.srx"));
            
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeFalseAskQueryResultToFile.srx")));

            RDFAskQueryResult askResult2 = RDFAskQueryResult.FromSparqlXmlResult(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeFalseAskQueryResultToFile.srx"));

            Assert.IsNotNull(askResult2);
            Assert.IsFalse(askResult2.AskResult);
        }

        [TestMethod]
        public async Task ShouldSerializeFalseAskQueryResultToFileAsync()
        {
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            await askResult.ToSparqlXmlResultAsync(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeFalseAskQueryResultToFileAsync.srx"));
            
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeFalseAskQueryResultToFileAsync.srx")));

            RDFAskQueryResult askResult2 = await RDFAskQueryResult.FromSparqlXmlResultAsync(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeFalseAskQueryResultToFileAsync.srx"));

            Assert.IsNotNull(askResult2);
            Assert.IsFalse(askResult2.AskResult);
        }

        [TestMethod]
        public void ShouldSerializeTrueAskQueryResultToFile()
        {
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            askResult.AskResult = true;
            askResult.ToSparqlXmlResult(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeTrueAskQueryResultToFile.srx"));
            
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeTrueAskQueryResultToFile.srx")));

            RDFAskQueryResult askResult2 = RDFAskQueryResult.FromSparqlXmlResult(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeTrueAskQueryResultToFile.srx"));

            Assert.IsNotNull(askResult2);
            Assert.IsTrue(askResult2.AskResult);
        }

        [TestMethod]
        public async Task ShouldSerializeTrueAskQueryResultToFileAsync()
        {
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            askResult.AskResult = true;
            await askResult.ToSparqlXmlResultAsync(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeTrueAskQueryResultToFileAsync.srx"));
            
            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeTrueAskQueryResultToFileAsync.srx")));

            RDFAskQueryResult askResult2 = await RDFAskQueryResult.FromSparqlXmlResultAsync(Path.Combine(Environment.CurrentDirectory, "RDFAskQueryResultTest_ShouldSerializeTrueAskQueryResultToFileAsync.srx"));

            Assert.IsNotNull(askResult2);
            Assert.IsTrue(askResult2.AskResult);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAskQueryResultBecauseMissingHead()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <boolean>TRUE</boolean>
</sparql>");

            Assert.ThrowsException<RDFQueryException>(() => RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAskQueryResultBecauseMissingBoolean()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head />
</sparql>");

            Assert.ThrowsException<RDFQueryException>(() => RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAskQueryResultBecauseHeadAfterBoolean()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <boolean>TRUE</boolean>
  <head />
</sparql>");

            Assert.ThrowsException<RDFQueryException>(() => RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAskQueryResultBecauseInvalidBoolean()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head />
  <boolean>hello</boolean>  
</sparql>");

            Assert.ThrowsException<RDFQueryException>(() => RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnDeserializingAskQueryResultBecauseMissingHeadAndBoolean()
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
                writer.WriteLine(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
</sparql>");

            Assert.ThrowsException<RDFQueryException>(() => RDFAskQueryResult.FromSparqlXmlResult(new MemoryStream(stream.ToArray())));
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFAskQueryResultTest_Should*"))
                File.Delete(file);
        }
        #endregion
    }
}