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
    /// Removes the quadruples which satisfy the given combination of CSPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
    /// </summary>
    public abstract RDFStore RemoveQuadruples(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null);

    /// <summary>
    /// Clears the quadruples of the store
    /// </summary>
    public abstract void ClearQuadruples();
    #endregion

    #region Select
    /// <summary>
    /// Checks if the store contains the given quadruple
    /// </summary>
    public abstract bool ContainsQuadruple(RDFQuadruple quadruple);

    /// <summary>
    /// Selects the quadruples which satisfy the given combination of CSPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public abstract List<RDFQuadruple> SelectQuadruples(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null);

    /// <summary>
    /// Gets a memory store containing quadruples which satisfy the given combination of CSPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params, if given, must be mutually exclusive!)
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public RDFMemoryStore this[RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null]
        => new RDFMemoryStore(SelectQuadruples(c, s, p, o, l));

    /// <summary>
    /// Gets a list containing the graphs saved in the store
    /// </summary>
    public List<RDFGraph> ExtractGraphs()
    {
        Dictionary<long, RDFGraph> graphs = [];
        foreach (RDFQuadruple q in SelectQuadruples())
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
        foreach (RDFQuadruple q in SelectQuadruples())
            contexts.TryAdd(q.Context.PatternMemberID, q.Context);
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
        foreach (RDFQuadruple q in SelectQuadruples())
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