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
using System.Data;
using System.Linq;

namespace RDFSharp.Model;

/// <summary>
/// RDFGraphIndex represents an automatically managed in-memory index structure for the triples of a graph.
/// </summary>
internal sealed class RDFGraphIndex : IDisposable
{
    #region Properties
    /// <summary>
    /// Table storing the hashed representations of the index's triples
    /// </summary>
    internal DataTable Triples { get; set; }

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
        Triples = new DataTable();
        Triples.Columns.Add("?TID", typeof(long));
        Triples.Columns.Add("?SID", typeof(long));
        Triples.Columns.Add("?PID", typeof(long));
        Triples.Columns.Add("?OID", typeof(long));
        Triples.Columns.Add("?TFV", typeof(RDFModelEnums.RDFTripleFlavors));
        Triples.PrimaryKey = [Triples.Columns["?TID"]];
        Triples.ExtendedProperties.Add("RES", new Dictionary<long, RDFResource>());
        Triples.ExtendedProperties.Add("LIT", new Dictionary<long, RDFLiteral>());
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
            Triples?.Clear();
            Triples?.ExtendedProperties.Clear();
            Triples?.Dispose();
            Triples = null;
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
        #region Guards
        if (Triples.Rows.Find(triple.TripleID) is not null)
            return this;
        #endregion

        //Merge the given triple into the table
        DataRow addRow = Triples.NewRow();
        addRow["?TID"] = triple.TripleID;
        addRow["?SID"] = triple.Subject.PatternMemberID;
        addRow["?PID"] = triple.Predicate.PatternMemberID;
        addRow["?OID"] = triple.Object.PatternMemberID;
        addRow["?TFV"] = triple.TripleFlavor;
        Triples.Rows.Add(addRow);

        //Update metadata with elements of the given triple
        ((Dictionary<long, RDFResource>)Triples.ExtendedProperties["RES"]).TryAdd(triple.Subject.PatternMemberID, (RDFResource)triple.Subject);
        ((Dictionary<long, RDFResource>)Triples.ExtendedProperties["RES"]).TryAdd(triple.Predicate.PatternMemberID, (RDFResource)triple.Predicate);
        if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            ((Dictionary<long, RDFResource>)Triples.ExtendedProperties["RES"]).TryAdd(triple.Object.PatternMemberID, (RDFResource)triple.Object);
        else
            ((Dictionary<long, RDFLiteral>)Triples.ExtendedProperties["LIT"]).TryAdd(triple.Object.PatternMemberID, (RDFLiteral)triple.Object);

        return this;
    }
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given triple from the index
    /// </summary>
    internal RDFGraphIndex Remove(RDFTriple triple)
    {
        //Remove the given triple from the table
        DataRow delRow = Triples.Rows.Find(triple.TripleID);
        if (delRow is null)
            return this;
        Triples.Rows.Remove(delRow);

        //Update metadata with elements of the given triple
        if (Triples.Select($"?SID == {triple.Subject.PatternMemberID} OR ?PID == {triple.Subject.PatternMemberID} OR (?OID == {triple.Subject.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO})").Length == 0)
            ((Dictionary<long, RDFResource>)Triples.ExtendedProperties["RES"]).Remove(triple.Subject.PatternMemberID);
        if (Triples.Select($"?SID == {triple.Predicate.PatternMemberID} OR ?PID == {triple.Predicate.PatternMemberID} OR (?OID == {triple.Predicate.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO})").Length == 0)
            ((Dictionary<long, RDFResource>)Triples.ExtendedProperties["RES"]).Remove(triple.Predicate.PatternMemberID);
        switch (triple.TripleFlavor)
        {
            case RDFModelEnums.RDFTripleFlavors.SPO:
                if (Triples.Select($"?SID == {triple.Object.PatternMemberID} OR ?PID == {triple.Object.PatternMemberID} OR (?OID == {triple.Object.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO})").Length == 0)
                    ((Dictionary<long, RDFResource>)Triples.ExtendedProperties["RES"]).Remove(triple.Object.PatternMemberID);
                break;
            case RDFModelEnums.RDFTripleFlavors.SPL:
                if (Triples.Select($"?OID == {triple.Object.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPL}").Length == 0)
                    ((Dictionary<long, RDFLiteral>)Triples.ExtendedProperties["LIT"]).Remove(triple.Object.PatternMemberID);
                break;
        }

        return this;
    }
    #endregion

    #endregion
}