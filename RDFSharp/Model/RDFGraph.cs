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

using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RDFSharp.Model;

/// <summary>
/// RDFGraph represents an Uri-named collection of triples
/// </summary>
public sealed class RDFGraph : RDFDataSource, IEquatable<RDFGraph>, IEnumerable<RDFTriple>, IDisposable
{
    #region Properties
    /// <summary>
    /// Uri of the graph
    /// </summary>
    public Uri Context { get; internal set; }

    /// <summary>
    /// Count of the graph's triples
    /// </summary>
    public long TriplesCount
        => Index.Triples.Rows.Count;

    /// <summary>
    /// Gets the enumerator on the graph's triples for iteration
    /// </summary>
    public IEnumerator<RDFTriple> TriplesEnumerator
    {
        get
        {
            Dictionary<long, RDFResource> resources = (Dictionary<long, RDFResource>)Index.Triples.ExtendedProperties["RES"];
            Dictionary<long, RDFLiteral> literals = (Dictionary<long, RDFLiteral>)Index.Triples.ExtendedProperties["LIT"];
            foreach (DataRow triple in Index.Triples.Rows)
            {
                yield return triple.Field<RDFModelEnums.RDFTripleFlavors>("?TFV") == RDFModelEnums.RDFTripleFlavors.SPO
                    ? new RDFTriple(resources[triple.Field<long>("?SID")], resources[triple.Field<long>("?PID")], resources[triple.Field<long>("?OID")])
                    : new RDFTriple(resources[triple.Field<long>("?SID")], resources[triple.Field<long>("?PID")], literals[triple.Field<long>("?OID")]);
            }
        }
    }

    /// <summary>
    /// Index on the triples of the graph
    /// </summary>
    internal RDFGraphIndex Index { get; set; }

    /// <summary>
    /// Flag indicating that the graph has already been disposed
    /// </summary>
    internal bool Disposed { get; set; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds an empty graph
    /// </summary>
    public RDFGraph()
    {
        Context = RDFNamespaceRegister.DefaultNamespace.NamespaceUri;
        Index = new RDFGraphIndex();
    }

    /// <summary>
    /// Builds a graph with the given list of triples
    /// </summary>
    public RDFGraph(List<RDFTriple> triples) : this()
        => triples?.ForEach(t => AddTriple(t));

    /// <summary>
    /// Destroys the graph instance
    /// </summary>
    ~RDFGraph()
        => Dispose(false);
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the graph
    /// </summary>
    public override string ToString()
        => Context.ToString();

    /// <summary>
    /// Performs the equality comparison between two graphs
    /// </summary>
    public bool Equals(RDFGraph other)
    {
        if (other == null || TriplesCount != other.TriplesCount)
            return false;
        return this.All(other.ContainsTriple);
    }

    /// <summary>
    /// Exposes a typed enumerator on the graph's triples
    /// </summary>
    IEnumerator<RDFTriple> IEnumerable<RDFTriple>.GetEnumerator()
        => TriplesEnumerator;

    /// <summary>
    /// Exposes an untyped enumerator on the graph's triples
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
        => TriplesEnumerator;

    /// <summary>
    /// Disposes the graph (IDisposable)
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the graph
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing)
        {
            Index?.Dispose();
            Index = null;
        }

