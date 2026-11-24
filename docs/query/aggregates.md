---
title: Aggregates
permalink: /query/aggregates/
tagline: Counting, summing and averaging groups of rows
lede: "An aggregate turns many rows into one number: how many, how much, the average, the highest. Grouping decides which rows go together. Having keeps only the groups worth reading."
---

> The cooperative closes its books: how many wines each producer makes, how well each subzone is scored on average, how many bottles each partner shop took in 2025.
{: .story}

Three data sources are in play, each said at its example. The count of wines runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}). The scores run on the store of the Store chapters, the catalog merged into the context `https://vallerosa.example/catalog` with [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}). The bottles run on [vallerosa-sales.ttl]({{ '/assets/data/vallerosa-sales.ttl' | relative_url }}).

## Grouping and aggregators

`RDFGroupByModifier` splits the rows in **groups** that share the same values for the variables you name and applies **aggregators** to each group. The result has one row per group: the grouping variables plus one column for each aggregator. You add the modifier with `AddModifier`, the aggregators with `AddAggregator` and every column that you want in the answer must be projected.

An aggregator takes the variable to aggregate and the variable that will hold the result: `new RDFCountAggregator(wine, wines)` is `COUNT(?wine) AS ?wines`.

| Class | SPARQL | What it gives |
|---|---|---|
| `RDFCountAggregator` | `COUNT(?x)` | how many bound values; with only the result variable it is `COUNT(*)` |
| `RDFSumAggregator` | `SUM(?x)` | the sum of the numeric values |
| `RDFAvgAggregator` | `AVG(?x)` | the average of the numeric values |
| `RDFMinAggregator` | `MIN(?x)` | the lowest value, as a number or as a string according to its flavor |
| `RDFMaxAggregator` | `MAX(?x)` | the highest value, with the same two flavors |
| `RDFSampleAggregator` | `SAMPLE(?x)` | one value of the group, any |
| `RDFGroupConcatAggregator` | `GROUP_CONCAT(?x; SEPARATOR="...")` | all the values joined in one string with the separator |

The `Distinct()` method on an aggregator counts or sums every value once, as in `COUNT(DISTINCT ?x)`. Without a list of grouping variables, `new RDFGroupByModifier()` puts every row in a single group, so the aggregators compute over the whole result.

## How many wines for each producer

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?producer (COUNT(?wine) AS ?wines)
WHERE {
  ?wine vr:producedBy ?producer .
}
GROUP BY ?producer
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFResource producedBy = new RDFResource(vr + "producedBy");

RDFVariable wine = new RDFVariable("wine");
RDFVariable producer = new RDFVariable("producer");
RDFVariable wines = new RDFVariable("wines");

RDFSelectQuery winesPerProducer = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer)))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { producer })
        .AddAggregator(new RDFCountAggregator(wine, wines)))
    .AddProjectionVariable(producer)
    .AddProjectionVariable(wines);

RDFSelectQueryResult counted = await winesPerProducer.ApplyToGraphAsync(catalog);
foreach (DataRow row in counted.SelectResults.Rows)
    Console.WriteLine($"{row["?PRODUCER"]} | {row["?WINES"]}");
```

</div>

<div class="result" markdown="1">

| ?PRODUCER | ?WINES |
|---|---|
| `https://vallerosa.example/id/poggio-alto` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/le-crete` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/tre-ulivi` | `2^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

Each producer makes two wines. The count comes out as an `xsd:integer`, the sum of integers is an integer too and the average is a decimal.

## The average score of each subzone

The average crosses three kinds of facts. The producer of a wine and the subzone of a producer are in the catalog, the score of a wine is in a guide. A store holds them side by side and a pattern with no context reads them all. Each subzone is the group and the average runs on every score that a guide gave to the wines made there:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?subzone (AVG(?score) AS ?averageScore)
WHERE {
  ?wine vr:producedBy ?producer ;
        vr:score ?score .
  ?producer vr:locatedIn ?subzone .
}
GROUP BY ?subzone
```

```csharp
RDFResource locatedIn = new RDFResource(vr + "locatedIn");
RDFResource hasScore = new RDFResource(vr + "score");

catalog.SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore catalogStore = new RDFMemoryStore();
await catalogStore.MergeGraphAsync(catalog);
RDFMemoryStore guides = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");
RDFMemoryStore store = await catalogStore.UnionWithAsync(guides);

RDFVariable subzone = new RDFVariable("subzone");
RDFVariable score = new RDFVariable("score");
RDFVariable averageScore = new RDFVariable("averageScore");

