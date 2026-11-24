---
title: Values
permalink: /query/values/
tagline: Handing the query a table of its own
lede: "Sometimes the list of things to look for is known before the query runs: three grapes, two producers, a few pairs. VALUES writes that list inside the query as a small table that joins with the rest."
---

> A partner shop wants the wines made from Sangiovese or Vermentino, the two grapes of its tasting night. The grapes come from the shop as a fixed list: nothing in the catalog says that these two belong together.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}), loaded as in [Variables and patterns]({{ '/query/patterns/' | relative_url }}).

## A table inside the query

The same job can be done with a filter, as in `FILTER (?grape IN (...))` of [Filters]({{ '/query/filters/' | relative_url }}) and for one column that is a fine choice. `VALUES` is different in kind: it is not a test but a set of rows that the query joins with the patterns, the way a table joins with another table. It reads better when the list is long, when it has several columns, or when it comes from the program that builds the query.

In C# the table is an `RDFValues`, built **column by column** with `AddColumn`: you give the variable and the list of its values, which are resources or literals. `RDFValues` goes into a pattern group with `AddValues`.

## The wines of two grapes

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?wine ?grape
WHERE {
  ?wine vr:grape ?grape .
  VALUES ?grape { vrd:sangiovese vrd:vermentino }
}
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFResource hasGrape = new RDFResource(vr + "grape");

RDFResource sangiovese = new RDFResource(vrd + "sangiovese");
RDFResource vermentino = new RDFResource(vrd + "vermentino");

RDFVariable wine = new RDFVariable("wine");
RDFVariable grape = new RDFVariable("grape");

RDFValues chosenGrapes = new RDFValues()
    .AddColumn(grape, new List<RDFPatternMember>
    {
        sangiovese,
        vermentino
    });

RDFSelectQuery byGrape = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasGrape, grape))
        .AddValues(chosenGrapes))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(grape);

RDFSelectQueryResult byGrapeResult = await byGrape.ApplyToGraphAsync(catalog);
foreach (DataRow row in byGrapeResult.SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?GRAPE"]}");
```

</div>

<div class="result" markdown="1">

| ?WINE | ?GRAPE |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `https://vallerosa.example/id/sangiovese` |
| `https://vallerosa.example/id/riserva-2019` | `https://vallerosa.example/id/sangiovese` |
| `https://vallerosa.example/id/vermentino-2023` | `https://vallerosa.example/id/vermentino` |
| `https://vallerosa.example/id/rosato-2024` | `https://vallerosa.example/id/sangiovese` |

</div>

Four wines: the Red, the Reserve and the rosé from Sangiovese and the Vermentino of the Plain. The Red is blended with Canaiolo, but its Canaiolo row is not in the table of values, so it does not come out. The `RDFValues` object is a plain value: you can keep it, change the list and use it again in other queries, here as `chosenGrapes`.

## Values at the end of the query

SPARQL also allows `VALUES` after the `WHERE` clause. There it joins with the result of the whole clause, before the modifiers run, rather than with one group. `SetValues` on the query does it and the same `chosenGrapes` serves again. Here the shop wants the English names, so the query is a little longer:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX vrd:  <https://vallerosa.example/id/>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?grape
WHERE {
  ?wine vr:grape ?grape ;
        rdfs:label ?label .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
VALUES ?grape { vrd:sangiovese vrd:vermentino }
```

```csharp
RDFPlainLiteral englishTag = new RDFPlainLiteral("en");

RDFVariable label = new RDFVariable("label");

RDFSelectQuery byGrapeAtTheEnd = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasGrape, grape))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag)))))
    .SetValues(chosenGrapes)
    .AddProjectionVariable(label)
    .AddProjectionVariable(grape);

RDFSelectQueryResult atTheEndResult = await byGrapeAtTheEnd.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?GRAPE |
|---|---|
| `Vallerosa Red@EN` | `https://vallerosa.example/id/sangiovese` |
| `Vallerosa Reserve@EN` | `https://vallerosa.example/id/sangiovese` |
| `Vermentino of the Plain@EN` | `https://vallerosa.example/id/vermentino` |
| `Valley Rosé@EN` | `https://vallerosa.example/id/sangiovese` |

</div>

The other query forms take a trailing table in the same way: `RDFAskQuery`, `RDFConstructQuery` and `RDFDescribeQuery` have `SetValues` too.

## Several columns

A table with more than one column ties the values together row by row. The shop is more demanding: Sangiovese only in a red wine, Vermentino only in a white one. Two columns with two rows each give two allowed pairs:

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?wine ?grape ?kind
WHERE {
  ?wine vr:grape ?grape ;
        a ?kind .
  VALUES (?grape ?kind) {
    (vrd:sangiovese vr:RedWine)
    (vrd:vermentino vr:WhiteWine)
  }
}
```

```csharp
RDFResource redWine = new RDFResource(vr + "RedWine");
RDFResource whiteWine = new RDFResource(vr + "WhiteWine");

RDFVariable kind = new RDFVariable("kind");

RDFValues pairs = new RDFValues()
    .AddColumn(grape, new List<RDFPatternMember>
    {
        sangiovese,
        vermentino
    })
    .AddColumn(kind, new List<RDFPatternMember>
    {
        redWine,
        whiteWine
    });

RDFSelectQuery byGrapeAndKind = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasGrape, grape))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, kind))
        .AddValues(pairs))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(grape)
    .AddProjectionVariable(kind);

RDFSelectQueryResult pairsResult = await byGrapeAndKind.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?GRAPE | ?KIND |
|---|---|---|
| `https://vallerosa.example/id/rosso-2021` | `https://vallerosa.example/id/sangiovese` | `https://vallerosa.example/vocab#RedWine` |
| `https://vallerosa.example/id/riserva-2019` | `https://vallerosa.example/id/sangiovese` | `https://vallerosa.example/vocab#RedWine` |
| `https://vallerosa.example/id/vermentino-2023` | `https://vallerosa.example/id/vermentino` | `https://vallerosa.example/vocab#WhiteWine` |

</div>

The rosé is gone: it is made from Sangiovese but it is a rosé and the pair `(vrd:sangiovese, vr:RoseWine)` is not in the table.

Next, the expressions that compute new values from the ones the query already has: [Bind and expressions]({{ '/query/expressions/' | relative_url }}).
