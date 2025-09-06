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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Store;

/// <summary>
/// RDFStore represents an abstract store in the RDF model.
/// </summary>
public abstract class RDFStore : RDFDataSource, IEquatable<RDFStore>
{
    #region Properties
    /// <summary>
    /// Unique representation of the store
    /// </summary>
    public long StoreID { get; set; }

    /// <summary>
    /// Type of the store
    /// </summary>
    public string StoreType { get; set; }

    /// <summary>
    /// Count of the store's quadruples
    /// </summary>
    public abstract long QuadruplesCount { get; }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the store
    /// </summary>
    public override string ToString()
        => StoreType;

    /// <summary>
    /// Performs the equality comparison between two stores
    /// </summary>
    public bool Equals(RDFStore other)
        => other != null && StoreID.Equals(other.StoreID);
    #endregion

    #region Methods

    #region Add
    /// <summary>
    /// Merges the given graph into the store, avoiding duplicate insertions
    /// </summary>
    public abstract RDFStore MergeGraph(RDFGraph graph);

    /// <summary>
    /// Adds the given quadruple to the store, avoiding duplicate insertions
    /// </summary>
    public abstract RDFStore AddQuadruple(RDFQuadruple quadruple);
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given quadruple from the store
    /// </summary>
    public abstract RDFStore RemoveQuadruple(RDFQuadruple quadruple);

    /// <summary>
    /// Removes the quadruples with the given context
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContext(RDFContext ctx);

    /// <summary>
    /// Removes the quadruples with the given subject
    /// </summary>
    public abstract RDFStore RemoveQuadruplesBySubject(RDFResource subj);

    /// <summary>
    /// Removes the quadruples with the given (non-blank) predicate
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByPredicate(RDFResource pred);

    /// <summary>
    /// Removes the quadruples with the given resource as object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByObject(RDFResource obj);

    /// <summary>
    /// Removes the quadruples with the given literal as object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByLiteral(RDFLiteral lit);

    /// <summary>
    /// Removes the quadruples with the given context and subject
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextSubject(RDFContext ctx, RDFResource subj);

    /// <summary>
    /// Removes the quadruples with the given context and predicate
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextPredicate(RDFContext ctx, RDFResource pred);

    /// <summary>
    /// Removes the quadruples with the given context and object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextObject(RDFContext ctx, RDFResource obj);

    /// <summary>
    /// Removes the quadruples with the given context and literal
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextLiteral(RDFContext ctx, RDFLiteral lit);

    /// <summary>
    /// Removes the quadruples with the given context, subject and predicate
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextSubjectPredicate(RDFContext ctx, RDFResource subj, RDFResource pred);

    /// <summary>
    /// Removes the quadruples with the given context, subject and object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextSubjectObject(RDFContext ctx, RDFResource subj, RDFResource obj);

    /// <summary>
    /// Removes the quadruples with the given context, subject and literal
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextSubjectLiteral(RDFContext ctx, RDFResource subj, RDFLiteral lit);

    /// <summary>
    /// Removes the quadruples with the given context, predicate and object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextPredicateObject(RDFContext ctx, RDFResource pred, RDFResource obj);

    /// <summary>
    /// Removes the quadruples with the given context, predicate and literal
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByContextPredicateLiteral(RDFContext ctx, RDFResource pred, RDFLiteral lit);

    /// <summary>
    /// Removes the quadruples with the given subject and predicate
    /// </summary>
    public abstract RDFStore RemoveQuadruplesBySubjectPredicate(RDFResource subj, RDFResource pred);

    /// <summary>
    /// Removes the quadruples with the given subject and object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesBySubjectObject(RDFResource subj, RDFResource obj);

    /// <summary>
    /// Removes the quadruples with the given subject and literal
    /// </summary>
    public abstract RDFStore RemoveQuadruplesBySubjectLiteral(RDFResource subj, RDFLiteral lit);

    /// <summary>
    /// Removes the quadruples with the given predicate and object
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByPredicateObject(RDFResource pred, RDFResource obj);

    /// <summary>
    /// Removes the quadruples with the given predicate and literal
    /// </summary>
    public abstract RDFStore RemoveQuadruplesByPredicateLiteral(RDFResource pred, RDFLiteral lit);

    /// <summary>
    /// Clears the quadruples of the store
    /// </summary>
    public abstract void ClearQuadruples();
    #endregion

    #region Select
    /// <summary>
    /// Gets a memory store containing quadruples with the specified combination of CSPOL accessors<br/>
    /// (null values are handled as * selectors. Obj and Lit params must be mutually exclusive!)
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public RDFMemoryStore this[RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit]
    {
        get
        {
            #region Guards
            if (obj != null && lit != null)
                throw new RDFStoreException("Cannot access a store when both object and literals are given: they must be mutually exclusive!");
            #endregion

            return SelectQuadruples(ctx, subj, pred, obj, lit);
        }
    }

    /// <summary>
    /// Checks if the store contains the given quadruple
    /// </summary>
    public abstract bool ContainsQuadruple(RDFQuadruple quadruple);

    /// <summary>
    /// Gets a memory store containing quadruples satisfying the given pattern
    /// </summary>
    public abstract RDFMemoryStore SelectQuadruples(RDFContext ctx, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit);

    /// <summary>
    /// Gets a store containing all quadruples
    /// </summary>
    public RDFMemoryStore SelectAllQuadruples()
        => SelectQuadruples(null, null, null, null, null);

    /// <summary>
    /// Gets a memory store containing quadruples with the specified context
    /// </summary>
    public RDFMemoryStore SelectQuadruplesByContext(RDFContext ctx)
        => SelectQuadruples(ctx, null, null, null, null);

