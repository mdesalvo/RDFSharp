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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RDFSharp.Store;

/// <summary>
/// RDFMemoryStore represents an in-memory RDF store engine.
/// </summary>
public sealed class RDFMemoryStore : RDFStore, IEnumerable<RDFQuadruple>, IDisposable
{
    #region Properties
    /// <summary>
    /// Count of the store's quadruples
    /// </summary>
    public override long QuadruplesCount
        => Quadruples.Rows.Count;

    /// <summary>
    /// Gets the enumerator on the store's quadruples for iteration
    /// </summary>
    public IEnumerator<RDFQuadruple> QuadruplesEnumerator
    {
        get
        {
            Dictionary<long, RDFContext> contexts = (Dictionary<long, RDFContext>)Quadruples.ExtendedProperties["CTX"]!;
            Dictionary<long, RDFResource> resources = (Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"]!;
            Dictionary<long, RDFLiteral> literals = (Dictionary<long, RDFLiteral>)Quadruples.ExtendedProperties["LIT"]!;
            foreach (DataRow quadruple in Quadruples.Rows)
            {
                yield return quadruple.Field<byte>("TFV") == 1
                    ? new RDFQuadruple(contexts[quadruple.Field<long>("CID")], resources[quadruple.Field<long>("SID")], resources[quadruple.Field<long>("PID")], resources[quadruple.Field<long>("OID")])
                    : new RDFQuadruple(contexts[quadruple.Field<long>("CID")], resources[quadruple.Field<long>("SID")], resources[quadruple.Field<long>("PID")], literals[quadruple.Field<long>("OID")]);
            }
        }
    }

    /// <summary>
    /// Table storing the hashed representations of the quadruples
    /// </summary>
    internal DataTable Quadruples { get; set; }

    /// <summary>
    /// Identifier of the store
    /// </summary>
    internal string StoreGUID { get; }

    /// <summary>
    /// Flag indicating that the store has already been disposed
    /// </summary>
    internal bool Disposed { get; set; }
    #endregion

    #region Ctors
    /// <summary>
    ///Builds an empty memory store
    /// </summary>
    public RDFMemoryStore()
    {
        StoreType = "MEMORY";
        StoreGUID = Guid.NewGuid().ToString("N");
        StoreID = RDFModelUtilities.CreateHash(ToString());
        Quadruples = new DataTable();
        Quadruples.Columns.Add("QID", typeof(long));
        Quadruples.Columns.Add("CID", typeof(long));
        Quadruples.Columns.Add("SID", typeof(long));
        Quadruples.Columns.Add("PID", typeof(long));
        Quadruples.Columns.Add("OID", typeof(long));
        Quadruples.Columns.Add("TFV", typeof(byte));
        Quadruples.PrimaryKey = [Quadruples.Columns["QID"]];
        Quadruples.ExtendedProperties.Add("CTX", new Dictionary<long, RDFContext>());
        Quadruples.ExtendedProperties.Add("RES", new Dictionary<long, RDFResource>());
        Quadruples.ExtendedProperties.Add("LIT", new Dictionary<long, RDFLiteral>());
    }

    /// <summary>
    /// Builds a memory store with the given list of quadruples
    /// </summary>
    public RDFMemoryStore(List<RDFQuadruple> quadruples) : this()
        => quadruples?.ForEach(q => AddQuadruple(q));

    /// <summary>
    /// Destroys the memory store instance
    /// </summary>
    ~RDFMemoryStore()
        => Dispose(false);
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the memory store
    /// </summary>
    public override string ToString()
        => $"{base.ToString()}|ID={StoreGUID}";

    /// <summary>
    /// Performs the equality comparison between two memory stores
    /// </summary>
    public bool Equals(RDFMemoryStore other)
    {
        if (other == null || QuadruplesCount != other.QuadruplesCount)
            return false;
        return StoreID == other.StoreID || this.All(other.ContainsQuadruple);
    }

    /// <summary>
    /// Exposes a typed enumerator on the store's quadruples
    /// </summary>
    IEnumerator<RDFQuadruple> IEnumerable<RDFQuadruple>.GetEnumerator()
        => QuadruplesEnumerator;

    /// <summary>
    /// Exposes an untyped enumerator on the store's quadruples
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
        => QuadruplesEnumerator;

    /// <summary>
    /// Disposes the memory store (IDisposable)
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the memory store
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing)
        {
            Quadruples?.Clear();
            Quadruples?.ExtendedProperties.Clear();
            Quadruples?.Dispose();
            Quadruples = null;
        }

