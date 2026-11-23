---
title: SPARQL endpoints
permalink: /query/endpoints/
tagline: Asking a remote service as if it were local
lede: A SPARQL endpoint is a service on the Web that answers queries over HTTP. RDFSharp talks to it with the same query objects you use on graphs and stores and the same result types come back.
---

> The cooperative wants to know more about its grapes and DBpedia knows a great deal about them. Instead of downloading anything it asks the service directly: which grapes are grown in Tuscany and where do they come from?
{: .story}

The examples that name DBpedia need a network connection: their results come from the live service, so they follow what DBpedia says on the day you run the code. The examples that need a server of your own (errors, authorization and updates) run against a test listener on the same machine and their outputs are what that listener received or answered. The page needs these namespaces of the library:

```csharp
using System.Data;
using RDFSharp.Model;   // resources, graphs
using RDFSharp.Query;   // endpoints, queries, options
```

## Meeting DBpedia

An endpoint is an `RDFSPARQLEndpoint` and its only required piece of information is the address. It is a data source like a graph or a store, so it works with the same queries. DBpedia has its own vocabulary: `dbo:` names its classes and properties and `dbr:` its resources.

```csharp
RDFNamespace dbr = new RDFNamespace("dbr", "http://dbpedia.org/resource/");
RDFNamespace dbo = new RDFNamespace("dbo", "http://dbpedia.org/ontology/");
RDFSPARQLEndpoint dbpedia = new RDFSPARQLEndpoint(new Uri("https://dbpedia.org/sparql"));
```

The first question is a `SELECT`: which grapes does DBpedia place in Tuscany? A query goes to an endpoint with `ApplyToSPARQLEndpointAsync` and the result is the same `RDFSelectQueryResult` you know from local data.

<div class="pair" markdown="1">

```sparql
PREFIX dbo: <http://dbpedia.org/ontology/>
PREFIX dbr: <http://dbpedia.org/resource/>

SELECT DISTINCT ?grape
WHERE {
  ?grape a dbo:Grape ;
         dbo:wineRegion dbr:Tuscany .
}
ORDER BY ?grape
```

```csharp
RDFVariable grape = new RDFVariable("grape");
RDFResource grapeClass = new RDFResource(dbo + "Grape");
RDFResource wineRegion = new RDFResource(dbo + "wineRegion");
RDFResource tuscany = new RDFResource(dbr + "Tuscany");

RDFSelectQuery tuscanGrapes = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(grape, RDFVocabulary.RDF.TYPE, grapeClass))
        .AddPattern(new RDFPattern(grape, wineRegion, tuscany)))
    .AddProjectionVariable(grape)
    .AddModifier(new RDFDistinctModifier())
    .AddModifier(new RDFOrderByModifier(grape, RDFQueryEnums.RDFOrderByFlavors.ASC));

RDFSelectQueryResult tuscanGrapesResult = await tuscanGrapes.ApplyToSPARQLEndpointAsync(dbpedia);
foreach (DataRow row in tuscanGrapesResult.SelectResults.Rows)
    Console.WriteLine(row["?GRAPE"]);
```

</div>

<div class="result" markdown="1">

| ?GRAPE |
|---|
| `http://dbpedia.org/resource/Aleatico` |
| `http://dbpedia.org/resource/Sangiovese` |

</div>

The column is `?GRAPE`: the engine writes variable names in upper case and the library gives the columns of a remote answer the same form, with the question mark.

## Asking yes or no

An `RDFAskQuery` asks whether a pattern has any solution and the endpoint answers with a single boolean, which you read from `AskResult`. Does DBpedia know a Tuscan grape of Italian origin? Does it know one of Greek origin?

```csharp
RDFResource origin = new RDFResource(dbo + "origin");

RDFAskQuery italianOrigin = new RDFAskQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(grape, RDFVocabulary.RDF.TYPE, grapeClass))
        .AddPattern(new RDFPattern(grape, wineRegion, tuscany))
        .AddPattern(new RDFPattern(grape, origin, new RDFResource(dbr + "Italy"))));
RDFAskQuery greekOrigin = new RDFAskQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(grape, RDFVocabulary.RDF.TYPE, grapeClass))
        .AddPattern(new RDFPattern(grape, wineRegion, tuscany))
        .AddPattern(new RDFPattern(grape, origin, new RDFResource(dbr + "Greece"))));

Console.WriteLine((await italianOrigin.ApplyToSPARQLEndpointAsync(dbpedia)).AskResult);
Console.WriteLine((await greekOrigin.ApplyToSPARQLEndpointAsync(dbpedia)).AskResult);
```

