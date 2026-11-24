---
title: Filters
permalink: /query/filters/
tagline: Keeping only the rows that pass a test
lede: "A pattern finds everything that has the right shape. A filter keeps the rows whose values also pass a test: a comparison, a language, a regular expression, the presence of other triples."
---

> The website wants a short list for people who prefer light wines: the whites under 13 degrees of alcohol, named in English. Later the cooperative adds tests on the organic flag and on the strength of the reds.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}), loaded as in [Variables and patterns]({{ '/query/patterns/' | relative_url }}). It uses `System.Text.RegularExpressions` for one example.

## A filter is an expression

In SPARQL a `FILTER` holds a condition that is true or false for each row. In RDFSharp the condition is an **expression**, an object that computes a value from variables and constants and `RDFFilter` wraps one that gives a boolean. There is no class per kind of filter: a comparison is an `RDFComparisonExpression`, a language test an `RDFLangMatchesExpression`, a negation an `RDFNotExpression` and so on. The same expressions serve in [Bind and expressions]({{ '/query/expressions/' | relative_url }}) to compute new values.

Two details save time. A constant is written with `RDFConstantExpression`, which wraps a resource or a literal. A number must be a typed literal with a numeric datatype. Numbers compare by value across `xsd:integer`, `xsd:decimal` and `xsd:double`. The page writes the threshold as a decimal like the alcohol of the catalog.

## The whites under 13 degrees

The filters go on the pattern group, next to the patterns they restrict, with `AddFilter`. Two filters in one group must both hold, exactly as two patterns must. The first keeps the English label, the second the weak wines.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?alcohol
WHERE {
  ?wine a vr:WhiteWine ;
        rdfs:label ?label ;
        vr:alcohol ?alcohol .
  FILTER (LANGMATCHES(LANG(?label), "en"))
  FILTER (?alcohol < 13)
}
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFResource whiteWine = new RDFResource(vr + "WhiteWine");
RDFResource hasAlcohol = new RDFResource(vr + "alcohol");

RDFPlainLiteral englishTag = new RDFPlainLiteral("en");
RDFTypedLiteral maximumLightAlcohol = new RDFTypedLiteral("13", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);

RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");
RDFVariable alcohol = new RDFVariable("alcohol");

RDFSelectQuery lightWhites = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, whiteWine))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, hasAlcohol, alcohol))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag))))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.LessThan,
            alcohol,
            new RDFConstantExpression(maximumLightAlcohol)))))
    .AddProjectionVariable(label)
    .AddProjectionVariable(alcohol);

RDFSelectQueryResult lightWhitesResult = await lightWhites.ApplyToGraphAsync(catalog);
foreach (DataRow row in lightWhitesResult.SelectResults.Rows)
    Console.WriteLine($"{row["?LABEL"]} | {row["?ALCOHOL"]}");
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?ALCOHOL |
|---|---|
| `Le Crete White@EN` | `12.0^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vermentino of the Plain@EN` | `12.5^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The two whites of the catalog come out, each with its English label. A word on the language test: `RDFLangMatchesExpression` takes the variable itself and applies `LANG()` on its own, so you do not wrap the variable in a language expression yourself. Without the language filter, every wine would come twice, once per label.

## Tests on the presence of triples

`RDFExistsExpression` holds a pattern group and is true when that group has at least one match for the values of the row. Wrapped in an `RDFNotExpression` it becomes `NOT EXISTS`. The reds that are not organic:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine
WHERE {
  ?wine a vr:RedWine .
  FILTER NOT EXISTS { ?wine vr:organic true }
}
```

```csharp
RDFResource redWine = new RDFResource(vr + "RedWine");
RDFResource isOrganic = new RDFResource(vr + "organic");

RDFTypedLiteral organicTrue = new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);

RDFSelectQuery conventionalReds = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, redWine))
        .AddFilter(new RDFFilter(new RDFNotExpression(new RDFExistsExpression(
            new RDFPatternGroup().AddPattern(new RDFPattern(wine, isOrganic, organicTrue)))))))
    .AddProjectionVariable(wine);

RDFSelectQueryResult conventionalResult = await conventionalReds.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` |

</div>

Without the `RDFNotExpression` the filter is a plain `EXISTS`. Asked of the whites instead of the reds, it answers with the two organic ones, `vrd:bianco-2023` and `vrd:vermentino-2023`.

