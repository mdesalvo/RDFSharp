---
title: Memory stores
permalink: /store/stores/
tagline: A graph that remembers where each fact came from
lede: A store is a collection of quadruples. The memory store keeps them in the process, indexed so that any combination of context, subject, predicate and object answers at once.
---

> The catalog and the two guides move into one store, each in its own context. Then the cooperative asks its first real question: which guide reviewed which wine?
{: .story}

## The store and the graph

A store is to quadruples what a graph is to triples: a set, with no duplicates and no order. `RDFStore` is the abstract class that says what every store can do and `RDFMemoryStore` is the implementation that lives in the memory of your process. The persistent ones, which keep the same promises on a database, are the subject of [the last page of this chapter]({{ '/store/extensions/' | relative_url }}).

Everything on this page works through `RDFStore`, so it works the same on every store. What belongs to `RDFMemoryStore` alone is the enumeration, the set operations and the readers such as `FromFileAsync`. Start with a small store that holds the two scores of the Vallerosa Red.

```csharp
using System.Data;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFContext guidaCalici = new RDFContext("https://guidacalici.example/2025");
RDFContext winescope = new RDFContext("https://winescope.example/2025");
RDFResource rosso = new RDFResource(vrd + "rosso-2021");
RDFResource score = new RDFResource(vr + "score");

RDFQuadruple scoreInCalici = new RDFQuadruple(guidaCalici, rosso, score,
    new RDFTypedLiteral("88", RDFModelEnums.RDFDatatypes.XSD_INTEGER));
RDFQuadruple scoreInWinescope = new RDFQuadruple(winescope, rosso, score,
    new RDFTypedLiteral("90", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

RDFMemoryStore draft = new RDFMemoryStore();
await (await draft.AddQuadrupleAsync(scoreInCalici)).AddQuadrupleAsync(scoreInWinescope);
await draft.AddQuadrupleAsync(scoreInCalici);
Console.WriteLine(draft.QuadruplesCount);
Console.WriteLine(await draft.ContainsQuadrupleAsync(scoreInWinescope));
```

<div class="result" markdown="1">

```
2
True
```

</div>

Adding the first quadruple a second time changes nothing: the count is still 2. `AddQuadruple` returns the store, so calls chain. The other ways to put a quadruple in are the constructor that takes a list and `MergeGraphAsync`, which comes a few paragraphs below.

## Removing quadruples

`RemoveQuadrupleAsync` takes one quadruple out. `RemoveQuadruplesAsync` takes out every quadruple that matches a pattern. `ClearQuadruplesAsync` empties the store.

```csharp
await draft.RemoveQuadrupleAsync(scoreInWinescope);
Console.WriteLine(await draft.ContainsQuadrupleAsync(scoreInWinescope));
Console.WriteLine(draft.QuadruplesCount);

await draft.AddQuadrupleAsync(scoreInWinescope);
await draft.RemoveQuadruplesAsync(c: guidaCalici);
Console.WriteLine(draft.QuadruplesCount);

await draft.ClearQuadruplesAsync();
Console.WriteLine(draft.QuadruplesCount);
```

<div class="result" markdown="1">

```
False
1
1
0
```

</div>

The pattern is made of five optional parameters named `c`, `s`, `p`, `o` and `l`, for context, subject, predicate, object and literal. A parameter left out matches anything, so `c: guidaCalici` means "every quadruple of that guide, whatever it says". The same five parameters drive the searches below. The object and the literal exclude each other because a quadruple has only one of them.

## Merging a graph

The catalog is a graph, not a set of quadruples. `MergeGraphAsync` turns every triple of a graph into a quadruple whose context is the context of the graph, which is why a graph meant for a store needs a context of its own. The file has none, so the catalog gets it with `SetContext`.

```csharp
RDFGraph catalog = (await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl"))
    .SetContext(new Uri("https://vallerosa.example/catalog"));

RDFMemoryStore catalogStore = new RDFMemoryStore();
await catalogStore.MergeGraphAsync(catalog);
Console.WriteLine(catalogStore.QuadruplesCount);
```

<div class="result" markdown="1">

```
143
```

</div>

All 143 triples of the catalog are now quadruples of one context. Duplicates are skipped here too, so merging the same graph twice is harmless.

## Loading the guides

The two guides come ready as a file in TriG, the variant of Turtle with room for named graphs: download [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}) and read it with `RDFMemoryStore.FromFileAsync`. The [next page]({{ '/store/formats/' | relative_url }}) is about the formats. A store of its own comes out, with one context for each guide. `UnionWithAsync` then builds the store that holds both.

