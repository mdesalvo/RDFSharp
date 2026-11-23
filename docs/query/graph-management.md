---
title: Load and manage graphs
permalink: /query/graph-management/
tagline: Moving whole contexts around instead of single triples
lede: "Seven operations work on named graphs as a whole: one loads a remote document and the others create, empty, drop, copy, move and merge the contexts of a store."
---

> The guides of 2025 are old news: their editions of 2026 are about to arrive. The cooperative archives what each guide said into a context of its own, so that the original contexts are free for the new scores.
{: .story}

## Operations on whole graphs

The operations of the [previous page]({{ '/query/insert-delete/' | relative_url }}) work on triples. These seven work on graphs, which a store keeps as contexts. Each has its own class and a `Silent()` call that turns a failure into an empty result.

| SPARQL Update | RDFSharp class | What it does on a store |
|---|---|---|
| `LOAD <source> INTO GRAPH <target>` | `RDFLoadOperation` | reads a remote RDF document into a context |
| `CREATE GRAPH <context>` | `RDFCreateOperation` | declares a context: no effect on a memory store |
| `CLEAR GRAPH <context>` | `RDFClearOperation` | removes every quadruple of a context |
| `DROP GRAPH <context>` | `RDFDropOperation` | removes a context: the same as `CLEAR` on a memory store |
| `COPY <source> TO <target>` | `RDFCopyOperation` | empties the target then copies the source into it |
| `MOVE <source> TO <target>` | `RDFMoveOperation` | does what `COPY` does then empties the source |
| `ADD <source> TO <target>` | `RDFAddOperation` | copies the source into the target and leaves what it holds |

They are meant for a store, the only data source that has contexts. The page works on the same store as the [Store chapters]({{ '/store/stores/' | relative_url }}): the catalog in its context and the two guides, 153 quadruples in all.

```csharp
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

RDFGraph catalog = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl")
    .SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore catalogStore = new RDFMemoryStore();
catalogStore.MergeGraph(catalog);
RDFMemoryStore store = catalogStore.UnionWith(
    RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig"));

Uri guidaCalici = new Uri("https://guidacalici.example/2025");
Uri winescope = new Uri("https://winescope.example/2025");
Uri archiveCalici = new Uri("https://vallerosa.example/archive/guidacalici-2025");
Uri archiveWinescope = new Uri("https://vallerosa.example/archive/winescope-2025");

Console.WriteLine(store.QuadruplesCount);
```

<div class="result" markdown="1">

```
153
```

</div>

## Create

`CREATE` declares a context before anything is written into it. A memory store does not record empty graphs, so the operation is legal and does nothing: it matters once it is sent to a SPARQL Update endpoint that keeps them. `SILENT` is there for the endpoints that raise an error when the context exists already.

<div class="pair" markdown="1">

```sparql
CREATE SILENT GRAPH <https://vallerosa.example/archive/guidacalici-2025> ;
CREATE SILENT GRAPH <https://vallerosa.example/archive/winescope-2025>
```

```csharp
RDFCreateOperation createCalici = new RDFCreateOperation(archiveCalici).Silent();
RDFCreateOperation createWinescope = new RDFCreateOperation(archiveWinescope).Silent();

RDFOperationResult createResult = createCalici.ApplyToStore(store);
createWinescope.ApplyToStore(store);
Console.WriteLine($"{createResult.InsertResultsCount} {createResult.DeleteResultsCount} {store.QuadruplesCount}");
```

</div>

<div class="result" markdown="1">

```
0 0 153
```

</div>

## Copy

`COPY` replaces the target with a copy of the source. The archive of Guida ai Calici gets its five scores and the original stays where it is.

<div class="pair" markdown="1">

```sparql
COPY <https://guidacalici.example/2025>
  TO <https://vallerosa.example/archive/guidacalici-2025>
```

```csharp
RDFCopyOperation copyCalici = new RDFCopyOperation()
    .SetFromContext(guidaCalici)
    .SetToContext(archiveCalici);

RDFOperationResult copyResult = copyCalici.ApplyToStore(store);
Console.WriteLine($"{copyResult.InsertResultsCount} {copyResult.DeleteResultsCount} {store.QuadruplesCount}");
```

</div>

<div class="result" markdown="1">

```
5 0 158
```

</div>

Five quadruples were inserted and none removed, because the target was empty. Had it held anything, `COPY` would have removed it first and the removed quadruples would be in `DeleteResults`.

## Clear

`CLEAR` empties a context and the context is then free. The copy is in place, so the original of Guida ai Calici can be cleared for the 2026 edition.

<div class="pair" markdown="1">

```sparql
CLEAR GRAPH <https://guidacalici.example/2025>
```

```csharp
RDFClearOperation clearCalici = new RDFClearOperation(guidaCalici);

RDFOperationResult clearResult = clearCalici.ApplyToStore(store);
Console.WriteLine($"{clearResult.InsertResultsCount} {clearResult.DeleteResultsCount} {store.QuadruplesCount}");
```

</div>

<div class="result" markdown="1">

```
0 5 153
```

</div>

`CLEAR` also takes a flavor instead of a context: `new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.NAMED)` is `CLEAR NAMED` and empties every named graph, `DEFAULT` empties the default graph only and `ALL` empties the whole store. `COPY` followed by `CLEAR` is exactly what `MOVE` does in one step.

## Move

`MOVE` copies a context into another and empties the source. WineScope goes the short way.

<div class="pair" markdown="1">

```sparql
MOVE <https://winescope.example/2025>
  TO <https://vallerosa.example/archive/winescope-2025>
```

