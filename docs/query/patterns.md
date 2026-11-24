---
title: Variables and patterns
permalink: /query/patterns/
tagline: Describing the shape of what you look for
lede: A query is a drawing of the triples you expect to find, with holes where the answer goes. Variables are the holes, patterns are the triples, pattern groups hold them together.
---

> The cooperative wants a first list for its website: every wine with the name of its producer. Then it asks for the organic flag where there is one and for reds and whites together.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}), loaded as in [Getting started]({{ '/getting-started/' | relative_url }}) with `RDFGraph.FromFileAsync` and at the end on a store that holds the catalog and the two guides, built as in [Memory stores]({{ '/store/stores/' | relative_url }}). It needs three namespaces of the library:

```csharp
using System.Data;
using RDFSharp.Model;   // resources, literals, graphs
using RDFSharp.Query;   // variables, patterns, queries
using RDFSharp.Store;   // contexts and memory stores
```

## Variables

A **variable** is a named placeholder: the query engine fills it with every value that makes the pattern true. In SPARQL it starts with a question mark. In C# it is an `RDFVariable` and you give it the name without the mark.

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFVariable wine = new RDFVariable("wine");
RDFVariable producer = new RDFVariable("producer");
RDFVariable label = new RDFVariable("label");
```

The engine writes every variable name in upper case, so `wine` becomes `?WINE` in the printed query and in the columns of the results.

## Patterns and pattern groups

A **pattern** is a triple in which any of the three positions can be a variable. It answers the question "which triples look like this?". A **pattern group** is a set of patterns that must all hold together for the same values of their shared variables: the engine joins them on the variables they have in common.

Two patterns tell the whole story of the first list: a wine has a producer and that producer has a label.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?wine ?label
WHERE {
  ?wine vr:producedBy ?producer .
  ?producer rdfs:label ?label .
}
```

```csharp
RDFResource producedBy = new RDFResource(vr + "producedBy");

RDFPattern wineProducedBy = new RDFPattern(wine, producedBy, producer);
RDFPattern producerLabel = new RDFPattern(producer, RDFVocabulary.RDFS.LABEL, label);

RDFPatternGroup winesAndProducers = new RDFPatternGroup()
    .AddPattern(wineProducedBy)
    .AddPattern(producerLabel);

RDFSelectQuery query = new RDFSelectQuery()
    .AddPatternGroup(winesAndProducers)
    .AddProjectionVariable(wine)
    .AddProjectionVariable(label);

RDFSelectQueryResult result = await query.ApplyToGraphAsync(catalog);
foreach (DataRow row in result.SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?LABEL"]}");
```

</div>

<div class="result" markdown="1">

| ?WINE | ?LABEL |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `Fattoria Poggio Alto` |
| `https://vallerosa.example/id/riserva-2019` | `Fattoria Poggio Alto` |
| `https://vallerosa.example/id/bianco-2023` | `Tenuta Le Crete` |
| `https://vallerosa.example/id/vinsanto-2015` | `Tenuta Le Crete` |
| `https://vallerosa.example/id/vermentino-2023` | `Cantina dei Tre Ulivi` |
| `https://vallerosa.example/id/rosato-2024` | `Cantina dei Tre Ulivi` |

</div>

Three things to notice. The projection, which says which variables come out, is explicit: `?producer` takes part in the join but it is not a column of the result. A pattern is built from resources, literals and variables and the constructor refuses anything else with an `RDFQueryException`. Last, `ApplyToGraphAsync` is the call that runs the query: it returns an `RDFSelectQueryResult` whose `SelectResults` is a plain `DataTable`. The other forms of query and their results are the subject of [Query forms]({{ '/query/query-forms/' | relative_url }}).

The rows below this one are all printed by the same loop, so from here on only the tables are shown.

## Optional patterns

A pattern group drops every row for which one of its patterns finds nothing. An **optional** pattern changes that: the row stays and the variables that the optional pattern would have filled stay **unbound**. In SPARQL this is `OPTIONAL { ... }`. In C# it is `Optional()` on the pattern, a left join in database terms.

All six wines of the catalog carry the organic flag, so the difference would not show. The harvest of 2025 has started and the cooperative has registered a new wine, `vrd:novello-2025`, whose flag nobody has declared yet. It lives in a small graph of its own, joined to the catalog by `UnionWithAsync`, so that the catalog stays untouched.

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine ?organic
WHERE {
  ?wine vr:producedBy ?producer .
  OPTIONAL { ?wine vr:organic ?organic }
}
```

```csharp
RDFResource redWine = new RDFResource(vr + "RedWine");
RDFResource isOrganic = new RDFResource(vr + "organic");

