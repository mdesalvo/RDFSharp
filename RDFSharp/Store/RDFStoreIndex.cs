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
using RDFSharp.Model;

namespace RDFSharp.Store;

/// <summary>
/// RDFStoreIndex represents an automatically managed in-memory index structure for the quadruples of a store.
/// </summary>
internal sealed class RDFStoreIndex : IDisposable
{
    #region Properties
    /// <summary>
    /// Hashed representation of the quadruples
    /// </summary>
    internal Dictionary<long, (long qid, long cid, long sid, long pid, long oid, byte tfv)> Hashes { get; set; }

    /// <summary>
    /// Register of the contexts
    /// </summary>
    internal Dictionary<long, RDFContext> Contexts { get; set; }

    /// <summary>
    /// Register of the resources
    /// </summary>
    internal Dictionary<long, RDFResource> Resources { get; set; }

    /// <summary>
    /// Register of the literals
    /// </summary>
    internal Dictionary<long, RDFLiteral> Literals { get; set; }

    /// <summary>
    /// Index on the contexts of the quadruples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXContexts { get; set; }

    /// <summary>
    /// Index on the subjects of the quadruples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXSubjects { get; set; }

    /// <summary>
    /// Index on the predicates of the quadruples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXPredicates { get; set; }

    /// <summary>
    /// Index on the objects of the quadruples
    /// </summary>
    internal Dictionary<long, HashSet<long>> IDXObjects { get; set; }

    /// <summary>
    /// Index on the literals of the quadruples
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
    internal RDFStoreIndex()
    {
        //Hashes
        Hashes = [];
        //Registers
        Contexts = [];
        Resources = [];
        Literals = [];
        //Indexes
        IDXContexts = [];
        IDXSubjects = [];
        IDXPredicates = [];
        IDXObjects = [];
        IDXLiterals = [];
    }

