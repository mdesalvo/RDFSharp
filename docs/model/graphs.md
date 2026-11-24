---
title: Graphs
permalink: /model/graphs/
tagline: A named set of triples you can search, combine and trim
lede: A graph is where triples live. It tells you whether a triple is in, finds the ones you describe, merges with other graphs and gives them back as a table.
---

> Each winery of the cooperative sends its own wines and the catalog is the sum of what arrives. When the last bottles of the rosé are sold, the wine leaves the catalog the same way it came in: by one call.
{: .story}

This chapter works with the namespace `RDFSharp.Model` and with `System.Data`. The shorthands below are the ones that every example of the chapter reuses:

```csharp
using System.Data;
using RDFSharp.Model;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFResource producedBy = new RDFResource(vr + "producedBy");
RDFResource hasPrice = new RDFResource(vr + "price");
RDFResource cooperativeClass = new RDFResource(vr + "Cooperative");
RDFResource redWine = new RDFResource(vr + "RedWine");
RDFResource whiteWine = new RDFResource(vr + "WhiteWine");
RDFResource roseWine = new RDFResource(vr + "RoseWine");

RDFResource vallerosa = new RDFResource(vrd + "vallerosa");
RDFResource poggioAlto = new RDFResource(vrd + "poggio-alto");
RDFResource treUlivi = new RDFResource(vrd + "tre-ulivi");
RDFResource rosso2021 = new RDFResource(vrd + "rosso-2021");
RDFResource riserva2019 = new RDFResource(vrd + "riserva-2019");
RDFResource vermentino2023 = new RDFResource(vrd + "vermentino-2023");
RDFResource rosato2024 = new RDFResource(vrd + "rosato-2024");
```

## Create a graph

An `RDFGraph` is a set of triples with a name. The name is its **context**, an absolute IRI that says where the information comes from. You give it with `SetContext` and `AddTriple` fills the graph. A triple added twice is stored once.

The two wineries each send a graph with their own wines: for each wine its class, its English label, its producer and its price. Both also restate who the cooperative is.

```csharp
RDFPlainLiteral rossoLabel = new RDFPlainLiteral("Vallerosa Red", "en");
RDFPlainLiteral riservaLabel = new RDFPlainLiteral("Vallerosa Reserve", "en");
RDFPlainLiteral vermentinoLabel = new RDFPlainLiteral("Vermentino of the Plain", "en");
RDFPlainLiteral rosatoLabel = new RDFPlainLiteral("Valley Rosé", "en");

RDFTypedLiteral rossoPrice = new RDFTypedLiteral("14.50", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
RDFTypedLiteral riservaPrice = new RDFTypedLiteral("32.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
RDFTypedLiteral vermentinoPrice = new RDFTypedLiteral("13.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
RDFTypedLiteral rosatoPrice = new RDFTypedLiteral("10.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);

RDFGraph fromPoggioAlto = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/catalog"))
    .AddTriple(new RDFTriple(vallerosa, RDFVocabulary.RDF.TYPE, cooperativeClass))
    .AddTriple(new RDFTriple(rosso2021, RDFVocabulary.RDF.TYPE, redWine))
    .AddTriple(new RDFTriple(rosso2021, RDFVocabulary.RDFS.LABEL, rossoLabel))
    .AddTriple(new RDFTriple(rosso2021, producedBy, poggioAlto))
    .AddTriple(new RDFTriple(rosso2021, hasPrice, rossoPrice))
    .AddTriple(new RDFTriple(riserva2019, RDFVocabulary.RDF.TYPE, redWine))
    .AddTriple(new RDFTriple(riserva2019, RDFVocabulary.RDFS.LABEL, riservaLabel))
    .AddTriple(new RDFTriple(riserva2019, producedBy, poggioAlto))
    .AddTriple(new RDFTriple(riserva2019, hasPrice, riservaPrice));

RDFGraph fromTreUlivi = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/catalog"))
    .AddTriple(new RDFTriple(vallerosa, RDFVocabulary.RDF.TYPE, cooperativeClass))
    .AddTriple(new RDFTriple(vermentino2023, RDFVocabulary.RDF.TYPE, whiteWine))
    .AddTriple(new RDFTriple(vermentino2023, RDFVocabulary.RDFS.LABEL, vermentinoLabel))
    .AddTriple(new RDFTriple(vermentino2023, producedBy, treUlivi))
    .AddTriple(new RDFTriple(vermentino2023, hasPrice, vermentinoPrice))
    .AddTriple(new RDFTriple(rosato2024, RDFVocabulary.RDF.TYPE, roseWine))
    .AddTriple(new RDFTriple(rosato2024, RDFVocabulary.RDFS.LABEL, rosatoLabel))
    .AddTriple(new RDFTriple(rosato2024, producedBy, treUlivi))
    .AddTriple(new RDFTriple(rosato2024, hasPrice, rosatoPrice));

Console.WriteLine(fromPoggioAlto.Context);
Console.WriteLine(fromPoggioAlto.TriplesCount);
```

