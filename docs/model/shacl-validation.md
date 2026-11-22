---
title: SHACL validation
permalink: /model/shacl-validation/
tagline: Check a graph against its shapes and read what is wrong
lede: A shapes graph is applied to a data graph and answers with a report. You read the report in code or write it out as RDF and you fix the data until it conforms.
---

> A new import arrives for the website: a wine called `vrd:nero-2024`. Nobody has looked at it yet. Before it reaches the catalog, the cooperative checks it against its rules and finds four mistakes in three lines of data.
{: .story}

## Apply the shapes

`Validate` is an extension method of `RDFShapesGraph`: it takes the **data graph** to check and returns an `RDFValidationReport`. The shapes are the ones that the [previous chapter]({{ '/model/shacl-shapes/' | relative_url }}) saved in `shapes-graph.ttl` and the data graph is the catalog.

One detail decides whether the check means anything. The shape of the wine targets the class `vr:Wine`, but the catalog only says that `vrd:rosso-2021` is a `vr:RedWine`. That a red wine is a wine is written in the vocabulary file, as `rdfs:subClassOf`, so the vocabulary must be in the data graph too. The catalog goes in together with it:

```csharp
using RDFSharp.Model;

RDFShapesGraph shapes = RDFShapesGraph.FromRDFGraph(RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "shapes-graph.ttl"));
RDFGraph vocabulary = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-vocab.ttl");
RDFGraph catalog = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");
RDFGraph data = vocabulary.UnionWith(catalog);

RDFValidationReport report = shapes.Validate(data);
Console.WriteLine(report.Conforms);
Console.WriteLine(report.ResultsCount);
```

<div class="result" markdown="1">

```
True
0
```

</div>

The report has two properties that you will use all the time: `Conforms` is true when there is nothing to complain about and `ResultsCount` is the number of complaints. The catalog of the cooperative follows its own rules.

## A sloppy import

This is what the import file says. It is a wine in Turtle, the way a producer would send it:

```turtle
# import.ttl
@prefix vr:   <https://vallerosa.example/vocab#> .
@prefix vrd:  <https://vallerosa.example/id/> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

vrd:nero-2024 a vr:RedWine ;
    rdfs:label "Vin rouge de Vallerosa"@fr ;
    vr:alcohol "135"^^xsd:decimal .
```

Four things are wrong and none of them is a syntax error:
- there is no `vr:vintage` at all
- the alcohol is 135, a typed decimal like the others, so XML Schema has no objection: the decimal point is missing
- the only label is in French
- there is no `vr:producedBy`

The import is a graph on its own. By itself it passes and that is a trap:

```csharp
RDFGraph imported = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "import.ttl");
Console.WriteLine(shapes.Validate(imported).Conforms);

RDFGraph candidate = data.UnionWith(imported);
RDFValidationReport badReport = shapes.Validate(candidate);
Console.WriteLine(badReport.Conforms);
Console.WriteLine(badReport.ResultsCount);
```

<div class="result" markdown="1">

```
True
False
4
```

</div>

The first answer is `True`: with no vocabulary the import is a wine of an unknown class `vr:RedWine`, nothing is a `vr:Wine` and a shape without focus nodes has nothing to check. A **conforming** report only says that no rule was broken for the nodes the shapes could see. The honest question is what the website would hold after the import: the catalog, the vocabulary and the new wine together. That graph, `candidate`, has four results.

## Read the report

An `RDFValidationReport` is a sequence of `RDFValidationResult`, one for each broken rule on each focus node:

```csharp
foreach (RDFValidationResult result in badReport)
{
    Console.WriteLine($"{result.Severity} on {result.FocusNode}");
    Console.WriteLine($"  shape: {result.SourceShape}");
    Console.WriteLine($"  rule:  {result.SourceConstraintComponent}");
    Console.WriteLine($"  path:  {result.ResultPath}");
    Console.WriteLine($"  value: {result.ResultValue}");
    Console.WriteLine($"  why:   {string.Join(" / ", result.ResultMessages)}");
}
```

