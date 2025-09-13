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
using RDFSharp.Query;

namespace RDFSharp.Model;

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
    public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; }

    /// <summary>
    /// Member acting as subject of the triple
    /// </summary>
    public RDFPatternMember Subject { get; }

    /// <summary>
    /// Member acting as predicate of the triple
    /// </summary>
    public RDFPatternMember Predicate { get; }

    /// <summary>
    /// Member acting as object of the triple
    /// </summary>
    public RDFPatternMember Object { get; }

    /// <summary>
    /// Representative of the triple's reification
    /// </summary>
    public RDFResource ReificationSubject => LazyReificationSubject.Value;
    private readonly Lazy<RDFResource> LazyReificationSubject;
    #endregion

    #region Ctors
    /// <summary>
    /// Builds a triple with SPO flavor
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public RDFTriple(RDFResource s, RDFResource p, RDFResource o) : this(s, p)
    {
        TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
        Object = o ?? new RDFResource();
    }

    /// <summary>
    /// Builds a triple with SPL flavor
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public RDFTriple(RDFResource s, RDFResource p, RDFLiteral l) : this(s, p)
    {
        TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
        Object = l ?? RDFPlainLiteral.Empty;
    }

    /// <summary>
    /// Initializer for common triple properties
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    private RDFTriple(RDFResource s, RDFResource p)
    {
        #region Guards
        if (p == null)
            throw new RDFModelException("Cannot create RDFTriple because given \"pred\" parameter is null");
        if (p.IsBlank)
            throw new RDFModelException("Cannot create RDFTriple because given \"pred\" parameter is a blank resource");
        #endregion

        Subject = s ?? new RDFResource();
        Predicate = p;
        LazyTripleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
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
        => other != null && TripleID == other.TripleID;

    /// <summary>
    /// Performs the equality comparison between two triples
    /// </summary>
    public override bool Equals(object other)
        => other is RDFTriple t && TripleID == t.TripleID;

    /// <summary>
    /// Calculates the hashcode of this triple
    /// </summary>
    public override int GetHashCode()
        => TripleID.GetHashCode();
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
    #endregion
}