<div class="result" markdown="1">

```
https://vallerosa.example/catalog
9
```

</div>

A graph can also start from a list of triples: `new RDFGraph(List<RDFTriple>)`. One that never met `SetContext` takes the default context of the library, which the Turtle serializer writes as the `@base` of a file (the [formats]({{ '/model/formats/' | relative_url }}) chapter shows it):

```csharp
RDFGraph untouched = new RDFGraph();
Console.WriteLine(untouched.Context);
```

<div class="result" markdown="1">

```
https://rdfsharp.codeplex.com/
```

</div>

## Look inside

`ContainsTripleAsync` tells whether a triple is in the graph and `TriplesCount` tells how many there are. A graph is also a sequence of triples, so `foreach` walks it:

```csharp
RDFTriple rossoPriceTriple = new RDFTriple(rosso2021, hasPrice, rossoPrice);
Console.WriteLine(await fromPoggioAlto.ContainsTripleAsync(rossoPriceTriple));
Console.WriteLine(await fromTreUlivi.ContainsTripleAsync(rossoPriceTriple));

foreach (RDFTriple triple in fromTreUlivi)
    if (triple.Predicate.Equals(RDFVocabulary.RDFS.LABEL))
        Console.WriteLine(triple.Object);
```

<div class="result" markdown="1">

```
True
False
Vermentino of the Plain@EN
Valley Rosé@EN
```

</div>

The same walk is available as an enumerator: `TriplesEnumerator` hands out the triples one at a time, for code that drives the iteration itself.

```csharp
IEnumerator<RDFTriple> walker = fromTreUlivi.TriplesEnumerator;
int labels = 0;
while (walker.MoveNext())
    if (walker.Current.Predicate.Equals(RDFVocabulary.RDFS.LABEL))
        labels++;
Console.WriteLine(labels);
```

<div class="result" markdown="1">

```
2
```

</div>

The triple you pass to `ContainsTripleAsync` does not have to be the object that went in: two triples with the same subject, predicate and object are the same triple.

Two graphs are equal when they hold the same triples: `Equals` ignores their contexts. `ToString` gives the context.

```csharp
RDFGraph copy = new RDFGraph(await fromTreUlivi.SelectTriplesAsync())
    .SetContext(new Uri("https://vallerosa.example/copy"));
Console.WriteLine(copy);
Console.WriteLine(copy.Equals(fromTreUlivi));
```

<div class="result" markdown="1">

```
https://vallerosa.example/copy
True
```

</div>

## Merge the contributions

The three **set operations** take the graph they are called on and another one and give back a new graph. Neither of the two is changed.

| Method | Gives |
|---|---|
| `UnionWithAsync` | the triples of both graphs |
| `IntersectWithAsync` | the triples found in both |
| `DifferenceWithAsync` | the triples of this graph that are not in the other |

The catalog is the union of the two contributions:

```csharp
RDFGraph merged = await fromPoggioAlto.UnionWithAsync(fromTreUlivi);
Console.WriteLine(merged.TriplesCount);
Console.WriteLine(merged.Context);
RDFGraph catalog = merged.SetContext(new Uri("https://vallerosa.example/catalog"));
Console.WriteLine(catalog.Context);
```

<div class="result" markdown="1">

```
17
https://rdfsharp.codeplex.com/
https://vallerosa.example/catalog
```

</div>

Nine and nine make seventeen, not eighteen: the sentence about the cooperative arrives from both wineries and a graph keeps it once. A new graph does not inherit the context of its parents, so the merged one starts in the default context and gets its name back with `SetContext`.

The other two operations tell what the wineries have in common and what only one of them says:

```csharp
RDFGraph shared = await fromPoggioAlto.IntersectWithAsync(fromTreUlivi);
Console.WriteLine(shared.TriplesCount);
foreach (RDFTriple triple in shared)
    Console.WriteLine(triple);

RDFGraph onlyPoggio = await catalog.DifferenceWithAsync(fromTreUlivi);
Console.WriteLine(onlyPoggio.TriplesCount);
Console.WriteLine(onlyPoggio.Equals(fromPoggioAlto));
```

