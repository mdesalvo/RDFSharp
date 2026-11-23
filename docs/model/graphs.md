---
title: Graphs
permalink: /model/graphs/
tagline: RDFGraph, an indexed set of triples
lede: An RDFGraph is a set of triples with a context IRI. It indexes every triple by subject, predicate, object and literal, so it can find, remove and combine triples without scanning them.
---

The examples use the Vallerosa catalog, loaded once at the top of the page:

```csharp
using System.Data;
using RDFSharp.Model;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");
catalog.SetContext(new Uri("https://vallerosa.example/catalog"));

RDFResource rosso = new RDFResource(vrd + "rosso-2021");
RDFResource poggioAlto = new RDFResource(vrd + "poggio-alto");
RDFResource producedBy = new RDFResource(vr + "producedBy");
RDFResource price = new RDFResource(vr + "price");
RDFResource redWine = new RDFResource(vr + "RedWine");
```

## The API

| Member | Returns | Does |
|---|---|---|
| `new RDFGraph()`, `new RDFGraph(List<RDFTriple>)` | | an empty graph, or one filled with the given triples |
| `Context` | `Uri` | the name of the graph, the library's default context until `SetContext` |
| `SetContext(Uri)` | the graph | renames the graph |
| `AddTriple(RDFTriple)` | the graph | adds a triple; a duplicate or a `null` is ignored |
| `TriplesCount`, `TriplesCountAsync` | `long` | the number of triples |
| `ContainsTriple(RDFTriple)` | `bool` | whether the triple is in the graph |
| `SelectTriples(s, p, o, l)` | `List<RDFTriple>` | the triples that match the selectors |
| `this[s, p, o, l]` | `RDFGraph` | the same triples, as a new graph |
| `RemoveTriple(RDFTriple)` | the graph | removes one triple |
| `RemoveTriples(s, p, o, l)` | the graph | removes the triples that match the selectors |
| `ClearTriples()` | | empties the graph |
| `UnionWith`, `IntersectWith`, `DifferenceWith` | `RDFGraph` | set operations, into a new graph |
| `TriplesEnumerator`, `TriplesEnumeratorAsync` | enumerators | walk the triples; the graph is also `IEnumerable<RDFTriple>` |
| `ToDataTable()`, `RDFGraph.FromDataTable(DataTable)` | `DataTable`, `RDFGraph` | a graph as a table of three columns and back |
| `ToFile`, `ToStream`, `FromFile`, `FromStream`, `FromUri` | | the RDF formats, see [RDF formats]({{ '/model/formats/' | relative_url }}) |
| `AddContainer`, `AddCollection`, `AddDatatype` | the graph | write a container, a collection or a datatype as triples |

Every method that reads or changes the graph also has an `...Async` form, which the examples use. The methods that return the graph can be chained.

## The selectors: s, p, o, l

`SelectTriples`, the indexer and `RemoveTriples` take the same four optional parameters. Each one fixes one position of the triple:

| Selector | Type | Fixes |
|---|---|---|
| `s` | `RDFResource` | the subject |
| `p` | `RDFResource` | the predicate |
| `o` | `RDFResource` | the object, when it is a resource |
| `l` | `RDFLiteral` | the object, when it is a literal |

The rules are four:
- a selector left `null` matches anything
- the selectors given must all hold: they combine with AND
- `o` and `l` exclude each other, because a triple has one object
- no selector at all selects every triple

A selector matches an RDF term exactly, not a value: the literal `"13"^^xsd:decimal` does not select a price written `"13.00"^^xsd:decimal`. A blank resource can be a subject or object selector like any other resource.

What the cooperative states about the Vallerosa Red, by subject:

```csharp
foreach (RDFTriple triple in await catalog.SelectTriplesAsync(s: rosso))
    Console.WriteLine($"{triple.Predicate} {triple.Object}");
```

<div class="result" markdown="1">

```
http://www.w3.org/1999/02/22-rdf-syntax-ns#type https://vallerosa.example/vocab#RedWine
http://www.w3.org/2000/01/rdf-schema#label Rosso di Vallerosa@IT
http://www.w3.org/2000/01/rdf-schema#label Vallerosa Red@EN
https://vallerosa.example/vocab#producedBy https://vallerosa.example/id/poggio-alto
https://vallerosa.example/vocab#vintage 2021Z^^http://www.w3.org/2001/XMLSchema#gYear
https://vallerosa.example/vocab#alcohol 13.5^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/vocab#price 14.50^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/vocab#organic false^^http://www.w3.org/2001/XMLSchema#boolean
https://vallerosa.example/vocab#grape https://vallerosa.example/id/sangiovese
https://vallerosa.example/vocab#grape https://vallerosa.example/id/canaiolo
https://vallerosa.example/vocab#blend bnode:fcd4437d772d47abb00d3e11aec73d10
```