    /// <summary>
    /// Gets a memory store containing quadruples with the specified subject
    /// </summary>
    public RDFMemoryStore SelectQuadruplesBySubject(RDFResource subj)
        => SelectQuadruples(null, subj, null, null, null);

    /// <summary>
    /// Gets a memory store containing quadruples with the specified predicate
    /// </summary>
    public RDFMemoryStore SelectQuadruplesByPredicate(RDFResource pred)
        => SelectQuadruples(null, null, pred, null, null);

    /// <summary>
    /// Gets a memory store containing quadruples with the specified object
    /// </summary>
    public RDFMemoryStore SelectQuadruplesByObject(RDFResource obj)
        => SelectQuadruples(null, null, null, obj, null);

    /// <summary>
    /// Gets a memory store containing quadruples with the specified literal
    /// </summary>
    public RDFMemoryStore SelectQuadruplesByLiteral(RDFLiteral lit)
        => SelectQuadruples(null, null, null, null, lit);

    /// <summary>
    /// Gets a list containing the graphs saved in the store
    /// </summary>
    public List<RDFGraph> ExtractGraphs()
    {
        Dictionary<long, RDFGraph> graphs = [];
        foreach (RDFQuadruple q in this as RDFMemoryStore ?? SelectAllQuadruples())
        {
            // Step 1: Cache-Update
            if (!graphs.TryGetValue(q.Context.PatternMemberID, out RDFGraph graph))
            {
                graph = new RDFGraph();
                graphs.Add(q.Context.PatternMemberID, graph);
                graphs[q.Context.PatternMemberID].SetContext(((RDFContext)q.Context).Context);
            }

            graph.AddTriple(q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                ? new RDFTriple((RDFResource)q.Subject, (RDFResource)q.Predicate, (RDFResource)q.Object)
                : new RDFTriple((RDFResource)q.Subject, (RDFResource)q.Predicate, (RDFLiteral)q.Object));
        }
        return [.. graphs.Values];
    }

    /// <summary>
    /// Gets a list containing the contexts saved in the store
    /// </summary>
    public List<RDFContext> ExtractContexts()
    {
        Dictionary<long, RDFPatternMember> contexts = [];
        foreach (RDFQuadruple q in this as RDFMemoryStore ?? SelectAllQuadruples())
        {
            if (!contexts.ContainsKey(q.Context.PatternMemberID))
                contexts.Add(q.Context.PatternMemberID, q.Context);
        }
        return [.. contexts.Values.OfType<RDFContext>()];
    }
    #endregion

    #region Convert

    #region Export
    /// <summary>
    /// Writes the store into a file in the given RDF format
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public void ToFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
    {
        #region Guards
        if (string.IsNullOrWhiteSpace(filepath))
            throw new RDFStoreException("Cannot write RDF store to file because given \"filepath\" parameter is null or empty.");
        #endregion

        switch (rdfFormat)
        {
            case RDFStoreEnums.RDFFormats.NQuads:
                RDFNQuads.Serialize(this, filepath);
                break;
            case RDFStoreEnums.RDFFormats.TriX:
                RDFTriX.Serialize(this, filepath);
                break;
            case RDFStoreEnums.RDFFormats.TriG:
                RDFTriG.Serialize(this, filepath);
                break;
        }
    }

    /// <summary>
    /// Asynchronously writes the store into a file in the given RDF format
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public Task ToFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath)
        => Task.Run(() => ToFile(rdfFormat, filepath));

    /// <summary>
    /// Writes the store into a stream in the given RDF format
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public void ToStream(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream)
    {
        #region Guards
        if (outputStream == null)
            throw new RDFStoreException("Cannot write RDF store to stream because given \"outputStream\" parameter is null.");
        #endregion

        switch (rdfFormat)
        {
            case RDFStoreEnums.RDFFormats.NQuads:
                RDFNQuads.Serialize(this, outputStream);
                break;
            case RDFStoreEnums.RDFFormats.TriX:
                RDFTriX.Serialize(this, outputStream);
                break;
            case RDFStoreEnums.RDFFormats.TriG:
                RDFTriG.Serialize(this, outputStream);
                break;
        }
    }

    /// <summary>
    /// Asynchronously writes the store into a stream in the given RDF format
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public Task ToStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream outputStream)
        => Task.Run(() => ToStream(rdfFormat, outputStream));

    /// <summary>
    /// Writes the store into a datatable with "Context-Subject-Predicate-Object" columns
    /// </summary>
    public DataTable ToDataTable()
    {
        //Create the structure of the result datatable
        DataTable result = new DataTable(ToString());
        result.Columns.Add("?CONTEXT", typeof(string));
        result.Columns.Add("?SUBJECT", typeof(string));
        result.Columns.Add("?PREDICATE", typeof(string));
        result.Columns.Add("?OBJECT", typeof(string));

        //Iterate the quadruples of the store to populate the result datatable
        result.BeginLoadData();
        foreach (RDFQuadruple q in SelectAllQuadruples())
        {
            DataRow newRow = result.NewRow();
            newRow["?CONTEXT"] = q.Context.ToString();
            newRow["?SUBJECT"] = q.Subject.ToString();
            newRow["?PREDICATE"] = q.Predicate.ToString();
            newRow["?OBJECT"] = q.Object.ToString();
            result.Rows.Add(newRow);
        }
        result.EndLoadData();

        return result;
    }

    /// <summary>
    /// Asynchronously writes the store into a datatable with "Context-Subject-Predicate-Object" columns
    /// </summary>
    public Task<DataTable> ToDataTableAsync()
        => Task.Run(ToDataTable);
    #endregion

    #endregion

    #endregion
}