    /// <summary>
    /// Destroys the index instance
    /// </summary>
    ~RDFStoreIndex()
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
            Contexts = null;
            Resources = null;
            Literals = null;
            //Indexes
            IDXContexts = null;
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
    /// Adds the given quadruple to the index
    /// </summary>
    internal RDFStoreIndex Add(RDFQuadruple quadruple)
    {
        #region Guards
        if (Hashes.ContainsKey(quadruple.QuadrupleID))
            return this;
        #endregion

        //Quadruple (Hash)
        Hashes.Add(quadruple.QuadrupleID, (quadruple.QuadrupleID, quadruple.Context.PatternMemberID,
            quadruple.Subject.PatternMemberID, quadruple.Predicate.PatternMemberID, quadruple.Object.PatternMemberID, (byte)quadruple.TripleFlavor));

        //Context (Register)
        Contexts.TryAdd(quadruple.Context.PatternMemberID, (RDFContext)quadruple.Context);
        //Context (Index)
        if (!IDXContexts.TryGetValue(quadruple.Context.PatternMemberID, out HashSet<long> contextsIndex))
            IDXContexts.Add(quadruple.Context.PatternMemberID, [quadruple.QuadrupleID]);
        else
            contextsIndex.Add(quadruple.QuadrupleID);

        //Subject (Register)
        Resources.TryAdd(quadruple.Subject.PatternMemberID, (RDFResource)quadruple.Subject);
        //Subject (Index)
        if (!IDXSubjects.TryGetValue(quadruple.Subject.PatternMemberID, out HashSet<long> subjectsIndex))
            IDXSubjects.Add(quadruple.Subject.PatternMemberID, [quadruple.QuadrupleID]);
        else
            subjectsIndex.Add(quadruple.QuadrupleID);

        //Predicate (Register)
        Resources.TryAdd(quadruple.Predicate.PatternMemberID, (RDFResource)quadruple.Predicate);
        //Predicate (Index)
        if (!IDXPredicates.TryGetValue(quadruple.Predicate.PatternMemberID, out HashSet<long> predicatesIndex))
            IDXPredicates.Add(quadruple.Predicate.PatternMemberID, [quadruple.QuadrupleID]);
        else
            predicatesIndex.Add(quadruple.QuadrupleID);

        //Object
        if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
        {
            //Register
            Resources.TryAdd(quadruple.Object.PatternMemberID, (RDFResource)quadruple.Object);
            //Index
            if (!IDXObjects.TryGetValue(quadruple.Object.PatternMemberID, out HashSet<long> objectsIndex))
                IDXObjects.Add(quadruple.Object.PatternMemberID, [quadruple.QuadrupleID]);
            else
                objectsIndex.Add(quadruple.QuadrupleID);
        }

        //Literal
        else
        {
            //Register
            Literals.TryAdd(quadruple.Object.PatternMemberID, (RDFLiteral)quadruple.Object);
            //Index
            if (!IDXLiterals.TryGetValue(quadruple.Object.PatternMemberID, out HashSet<long> literalsIndex))
                IDXLiterals.Add(quadruple.Object.PatternMemberID, [quadruple.QuadrupleID]);
            else
                literalsIndex.Add(quadruple.QuadrupleID);
        }

        return this;
    }
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given quadruple from the index
    /// </summary>
    internal RDFStoreIndex Remove(RDFQuadruple quadruple)
    {
        //Quadruple (Hash)
        Hashes.Remove(quadruple.QuadrupleID);

        //Context
        if (IDXContexts.TryGetValue(quadruple.Context.PatternMemberID, out HashSet<long> contextsIndex)
             && contextsIndex.Contains(quadruple.QuadrupleID))
        {
            contextsIndex.Remove(quadruple.QuadrupleID);
            if (contextsIndex.Count == 0)
                IDXContexts.Remove(quadruple.Context.PatternMemberID);
        }

        //Subject
        if (IDXSubjects.TryGetValue(quadruple.Subject.PatternMemberID, out HashSet<long> subjectsIndex)
             && subjectsIndex.Contains(quadruple.QuadrupleID))
        {
            subjectsIndex.Remove(quadruple.QuadrupleID);
            if (subjectsIndex.Count == 0)
                IDXSubjects.Remove(quadruple.Subject.PatternMemberID);
        }

        //Predicate
        if (IDXPredicates.TryGetValue(quadruple.Predicate.PatternMemberID, out HashSet<long> predicatesIndex)
             && predicatesIndex.Contains(quadruple.QuadrupleID))
        {
            predicatesIndex.Remove(quadruple.QuadrupleID);
            if (predicatesIndex.Count == 0)
                IDXPredicates.Remove(quadruple.Predicate.PatternMemberID);
        }

        //Object
        if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
        {
            if (IDXObjects.TryGetValue(quadruple.Object.PatternMemberID, out HashSet<long> objectsIndex)
                 && objectsIndex.Contains(quadruple.QuadrupleID))
            {
                objectsIndex.Remove(quadruple.QuadrupleID);
                if (objectsIndex.Count == 0)
                    IDXObjects.Remove(quadruple.Object.PatternMemberID);
            }
        }

        //Literal
        else
        {
            if (IDXLiterals.TryGetValue(quadruple.Object.PatternMemberID, out HashSet<long> literalsIndex)
                 && literalsIndex.Contains(quadruple.QuadrupleID))
            {
                literalsIndex.Remove(quadruple.QuadrupleID);
                if (literalsIndex.Count == 0)
                    IDXLiterals.Remove(quadruple.Object.PatternMemberID);
            }
        }

        //Context (Register)
        if (!IDXContexts.ContainsKey(quadruple.Context.PatternMemberID))
            Contexts.Remove(quadruple.Context.PatternMemberID);

        //Subject (Register)
        if (!IDXSubjects.ContainsKey(quadruple.Subject.PatternMemberID)
             && !IDXPredicates.ContainsKey(quadruple.Subject.PatternMemberID)
             && !IDXObjects.ContainsKey(quadruple.Subject.PatternMemberID))
        {
            Resources.Remove(quadruple.Subject.PatternMemberID);
        }

        //Predicate (Register)
        if (!IDXSubjects.ContainsKey(quadruple.Predicate.PatternMemberID)
             && !IDXPredicates.ContainsKey(quadruple.Predicate.PatternMemberID)
             && !IDXObjects.ContainsKey(quadruple.Predicate.PatternMemberID))
        {
            Resources.Remove(quadruple.Predicate.PatternMemberID);
        }

        //Object (Register)
        if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
        {
            if (!IDXSubjects.ContainsKey(quadruple.Object.PatternMemberID)
                 && !IDXPredicates.ContainsKey(quadruple.Object.PatternMemberID)
                 && !IDXObjects.ContainsKey(quadruple.Object.PatternMemberID))
            {
                Resources.Remove(quadruple.Object.PatternMemberID);
            }
        }

        //Literal (Register)
        else
        {
            if (!IDXSubjects.ContainsKey(quadruple.Object.PatternMemberID)
                 && !IDXPredicates.ContainsKey(quadruple.Object.PatternMemberID)
                 && !IDXLiterals.ContainsKey(quadruple.Object.PatternMemberID))
            {
                Literals.Remove(quadruple.Object.PatternMemberID);
            }
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
        Contexts.Clear();
        Resources.Clear();
        Literals.Clear();
        //Indexes
        IDXContexts.Clear();
        IDXSubjects.Clear();
        IDXPredicates.Clear();
        IDXObjects.Clear();
        IDXLiterals.Clear();
    }
    #endregion

    #region Select
    /// <summary>
    /// Selects the quadruples indexed by the given context
    /// </summary>
    internal HashSet<long> LookupIndexByContext(RDFContext ctx)
        => IDXContexts.TryGetValue(ctx.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the quadruples indexed by the given subject
    /// </summary>
    internal HashSet<long> LookupIndexBySubject(RDFResource subj)
        => IDXSubjects.TryGetValue(subj.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the quadruples indexed by the given predicate
    /// </summary>
    internal HashSet<long> LookupIndexByPredicate(RDFResource pred)
        => IDXPredicates.TryGetValue(pred.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the quadruples indexed by the given object
    /// </summary>
    internal HashSet<long> LookupIndexByObject(RDFResource obj)
        => IDXObjects.TryGetValue(obj.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;

    /// <summary>
    /// Selects the quadruples indexed by the given literal
    /// </summary>
    internal HashSet<long> LookupIndexByLiteral(RDFLiteral lit)
        => IDXLiterals.TryGetValue(lit.PatternMemberID, out HashSet<long> index) ? index : RDFModelUtilities.EmptyHashSet;
    #endregion

    #endregion
}