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
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFVariableTest
{
    #region Tests
    [DataTestMethod]
    [DataRow("var")]
    [DataRow("vaR")]
    [DataRow("?var")]
    [DataRow(" ?var ")]
    [DataRow("?var?")]
    [DataRow("$var$")]
    public void ShouldCreateVariable(string variableName)
    {
        string effectiveVariableName = $"?{variableName.Trim(' ', '?', '$').ToUpperInvariant()}";
        RDFVariable variable1 = new RDFVariable(variableName);

        Assert.IsNotNull(variable1);
        Assert.IsTrue(variable1.VariableName.Equals(effectiveVariableName));
        Assert.IsTrue(variable1.ToString().Equals(effectiveVariableName));

        RDFVariable variable2 = new RDFVariable(variableName.ToUpperInvariant());

        Assert.IsNotNull(variable2);
        Assert.IsTrue(variable2.VariableName.Equals(effectiveVariableName));
        Assert.IsTrue(variable2.ToString().Equals(effectiveVariableName));

        Assert.IsTrue(variable1.Equals(variable2));
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow("??")]
    [DataRow("$$")]
    [DataRow("?  $")]
    public void ShouldThrowExceptionOnCreatingVariable(string varname)
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFVariable(varname));
    #endregion
}