<div class="result" markdown="1">

```
Violation on https://vallerosa.example/id/nero-2024
  shape: https://vallerosa.example/vocab#WineVintageShape
  rule:  http://www.w3.org/ns/shacl#MinCountConstraintComponent
  path:  ?START <https://vallerosa.example/vocab#vintage> ?END
  value: 
  why:   Must have a minimum of 1 occurrences
Violation on https://vallerosa.example/id/nero-2024
  shape: https://vallerosa.example/vocab#WineAlcoholShape
  rule:  http://www.w3.org/ns/shacl#MaxInclusiveConstraintComponent
  path:  ?START <https://vallerosa.example/vocab#alcohol> ?END
  value: 135^^http://www.w3.org/2001/XMLSchema#decimal
  why:   Must have values lower or equal than <20^^http://www.w3.org/2001/XMLSchema#decimal>
Violation on https://vallerosa.example/id/nero-2024
  shape: https://vallerosa.example/vocab#WineLabelShape
  rule:  http://www.w3.org/ns/shacl#LanguageInConstraintComponent
  path:  ?START <http://www.w3.org/2000/01/rdf-schema#label> ?END
  value: Vin rouge de Vallerosa@FR
  why:   Not a language from the sh:languageIn enumeration
Violation on https://vallerosa.example/id/nero-2024
  shape: https://vallerosa.example/vocab#WineProducerShape
  rule:  http://www.w3.org/ns/shacl#MinCountConstraintComponent
  path:  ?START <https://vallerosa.example/vocab#producedBy> ?END
  value: 
  why:   Must have a minimum of 1 occurrences
```

</div>

Every result answers the same questions:

| Property | Answers | In the first result |
|---|---|---|
| `Severity` | how serious: `Violation`, `Warning` or `Info` | a violation |
| `FocusNode` | which node broke the rule | `vrd:nero-2024` |
| `SourceShape` | which shape holds the rule | the vintage shape |
| `SourceConstraintComponent` | which kind of constraint failed | `sh:MinCountConstraintComponent` |
| `ResultPath` | along which property | `vr:vintage` (the library prints a path between the variables `?START` and `?END`) |
| `ResultValue` | which value was wrong | none: a missing value has nothing to show |
| `ResultMessages` | an explanation in words | "Must have a minimum of 1 occurrences" |

Read the four results as the four mistakes: the missing vintage, the alcohol of 135 that exceeds the maximum of 20, the French label that is not in the list of languages and the missing producer. The two that are about absence have no value and the two that are about a bad value carry it. The label is shown as `@FR`: the library writes every tag in upper case and the catalog's own `@en` labels pass a list of `it` and `en` because the comparison ignores the case.

## The report as RDF

The report is RDF as well, as SHACL defines a vocabulary for it. `ToRDFGraph` gives the graph and from there the usual serializers can write it, so a report can be sent back to the producer who made the mistake:

```csharp
RDFGraph reportGraph = badReport.ToRDFGraph().SetContext(new Uri("https://vallerosa.example/report"));
Console.WriteLine(reportGraph.TriplesCount);
reportGraph.ToFile(RDFModelEnums.RDFFormats.Turtle, "report.ttl");
```

<div class="result" markdown="1">

```
36
```

</div>

<div class="result" data-label="report.ttl" markdown="1">

