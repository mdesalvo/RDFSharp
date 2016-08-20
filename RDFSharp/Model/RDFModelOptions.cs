/*
   Copyright 2012-2016 Marco De Salvo

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

namespace RDFSharp.Model 
{

    /// <summary>
    /// RDFModelOptions centralizes configuration and customization of behavior of many aspects of the "RDFSharp.Model" sublibrary
    /// </summary>
    public static class RDFModelOptions {

        #region Properties
        /// <summary>
        /// List of accepted string representations for non-XSD boolean TRUE
        /// </summary>
        internal static List<String> BooleanTrueAlternatives { get; set; }

        /// <summary>
        /// List of accepted string representations for non-XSD boolean FALSE
        /// </summary>
        internal static List<String> BooleanFalseAlternatives { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Static-ctor to initialize the model options configuration
        /// </summary>
        static RDFModelOptions() {
            BooleanTrueAlternatives  = new List<String>() { "t", "true",  "y", "yes", "on",  "1" };
            BooleanFalseAlternatives = new List<String>() { "f", "false", "n", "no",  "off", "0" };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given alternative representation (trimmed and lowered) for non-XSD boolean TRUE.
        /// </summary>
        public static void AddBooleanTrueAlternative(String booleanValue) {
            if (booleanValue != null && booleanValue.Trim() != String.Empty) {
                if (!BooleanFalseAlternatives.Contains(booleanValue.Trim().ToLowerInvariant())) {
                     if (!BooleanTrueAlternatives.Contains(booleanValue.Trim().ToLowerInvariant())) {
                          BooleanTrueAlternatives.Add(booleanValue.Trim().ToLowerInvariant());
                     }
                }
            }
        }

        /// <summary>
        /// Adds the given alternative representation (trimmed and lowered) for non-XSD boolean FALSE.
        /// </summary>
        public static void AddBooleanFalseAlternative(String booleanValue) {
            if (booleanValue != null && booleanValue.Trim() != String.Empty) {
                if (!BooleanTrueAlternatives.Contains(booleanValue.Trim().ToLowerInvariant())) {
                     if (!BooleanFalseAlternatives.Contains(booleanValue.Trim().ToLowerInvariant())) {
                          BooleanFalseAlternatives.Add(booleanValue.Trim().ToLowerInvariant());
                     }
                }
            }
        }
        #endregion

    }

}