```csharp
RDFMemoryStore guides = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");
Console.WriteLine(guides.QuadruplesCount);

RDFMemoryStore store = await catalogStore.UnionWithAsync(guides);
Console.WriteLine(store.QuadruplesCount);
```

<div class="result" markdown="1">

```
10
153
```

</div>

143 plus 10 makes 153, as it should: the sources do not overlap, since no quadruple is shared. To see how the store is divided, ask for its graphs back with `ExtractGraphsAsync`. It gives one `RDFGraph` for each context, which is also the way to list the contexts of a store.

```csharp
foreach (RDFGraph graph in await store.ExtractGraphsAsync())
    Console.WriteLine($"{graph.Context} {graph.TriplesCount}");
```

<div class="result" markdown="1">

```
https://vallerosa.example/catalog 143
https://guidacalici.example/2025 5
https://winescope.example/2025 5
```

</div>

## Searching

`SelectQuadruplesAsync` takes the same five parameters as `RemoveQuadruplesAsync` and returns the list of quadruples that satisfy all the given ones. What the Vallerosa Red is known for in WineScope, by context and subject:

```csharp
List<RDFQuadruple> inWinescope = await store.SelectQuadruplesAsync(c: winescope, s: rosso);
foreach (RDFQuadruple quadruple in inWinescope)
    Console.WriteLine(quadruple);
```

<div class="result" markdown="1">

```
https://winescope.example/2025 https://vallerosa.example/id/rosso-2021 https://vallerosa.example/vocab#score 90^^http://www.w3.org/2001/XMLSchema#integer
```

</div>

The five parameters combine freely. Here is who gave a 90 and which wines are red, by literal and by predicate and object:

```csharp
RDFTypedLiteral ninety = new RDFTypedLiteral("90", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
foreach (RDFQuadruple quadruple in await store.SelectQuadruplesAsync(l: ninety))
    Console.WriteLine(quadruple);

RDFResource redWine = new RDFResource(vr + "RedWine");
foreach (RDFQuadruple quadruple in await store.SelectQuadruplesAsync(p: RDFVocabulary.RDF.TYPE, o: redWine))
    Console.WriteLine(quadruple);
```

<div class="result" markdown="1">

```
https://winescope.example/2025 https://vallerosa.example/id/rosso-2021 https://vallerosa.example/vocab#score 90^^http://www.w3.org/2001/XMLSchema#integer
https://vallerosa.example/catalog https://vallerosa.example/id/rosso-2021 http://www.w3.org/1999/02/22-rdf-syntax-ns#type https://vallerosa.example/vocab#RedWine
https://vallerosa.example/catalog https://vallerosa.example/id/riserva-2019 http://www.w3.org/1999/02/22-rdf-syntax-ns#type https://vallerosa.example/vocab#RedWine
```

</div>

When you want a store back instead of a list, use the indexer: `store[s: rosso]` is a new memory store with everything said about the Vallerosa Red, by every source. Asking for an object and a literal together is refused:

```csharp
Console.WriteLine(store[s: rosso].QuadruplesCount);

try
{
    await store.SelectQuadruplesAsync(o: rosso, l: new RDFPlainLiteral("Vallerosa Red", "en"));
}
catch (RDFStoreException ex)
{
    Console.WriteLine(ex.Message);
}
```

<div class="result" markdown="1">

```
13
Cannot access a store when both object and literals are given: they must be mutually exclusive!
```

</div>

The 13 quadruples of the Vallerosa Red are 11 from the catalog plus the two scores. Now the question of the story. Every score is a quadruple with the predicate `vr:score` whose context is the guide. Sorted by wine, the answer reads off the list:

```csharp
List<RDFQuadruple> scores = await store.SelectQuadruplesAsync(p: score);
foreach (RDFQuadruple quadruple in scores
    .OrderBy(q => q.Subject.ToString(), StringComparer.Ordinal)
    .ThenBy(q => q.Context.ToString(), StringComparer.Ordinal))
    Console.WriteLine($"{quadruple.Subject} | {quadruple.Context} | {((RDFLiteral)quadruple.Object).Value}");
```

<div class="result" markdown="1">

