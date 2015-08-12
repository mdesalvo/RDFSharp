/*
   Copyright 2012-2015 Marco De Salvo

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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQueryEnums represents a collector for all the enumerations used by the "RDFSharp.Query" namespace
    /// </summary>
    public static class RDFQueryEnums {

        /// <summary>
        /// RDFPatternHoles represents an enumeration for possible positions of holes in a pattern.
        /// </summary>
        internal enum RDFPatternHoles    { C, S, P, O, CS, CP, CO, CSP, CSO, CPO, CSPO, SP, SO, PO, SPO };

        /// <summary>
        /// RDFOrderByFlavors represents an enumeration for possible directions of query results ordering on a given variable.
        /// </summary>
        public enum RDFOrderByFlavors    { ASC, DESC };

        /// <summary>
        /// RDFComparisonFlavors represents an enumeration for possible comparison modes between two patten members.
        /// </summary>
        public enum RDFComparisonFlavors { LessOrEqualThan, LessThan, EqualTo, NotEqualTo, GreaterThan, GreaterOrEqualThan };

    }

}