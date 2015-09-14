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
using System.Text;
using System.Text.RegularExpressions;
using RDFSharp.Query;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFTriple represents a triple in the RDF model.
    /// </summary>
    public class RDFTriple: IEquatable<RDFTriple> {

        #region Properties
        /// <summary>
        /// Unique representation of the triple
        /// </summary>
        internal Int64 TripleID { get; set; }

        /// <summary>
        /// Flavor of the triple
        /// </summary>
        public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; internal set; }

        /// <summary>
        /// Member acting as subject token of the triple
        /// </summary>
        public RDFPatternMember Subject { get; internal set; }

        /// <summary>
        /// Member acting as predicate token of the triple
        /// </summary>
        public RDFPatternMember Predicate { get; internal set; }

        /// <summary>
        /// Member acting as object token of the triple
        /// </summary>
        public RDFPatternMember Object { get; internal set; }

        /// <summary>
        /// Subject of the triple's reification
        /// </summary>
        public RDFResource ReificationSubject { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFResource obj) {

            //TripleFlavor
            this.TripleFlavor      = RDFModelEnums.RDFTripleFlavors.SPO;

            //Subject
            this.Subject           = (subj ?? new RDFResource());

            //Predicate
            if (pred != null) {
                if (pred.IsBlank) {
                    throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");
                }
                this.Predicate     = pred;
            }
            else {
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            }

            //Object
            this.Object            = (obj ?? new RDFResource());

            //TripleID
            this.TripleID          = RDFModelUtilities.CreateHash(this.ToString());

            //ReificationSubject
            this.ReificationSubject= new RDFResource("bnode:" + this.TripleID);

        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFLiteral lit) {

            //TripleFlavor
            this.TripleFlavor      = RDFModelEnums.RDFTripleFlavors.SPL;

            //Subject
            this.Subject           = (subj ?? new RDFResource());

            //Predicate
            if (pred != null) {
                if (pred.IsBlank) {
                    throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");
                }
                this.Predicate     = pred;
            }
            else {
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            }

            //Object
            this.Object            = (lit ?? new RDFPlainLiteral(String.Empty));

            //TripleID
            this.TripleID          = RDFModelUtilities.CreateHash(this.ToString());

            //ReificationSubject
            this.ReificationSubject= new RDFResource("bnode:" + this.TripleID);

        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the triple
        /// </summary>
        public override String ToString() {
            return this.Subject + " " + this.Predicate + " " + this.Object;
        }

        /// <summary>
        /// Performs the equality comparison between two triples
        /// </summary>
        public Boolean Equals(RDFTriple other) {
            return (other != null && this.TripleID.Equals(other.TripleID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification graph of the triple
        /// </summary>
        public RDFGraph ReifyTriple() {
            RDFGraph reifGraph     = new RDFGraph();

            reifGraph.AddTriple(new RDFTriple(this.ReificationSubject,     RDFVocabulary.RDF.TYPE,      RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(this.ReificationSubject,     RDFVocabulary.RDF.SUBJECT,   (RDFResource)this.Subject));
            reifGraph.AddTriple(new RDFTriple(this.ReificationSubject,     RDFVocabulary.RDF.PREDICATE, (RDFResource)this.Predicate));
            if (this.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.OBJECT,    (RDFResource)this.Object));
            }
            else {
                reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.OBJECT,    (RDFLiteral)this.Object));
            }

            return reifGraph;
        }

        /// <summary>
        /// Gives the N-Triples string representation of this triple
        /// </summary>
        public String ToNTriples() {
            var ntriple = new StringBuilder();

            //Subject
            if (((RDFResource)this.Subject).IsBlank) {
                ntriple.Append(this.Subject.ToString().Replace("bnode:", "_:") + " ");
            }
            else {
                ntriple.Append("<" + this.Subject + "> ");
            }

            //Predicate
            ntriple.Append("<" + this.Predicate + "> ");

            //Object
            if (this.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                if (((RDFResource)this.Object).IsBlank) {
                    ntriple.Append(this.Object.ToString().Replace("bnode:", "_:") + " .");
                }
                else {
                    ntriple.Append("<" + this.Object + "> .");
                }
            }
            else {
                if (this.Object is RDFPlainLiteral) {
                    if (((RDFPlainLiteral)this.Object).Language != String.Empty) {
                        ntriple.Append("\"" + ((RDFPlainLiteral)this.Object).Value + "\"@" + ((RDFPlainLiteral)this.Object).Language + " .");
                    }
                    else {
                        ntriple.Append("\"" + ((RDFPlainLiteral)this.Object).Value + "\" .");
                    }
                }
                else {
                    ntriple.Append("\"" + ((RDFTypedLiteral)this.Object).Value + "\"^^<" + ((RDFTypedLiteral)this.Object).Datatype + "> .");
                }
            }

            return RDFSerializerUtilities.Unicode_To_ASCII(ntriple.ToString());
        }

        /// <summary>
        /// Builds a triple from the given N-Triples string representation
        /// </summary>
        public static RDFTriple FromNTriples(String ntriple) {
            if (ntriple          != null) {
                String[] tokens   = new String[3];
                
                //Preliminary checks (non-ASCII encoding to throw error, empty/comment line to ignore)
                if (ntriple      != Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(ntriple))) {
                    throw new RDFModelException("Given \"ntriple\" string contains non-ASCII characters");
                }
                ntriple           = RDFSerializerUtilities.ValidNTriples["StartSpace"].Value.Replace(ntriple, String.Empty);
                ntriple           = RDFSerializerUtilities.ValidNTriples["EndSpace"].Value.Replace(ntriple,   String.Empty);
                if (ntriple      == String.Empty || ntriple.StartsWith("#")) {
                    return null;
                }

                //Effective parsing
                if (ntriple.StartsWith("<")) {
                    ntriple       = RDFSerializerUtilities.ASCII_To_Unicode(ntriple);
                    if (RDFSerializerUtilities.ValidNTriples["Su_P_Ou"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        tokens[0] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[1] = uris[1].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = uris[2].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFResource(tokens[2]));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Su_P_Ob"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var buris = RDFSerializerUtilities.ValidNTriples["BndUri"].Value.Matches(ntriple);
                        tokens[0] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[1] = uris[1].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = buris[0].Value;
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFResource(tokens[2]));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Su_P_Lp"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var lp    = RDFSerializerUtilities.ValidNTriples["Lplain"].Value.Matches(ntriple);
                        tokens[0] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[1] = uris[1].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = lp[0].Value.Trim(new Char[] { '\"' }).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFPlainLiteral(tokens[2]));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Su_P_Lpl"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var lpl   = RDFSerializerUtilities.ValidNTriples["Lplainlang"].Value.Matches(ntriple);
                        tokens[0] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[1] = uris[1].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = lpl[0].Value;
                        var litVl = tokens[2].Substring(0, tokens[2].LastIndexOf("@")).Trim(new Char[] { '\"' }).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        var litLn = tokens[2].Substring(tokens[2].LastIndexOf("@")+1);
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFPlainLiteral(litVl, litLn));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Su_P_Lt"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var lt    = RDFSerializerUtilities.ValidNTriples["Ltyped"].Value.Matches(ntriple);
                        tokens[0] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[1] = uris[1].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = lt[0].Value;
                        var litVl = tokens[2].Substring(0, tokens[2].LastIndexOf("^^<")).Trim(new Char[] { '\"' }).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        var litDt = tokens[2].Substring(tokens[2].LastIndexOf("^^<") + 3).TrimEnd(new Char[] { '>' });
                        return new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFTypedLiteral(litVl, RDFModelUtilities.GetDatatypeFromString(litDt)));
                    }
                    else {
                        throw new RDFModelException("Given \"ntriple\" string does not represent a well-formed N-Triple");
                    }
                }
                else if (ntriple.StartsWith("_:")) {
                    ntriple       = RDFSerializerUtilities.ASCII_To_Unicode(ntriple);
                    if (RDFSerializerUtilities.ValidNTriples["Sb_P_Ou"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var buris = RDFSerializerUtilities.ValidNTriples["BndUri"].Value.Matches(ntriple);
                        tokens[0] = buris[0].Value;
                        tokens[1] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = uris[1].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFResource(tokens[2]));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Sb_P_Ob"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var buris = RDFSerializerUtilities.ValidNTriples["BndUri"].Value.Matches(ntriple);
                        tokens[0] = buris[0].Value;
                        tokens[1] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = buris[1].Value;
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFResource(tokens[2]));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Sb_P_Lp"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var buris = RDFSerializerUtilities.ValidNTriples["BndUri"].Value.Matches(ntriple);
                        var lp    = RDFSerializerUtilities.ValidNTriples["Lplain"].Value.Matches(ntriple);
                        tokens[0] = buris[0].Value;
                        tokens[1] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = lp[0].Value.Trim(new Char[] { '\"' }).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFPlainLiteral(tokens[2]));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Sb_P_Lpl"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var buris = RDFSerializerUtilities.ValidNTriples["BndUri"].Value.Matches(ntriple);
                        var lpl   = RDFSerializerUtilities.ValidNTriples["Lplainlang"].Value.Matches(ntriple);
                        tokens[0] = buris[0].Value;
                        tokens[1] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = lpl[0].Value;
                        var litVl = tokens[2].Substring(0, tokens[2].LastIndexOf("@")).Trim(new Char[] { '\"' }).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        var litLn = tokens[2].Substring(tokens[2].LastIndexOf("@")+1);
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFPlainLiteral(litVl, litLn));
                    }
                    else if (RDFSerializerUtilities.ValidNTriples["Sb_P_Lt"].Value.Match(ntriple).Success) {
                        var uris  = RDFSerializerUtilities.ValidNTriples["BrkUri"].Value.Matches(ntriple);
                        var buris = RDFSerializerUtilities.ValidNTriples["BndUri"].Value.Matches(ntriple);
                        var lt    = RDFSerializerUtilities.ValidNTriples["Ltyped"].Value.Matches(ntriple);
                        tokens[0] = buris[0].Value;
                        tokens[1] = uris[0].Value.TrimStart(new Char[] { '<' }).TrimEnd(new Char[] { '>' });
                        tokens[2] = lt[0].Value;
                        var litVl = tokens[2].Substring(0, tokens[2].LastIndexOf("^^<")).Trim(new Char[] { '\"' }).Replace("\\\"", "\"").Replace("\\\\", "\\");
                        var litDt = tokens[2].Substring(tokens[2].LastIndexOf("^^<") + 3).TrimEnd(new Char[] { '>' });
                        return  new RDFTriple(new RDFResource(tokens[0]), new RDFResource(tokens[1]), new RDFTypedLiteral(litVl, RDFModelUtilities.GetDatatypeFromString(litDt)));
                    }
                    else {
                        throw new RDFModelException("Given \"ntriple\" string does not represent a well-formed N-Triple");
                    }
                }
                else {
                    throw new RDFModelException("Given \"ntriple\" string does not represent a well-formed N-Triple");
                }

            }
            throw new RDFModelException("Given \"ntriple\" string is null");
        }
        #endregion

    }

}