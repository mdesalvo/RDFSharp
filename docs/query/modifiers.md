---
title: Modifiers
permalink: /query/modifiers/
tagline: Sorting, slicing and deduplicating the answer
lede: "Patterns and filters decide which rows are in the answer. Modifiers decide how the answer is presented: in what order, how many rows, starting from where and without repetitions."
---

> The cooperative prints a price list for the partner shops: every wine from the cheapest to the dearest. The sales team also wants the three most expensive wines at a glance.
{: .story}

Every example on this page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}), loaded as in [Variables and patterns]({{ '/query/patterns/' | relative_url }}). The wines are shown by their English label, so all the queries share the same pattern group:

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFResource hasPrice = new RDFResource(vr + "price");

RDFPlainLiteral englishTag = new RDFPlainLiteral("en");

RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");
RDFVariable price = new RDFVariable("price");
RDFVariable alcohol = new RDFVariable("alcohol");
RDFVariable producer = new RDFVariable("producer");

RDFPatternGroup priceList = new RDFPatternGroup()
    .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
    .AddPattern(new RDFPattern(wine, hasPrice, price))
    .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
        new RDFConstantExpression(englishTag))));
```

## What a modifier is

A modifier works on the table of rows that the patterns and filters have produced. The four of this page do not change what the rows say, only which ones are kept and in what order (grouping, which does, has [its own page]({{ '/query/aggregates/' | relative_url }})). You add them to the query with `AddModifier`, after the pattern groups. The query applies them in the order that SPARQL prescribes, whatever the order of the calls: sort first, then deduplicate, then skip, then keep. The order of the calls matters only between two sorts, as the next sections show.

| SPARQL | Class | What it does | How many per query |
|---|---|---|---|
| `ORDER BY` | `RDFOrderByModifier` | sorts the rows on a variable or on an expression | as many as you need |
| `LIMIT` | `RDFLimitModifier` | keeps the first rows only | one |
| `OFFSET` | `RDFOffsetModifier` | skips the first rows | one |
| `DISTINCT` | `RDFDistinctModifier` | removes the repeated rows | one |
| `GROUP BY` | `RDFGroupByModifier` | partitions the rows and computes aggregates: see [Aggregates]({{ '/query/aggregates/' | relative_url }}) | one |

## Sorting

`RDFOrderByModifier` takes the variable to sort on and a direction from `RDFQueryEnums.RDFOrderByFlavors`: `ASC` or `DESC`. The price list, from the cheapest wine:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?price
WHERE {
  ?wine rdfs:label ?label ;
        vr:price ?price .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
ORDER BY ASC(?price)
```

```csharp
RDFSelectQuery cheapestFirst = new RDFSelectQuery()
    .AddPatternGroup(priceList)
    .AddModifier(new RDFOrderByModifier(price, RDFQueryEnums.RDFOrderByFlavors.ASC))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price);

RDFSelectQueryResult cheapest = await cheapestFirst.ApplyToGraphAsync(catalog);
foreach (DataRow row in cheapest.SelectResults.Rows)
    Console.WriteLine($"{row["?LABEL"]} | {row["?PRICE"]}");
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?PRICE |
|---|---|
| `Valley Rosé@EN` | `10.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Le Crete White@EN` | `11.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vermentino of the Plain@EN` | `13.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Red@EN` | `14.50^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Reserve@EN` | `32.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Valley Vin Santo@EN` | `38.00^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

Swap `ASC` for `DESC` and the list reads from the dearest. The sort is on the value of the variable, so it is also the place where the datatype matters: the prices are `xsd:decimal` and you expect them in numeric order.

### More than one key

Add one `RDFOrderByModifier` for each key. The first one added is the main key, the next ones only break the ties. The wines from the lightest to the strongest and the dearest first among those that are equally strong:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?alcohol ?price
WHERE {
  ?wine rdfs:label ?label ;
        vr:alcohol ?alcohol ;
        vr:price ?price .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
ORDER BY ASC(?alcohol) DESC(?price)
```

```csharp
RDFResource hasAlcohol = new RDFResource(vr + "alcohol");

RDFSelectQuery byStrength = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, hasAlcohol, alcohol))
        .AddPattern(new RDFPattern(wine, hasPrice, price))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag)))))
    .AddModifier(new RDFOrderByModifier(alcohol, RDFQueryEnums.RDFOrderByFlavors.ASC))
    .AddModifier(new RDFOrderByModifier(price, RDFQueryEnums.RDFOrderByFlavors.DESC))
    .AddProjectionVariable(label)
    .AddProjectionVariable(alcohol)
    .AddProjectionVariable(price);

