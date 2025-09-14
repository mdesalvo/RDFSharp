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

namespace RDFSharp.Model;

/// <summary>
/// RDFGraphIndex represents an automatically managed in-memory index structure for the triples of a graph.
/// </summary>
internal sealed class RDFGraphIndex : IDisposable
{
    #region Properties
    /// <summary>
    /// Hashed representation of the triples
    /// </summary>
    internal Dictionary<long, RDFHashedTriple> Hashes { get; set; }

    /// <summary>
    /// Register of the resources
    /// </summary>
    internal Dictionary<long, RDFResource> Resources { get; set; }

    /// <summary>
    /// Register of the literals
    /// </summary>
    internal Dictionary<long, RDFLiteral> Literals { get; set; }

    /// <summary>
    /// Index on the subjects of the triples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXSubjects { get; set; }

    /// <summary>
    /// Index on the predicates of the triples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXPredicates { get; set; }

    /// <summary>
    /// Index on the objects of the triples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXObjects { get; set; }

    /// <summary>
    /// Index on the literals of the triples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXLiterals { get; set; }

    /// <summary>
    /// Flag indicating that the index has already been disposed
    /// </summary>
    internal bool Disposed { get; set; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds an empty index
    /// </summary>
    internal RDFGraphIndex()
    {
        //Hashes
        Hashes = [];
        //Registers
        Resources = [];
        Literals = [];
        //Indexes
        IDXSubjects = [];
        IDXPredicates = [];
        IDXObjects = [];
        IDXLiterals = [];
    }

    /// <summary>
    /// Destroys the index instance
    /// </summary>
    ~RDFGraphIndex()
        => Dispose(false);
    #endregion

    #region Interfaces
    /// <summary>
    /// Disposes the index (IDisposable)
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the index
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing)
        {
            Clear();

            //Hashes
            Hashes = null;
            //Registers
            Resources = null;
            Literals = null;
            //Indexes
            IDXSubjects = null;
            IDXPredicates = null;
            IDXObjects = null;
            IDXLiterals = null;
        }

