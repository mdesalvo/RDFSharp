---
title: Tips and tricks
permalink: /query/tips/
tagline: How the engine works and how to help it
lede: The engine is straightforward once you know a few of its habits. Here is what it reorders by itself, what it cannot guess from your data and how to keep your queries light and readable.
---

> The cooperative's questions have grown and one of them misbehaves: an average of prices that comes out empty. It is not a bug in the query. It comes from a habit of the engine that is worth knowing, like the others on this page.
{: .story}

This page works on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}), loaded as in [Getting started]({{ '/getting-started/' | relative_url }}).

```csharp
using System.Data;
using RDFSharp.Model;   // resources, literals, graphs
using RDFSharp.Query;   // variables, patterns, queries

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");
RDFVariable price = new RDFVariable("price");
RDFVariable producer = new RDFVariable("producer");

RDFResource rosso = new RDFResource(vrd + "rosso-2021");
RDFResource producedBy = new RDFResource(vr + "producedBy");
```

## The order of your patterns

The engine answers a pattern group by evaluating its patterns and joining the results. The order of the joins decides how big the intermediate tables become. A bad order can multiply rows that a good one never creates. On a graph or on a memory store you do not have to care: before the evaluation the optimizer reorders the patterns by itself. It follows three rules, in this order:

1. patterns that share a variable with the ones already placed come first, so that no join multiplies every row by every row
2. among those, the pattern that brings in fewer new variables comes first, because it behaves like a filter
3. on a tie, the pattern with fewer matching triples comes first, estimated from the indexes of the data source

A pattern that begins with a constant, such as one naming `vrd:poggio-alto`, is usually the most selective one and ends up first whatever its position in your query. The reordering never changes the answer: it changes only the way the engine gets there.

```csharp
RDFPattern wineLabel = new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label);
RDFPattern winePrice = new RDFPattern(wine, new RDFResource(vr + "price"), price);
RDFPattern wineOfPoggio = new RDFPattern(wine, new RDFResource(vr + "producedBy"), new RDFResource(vrd + "poggio-alto"));

RDFSelectQuery broadFirst = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(winePrice)
        .AddPattern(wineLabel)
        .AddPattern(wineOfPoggio))
    .AddProjectionVariable(wine);
RDFSelectQuery narrowFirst = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineOfPoggio)
        .AddPattern(winePrice)
        .AddPattern(wineLabel))
    .AddProjectionVariable(wine);

Console.WriteLine(broadFirst.ApplyToGraph(catalog).SelectResultsCount);
Console.WriteLine(narrowFirst.ApplyToGraph(catalog).SelectResultsCount);
```

<div class="result" markdown="1">

```
4
4
```

</div>

Two wines of Poggio Alto with two labels each: four rows in both orders. The optimizer has limits that you should know:

- It reorders only consecutive inner-join patterns. An `Optional()` pattern, a union or a minus, a `BIND`, a `VALUES` and a property path act as barriers: they keep their place and the patterns before and after them are reordered separately.
- It acts only when the data source is an `RDFGraph` or an `RDFMemoryStore`. On a [federation]({{ '/query/federations/' | relative_url }}) and on a [SPARQL endpoint]({{ '/query/endpoints/' | relative_url }}) the patterns run in the order you wrote them.

So on a federation or an endpoint the order is yours to choose: write first the patterns with constants and then the ones that connect to them. The habit costs nothing on a graph either.

## Project only what you need

A `SELECT` without projection variables behaves like `SELECT *`: every variable that takes part in the patterns becomes a column of the result.

```csharp
RDFSelectQuery withoutProjection = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineLabel)
        .AddPattern(new RDFPattern(wine, producedBy, producer)));
RDFSelectQueryResult wide = withoutProjection.ApplyToGraph(catalog);
foreach (DataColumn column in wide.SelectResults.Columns)
    Console.WriteLine(column.ColumnName);
```

<div class="result" markdown="1">

```
?WINE
?PRODUCER
?LABEL
```

</div>

Adding the projection of the one variable you want leaves a table with one column:

```csharp
RDFSelectQuery withProjection = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineLabel)
        .AddPattern(new RDFPattern(wine, producedBy, producer)))
    .AddProjectionVariable(label);
RDFSelectQueryResult narrow = withProjection.ApplyToGraph(catalog);
foreach (DataColumn column in narrow.SelectResults.Columns)
    Console.WriteLine(column.ColumnName);
```

<div class="result" markdown="1">

```
?LABEL
```

</div>

At the top level of a query the projection is applied after the joins, so it makes the result smaller and does not make the joins cheaper. It matters most in a subquery, which hands to the outer query only the columns it projects. Here the inner query uses a price pattern to keep the wines that have one and projects only `?wine`: the outer query receives a single column to join with the labels.

```csharp
RDFSelectQuery poggioWinesWithPrice = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineOfPoggio)
        .AddPattern(winePrice))
    .AddProjectionVariable(wine);
RDFSelectQuery labelledPoggioWines = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineLabel))
    .AddSubQuery(poggioWinesWithPrice);
RDFSelectQueryResult labelled = labelledPoggioWines.ApplyToGraph(catalog);
foreach (DataColumn column in labelled.SelectResults.Columns)
    Console.WriteLine(column.ColumnName);
```