RDFSelectQueryResult byStrengthResult = await byStrength.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?ALCOHOL | ?PRICE |
|---|---|---|
| `Le Crete White@EN` | `12.0^^http://www.w3.org/2001/XMLSchema#decimal` | `11.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Valley Rosé@EN` | `12.0^^http://www.w3.org/2001/XMLSchema#decimal` | `10.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vermentino of the Plain@EN` | `12.5^^http://www.w3.org/2001/XMLSchema#decimal` | `13.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Red@EN` | `13.5^^http://www.w3.org/2001/XMLSchema#decimal` | `14.50^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Reserve@EN` | `14.0^^http://www.w3.org/2001/XMLSchema#decimal` | `32.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Valley Vin Santo@EN` | `16.0^^http://www.w3.org/2001/XMLSchema#decimal` | `38.00^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The Le Crete White and the Valley Rosé both have 12 degrees, so the price decides: the white costs 11 euros, the rosé 10 and the white comes first.

An order can also rest on a computed value rather than on a variable: `RDFOrderByModifier` has a second constructor that takes an expression, as in `ORDER BY STRLEN(?label)`. [Bind and expressions]({{ '/query/expressions/' | relative_url }}) shows how.

## The three most expensive wines

`RDFLimitModifier` keeps the first rows of the sorted table. Sort from the dearest and keep three:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?price
WHERE {
  ?wine rdfs:label ?label ;
        vr:price ?price .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
ORDER BY DESC(?price)
LIMIT 3
```

```csharp
RDFSelectQuery topThree = new RDFSelectQuery()
    .AddPatternGroup(priceList)
    .AddModifier(new RDFOrderByModifier(price, RDFQueryEnums.RDFOrderByFlavors.DESC))
    .AddModifier(new RDFLimitModifier(3))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price);

RDFSelectQueryResult top = await topThree.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?PRICE |
|---|---|
| `Valley Vin Santo@EN` | `38.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Reserve@EN` | `32.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Red@EN` | `14.50^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

A limit of zero keeps no rows at all. To read the list page by page, add `RDFOffsetModifier`, which skips the given number of rows before the limit counts. The second page of three wines:

```csharp
RDFSelectQuery nextThree = new RDFSelectQuery()
    .AddPatternGroup(priceList)
    .AddModifier(new RDFOrderByModifier(price, RDFQueryEnums.RDFOrderByFlavors.DESC))
    .AddModifier(new RDFLimitModifier(3))
    .AddModifier(new RDFOffsetModifier(3))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price);

RDFSelectQueryResult next = await nextThree.ApplyToGraphAsync(catalog);
```

<div class="result" markdown="1">

| ?LABEL | ?PRICE |
|---|---|
| `Vermentino of the Plain@EN` | `13.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Le Crete White@EN` | `11.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Valley Rosé@EN` | `10.00^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The SPARQL text of this query is the one above with `OFFSET 3` after `LIMIT 3`. An offset beyond the number of rows leaves an empty table. Without an `ORDER BY`, limit and offset slice a table whose order is not guaranteed, so use them together.

## Removing repetitions

The six wines name only three producers, so the pattern gives each producer more than once. `RDFDistinctModifier` keeps each row once:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT DISTINCT ?producer
WHERE {
  ?wine vr:producedBy ?producer .
}
```

```csharp
RDFResource producedBy = new RDFResource(vr + "producedBy");

RDFSelectQuery distinctProducers = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer)))
    .AddModifier(new RDFDistinctModifier())
    .AddProjectionVariable(producer);

RDFSelectQueryResult distinctResult = await distinctProducers.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?PRODUCER |
|---|
| `https://vallerosa.example/id/poggio-alto` |
| `https://vallerosa.example/id/le-crete` |
| `https://vallerosa.example/id/tre-ulivi` |

</div>

Without the modifier the table has six rows, one for each wine. Distinct looks at the projected columns only, so projecting one more variable, the wine, would bring the repetitions back.

> SELECT is the only query form that accepts all five modifiers. The other query forms take the same classes but not `RDFDistinctModifier`, because they do not project variables: [Query forms]({{ '/query/query-forms/' | relative_url }}) tells which form takes what.
{: .tip}
