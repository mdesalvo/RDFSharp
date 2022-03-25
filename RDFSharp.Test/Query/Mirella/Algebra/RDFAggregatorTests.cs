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
using System.Reflection;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFAggregatorTests
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateAggregator()
        {
            RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsFalse(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.ToString().Equals(string.Empty));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAggregatorBecauseNullAggregatorVariable()
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAggregatorBecauseNullPartitionVariable()
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFAggregator(new RDFVariable("?AGGVAR"), null as RDFVariable));
        #endregion
    }
}