</div>

The last triple points to a blank node: the first cell of the blend list, whose name changes at every load. The red wines, by predicate and object. The wines of Fattoria Poggio Alto, by predicate and object again. A price, by predicate and literal:

```csharp
List<RDFTriple> reds = await catalog.SelectTriplesAsync(p: RDFVocabulary.RDF.TYPE, o: redWine);
List<RDFTriple> ofPoggioAlto = await catalog.SelectTriplesAsync(p: producedBy, o: poggioAlto);
List<RDFTriple> priced13 = await catalog.SelectTriplesAsync(p: price, l: new RDFTypedLiteral("13.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
List<RDFTriple> priced13Short = await catalog.SelectTriplesAsync(p: price, l: new RDFTypedLiteral("13", RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
List<RDFTriple> everything = await catalog.SelectTriplesAsync();

Console.WriteLine(reds.Count);
Console.WriteLine(ofPoggioAlto.Count);
Console.WriteLine(priced13[0].Subject);
Console.WriteLine(priced13Short.Count);
Console.WriteLine(everything.Count);
```

<div class="result" markdown="1">

```
2
2
https://vallerosa.example/id/vermentino-2023
0
143
```

</div>

The price written `13` finds nothing: the catalog writes `13.00` and the two are different literals. Giving both `o` and `l` is an error:

```csharp
try
{
    await catalog.SelectTriplesAsync(o: redWine, l: new RDFPlainLiteral("red"));
}
catch (RDFModelException ex)
{
    Console.WriteLine(ex.Message);
}
```

<div class="result" markdown="1">

```
Cannot access a graph when both object and literals are given: they must be mutually exclusive!
```

</div>

**How a selection runs.** Each selector given is a lookup in one of the four indexes, which returns the identifiers of the matching triples. The graph intersects those sets starting from the smallest one, so the cost depends on the most selective selector and not on the size of the graph. A selection without selectors is the only one that reads every triple.

## The three forms of a selection

The same selectors give three results:

| Call | Result | The graph |
|---|---|---|
| `SelectTriples(...)` | a `List<RDFTriple>` | is unchanged |
| `graph[...]` | a new `RDFGraph` with the matching triples | is unchanged |
| `RemoveTriples(...)` | the graph itself, without the matching triples | loses them |

The indexer is the form to use when the result goes on as a graph, for example to a file or to a set operation. The new graph has the library's default context, not the one of the source: give it its own with `SetContext`.

```csharp
RDFGraph prices = catalog[p: price].SetContext(new Uri("https://vallerosa.example/prices"));
Console.WriteLine(prices.Context);
Console.WriteLine(prices.TriplesCount);
```

<div class="result" markdown="1">

```
https://vallerosa.example/prices
6
```

</div>

`RemoveTriples` with no selector matches every triple and empties the graph, exactly like `ClearTriples`.

## Building a graph

A graph starts empty or from a list of triples and grows with `AddTriple`. A triple added twice is stored once: the graph is a set.

```csharp
RDFTriple rossoProducer = new RDFTriple(rosso, producedBy, poggioAlto);
RDFTriple rossoType = new RDFTriple(rosso, RDFVocabulary.RDF.TYPE, redWine);

RDFGraph draft = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/draft"))
    .AddTriple(rossoProducer)
    .AddTriple(rossoType)
    .AddTriple(rossoProducer);

Console.WriteLine(draft.TriplesCount);
Console.WriteLine(draft);
Console.WriteLine(new RDFGraph().Context);
```

<div class="result" markdown="1">

```
2
https://vallerosa.example/draft
https://rdfsharp.codeplex.com/
```

</div>

`ToString` gives the context. A graph that never met `SetContext` keeps the library's default context, which the Turtle writer turns into the `@base` of a file: name every graph you write.

## Membership and equality

