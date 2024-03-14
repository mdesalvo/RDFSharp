/*
   Copyright 2012-2024 Marco De Salvo

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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFStoreUtilities is a collector of reusable utility methods for RDF store management
    /// </summary>
    public static class RDFStoreUtilities
    {
        #region Select
        /// <summary>
        /// Parses the current quadruple of the data reader
        /// </summary>
        public static RDFQuadruple ParseQuadruple(IDataReader fetchedQuadruples)
        {
            if (fetchedQuadruples == null)
                throw new RDFStoreException("Cannot parse quadruple because given \"fetchedQuadruples\" parameter is null.");
            
            RDFContext qContext = new RDFContext(fetchedQuadruples["Context"].ToString());
            RDFResource qSubject = new RDFResource(fetchedQuadruples["Subject"].ToString());
            RDFResource qPredicate = new RDFResource(fetchedQuadruples["Predicate"].ToString());

            //SPO-flavour quadruple
            if (fetchedQuadruples["TripleFlavor"].ToString().Equals("1"))
            {
                RDFResource qObject = new RDFResource(fetchedQuadruples["Object"].ToString());
                return new RDFQuadruple(qContext, qSubject, qPredicate, qObject);
            }

            //SPL-flavour quadruple
            string literal = fetchedQuadruples["Object"].ToString();

            //PlainLiteral
            int lastIndexOfDatatype = literal.LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
            if (!literal.Contains("^^")
                  || literal.EndsWith("^^")
                  || RDFModelUtilities.GetUriFromString(literal.Substring(lastIndexOfDatatype + 2)) == null)
            {
                RDFPlainLiteral pLit;
                if (RDFNTriples.regexLPL.Value.Match(literal).Success)
                {
                    int lastIndexOfLanguage = literal.LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
                    string pLitValue = literal.Substring(0, lastIndexOfLanguage);
                    string pLitLang = literal.Substring(lastIndexOfLanguage + 1);
                    pLit = new RDFPlainLiteral(pLitValue, pLitLang);
                }
                else
                    pLit = new RDFPlainLiteral(literal);
                return new RDFQuadruple(qContext, qSubject, qPredicate, pLit);
            }

            //TypedLiteral
            string tLitValue = literal.Substring(0, lastIndexOfDatatype);
            string tLitDatatype = literal.Substring(lastIndexOfDatatype + 2);
            RDFModelEnums.RDFDatatypes dt = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
            RDFTypedLiteral tLit = new RDFTypedLiteral(tLitValue, dt);
            return new RDFQuadruple(qContext, qSubject, qPredicate, tLit);
        }

        /// <summary>
        /// Selects the quadruples corresponding to the given pattern from the given store
        /// </summary>
        internal static List<RDFQuadruple> SelectQuadruples(RDFMemoryStore store, RDFContext ctx, RDFResource subj, 
            RDFResource pred, RDFResource obj, RDFLiteral lit)
        {
            List<RDFQuadruple> matchResult = new List<RDFQuadruple>();
            if (store != null)
            {
                List<RDFIndexedQuadruple> C = new List<RDFIndexedQuadruple>();
                List<RDFIndexedQuadruple> S = new List<RDFIndexedQuadruple>();
                List<RDFIndexedQuadruple> P = new List<RDFIndexedQuadruple>();
                List<RDFIndexedQuadruple> O = new List<RDFIndexedQuadruple>();
                List<RDFIndexedQuadruple> L = new List<RDFIndexedQuadruple>();
                List<RDFIndexedQuadruple> matchResultIndexedQuadruples = new List<RDFIndexedQuadruple>();
                StringBuilder queryFilters = new StringBuilder();

                //Filter by Context
                if (ctx != null)
                {
                    queryFilters.Append('C');
                    foreach (long q in store.StoreIndex.SelectIndexByContext(ctx))
                        C.Add(store.IndexedQuadruples[q]);
                }

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append('S');
                    foreach (long q in store.StoreIndex.SelectIndexBySubject(subj))
                        S.Add(store.IndexedQuadruples[q]);
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append('P');
                    foreach (long q in store.StoreIndex.SelectIndexByPredicate(pred))
                        P.Add(store.IndexedQuadruples[q]);
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append('O');
                    foreach (long q in store.StoreIndex.SelectIndexByObject(obj))
                        O.Add(store.IndexedQuadruples[q]);
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append('L');
                    foreach (long q in store.StoreIndex.SelectIndexByLiteral(lit))
                        L.Add(store.IndexedQuadruples[q]);
                }

                //Intersect the filters
                switch (queryFilters.ToString())
                {
                    case "C":
                        matchResultIndexedQuadruples = C;
                        break;
                    case "S":
                        matchResultIndexedQuadruples = S;
                        break;
                    case "P":
                        matchResultIndexedQuadruples = P;
                        break;
                    case "O":
                        matchResultIndexedQuadruples = O;
                        break;
                    case "L":
                        matchResultIndexedQuadruples = L;
                        break;
                    case "CS":
                        matchResultIndexedQuadruples = C.Intersect(S).ToList();
                        break;
                    case "CP":
                        matchResultIndexedQuadruples = C.Intersect(P).ToList();
                        break;
                    case "CO":
                        matchResultIndexedQuadruples = C.Intersect(O).ToList();
                        break;
                    case "CL":
                        matchResultIndexedQuadruples = C.Intersect(L).ToList();
                        break;
                    case "CSP":
                        matchResultIndexedQuadruples = C.Intersect(S).Intersect(P).ToList();
                        break;
                    case "CSO":
                        matchResultIndexedQuadruples = C.Intersect(S).Intersect(O).ToList();
                        break;
                    case "CSL":
                        matchResultIndexedQuadruples = C.Intersect(S).Intersect(L).ToList();
                        break;
                    case "CPO":
                        matchResultIndexedQuadruples = C.Intersect(P).Intersect(O).ToList();
                        break;
                    case "CPL":
                        matchResultIndexedQuadruples = C.Intersect(P).Intersect(L).ToList();
                        break;
                    case "CSPO":
                        matchResultIndexedQuadruples = C.Intersect(S).Intersect(P).Intersect(O).ToList();
                        break;
                    case "CSPL":
                        matchResultIndexedQuadruples = C.Intersect(S).Intersect(P).Intersect(L).ToList();
                        break;
                    case "SP":
                        matchResultIndexedQuadruples = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        matchResultIndexedQuadruples = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        matchResultIndexedQuadruples = S.Intersect(L).ToList();
                        break;
                    case "SPO":
                        matchResultIndexedQuadruples = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        matchResultIndexedQuadruples = S.Intersect(P).Intersect(L).ToList();
                        break;
                    case "PO":
                        matchResultIndexedQuadruples = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        matchResultIndexedQuadruples = P.Intersect(L).ToList();
                        break;
                    default:
                        matchResultIndexedQuadruples = store.IndexedQuadruples.Values.ToList();
                        break;
                }

                //Decompress indexed quadruples
                matchResultIndexedQuadruples.ForEach(indexedQuadruple => matchResult.Add(new RDFQuadruple(indexedQuadruple, store.StoreIndex)));
            }
            return matchResult;
        }
        #endregion
    }
}