        Disposed = true;
    }
    #endregion

    #region Methods

    #region Add
    /// <summary>
    /// Sets the context of the graph to the given absolute Uri
    /// </summary>
    public RDFGraph SetContext(Uri contextUri)
    {
        if (contextUri?.IsAbsoluteUri == true
            && !contextUri.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase)
            && !contextUri.ToString().StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase))
        {
            Context = contextUri;
        }
        return this;
    }

    /// <summary>
    /// Adds the given triple to the graph, avoiding duplicate insertions
    /// </summary>
    public RDFGraph AddTriple(RDFTriple triple)
    {
        if (triple != null)
            Index.Add(triple);
        return this;
    }

    /// <summary>
    /// Adds the given container to the graph
    /// </summary>
    public RDFGraph AddContainer(RDFContainer container)
    {
        if (container != null)
        {
            //Reify the container to get its graph representation
            foreach (RDFTriple t in container.ReifyContainer())
                Index.Add(t);
        }
        return this;
    }

    /// <summary>
    /// Adds the given collection to the graph
    /// </summary>
    public RDFGraph AddCollection(RDFCollection collection)
    {
        if (collection != null)
        {
            //Reify the collection to get its graph representation
            foreach (RDFTriple t in collection.ReifyCollection())
                Index.Add(t);
        }
        return this;
    }

    /// <summary>
    /// Adds the given datatype to the graph
    /// </summary>
    public RDFGraph AddDatatype(RDFDatatype datatype)
    {
        if (datatype != null)
        {
            //Reify the datatype to get its graph representation
            foreach (RDFTriple t in datatype.ToRDFGraph())
                Index.Add(t);
        }
        return this;
    }
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given triple from the graph
    /// </summary>
    public RDFGraph RemoveTriple(RDFTriple triple)
    {
        if (triple != null)
            Index.Remove(triple);
        return this;
    }

    /// <summary>
    /// Removes the triples which satisfy the given combination of SPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params must be mutually exclusive!)
    /// </summary>
    public RDFGraph RemoveTriples(RDFResource s, RDFResource p, RDFResource o, RDFLiteral l)
    {
        foreach (RDFTriple triple in SelectTriples(s, p, o, l))
            Index.Remove(triple);
        return this;
    }

    /// <summary>
    /// Clears the triples and metadata of the graph
    /// </summary>
    public void ClearTriples()
    {
        Index.Triples.Clear();
        ((Dictionary<long, RDFResource>)Index.Triples.ExtendedProperties["RES"]).Clear();
        ((Dictionary<long, RDFLiteral>)Index.Triples.ExtendedProperties["LIT"]).Clear();
    }
    #endregion

    #region Select
    /// <summary>
    /// Checks if the graph contains the given triple
    /// </summary>
    public bool ContainsTriple(RDFTriple triple)
        => triple is not null && Index.Triples.Rows.Find(triple.TripleID) is not null;

    /// <summary>
    /// Selects the triples which satisfy the given combination of SPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params must be mutually exclusive!)
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public List<RDFTriple> SelectTriples(RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null)
    {
        #region Guards
        if (o != null && l != null)
            throw new RDFModelException("Cannot access a graph when both object and literals are given: they must be mutually exclusive!");
        #endregion

        Dictionary<long, RDFResource> resources = (Dictionary<long, RDFResource>)Index.Triples.ExtendedProperties["RES"];
        Dictionary<long, RDFLiteral> literals = (Dictionary<long, RDFLiteral>)Index.Triples.ExtendedProperties["LIT"];
        StringBuilder queryFilters = new StringBuilder(3);
        if (s != null) queryFilters.Append('S');
        if (p != null) queryFilters.Append('P');
        if (o != null) queryFilters.Append('O');
        if (l != null) queryFilters.Append('L');
        DataRow[] selectedTriples = queryFilters.ToString() switch
        {
            "S"   => Index.Triples.Select($"?SID == {s.PatternMemberID}"),
            "P"   => Index.Triples.Select($"?PID == {p.PatternMemberID}"),
            "O"   => Index.Triples.Select($"?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO}"),
            "L"   => Index.Triples.Select($"?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPL}"),
            "SP"  => Index.Triples.Select($"?SID == {s.PatternMemberID} AND ?PID == {p.PatternMemberID}"),
            "SO"  => Index.Triples.Select($"?SID == {s.PatternMemberID} AND ?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO}"),
            "SL"  => Index.Triples.Select($"?SID == {s.PatternMemberID} AND ?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPL}"),
            "PO"  => Index.Triples.Select($"?PID == {p.PatternMemberID} AND ?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO}"),
            "PL"  => Index.Triples.Select($"?PID == {p.PatternMemberID} AND ?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPL}"),
            "SPO" => Index.Triples.Select($"?SID == {s.PatternMemberID} AND ?PID == {p.PatternMemberID} AND ?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPO}"),
            "SPL" => Index.Triples.Select($"?SID == {s.PatternMemberID} AND ?PID == {p.PatternMemberID} AND ?OID == {o.PatternMemberID} AND ?TFV == {RDFModelEnums.RDFTripleFlavors.SPL}"),
            _     => [.. Index.Triples.Rows.Cast<DataRow>()]
        };

        //Decompress hashes
        List<RDFTriple> result = new List<RDFTriple>(selectedTriples.Length);
        foreach (DataRow selectedTriple in selectedTriples)
        {
            RDFResource subj = resources[selectedTriple.Field<long>("?SID")];
            RDFResource pred = resources[selectedTriple.Field<long>("?PID")];
            switch (selectedTriple.Field<RDFModelEnums.RDFTripleFlavors>("?TFV"))
            {
                case RDFModelEnums.RDFTripleFlavors.SPO:
                    RDFResource obj = resources[selectedTriple.Field<long>("?OID")];
                    result.Add(new RDFTriple(subj, pred, obj));
                    break;
                case RDFModelEnums.RDFTripleFlavors.SPL:
                    RDFLiteral lit = literals[selectedTriple.Field<long>("?OID")];
                    result.Add(new RDFTriple(subj, pred, lit));
                    break;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the subgraph containing the triples which satisfy the given combination of SPOL accessors<br/>
    /// (null values are handled as * selectors. Object and Literal params must be mutually exclusive!)
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public RDFGraph this[RDFResource s=null, RDFResource p=null, RDFResource o=null, RDFLiteral l=null]
        => new RDFGraph(SelectTriples(s, p, o, l));
    #endregion

    #region Set
    /// <summary>
    /// Builds an intersection graph from this graph and a given one
    /// </summary>
    public RDFGraph IntersectWith(RDFGraph graph)
    {
        RDFGraph result = new RDFGraph();
        if (graph != null)
        {
            //Add intersection triples
            foreach (RDFTriple t in this)
            {
                if (graph.Index.Triples.Rows.Find(t.TripleID) is not null)
                    result.Index.Add(t);
            }
        }
        return result;
    }

    /// <summary>
    /// Builds a union graph from this graph and a given one
    /// </summary>
    public RDFGraph UnionWith(RDFGraph graph)
    {
        RDFGraph result = new RDFGraph();

        //Add triples from this graph
        foreach (RDFTriple t in this)
            result.Index.Add(t);

        //Manage the given graph
        if (graph != null)
        {
            //Add triples from the given graph
            foreach (RDFTriple t in graph)
                result.Index.Add(t);
        }

        return result;
    }

    /// <summary>
    /// Builds a difference graph from this graph and a given one
    /// </summary>
    public RDFGraph DifferenceWith(RDFGraph graph)
    {
        RDFGraph result = new RDFGraph();

        if (graph != null)
        {
            //Add difference triples
            foreach (RDFTriple t in this)
            {
                if (graph.Index.Triples.Rows.Find(t.TripleID) is null)
                    result.Index.Add(t);
            }
        }
        else
        {
            //Add triples from this graph
            foreach (RDFTriple t in this)
                result.Index.Add(t);
        }

        return result;
    }
    #endregion

    #region Convert

    #region Export
    /// <summary>
    /// Writes the graph into a file in the given RDF format.
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public void ToFile(RDFModelEnums.RDFFormats rdfFormat, string filepath)
    {
        #region Guards
        if (string.IsNullOrEmpty(filepath))
            throw new RDFModelException("Cannot write RDF graph to file because given \"filepath\" parameter is null or empty.");
        #endregion

        switch (rdfFormat)
        {
            case RDFModelEnums.RDFFormats.NTriples:
                RDFNTriples.Serialize(this, filepath);
                break;
            case RDFModelEnums.RDFFormats.RdfXml:
                RDFXml.Serialize(this, filepath);
                break;
            case RDFModelEnums.RDFFormats.TriX:
                RDFTriX.Serialize(this, filepath);
                break;
            case RDFModelEnums.RDFFormats.Turtle:
                RDFTurtle.Serialize(this, filepath);
                break;
        }
    }

    /// <summary>
    /// Asynchronously writes the graph into a file in the given RDF format
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public Task ToFileAsync(RDFModelEnums.RDFFormats rdfFormat, string filepath)
        => Task.Run(() => ToFile(rdfFormat, filepath));

    /// <summary>
    /// Writes the graph into a stream in the given RDF format (at the end the stream is closed)
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public void ToStream(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream)
    {
        #region Guards
        if (outputStream == null)
            throw new RDFModelException("Cannot write RDF graph to stream because given \"outputStream\" parameter is null.");
        #endregion

        switch (rdfFormat)
        {
            case RDFModelEnums.RDFFormats.NTriples:
                RDFNTriples.Serialize(this, outputStream);
                break;
            case RDFModelEnums.RDFFormats.RdfXml:
                RDFXml.Serialize(this, outputStream);
                break;
            case RDFModelEnums.RDFFormats.TriX:
                RDFTriX.Serialize(this, outputStream);
                break;
            case RDFModelEnums.RDFFormats.Turtle:
                RDFTurtle.Serialize(this, outputStream);
                break;
        }
    }

    /// <summary>
    /// Asynchronously writes the graph into a stream in the given RDF format (at the end the stream is closed)
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public Task ToStreamAsync(RDFModelEnums.RDFFormats rdfFormat, Stream outputStream)
        => Task.Run(() => ToStream(rdfFormat, outputStream));

    /// <summary>
    /// Writes the graph into a datatable with "Subject-Predicate-Object" columns
    /// </summary>
    public DataTable ToDataTable()
    {
        //Create the structure of the result datatable
        DataTable result = new DataTable(ToString());
        result.Columns.Add("?SUBJECT", typeof(string));
        result.Columns.Add("?PREDICATE", typeof(string));
        result.Columns.Add("?OBJECT", typeof(string));

        //Iterate the triples of the graph to populate the result datatable
        result.BeginLoadData();
        foreach (RDFTriple t in this)
        {
            DataRow newRow = result.NewRow();
            newRow["?SUBJECT"] = t.Subject.ToString();
            newRow["?PREDICATE"] = t.Predicate.ToString();
            newRow["?OBJECT"] = t.Object.ToString();
            result.Rows.Add(newRow);
        }
        result.EndLoadData();

        return result;
    }

    /// <summary>
    /// Asynchronously writes the graph into a datatable with "Subject-Predicate-Object" columns
    /// </summary>
    public Task<DataTable> ToDataTableAsync()
        => Task.Run(ToDataTable);
    #endregion

    #region Import
    /// <summary>
    /// Reads a graph from a file of the given RDF format.
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static RDFGraph FromFile(RDFModelEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (string.IsNullOrEmpty(filepath))
            throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter is null or empty.");
        if (!File.Exists(filepath))
            throw new RDFModelException("Cannot read RDF graph from file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
        #endregion

        RDFGraph graph = rdfFormat switch
        {
            RDFModelEnums.RDFFormats.RdfXml => RDFXml.Deserialize(filepath),
            RDFModelEnums.RDFFormats.Turtle => RDFTurtle.Deserialize(filepath),
            RDFModelEnums.RDFFormats.NTriples => RDFNTriples.Deserialize(filepath),
            RDFModelEnums.RDFFormats.TriX => RDFTriX.Deserialize(filepath),
            _ => null
        };

        #region Datatype Discovery
        if (enableDatatypeDiscovery && graph != null)
            RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
        #endregion

        return graph;
    }

    /// <summary>
    /// Asynchronously reads a graph from a file of the given RDF format
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static Task<RDFGraph> FromFileAsync(RDFModelEnums.RDFFormats rdfFormat, string filepath, bool enableDatatypeDiscovery = false)
        => Task.Run(() => FromFile(rdfFormat, filepath, enableDatatypeDiscovery));

    /// <summary>
    /// Reads a graph from a stream of the given RDF format.
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery=false)
        => FromStream(rdfFormat, inputStream, null, enableDatatypeDiscovery);
    internal static RDFGraph FromStream(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, Uri graphContext, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (inputStream == null)
        {
            throw new RDFModelException("Cannot read RDF graph from stream because given \"inputStream\" parameter is null.");
        }
        #endregion

        RDFGraph graph = rdfFormat switch
        {
            RDFModelEnums.RDFFormats.RdfXml => RDFXml.Deserialize(inputStream, graphContext),
            RDFModelEnums.RDFFormats.Turtle => RDFTurtle.Deserialize(inputStream, graphContext),
            RDFModelEnums.RDFFormats.NTriples => RDFNTriples.Deserialize(inputStream, graphContext),
            RDFModelEnums.RDFFormats.TriX => RDFTriX.Deserialize(inputStream, graphContext),
            _ => null
        };

        #region Datatype Discovery
        if (enableDatatypeDiscovery && graph != null)
            RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
        #endregion

        return graph;
    }

    /// <summary>
    /// Asynchronously reads a graph from a stream of the given RDF format
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static Task<RDFGraph> FromStreamAsync(RDFModelEnums.RDFFormats rdfFormat, Stream inputStream, bool enableDatatypeDiscovery = false)
        => Task.Run(() => FromStream(rdfFormat, inputStream, enableDatatypeDiscovery));

    /// <summary>
    /// Reads a graph from a datatable with "Subject-Predicate-Object" columns.
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static RDFGraph FromDataTable(DataTable table, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (table == null)
            throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter is null.");
        if (!(table.Columns.Contains("?SUBJECT") && table.Columns.Contains("?PREDICATE") && table.Columns.Contains("?OBJECT")))
            throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter does not have the required columns \"?SUBJECT\", \"?PREDICATE\", \"?OBJECT\".");
        #endregion

        RDFGraph graph = new RDFGraph();

        #region Parse Table

        #region CONTEXT
        //Parse the name of the datatable for Uri, in order to assign the graph name
        if (Uri.TryCreate(table.TableName, UriKind.Absolute, out Uri graphUri))
            graph.SetContext(graphUri);
        #endregion

        #region SUBJECT-PREDICATE-OBJECT
        foreach (DataRow tableRow in table.Rows)
        {
            #region SUBJECT
            if (tableRow.IsNull("?SUBJECT") || string.IsNullOrEmpty(tableRow["?SUBJECT"].ToString()))
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"?SUBJECT\" column.");

            RDFPatternMember rowSubj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?SUBJECT"].ToString());
            if (rowSubj is not RDFResource subj)
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"?SUBJECT\" column.");
            #endregion

            #region PREDICATE
            if (tableRow.IsNull("?PREDICATE") || string.IsNullOrEmpty(tableRow["?PREDICATE"].ToString()))
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null or empty value in the \"?PREDICATE\" column.");

            RDFPatternMember rowPred = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?PREDICATE"].ToString());
            if (rowPred is not RDFResource pred)
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row not having a resource in the \"?PREDICATE\" column.");

            if (pred.IsBlank)
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having a blank resource in the \"?PREDICATE\" column.");
            #endregion

            #region OBJECT
            if (tableRow.IsNull("?OBJECT"))
                throw new RDFModelException("Cannot read RDF graph from datatable because given \"table\" parameter contains a row having null value in the \"?OBJECT\" column.");

            RDFPatternMember rowObj = RDFQueryUtilities.ParseRDFPatternMember(tableRow["?OBJECT"].ToString());
            if (rowObj is RDFResource rowObjRes)
                graph.AddTriple(new RDFTriple(subj, pred, rowObjRes));
            else
                graph.AddTriple(new RDFTriple(subj, pred, (RDFLiteral)rowObj));
            #endregion
        }
        #endregion

        #endregion

        #region Datatype Discovery
        if (enableDatatypeDiscovery)
            RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
        #endregion

        return graph;
    }

    /// <summary>
    /// Asynchronously reads a graph from a datatable with "Subject-Predicate-Object" columns
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static Task<RDFGraph> FromDataTableAsync(DataTable table, bool enableDatatypeDiscovery=false)
        => Task.Run(() => FromDataTable(table, enableDatatypeDiscovery));

    /// <summary>
    /// Reads a graph by trying to dereference the given Uri
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static RDFGraph FromUri(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
        => FromUriAsync(uri, timeoutMilliseconds, enableDatatypeDiscovery).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously reads a graph by trying to dereference the given Uri
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public static async Task<RDFGraph> FromUriAsync(Uri uri, int timeoutMilliseconds=20000, bool enableDatatypeDiscovery=false)
    {
        #region Guards
        if (uri == null)
            throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter is null.");
        if (!uri.IsAbsoluteUri)
            throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter does not represent an absolute Uri.");
        #endregion

        RDFGraph graph = new RDFGraph();
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
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/rdf+xml"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/turtle"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/turtle"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-turtle"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/n-triples"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/trix"));

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
                        responseContentType = "application/rdf+xml"; //Fallback to RDF/XML
                }

                // Read response data
                await using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    //RDF/XML
                    if (responseContentType.Contains("application/rdf+xml"))
                        graph = FromStream(RDFModelEnums.RDFFormats.RdfXml, responseStream, remappedUri);

                    //TURTLE
                    else if (responseContentType.Contains("text/turtle")
                              || responseContentType.Contains("application/turtle")
                              || responseContentType.Contains("application/x-turtle"))
                        graph = FromStream(RDFModelEnums.RDFFormats.Turtle, responseStream, remappedUri);

                    //N-TRIPLES
                    else if (responseContentType.Contains("application/n-triples"))
                        graph = FromStream(RDFModelEnums.RDFFormats.NTriples, responseStream, remappedUri);

                    //TRIX
                    else if (responseContentType.Contains("application/trix"))
                        graph = FromStream(RDFModelEnums.RDFFormats.TriX, responseStream, remappedUri);

                    #region Datatype Discovery
                    if (enableDatatypeDiscovery)
                        RDFModelUtilities.ExtractAndRegisterDatatypes(graph);
                    #endregion
                }
            }
        }
        catch (Exception ex)
        {
            throw new RDFModelException($"Cannot read RDF graph from Uri {uri} because: " + ex.Message);
        }

        return graph;
    }
    #endregion

    #endregion

    #endregion
}