`ContainsTriple` asks the index, so it costs the same on any graph. The triple you pass does not have to be the object that went in: two triples with the same subject, predicate and object are the same triple. `Equals` compares two graphs by their triples and ignores their contexts.

```csharp
Console.WriteLine(await catalog.ContainsTripleAsync(new RDFTriple(rosso, producedBy, poggioAlto)));
Console.WriteLine(draft.Equals(catalog[s: rosso, p: producedBy].UnionWith(catalog[s: rosso, p: RDFVocabulary.RDF.TYPE])));
```

<div class="result" markdown="1">

```
True
True
```

</div>

## Set operations

`UnionWith`, `IntersectWith` and `DifferenceWith` take the graph they are called on and another one. Each builds a new graph and leaves both operands as they were. The new graph has the default context.

| Method | Gives |
|---|---|
| `a.UnionWith(b)` | the triples of `a` and of `b` |
| `a.IntersectWith(b)` | the triples found in both |
| `a.DifferenceWith(b)` | the triples of `a` that are not in `b` |

The cooperative merges what each winery sends. Both wineries restate the type of the cooperative, so the union keeps that triple once:

```csharp
RDFResource cooperative = new RDFResource(vrd + "vallerosa");
RDFTriple cooperativeType = new RDFTriple(cooperative, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "Cooperative"));

RDFGraph fromPoggioAlto = catalog[p: producedBy, o: poggioAlto]
    .AddTriple(cooperativeType);
RDFGraph fromTreUlivi = catalog[p: producedBy, o: new RDFResource(vrd + "tre-ulivi")]
    .AddTriple(cooperativeType);

Console.WriteLine((await fromPoggioAlto.UnionWithAsync(fromTreUlivi)).TriplesCount);
Console.WriteLine((await fromPoggioAlto.IntersectWithAsync(fromTreUlivi)).TriplesCount);
Console.WriteLine((await fromPoggioAlto.DifferenceWithAsync(fromTreUlivi)).TriplesCount);
```

<div class="result" markdown="1">

```
5
1
2
```

</div>

## Walking the triples

A graph is an `IEnumerable<RDFTriple>`, so `foreach` walks it. `TriplesEnumerator` gives the same walk as an enumerator, for code that drives it with `MoveNext`. On `net8.0` and later the graph is also an `IAsyncEnumerable<RDFTriple>`: `await foreach` walks it, directly or through `TriplesEnumeratorAsync`. A cancellation token goes in with `WithCancellation`. The order of the walk is the order of the index, not the order of insertion.

```csharp
IEnumerator<RDFTriple> walker = draft.TriplesEnumerator;
while (walker.MoveNext())
    Console.WriteLine(walker.Current.Predicate);

int labels = 0;
await foreach (RDFTriple triple in catalog.TriplesEnumeratorAsync)
    if (triple.Predicate.Equals(RDFVocabulary.RDFS.LABEL))
        labels++;
Console.WriteLine(labels);
```

<div class="result" markdown="1">

```
https://vallerosa.example/vocab#producedBy
http://www.w3.org/1999/02/22-rdf-syntax-ns#type
34
```

</div>

## A graph as a table

`ToDataTable` writes one row for each triple in the columns `?SUBJECT`, `?PREDICATE` and `?OBJECT`. It names the table after the context. `RDFGraph.FromDataTable` reads such a table back and takes the context from its name. A literal keeps its language tag or its datatype in the cell, which is what lets the triples come back unchanged.

```csharp
DataTable table = await draft.ToDataTableAsync();
Console.WriteLine(table.TableName);
foreach (DataRow row in table.Rows)
    Console.WriteLine($"{row["?SUBJECT"]} | {row["?PREDICATE"]} | {row["?OBJECT"]}");

RDFGraph back = await RDFGraph.FromDataTableAsync(table);
Console.WriteLine(back.Equals(draft));
```

<div class="result" markdown="1">

```
https://vallerosa.example/draft
https://vallerosa.example/id/rosso-2021 | https://vallerosa.example/vocab#producedBy | https://vallerosa.example/id/poggio-alto
https://vallerosa.example/id/rosso-2021 | http://www.w3.org/1999/02/22-rdf-syntax-ns#type | https://vallerosa.example/vocab#RedWine
True
```

</div>

## Disposing

A graph is `IDisposable`. On `net8.0` and later it is also `IAsyncDisposable`. Disposing it releases its indexes at once: worth doing for a large graph that is no longer needed.
