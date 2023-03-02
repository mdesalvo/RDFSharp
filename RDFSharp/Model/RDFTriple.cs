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

using RDFSharp.Query;
using System;
using System.Threading.Tasks;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTriple represents a triple in the RDF model.
    /// </summary>
    public class RDFTriple : IEquatable<RDFTriple>
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
        public RDFResource ReificationSubject => LazyReificationSubject.Value;
        private readonly Lazy<RDFResource> LazyReificationSubject;
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFResource obj)
        {
            if (pred == null)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");

            TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
            Subject = subj ?? new RDFResource();
            Predicate = pred;
            Object = obj ?? new RDFResource();
            LazyTripleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", TripleID.ToString())));
        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFLiteral lit)
        {
            if (pred == null)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");

            TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
            Subject = subj ?? new RDFResource();
            Predicate = pred;
            Object = lit ?? new RDFPlainLiteral(string.Empty);
            LazyTripleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", TripleID.ToString())));
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
            LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", TripleID.ToString())));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the triple
        /// </summary>
        public override string ToString()
            => string.Concat(Subject.ToString(), " ", Predicate.ToString(), " ", Object.ToString());

        /// <summary>
        /// Performs the equality comparison between two triples
        /// </summary>
        public bool Equals(RDFTriple other)
            => other != null && TripleID.Equals(other.TripleID);
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification graph of the triple
        /// </summary>
        public RDFGraph ReifyTriple()
        {
            RDFGraph reifGraph = new RDFGraph();

            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)Subject));
            reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)Predicate));
            if (TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)Object));
            else
                reifGraph.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)Object));

            return reifGraph;
        }

        /// <summary>
        /// Builds the reification asynchronous graph of the triple
        /// </summary>
        public Task<RDFAsyncGraph> ReifyTripleAsync()
            => Task.Run(() => new RDFAsyncGraph(ReifyTriple()));
        #endregion
    }

    /// <summary>
    /// RDFIndexedTriple represents the internal hashed representation of a triple in the library
    /// </summary>
    internal class RDFIndexedTriple : IEquatable<RDFIndexedTriple>
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