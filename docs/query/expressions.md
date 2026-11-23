---
title: Bind and expressions
permalink: /query/expressions/
tagline: Computing new values from the ones you have
lede: An expression is a small calculation on the values of a row. BIND stores its result in a new variable, a projection shows it as a column, a filter or a sort uses it as a test or a key.
---

> The partner shops ask for the price with Italian VAT and the website wants the names in capitals and the age of each wine. The cooperative stops reading values and starts computing them.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}), loaded as in [Variables and patterns]({{ '/query/patterns/' | relative_url }}). The spatial expressions, which work on geometries, have their own page: [GeoSPARQL]({{ '/query/geosparql/' | relative_url }}).

## What an expression is

Every expression is an object that holds its arguments and computes a value from them for each row. The arguments are variables, constants or other expressions, so a calculation is a tree: `ROUND(?price * 122) / 100` is a division with a rounding on its left and a number on its right and the rounding holds a multiplication.

A few habits cover almost every case:
- the constructors take a variable or an expression where a value is expected, so `new RDFUpperCaseExpression(label)` and `new RDFUpperCaseExpression(otherExpression)` both exist
- a fixed value is an `RDFConstantExpression` around a resource or a literal. The arithmetic classes also take a typed literal directly
- a number is a typed literal with its datatype, as in `new RDFTypedLiteral("122", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)`
- an expression that cannot be computed for a row, for example because a variable is unbound or has the wrong type, gives no value: the row is not dropped, but the variable that should have received the result stays unbound

## Bind: the price with VAT

`RDFBind` holds an expression and the new variable that receives its value. You add it to a pattern group with `AddBind`, after the patterns that bind the variables it uses. The new variable must not exist in the group yet: a second `BIND` on the same name is refused with an `RDFQueryException`.

The price with a 22% VAT, rounded to cents: multiply by 122, round to a whole number, divide by 100.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?price ?withVat
WHERE {
  ?wine rdfs:label ?label ;
        vr:price ?price .
  BIND (ROUND(?price * 122) / 100 AS ?withVat)
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFVariable wine = new RDFVariable("wine");
RDFVariable producer = new RDFVariable("producer");
RDFVariable label = new RDFVariable("label");
RDFVariable price = new RDFVariable("price");
RDFVariable withVat = new RDFVariable("withVat");
RDFConstantExpression english = new RDFConstantExpression(new RDFPlainLiteral("en"));

RDFDivideExpression withVatExpression = new RDFDivideExpression(
    new RDFRoundExpression(
        new RDFMultiplyExpression(price, new RDFTypedLiteral("122", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))),
    new RDFTypedLiteral("100", RDFModelEnums.RDFDatatypes.XSD_DECIMAL));

RDFSelectQuery priceWithVat = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "price"), price))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label, english)))
        .AddBind(new RDFBind(withVatExpression, withVat)))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price)
    .AddProjectionVariable(withVat);

RDFSelectQueryResult vat = priceWithVat.ApplyToGraph(catalog);
foreach (DataRow row in vat.SelectResults.Rows)
    Console.WriteLine($"{row["?LABEL"]} | {row["?PRICE"]} | {row["?WITHVAT"]}");
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?PRICE | ?WITHVAT |
|---|---|---|
| `Vallerosa Red@EN` | `14.50^^http://www.w3.org/2001/XMLSchema#decimal` | `17.69^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vallerosa Reserve@EN` | `32.00^^http://www.w3.org/2001/XMLSchema#decimal` | `39.04^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Le Crete White@EN` | `11.00^^http://www.w3.org/2001/XMLSchema#decimal` | `13.42^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Valley Vin Santo@EN` | `38.00^^http://www.w3.org/2001/XMLSchema#decimal` | `46.36^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Vermentino of the Plain@EN` | `13.00^^http://www.w3.org/2001/XMLSchema#decimal` | `15.86^^http://www.w3.org/2001/XMLSchema#decimal` |
| `Valley Rosé@EN` | `10.00^^http://www.w3.org/2001/XMLSchema#decimal` | `12.2^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The Vallerosa Red, at 14.50 euros, becomes 17.69: the 14.50 times 122 is 1769, which rounds to itself and the division brings it back to cents. Decimal arithmetic keeps the result a decimal, written in its shortest form, so the last column prints `12.2` and not `12.20`.

## Expressions in the projection

`BIND` is not the only place for a calculation. `AddProjectionVariable` takes an optional second argument, an expression and the column is then computed rather than read: this is `SELECT (expr AS ?x)`. The result is the same as with `BIND` when the calculation is the same:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?price (ROUND(?price * 122) / 100 AS ?withVat)
WHERE {
  ?wine rdfs:label ?label ;
        vr:price ?price .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFSelectQuery priceWithVatProjected = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "price"), price))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label, english))))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price)
    .AddProjectionVariable(withVat, withVatExpression);
```

