---
title: Persistent stores
permalink: /store/extensions/
tagline: The same store on a database that outlives the process
lede: A memory store forgets everything when the program ends. RDFSharp.Extensions brings nine more implementations of the same abstraction, each one backed by a database, so the code you already wrote keeps working.
---

> The cooperative's store lives in memory, so every restart of the program means loading the catalog and the guides again. The time has come to put it on a database. The cooperative picks SQLite, a single file that needs no server.
{: .story}

## One idea: every provider is a store

`RDFMemoryStore` is one way to be an `RDFStore`. The others live in a separate package family, [RDFSharp.Extensions](https://github.com/mdesalvo/RDFSharp.Extensions), one NuGet package for each database. Each package holds one class that inherits from `RDFStore` and implements the same members: `AddQuadruple`, `MergeGraph`, `RemoveQuadruples`, `SelectQuadruples`, `ContainsQuadruple`, `QuadruplesCount` and their `Async` twins.

This is the whole point of the design. Everything that takes an `RDFStore` takes a persistent one: the searches and the conversions of [Memory stores]({{ '/store/stores/' | relative_url }}), the file formats of [Store formats]({{ '/store/formats/' | relative_url }}) and every query of the [Query]({{ '/query/' | relative_url }}) chapters. What changes is where the quadruples live and how long they stay.

| Provider | Package | Driver | Constructor |
|---|---|---|---|
| Azure Table | `RDFSharp.Extensions.AzureTable` | Azure.Data.Tables | `RDFAzureTableStore(string connectionString)` |
| Firebird | `RDFSharp.Extensions.Firebird` | FirebirdSql.Data.FirebirdClient | `RDFFirebirdStore(string connectionString, RDFFirebirdStoreOptions options = null)` |
| MongoDB | `RDFSharp.Extensions.MongoDB` | MongoDB.Driver | `RDFMongoDBStore(string connectionString, string databaseName = "rdfsharp")` |
| MySQL | `RDFSharp.Extensions.MySQL` | MySql.Data | `RDFMySQLStore(string connectionString, RDFMySQLStoreOptions options = null)` |
| Neo4j | `RDFSharp.Extensions.Neo4j` | Neo4j.Driver | `RDFNeo4jStore(string uri, string username, string password, string databaseName = "neo4j")` |
| Oracle | `RDFSharp.Extensions.Oracle` | Oracle.ManagedDataAccess.Core | `RDFOracleStore(string connectionString, RDFOracleStoreOptions options = null)` |
| PostgreSQL | `RDFSharp.Extensions.PostgreSQL` | Npgsql | `RDFPostgreSQLStore(string connectionString, RDFPostgreSQLStoreOptions options = null)` |
| SQLite | `RDFSharp.Extensions.SQLite` | System.Data.SQLite | `RDFSQLiteStore(string connectionString, RDFSQLiteStoreOptions options = null)` |
| SQL Server | `RDFSharp.Extensions.SQLServer` | Microsoft.Data.SqlClient | `RDFSQLServerStore(string connectionString, RDFSQLServerStoreOptions options = null)` |

Every class lives in the namespace of its package, for example `RDFSharp.Extensions.SQLite`. The Azure Table store also has a parameterless constructor that points to the local emulator.

## Install the provider

A package brings the provider's driver along. Its version follows the one of RDFSharp, so install the one that matches the library you use.

```shell
dotnet add package RDFSharp.Extensions.SQLite
```

## Move the store to SQLite

The cooperative starts from what the previous pages built: the catalog in its own context and the two guides, together in a memory store.

```csharp
using RDFSharp.Extensions.SQLite;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

RDFGraph catalog = (await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl"))
    .SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore guides = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");

RDFMemoryStore memory = new RDFMemoryStore();
await memory.MergeGraphAsync(catalog);
memory = await memory.UnionWithAsync(guides);
Console.WriteLine(memory.QuadruplesCount);
```

<div class="result" markdown="1">

```
153
```

</div>

A SQLite store is built from a connection string. If the file does not exist the store creates it, with the table it needs, so the first run of a program and the hundredth need no different code. Moving the data is a loop over the graphs of the memory store, one `MergeGraphAsync` each.

```csharp
RDFSQLiteStore sqlite = new RDFSQLiteStore("Data Source=vallerosa.db");
Console.WriteLine(sqlite.QuadruplesCount);

foreach (RDFGraph graph in await memory.ExtractGraphsAsync())
    await sqlite.MergeGraphAsync(graph);
Console.WriteLine(sqlite.QuadruplesCount);
```

<div class="result" markdown="1">

```
0
153
```

</div>

The store was empty and holds 153 quadruples after the move, the same number as the memory store. `MergeGraphAsync` is the right tool here and not a loop of `AddQuadruple`: the SQLite store inserts the whole graph inside one transaction, instead of opening one for every quadruple.

## The same code, a different store

From here on `sqlite` answers to everything the memory store answered to. The searches of the previous page, for instance, that ask what WineScope said about the Vallerosa Red:

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFResource hasScore = new RDFResource(vr + "score");

RDFResource rosso2021 = new RDFResource(vrd + "rosso-2021");
RDFContext winescopeContext = new RDFContext("https://winescope.example/2025");

foreach (RDFQuadruple quadruple in await sqlite.SelectQuadruplesAsync(c: winescopeContext, s: rosso2021, p: hasScore))
    Console.WriteLine(quadruple);

foreach (RDFGraph graph in await sqlite.ExtractGraphsAsync())
    Console.WriteLine($"{graph.Context} {graph.TriplesCount}");
```

<div class="result" markdown="1">

```
https://winescope.example/2025 https://vallerosa.example/id/rosso-2021 https://vallerosa.example/vocab#score 90^^http://www.w3.org/2001/XMLSchema#integer
https://vallerosa.example/catalog 143
https://winescope.example/2025 5
https://guidacalici.example/2025 5
```

</div>

The same holds for SPARQL, since a query applies to any store. This is the question of the story, narrowed to one wine: which guide gave which score to the Vallerosa Red? The context of a quadruple is what `GRAPH` binds.

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?guide ?value
WHERE {
  GRAPH ?guide { vrd:rosso-2021 vr:score ?value }
}
```

```csharp
RDFVariable guide = new RDFVariable("guide");
RDFVariable value = new RDFVariable("value");

RDFSelectQuery query = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(guide, rosso2021, hasScore, value)))
    .AddProjectionVariable(guide)
    .AddProjectionVariable(value);

RDFSelectQueryResult result = await query.ApplyToStoreAsync(sqlite);
foreach (System.Data.DataRow row in result.SelectResults.Rows)
    Console.WriteLine($"{row["?GUIDE"]} | {row["?VALUE"]}");
```

</div>

<div class="result" markdown="1">

```
https://winescope.example/2025 | 90^^http://www.w3.org/2001/XMLSchema#integer
https://guidacalici.example/2025 | 88^^http://www.w3.org/2001/XMLSchema#integer
```

</div>

The four-argument `RDFPattern` puts the context first. The query is the one you would run on `memory`: only the argument of `ApplyToStoreAsync` changes.

## It is still there tomorrow

Close the store and the program. The data stays in `vallerosa.db`. A new store on the same connection string finds it where it was left:

```csharp
sqlite.Dispose();

RDFSQLiteStore reopened = new RDFSQLiteStore("Data Source=vallerosa.db");
Console.WriteLine(reopened.QuadruplesCount);
```

<div class="result" markdown="1">

```
153
```

</div>

The formats work on any store too. A persistent store can write a backup file. A memory store can read it:

```csharp
await reopened.ToFileAsync(RDFStoreEnums.RDFFormats.NQuads, "vallerosa-backup.nq");
RDFMemoryStore restored = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.NQuads, "vallerosa-backup.nq");
Console.WriteLine(restored.QuadruplesCount);
```

<div class="result" markdown="1">

```
153
```

</div>

## What the providers share

A provider never keeps a connection open between two calls. Every operation opens its own, does its work and closes it, relying on the connection pool of the driver. One store instance is therefore safe to share between threads: it can be registered as a single instance in the services of a web application. Errors of the database surface as an `RDFStoreException` whose message starts with what the store was doing.

The SQL providers (Firebird, MySQL, Oracle, PostgreSQL, SQLite and SQL Server) keep the data in a table called `Quadruples`, created on the first connection together with the indexes for the combinations of context, subject, predicate and object that the searches use. The table sits well next to the tables of an existing database:

| Column | Holds |
|---|---|
| `QuadrupleID` | the primary key, a number that identifies the quadruple |
| `TripleFlavor` | 1 when the object is a resource, 2 when it is a literal |
| `Context`, `Subject`, `Predicate`, `Object` | the original text of each part |
| `ContextID`, `SubjectID`, `PredicateID`, `ObjectID` | a number for each part, which the indexes use |

The three others take the shape of their engine. Neo4j stores a graph: a `Resource` node with a `uri` for each resource, a `Literal` node with a `value` for each literal and a `Property` relationship between them, which carries the `uri` of the predicate and the `ctx` of the quadruple. MongoDB keeps the quadruples in the collection `quadruples` of a database that is called `rdfsharp` unless you say otherwise. Azure Table keeps them in a table called `Quadruples`.

Every SQL store accepts an options object with three timeouts in seconds: `SelectTimeout`, `DeleteTimeout` and `InsertTimeout`, 120 by default. Raise them when a large merge runs long. Most of these stores also have a method that asks the engine to tidy its files, `OptimizeAsync` on SQLite, MySQL, PostgreSQL and SQL Server, `Optimize` on Oracle.

> A persistent store is as safe as any other store but it is not as fast as a memory one: every call is a round trip to the database. Ask for what you need with the selectors of the search instead of selecting everything and filtering in code, since the selectors become the `WHERE` clause of the database.
{: .tip}

> **Where next.** The Store chapter ends here. The [Query]({{ '/query/' | relative_url }}) chapters ask the cooperative's questions in SPARQL, on a graph as well as on a store.
{: .tip}