<div class="result" markdown="1">

```
1
https://vallerosa.example/id/vallerosa http://www.w3.org/1999/02/22-rdf-syntax-ns#type https://vallerosa.example/vocab#Cooperative
8
False
```

</div>

What Poggio Alto alone says is eight triples, one less than it sent: the ninth is the shared one, which the other winery says too. That is also why the difference is not equal to the original contribution.

The operations return graphs, so they chain: `a.IntersectWith(b).UnionWith(c)` reads left to right.

## Find triples

`SelectTriplesAsync` answers the question "which triples look like this?". It takes four optional arguments, the **selectors**, one for each position of a triple:

| Selector | Type | Fixes |
|---|---|---|
| `s` | `RDFResource` | the subject |
| `p` | `RDFResource` | the predicate |
| `o` | `RDFResource` | the object, when it is a resource |
| `l` | `RDFLiteral` | the object, when it is a literal |

A selector you leave out matches anything. The ones you give must all hold, so they narrow the result together. With no selector at all you get every triple of the graph. Pass them by name, as in the examples: the order of the parameters then does not matter.

```csharp
foreach (RDFTriple triple in await catalog.SelectTriplesAsync(s: rosso2021))
    Console.WriteLine($"{triple.Predicate} | {triple.Object}");
```

<div class="result" markdown="1">

```
http://www.w3.org/1999/02/22-rdf-syntax-ns#type | https://vallerosa.example/vocab#RedWine
http://www.w3.org/2000/01/rdf-schema#label | Vallerosa Red@EN
https://vallerosa.example/vocab#producedBy | https://vallerosa.example/id/poggio-alto
https://vallerosa.example/vocab#price | 14.50^^http://www.w3.org/2001/XMLSchema#decimal
```

</div>

Everything the catalog says about the Vallerosa Red. By predicate, every producer link of the catalog:

```csharp
foreach (RDFTriple triple in await catalog.SelectTriplesAsync(p: producedBy))
    Console.WriteLine($"{triple.Subject} | {triple.Object}");
```

<div class="result" markdown="1">

```
https://vallerosa.example/id/rosso-2021 | https://vallerosa.example/id/poggio-alto
https://vallerosa.example/id/riserva-2019 | https://vallerosa.example/id/poggio-alto
https://vallerosa.example/id/vermentino-2023 | https://vallerosa.example/id/tre-ulivi
https://vallerosa.example/id/rosato-2024 | https://vallerosa.example/id/tre-ulivi
```

</div>

By object, the red wines. By literal, whatever costs exactly 13.00:

```csharp
foreach (RDFTriple triple in await catalog.SelectTriplesAsync(o: redWine))
    Console.WriteLine(triple.Subject);

foreach (RDFTriple triple in await catalog.SelectTriplesAsync(l: vermentinoPrice))
    Console.WriteLine($"{triple.Subject} | {triple.Predicate}");
```

<div class="result" markdown="1">

```
https://vallerosa.example/id/rosso-2021
https://vallerosa.example/id/riserva-2019
https://vallerosa.example/id/vermentino-2023 | https://vallerosa.example/vocab#price
```

</div>

A selector compares RDF terms, not values. The catalog writes the price as `13.00`: a decimal written `13` has the same value but is another literal, so it finds nothing. Selectors also combine freely, here the predicate and the object together:

```csharp
RDFTypedLiteral vermentinoShortPrice = new RDFTypedLiteral("13", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
Console.WriteLine((await catalog.SelectTriplesAsync(l: vermentinoShortPrice)).Count);
Console.WriteLine((await catalog.SelectTriplesAsync(p: RDFVocabulary.RDF.TYPE, o: redWine)).Count);
Console.WriteLine((await catalog.SelectTriplesAsync()).Count);
```

<div class="result" markdown="1">

```
0
2
17
```

</div>

When you want a graph rather than a list, use the indexer: it takes the same four arguments and returns the matching triples as a new `RDFGraph`. That graph is ready for the next operation, for example to save only the wines of one winery:

```csharp
RDFGraph ofTreUlivi = catalog[p: producedBy, o: treUlivi];
Console.WriteLine(ofTreUlivi.TriplesCount);
Console.WriteLine(ofTreUlivi.Context);
```

<div class="result" markdown="1">

```
2
https://rdfsharp.codeplex.com/
```

</div>

The new graph does not inherit the context of the catalog: it has the library's default one, so give it its own with `SetContext` before you write it anywhere.