```
https://vallerosa.example/id/bianco-2023 | https://guidacalici.example/2025 | 86
https://vallerosa.example/id/riserva-2019 | https://guidacalici.example/2025 | 94
https://vallerosa.example/id/riserva-2019 | https://winescope.example/2025 | 92
https://vallerosa.example/id/rosato-2024 | https://winescope.example/2025 | 84
https://vallerosa.example/id/rosso-2021 | https://guidacalici.example/2025 | 88
https://vallerosa.example/id/rosso-2021 | https://winescope.example/2025 | 90
https://vallerosa.example/id/vermentino-2023 | https://guidacalici.example/2025 | 89
https://vallerosa.example/id/vermentino-2023 | https://winescope.example/2025 | 87
https://vallerosa.example/id/vinsanto-2015 | https://guidacalici.example/2025 | 95
https://vallerosa.example/id/vinsanto-2015 | https://winescope.example/2025 | 96
```

</div>

Four wines appear twice and two appear once: Guida ai Calici reviewed the white blend and never the rosé, WineScope the other way round. The two guides gave the Vallerosa Red 88 and 90 and the store keeps both. How to ask such questions in SPARQL, against a store, is the work of the [Query]({{ '/query/' | relative_url }}) chapters.

## Iterating and converting

A memory store is an `IEnumerable<RDFQuadruple>`, so `foreach` walks it. The `QuadruplesEnumerator` property gives the same walk as an enumerator. `ToDataTableAsync` writes the store into a table of four columns, one row per quadruple. `RDFMemoryStore.FromDataTableAsync` reads such a table back.

```csharp
int walked = 0;
foreach (RDFQuadruple quadruple in guides)
    walked++;
Console.WriteLine(walked);

DataTable table = await guides.ToDataTableAsync();
foreach (DataColumn column in table.Columns)
    Console.WriteLine(column.ColumnName);

RDFMemoryStore fromTable = await RDFMemoryStore.FromDataTableAsync(table);
Console.WriteLine(fromTable.QuadruplesCount);
```

<div class="result" markdown="1">

```
10
?CONTEXT
?SUBJECT
?PREDICATE
?OBJECT
10
```

</div>

## Set operations

Because a store is a set, two stores can be combined as sets. `UnionWithAsync` has already merged the catalog and the guides. `DifferenceWithAsync` takes away what another store holds, `IntersectWithAsync` keeps what both hold. Each one builds a new memory store and leaves the two operands alone.

```csharp
RDFMemoryStore guidesAgain = await store.DifferenceWithAsync(catalogStore);
Console.WriteLine(guidesAgain.QuadruplesCount);

RDFMemoryStore common = await guides.IntersectWithAsync(catalogStore);
Console.WriteLine(common.QuadruplesCount);
```

<div class="result" markdown="1">

```
10
0
```

</div>

Taking the catalog away from the whole leaves the 10 quadruples of the guides. The guides and the catalog have nothing in common, so their intersection is empty.

## Synchronous forms

Every `...Async` method of this page also has a synchronous form without the suffix, such as `MergeGraph`, `SelectQuadruples` or `FromFile`, with `QuadruplesCount` and `QuadruplesCountAsync` for the count. On a memory store the asynchronous forms run the same work on another thread of the pool: that keeps a user interface responsive, but on a server it only moves the work from one thread to another. The persistent stores, which wait on a database, are where they matter.

```csharp
RDFMemoryStore asyncStore = new RDFMemoryStore();
await asyncStore.MergeGraphAsync(catalog);
Console.WriteLine(await asyncStore.QuadruplesCountAsync);

List<RDFQuadruple> names = await asyncStore.SelectQuadruplesAsync(s: rosso, p: RDFVocabulary.RDFS.LABEL);
Console.WriteLine(names.Count);
```

<div class="result" markdown="1">

```
143
2
```

</div>

The Vallerosa Red has two labels, one in English and one in Italian. On `net8.0` and later a memory store is also an `IAsyncEnumerable<RDFQuadruple>`, so it can be walked with `await foreach`. A cancellation token goes in through `WithCancellation`:

```csharp
int streamed = 0;
await foreach (RDFQuadruple quadruple in guides)
    streamed++;
Console.WriteLine(streamed);
```

<div class="result" markdown="1">

```
10
```

</div>

> A memory store is `IDisposable`. Dispose it, with `using` or by calling `Dispose`, when you are done with a large store: it releases the indexes at once instead of waiting for the garbage collector.
{: .tip}

> **Where next.** [Store formats]({{ '/store/formats/' | relative_url }}) saves this store as N-Quads, TriX and TriG and reads it back.
{: .tip}
