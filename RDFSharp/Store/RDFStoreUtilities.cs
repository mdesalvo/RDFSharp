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
        /// Empty list of quadruples to be returned in case of no query results
        /// </summary>
        internal static readonly List<RDFQuadruple> EmptyQuadrupleList = new List<RDFQuadruple>();

        /// <summary>
        /// Parses the current quadruple of the data reader
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        public static RDFQuadruple ParseQuadruple(IDataReader fetchedQuadruples)
        {
            #region Guards
            if (fetchedQuadruples == null)
                throw new RDFStoreException("Cannot parse quadruple because given \"fetchedQuadruples\" parameter is null.");
            #endregion

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
            //Detect presence of semantically valid datatype indicator ("^^")
            int lastIndexOfDatatype = literal.LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
            if (lastIndexOfDatatype == -1
                 || lastIndexOfDatatype == literal.Length - 2 //EndsWith "^^"
                 || RDFModelUtilities.GetUriFromString(literal.Substring(lastIndexOfDatatype + 2)) == null)
            {
                RDFPlainLiteral pLit;
                if (RDFNTriples.regexLPL.Value.Match(literal).Success)
                {
                    int lastIndexOfLanguage = literal.LastIndexOf('@');
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
        internal static List<RDFQuadruple> SelectQuadruples(RDFMemoryStore store, RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit)
        {
            #region Utilities
            void LookupIndex(HashSet<long> lookup, out List<RDFHashedQuadruple> result)
            {
                result = new List<RDFHashedQuadruple>(lookup.Count);
                result.AddRange(lookup.Select(q => store.Index.Hashes[q]));
            }
            #endregion

            if (store != null)
            {
                StringBuilder queryFilters = new StringBuilder(4);
                List<RDFHashedQuadruple> C=null, S=null, P=null, O=null, L=null, hashedQuadruples;

                //Filter by Context
                if (ctx != null)
                {
                    queryFilters.Append('C');
                    LookupIndex(store.Index.LookupIndexByContext(ctx), out C);
                }

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append('S');
                    LookupIndex(store.Index.LookupIndexBySubject(subj), out S);
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append('P');
                    LookupIndex(store.Index.LookupIndexByPredicate(pred), out P);
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append('O');
                    LookupIndex(store.Index.LookupIndexByObject(obj), out O);
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append('L');
                    LookupIndex(store.Index.LookupIndexByLiteral(lit), out L);
                }

                //Intersect the filters
                switch (queryFilters.ToString())
                {
                    case "C":
                        hashedQuadruples = C;
                        break;
                    case "S":
                        hashedQuadruples = S;
                        break;
                    case "P":
                        hashedQuadruples = P;
                        break;
                    case "O":
                        hashedQuadruples = O;
                        break;
                    case "L":
                        hashedQuadruples = L;
                        break;
                    case "CS":
                        hashedQuadruples = C.Intersect(S).ToList();
                        break;
                    case "CP":
                        hashedQuadruples = C.Intersect(P).ToList();
                        break;
                    case "CO":
                        hashedQuadruples = C.Intersect(O).ToList();
                        break;
                    case "CL":
                        hashedQuadruples = C.Intersect(L).ToList();
                        break;
                    case "CSP":
                        hashedQuadruples = C.Intersect(S).Intersect(P).ToList();
                        break;
                    case "CSO":
                        hashedQuadruples = C.Intersect(S).Intersect(O).ToList();
                        break;
                    case "CSL":
                        hashedQuadruples = C.Intersect(S).Intersect(L).ToList();
                        break;
                    case "CPO":
                        hashedQuadruples = C.Intersect(P).Intersect(O).ToList();
                        break;
                    case "CPL":
                        hashedQuadruples = C.Intersect(P).Intersect(L).ToList();
                        break;
                    case "CSPO":
                        hashedQuadruples = C.Intersect(S).Intersect(P).Intersect(O).ToList();
                        break;
                    case "CSPL":
                        hashedQuadruples = C.Intersect(S).Intersect(P).Intersect(L).ToList();
                        break;
                    case "SP":
                        hashedQuadruples = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        hashedQuadruples = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        hashedQuadruples = S.Intersect(L).ToList();
                        break;
                    case "SPO":
                        hashedQuadruples = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        hashedQuadruples = S.Intersect(P).Intersect(L).ToList();
                        break;
                    case "PO":
                        hashedQuadruples = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        hashedQuadruples = P.Intersect(L).ToList();
                        break;
                    default:
                        hashedQuadruples = store.Index.Hashes.Values.ToList();
                        break;
                }

                //Decompress hashed quadruples
                return hashedQuadruples.ConvertAll(hq => new RDFQuadruple(hq, store.Index));
            }
            return EmptyQuadrupleList;
        }
        #endregion
    }
}