<div class="result" markdown="1">

```
?WINE
?LABEL
```

</div>

The price never reaches the outer table. See [Subqueries]({{ '/query/subqueries/' | relative_url }}) for the rest of the story.

## Avoid the pattern that matches everything

`?s ?p ?o` has a variable in every position, so every triple of the data source matches it. On the catalog these are 143 rows. On a larger source they are all its triples.

```csharp
RDFSelectQuery everything = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))));
Console.WriteLine(everything);
Console.WriteLine(everything.ApplyToGraph(catalog).SelectResultsCount);
```

<div class="result" markdown="1">

```
SELECT *
WHERE {
  {
    ?S ?P ?O .
  }
}

143
```

</div>

On a federation the damage grows, because every pattern is asked of every member on its own, as the [federations]({{ '/query/federations/' | relative_url }}) page explains. The query above would send exactly this text to each endpoint of the federation and wait for all the triples that it holds. Give every pattern a constant in at least one position and pair a poorly selective pattern with more selective ones.

## Type your literals

The engine computes with the datatype of a literal. A price written as a typed `xsd:decimal` can be averaged and compared. A price written as a plain literal is text and arithmetic on text finds nothing to work with. A partner shop has returned the prices of three wines as plain text. The catalog holds them as decimals:

```csharp
RDFGraph importedPrices = new RDFGraph();
importedPrices.AddTriple(new RDFTriple(rosso, new RDFResource(vr + "price"), new RDFPlainLiteral("14.50")));
importedPrices.AddTriple(new RDFTriple(new RDFResource(vrd + "riserva-2019"), new RDFResource(vr + "price"), new RDFPlainLiteral("32.00")));
importedPrices.AddTriple(new RDFTriple(new RDFResource(vrd + "bianco-2023"), new RDFResource(vr + "price"), new RDFPlainLiteral("11.00")));

RDFVariable average = new RDFVariable("average");
RDFSelectQuery averagePrice = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPattern(winePrice))
    .AddModifier(new RDFGroupByModifier().AddAggregator(new RDFAvgAggregator(price, average)));

Console.WriteLine("typed: " + averagePrice.ApplyToGraph(catalog).SelectResults.Rows[0]["?AVERAGE"]);
Console.WriteLine("plain: " + averagePrice.ApplyToGraph(importedPrices).SelectResults.Rows[0]["?AVERAGE"]);
```

<div class="result" markdown="1">

```
typed: 19.75^^http://www.w3.org/2001/XMLSchema#decimal
plain: 
```

</div>

The average of the plain prices is empty, with no error to tell you why. The same happens with comparisons and with computed values: a filter for prices below 13 keeps two wines of the catalog and none of the import. The price with VAT of an imported wine is unbound.

```csharp
RDFSelectQuery under13 = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(winePrice)
        .AddFilter(new RDFFilter(new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.LessThan, price,
            new RDFConstantExpression(new RDFTypedLiteral("13", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))))))
    .AddProjectionVariable(wine);
Console.WriteLine("typed: " + under13.ApplyToGraph(catalog).SelectResultsCount);
Console.WriteLine("plain: " + under13.ApplyToGraph(importedPrices).SelectResultsCount);

RDFVariable withVat = new RDFVariable("withVat");
RDFSelectQuery priceWithVat = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(winePrice)
        .AddBind(new RDFBind(new RDFMultiplyExpression(price,
            new RDFTypedLiteral("1.22", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)), withVat)))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(withVat);
foreach (DataRow row in priceWithVat.ApplyToGraph(importedPrices).SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?WITHVAT"]}");
```

<div class="result" markdown="1">

```
typed: 2
plain: 0
https://vallerosa.example/id/rosso-2021 | 
https://vallerosa.example/id/riserva-2019 | 
https://vallerosa.example/id/bianco-2023 | 
```

</div>

The rule has two halves. Use typed literals for what you will count, compare, add or measure: prices, years, alcohol degrees, coordinates. Use plain literals for text that you will search, match with a regular expression or show: labels and descriptions. When data comes from outside, check its datatypes before you trust an aggregate over it: the [SHACL validation]({{ '/model/shacl-validation/' | relative_url }}) of the Model chapter can do it for you.

## Readable queries

Prefixes do not change what a query means or how fast it runs, only how it reads when you print it. Add the namespaces you use with `AddPrefix` and the printed query shortens its IRIs:

```csharp
RDFSelectQuery prefixed = new RDFSelectQuery()
    .AddPrefix(vr)
    .AddPrefix(vrd)
    .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(wineLabel)
        .AddPattern(wineOfPoggio))
    .AddProjectionVariable(label);
Console.WriteLine(prefixed);
```

<div class="result" markdown="1">

```
PREFIX vr: <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?LABEL
WHERE {
  {
    ?WINE rdfs:label ?LABEL .
    ?WINE vr:producedBy vrd:poggio-alto .
  }
}
```

</div>

The prefixes of the [namespace register]({{ '/model/namespaces/' | relative_url }}) such as `rdfs` are one call away. Your own come from the `RDFNamespace` objects you already have.
