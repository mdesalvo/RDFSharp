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

namespace RDFSharp.Store;

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
    public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; }

    /// <summary>
    /// Member acting as context token of the quadruple
    /// </summary>
    public RDFPatternMember Context { get; }

    /// <summary>
    /// Member acting as subject token of the quadruple
    /// </summary>
    public RDFPatternMember Subject { get; }

    /// <summary>
    /// Member acting as predicate token of the quadruple
    /// </summary>
    public RDFPatternMember Predicate { get; }

    /// <summary>
    /// Member acting as object token of the quadruple
    /// </summary>
    public RDFPatternMember Object { get; }

    /// <summary>
    /// Subject of the quadruple's reification
    /// </summary>
    public RDFResource ReificationSubject => LazyReificationSubject.Value;
    private readonly Lazy<RDFResource> LazyReificationSubject;
    #endregion

    #region Ctors
    /// <summary>
    /// Builds a quadruple from the given triple and the given context
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public RDFQuadruple(RDFContext c, RDFTriple t)
    {
        #region Guards
        if (t == null)
            throw new RDFStoreException("Cannot create RDFQuadruple because given \"t\" parameter is null");
        #endregion

        Context = c ?? new RDFContext();
        TripleFlavor = t.TripleFlavor;
        Subject = t.Subject;
        Predicate = t.Predicate;
        Object = t.Object;
        LazyQuadrupleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
        LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource($"bnode:{QuadrupleID}"));
    }

    /// <summary>
    /// Builds a quadruple with SPO flavor
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public RDFQuadruple(RDFContext c, RDFResource s, RDFResource p, RDFResource o) : this(c, s, p)
    {
        TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
        Object = o ?? new RDFResource();
    }

    /// <summary>
    /// Builds a quadruple with SPL flavor
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public RDFQuadruple(RDFContext c, RDFResource s, RDFResource p, RDFLiteral l) : this(c, s, p)
    {
        TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
        Object = l ?? RDFPlainLiteral.Empty;
    }

    /// <summary>
    /// Initializes common quadruple properties
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    private RDFQuadruple(RDFContext c, RDFResource s, RDFResource p)
    {
        #region Guards
        if (p == null)
            throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is null");
        if (p.IsBlank)
            throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is a blank resource");
        #endregion

        Context = c ?? new RDFContext();
        Subject = s ?? new RDFResource();
        Predicate = p;
        LazyQuadrupleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(ToString()));
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
        => other != null && QuadrupleID == other.QuadrupleID;

    /// <summary>
    /// Performs the equality comparison between two quadruples
    /// </summary>
    public override bool Equals(object other)
        => other is RDFQuadruple q && QuadrupleID == q.QuadrupleID;

    /// <summary>
    /// Calculates the hashcode of this quadruple
    /// </summary>
    public override int GetHashCode()
        => QuadrupleID.GetHashCode();
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
    #endregion
}