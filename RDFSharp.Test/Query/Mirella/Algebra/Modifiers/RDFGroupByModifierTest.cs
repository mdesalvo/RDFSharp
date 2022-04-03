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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFGroupByModifierTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateGroupByModifier()
        {
            RDFVariable variable = new RDFVariable("?VAR");
            List<RDFVariable> partitionVariables = new List<RDFVariable>() { variable };
            RDFGroupByModifier modifier = new RDFGroupByModifier(partitionVariables);

            Assert.IsNotNull(modifier);
            Assert.IsNotNull(modifier.PartitionVariables);
            Assert.IsTrue(modifier.PartitionVariables.Count == 1);
            Assert.IsTrue(modifier.PartitionVariables[0].Equals(variable));
            Assert.IsNotNull(modifier.Aggregators);
            Assert.IsTrue(modifier.Aggregators.Count == 1);
            Assert.IsTrue(modifier.Aggregators[0].ProjectionVariable.Equals(variable));
            Assert.IsTrue(modifier.Aggregators[0].AggregatorVariable.Equals(variable));
            Assert.IsTrue(modifier.IsEvaluable);
            Assert.IsTrue(modifier.ToString().Equals("GROUP BY ?VAR"));
            Assert.IsNotNull(modifier.QueryMemberStringID);
            Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
            Assert.IsTrue(modifier.Equals(modifier));
            Assert.IsFalse(modifier.Equals(new RDFGroupByModifier(partitionVariables)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGroupByModifierBecauseNullVariables()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGroupByModifier(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGroupByModifierBecauseEmptyVariables()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGroupByModifier(new List<RDFVariable>()));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingGroupByModifierBecauseNullItemInVariables()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFGroupByModifier(new List<RDFVariable>() { null }));

        [TestMethod]
        public void ShouldAddAggregator()
        {
            RDFVariable variable1 = new RDFVariable("?VAR1");
            RDFVariable variable2 = new RDFVariable("?VAR2");
            RDFVariable variable3 = new RDFVariable("?VAR3");
            List<RDFVariable> partitionVariables = new List<RDFVariable>() { variable1 };
            RDFGroupByModifier modifier = new RDFGroupByModifier(partitionVariables);
            RDFAggregator aggregator = new RDFAggregator(variable2, variable3);
            modifier.AddAggregator(aggregator);
            modifier.AddAggregator(null); //Will be discarded, since null aggregators are not allowed

            Assert.IsNotNull(modifier);
            Assert.IsNotNull(modifier.PartitionVariables);
            Assert.IsTrue(modifier.PartitionVariables.Count == 1);
            Assert.IsTrue(modifier.PartitionVariables[0].Equals(variable1));
            Assert.IsNotNull(modifier.Aggregators);
            Assert.IsTrue(modifier.Aggregators.Count == 2);
            Assert.IsTrue(modifier.Aggregators[0].AggregatorVariable.Equals(variable1));
            Assert.IsTrue(modifier.Aggregators[0].ProjectionVariable.Equals(variable1));            
            Assert.IsTrue(modifier.Aggregators[1].AggregatorVariable.Equals(variable2));
            Assert.IsTrue(modifier.Aggregators[1].ProjectionVariable.Equals(variable3));
            Assert.IsTrue(modifier.IsEvaluable);
            Assert.IsTrue(modifier.ToString().Equals("GROUP BY ?VAR1"));
            Assert.IsNotNull(modifier.QueryMemberStringID);
            Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
            Assert.IsTrue(modifier.Equals(modifier));
            Assert.IsFalse(modifier.Equals(new RDFGroupByModifier(partitionVariables).AddAggregator(aggregator)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingAggregatorBecauseDuplicatePartitionVariable()
        {
            RDFVariable variable1 = new RDFVariable("?VAR1");
            RDFVariable variable2 = new RDFVariable("?VAR2");
            RDFVariable variable3 = new RDFVariable("?VAR3");
            List<RDFVariable> partitionVariables = new List<RDFVariable>() { variable1 };
            RDFGroupByModifier modifier = new RDFGroupByModifier(partitionVariables);
            RDFAggregator aggregator = new RDFAggregator(variable2, variable1);

            Assert.ThrowsException<RDFQueryException>(() => modifier.AddAggregator(aggregator));
        }
        #endregion
    }
}