An object that is a resource and an object that is a literal exclude each other, because a triple has one object only. Asking for both is an error:

```csharp
try
{
    await catalog.SelectTriplesAsync(o: redWine, l: vermentinoPrice);
}
catch (RDFModelException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
Cannot access a graph when both object and literals are given: they must be mutually exclusive!
```

</div>

Every selector you give is a lookup in one of the four indexes of the graph, by subject, predicate, object or literal, which returns the triples that have that term. The graph then intersects those sets starting from the smallest, so a selection costs as much as its most selective selector and does not depend on the size of the graph. Only a selection with no selector reads every triple.

The same selectors drive three methods, which differ in what they give back:

| Call | Gives | The graph |
|---|---|---|
| `SelectTriplesAsync(...)` | a `List<RDFTriple>` | stays as it is |
| `graph[...]` | a new `RDFGraph` | stays as it is |
| `RemoveTriplesAsync(...)` | the graph itself | loses the matching triples |

## Remove triples

The rosé is sold out. `RemoveTripleAsync` removes one triple and `RemoveTriplesAsync` removes all those that match the same four arguments of `SelectTriplesAsync`. First the price of the rosé, a single triple, then everything else that has the rosé as subject:

```csharp
await catalog.RemoveTripleAsync(new RDFTriple(rosato2024, hasPrice, rosatoPrice));
Console.WriteLine(catalog.TriplesCount);

await catalog.RemoveTriplesAsync(s: rosato2024);
Console.WriteLine(catalog.TriplesCount);
Console.WriteLine((await catalog.SelectTriplesAsync(s: rosato2024)).Count);
```

<div class="result" markdown="1">

```
16
13
0
```

</div>

> Mind the arguments of `RemoveTriplesAsync`: a call with none of them matches every triple and empties the graph, exactly like `ClearTriplesAsync()`.
{: .warn}

## Move a graph through a table

A graph converts to a `DataTable` with one row for each triple and the columns `?SUBJECT`, `?PREDICATE` and `?OBJECT`. The table is named after the context of the graph. `RDFGraph.FromDataTableAsync` does the opposite and reads the context back from the name of the table.

```csharp
DataTable table = await catalog.ToDataTableAsync();
Console.WriteLine(table.TableName);
foreach (DataColumn column in table.Columns)
    Console.WriteLine(column.ColumnName);
Console.WriteLine(table.Rows.Count);

RDFGraph restored = await RDFGraph.FromDataTableAsync(table);
Console.WriteLine(restored.Context);
Console.WriteLine(restored.Equals(catalog));
```

<div class="result" markdown="1">

```
https://vallerosa.example/catalog
?SUBJECT
?PREDICATE
?OBJECT
13
https://vallerosa.example/catalog
True
```

</div>

The cells are the text of each term, so the table is a good bridge to a grid control or to a spreadsheet export. The text of a literal keeps its language tag or its datatype, which is what lets `FromDataTableAsync` rebuild the same triples.

## Synchronous forms

Every `...Async` method of this chapter also has a synchronous form without the suffix, such as `SelectTriples`, `UnionWith` or `FromFile`, for code that cannot await. The count has both forms too: `TriplesCount` and `TriplesCountAsync`.

```csharp
long total = await catalog.TriplesCountAsync;
Console.WriteLine(total);

List<RDFTriple> ofRosso = await catalog.SelectTriplesAsync(s: rosso2021);
Console.WriteLine(ofRosso.Count);
```

<div class="result" markdown="1">

```
13
4
```

</div>

On `net8.0` and later the graph is also an `IAsyncEnumerable<RDFTriple>`, so you can walk it with `await foreach`. This one is not available on `netstandard2.0`:

```csharp
RDFPlainLiteral rossoItalianLabel = new RDFPlainLiteral("Rosso di Vallerosa", "it");

await catalog.AddTripleAsync(new RDFTriple(rosso2021, RDFVocabulary.RDFS.LABEL, rossoItalianLabel));

await foreach (RDFTriple triple in catalog.TriplesEnumeratorAsync)
    if (triple.Subject.Equals(rosso2021) && triple.Predicate.Equals(RDFVocabulary.RDFS.LABEL))
        Console.WriteLine(triple.Object);
```

<div class="result" markdown="1">

```
Vallerosa Red@EN
Rosso di Vallerosa@IT
```

</div>

The catalog is now what the cooperative wanted: the wines of the two wineries in one graph. The next chapter writes it to files that other people can read.