RDFResource novello2025 = new RDFResource(vrd + "novello-2025");
RDFResource poggioAlto = new RDFResource(vrd + "poggio-alto");

RDFVariable organic = new RDFVariable("organic");

RDFGraph harvest = new RDFGraph()
    .AddTriple(new RDFTriple(novello2025, RDFVocabulary.RDF.TYPE, redWine))
    .AddTriple(new RDFTriple(novello2025, producedBy, poggioAlto));
RDFGraph catalogAndHarvest = await catalog.UnionWithAsync(harvest);

RDFSelectQuery organicFlags = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineProducedBy)
        .AddPattern(new RDFPattern(wine, isOrganic, organic).Optional()))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(organic);

RDFSelectQueryResult flags = await organicFlags.ApplyToGraphAsync(catalogAndHarvest);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?ORGANIC |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `false^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/riserva-2019` | `false^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/bianco-2023` | `true^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/vinsanto-2015` | `false^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/vermentino-2023` | `true^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/rosato-2024` | `false^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/novello-2025` |  |

</div>

The last row has an empty cell: in the `DataTable` it holds `DBNull`, the sign of an unbound variable. Without `Optional()` the new wine would not appear at all.

## Union and minus between patterns

Two operators combine alternatives. **Union** keeps the rows of both sides: it is the "or". **Minus** keeps the rows of the left side that have no counterpart on the right: it is the "except". Both are written as methods that build a tree. `Union` and `Minus` called on a pattern give an `RDFBinaryPatternGroupMember` and the group takes it with `AddBinaryPatternGroupMember`. Since the result is again a binary member, you can nest as deep as you need: `a.Union(b.Minus(c))`.

Reds and whites together, on the same variable:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine
WHERE {
  { ?wine a vr:RedWine }
  UNION
  { ?wine a vr:WhiteWine }
}
```

```csharp
RDFResource whiteWine = new RDFResource(vr + "WhiteWine");

RDFSelectQuery redsAndWhites = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddBinaryPatternGroupMember(
            new RDFPattern(wine, RDFVocabulary.RDF.TYPE, redWine)
                .Union(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, whiteWine))))
    .AddProjectionVariable(wine);

RDFSelectQueryResult both = await redsAndWhites.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` |
| `https://vallerosa.example/id/bianco-2023` |
| `https://vallerosa.example/id/vermentino-2023` |

</div>

The Vin Santo and the rosé are neither red nor white, so they are not in the list. Minus works the same way. The wines that are not organic are the wines that have a producer, except those flagged `true`:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine
WHERE {
  { ?wine vr:producedBy ?producer }
  MINUS
  { ?wine vr:organic true }
}
```

```csharp
RDFTypedLiteral organicTrue = new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);

RDFSelectQuery conventional = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddBinaryPatternGroupMember(
            wineProducedBy.Minus(new RDFPattern(wine, isOrganic, organicTrue))))
    .AddProjectionVariable(wine);

RDFSelectQueryResult notOrganic = await conventional.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` |
| `https://vallerosa.example/id/vinsanto-2015` |
| `https://vallerosa.example/id/rosato-2024` |

</div>

## Union and minus between pattern groups

When each side needs more than one pattern, or its own filters, combine whole pattern groups instead. The methods are the same, but they live on `RDFPatternGroup` and give an `RDFBinaryQueryMember`, which the query takes with `AddBinaryQueryMember`. Pattern groups can also be joined as optional, with `Optional()` on the group and the same operators work on subqueries, which are the subject of [their own page]({{ '/query/subqueries/' | relative_url }}).

The web page needs the names too, so each side carries two patterns:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?wine ?label
WHERE {
  { ?wine a vr:RedWine ; rdfs:label ?label }
  UNION
  { ?wine a vr:WhiteWine ; rdfs:label ?label }
}
```

```csharp
RDFPatternGroup reds = new RDFPatternGroup()
    .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, redWine))
    .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label));
RDFPatternGroup whites = new RDFPatternGroup()
    .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, whiteWine))
    .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label));

RDFSelectQuery redsAndWhitesNamed = new RDFSelectQuery()
    .AddBinaryQueryMember(reds.Union(whites))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(label);

