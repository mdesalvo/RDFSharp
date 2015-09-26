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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFStoreUtilities is a collector of reusable utility methods for RDF store management
    /// </summary>
    internal static class RDFStoreUtilities {

        #region Select
        /// <summary>
        /// Parses the current quadruple of the data reader 
        /// </summary>
        internal static RDFQuadruple ParseQuadruple(IDataReader fetchedQuadruples) {

            if (fetchedQuadruples != null) { 
                RDFContext  qContext      = new RDFContext(fetchedQuadruples["Context"].ToString());
                RDFResource qSubject      = new RDFResource(fetchedQuadruples["Subject"].ToString());
                RDFResource qPredicate    = new RDFResource(fetchedQuadruples["Predicate"].ToString());

                //SPO-flavour quadruple
                if (fetchedQuadruples["TripleFlavor"].Equals(1)) {
                    RDFResource qObject   = new RDFResource(fetchedQuadruples["Object"].ToString());
                    return new RDFQuadruple(qContext, qSubject, qPredicate, qObject);
                }

                //SPL-flavour quadruple
                String literal            = fetchedQuadruples["Object"].ToString();

                //PlainLiteral
                if (!literal.Contains("^^") || literal.EndsWith("^^") || RDFModelUtilities.GetUriFromString(literal.Substring(literal.LastIndexOf("^^", StringComparison.Ordinal) + 2)) == null) {
                    RDFPlainLiteral pLit  = null;
                    if (literal.Contains("@")) {
                        if (!literal.EndsWith("@")) {
                            Int32 lastAmp = literal.LastIndexOf('@');
                            pLit          = new RDFPlainLiteral(literal.Substring(0, lastAmp), literal.Substring(lastAmp + 1));
                        }
                        else {
                            pLit          = new RDFPlainLiteral(literal);
                        }
                    }
                    else {
                        pLit              = new RDFPlainLiteral(literal);
                    }
                    return new RDFQuadruple(qContext, qSubject, qPredicate, pLit);
                }

                //TypedLiteral
                String tLitValue          = literal.Substring(0, literal.LastIndexOf("^^", StringComparison.Ordinal));
                String tLitDatatype       = literal.Substring(literal.LastIndexOf("^^", StringComparison.Ordinal) + 2);
                RDFDatatype dt            = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                RDFTypedLiteral tLit      = new RDFTypedLiteral(tLitValue, dt);
                return new RDFQuadruple(qContext, qSubject, qPredicate, tLit);
            }
            throw new RDFStoreException("Cannot parse quadruple because given \"fetchedQuadruples\" parameter is null.");

        }

        /// <summary>
        /// Selects the quadruples corresponding to the given pattern from the given store
        /// </summary>
        internal static List<RDFQuadruple> SelectQuadruples(RDFMemoryStore store,
                                                            RDFContext  ctx,
                                                            RDFResource subj,
                                                            RDFResource pred,
                                                            RDFResource obj,
                                                            RDFLiteral  lit) {
            var matchCtx         = new List<RDFQuadruple>();
            var matchSubj        = new List<RDFQuadruple>();
            var matchPred        = new List<RDFQuadruple>();
            var matchObj         = new List<RDFQuadruple>();
            var matchLit         = new List<RDFQuadruple>();
            var matchResult      = new List<RDFQuadruple>();
            if (store           != null) {

                //Filter by Context
                if (ctx         != null) {
                    foreach (var q in store.StoreIndex.SelectIndexByContext(ctx).Keys) {
                        matchCtx.Add(store.Quadruples[q]);
                    }
                }

                //Filter by Subject
                if (subj        != null) {
                    foreach (var q in store.StoreIndex.SelectIndexBySubject(subj).Keys) {
                        matchSubj.Add(store.Quadruples[q]);
                    }
                }

                //Filter by Predicate
                if (pred        != null) {
                    foreach (var q in store.StoreIndex.SelectIndexByPredicate(pred).Keys) {
                        matchPred.Add(store.Quadruples[q]);
                    }
                }

                //Filter by Object
                if (obj         != null) {
                    foreach (var q in store.StoreIndex.SelectIndexByObject(obj).Keys) {
                        matchObj.Add(store.Quadruples[q]);
                    }
                }

                //Filter by Literal
                if (lit         != null) {
                    foreach (var q in store.StoreIndex.SelectIndexByLiteral(lit).Keys) {
                        matchLit.Add(store.Quadruples[q]);
                    }
                }

                //Intersect the filters
                if (ctx                        != null) {
                    if (subj                   != null) {
                        if (pred               != null) {
                            if (obj            != null) {
                                //C->S->P->O
                                matchResult     = matchCtx.Intersect(matchSubj)
                                                          .Intersect(matchPred)
                                                          .Intersect(matchObj)
                                                          .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //C->S->P->L
                                    matchResult = matchCtx.Intersect(matchSubj)
                                                          .Intersect(matchPred)
                                                          .Intersect(matchLit)
                                                          .ToList<RDFQuadruple>();
                                }
                                else {
                                    //C->S->P->
                                    matchResult = matchCtx.Intersect(matchSubj)
                                                          .Intersect(matchPred)
                                                          .ToList<RDFQuadruple>();
                                }
                            }
                        }
                        else {
                            if (obj            != null) {
                                //C->S->->O
                                matchResult     = matchCtx.Intersect(matchSubj)
                                                          .Intersect(matchObj)
                                                          .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //C->S->->L
                                    matchResult = matchCtx.Intersect(matchSubj)
                                                          .Intersect(matchLit)
                                                          .ToList<RDFQuadruple>();
                                }
                                else {
                                    //C->S->->
                                    matchResult = matchCtx.Intersect(matchSubj)
                                                          .ToList<RDFQuadruple>();
                                }
                            }
                        }
                    }
                    else {
                        if (pred               != null) {
                            if (obj            != null) {
                                //C->->P->O
                                matchResult     = matchCtx.Intersect(matchPred)
                                                          .Intersect(matchObj)
                                                          .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //C->->P->L
                                    matchResult = matchCtx.Intersect(matchPred)
                                                          .Intersect(matchLit)
                                                          .ToList<RDFQuadruple>();
                                }
                                else {
                                    //C->->P->
                                    matchResult = matchCtx.Intersect(matchPred)
                                                          .ToList<RDFQuadruple>();
                                }
                            }
                        }
                        else {
                            if (obj            != null) {
                                //C->->->O
                                matchResult     = matchCtx.Intersect(matchObj)
                                                          .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //C->->->L
                                    matchResult = matchCtx.Intersect(matchLit)
                                                          .ToList<RDFQuadruple>();
                                }
                                else {
                                    //C->->->
                                    matchResult = matchCtx;
                                }
                            }
                        }
                    }
                }
                else {
                    if (subj                   != null) {
                        if (pred               != null) {
                            if (obj            != null) {
                                //->S->P->O
                                matchResult     = matchSubj.Intersect(matchPred)
                                                           .Intersect(matchObj)
                                                           .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //->S->P->L
                                    matchResult = matchSubj.Intersect(matchPred)
                                                           .Intersect(matchLit)
                                                           .ToList<RDFQuadruple>();
                                }
                                else {
                                    //->S->P->
                                    matchResult = matchSubj.Intersect(matchPred)
                                                           .ToList<RDFQuadruple>();
                                }
                            }
                        }
                        else {
                            if (obj            != null) {
                                //->S->->O
                                matchResult     = matchSubj.Intersect(matchObj)
                                                           .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //->S->->L
                                    matchResult = matchSubj.Intersect(matchLit)
                                                           .ToList<RDFQuadruple>();
                                }
                                else {
                                    //->S->->
                                    matchResult = matchSubj;
                                }
                            }
                        }
                    }
                    else {
                        if (pred               != null) {
                            if (obj            != null) {
                                //->->P->O
                                matchResult     = matchPred.Intersect(matchObj)
                                                           .ToList<RDFQuadruple>();
                            }
                            else {
                                if (lit        != null) {
                                    //->->P->L
                                    matchResult = matchPred.Intersect(matchLit)
                                                           .ToList<RDFQuadruple>();
                                }
                                else {
                                    //->->P->
                                    matchResult = matchPred;
                                }
                            }
                        }
                        else {
                            if (obj            != null) {
                                //->->->O
                                matchResult     = matchObj;
                            }
                            else {
                                if (lit        != null) {
                                    //->->->L
                                    matchResult = matchLit;
                                }
                                else {
                                    //->->->
                                    matchResult = store.Quadruples.Values.ToList<RDFQuadruple>();
                                }
                            }
                        }
                    }
                }

            }
            return matchResult;
        }
        #endregion

        #region Serialization

        #region RDFNQuads
        /// <summary>
        /// Tries to parse the given N-Quad
        /// </summary>
        internal static String[] ParseNQuadruple(String ntriple) {
            String[] tokens   = new String[3];

            //A legal N-Quad starts with "_:" of blanks or "<" of non-blanks
            if (ntriple.StartsWith("_:") || ntriple.StartsWith("<")) {

                //Parse N-Quad by exploiting surrounding spaces and angle brackets of predicate
                tokens        = RDFModelUtilities.regexNT.Value.Split(ntriple, 3);

                //An illegal N-Quad cannot be splitted into 4 parts with this regex
                if (tokens.Length != 4) {
                    throw new Exception("found illegal N-Quad, predicate must be surrounded by \" <\" and \"> \"");
                }

                //Check subject for well-formedness
                tokens[0]     = tokens[0].Trim(new Char[] { ' ', '\n', '\r', '\t' });
                if (tokens[0].Contains(" ")) {
                    throw new Exception("found illegal N-Quad, subject Uri cannot contain spaces");
                }
                if ((tokens[0].StartsWith("<") && !tokens[0].EndsWith(">")) ||
                    (tokens[0].StartsWith("_:") && tokens[0].EndsWith(">")) ||
                    (tokens[0].Count(c => c.Equals('<')) > 1) ||
                    (tokens[0].Count(c => c.Equals('>')) > 1)) {
                    throw new Exception("found illegal N-Quad, subject Uri is not well-formed");
                }

                //Check predicate for well-formedness
                tokens[1]     = tokens[1].Trim(new Char[] { ' ', '\n', '\r', '\t' });
                if (tokens[1].Contains(" ")) {
                    throw new Exception("found illegal N-Quad, predicate Uri cannot contain spaces");
                }
                if ((tokens[1].Count(c => c.Equals('<')) > 1) ||
                    (tokens[1].Count(c => c.Equals('>')) > 1)) {
                    throw new Exception("found illegal N-Quad, predicate Uri is not well-formed");
                }

                //Check object for well-formedness
                tokens[2]     = tokens[2].Trim(new Char[] { ' ', '\n', '\r', '\t' });
                if (tokens[2].StartsWith("<")) {
                    if (tokens[2].Contains(" ")) {
                        throw new Exception("found illegal N-Quad, object Uri cannot contain spaces");
                    }
                    if ((!tokens[2].EndsWith(">") ||
                         (tokens[2].Count(c => c.Equals('<')) > 1) ||
                         (tokens[2].Count(c => c.Equals('>')) > 1))) {
                        throw new Exception("found illegal N-Quad, object Uri is not well-formed");
                    }
                }
                else if (tokens[2].StartsWith("_:")) {
                     if (tokens[2].EndsWith(">")) {
                        throw new Exception("found illegal N-Quad, object Uri is not well-formed");
                     }
                }

            }
            else {
                throw new Exception("found illegal N-Quad, must start with \"_:\" or with \"<\"");
            }

            return tokens;
        }
        #endregion

        #endregion

    }

}