<div class="result" markdown="1">

```
True
False
```

</div>

The Sangiovese makes the first answer true. The second is false: none of the Tuscan grapes of DBpedia comes from Greece.

## Default and named graphs

A SPARQL service may hold several graphs and the standard lets a client choose which ones a query sees. `AddDefaultGraphUri` and `AddNamedGraphUri` add these choices to the endpoint, which sends them with every request. Both return the endpoint, so they chain.

```csharp
RDFSPARQLEndpoint dbpediaGraph = new RDFSPARQLEndpoint(new Uri("https://dbpedia.org/sparql"))
    .AddDefaultGraphUri("http://dbpedia.org");
Console.WriteLine((await tuscanGrapes.ApplyToSPARQLEndpointAsync(dbpediaGraph)).SelectResultsCount);
```

<div class="result" markdown="1">

```
2
```

</div>

## POST and raw queries

Every `ApplyToSPARQLEndpointAsync` also has a synchronous form, `ApplyToSPARQLEndpoint`, which keeps the calling thread waiting for the network. By default the query goes out as a `GET` with the text in the address. Long queries can overflow the length that servers accept for an address: the query option `QueryMethod` moves them into the body of a `POST`.

```csharp
RDFSelectQueryResult asyncResult = await tuscanGrapes.ApplyToSPARQLEndpointAsync(dbpedia);
Console.WriteLine(asyncResult.SelectResultsCount);

RDFSPARQLEndpointQueryOptions postOptions = new RDFSPARQLEndpointQueryOptions(
    20000,
    RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException,
    RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post);
RDFSelectQueryResult postResult = await tuscanGrapes.ApplyToSPARQLEndpointAsync(dbpedia, postOptions);
Console.WriteLine(postResult.SelectResultsCount);
```

<div class="result" markdown="1">

```
2
2
```

</div>

Sometimes the query is already text: written by a person or copied from a tool. The static `ApplyRawToSPARQLEndpointAsync` of each query type sends it as it is. The text uses a lower-case `?origin` and the column still comes back in the library's form:

```csharp
RDFSelectQueryResult rawResult = await RDFSelectQuery.ApplyRawToSPARQLEndpointAsync(
    "SELECT ?origin WHERE { <http://dbpedia.org/resource/Sangiovese> <http://dbpedia.org/ontology/origin> ?origin }",
    dbpedia);
foreach (DataColumn column in rawResult.SelectResults.Columns)
    Console.WriteLine(column.ColumnName);
Console.WriteLine(rawResult.SelectResults.Rows[0]["?ORIGIN"]);
```

<div class="result" markdown="1">

```
?ORIGIN
http://dbpedia.org/resource/Italy
```

</div>

Whatever case you write in the query and whatever the endpoint answers, you read the rows with the upper-case name and the question mark.

## Query options

The `RDFSPARQLEndpointQueryOptions` object tunes one call. It has three properties and all of them are optional.

| Property | Default | What it decides |
|---|---|---|
| `TimeoutMilliseconds` | `-1` | how long to wait for the answer, where `-1` means without limit |
| `ErrorBehavior` | `ThrowException` | what happens when the call fails: an `RDFQueryException` or an empty result (`GiveEmptyResult`) |
| `QueryMethod` | `Get` | the HTTP method: `Get` or `Post` |

The two behaviors on error deserve a look. The address below is a port where nothing listens, which is how an endpoint that is down looks from the outside. The first call keeps the default and the second asks for an empty result.

```csharp
RDFSPARQLEndpoint offline = new RDFSPARQLEndpoint(new Uri("http://localhost:3031/sparql"));
try
{
    await tuscanGrapes.ApplyToSPARQLEndpointAsync(offline);
}
catch (RDFQueryException exception)
{
    Console.WriteLine(exception.Message);
}

RDFSPARQLEndpointQueryOptions quietOptions = new RDFSPARQLEndpointQueryOptions(
    2000, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult);
RDFSelectQueryResult quietResult = await tuscanGrapes.ApplyToSPARQLEndpointAsync(offline, quietOptions);
Console.WriteLine($"{quietResult.SelectResultsCount} rows, {quietResult.SelectResults.Columns.Count} columns");
```

<div class="result" markdown="1">

```
SELECT query on SPARQL endpoint failed because: Connection refused [::ffff:127.0.0.1]:3031 (localhost:3031)
0 rows, 0 columns
```

</div>

The text after "because" comes from the system and changes with it. The empty result has no rows and no columns: it cannot be told apart from an endpoint that answers with nothing, so use `GiveEmptyResult` where a missing answer is acceptable and keep the default where it is not. A timeout fails like any other error. A server on the same machine that takes three seconds to answer, called with a timeout of half a second, ends like this:

```csharp
RDFSPARQLEndpoint slow = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/slow/sparql"));
try
{
    await tuscanGrapes.ApplyToSPARQLEndpointAsync(slow, new RDFSPARQLEndpointQueryOptions(500));
}
catch (RDFQueryException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
SELECT query on SPARQL endpoint failed because: The operation has timed out.
```

</div>

## Authorization

Some endpoints are not open. `SetBasicAuthorizationHeader` takes the already encoded value of a Basic scheme and `SetBearerAuthorizationHeader` takes a token. The endpoint remembers the header and sends it with every request, for queries as well as for updates. A partner shop that protects its own endpoint with a token would be reached like this:

```csharp
RDFSPARQLEndpoint shop = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/vallerosa/sparql"))
    .SetBearerAuthorizationHeader("eyJhbGciOiJIUzI1NiJ9");
RDFSPARQLEndpoint shopBasic = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/vallerosa/sparql"))
    .SetBasicAuthorizationHeader("ZGVtbzpkZW1v");
await RDFSelectQuery.ApplyRawToSPARQLEndpointAsync("SELECT ?wine WHERE { ?wine a <https://vallerosa.example/vocab#RedWine> }", shop);
await RDFSelectQuery.ApplyRawToSPARQLEndpointAsync("SELECT ?wine WHERE { ?wine a <https://vallerosa.example/vocab#RedWine> }", shopBasic);
```

<div class="result" markdown="1" data-label="What the test listener received">

```
GET /vallerosa/sparql
Authorization: Bearer eyJhbGciOiJIUzI1NiJ9
GET /vallerosa/sparql
Authorization: Basic ZGVtbzpkZW1v
```

</div>

## Updates on an endpoint

The same endpoint class receives SPARQL Update, as long as the service exposes an update address: the cooperative runs a Fuseki server and archives there the 2025 editions of the guides, with the contexts of [operation sets]({{ '/query/operation-sets/' | relative_url }}). An operation is sent with `ApplyToSPARQLUpdateEndpointAsync` and the answer is a boolean: `true` when the endpoint accepted it. Nothing here touches a real service: a test listener on the same machine stands in for the server and records what arrives.

```csharp
RDFSPARQLEndpoint fuseki = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/vallerosa/update"));
RDFCopyOperation archiveCalici = new RDFCopyOperation()
    .SetFromContext(new Uri("https://guidacalici.example/2025"))
    .SetToContext(new Uri("https://vallerosa.example/archive/guidacalici-2025"));
bool archived = await archiveCalici.ApplyToSPARQLUpdateEndpointAsync(fuseki);
Console.WriteLine(archived);
```

<div class="result" markdown="1" data-label="What the test listener received">

```
POST /vallerosa/update
Content-Type: application/sparql-update
COPY <https://guidacalici.example/2025> TO <https://vallerosa.example/archive/guidacalici-2025>
```

</div>

An update always travels as a `POST`. The options of an update are `RDFSPARQLEndpointOperationOptions` and they have a timeout and a content type. The default, `Sparql_Update`, posts the text of the operation as it is. Some servers want the other form that the standard allows, a web form with the operation in a field called `update`: that is `X_WWW_FormUrlencoded`.

```csharp
RDFSPARQLEndpointOperationOptions formOptions = new RDFSPARQLEndpointOperationOptions(
    10000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded);
Console.WriteLine(await archiveCalici.ApplyToSPARQLUpdateEndpointAsync(fuseki, formOptions));
```

<div class="result" markdown="1" data-label="What the test listener received">

```
POST /vallerosa/update
Content-Type: application/x-www-form-urlencoded
update=COPY+%3chttps%3a%2f%2fguidacalici.example%2f2025%3e+TO+%3chttps%3a%2f%2fvallerosa.example%2farchive%2fguidacalici-2025%3e
```

</div>

An `RDFOperationSet` is sent as one request, with its operations separated by a semicolon, so the server decides how the chain is applied.

```csharp
RDFCopyOperation archiveWinescope = new RDFCopyOperation()
    .SetFromContext(new Uri("https://winescope.example/2025"))
    .SetToContext(new Uri("https://vallerosa.example/archive/winescope-2025"));
RDFOperationSet archive = new RDFOperationSet()
    .AddOperation(archiveCalici)
    .AddOperation(archiveWinescope);
Console.WriteLine(await archive.ApplyToSPARQLUpdateEndpointAsync(fuseki));
```

<div class="result" markdown="1" data-label="What the test listener received">