</div>

Choose `BIND` when later patterns or filters of the group need the value and the projection when the value is only meant to be shown. The label of each wine in capitals is such a case:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label (UCASE(?label) AS ?shout)
WHERE {
  ?wine vr:producedBy ?producer ;
        rdfs:label ?label .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFVariable shout = new RDFVariable("shout");

RDFSelectQuery upperCase = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "producedBy"), producer))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label, english))))
    .AddProjectionVariable(label)
    .AddProjectionVariable(shout, new RDFUpperCaseExpression(label));

RDFSelectQueryResult upperCaseResult = upperCase.ApplyToGraph(catalog);
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?SHOUT |
|---|---|
| `Vallerosa Red@EN` | `VALLEROSA RED@EN` |
| `Vallerosa Reserve@EN` | `VALLEROSA RESERVE@EN` |
| `Le Crete White@EN` | `LE CRETE WHITE@EN` |
| `Valley Vin Santo@EN` | `VALLEY VIN SANTO@EN` |
| `Vermentino of the Plain@EN` | `VERMENTINO OF THE PLAIN@EN` |
| `Valley Rosé@EN` | `VALLEY ROSÉ@EN` |

</div>

The producer pattern keeps the query on wines: without it the labels of the areas and of the cooperative, which are in English too, would come along.

## Expressions in a sort

`RDFOrderByModifier` has a second constructor that takes an expression, so the sort key can be a calculation. The wines from the shortest name to the longest:

```csharp
RDFSelectQuery byLabelLength = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "producedBy"), producer))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label, english))))
    .AddModifier(new RDFOrderByModifier(new RDFStrLenExpression(label), RDFQueryEnums.RDFOrderByFlavors.ASC))
    .AddProjectionVariable(label);
```

<div class="result" markdown="1">

| ?LABEL |
|---|
| `Valley Rosé@EN` |
| `Vallerosa Red@EN` |
| `Le Crete White@EN` |
| `Valley Vin Santo@EN` |
| `Vallerosa Reserve@EN` |
| `Vermentino of the Plain@EN` |

</div>

## The age of a wine

The vintage of a wine is an `xsd:gYear`, a bare year. `YEAR` is made for dates and date-times, so it has no value for a bare year. The year is in the text of the literal instead: take the first four characters of its string, turn them into a number with `STRDT` and subtract from 2025.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX xsd:  <http://www.w3.org/2001/XMLSchema#>

