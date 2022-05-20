/*
   Copyright 2012-2022 Marco De Salvo

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
    internal static class RDFStoreUtilities
    {
        #region Select
        /// <summary>
        /// Parses the current quadruple of the data reader
        /// </summary>
        internal static RDFQuadruple ParseQuadruple(IDataReader fetchedQuadruples)
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
                RDFPlainLiteral pLit = null;
                if (RDFNTriples.regexLPL.Value.Match(literal).Success)
                {
                    int lastIndexOfLanguage = literal.LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
                    string pLitValue = literal.Substring(0, lastIndexOfLanguage);
                    string pLitLang = literal.Substring(lastIndexOfLanguage + 1);
                    pLit = new RDFPlainLiteral(pLitValue, pLitLang);
                }
                else
                {
                    pLit = new RDFPlainLiteral(literal);
                }
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
        internal static List<RDFQuadruple> SelectQuadruples(RDFMemoryStore store, RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit)
        {
            List<RDFQuadruple> matchResult = new List<RDFQuadruple>();
            if (store != null)
            {
                List<RDFQuadruple> C = new List<RDFQuadruple>();
                List<RDFQuadruple> S = new List<RDFQuadruple>();
                List<RDFQuadruple> P = new List<RDFQuadruple>();
                List<RDFQuadruple> O = new List<RDFQuadruple>();
                List<RDFQuadruple> L = new List<RDFQuadruple>();
                StringBuilder queryFilters = new StringBuilder();

                //Filter by Context
                if (ctx != null)
                {
                    queryFilters.Append("C");
                    foreach (long q in store.StoreIndex.SelectIndexByContext(ctx))
                        C.Add(store.Quadruples[q]);
                }

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append("S");
                    foreach (long q in store.StoreIndex.SelectIndexBySubject(subj))
                        S.Add(store.Quadruples[q]);
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append("P");
                    foreach (long q in store.StoreIndex.SelectIndexByPredicate(pred))
                        P.Add(store.Quadruples[q]);
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append("O");
                    foreach (long q in store.StoreIndex.SelectIndexByObject(obj))
                        O.Add(store.Quadruples[q]);
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append("L");
                    foreach (var q in store.StoreIndex.SelectIndexByLiteral(lit))
                        L.Add(store.Quadruples[q]);
                }

                //Intersect the filters
                string queryFilter = queryFilters.ToString();
                switch (queryFilter)
                {
                    case "C":
                        matchResult = C;
                        break;
                    case "S":
                        matchResult = S;
                        break;
                    case "P":
                        matchResult = P;
                        break;
                    case "O":
                        matchResult = O;
                        break;
                    case "L":
                        matchResult = L;
                        break;
                    case "CS":
                        matchResult = C.Intersect(S).ToList();
                        break;
                    case "CP":
                        matchResult = C.Intersect(P).ToList();
                        break;
                    case "CO":
                        matchResult = C.Intersect(O).ToList();
                        break;
                    case "CL":
                        matchResult = C.Intersect(L).ToList();
                        break;
                    case "CSP":
                        matchResult = C.Intersect(S).Intersect(P).ToList();
                        break;
                    case "CSO":
                        matchResult = C.Intersect(S).Intersect(O).ToList();
                        break;
                    case "CSL":
                        matchResult = C.Intersect(S).Intersect(L).ToList();
                        break;
                    case "CPO":
                        matchResult = C.Intersect(P).Intersect(O).ToList();
                        break;
                    case "CPL":
                        matchResult = C.Intersect(P).Intersect(L).ToList();
                        break;
                    case "CSPO":
                        matchResult = C.Intersect(S).Intersect(P).Intersect(O).ToList();
                        break;
                    case "CSPL":
                        matchResult = C.Intersect(S).Intersect(P).Intersect(L).ToList();
                        break;
                    case "SP":
                        matchResult = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        matchResult = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        matchResult = S.Intersect(L).ToList();
                        break;
                    case "SPO":
                        matchResult = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        matchResult = S.Intersect(P).Intersect(L).ToList();
                        break;
                    case "PO":
                        matchResult = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        matchResult = P.Intersect(L).ToList();
                        break;
                    default:
                        matchResult = store.ToList();
                        break;
                }
            }
            return matchResult;
        }
        #endregion
    }
}