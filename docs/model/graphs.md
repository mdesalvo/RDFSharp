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
RDFResource cooperative = new RDFResource(vrd + "vallerosa");
RDFResource poggioAlto = new RDFResource(vrd + "poggio-alto");
RDFResource treUlivi = new RDFResource(vrd + "tre-ulivi");
RDFResource rosso = new RDFResource(vrd + "rosso-2021");
RDFResource riserva = new RDFResource(vrd + "riserva-2019");
RDFResource vermentino = new RDFResource(vrd + "vermentino-2023");
RDFResource rosato = new RDFResource(vrd + "rosato-2024");
RDFResource redWine = new RDFResource(vr + "RedWine");
```

## Create a graph

An `RDFGraph` is a set of triples with a name. The name is its **context**, an absolute IRI that says where the information comes from. You give it with `SetContext` and `AddTriple` fills the graph. A triple added twice is stored once.

The two wineries each send a graph with their own wines: for each wine its class, its English label, its producer and its price. Both also restate who the cooperative is.

```csharp
RDFGraph fromPoggioAlto = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/catalog"))
    .AddTriple(new RDFTriple(cooperative, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "Cooperative")))
    .AddTriple(new RDFTriple(rosso, RDFVocabulary.RDF.TYPE, redWine))
    .AddTriple(new RDFTriple(rosso, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Vallerosa Red", "en")))
    .AddTriple(new RDFTriple(rosso, producedBy, poggioAlto))
    .AddTriple(new RDFTriple(rosso, hasPrice, new RDFTypedLiteral("14.50", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))
    .AddTriple(new RDFTriple(riserva, RDFVocabulary.RDF.TYPE, redWine))
    .AddTriple(new RDFTriple(riserva, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Vallerosa Reserve", "en")))
    .AddTriple(new RDFTriple(riserva, producedBy, poggioAlto))
    .AddTriple(new RDFTriple(riserva, hasPrice, new RDFTypedLiteral("32.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));

RDFGraph fromTreUlivi = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/catalog"))
    .AddTriple(new RDFTriple(cooperative, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "Cooperative")))
    .AddTriple(new RDFTriple(vermentino, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "WhiteWine")))
    .AddTriple(new RDFTriple(vermentino, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Vermentino of the Plain", "en")))
    .AddTriple(new RDFTriple(vermentino, producedBy, treUlivi))
    .AddTriple(new RDFTriple(vermentino, hasPrice, new RDFTypedLiteral("13.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))
    .AddTriple(new RDFTriple(rosato, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RoseWine")))
    .AddTriple(new RDFTriple(rosato, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Valley Rosé", "en")))
    .AddTriple(new RDFTriple(rosato, producedBy, treUlivi))
    .AddTriple(new RDFTriple(rosato, hasPrice, new RDFTypedLiteral("10.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));

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

`ContainsTriple` tells whether a triple is in the graph and `TriplesCount` tells how many there are. A graph is also a sequence of triples, so `foreach` walks it:

```csharp
RDFTriple rossoPrice = new RDFTriple(rosso, hasPrice, new RDFTypedLiteral("14.50", RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
Console.WriteLine(fromPoggioAlto.ContainsTriple(rossoPrice));
Console.WriteLine(fromTreUlivi.ContainsTriple(rossoPrice));

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

The triple you pass to `ContainsTriple` does not have to be the object that went in: two triples with the same subject, predicate and object are the same triple.

## Merge the contributions

The three **set operations** take the graph they are called on and another one and give back a new graph. Neither of the two is changed.

| Method | Gives |
|---|---|
| `UnionWith` | the triples of both graphs |
| `IntersectWith` | the triples found in both |
| `DifferenceWith` | the triples of this graph that are not in the other |

The catalog is the union of the two contributions:

```csharp
RDFGraph merged = fromPoggioAlto.UnionWith(fromTreUlivi);
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
RDFGraph shared = fromPoggioAlto.IntersectWith(fromTreUlivi);
Console.WriteLine(shared.TriplesCount);
foreach (RDFTriple triple in shared)
    Console.WriteLine(triple);

RDFGraph onlyPoggio = catalog.DifferenceWith(fromTreUlivi);
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

`SelectTriples` answers the question "which triples look like this?". It takes four optional arguments, one for each part of a triple: `s` for the subject, `p` for the predicate, `o` for an object that is a resource and `l` for an object that is a literal. An argument you leave out matches anything and the ones you give must all hold.

```csharp
foreach (RDFTriple triple in catalog.SelectTriples(s: rosso))
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
foreach (RDFTriple triple in catalog.SelectTriples(p: producedBy))
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
foreach (RDFTriple triple in catalog.SelectTriples(o: redWine))
    Console.WriteLine(triple.Subject);

RDFTypedLiteral thirteen = new RDFTypedLiteral("13.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
foreach (RDFTriple triple in catalog.SelectTriples(l: thirteen))
    Console.WriteLine($"{triple.Subject} | {triple.Predicate}");
```

<div class="result" markdown="1">

```
https://vallerosa.example/id/rosso-2021
https://vallerosa.example/id/riserva-2019
https://vallerosa.example/id/vermentino-2023 | https://vallerosa.example/vocab#price
```

</div>

When you want a graph rather than a list, use the indexer: it takes the same four arguments and returns the matching triples as a new `RDFGraph`. That graph is ready for the next operation, for example to save only the wines of one winery:

```csharp
RDFGraph ofTreUlivi = catalog[p: producedBy, o: treUlivi];
Console.WriteLine(ofTreUlivi.TriplesCount);
```

<div class="result" markdown="1">

```
2
```

</div>

An object that is a resource and an object that is a literal exclude each other, because a triple has one object only. Asking for both is an error:

```csharp
try
{
    catalog.SelectTriples(o: redWine, l: thirteen);
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

## Remove triples

The rosé is sold out. `RemoveTriple` removes one triple and `RemoveTriples` removes all those that match the same four arguments of `SelectTriples`. First the price of the rosé, a single triple, then everything else that has the rosé as subject:

```csharp
catalog.RemoveTriple(new RDFTriple(rosato, hasPrice, new RDFTypedLiteral("10.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
Console.WriteLine(catalog.TriplesCount);

catalog.RemoveTriples(s: rosato);
Console.WriteLine(catalog.TriplesCount);
Console.WriteLine(catalog.SelectTriples(s: rosato).Count);
```

<div class="result" markdown="1">

```
16
13
0
```

</div>

> Mind the arguments of `RemoveTriples`: a call with none of them matches every triple and empties the graph, exactly like `ClearTriples()`.
{: .warn}

## Move a graph through a table

A graph converts to a `DataTable` with one row for each triple and the columns `?SUBJECT`, `?PREDICATE` and `?OBJECT`. The table is named after the context of the graph. `RDFGraph.FromDataTable` does the opposite and reads the context back from the name of the table.

```csharp
DataTable table = catalog.ToDataTable();
Console.WriteLine(table.TableName);
foreach (DataColumn column in table.Columns)
    Console.WriteLine(column.ColumnName);
Console.WriteLine(table.Rows.Count);

RDFGraph restored = RDFGraph.FromDataTable(table);
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

The cells are the text of each term, so the table is a good bridge to a grid control or to a spreadsheet export. The text of a literal keeps its language tag or its datatype, which is what lets `FromDataTable` rebuild the same triples.

## Without blocking

Every operation that reads or writes a graph has a twin that returns a `Task` and carries the suffix `Async`: `AddTripleAsync`, `SelectTriplesAsync`, `UnionWithAsync`, `ToDataTableAsync`, `FromFileAsync` and the others. They are available on every target of the library.

```csharp
long total = await catalog.TriplesCountAsync;
Console.WriteLine(total);

List<RDFTriple> ofRosso = await catalog.SelectTriplesAsync(s: rosso);
Console.WriteLine(ofRosso.Count);
```

<div class="result" markdown="1">

```
13
4
```

</div>

On `net8.0` the graph is also an `IAsyncEnumerable<RDFTriple>`, so you can walk it with `await foreach`. This one is not available on `netstandard2.0`:

```csharp
await catalog.AddTripleAsync(new RDFTriple(rosso, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Rosso di Vallerosa", "it")));

await foreach (RDFTriple triple in catalog.TriplesEnumeratorAsync)
    if (triple.Subject.Equals(rosso) && triple.Predicate.Equals(RDFVocabulary.RDFS.LABEL))
        Console.WriteLine(triple.Object);
```

<div class="result" markdown="1">

```
Vallerosa Red@EN
Rosso di Vallerosa@IT
```

</div>

The catalog is now what the cooperative wanted: the wines of the two wineries in one graph. The next chapter writes it to files that other people can read.
