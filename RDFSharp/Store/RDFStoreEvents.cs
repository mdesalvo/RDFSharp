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
        public delegate void RDFOnQuadrupleAddedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnQuadrupleAdded(String eventMessage)
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
        public delegate void RDFOnQuadrupleRemovedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnQuadrupleRemoved(String eventMessage)
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
        public delegate void RDFOnStoreClearedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreCleared(String eventMessage)
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
        public delegate void RDFOnStoreInitializedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreInitialized(String eventMessage)
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
        public delegate void RDFOnStoreOptimizedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreOptimized(String eventMessage)
        {
            Parallel.Invoke(() => OnStoreOptimized(DateTime.Now.ToString() + ";STORE_OPTIMIZED;" + eventMessage));
        }
        #endregion

        #endregion

        #region Federation

        #region OnStoreAdded
        /// <summary>
        /// Event representing an information message generated when a store has been added to a federation
        /// </summary>
        public static event RDFOnStoreAddedEventHandler OnStoreAdded = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a store has been added to a federation
        /// </summary>
        public delegate void RDFOnStoreAddedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreAdded(String eventMessage)
        {
            Parallel.Invoke(() => OnStoreAdded(DateTime.Now.ToString() + ";STORE_ADDED;" + eventMessage));
        }
        #endregion

        #region OnStoreRemoved
        /// <summary>
        /// Event representing an information message generated when a store has been removed from a federation
        /// </summary>
        public static event RDFOnStoreRemovedEventHandler OnStoreRemoved = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a store has been removed from a federation
        /// </summary>
        public delegate void RDFOnStoreRemovedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnStoreRemoved(String eventMessage)
        {
            Parallel.Invoke(() => OnStoreRemoved(DateTime.Now.ToString() + ";STORE_REMOVED;" + eventMessage));
        }
        #endregion

        #region OnFederationCleared
        /// <summary>
        /// Event representing an information message generated when a federation has been cleared
        /// </summary>
        public static event RDFOnFederationClearedEventHandler OnFederationCleared = delegate { };

        /// <summary>
        /// Delegate to handle information events generated when a federation has been cleared
        /// </summary>
        public delegate void RDFOnFederationClearedEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseOnFederationCleared(String eventMessage)
        {
            Parallel.Invoke(() => OnFederationCleared(DateTime.Now.ToString() + ";FEDERATION_CLEARED;" + eventMessage));
        }
        #endregion

        #endregion

    }

}