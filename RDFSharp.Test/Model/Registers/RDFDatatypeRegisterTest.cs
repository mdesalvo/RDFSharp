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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFDatatypeRegisterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldInitializeRegister()
        {
            Assert.IsTrue(RDFDatatypeRegister.DatatypesCount >= 50);

            int i=0;
            IEnumerator<RDFDatatype> datatypes = RDFDatatypeRegister.DatatypesEnumerator;
            while(datatypes.MoveNext())
                i++;
            Assert.IsTrue(i >= 50);
        }

        [TestMethod]
        public void ShouldGetDatatype()
        {
            Assert.IsNotNull(RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.INTEGER.ToString()));
            Assert.IsNotNull(RDFDatatypeRegister.GetDatatype(RDFModelEnums.RDFDatatypes.XSD_INTEGER));
            //Test that these are built-in datatypes
            Assert.IsTrue(RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.INTEGER.ToString()).IsBuiltIn);
        }

        [TestMethod]
        public void ShouldNotGetDatatype()
        {
            Assert.IsNull(RDFDatatypeRegister.GetDatatype(null));
            Assert.IsNull(RDFDatatypeRegister.GetDatatype("ex:dt"));
        }

        [TestMethod]
        public void ShouldAddDatatype()
        {
            RDFDatatypeRegister.AddDatatype(new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                new RDFLengthFacet(6) ]));
            RDFDatatypeRegister.AddDatatype(new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
                new RDFLengthFacet(6) ])); //This will not be added again, since we avoid duplicates
            RDFDatatypeRegister.AddDatatype(null); //This will not be added, since we avoid nulls

            Assert.IsTrue(RDFDatatypeRegister.DatatypesCount >= 51);
            Assert.IsNotNull(RDFDatatypeRegister.GetDatatype("ex:length6"));
            //Test that this isn't built-in datatype
            Assert.IsFalse(RDFDatatypeRegister.GetDatatype("ex:length6").IsBuiltIn);
        }
        #endregion
    }
}