## Is it bound

When a pattern is optional, a variable can stay empty and `RDFBoundExpression` tests exactly that. Reusing the new wine `vrd:novello-2025` of [the previous page]({{ '/query/patterns/' | relative_url }}), whose organic flag nobody has declared, here is how to find every red wine that still waits for it:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine
WHERE {
  ?wine a vr:RedWine .
  OPTIONAL { ?wine vr:organic ?organic }
  FILTER (!BOUND(?organic))
}
```

```csharp
RDFResource producedBy = new RDFResource(vr + "producedBy");

RDFResource novello2025 = new RDFResource(vrd + "novello-2025");
RDFResource poggioAlto = new RDFResource(vrd + "poggio-alto");

RDFVariable organic = new RDFVariable("organic");

RDFGraph harvest = new RDFGraph()
    .AddTriple(new RDFTriple(novello2025, RDFVocabulary.RDF.TYPE, redWine))
    .AddTriple(new RDFTriple(novello2025, producedBy, poggioAlto));
RDFGraph catalogAndHarvest = await catalog.UnionWithAsync(harvest);

RDFSelectQuery undeclared = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, redWine))
        .AddPattern(new RDFPattern(wine, isOrganic, organic).Optional())
        .AddFilter(new RDFFilter(new RDFNotExpression(new RDFBoundExpression(organic)))))
    .AddProjectionVariable(wine);

RDFSelectQueryResult undeclaredResult = await undeclared.ApplyToGraphAsync(catalogAndHarvest);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/novello-2025` |

</div>

## Combining tests

`RDFBooleanAndExpression` and `RDFBooleanOrExpression` take two expressions each and `RDFNotExpression` takes one, so any boolean formula is a tree. The strong wines are those with at least 14 degrees that are either red or sweet:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine ?alcohol
WHERE {
  ?wine a ?type ;
        vr:alcohol ?alcohol .
  FILTER (?alcohol >= 14 &&
          (?type = vr:RedWine || ?type = vr:SweetWine))
}
```

```csharp
RDFResource sweetWine = new RDFResource(vr + "SweetWine");

RDFTypedLiteral minimumStrongAlcohol = new RDFTypedLiteral("14", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);

RDFVariable type = new RDFVariable("type");

RDFComparisonExpression strong = new RDFComparisonExpression(
    RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
    alcohol,
    new RDFConstantExpression(minimumStrongAlcohol));
RDFComparisonExpression isRed = new RDFComparisonExpression(
    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
    type,
    new RDFConstantExpression(redWine));
RDFComparisonExpression isSweet = new RDFComparisonExpression(
    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
    type,
    new RDFConstantExpression(sweetWine));

RDFSelectQuery strongWines = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, type))
        .AddPattern(new RDFPattern(wine, hasAlcohol, alcohol))
        .AddFilter(new RDFFilter(new RDFBooleanAndExpression(
            strong,
            new RDFBooleanOrExpression(isRed, isSweet)))))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(alcohol);

RDFSelectQueryResult strongResult = await strongWines.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?ALCOHOL |
|---|---|
| `https://vallerosa.example/id/riserva-2019` | `14.0^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/vinsanto-2015` | `16.0^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

Building each comparison in a variable first keeps the formula readable. The Vallerosa Red has 13.5 degrees and falls short.

## A filter on a group and a filter on the query

A filter added to a pattern group sees only the variables of that group. A filter added to the query with `RDFSelectQuery.AddFilter` applies to the whole `WHERE` clause, after all the groups have been joined, so it can compare variables that live in different groups. The wines that are both light and cheap ask for the alcohol from one group and for the price from another:

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine ?alcohol ?price
WHERE {
  { ?wine vr:alcohol ?alcohol }
  { ?wine vr:price ?price }
  FILTER (?alcohol < 13 && ?price < 12)
}
```

```csharp
RDFResource hasPrice = new RDFResource(vr + "price");

RDFTypedLiteral priceCap = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);

RDFVariable price = new RDFVariable("price");

RDFSelectQuery lightAndCheap = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasAlcohol, alcohol)))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasPrice, price)))
    .AddFilter(new RDFFilter(new RDFBooleanAndExpression(
        new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.LessThan,
            alcohol,
            new RDFConstantExpression(maximumLightAlcohol)),
        new RDFComparisonExpression(
            RDFQueryEnums.RDFComparisonFlavors.LessThan,
            price,
            new RDFConstantExpression(priceCap)))))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(alcohol)
    .AddProjectionVariable(price);