```
POST /vallerosa/update
Content-Type: application/sparql-update
COPY <https://guidacalici.example/2025> TO <https://vallerosa.example/archive/guidacalici-2025>
;
COPY <https://winescope.example/2025> TO <https://vallerosa.example/archive/winescope-2025>
```

</div>

The endpoint's default and named graphs and its authorization header travel with the update too. With `AddDefaultGraphUri` the request carries a `using-graph-uri` parameter in the address, or in the body for the form content type. The synchronous form is `ApplyToSPARQLUpdateEndpoint`.

```csharp
RDFSPARQLEndpoint secured = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/vallerosa/update"))
    .AddDefaultGraphUri("https://vallerosa.example/catalog")
    .SetBearerAuthorizationHeader("eyJhbGciOiJIUzI1NiJ9");
Console.WriteLine(await archiveCalici.ApplyToSPARQLUpdateEndpointAsync(secured));
```

<div class="result" markdown="1" data-label="What the test listener received">

```
POST /vallerosa/update?using-graph-uri=https://vallerosa.example/catalog
Content-Type: application/sparql-update
Authorization: Bearer eyJhbGciOiJIUzI1NiJ9
COPY <https://guidacalici.example/2025> TO <https://vallerosa.example/archive/guidacalici-2025>
```

</div>

When the server refuses an operation, the HTTP error becomes an `RDFQueryException`. The operations that have a `SILENT` form (load, clear, create, drop, add, copy and move) turn that failure into a `false`. Here the listener answers every request on this address with an HTTP 500:

```csharp
RDFSPARQLEndpoint broken = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/broken/update"));
RDFCopyOperation silentArchive = new RDFCopyOperation()
    .SetFromContext(new Uri("https://guidacalici.example/2025"))
    .SetToContext(new Uri("https://vallerosa.example/archive/guidacalici-2025"))
    .Silent();
Console.WriteLine(await silentArchive.ApplyToSPARQLUpdateEndpointAsync(broken));
try
{
    await archiveCalici.ApplyToSPARQLUpdateEndpointAsync(broken);
}
catch (RDFQueryException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
False
Operation on SPARQL UPDATE endpoint http://localhost:3030/broken/update failed because: The remote server returned an error: (500) Internal Server Error.; Endpoint's response was: 
```

</div>

The insert and delete operations have no `SILENT` form and always raise. An operation set raises too: its operations travel in one request, so the library cannot give each of them its own handling of errors.

## Which formats the endpoint speaks

Endpoints answer in the standard **SPARQL Query Results XML Format**. RDFSharp supports nothing else for `SELECT` and `ASK`. Both queries go out with the header `Accept: application/sparql-results+xml` and the body of the answer is read with `FromSparqlXmlResult`, the same reader you can call on a file. JSON and CSV answers are not supported: an endpoint that replies with one of them breaks the read. The listener below does exactly that, with a JSON body:

```csharp
RDFSPARQLEndpoint jsonOnly = new RDFSPARQLEndpoint(new Uri("http://localhost:3030/json/sparql"));
try
{
    await RDFSelectQuery.ApplyRawToSPARQLEndpointAsync("SELECT ?wine WHERE { ?wine a <https://vallerosa.example/vocab#RedWine> }", jsonOnly);
}
catch (RDFQueryException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
Cannot read given "SPARQL Query Results XML Format" source because: Data at the root level is invalid. Line 1, position 1.
```

</div>

`CONSTRUCT` and `DESCRIBE` do not return rows: they ask for `application/turtle` or `text/turtle` and read the answer as an RDF graph, which you obtain with `ToRDFGraphAsync`.

```csharp
RDFConstructQuery sangioveseOrigin = new RDFConstructQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource(dbr + "Sangiovese"), origin, new RDFVariable("country"))))
    .AddTemplate(new RDFPattern(new RDFResource(dbr + "Sangiovese"), origin, new RDFVariable("country")));
RDFGraph origins = await (await sangioveseOrigin.ApplyToSPARQLEndpointAsync(dbpedia)).ToRDFGraphAsync();
Console.WriteLine(origins.TriplesCount);
foreach (RDFTriple triple in origins)
    Console.WriteLine(triple);
```

<div class="result" markdown="1">

```
1
http://dbpedia.org/resource/Sangiovese http://dbpedia.org/ontology/origin http://dbpedia.org/resource/Italy
```

</div>

The query forms and their results, including the XML format that the library reads and writes, are the subject of [Query forms]({{ '/query/query-forms/' | relative_url }}). To combine an endpoint with local data in one query, read [Federations]({{ '/query/federations/' | relative_url }}).