        Disposed = true;
    }
    #endregion

    #region Methods

    #region Add
    /// <summary>
    /// Merges the given graph into the store, avoiding duplicate insertions
    /// </summary>
    public override RDFStore MergeGraph(RDFGraph graph)
    {
        if (graph != null)
        {
            RDFContext graphCtx = new RDFContext(graph.Context);
            foreach (RDFTriple triple in graph)
                AddQuadruple(new RDFQuadruple(graphCtx, triple));
        }
        return this;
    }

    /// <summary>
    /// Adds the given quadruple to the store, avoiding duplicate insertions
    /// </summary>
    public override RDFStore AddQuadruple(RDFQuadruple quadruple)
    {
        if (quadruple != null)
        {
            #region Guards
            if (Quadruples.Rows.Find(quadruple.QuadrupleID) is not null)
                return this;
            #endregion

            //Merge the given quadruple into the table
            DataRow addRow = Quadruples.NewRow();
            addRow["QID"] = quadruple.QuadrupleID;
            addRow["CID"] = quadruple.Context.PatternMemberID;
            addRow["SID"] = quadruple.Subject.PatternMemberID;
            addRow["PID"] = quadruple.Predicate.PatternMemberID;
            addRow["OID"] = quadruple.Object.PatternMemberID;
            addRow["TFV"] = quadruple.TripleFlavor;
            Quadruples.Rows.Add(addRow);

            //Update metadata with elements of the given quadruple
            ((Dictionary<long, RDFContext>)Quadruples.ExtendedProperties["CTX"])!.TryAdd(quadruple.Context.PatternMemberID, (RDFContext)quadruple.Context);
            ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.TryAdd(quadruple.Subject.PatternMemberID, (RDFResource)quadruple.Subject);
            ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.TryAdd(quadruple.Predicate.PatternMemberID, (RDFResource)quadruple.Predicate);
            if (quadruple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.TryAdd(quadruple.Object.PatternMemberID, (RDFResource)quadruple.Object);
            else
                ((Dictionary<long, RDFLiteral>)Quadruples.ExtendedProperties["LIT"])!.TryAdd(quadruple.Object.PatternMemberID, (RDFLiteral)quadruple.Object);
        }
        return this;
    }
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given quadruple from the store
    /// </summary>
    public override RDFStore RemoveQuadruple(RDFQuadruple quadruple)
    {
        if (quadruple != null)
        {
            //Remove the given triple from the table
            DataRow delRow = Quadruples.Rows.Find(quadruple.QuadrupleID);
            if (delRow is null)
                return this;
            Quadruples.Rows.Remove(delRow);

            //Update metadata with elements from the given triple
            if (Quadruples.Select($"CID = {quadruple.Context.PatternMemberID}").Length == 0)
                ((Dictionary<long, RDFContext>)Quadruples.ExtendedProperties["CTX"])!.Remove(quadruple.Context.PatternMemberID);
            if (Quadruples.Select($"SID = {quadruple.Subject.PatternMemberID} OR PID = {quadruple.Subject.PatternMemberID} OR (OID = {quadruple.Subject.PatternMemberID} AND TFV = 1)").Length == 0)
                ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.Remove(quadruple.Subject.PatternMemberID);
            if (Quadruples.Select($"SID = {quadruple.Predicate.PatternMemberID} OR PID = {quadruple.Predicate.PatternMemberID} OR (OID = {quadruple.Predicate.PatternMemberID} AND TFV = 1)").Length == 0)
                ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.Remove(quadruple.Predicate.PatternMemberID);
            switch (quadruple.TripleFlavor)
            {
                case RDFModelEnums.RDFTripleFlavors.SPO:
                    if (Quadruples.Select($"SID = {quadruple.Object.PatternMemberID} OR PID = {quadruple.Object.PatternMemberID} OR (OID = {quadruple.Object.PatternMemberID} AND TFV = 1)").Length == 0)
                        ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.Remove(quadruple.Object.PatternMemberID);
                    break;
                case RDFModelEnums.RDFTripleFlavors.SPL:
                    if (Quadruples.Select($"OID = {quadruple.Object.PatternMemberID} AND TFV = 2").Length == 0)
                        ((Dictionary<long, RDFLiteral>)Quadruples.ExtendedProperties["LIT"])!.Remove(quadruple.Object.PatternMemberID);
                    break;
            }
        }
        return this;
    }

    /// <summary>
    /// Removes the quadruples which satisfy the given combination of CSPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params must be mutually exclusive!)
    /// </summary>
    public override RDFStore RemoveQuadruples(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
    {
        foreach (RDFQuadruple quadruple in SelectQuadruples(c, s, p, o, l))
            RemoveQuadruple(quadruple);
        return this;
    }

    /// <summary>
    /// Clears the quadruples of the store
    /// </summary>
    public override void ClearQuadruples()
    {
        Quadruples.Clear();
        ((Dictionary<long, RDFContext>)Quadruples.ExtendedProperties["CTX"])!.Clear();
        ((Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"])!.Clear();
        ((Dictionary<long, RDFLiteral>)Quadruples.ExtendedProperties["LIT"])!.Clear();
    }
    #endregion

    #region Select
    /// <summary>
    /// Checks if the store contains the given quadruple
    /// </summary>
    public override bool ContainsQuadruple(RDFQuadruple quadruple)
        => quadruple is not null && Quadruples.Rows.Find(quadruple.QuadrupleID) is not null;

    /// <summary>
    /// Selects the quadruples which satisfy the given combination of CSPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params must be mutually exclusive!)
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public override List<RDFQuadruple> SelectQuadruples(RDFContext c=null, RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
    {
        #region Guards
        if (o != null && l != null)
            throw new RDFStoreException("Cannot access a store when both object and literals are given: they must be mutually exclusive!");
        #endregion

        //Query
        Dictionary<long, RDFContext> contexts = (Dictionary<long, RDFContext>)Quadruples.ExtendedProperties["CTX"]!;
        Dictionary<long, RDFResource> resources = (Dictionary<long, RDFResource>)Quadruples.ExtendedProperties["RES"]!;
        Dictionary<long, RDFLiteral> literals = (Dictionary<long, RDFLiteral>)Quadruples.ExtendedProperties["LIT"]!;
        StringBuilder queryFilters = new StringBuilder(4);
        if (c != null) queryFilters.Append('C');
        if (s != null) queryFilters.Append('S');
        if (p != null) queryFilters.Append('P');
        if (o != null) queryFilters.Append('O');
        if (l != null) queryFilters.Append('L');
        DataRow[] selectedQuadruples = queryFilters.ToString() switch
        {
            "C"    => Quadruples.Select($"CID = {c!.PatternMemberID}"),
            "S"    => Quadruples.Select($"SID = {s!.PatternMemberID}"),
            "P"    => Quadruples.Select($"PID = {p!.PatternMemberID}"),
            "O"    => Quadruples.Select($"OID = {o!.PatternMemberID} AND TFV = 1"),
            "L"    => Quadruples.Select($"OID = {l!.PatternMemberID} AND TFV = 2"),
            "CS"   => Quadruples.Select($"CID = {c!.PatternMemberID} AND SID = {s!.PatternMemberID}"),
            "CP"   => Quadruples.Select($"CID = {c!.PatternMemberID} AND PID = {p!.PatternMemberID}"),
            "CO"   => Quadruples.Select($"CID = {c!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "CL"   => Quadruples.Select($"CID = {c!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            "CSP"  => Quadruples.Select($"CID = {c!.PatternMemberID} AND SID = {s!.PatternMemberID} AND PID = {p!.PatternMemberID}"),
            "CSO"  => Quadruples.Select($"CID = {c!.PatternMemberID} AND SID = {s!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "CSL"  => Quadruples.Select($"CID = {c!.PatternMemberID} AND SID = {s!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            "CPO"  => Quadruples.Select($"CID = {c!.PatternMemberID} AND PID = {p!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "CPL"  => Quadruples.Select($"CID = {c!.PatternMemberID} AND PID = {p!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            "CSPO" => Quadruples.Select($"CID = {c!.PatternMemberID} AND SID = {s!.PatternMemberID} AND PID = {p!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "CSPL" => Quadruples.Select($"CID = {c!.PatternMemberID} AND SID = {s!.PatternMemberID} AND PID = {p!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            "SP"   => Quadruples.Select($"SID = {s!.PatternMemberID} AND PID = {p!.PatternMemberID}"),
            "SO"   => Quadruples.Select($"SID = {s!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "SL"   => Quadruples.Select($"SID = {s!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            "PO"   => Quadruples.Select($"PID = {p!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "PL"   => Quadruples.Select($"PID = {p!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            "SPO"  => Quadruples.Select($"SID = {s!.PatternMemberID} AND PID = {p!.PatternMemberID} AND OID = {o!.PatternMemberID} AND TFV = 1"),
            "SPL"  => Quadruples.Select($"SID = {s!.PatternMemberID} AND PID = {p!.PatternMemberID} AND OID = {l!.PatternMemberID} AND TFV = 2"),
            _      => [.. Quadruples.Rows.Cast<DataRow>()]
        };

        //Decompression
        List<RDFQuadruple> result = new List<RDFQuadruple>(selectedQuadruples.Length);
        foreach (DataRow selectedQuadruple in selectedQuadruples)
        {
            RDFContext ctx = contexts[selectedQuadruple.Field<long>("CID")];
            RDFResource subj = resources[selectedQuadruple.Field<long>("SID")];
            RDFResource pred = resources[selectedQuadruple.Field<long>("PID")];
            switch (selectedQuadruple.Field<byte>("TFV"))
            {
                case 1: //SPO
                    RDFResource obj = resources[selectedQuadruple.Field<long>("OID")];
                    result.Add(new RDFQuadruple(ctx, subj, pred, obj));
                    break;
                case 2: //SPL
                    RDFLiteral lit = literals[selectedQuadruple.Field<long>("OID")];
                    result.Add(new RDFQuadruple(ctx, subj, pred, lit));
                    break;
            }
        }
        return result;
    }
    #endregion

    #region Set
    /// <summary>
    /// Builds a new intersection store from this store and a given one
    /// </summary>
    public RDFMemoryStore IntersectWith(RDFStore store)
    {
        RDFMemoryStore result = new RDFMemoryStore();
        if (store != null)
        {
            //Add intersection quadruples
            foreach (RDFQuadruple q in this)
            {
                if (store.ContainsQuadruple(q))
                    result.AddQuadruple(q);
            }
        }
        return result;
    }

    /// <summary>
    /// Builds a new union store from this store and a given one
    /// </summary>
    public RDFMemoryStore UnionWith(RDFStore store)
    {
        RDFMemoryStore result = new RDFMemoryStore();

        //Add quadruples from this store
        foreach (RDFQuadruple q in this)
            result.AddQuadruple(q);

        //Manage the given store
        if (store != null)
        {
            //Add quadruples from the given store
            foreach (RDFQuadruple q in store as RDFMemoryStore ?? store[null, null, null, null, null])
                result.AddQuadruple(q);
        }

        return result;
    }

    /// <summary>
    /// Builds a new difference store from this store and a given one
    /// </summary>
    public RDFMemoryStore DifferenceWith(RDFStore store)
    {
        RDFMemoryStore result = new RDFMemoryStore();

        if (store != null)
        {
            //Add difference quadruples
            foreach (RDFQuadruple q in this)
            {
                if (!store.ContainsQuadruple(q))
                    result.AddQuadruple(q);
            }
        }
        else
        {
            //Add quadruples from this store
            foreach (RDFQuadruple q in this)
                result.AddQuadruple(q);
        }

        return result;
    }
    #endregion

    #region Convert

    #region Import
    /// <summary>
    /// Reads a memory store from a file of the given RDF format.
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static RDFMemoryStore FromFile(RDFStoreEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (string.IsNullOrWhiteSpace(filepath))
            throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter is null or empty.");
        if (!File.Exists(filepath))
            throw new RDFStoreException("Cannot read RDF memory store from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
        #endregion

        RDFMemoryStore memStore = rdfFormat switch
        {
            RDFStoreEnums.RDFFormats.NQuads => RDFNQuads.Deserialize(filepath),
            RDFStoreEnums.RDFFormats.TriX => RDFTriX.Deserialize(filepath),
            RDFStoreEnums.RDFFormats.TriG => RDFTriG.Deserialize(filepath),
            _ => null
        };

        #region Datatype Discovery
        if (enableDatatypeDiscovery && memStore != null)
        {
            foreach (RDFGraph graph in memStore.ExtractGraphs())
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
        }
        #endregion

        return memStore;
    }

    /// <summary>
    /// Asynchronously reads a memory store from a file of the given RDF format.
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static Task<RDFMemoryStore> FromFileAsync(RDFStoreEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
        => Task.Run(() => FromFile(rdfFormat, filepath, enableDatatypeDiscovery));

    /// <summary>
    /// Reads a memory store from a stream of the given RDF format.
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static RDFMemoryStore FromStream(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (inputStream == null)
            throw new RDFStoreException("Cannot read RDF memory store from stream because given \"inputStream\" parameter is null.");
        #endregion

        RDFMemoryStore memStore = rdfFormat switch
        {
            RDFStoreEnums.RDFFormats.NQuads => RDFNQuads.Deserialize(inputStream),
            RDFStoreEnums.RDFFormats.TriX => RDFTriX.Deserialize(inputStream),
            RDFStoreEnums.RDFFormats.TriG => RDFTriG.Deserialize(inputStream),
            _ => null
        };

        #region Datatype Discovery
        if (enableDatatypeDiscovery && memStore != null)
        {
            foreach (RDFGraph graph in memStore.ExtractGraphs())
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
        }
        #endregion

        return memStore;
    }

    /// <summary>
    /// Asynchronously reads a memory store from a stream of the given RDF format.
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static Task<RDFMemoryStore> FromStreamAsync(RDFStoreEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
        => Task.Run(() => FromStream(rdfFormat, inputStream, enableDatatypeDiscovery));

    /// <summary>
    /// Reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static RDFMemoryStore FromDataTable(DataTable table, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (table == null)
            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter is null.");
        if (!(table.Columns.Contains("?SUBJECT") && table.Columns.Contains("?PREDICATE") && table.Columns.Contains("?OBJECT")))
            throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter does not have the required columns \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");
        #endregion

        RDFMemoryStore memStore = new RDFMemoryStore();
        RDFContext defaultContext = new RDFContext();
        bool hasContextColumn = table.Columns.Contains("?CONTEXT");

        #region Parse Table
        foreach (DataRow tableRow in table.Rows)
        {
            #region CONTEXT
            RDFPatternMember rowContext;
            if (hasContextColumn)
            {
                if (tableRow.IsNull("?CONTEXT") || string.IsNullOrEmpty(tableRow["?CONTEXT"].ToString()))
                {
                    rowContext = defaultContext;
                }
                else
                {
                    rowContext = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?CONTEXT"].ToString());
                    if (rowContext is not RDFResource resource)
                        throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?CONTEXT\" column.");
                    if (resource.IsBlank)
                        throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource in the \"?CONTEXT\" column.");
                }
            }
            else
            {
                rowContext = defaultContext;
            }
            #endregion

            #region SUBJECT
            if (tableRow.IsNull("?SUBJECT") || string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?SUBJECT\" column.");

            RDFPatternMember rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
            if (rowSubj is not RDFResource subj)
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
            #endregion

            #region PREDICATE
            if (tableRow.IsNull("?PREDICATE") || string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having null or empty value in the \"?PREDICATE\" column.");

            RDFPatternMember rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
            if (rowPred is not RDFResource pred)
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row not having a resource in the \"?PREDICATE\" column.");
            if (pred.IsBlank)
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having a blank resource in the \"?PREDICATE\" column.");
            #endregion

            #region OBJECT
            if (tableRow.IsNull("?OBJECT"))
                throw new RDFStoreException("Cannot read RDF memory store from datatable because given \"table\" parameter contains a row having NULL value in the \"?OBJECT\" column.");

            RDFPatternMember rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
            if (rowObj is RDFResource objRes)
                memStore.AddQuadruple(new RDFQuadruple(new RDFContext(rowContext.ToString()), subj, pred, objRes));
            else
                memStore.AddQuadruple(new RDFQuadruple(new RDFContext(rowContext.ToString()), subj, pred, (RDFLiteral)rowObj));
            #endregion
        }
        #endregion

        #region Datatype Discovery
        if (enableDatatypeDiscovery)
        {
            foreach (RDFGraph graph in memStore.ExtractGraphs())
                RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
        }
        #endregion

        return memStore;
    }

    /// <summary>
    /// Asynchronously reads a memory store from a datatable with "Context-Subject-Predicate-Object" columns.
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static Task<RDFMemoryStore> FromDataTableAsync(DataTable table, bool enableDatatypeDiscovery=false)
        => Task.Run(() => FromDataTable(table, enableDatatypeDiscovery));

    /// <summary>
    /// Reads a memory store by trying to dereference the given Uri
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static RDFMemoryStore FromUri(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
        => FromUriAsync(uri, timeoutMilliseconds, enableDatatypeDiscovery).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously reads a memory store by trying to dereference the given Uri
    /// </summary>
    /// <exception cref="RDFStoreException"></exception>
    public static async Task<RDFMemoryStore> FromUriAsync(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery = false)
    {
        #region Guards
        if (uri == null)
            throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter is null.");
        if (!uri.IsAbsoluteUri)
            throw new RDFStoreException("Cannot read RDF memory store from Uri because given \"uri\" parameter does not represent an absolute Uri.");
        #endregion

        RDFMemoryStore memStore = new RDFMemoryStore();
        try
        {
            //Grab eventual dereference Uri
            Uri remappedUri = RDFModelUtilities.RemapUriForDereference(uri);

            //Build an HTTP client to execute the request
            using (HttpClient httpClient = new HttpClient(
                new HttpClientHandler
                {
                   MaxAutomaticRedirections = 2,
                   AllowAutoRedirect = true
                }))
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/n-quads"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/trix"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/trig"));

                // Execute the request and ensure it is successful
                HttpResponseMessage response = await httpClient.GetAsync(remappedUri);
                response.EnsureSuccessStatusCode();

                // Detect ContentType from response
                string responseContentType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrWhiteSpace(responseContentType))
                {
                    if (response.Headers.TryGetValues("ContentType", out var contentTypeValues))
                        responseContentType = contentTypeValues.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(responseContentType))
                        responseContentType = "application/n-quads"; // Fallback to N-QUADS
                }

                // Read response data
                await using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    // N-QUADS
                    if (responseContentType.Contains("application/n-quads", StringComparison.Ordinal))
                        memStore = await FromStreamAsync(RDFStoreEnums.RDFFormats.NQuads, responseStream);

                    // TRIX
                    else if (responseContentType.Contains("application/trix", StringComparison.Ordinal))
                        memStore = await FromStreamAsync(RDFStoreEnums.RDFFormats.TriX, responseStream);

                    // TRIG
                    else if (responseContentType.Contains("application/trig", StringComparison.Ordinal))
                        memStore = await FromStreamAsync(RDFStoreEnums.RDFFormats.TriG, responseStream);

                    #region Datatype Discovery
                    if (enableDatatypeDiscovery)
                    {
                        foreach (RDFGraph graph in memStore.ExtractGraphs())
                            RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
                    }
                    #endregion
                }
            }
        }
        catch (Exception ex)
        {
            throw new RDFStoreException("Cannot read RDF memory store from Uri because: " + ex.Message);
        }

        return memStore;
    }
    #endregion

    #endregion

    #endregion
}