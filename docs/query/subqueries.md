---
title: Subqueries
permalink: /query/subqueries/
tagline: A query inside a query
lede: "Some questions have two steps: first compute something, then use it. A subquery is a full SELECT that runs first and hands its columns to the query around it."
---

> Which wine does each producer take pride in? The cooperative wants the best-scored wine of every winery for its brochure. One step finds the top score of each producer, the other finds the wine that earned it.
{: .story}

This page runs on a store that holds the catalog and the two guides: the catalog merged into the context `https://vallerosa.example/catalog`, joined with [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}), as in [Memory stores]({{ '/store/stores/' | relative_url }}). The scores live in the guides and the producers in the catalog, so only a store can answer.

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFGraph catalog = (await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl"))
    .SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore catalogStore = new RDFMemoryStore();
await catalogStore.MergeGraphAsync(catalog);
RDFMemoryStore guides = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");
RDFMemoryStore store = await catalogStore.UnionWithAsync(guides);
```

## Why a second query

The top score of a producer is an aggregate: the maximum of the scores of all its wines, which [Aggregates]({{ '/query/aggregates/' | relative_url }}) explains. An aggregate collapses rows, so once the maximum is computed, the wine that earned it is gone from the table. To get both, you compute the maximum in a query of its own and then join its result with the wines. That is a subquery.

A subquery is an `RDFSelectQuery` like any other, with its patterns, filters, modifiers and projection. You add it to the outer query with `AddSubQuery`. Two rules hold:
- only a SELECT can be a subquery
- the subquery hands up to its parent the variables that it projects and only those: the others stay inside

## The best wine of each producer

The inner query asks for the top score of each producer. It uses its own variables, `anyWine` and `anyScore`, since the wine of the outer query is another thing. It projects `producer` and `score`, the two names that the outer query will join on.

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?producer ?wine ?score
WHERE {
  ?wine vr:producedBy ?producer ;
        vr:score ?score .
  {
    SELECT ?producer (MAX(?anyScore) AS ?score)
    WHERE {
      ?anyWine vr:producedBy ?producer ;
               vr:score ?anyScore .
    }
    GROUP BY ?producer
  }
}
```

```csharp
RDFResource producedBy = new RDFResource(vr + "producedBy");
RDFResource hasScore = new RDFResource(vr + "score");

RDFVariable wine = new RDFVariable("wine");
RDFVariable producer = new RDFVariable("producer");
RDFVariable score = new RDFVariable("score");
RDFVariable anyWine = new RDFVariable("anyWine");
RDFVariable anyScore = new RDFVariable("anyScore");

RDFSelectQuery topScores = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(anyWine, producedBy, producer))
        .AddPattern(new RDFPattern(anyWine, hasScore, anyScore)))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { producer })
        .AddAggregator(new RDFMaxAggregator(anyScore, score, RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)))
    .AddProjectionVariable(producer)
    .AddProjectionVariable(score);

RDFSelectQuery bestWines = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer))
        .AddPattern(new RDFPattern(wine, hasScore, score)))
    .AddSubQuery(topScores)
    .AddProjectionVariable(producer)
    .AddProjectionVariable(wine)
    .AddProjectionVariable(score);

RDFSelectQueryResult best = await bestWines.ApplyToStoreAsync(store);
foreach (DataRow row in best.SelectResults.Rows)
    Console.WriteLine($"{row["?PRODUCER"]} | {row["?WINE"]} | {row["?SCORE"]}");
```

</div>

<div class="result" markdown="1">

| ?PRODUCER | ?WINE | ?SCORE |
|---|---|---|
| `https://vallerosa.example/id/poggio-alto` | `https://vallerosa.example/id/riserva-2019` | `94^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/le-crete` | `https://vallerosa.example/id/vinsanto-2015` | `96^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/tre-ulivi` | `https://vallerosa.example/id/vermentino-2023` | `89^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

Read it from the inside out. The inner query yields three rows, one for each producer:

| ?PRODUCER | ?SCORE |
|---|---|
| `https://vallerosa.example/id/poggio-alto` | `94^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/le-crete` | `96^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/tre-ulivi` | `89^^http://www.w3.org/2001/XMLSchema#integer` |

The outer patterns list every wine with each score that a guide gave it and the join keeps the rows where the producer and the score are the ones of the inner table. The Vallerosa Red, with its 88 and its 90, is out: its best is below the 94 of the Reserve. The Valley Vin Santo wins for Tenuta Le Crete with the 96 of WineScope.

The `RDFMaxAggregator` is in its `Numeric` flavor, which compares the scores as numbers. The other aggregators and the HAVING clause are in [Aggregates]({{ '/query/aggregates/' | relative_url }}).

## An optional subquery

A subquery joins like a pattern group does, so it can be optional too. `Optional()` on the subquery keeps the rows of the outer query that find no match, with the projected variables left unbound. The wines with the scores of 90 or above, for the wines that have one:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine ?acclaimed
WHERE {
  ?wine vr:producedBy ?producer .
  OPTIONAL {
    SELECT ?wine ?acclaimed
    WHERE {
      ?wine vr:score ?acclaimed .
      FILTER (?acclaimed >= 90)
    }
  }
}
```

```csharp
RDFTypedLiteral acclaimThreshold = new RDFTypedLiteral("90", RDFModelEnums.RDFDatatypes.XSD_INTEGER);

RDFVariable acclaimed = new RDFVariable("acclaimed");

RDFSelectQuery ninetyPlus = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasScore, acclaimed))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
            acclaimed,
            new RDFConstantExpression(acclaimThreshold)))))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(acclaimed)
    .Optional();

RDFSelectQuery winesWithAcclaim = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer)))
    .AddSubQuery(ninetyPlus)
    .AddProjectionVariable(wine)
    .AddProjectionVariable(acclaimed);

RDFSelectQueryResult acclaimedResult = await winesWithAcclaim.ApplyToStoreAsync(store);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?ACCLAIMED |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `90^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/riserva-2019` | `94^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/riserva-2019` | `92^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/bianco-2023` |  |
| `https://vallerosa.example/id/vinsanto-2015` | `95^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vinsanto-2015` | `96^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vermentino-2023` |  |
| `https://vallerosa.example/id/rosato-2024` |  |

</div>

The white blend, the Vermentino and the rosé have no score of 90 or above, so their second column stays empty. The Reserve and the Vin Santo appear twice, once for each guide that rates them at 90 or more.

## Subqueries and the other members

Where a subquery can sit is wider than where it is added:
- `AddSubQuery` is on every query form, so an ASK, a CONSTRUCT or a DESCRIBE can have subqueries too
- `Union` and `Minus` work between a subquery and a pattern group, another subquery or another operator tree; the result goes to the query with `AddBinaryQueryMember`, as shown in [Variables and patterns]({{ '/query/patterns/' | relative_url }})
- a subquery can have subqueries of its own, to any depth
- a subquery can be given as the argument of `RDFExistsExpression`, in place of a pattern group, to use it as the test of a [filter]({{ '/query/filters/' | relative_url }})

The next page takes up what the inner query used: [Aggregates]({{ '/query/aggregates/' | relative_url }}).