RDFSelectQueryResult lightAndCheapResult = await lightAndCheap.ApplyToGraphAsync(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?ALCOHOL | ?PRICE |
|---|---|---|
| `https://vallerosa.example/id/bianco-2023` | `12.0^^http://www.w3.org/2001/XMLSchema#decimal` | `11.00^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/rosato-2024` | `12.0^^http://www.w3.org/2001/XMLSchema#decimal` | `10.00^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

## More tests

A regular expression and a list of terms, the two filters that people reach for after the comparison. The wines whose label starts with "Vallerosa", whatever the case and the wines made from Vermentino or Canaiolo:

```csharp
using System.Text.RegularExpressions;

RDFResource hasGrape = new RDFResource(vr + "grape");
RDFResource vermentino = new RDFResource(vrd + "vermentino");
RDFResource canaiolo = new RDFResource(vrd + "canaiolo");

RDFVariable producer = new RDFVariable("producer");
RDFVariable grape = new RDFVariable("grape");

RDFSelectQuery vallerosaNamed = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, producedBy, producer))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFRegexExpression(label, new Regex("^vallerosa", RegexOptions.IgnoreCase)))))
    .AddProjectionVariable(label);

RDFSelectQuery twoGrapes = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, hasGrape, grape))
        .AddFilter(new RDFFilter(new RDFInExpression(grape,
            new List<RDFPatternMember> { vermentino, canaiolo }))))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(grape);

RDFSelectQueryResult vallerosaNamedResult = await vallerosaNamed.ApplyToGraphAsync(catalog);
RDFSelectQueryResult twoGrapesResult = await twoGrapes.ApplyToGraphAsync(catalog);
```

<div class="result" markdown="1">

| ?LABEL |
|---|
| `Vallerosa Red@EN` |
| `Vallerosa Riserva@IT` |
| `Vallerosa Reserve@EN` |

</div>

<div class="result" markdown="1">

| ?WINE | ?GRAPE |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `https://vallerosa.example/id/canaiolo` |
| `https://vallerosa.example/id/vermentino-2023` | `https://vallerosa.example/id/vermentino` |

</div>

The producer pattern keeps the query on wines: without it the label of the cooperative, "Vallerosa Wine Cooperative", would match too. The other tests all follow the same shape, an expression class inside `RDFFilter`:

| Expression class | SPARQL | What it asks at Vallerosa |
|---|---|---|
| `RDFComparisonExpression` | `?a < ?b`, `=`, `!=`, `<=`, `>`, `>=` | whether a value is above, below or equal to another |
| `RDFLangMatchesExpression` | `LANGMATCHES(LANG(?x), "en")` | a label in English |
| `RDFHasLangExpression` | `HASLANG(?x)` | a label that carries a language tag at all |
| `RDFRegexExpression` | `REGEX(?x, "...", "i")` | a label that matches a pattern |
| `RDFContainsExpression`, `RDFStrStartsExpression`, `RDFStrEndsExpression` | `CONTAINS`, `STRSTARTS`, `STRENDS` | a label with a given word in it, at its start or at its end |
| `RDFInExpression` | `?x IN (...)` | a grape among a fixed list |
| `RDFBoundExpression` | `BOUND(?x)` | an optional value that is there |
| `RDFExistsExpression` | `EXISTS { ... }` and `NOT EXISTS { ... }` | the presence or the absence of other triples |
| `RDFIsLiteralExpression`, `RDFIsNumericExpression`, `RDFIsBlankExpression`, `RDFIsUriExpression` | `ISLITERAL`, `ISNUMERIC`, `ISBLANK`, `ISURI` | the kind of term that an object is: a number among the values of a wine, the blank list of its blend |
| `RDFSameTermExpression` | `SAMETERM(?a, ?b)` | the identical term, not only an equal value |
| `RDFBooleanAndExpression`, `RDFBooleanOrExpression`, `RDFNotExpression` | `&&`, `\|\|`, `!` | any combination of the above |

The next page puts the surviving rows in order: [Modifiers]({{ '/query/modifiers/' | relative_url }}).
