/*
   Copyright 2012-2020 Marco De Salvo

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
using System.Threading.Tasks;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFModelEvents represents a collector for all the events generated within the "RDFSharp.Model" namespace
    /// </summary>
    public static class RDFModelEvents
    {

        #region Graph

        #region OnTripleAdded
        /// <summary>
        /// Event representing an information message generated when a triple has been added to a graph
        /// </summary>
        public static event RDFOnTripleAddedEventHandler OnTripleAdded = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a triple has been added to a graph
        /// </summary>
        public delegate void RDFOnTripleAddedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnTripleAdded(string eventMessage)
        {
            Parallel.Invoke(() => OnTripleAdded(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";TRIPLE_ADDED;" + eventMessage));
        }
        #endregion

        #region OnTripleRemoved
        /// <summary>
        /// Event representing an information message generated when a triple has been removed from a graph
        /// </summary>
        public static event RDFOnTripleRemovedEventHandler OnTripleRemoved = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a triple has been removed from a graph
        /// </summary>
        public delegate void RDFOnTripleRemovedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnTripleRemoved(string eventMessage)
        {
            Parallel.Invoke(() => OnTripleRemoved(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";TRIPLE_REMOVED;" + eventMessage));
        }
        #endregion

        #region OnGraphCleared
        /// <summary>
        /// Event representing an information message generated when a graph has been cleared
        /// </summary>
        public static event RDFOnGraphClearedEventHandler OnGraphCleared = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a graph has been cleared
        /// </summary>
        public delegate void RDFOnGraphClearedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnGraphCleared(string eventMessage)
        {
            Parallel.Invoke(() => OnGraphCleared(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";GRAPH_CLEARED;" + eventMessage));
        }
        #endregion

        #endregion

    }

}