﻿/*
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
using System.Threading.Tasks;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTriple represents a triple in the RDF model.
    /// </summary>
    public sealed class RDFTriple : IEquatable<RDFTriple>
    {
        #region Properties
        /// <summary>
        /// Unique representation of the triple
        /// </summary>
        internal long TripleID => LazyTripleID.Value;
        private readonly Lazy<long> LazyTripleID;

        /// <summary>
        /// Flavor of the triple
        /// </summary>
        public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; internal set; }

        /// <summary>
        /// Member acting as subject of the triple
        /// </summary>
        public RDFPatternMember Subject { get; internal set; }

        /// <summary>
        /// Member acting as predicate of the triple
        /// </summary>
        public RDFPatternMember Predicate { get; internal set; }

        /// <summary>
        /// Member acting as object of the triple
        /// </summary>
        public RDFPatternMember Object { get; internal set; }

        /// <summary>
        /// Representative of the triple's reification
        /// </summary>
        public RDFResource ReificationSubject => LazyReificationSubject.Value;
        private readonly Lazy<RDFResource> LazyReificationSubject;
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFResource obj)
            : this(subj, pred)
        {
            TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
            Object = obj ?? new RDFResource();
        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFLiteral lit)
            : this(subj, pred)
        {
            TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
            Object = lit ?? RDFPlainLiteral.Empty;
        }

        /// <summary>
        /// Initializer-ctor for common triple properties
        /// </summary>
        private RDFTriple(RDFResource subj, RDFResource pred)
        {
            #region Guards
            if (pred == null)
                throw new RDFModelException("Cannot create RDFTriple because given \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFModelException("Cannot create RDFTriple because given \"pred\" parameter is a blank resource");
            #endregion

            Subject = subj ?? new RDFResource();
            Predicate = pred;
            LazyTripleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource($"bnode:{TripleID}"));
        }

        /// <summary>
        /// Default-ctor to build a triple from the given indexed triple
        /// </summary>
        internal RDFTriple(RDFIndexedTriple indexedTriple, RDFGraphIndex graphIndex)
        {
            TripleFlavor = indexedTriple.TripleFlavor;
            Subject = graphIndex.ResourcesRegister[indexedTriple.SubjectID];
            Predicate = graphIndex.ResourcesRegister[indexedTriple.PredicateID];
            if (indexedTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                Object = graphIndex.ResourcesRegister[indexedTriple.ObjectID];
            else
                Object = graphIndex.LiteralsRegister[indexedTriple.ObjectID];
            LazyTripleID = new Lazy<long>(() => indexedTriple.TripleID);
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource($"bnode:{TripleID}"));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the triple
        /// </summary>
        public override string ToString()
            => $"{Subject} {Predicate} {Object}";

        /// <summary>
        /// Performs the equality comparison between two triples
        /// </summary>
        public bool Equals(RDFTriple other)
            => other != null && TripleID.Equals(other.TripleID);
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification graph of the triple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the triple when it IS NOT rdf:TripleTerm
        /// </summary>
        public RDFGraph ReifyTriple(List<(RDFResource annPredicate,RDFPatternMember annObject)> tripleAnnotations=null)
        {
            RDFGraph reifGraph = new RDFGraph();

            // Standard reification
            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)Subject));
            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)Predicate));
            reifGraph.AddTriple(TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                ? new RDFTriple(ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)Object)
                : new RDFTriple(ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)Object));

            // Linked annotations
            if (tripleAnnotations?.Count > 0)
            {
                foreach ((RDFResource annPredicate, RDFPatternMember annObject) in tripleAnnotations)
                {
                    switch (annObject)
                    {
                        case RDFResource annObjRes:
                            reifGraph.AddTriple(new RDFTriple(ReificationSubject, annPredicate, annObjRes));
                            break;
                        case RDFLiteral annObjLit:
                            reifGraph.AddTriple(new RDFTriple(ReificationSubject, annPredicate, annObjLit));
                            break;
                    }
                }
            }

            return reifGraph;
        }

        /// <summary>
        /// Asynchronously builds the reification graph of the triple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the triple when it IS NOT rdf:TripleTerm
        /// </summary>
        public Task<RDFGraph> ReifyTripleAsync(List<(RDFResource annPredicate, RDFPatternMember annObject)> tripleAnnotations = null)
            => Task.Run(() => ReifyTriple(tripleAnnotations));

        /// <summary>
        /// Builds the reification graph of the triple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the triple when it IS rdf:TripleTerm (RDF 1.2)
        /// </summary>
        public RDFGraph ReifyTripleTerm(List<(RDFResource annPredicate, RDFPatternMember annObject)> tripleAnnotations=null)
        {
            RDFGraph reifGraph = new RDFGraph();
            RDFResource ttIdentifier = new RDFResource($"bnode:TT{TripleID}");

            // TripleTerm reification
            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.REIFIES, ttIdentifier));
            reifGraph.AddTriple(new RDFTriple(ttIdentifier, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.TRIPLE_TERM));
            reifGraph.AddTriple(new RDFTriple(ttIdentifier, RDFVocabulary.RDF.TT_SUBJECT, (RDFResource)Subject));
            reifGraph.AddTriple(new RDFTriple(ttIdentifier, RDFVocabulary.RDF.TT_PREDICATE, (RDFResource)Predicate));
            reifGraph.AddTriple(TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                ? new RDFTriple(ttIdentifier, RDFVocabulary.RDF.TT_OBJECT, (RDFResource)Object)
                : new RDFTriple(ttIdentifier, RDFVocabulary.RDF.TT_OBJECT, (RDFLiteral)Object));

            // Linked annotations
            if (tripleAnnotations?.Count > 0)
            {
                foreach ((RDFResource annPredicate, RDFPatternMember annObject) in tripleAnnotations)
                {
                    switch (annObject)
                    {
                        case RDFResource annObjRes:
                            reifGraph.AddTriple(new RDFTriple(ReificationSubject, annPredicate, annObjRes));
                            break;
                        case RDFLiteral annObjLit:
                            reifGraph.AddTriple(new RDFTriple(ReificationSubject, annPredicate, annObjLit));
                            break;
                    }
                }
            }

            return reifGraph;
        }

        /// <summary>
        /// Asynchonously builds the reification graph of the triple and includes the given annotations.<br/>
        /// Use this method to assert knowledge about the triple when it IS rdf:TripleTerm (RDF 1.2)
        /// </summary>
        public Task<RDFGraph> ReifyTripleTermAsync(List<(RDFResource annPredicate, RDFPatternMember annObject)> tripleAnnotations=null)
            => Task.Run(() => ReifyTripleTerm(tripleAnnotations));
        #endregion
    }

    /// <summary>
    /// RDFIndexedTriple represents the internal hashed representation of a triple in the library
    /// </summary>
    internal sealed class RDFIndexedTriple : IEquatable<RDFIndexedTriple>
    {
        #region Properties
        /// <summary>
        /// Identifier of the triple
        /// </summary>
        internal long TripleID { get; set; }

        /// <summary>
        /// Identifier of the member acting as subject token of the triple
        /// </summary>
        internal long SubjectID { get; set; }

        /// <summary>
        /// Identifier of the member acting as predicate token of the triple
        /// </summary>
        internal long PredicateID { get; set; }

        /// <summary>
        /// Identifier of the member acting as object token of the triple
        /// </summary>
        internal long ObjectID { get; set; }

        /// <summary>
        /// Flavor of the triple
        /// </summary>
        internal RDFModelEnums.RDFTripleFlavors TripleFlavor { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Default-ctor to build an indexed triple from the given triple
        /// </summary>
        internal RDFIndexedTriple(RDFTriple triple)
        {
            TripleFlavor = triple.TripleFlavor;
            TripleID = triple.TripleID;
            SubjectID = triple.Subject.PatternMemberID;
            PredicateID = triple.Predicate.PatternMemberID;
            ObjectID = triple.Object.PatternMemberID;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Performs the equality comparison between two indexed triples
        /// </summary>
        public bool Equals(RDFIndexedTriple other)
            => other != null && TripleID.Equals(other.TripleID);
        #endregion
    }
}