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

using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFQuadruple represents a quadruple (context-aware triple) in the RDF store.
    /// </summary>
    public sealed class RDFQuadruple : IEquatable<RDFQuadruple>
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
        /// Triple-based ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFTriple triple)
        {
            #region Guards
            if (triple == null)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"triple\" parameter is null");
            #endregion

            Context = context ?? new RDFContext();
            TripleFlavor = triple.TripleFlavor;
            Subject = triple.Subject;
            Predicate = triple.Predicate;
            Object = triple.Object;
            LazyQuadrupleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource($"bnode:{QuadrupleID}"));
        }

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
            Object = lit ?? RDFPlainLiteral.Empty;
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
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource($"bnode:{QuadrupleID}"));
        }

        /// <summary>
        /// Default-ctor to build a quadruple from the given indexed quadruple
        /// </summary>
        internal RDFQuadruple(RDFIndexedQuadruple indexedQuadruple, RDFStoreIndex storeIndex)
        {
            Context = storeIndex.ContextsRegister[indexedQuadruple.ContextID];
            Subject = storeIndex.ResourcesRegister[indexedQuadruple.SubjectID];
            Predicate = storeIndex.ResourcesRegister[indexedQuadruple.PredicateID];
            if (indexedQuadruple.TripleFlavor == 1) //SPO
            {
                TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
                Object = storeIndex.ResourcesRegister[indexedQuadruple.ObjectID];
            }
            else
            {
                TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
                Object = storeIndex.LiteralsRegister[indexedQuadruple.ObjectID];
            }
            LazyQuadrupleID = new Lazy<long>(() => indexedQuadruple.QuadrupleID);
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource($"bnode:{QuadrupleID}"));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the quadruple
        /// </summary>
        public override string ToString()
            => $"{Context} {Subject} {Predicate} {Object}";

        /// <summary>
        /// Performs the equality comparison between two quadruples
        /// </summary>
        public bool Equals(RDFQuadruple other)
            => other != null && QuadrupleID.Equals(other.QuadrupleID);
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification store of the quadruple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the quadruple when it IS NOT rdf:TripleTerm
        /// </summary>
        public RDFMemoryStore ReifyQuadruple(List<(RDFResource annPredicate, RDFPatternMember annObject)> quadrupleAnnotations=null)
        {
            RDFMemoryStore reifStore = new RDFMemoryStore();

            // Standard reification
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)Subject));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)Predicate));
            reifStore.AddQuadruple(TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                ? new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)Object)
                : new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)Object));

            // Linked annotations
            if (quadrupleAnnotations?.Count > 0)
            {
                foreach ((RDFResource annPredicate, RDFPatternMember annObject) in quadrupleAnnotations)
                {
                    switch (annObject)
                    {
                        case RDFResource annObjRes:
                            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, annPredicate, annObjRes));
                            break;
                        case RDFLiteral annObjLit:
                            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, annPredicate, annObjLit));
                            break;
                    }
                }
            }

            return reifStore;
        }

        /// <summary>
        /// Asynchronously builds the reification store of the quadruple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the quadruple when it IS NOT rdf:TripleTerm
        /// </summary>
        public Task<RDFMemoryStore> ReifyQuadrupleAsync(List<(RDFResource annPredicate, RDFPatternMember annObject)> quadrupleAnnotations=null)
            => Task.Run(() => ReifyQuadruple(quadrupleAnnotations));

        /// <summary>
        /// Builds the reification store of the quadruple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the quadruple when it IS rdf:TripleTerm
        /// </summary>
        public RDFMemoryStore ReifyQuadrupleTerm(List<(RDFResource annPredicate, RDFPatternMember annObject)> quadrupleAnnotations=null)
        {
            RDFMemoryStore reifStore = new RDFMemoryStore();
            RDFResource ttIdentifier = new RDFResource($"bnode:TT{QuadrupleID}");

            // TripleTerm reification
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, RDFVocabulary.RDF.REIFIES, ttIdentifier));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ttIdentifier, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ttIdentifier, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)Subject));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ttIdentifier, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)Predicate));
            reifStore.AddQuadruple(TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                ? new RDFQuadruple((RDFContext)Context, ttIdentifier, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)Object)
                : new RDFQuadruple((RDFContext)Context, ttIdentifier, RDFVocabulary.RDF.TT_OBJECT, (RDFLiteral)Object));

            // Linked annotations
            if (quadrupleAnnotations?.Count > 0)
            {
                foreach ((RDFResource annPredicate, RDFPatternMember annObject) in quadrupleAnnotations)
                {
                    switch (annObject)
                    {
                        case RDFResource annObjRes:
                            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, annPredicate, annObjRes));
                            break;
                        case RDFLiteral annObjLit:
                            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)Context, ReificationSubject, annPredicate, annObjLit));
                            break;
                    }
                }
            }

            return reifStore;
        }

        /// <summary>
        /// Asynchronously builds the reification store of the quadruple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the quadruple when it IS rdf:TripleTerm
        /// </summary>
        public Task<RDFMemoryStore> ReifyQuadrupleTermAsync(List<(RDFResource annPredicate, RDFPatternMember annObject)> quadrupleAnnotations = null)
            => Task.Run(() => ReifyQuadrupleTerm(quadrupleAnnotations));
        #endregion
    }

    /// <summary>
    /// RDFIndexedQuadruple represents the internal hashed representation of a quadruple
    /// </summary>
    internal sealed class RDFIndexedQuadruple : IEquatable<RDFIndexedQuadruple>
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
        /// Flavor of the quadruple (1=SPO, 2=SPL)
        /// </summary>
        internal byte TripleFlavor { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Default-ctor to build an indexed quadruple from the given quadruple
        /// </summary>
        internal RDFIndexedQuadruple(RDFQuadruple quadruple)
        {
            TripleFlavor = (byte)quadruple.TripleFlavor;
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