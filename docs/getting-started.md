---
title: Getting started
permalink: /getting-started/
tagline: From an empty project to a first answer
lede: Three steps to a console program that loads RDF data and asks it a SPARQL question.
---

## 1. Install

RDFSharp is one NuGet package for `netstandard2.0`, `net8.0` and `net10.0`.

```
dotnet new console -n Vallerosa
cd Vallerosa
dotnet add package RDFSharp
```

## 2. Get the data

Download [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}) into the project folder. It is the catalog that every example of this manual uses: [Meet Vallerosa]({{ '/vallerosa/' | relative_url }}) tells what is inside.

## 3. Ask a question

Replace `Program.cs` with this program. It loads the catalog and asks for the red wines under 20 euros with their English names.

```csharp
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");
RDFVariable price = new RDFVariable("price");

RDFSelectQuery query = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RedWine")))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "price"), price))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(new RDFPlainLiteral("en")))))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            price, new RDFConstantExpression(new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))))))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price);

RDFSelectQueryResult result = await query.ApplyToGraphAsync(catalog);
foreach (DataRow row in result.SelectResults.Rows)
    Console.WriteLine($"{row["?LABEL"]} | {row["?PRICE"]}");
```

Run it with `dotnet run`:

<div class="result" markdown="1">

```
Vallerosa Red@EN | 14.50^^http://www.w3.org/2001/XMLSchema#decimal
```

</div>

The answer is a `DataTable` with one column for each projected variable, named in upper case.

