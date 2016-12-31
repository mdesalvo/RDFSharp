/*
   Copyright 2012-2017 Marco De Salvo

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
        public RDFResource ReificationSubject {
            get { return new RDFResource("bnode:" + this.TripleID); }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFResource obj) {

            //TripleFlavor
            this.TripleFlavor  = RDFModelEnums.RDFTripleFlavors.SPO;

            //Subject
            this.Subject       = (subj ?? new RDFResource());

            //Predicate
            if (pred != null) {
                if (pred.IsBlank) {
                    throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");
                }
                this.Predicate = pred;
            }
            else {
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            }

            //Object
            this.Object        = (obj ?? new RDFResource());

            //TripleID
            this.TripleID      = RDFModelUtilities.CreateHash(this.ToString());

        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFLiteral lit) {

            //TripleFlavor
            this.TripleFlavor  = RDFModelEnums.RDFTripleFlavors.SPL;

            //Subject
            this.Subject       = (subj ?? new RDFResource());

            //Predicate
            if (pred != null) {
                if (pred.IsBlank) {
                    throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");
                }
                this.Predicate = pred;
            }
            else {
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            }

            //Object
            this.Object        = (lit ?? new RDFPlainLiteral(String.Empty));

            //TripleID
            this.TripleID      = RDFModelUtilities.CreateHash(this.ToString());

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
            var reifGraph = new RDFGraph();
            var reifSubj  = this.ReificationSubject;

            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.SUBJECT, (RDFResource)this.Subject));
            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.PREDICATE, (RDFResource)this.Predicate));
            if (this.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.OBJECT, (RDFResource)this.Object));
            }
            else {
                reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.OBJECT, (RDFLiteral)this.Object));
            }

            return reifGraph;
        }
        #endregion

    }

}