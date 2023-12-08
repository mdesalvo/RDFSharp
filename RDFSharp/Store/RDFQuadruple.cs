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
using RDFSharp.Query;
using System;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFQuadruple represents a quadruple in the RDF store.
    /// </summary>
    public class RDFQuadruple : IEquatable<RDFQuadruple>
    {
        #region Properties
        /// <summary>
        /// Unique representation of the quadruple
        /// </summary>
        public long QuadrupleID => LazyQuadrupleID.Value;
        private readonly Lazy<long> LazyQuadrupleID;

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
        public RDFResource ReificationSubject => LazyReificationSubject.Value;
        private readonly Lazy<RDFResource> LazyReificationSubject;
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred, RDFResource obj)
            : this(context, subj, pred)
        {
            TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;   
            Object = obj ?? new RDFResource();
        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred, RDFLiteral lit)
            : this(context, subj, pred)
        {
            TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
            Object = lit ?? new RDFPlainLiteral(string.Empty);
        }

        /// <summary>
        /// Initializer-ctor for common quadruple properties
        /// </summary>
        private RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred)
        {
            #region Guards
            if (pred == null)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is a blank resource");
            #endregion

            Context = context ?? new RDFContext();
            Subject = subj ?? new RDFResource();
            Predicate = pred;
            LazyQuadrupleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", QuadrupleID.ToString())));
        }

        /// <summary>
        /// Default-ctor to build a quadruple from the given indexed quadruple
        /// </summary>
        internal RDFQuadruple(RDFIndexedQuadruple indexedQuadruple, RDFStoreIndex storeIndex)
        {
            TripleFlavor = indexedQuadruple.TripleFlavor;
            Context = storeIndex.ContextsRegister[indexedQuadruple.ContextID];
            Subject = storeIndex.ResourcesRegister[indexedQuadruple.SubjectID];
            Predicate = storeIndex.ResourcesRegister[indexedQuadruple.PredicateID];
            if (indexedQuadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                Object = storeIndex.ResourcesRegister[indexedQuadruple.ObjectID];
            else
                Object = storeIndex.LiteralsRegister[indexedQuadruple.ObjectID];
            LazyQuadrupleID = new Lazy<long>(() => indexedQuadruple.QuadrupleID);
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", QuadrupleID.ToString())));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the quadruple
        /// </summary>
        public override string ToString()
            => string.Concat(Context.ToString(), " ", Subject.ToString(), " ", Predicate.ToString(), " ", Object.ToString());

        /// <summary>
        /// Performs the equality comparison between two quadruples
        /// </summary>
        public bool Equals(RDFQuadruple other)
            => other != null && QuadrupleID.Equals(other.QuadrupleID);
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification store of the quadruple
        /// </summary>
        public RDFMemoryStore ReifyQuadruple()
        {
            RDFMemoryStore reifStore = new RDFMemoryStore();
            
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)Subject));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)Predicate));
            if (TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)Object));
            else
                reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)Object));

            return reifStore;
        }
        #endregion
    }

    /// <summary>
    /// RDFIndexedQuadruple represents the internal hashed representation of a quadruple in the library
    /// </summary>
    internal class RDFIndexedQuadruple : IEquatable<RDFIndexedQuadruple>
    {
        #region Properties
        /// <summary>
        /// Identifier of the quadruple
        /// </summary>
        internal long QuadrupleID { get; set; }

        /// <summary>
        /// Identifier of the member acting as context token of the quadruple
        /// </summary>
        internal long ContextID { get; set; }

        /// <summary>
        /// Identifier of the member acting as subject token of the quadruple
        /// </summary>
        internal long SubjectID { get; set; }

        /// <summary>
        /// Identifier of the member acting as predicate token of the quadruple
        /// </summary>
        internal long PredicateID { get; set; }

        /// <summary>
        /// Identifier of the member acting as object token of the quadruple
        /// </summary>
        internal long ObjectID { get; set; }

        /// <summary>
        /// Flavor of the quadruple
        /// </summary>
        internal RDFModelEnums.RDFTripleFlavors TripleFlavor { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Default-ctor to build an indexed quadruple from the given quadruple
        /// </summary>
        internal RDFIndexedQuadruple(RDFQuadruple quadruple)
        {
            TripleFlavor = quadruple.TripleFlavor;
            QuadrupleID = quadruple.QuadrupleID;
            ContextID = quadruple.Context.PatternMemberID;
            SubjectID = quadruple.Subject.PatternMemberID;
            PredicateID = quadruple.Predicate.PatternMemberID;
            ObjectID = quadruple.Object.PatternMemberID;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Performs the equality comparison between two indexed quadruples
        /// </summary>
        public bool Equals(RDFIndexedQuadruple other)
            => other != null && QuadrupleID.Equals(other.QuadrupleID);
        #endregion
    }
}