SELECT ?label ?vintage ?age
WHERE {
  ?wine vr:vintage ?vintage ;
        rdfs:label ?label .
  BIND (2025 - STRDT(SUBSTR(STR(?vintage), 1, 4), xsd:integer) AS ?age)
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFVariable vintage = new RDFVariable("vintage");
RDFVariable age = new RDFVariable("age");

RDFSelectQuery ages = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "vintage"), vintage))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label, english)))
        .AddBind(new RDFBind(
            new RDFSubtractExpression(
                new RDFConstantExpression(new RDFTypedLiteral("2025", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
                new RDFStrDtExpression(
                    new RDFSubstringExpression(new RDFStrExpression(vintage), 1, 4),
                    new RDFConstantExpression(RDFVocabulary.XSD.INTEGER))),
            age)))
    .AddProjectionVariable(label)
    .AddProjectionVariable(vintage)
    .AddProjectionVariable(age);

RDFSelectQueryResult agesResult = ages.ApplyToGraph(catalog);
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?VINTAGE | ?AGE |
|---|---|---|
| `Vallerosa Red@EN` | `2021Z^^http://www.w3.org/2001/XMLSchema#gYear` | `4^^http://www.w3.org/2001/XMLSchema#integer` |
| `Vallerosa Reserve@EN` | `2019Z^^http://www.w3.org/2001/XMLSchema#gYear` | `6^^http://www.w3.org/2001/XMLSchema#integer` |
| `Le Crete White@EN` | `2023Z^^http://www.w3.org/2001/XMLSchema#gYear` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `Valley Vin Santo@EN` | `2015Z^^http://www.w3.org/2001/XMLSchema#gYear` | `10^^http://www.w3.org/2001/XMLSchema#integer` |
| `Vermentino of the Plain@EN` | `2023Z^^http://www.w3.org/2001/XMLSchema#gYear` | `2^^http://www.w3.org/2001/XMLSchema#integer` |
| `Valley Rosé@EN` | `2024Z^^http://www.w3.org/2001/XMLSchema#gYear` | `1^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

## A condition and a fallback

`RDFIfExpression` takes a condition and two expressions. Together with `RDFBoundExpression` it gives a readable fallback for an optional value. With the undeclared wine `vrd:novello-2025` of [the previous pages]({{ '/query/patterns/' | relative_url }}), the organic flag of each wine as text, or "not declared":

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?wine ?declared
WHERE {
  ?wine vr:producedBy ?producer .
  OPTIONAL { ?wine vr:organic ?organic }
  BIND (IF(BOUND(?organic), STR(?organic), "not declared") AS ?declared)
}
```

```csharp
RDFResource novello = new RDFResource(vrd + "novello-2025");
RDFGraph harvest = new RDFGraph()
    .AddTriple(new RDFTriple(novello, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RedWine")))
    .AddTriple(new RDFTriple(novello, new RDFResource(vr + "producedBy"), new RDFResource(vrd + "poggio-alto")));
RDFGraph catalogAndHarvest = catalog.UnionWith(harvest);

RDFVariable organic = new RDFVariable("organic");
RDFVariable declared = new RDFVariable("declared");

RDFSelectQuery organicOrNot = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "producedBy"), producer))
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "organic"), organic).Optional())
        .AddBind(new RDFBind(
            new RDFIfExpression(
                new RDFBoundExpression(organic),
                new RDFStrExpression(organic),
                new RDFConstantExpression(new RDFPlainLiteral("not declared"))),
            declared)))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(declared);

RDFSelectQueryResult organicOrNotResult = organicOrNot.ApplyToGraph(catalogAndHarvest);
```

</div>

<div class="result" markdown="1">

| ?WINE | ?DECLARED |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `false` |
| `https://vallerosa.example/id/riserva-2019` | `false` |
| `https://vallerosa.example/id/bianco-2023` | `true` |
| `https://vallerosa.example/id/vinsanto-2015` | `false` |
| `https://vallerosa.example/id/vermentino-2023` | `true` |
| `https://vallerosa.example/id/rosato-2024` | `false` |
| `https://vallerosa.example/id/novello-2025` | `not declared` |

</div>

## A fingerprint and a new IRI

Hash expressions give the hexadecimal digest of a text. A short code that identifies each wine without showing its IRI is the SHA-256 of that IRI as a string. The term functions build new terms: here `STRAFTER` cuts the name of a wine out of its IRI, `CONCAT` puts it after the address of the partner shop and `IRI` turns the text into a resource.

```csharp
RDFVariable fingerprint = new RDFVariable("fingerprint");
RDFVariable shopPage = new RDFVariable("shopPage");

RDFSelectQuery fingerprints = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "producedBy"), producer)))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(fingerprint, new RDFSHA256Expression(new RDFStrExpression(wine)));

RDFSelectQuery shopPages = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "producedBy"), producer))
        .AddBind(new RDFBind(
            new RDFIriExpression(new RDFConcatExpression(
                new RDFConstantExpression(new RDFPlainLiteral("https://shop.example/wines/")),
                new RDFStrAfterExpression(
                    new RDFStrExpression(wine),
                    new RDFConstantExpression(new RDFPlainLiteral(vrd.ToString()))))),
            shopPage)))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(shopPage);
```

<div class="result" markdown="1">

| ?WINE | ?FINGERPRINT |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `78053c4bbbdb5be6343e49dae10853093fdf328bd376d3c7fd8bfd13facec123` |
| `https://vallerosa.example/id/riserva-2019` | `2113dd6b1ad26b840e1fcfceeabe8a3f7a75dd1af5a03e69651b96e46e7d852b` |
| `https://vallerosa.example/id/bianco-2023` | `52853692611346f958bcfbbcaf66f09622e779afc7544762be7f46603051de57` |
| `https://vallerosa.example/id/vinsanto-2015` | `392b3e572f019c9d480dac2bcce80ee328dcb6084a50aaef5a6cf1427fc4ab7d` |
| `https://vallerosa.example/id/vermentino-2023` | `edf03cacac8c262da08771f6150f0df65f43b856a055d12a5259ccd17807efc9` |
| `https://vallerosa.example/id/rosato-2024` | `91b42b9f478d4650084a659a9e3a8a1fb802f74c898e7bd7b09ee418c0819e95` |

</div>

<div class="result" markdown="1">

| ?WINE | ?SHOPPAGE |
|---|---|
| `https://vallerosa.example/id/rosso-2021` | `https://shop.example/wines/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` | `https://shop.example/wines/riserva-2019` |
| `https://vallerosa.example/id/bianco-2023` | `https://shop.example/wines/bianco-2023` |
| `https://vallerosa.example/id/vinsanto-2015` | `https://shop.example/wines/vinsanto-2015` |
| `https://vallerosa.example/id/vermentino-2023` | `https://shop.example/wines/vermentino-2023` |
| `https://vallerosa.example/id/rosato-2024` | `https://shop.example/wines/rosato-2024` |

</div>

## All the expressions

Each class below is built from variables, constants and other expressions as in the examples above. The filter expressions of the [Filters]({{ '/query/filters/' | relative_url }}) page are expressions too and the aggregators of the [Aggregates]({{ '/query/aggregates/' | relative_url }}) page can run on them.

| Family | Class | SPARQL | What it gives |
|---|---|---|---|
| Arithmetic | `RDFAddExpression`, `RDFSubtractExpression`, `RDFMultiplyExpression`, `RDFDivideExpression` | `+` `-` `*` `/` | the result of the operation on two numbers |
| Arithmetic | `RDFAbsExpression` | `ABS` | the absolute value |
| Arithmetic | `RDFRoundExpression`, `RDFCeilExpression`, `RDFFloorExpression` | `ROUND` `CEIL` `FLOOR` | the number rounded to the nearest, upper or lower whole value |
| Arithmetic | `RDFRandExpression` | `RAND` | a random number between 0 and 1 |
| Comparison | `RDFComparisonExpression` | `=` `!=` `<` `<=` `>` `>=` | true or false for two values |
| Comparison | `RDFSameTermExpression`, `RDFInExpression` | `SAMETERM`, `IN` | whether two terms are the same, whether a term is in a list |
| Boolean | `RDFBooleanAndExpression`, `RDFBooleanOrExpression`, `RDFNotExpression` | `&&` `\|\|` `!` | the logical combination of conditions |
| Test | `RDFBoundExpression`, `RDFExistsExpression` | `BOUND`, `EXISTS` | whether a variable has a value, whether a group of patterns has a match |
| Test | `RDFIsBlankExpression`, `RDFIsLiteralExpression`, `RDFIsNumericExpression`, `RDFIsUriExpression` | `ISBLANK` `ISLITERAL` `ISNUMERIC` `ISURI` | the kind of a term |
| Conditional | `RDFIfExpression` | `IF` | one of two values according to a condition |
| Conditional | `RDFCoalesceExpression` | `COALESCE` | the first of its two arguments that gives a value |
| String | `RDFConcatExpression` | `CONCAT` | two strings joined |
| String | `RDFStrLenExpression`, `RDFSubstringExpression` | `STRLEN`, `SUBSTR` | the length of a string, a part of it |
| String | `RDFUpperCaseExpression`, `RDFLowerCaseExpression` | `UCASE`, `LCASE` | the string in upper or lower case |
| String | `RDFContainsExpression`, `RDFStrStartsExpression`, `RDFStrEndsExpression` | `CONTAINS` `STRSTARTS` `STRENDS` | whether a string has a given part, start or end |
| String | `RDFStrBeforeExpression`, `RDFStrAfterExpression` | `STRBEFORE`, `STRAFTER` | what comes before or after the first occurrence of a text |
| String | `RDFRegexExpression`, `RDFReplaceExpression` | `REGEX`, `REPLACE` | a match on a pattern, the text with the matches replaced |
| String | `RDFEncodeForURIExpression` | `ENCODE_FOR_URI` | the string made safe for an IRI |
| Date and time | `RDFYearExpression`, `RDFMonthExpression`, `RDFDayExpression` | `YEAR` `MONTH` `DAY` | a part of a date or date-time |
| Date and time | `RDFHoursExpression`, `RDFMinutesExpression`, `RDFSecondsExpression` | `HOURS` `MINUTES` `SECONDS` | a part of the time |
| Date and time | `RDFTimezoneExpression`, `RDFTzExpression` | `TIMEZONE`, `TZ` | the time zone as a duration, as a text |
| Date and time | `RDFNowExpression` | `NOW` | the current date and time |
| Hash | `RDFMD5Expression`, `RDFSHA1Expression`, `RDFSHA256Expression`, `RDFSHA384Expression`, `RDFSHA512Expression` | `MD5` `SHA1` `SHA256` `SHA384` `SHA512` | the hexadecimal digest of a string |
| Term | `RDFStrExpression` | `STR` | the lexical form of a term, without language or datatype |
| Term | `RDFLangExpression`, `RDFLangDirExpression`, `RDFHasLangExpression`, `RDFHasLangDirExpression`, `RDFLangMatchesExpression` | `LANG` `LANGDIR` `HASLANG` `HASLANGDIR` `LANGMATCHES` | the language tag and its direction of a literal, whether it has one, whether it matches a range |
| Term | `RDFDatatypeExpression` | `DATATYPE` | the datatype of a literal |
| Term | `RDFStrDtExpression`, `RDFStrLangExpression`, `RDFStrLangDirExpression` | `STRDT` `STRLANG` `STRLANGDIR` | a typed literal, or a literal with a language, built from a string |
| Term | `RDFIriExpression`, `RDFBNodeExpression` | `IRI`, `BNODE` | a resource from a string, a new blank node |
| Term | `RDFUUIDExpression`, `RDFStrUUIDExpression` | `UUID`, `STRUUID` | a fresh IRI or a fresh string, made from a random identifier |
| Term | `RDFConstantExpression`, `RDFVariableExpression` | the constant, the variable | a fixed term, the value of a variable |

> One point to remember when you build a query with a calculation: `RDFNowExpression`, `RDFRandExpression` and the UUID expressions give a different value every time they are evaluated.
{: .tip}

A query ends as a table, a truth value or a graph, according to its form. The next page tells how: [Query forms]({{ '/query/query-forms/' | relative_url }}).
