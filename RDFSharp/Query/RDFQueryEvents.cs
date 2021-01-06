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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQueryEvents represents a collector for all the events generated within the "RDFSharp.Query" namespace
    /// </summary>
    public static class RDFQueryEvents
    {

        #region Query

        #region OnASKQueryEvaluation
        /// <summary>
        /// Event representing an information message generated during SPARQL ASK query evaluation
        /// </summary>
        public static event RDFASKQueryEvaluationEventHandler OnASKQueryEvaluation = delegate { };

        /// <summary>
        /// Delegate to handle information events generated during SPARQL ASK query evaluation
        /// </summary>
        public delegate void RDFASKQueryEvaluationEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseASKQueryEvaluation(string eventMessage)
        {
            Parallel.Invoke(() => OnASKQueryEvaluation(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";ASK_QUERY_EVALUATION;" + eventMessage));
        }
        #endregion

        #region OnCONSTRUCTQueryEvaluation
        /// <summary>
        /// Event representing an information message generated during SPARQL CONSTRUCT query evaluation
        /// </summary>
        public static event RDFCONSTRUCTQueryEvaluationEventHandler OnCONSTRUCTQueryEvaluation = delegate { };

        /// <summary>
        /// Delegate to handle information events generated during SPARQL CONSTRUCT query evaluation
        /// </summary>
        public delegate void RDFCONSTRUCTQueryEvaluationEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseCONSTRUCTQueryEvaluation(string eventMessage)
        {
            Parallel.Invoke(() => OnCONSTRUCTQueryEvaluation(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";CONSTRUCT_QUERY_EVALUATION;" + eventMessage));
        }
        #endregion

        #region OnDESCRIBEQueryEvaluation
        /// <summary>
        /// Event representing an information message generated during SPARQL DESCRIBE query evaluation
        /// </summary>
        public static event RDFDESCRIBEQueryEvaluationEventHandler OnDESCRIBEQueryEvaluation = delegate { };

        /// <summary>
        /// Delegate to handle information events generated during SPARQL DESCRIBE query evaluation
        /// </summary>
        public delegate void RDFDESCRIBEQueryEvaluationEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseDESCRIBEQueryEvaluation(string eventMessage)
        {
            Parallel.Invoke(() => OnDESCRIBEQueryEvaluation(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";DESCRIBE_QUERY_EVALUATION;" + eventMessage));
        }
        #endregion

        #region OnSELECTQueryEvaluation
        /// <summary>
        /// Event representing an information message generated during SPARQL SELECT query evaluation
        /// </summary>
        public static event RDFSELECTQueryEvaluationEventHandler OnSELECTQueryEvaluation = delegate { };

        /// <summary>
        /// Delegate to handle information events generated during SPARQL SELECT query evaluation
        /// </summary>
        public delegate void RDFSELECTQueryEvaluationEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseSELECTQueryEvaluation(string eventMessage)
        {
            Parallel.Invoke(() => OnSELECTQueryEvaluation(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";SELECT_QUERY_EVALUATION;" + eventMessage));
        }
        #endregion

        #region OnGENERICQueryEvaluation
        /// <summary>
        /// Event representing an information message generated during SPARQL query evaluation
        /// </summary>
        public static event RDFGENERICQueryEvaluationEventHandler OnGENERICQueryEvaluation = delegate { };

        /// <summary>
        /// Delegate to handle information events generated during SPARQL query evaluation
        /// </summary>
        public delegate void RDFGENERICQueryEvaluationEventHandler(string eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseGENERICQueryEvaluation(string eventMessage)
        {
            Parallel.Invoke(() => OnGENERICQueryEvaluation(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + ";GENERIC_QUERY_EVALUATION;" + eventMessage));
        }
        #endregion

        #endregion

    }

}