        Disposed = true;
    }
    #endregion

    #region Methods

    #region Add
    /// <summary>
    /// Adds the given triple to the index
    /// </summary>
    internal RDFGraphIndex Add(RDFTriple triple)
    {
        //Triple (Hash)
        if (Hashes.ContainsKey(triple.TripleID))
            return this;
        Hashes.Add(triple.TripleID, new RDFHashedTriple(triple));

        //Subject (Register)
        if (!Resources.ContainsKey(triple.Subject.PatternMemberID))
            Resources.Add(triple.Subject.PatternMemberID, (RDFResource)triple.Subject);
        //Subject (Index)
        if (!IDXSubjects.TryGetValue(triple.Subject.PatternMemberID, out HashSet<long> subjectsIndex))
            IDXSubjects.Add(triple.Subject.PatternMemberID, [triple.TripleID]);
        else
            subjectsIndex.Add(triple.TripleID);

        //Predicate (Register)
        if (!Resources.ContainsKey(triple.Predicate.PatternMemberID))
            Resources.Add(triple.Predicate.PatternMemberID, (RDFResource)triple.Predicate);
        //Predicate (Index)
        if (!IDXPredicates.TryGetValue(triple.Predicate.PatternMemberID, out HashSet<long> predicatesIndex))
            IDXPredicates.Add(triple.Predicate.PatternMemberID, [triple.TripleID]);
        else
            predicatesIndex.Add(triple.TripleID);

        //Object
        if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
        {
            //Register
            if (!Resources.ContainsKey(triple.Object.PatternMemberID))
                Resources.Add(triple.Object.PatternMemberID, (RDFResource)triple.Object);
            //Index
            if (!IDXObjects.TryGetValue(triple.Object.PatternMemberID, out HashSet<long> objectsIndex))
                IDXObjects.Add(triple.Object.PatternMemberID, [triple.TripleID]);
            else
                objectsIndex.Add(triple.TripleID);
        }

        //Literal
        else
        {
            //Register
            if (!Literals.ContainsKey(triple.Object.PatternMemberID))
                Literals.Add(triple.Object.PatternMemberID, (RDFLiteral)triple.Object);
            //Index
            if (!IDXLiterals.TryGetValue(triple.Object.PatternMemberID, out HashSet<long> literalsIndex))
                IDXLiterals.Add(triple.Object.PatternMemberID, [triple.TripleID]);
            else
                literalsIndex.Add(triple.TripleID);
        }

        return this;
    }
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given triple from the index
    /// </summary>
    internal RDFGraphIndex Remove(RDFTriple triple)
    {
        //Triple (Hash)
        Hashes.Remove(triple.TripleID);

        //Subject (Index)
        if (IDXSubjects.TryGetValue(triple.Subject.PatternMemberID, out HashSet<long> subjectsIndex)
            && subjectsIndex.Contains(triple.TripleID))
        {
            subjectsIndex.Remove(triple.TripleID);
            if (subjectsIndex.Count == 0)
                IDXSubjects.Remove(triple.Subject.PatternMemberID);
        }

        //Predicate (Index)
        if (IDXPredicates.TryGetValue(triple.Predicate.PatternMemberID, out HashSet<long> predicatesIndex)
            && predicatesIndex.Contains(triple.TripleID))
        {
            predicatesIndex.Remove(triple.TripleID);
            if (predicatesIndex.Count == 0)
                IDXPredicates.Remove(triple.Predicate.PatternMemberID);
        }

        //Object (Index)
        if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
        {
            if (IDXObjects.TryGetValue(triple.Object.PatternMemberID, out HashSet<long> objectsIndex)
                && objectsIndex.Contains(triple.TripleID))
            {
                objectsIndex.Remove(triple.TripleID);
                if (objectsIndex.Count == 0)
                    IDXObjects.Remove(triple.Object.PatternMemberID);
            }
        }

        //Literal (Index)
        else
        {
            if (IDXLiterals.TryGetValue(triple.Object.PatternMemberID, out HashSet<long> literalsIndex)
                && literalsIndex.Contains(triple.TripleID))
            {
                literalsIndex.Remove(triple.TripleID);
                if (literalsIndex.Count == 0)
                    IDXLiterals.Remove(triple.Object.PatternMemberID);
            }
        }

        //Subject (Register)
        if (!IDXSubjects.ContainsKey(triple.Subject.PatternMemberID)
            && !IDXPredicates.ContainsKey(triple.Subject.PatternMemberID)
            && !IDXObjects.ContainsKey(triple.Subject.PatternMemberID))
            Resources.Remove(triple.Subject.PatternMemberID);

        //Predicate (Register)
        if (!IDXSubjects.ContainsKey(triple.Predicate.PatternMemberID)
            && !IDXPredicates.ContainsKey(triple.Predicate.PatternMemberID)
            && !IDXObjects.ContainsKey(triple.Predicate.PatternMemberID))
            Resources.Remove(triple.Predicate.PatternMemberID);

        //Object (Register)
        if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
        {
            if (!IDXSubjects.ContainsKey(triple.Object.PatternMemberID)
                && !IDXPredicates.ContainsKey(triple.Object.PatternMemberID)
                && !IDXObjects.ContainsKey(triple.Object.PatternMemberID))
                Resources.Remove(triple.Object.PatternMemberID);
        }

        //Literal (Register)
        else
        {
            if (!IDXSubjects.ContainsKey(triple.Object.PatternMemberID)
                && !IDXPredicates.ContainsKey(triple.Object.PatternMemberID)
                && !IDXLiterals.ContainsKey(triple.Object.PatternMemberID))
                Literals.Remove(triple.Object.PatternMemberID);
        }

        return this;
    }

    /// <summary>
    /// Clears the index
    /// </summary>
    internal void Clear()
    {
        //Hash
        Hashes.Clear();
        //Registers
        Resources.Clear();
        Literals.Clear();
        //Indexes
        IDXSubjects.Clear();
        IDXPredicates.Clear();
        IDXObjects.Clear();
        IDXLiterals.Clear();
    }
    #endregion

    #region Select
    /// <summary>
    /// Selects the triples indexed by the given subject
    /// </summary>
    internal HashSet<long> LookupIndexBySubject(RDFResource subj)
        => IDXSubjects.TryGetValue(subj.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the triples indexed by the given predicate
    /// </summary>
    internal HashSet<long> LookupIndexByPredicate(RDFResource pred)
        => IDXPredicates.TryGetValue(pred.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the triples indexed by the given object
    /// </summary>
    internal HashSet<long> LookupIndexByObject(RDFResource obj)
        => IDXObjects.TryGetValue(obj.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the triples indexed by the given literal
    /// </summary>
    internal HashSet<long> LookupIndexByLiteral(RDFLiteral lit)
        => IDXLiterals.TryGetValue(lit.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;
    #endregion

    #endregion
}