RDFSelectQuery scorePerSubzone = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer))
        .AddPattern(new RDFPattern(producer, locatedIn, subzone))
        .AddPattern(new RDFPattern(wine, hasScore, score)))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { subzone })
        .AddAggregator(new RDFAvgAggregator(score, averageScore)))
    .AddProjectionVariable(subzone)
    .AddProjectionVariable(averageScore);

RDFSelectQueryResult averaged = await scorePerSubzone.ApplyToStoreAsync(store);
```

</div>

<div class="result" markdown="1">

| ?SUBZONE | ?AVERAGESCORE |
|---|---|
| `https://vallerosa.example/id/poggio` | `91.0^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/colle-nord` | `92.33333333333333333333333333^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/piana` | `86.66666666666666666666666667^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The Hilltop averages four scores, 88 and 90 for the Red and 94 and 92 for the Reserve. The North Hill averages three: the 86 of the white blend and the 95 and 96 of the Vin Santo. The Plain averages three as well: 89 and 87 for the Vermentino and the single 84 of WineScope for the rosé.

## Bottles for each shop

The ledger has one `vr:Sale` for each delivery. Two aggregators on the same group give the bottles that each shop took and how many deliveries it took them in:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?shop (SUM(?bottles) AS ?totalBottles)
             (COUNT(?sale) AS ?deliveries)
WHERE {
  ?sale vr:soldTo ?shop ;
        vr:bottles ?bottles .
}
GROUP BY ?shop
```

```csharp
RDFGraph sales = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-sales.ttl");

RDFResource soldTo = new RDFResource(vr + "soldTo");
RDFResource hasBottles = new RDFResource(vr + "bottles");

RDFVariable sale = new RDFVariable("sale");
RDFVariable shop = new RDFVariable("shop");
RDFVariable bottles = new RDFVariable("bottles");
RDFVariable totalBottles = new RDFVariable("totalBottles");
RDFVariable deliveries = new RDFVariable("deliveries");

RDFSelectQuery bottlesPerShop = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(sale, soldTo, shop))
        .AddPattern(new RDFPattern(sale, hasBottles, bottles)))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { shop })
        .AddAggregator(new RDFSumAggregator(bottles, totalBottles))
        .AddAggregator(new RDFCountAggregator(sale, deliveries)))
    .AddProjectionVariable(shop)
    .AddProjectionVariable(totalBottles)
    .AddProjectionVariable(deliveries);

RDFSelectQueryResult perShop = await bottlesPerShop.ApplyToGraphAsync(sales);
```

</div>

<div class="result" markdown="1">

| ?SHOP | ?TOTALBOTTLES | ?DELIVERIES |
|---|---|---|
| `https://vallerosa.example/id/enoteca-del-campo` | `180^^http://www.w3.org/2001/XMLSchema#integer` | `3^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/bottega-arno` | `198^^http://www.w3.org/2001/XMLSchema#integer` | `3^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/cantinetta-torre` | `132^^http://www.w3.org/2001/XMLSchema#integer` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/due-torri` | `72^^http://www.w3.org/2001/XMLSchema#integer` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/sette-colli` | `42^^http://www.w3.org/2001/XMLSchema#integer` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/navigli` | `42^^http://www.w3.org/2001/XMLSchema#integer` | `2^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

The totals are the ones of [Meet Vallerosa]({{ '/vallerosa/' | relative_url }}). The Bottega Arno is the best customer with 198 bottles.

## Keeping only some groups

`HAVING` is to groups what a filter is to rows: it runs after the aggregation and can look at the aggregated values. `SetHavingExpression` on the modifier takes a boolean expression in which the variable of an aggregator stands for its result. The shops that took more than 100 bottles:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?shop (SUM(?bottles) AS ?totalBottles)
WHERE {
  ?sale vr:soldTo ?shop ;
        vr:bottles ?bottles .
}
GROUP BY ?shop
HAVING (SUM(?bottles) > 100)
```

```csharp
RDFTypedLiteral bottlesThreshold = new RDFTypedLiteral("100", RDFModelEnums.RDFDatatypes.XSD_INTEGER);

RDFSelectQuery bigShops = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(sale, soldTo, shop))
        .AddPattern(new RDFPattern(sale, hasBottles, bottles)))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { shop })
        .AddAggregator(new RDFSumAggregator(bottles, totalBottles))
        .SetHavingExpression(new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
            totalBottles,
            new RDFConstantExpression(bottlesThreshold))))
    .AddProjectionVariable(shop)
    .AddProjectionVariable(totalBottles);

