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

using System.Diagnostics.CodeAnalysis;

namespace RDFSharp
{
    /// <summary>
    /// RDFConfiguration represents an handy facility for fine-tuning specific technical aspects of RDFSharp
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class RDFConfiguration
    {
        /// <summary>
        /// Initial capacity of the dictionaries storing computed hashes of triples/quadruples.<br/><br/>
        /// Default value is 131, which helps at saving up to 131 triples/quadruples in a graph/store<br/>
        /// until letting the Garbage Collector trigger the automatic (and expensive) internal resizes.<br/><br/>
        /// If you need a more aggressive value, to get even better performances, ensure to give a prime number.
        /// </summary>
        public static int InitialCapacityOfHashes { get; set; } = 131;

        /// <summary>
        /// Initial capacity of the dictionaries storing instances of resource/literal.<br/><br/>
        /// Default value is 131, which helps at saving up to 131 resources and 131 literals in a graph/store<br/>
        /// until letting the Garbage Collector trigger the automatic (and expensive) internal resizes.<br/><br/>
        /// If you need a more aggressive value, to get even better performances, ensure to give a prime number.
        /// </summary>
        public static int InitialCapacityOfRegisters { get; set; } = 131;

        /// <summary>
        /// Initial capacity of the dictionaries storing S-P-O-L indexes.<br/><br/>
        /// Default value is 131, which helps at indexing up to 131 subjects, predicates, objects and literals in a graph/store
        /// until letting the Garbage Collector trigger the automatic (and expensive) internal resizes.<br/><br/>
        /// If you need a more aggressive value, to get even better performances, ensure to give a prime number.
        /// </summary>
        public static int InitialCapacityOfIndexes { get; set; } = 131;
    }
}