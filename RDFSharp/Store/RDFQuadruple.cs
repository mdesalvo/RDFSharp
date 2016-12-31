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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFQuadruple represents a quadruple in the RDF store.
    /// </summary>
    public class RDFQuadruple: IEquatable<RDFQuadruple> {

        #region Properties
        /// <summary>
        /// Unique representation of the quadruple
        /// </summary>
        public Int64 QuadrupleID { get; internal set; }

        /// <summary>
        /// Flavor of the triple nested into the quadruple
        /// </summary>
        public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; internal set; }

        /// <summary>
        /// Member acting as context token of the quadruple
        /// </summary>
        public RDFPatternMember Context { get; internal set; }

        /// <summary>
        /// Member acting as subject token of the quadruple
        /// </summary>
        public RDFPatternMember Subject { get; internal set; }

        /// <summary>
        /// Member acting as predicate token of the quadruple
        /// </summary>
        public RDFPatternMember Predicate { get; internal set; }

        /// <summary>
        /// Member acting as object token of the quadruple
        /// </summary>
        public RDFPatternMember Object { get; internal set; }

        /// <summary>
        /// Subject of the quadruple's reification
        /// </summary>
        public RDFResource ReificationSubject {
            get { return new RDFResource("bnode:" + this.QuadrupleID); }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred, RDFResource obj) {

            //TripleFlavor
            this.TripleFlavor  = RDFModelEnums.RDFTripleFlavors.SPO;

            //Context
            this.Context       = (context ?? new RDFContext());

            //Subject
            this.Subject       = (subj ?? new RDFResource());

            //Predicate
            if (pred != null) {
                if (pred.IsBlank) {
                    throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is a blank resource");
                }
                this.Predicate = pred;
            }
            else {
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is null");
            }

            //Object
            this.Object        = (obj ?? new RDFResource());

            //QuadrupleID
            this.QuadrupleID   = RDFModelUtilities.CreateHash(this.ToString());

        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred, RDFLiteral lit) {

            //TripleFlavor
            this.TripleFlavor  = RDFModelEnums.RDFTripleFlavors.SPL;

            //Context
            this.Context       = (context ?? new RDFContext());

            //Subject
            this.Subject       = (subj ?? new RDFResource());

            //Predicate
            if (pred != null) {
                if (pred.IsBlank) {
                    throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is a blank resource");
                }
                this.Predicate = pred;
            }
            else {
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is null");
            }

            //Object
            this.Object        = (lit ?? new RDFPlainLiteral(String.Empty));

            //QuadrupleID
            this.QuadrupleID   = RDFModelUtilities.CreateHash(this.ToString());

        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the quadruple
        /// </summary>
        public override String ToString() {
            return this.Context + " " + this.Subject + " " + this.Predicate + " " + this.Object;
        }

        /// <summary>
        /// Performs the equality comparison between two quadruples
        /// </summary>
        public Boolean Equals(RDFQuadruple other) {
            return (other != null && this.QuadrupleID.Equals(other.QuadrupleID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification store of the quadruple
        /// </summary>
        public RDFMemoryStore ReifyQuadruple() {
            var reifStore = new RDFMemoryStore();
            var reifSubj  = this.ReificationSubject;

            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, reifSubj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, reifSubj, RDFVocabulary.RDF.SUBJECT, (RDFResource)this.Subject));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, reifSubj, RDFVocabulary.RDF.PREDICATE, (RDFResource)this.Predicate));
            if (this.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, reifSubj, RDFVocabulary.RDF.OBJECT, (RDFResource)this.Object));
            }
            else {
                reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, reifSubj, RDFVocabulary.RDF.OBJECT, (RDFLiteral)this.Object));
            }

            return reifStore;
        }
        #endregion

    }

}