RDFSelectQueryResult bigShopsResult = await bigShops.ApplyToGraphAsync(sales);
```

</div>

<div class="result" markdown="1">

| ?SHOP | ?TOTALBOTTLES |
|---|---|
| `https://vallerosa.example/id/enoteca-del-campo` | `180^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/bottega-arno` | `198^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/cantinetta-torre` | `132^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

## The rest of the family

Minimum, maximum, a sample and a concatenation work as the others. The two flavors of `RDFMinAggregator` and `RDFMaxAggregator`, `Numeric` and `String`, say how the values are compared: use `Numeric` for prices and scores, where 9 is less than 10. Use `String` for names. The cheapest and the dearest wine of each producer, one label picked from the group and all of them in one string:

```csharp
RDFResource hasPrice = new RDFResource(vr + "price");

RDFPlainLiteral englishTag = new RDFPlainLiteral("en");

RDFVariable price = new RDFVariable("price");
RDFVariable label = new RDFVariable("label");
RDFVariable cheapest = new RDFVariable("cheapest");
RDFVariable dearest = new RDFVariable("dearest");
RDFVariable anyLabel = new RDFVariable("anyLabel");
RDFVariable allLabels = new RDFVariable("allLabels");

RDFSelectQuery producerSummary = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer))
        .AddPattern(new RDFPattern(wine, hasPrice, price))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag)))))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { producer })
        .AddAggregator(new RDFMinAggregator(price, cheapest, RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric))
        .AddAggregator(new RDFMaxAggregator(price, dearest, RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric))
        .AddAggregator(new RDFSampleAggregator(label, anyLabel))
        .AddAggregator(new RDFGroupConcatAggregator(label, allLabels, " / ")))
    .AddProjectionVariable(producer)
    .AddProjectionVariable(cheapest)
    .AddProjectionVariable(dearest)
    .AddProjectionVariable(anyLabel)
    .AddProjectionVariable(allLabels);

RDFSelectQueryResult summary = await producerSummary.ApplyToGraphAsync(catalog);
```

<div class="result" markdown="1">

| ?PRODUCER | ?CHEAPEST | ?DEAREST | ?ANYLABEL | ?ALLLABELS |
|---|---|---|---|---|
| `https://vallerosa.example/id/poggio-alto` | `14.50^^http://www.w3.org/2001/XMLSchema#decimal` | `32.00^^http://www.w3.org/2001/XMLSchema#decimal` | `Vallerosa Red@EN` | `Vallerosa Red@EN / Vallerosa Reserve@EN` |
| `https://vallerosa.example/id/le-crete` | `11.00^^http://www.w3.org/2001/XMLSchema#decimal` | `38.00^^http://www.w3.org/2001/XMLSchema#decimal` | `Le Crete White@EN` | `Le Crete White@EN / Valley Vin Santo@EN` |
| `https://vallerosa.example/id/tre-ulivi` | `10.00^^http://www.w3.org/2001/XMLSchema#decimal` | `13.00^^http://www.w3.org/2001/XMLSchema#decimal` | `Vermentino of the Plain@EN` | `Vermentino of the Plain@EN / Valley Rosé@EN` |

</div>

`SAMPLE` returns one value of each group and the language leaves open which one, so do not rely on it. A last count, without a grouping variable, runs over the whole ledger. It asks how many shops and how many different wines the 14 sales involve, each counted once with `Distinct()`:

```csharp
RDFResource hasSoldWine = new RDFResource(vr + "soldWine");

RDFVariable soldWine = new RDFVariable("soldWine");
RDFVariable shopCount = new RDFVariable("shopCount");
RDFVariable wineKinds = new RDFVariable("wineKinds");

RDFSelectQuery overall = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(sale, soldTo, shop))
        .AddPattern(new RDFPattern(sale, hasSoldWine, soldWine)))
    .AddModifier(new RDFGroupByModifier()
        .AddAggregator(new RDFCountAggregator(shop, shopCount).Distinct())
        .AddAggregator(new RDFCountAggregator(soldWine, wineKinds).Distinct()))
    .AddProjectionVariable(shopCount)
    .AddProjectionVariable(wineKinds);
RDFSelectQueryResult totals = await overall.ApplyToGraphAsync(sales);
```

<div class="result" markdown="1">

| ?SHOPCOUNT | ?WINEKINDS |
|---|---|
| `6^^http://www.w3.org/2001/XMLSchema#integer` | `6^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

An aggregate can also work on an expression instead of a variable: every aggregator has a constructor that takes an `RDFExpression`, as in `SUM(?price * ?bottles)`. The expressions are the subject of [Bind and expressions]({{ '/query/expressions/' | relative_url }}). To use an aggregate inside another query, put the query in a [subquery]({{ '/query/subqueries/' | relative_url }}).
