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

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFStoreEvents represents a collector for all the events generated within the "RDFSharp.Store" namespace
    /// </summary>
    public static class RDFStoreEvents
    {

        #region Store

        #region OnQuadrupleAdded
        /// <summary>
        /// Event representing an information message generated when a quadruple has been added to a store
        /// </summary>
        public static event RDFOnQuadrupleAddedEventHandler OnQuadrupleAdded = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a quadruple has been added to a store
        /// </summary>
        public delegate void RDFOnQuadrupleAddedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnQuadrupleAdded(string eventMessage)
        {
            Parallel.Invoke(() => OnQuadrupleAdded(DateTime.Now.ToString() + ";QUADRUPLE_ADDED;" + eventMessage));
        }
        #endregion

        #region OnQuadrupleRemoved
        /// <summary>
        /// Event representing an information message generated when a quadruple has been removed to a store
        /// </summary>
        public static event RDFOnQuadrupleRemovedEventHandler OnQuadrupleRemoved = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a quadruple has been removed to a store
        /// </summary>
        public delegate void RDFOnQuadrupleRemovedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnQuadrupleRemoved(string eventMessage)
        {
            Parallel.Invoke(() => OnQuadrupleRemoved(DateTime.Now.ToString() + ";QUADRUPLE_REMOVED;" + eventMessage));
        }
        #endregion

        #region OnStoreCleared
        /// <summary>
        /// Event representing an information message generated when a store has been cleared
        /// </summary>
        public static event RDFOnStoreClearedEventHandler OnStoreCleared = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a store has been cleared
        /// </summary>
        public delegate void RDFOnStoreClearedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreCleared(string eventMessage)
        {
            Parallel.Invoke(() => OnStoreCleared(DateTime.Now.ToString() + ";STORE_CLEARED;" + eventMessage));
        }
        #endregion

        #region OnStoreInitialized
        /// <summary>
        /// Event representing an information message generated when a store has been initialized
        /// </summary>
        public static event RDFOnStoreInitializedEventHandler OnStoreInitialized = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a store has been initialized
        /// </summary>
        public delegate void RDFOnStoreInitializedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreInitialized(string eventMessage)
        {
            Parallel.Invoke(() => OnStoreInitialized(DateTime.Now.ToString() + ";STORE_INITIALIZED;" + eventMessage));
        }
        #endregion

        #region OnStoreOptimized
        /// <summary>
        /// Event representing an information message generated when a store has been optimized
        /// </summary>
        public static event RDFOnStoreOptimizedEventHandler OnStoreOptimized = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a store has been optimized
        /// </summary>
        public delegate void RDFOnStoreOptimizedEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreOptimized(string eventMessage)
        {
            Parallel.Invoke(() => OnStoreOptimized(DateTime.Now.ToString() + ";STORE_OPTIMIZED;" + eventMessage));
        }
        #endregion

        #endregion

    }

}