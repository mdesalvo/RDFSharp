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

using System;

namespace RDFSharp.Model {

    /// <summary>
    /// RDFModelEvents represents a collector for all the events generated within the "RDFSharp.Model" namespace
    /// </summary>
    public static class RDFModelEvents {

        #region OnModelWarning
        /// <summary>
        /// Event representing a warning message generated within the "RDFSharp.Model" namespace
        /// </summary>
        public static event RDFModelWarningEventHandler OnModelWarning = delegate { };

        /// <summary>
        /// Delegate to handle warning events generated within the "RDFSharp.Model" namespace
        /// </summary>
        public delegate void RDFModelWarningEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the warning event handler
        /// </summary>
        internal static void RaiseModelWarning(String eventMessage) {
            RDFModelEvents.OnModelWarning(eventMessage);
        }
        #endregion

    }

}