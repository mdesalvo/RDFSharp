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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RDFSharp.Model;

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
            if (string.Equals(fetchedQuadruples["TripleFlavor"].ToString(), "1"))
            {
                RDFResource qObject = new RDFResource(fetchedQuadruples["Object"].ToString());
                return new RDFQuadruple(qContext, qSubject, qPredicate, qObject);
            }

            //SPL-flavour quadruple
            string literal = fetchedQuadruples["Object"].ToString();

            //PlainLiteral
            int lastIndexOfDatatype = literal.LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
            if (!literal.Contains("^^")
                  || literal.EndsWith("^^", StringComparison.Ordinal)
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
                {
                    pLit = new RDFPlainLiteral(literal);
                }

                return new RDFQuadruple(qContext, qSubject, qPredicate, pLit);
            }

            //TypedLiteral
            string tLitValue = literal.Substring(0, lastIndexOfDatatype);
            string tLitDatatype = literal.Substring(lastIndexOfDatatype + 2);
            RDFTypedLiteral tLit = new RDFTypedLiteral(tLitValue, RDFDatatypeRegister.GetDatatype(tLitDatatype));
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
                List<RDFHashedQuadruple> C = new List<RDFHashedQuadruple>();
                List<RDFHashedQuadruple> S = new List<RDFHashedQuadruple>();
                List<RDFHashedQuadruple> P = new List<RDFHashedQuadruple>();
                List<RDFHashedQuadruple> O = new List<RDFHashedQuadruple>();
                List<RDFHashedQuadruple> L = new List<RDFHashedQuadruple>();
                List<RDFHashedQuadruple> matchResultHashedQuadruples;
                StringBuilder queryFilters = new StringBuilder();

                //Filter by Context
                if (ctx != null)
                {
                    queryFilters.Append('C');
                    C.AddRange(store.Index.LookupIndexByContext(ctx).Select(q => store.Index.Hashes[q]));
                }

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append('S');
                    S.AddRange(store.Index.LookupIndexBySubject(subj).Select(q => store.Index.Hashes[q]));
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append('P');
                    P.AddRange(store.Index.LookupIndexByPredicate(pred).Select(q => store.Index.Hashes[q]));
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append('O');
                    O.AddRange(store.Index.LookupIndexByObject(obj).Select(q => store.Index.Hashes[q]));
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append('L');
                    L.AddRange(store.Index.LookupIndexByLiteral(lit).Select(q => store.Index.Hashes[q]));
                }

                //Intersect the filters
                switch (queryFilters.ToString())
                {
                    case "C":
                        matchResultHashedQuadruples = C;
                        break;
                    case "S":
                        matchResultHashedQuadruples = S;
                        break;
                    case "P":
                        matchResultHashedQuadruples = P;
                        break;
                    case "O":
                        matchResultHashedQuadruples = O;
                        break;
                    case "L":
                        matchResultHashedQuadruples = L;
                        break;
                    case "CS":
                        matchResultHashedQuadruples = C.Intersect(S).ToList();
                        break;
                    case "CP":
                        matchResultHashedQuadruples = C.Intersect(P).ToList();
                        break;
                    case "CO":
                        matchResultHashedQuadruples = C.Intersect(O).ToList();
                        break;
                    case "CL":
                        matchResultHashedQuadruples = C.Intersect(L).ToList();
                        break;
                    case "CSP":
                        matchResultHashedQuadruples = C.Intersect(S).Intersect(P).ToList();
                        break;
                    case "CSO":
                        matchResultHashedQuadruples = C.Intersect(S).Intersect(O).ToList();
                        break;
                    case "CSL":
                        matchResultHashedQuadruples = C.Intersect(S).Intersect(L).ToList();
                        break;
                    case "CPO":
                        matchResultHashedQuadruples = C.Intersect(P).Intersect(O).ToList();
                        break;
                    case "CPL":
                        matchResultHashedQuadruples = C.Intersect(P).Intersect(L).ToList();
                        break;
                    case "CSPO":
                        matchResultHashedQuadruples = C.Intersect(S).Intersect(P).Intersect(O).ToList();
                        break;
                    case "CSPL":
                        matchResultHashedQuadruples = C.Intersect(S).Intersect(P).Intersect(L).ToList();
                        break;
                    case "SP":
                        matchResultHashedQuadruples = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        matchResultHashedQuadruples = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        matchResultHashedQuadruples = S.Intersect(L).ToList();
                        break;
                    case "SPO":
                        matchResultHashedQuadruples = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        matchResultHashedQuadruples = S.Intersect(P).Intersect(L).ToList();
                        break;
                    case "PO":
                        matchResultHashedQuadruples = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        matchResultHashedQuadruples = P.Intersect(L).ToList();
                        break;
                    default:
                        matchResultHashedQuadruples = store.Index.Hashes.Values.ToList();
                        break;
                }

                //Decompress hashed quadruples
                matchResultHashedQuadruples.ForEach(hashedQuadruple => matchResult.Add(new RDFQuadruple(hashedQuadruple, store.Index)));
            }
            return matchResult;
        }
        #endregion
    }
}