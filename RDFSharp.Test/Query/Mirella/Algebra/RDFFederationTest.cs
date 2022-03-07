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
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFFederationTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateFederation()
        {
            RDFFederation federation = new RDFFederation();

            Assert.IsNotNull(federation);
            Assert.IsNotNull(federation.DataSources);
            Assert.IsTrue(federation.DataSourcesCount == 0);
            Assert.IsTrue(federation.FederationName.StartsWith("FEDERATION|ID="));
            Assert.IsTrue(federation.ToString().Equals(federation.FederationName));

            int i = 0;
            foreach (RDFDataSource s in federation) i++;
            Assert.IsTrue(i == 0);

            int j = 0;
            IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
            while (datasourcesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 0);
        }

        [TestMethod]
        public void ShouldAddGraphToFederation()
        {
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(new RDFGraph());
            federation.AddGraph(null); //Will be discarded, since null is not allowed

            Assert.IsTrue(federation.DataSourcesCount == 1);

            int i = 0;
            foreach (RDFDataSource s in federation) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
            while (datasourcesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldAddStoreToFederation()
        {
            RDFFederation federation = new RDFFederation();
            federation.AddStore(new RDFMemoryStore());
            federation.AddStore(null); //Will be discarded, since null is not allowed

            Assert.IsTrue(federation.DataSourcesCount == 1);

            int i = 0;
            foreach (RDFDataSource s in federation) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
            while (datasourcesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldAddFederationToFederation()
        {
            RDFFederation federation = new RDFFederation();
            federation.AddFederation(new RDFFederation());
            federation.AddFederation(federation); //Will be discarded, since self-reference is not allowed
            federation.AddFederation(null); //Will be discarded, since null is not allowed

            Assert.IsTrue(federation.DataSourcesCount == 1);

            int i = 0;
            foreach (RDFDataSource s in federation) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
            while (datasourcesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldAddSPARQLEndpoinToFederation()
        {
            RDFFederation federation = new RDFFederation();
            federation.AddSPARQLEndpoint(new RDFSPARQLEndpoint(new Uri("ex:sparqlEndpoint")));
            federation.AddSPARQLEndpoint(null); //Will be discarded, since null is not allowed

            Assert.IsTrue(federation.DataSourcesCount == 1);

            int i = 0;
            foreach (RDFDataSource s in federation) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
            while (datasourcesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldClearFederation()
        {
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(new RDFGraph());
            federation.ClearDataSources();

            Assert.IsNotNull(federation);
            Assert.IsNotNull(federation.DataSources);
            Assert.IsTrue(federation.DataSourcesCount == 0);

            int i = 0;
            foreach (RDFDataSource s in federation) i++;
            Assert.IsTrue(i == 0);

            int j = 0;
            IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
            while (datasourcesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 0);
        }
        #endregion
    }
}