RDFSelectQueryResult named = await redsAndWhitesNamed.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?LABEL |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `Rosso di Vallerosa@IT` |
| `https://vallerosa.example/id/rosso-2021` | `Vallerosa Red@EN` |
| `https://vallerosa.example/id/riserva-2019` | `Vallerosa Riserva@IT` |
| `https://vallerosa.example/id/riserva-2019` | `Vallerosa Reserve@EN` |
| `https://vallerosa.example/id/bianco-2023` | `Bianco delle Crete@IT` |
| `https://vallerosa.example/id/bianco-2023` | `Le Crete White@EN` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino della Piana@IT` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino of the Plain@EN` |

</div>

Every wine comes twice, once for each language of its label. [Filters]({{ '/query/filters/' | relative_url }}) are what keeps one of them.

## Patterns on a store

A store keeps quadruples, so a pattern on a store can say a fourth thing: **where** the triple was stated. The context goes in the first position of the constructor, ahead of the subject and it is either an `RDFContext` for a fixed source or a variable that the engine fills with the contexts it finds.

The store of this section is the one of the Store chapters: the catalog merged into the context `https://vallerosa.example/catalog`, plus the two guides of [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}). `ApplyToStoreAsync` is the counterpart of `ApplyToGraphAsync`.

```csharp
RDFResource hasScore = new RDFResource(vr + "score");

catalog.SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore catalogStore = new RDFMemoryStore();
await catalogStore.MergeGraphAsync(catalog);
RDFMemoryStore guides = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");
RDFMemoryStore store = await catalogStore.UnionWithAsync(guides);

RDFVariable guide = new RDFVariable("guide");
RDFVariable score = new RDFVariable("score");

RDFSelectQuery scoresByGuide = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(guide, wine, hasScore, score)))
    .AddProjectionVariable(guide)
    .AddProjectionVariable(wine)
    .AddProjectionVariable(score);

RDFSelectQueryResult guideScores = await scoresByGuide.ApplyToStoreAsync(store);
```

<div class="result" markdown="1">

| ?GUIDE | ?WINE | ?SCORE |
|---|---|---|
| `https://guidacalici.example/2025` | `https://vallerosa.example/id/rosso-2021` | `88^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://guidacalici.example/2025` | `https://vallerosa.example/id/riserva-2019` | `94^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://guidacalici.example/2025` | `https://vallerosa.example/id/bianco-2023` | `86^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://guidacalici.example/2025` | `https://vallerosa.example/id/vinsanto-2015` | `95^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://guidacalici.example/2025` | `https://vallerosa.example/id/vermentino-2023` | `89^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://winescope.example/2025` | `https://vallerosa.example/id/rosso-2021` | `90^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://winescope.example/2025` | `https://vallerosa.example/id/riserva-2019` | `92^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://winescope.example/2025` | `https://vallerosa.example/id/vinsanto-2015` | `96^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://winescope.example/2025` | `https://vallerosa.example/id/vermentino-2023` | `87^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://winescope.example/2025` | `https://vallerosa.example/id/rosato-2024` | `84^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

With a fixed context the pattern looks into that source only. Here is what WineScope alone says:

```csharp
RDFContext winescopeContext = new RDFContext("https://winescope.example/2025");

RDFSelectQuery winescopeScores = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(winescopeContext, wine, hasScore, score)))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(score);

RDFSelectQueryResult winescopeResult = await winescopeScores.ApplyToStoreAsync(store);
```

<div class="result" markdown="1">

| ?WINE | ?SCORE |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `90^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/riserva-2019` | `92^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vinsanto-2015` | `96^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vermentino-2023` | `87^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/rosato-2024` | `84^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

A pattern with no context at all, the three-position constructor, looks into every context of the store at once. This is why the same patterns work on a graph and on a store: on a store they read across all sources unless you pin one down. A SPARQL service behaves differently: there a pattern outside `GRAPH` sees only the default graph. Keep it in mind when the same query moves from a store to an [endpoint]({{ '/query/endpoints/' | relative_url }}).

> On a graph, use the three-position constructor: the graph is the only source there is. The four-position constructor is for stores, where the context is a real part of the question.
{: .tip}

## What comes next

You now know how to say what you look for. To keep only some of the rows, go to [Filters]({{ '/query/filters/' | relative_url }}). To order and cut them, go to [Modifiers]({{ '/query/modifiers/' | relative_url }}).
