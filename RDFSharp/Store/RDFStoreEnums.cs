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

namespace RDFSharp.Store;

/// <summary>
/// RDFStoreEnums represents a collector for all the enumerations used by the "RDFSharp.Store" namespace
/// </summary>
public static class RDFStoreEnums
{
    /// <summary>
    /// RDFStoreSQLErrors represents an enumeration for situations which can be found on a SQL-backing store
    /// </summary>
    public enum RDFStoreSQLErrors
    {
        /// <summary>
        /// Indicates that diagnostics on the selected database has passed
        /// </summary>
        NoErrors = 0,
        /// <summary>
        /// Indicates that diagnostics on the selected database has not passed because of a connection error
        /// </summary>
        InvalidDataSource = 1,
        /// <summary>
        /// Indicates that diagnostics on the selected database has not passed because it's not ready for use with RDFSharp
        /// </summary>
        QuadruplesTableNotFound = 2
    }

    /// <summary>
    /// RDFFormats represents an enumeration for supported RDF store serialization data formats.
    /// </summary>
    public enum RDFFormats
    {
        /// <summary>
        /// N-Quads serialization (https://www.w3.org/TR/n-quads/)
        /// </summary>
        NQuads = 1,
        /// <summary>
        /// TriX serialization (http://www.w3.org/2004/03/trix/trix-1/)
        /// </summary>
        TriX = 2,
        /// <summary>
        /// TriG serialization (https://www.w3.org/TR/trig/)
        /// </summary>
        TriG = 3
    }
}