```csharp
RDFMoveOperation moveWinescope = new RDFMoveOperation()
    .SetFromContext(winescope)
    .SetToContext(archiveWinescope);

RDFOperationResult moveResult = moveWinescope.ApplyToStore(store);
Console.WriteLine($"{moveResult.InsertResultsCount} {moveResult.DeleteResultsCount} {store.QuadruplesCount}");
foreach (RDFGraph graph in store.ExtractGraphs())
    Console.WriteLine($"{graph.Context} {graph.TriplesCount}");
```

</div>

<div class="result" markdown="1">

```
5 5 153
https://vallerosa.example/catalog 143
https://vallerosa.example/archive/winescope-2025 5
https://vallerosa.example/archive/guidacalici-2025 5
```

</div>

Five quadruples came in and five went out, so the store still holds 153. Its three contexts are the catalog and the two archives. The two original contexts of the guides are gone for good: a context with no quadruples does not exist in a memory store, which is why `CREATE` has nothing to do there.

## Add and drop

`ADD` is `COPY` without the emptying of the target: the target keeps what it had and receives the source on top. An auditor wants both archives in one place, in a context made for the occasion, then drops it when the audit is over.

<div class="pair" markdown="1">

```sparql
ADD <https://vallerosa.example/archive/guidacalici-2025>
 TO <https://vallerosa.example/archive/guides-2025> ;
ADD <https://vallerosa.example/archive/winescope-2025>
 TO <https://vallerosa.example/archive/guides-2025>
```

```csharp
Uri audit = new Uri("https://vallerosa.example/archive/guides-2025");

RDFOperationResult addCalici = new RDFAddOperation()
    .SetFromContext(archiveCalici)
    .SetToContext(audit)
    .ApplyToStore(store);
RDFOperationResult addWinescope = new RDFAddOperation()
    .SetFromContext(archiveWinescope)
    .SetToContext(audit)
    .ApplyToStore(store);
Console.WriteLine($"{addCalici.InsertResultsCount} {addWinescope.InsertResultsCount} {store.QuadruplesCount}");
```

</div>

<div class="result" markdown="1">

```
5 5 163
```

</div>

The two `ADD` operations stack up: 5 and 5 quadruples now live in the audit context, which brings the store to 163. `DROP` removes the context when it is no longer needed.

<div class="pair" markdown="1">

```sparql
DROP GRAPH <https://vallerosa.example/archive/guides-2025>
```

```csharp
RDFOperationResult dropResult = new RDFDropOperation(audit).ApplyToStore(store);
Console.WriteLine($"{dropResult.DeleteResultsCount} {store.QuadruplesCount}");
```

</div>

<div class="result" markdown="1">

```
10 153
```

</div>

On a memory store `DROP` and `CLEAR` do the same thing: there is no empty graph left behind to tell them apart. Like `CLEAR`, `DROP` also takes a flavor (`DEFAULT`, `NAMED` or `ALL`) in place of a context.

## Load

`LOAD` reads an RDF document that lives at a web address and writes it into a context. The editions of 2026 will come that way: each guide publishes its scores and the cooperative loads them into the context of the year.

<div class="pair" markdown="1">

```sparql
LOAD SILENT <https://guidacalici.example/2026/scores.trig>
  INTO GRAPH <https://guidacalici.example/2026>
```

```csharp
RDFLoadOperation loadScores = new RDFLoadOperation(new Uri("https://guidacalici.example/2026/scores.trig"))
    .SetContext(new Uri("https://guidacalici.example/2026"))
    .Silent();

RDFOperationResult loadResult = loadScores.ApplyToStore(store);
Console.WriteLine($"{loadResult.InsertResultsCount} {store.QuadruplesCount}");
```

</div>

<div class="result" markdown="1">

```
0 153
```

</div>

The guides of the manual are fictional and their addresses lead nowhere, so the load fails and `SILENT` turns the failure into an empty result: no quadruple arrives and the store is untouched. Without `Silent()` the same call throws an `RDFStoreException`. When the address does answer, the store loads what the server sends if it declares itself as N-Quads, TriX or TriG and the context given to `SetContext` takes every quadruple of the document, even when the document names several graphs. Without `SetContext` the quadruples keep their own contexts.

The operation speaks HTTP and HTTPS only. It cannot load a `file:` address, so a document on your disk is read the way the Store chapters read it, with `RDFMemoryStore.FromFile` and added with `UnionWith`.

## On a graph

A graph has no contexts, so these operations lose most of their meaning on one. `CLEAR` and `DROP` empty the graph and `CREATE` has nothing to do. `COPY`, `MOVE` and `ADD` need two graphs to tell apart and refuse to run, with an error that says why. `SILENT` is what turns that error into an empty result.

```csharp
RDFOperationResult silentResult = copyCalici.Silent().ApplyToGraph(catalog);
Console.WriteLine(silentResult.InsertResultsCount);

try
{
    new RDFMoveOperation().SetFromContext(winescope).SetToContext(archiveWinescope).ApplyToGraph(catalog);
}
catch (RDFQueryException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
0
SPARQL MOVE makes no sense on an RDFGraph: it moves data between two named graphs, but an RDFGraph is a single contextless graph (there is no source/destination graph to tell apart). Apply this MOVE to an RDFStore (whose quadruples carry a context) instead; or, if you really mean to work within a single graph, use a GRAPH-scoped INSERT/DELETE WHERE operation.
```

</div>

> Against a SPARQL Update endpoint the same seven operations become requests: the [endpoints]({{ '/query/endpoints/' | relative_url }}) page shows how to send them. To run several of them as one unit, in order, see [operation sets]({{ '/query/operation-sets/' | relative_url }}).
{: .tip}