```turtle
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix sh: <http://www.w3.org/ns/shacl#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@base <https://vallerosa.example/report>.

_:26e4ff53fc7047e2bc2ef56e525b3df5 a sh:ValidationReport; 
                                   sh:conforms "false"^^xsd:boolean; 
                                   sh:result _:a7a9701fa0e9484ba81c6b0dcf3a9b79, _:440c1472bd3840e58cfafb8e4dfc704f, _:b5fbedc39b71401d914c10b59efe3baf, _:37eae32e26e5493db9b63bcd6679a0d5. 
_:37eae32e26e5493db9b63bcd6679a0d5 a sh:ValidationResult; 
                                   sh:focusNode <https://vallerosa.example/id/nero-2024>; 
                                   sh:resultMessage "Must have a minimum of 1 occurrences"; 
                                   sh:resultPath <https://vallerosa.example/vocab#producedBy>; 
                                   sh:resultSeverity sh:Violation; 
                                   sh:sourceConstraintComponent sh:MinCountConstraintComponent; 
                                   sh:sourceShape <https://vallerosa.example/vocab#WineProducerShape>. 
_:440c1472bd3840e58cfafb8e4dfc704f a sh:ValidationResult; 
                                   sh:focusNode <https://vallerosa.example/id/nero-2024>; 
                                   sh:resultMessage "Must have values lower or equal than <20^^http://www.w3.org/2001/XMLSchema#decimal>"; 
                                   sh:resultPath <https://vallerosa.example/vocab#alcohol>; 
                                   sh:resultSeverity sh:Violation; 
                                   sh:sourceConstraintComponent sh:MaxInclusiveConstraintComponent; 
                                   sh:sourceShape <https://vallerosa.example/vocab#WineAlcoholShape>; 
                                   sh:value "135"^^xsd:decimal. 
_:a7a9701fa0e9484ba81c6b0dcf3a9b79 a sh:ValidationResult; 
                                   sh:focusNode <https://vallerosa.example/id/nero-2024>; 
                                   sh:resultMessage "Must have a minimum of 1 occurrences"; 
                                   sh:resultPath <https://vallerosa.example/vocab#vintage>; 
                                   sh:resultSeverity sh:Violation; 
                                   sh:sourceConstraintComponent sh:MinCountConstraintComponent; 
                                   sh:sourceShape <https://vallerosa.example/vocab#WineVintageShape>. 
_:b5fbedc39b71401d914c10b59efe3baf a sh:ValidationResult; 
                                   sh:focusNode <https://vallerosa.example/id/nero-2024>; 
                                   sh:resultMessage "Not a language from the sh:languageIn enumeration"; 
                                   sh:resultPath rdfs:label; 
                                   sh:resultSeverity sh:Violation; 
                                   sh:sourceConstraintComponent sh:LanguageInConstraintComponent; 
                                   sh:sourceShape <https://vallerosa.example/vocab#WineLabelShape>; 
                                   sh:value "Vin rouge de Vallerosa"@FR. 
```

</div>

One `sh:ValidationReport` with `sh:conforms false` and four `sh:result`, each a `sh:ValidationResult` with the same facts that the loop printed. This time the path is a plain IRI, `sh:resultPath vr:vintage` and the names with `_:` are blank resources that change at every run. A report that conforms is a single `sh:ValidationReport` with `sh:conforms true` and no result.

## Fix it and validate again

The producer sends the import again. This time it has two labels in the languages of the catalog, a producer, a vintage and an alcohol of 13.5:

```turtle
# import-fixed.ttl
@prefix vr:   <https://vallerosa.example/vocab#> .
@prefix vrd:  <https://vallerosa.example/id/> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

vrd:nero-2024 a vr:RedWine ;
    rdfs:label "Nero di Vallerosa"@it, "Vallerosa Black"@en ;
    vr:producedBy vrd:poggio-alto ;
    vr:vintage "2024"^^xsd:gYear ;
    vr:alcohol 13.5 .
```

```csharp
RDFGraph fixedImport = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "import-fixed.ttl");
RDFValidationReport goodReport = shapes.Validate(data.UnionWith(fixedImport));
Console.WriteLine(goodReport.Conforms);
Console.WriteLine(goodReport.ResultsCount);
```

<div class="result" markdown="1">

```
True
0
```

</div>

The catalog with the corrected wine conforms and the import can go on to the website. A process that applies the shapes at every import has a simple rule: when `Conforms` is false, the data does not move on and the report goes back to whoever sent it.

`ValidateAsync` and `ToRDFGraphAsync` are the asynchronous twins, for a validation that should not block. With a shapes graph in a file and a report that goes back as RDF, the cooperative has the whole loop: rules that anybody can read, data that is checked before it is published and a report that explains itself.
