/*
   Copyright 2012-2019 Marco De Salvo

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
    public static class RDFQueryEvents {

        #region Query

        #region OnASKQueryEvaluation
        /// <summary>
        /// Event representing an information message generated during SPARQL ASK query evaluation
        /// </summary>
        public static event RDFASKQueryEvaluationEventHandler OnASKQueryEvaluation = delegate { };

        /// <summary>
        /// Delegate to handle information events generated during SPARQL ASK query evaluation
        /// </summary>
        public delegate void RDFASKQueryEvaluationEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseASKQueryEvaluation(String eventMessage) {
            Parallel.Invoke(() => OnASKQueryEvaluation(DateTime.Now.ToString() + ";ASK_QUERY_EVALUATION;" + eventMessage));
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
        public delegate void RDFCONSTRUCTQueryEvaluationEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseCONSTRUCTQueryEvaluation(String eventMessage) {
            Parallel.Invoke(() => OnCONSTRUCTQueryEvaluation(DateTime.Now.ToString() + ";CONSTRUCT_QUERY_EVALUATION;" + eventMessage));
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
        public delegate void RDFDESCRIBEQueryEvaluationEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseDESCRIBEQueryEvaluation(String eventMessage) {
            Parallel.Invoke(() => OnDESCRIBEQueryEvaluation(DateTime.Now.ToString() + ";DESCRIBE_QUERY_EVALUATION;" + eventMessage));
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
        public delegate void RDFSELECTQueryEvaluationEventHandler(String eventMessage);

        /// <summary>
        /// Internal invoker of the subscribed information event handler
        /// </summary>
        internal static void RaiseSELECTQueryEvaluation(String eventMessage) {
            Parallel.Invoke(() => OnSELECTQueryEvaluation(DateTime.Now.ToString() + ";SELECT_QUERY_EVALUATION;" + eventMessage));
        }
        #endregion

        #endregion

    }

}