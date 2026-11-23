---
title: Parsing SPARQL
permalink: /query/parser/
tagline: From SPARQL text to query objects and back
lede: A query does not have to be built in C#. RDFSharp reads SPARQL text into the same objects that the fluent calls build. It prints any query object back as SPARQL text.
---

> The cooperative keeps its questions in text files, in plain SPARQL, so that the people who know the wines can read and change them without opening a C# project. The service that runs them has to turn the text into query objects. It must also say clearly when a question is written wrong.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}) and at one point on the store of the Store chapters, with [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}) beside it. It needs these namespaces of the library:

```csharp
using System.Data;
using RDFSharp.Model;   // resources, literals, graphs
using RDFSharp.Query;   // queries, operations and their parser
using RDFSharp.Store;   // memory stores
```

## From text to a query object

The parser covers the SPARQL 1.1 grammar: patterns, filters, property paths, aggregates, subqueries, `VALUES`, `EXISTS`, `SERVICE` and the four query forms. Every query class has a static `FromString` method that takes the text and returns the object that `AddPatternGroup` and its companions would have built. After that the query is an ordinary `RDFSelectQuery`: you apply it to a graph, to a store or to a federation as you always did.

The first question of the cooperative is the one of the [Filters]({{ '/query/filters/' | relative_url }}) chapter, written as text: the white wines under 13 degrees, with their English name and their alcohol.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?wine ?label ?alcohol
WHERE {
  ?wine a vr:WhiteWine ;
        vr:alcohol ?alcohol ;
        rdfs:label ?label .
  FILTER (?alcohol < 13 && LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

string whitesText = @"
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?wine ?label ?alcohol
WHERE {
  ?wine a vr:WhiteWine ;
        vr:alcohol ?alcohol ;
        rdfs:label ?label .
  FILTER (?alcohol < 13 && LANGMATCHES(LANG(?label), ""en""))
}";

RDFSelectQuery whites = RDFSelectQuery.FromString(whitesText);

RDFSelectQueryResult whitesResult = whites.ApplyToGraph(catalog);
foreach (DataRow row in whitesResult.SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?LABEL"]} | {row["?ALCOHOL"]}");
```

</div>

<div class="result" markdown="1">

| ?WINE | ?LABEL | ?ALCOHOL |
|---|---|---|
| `https://vallerosa.example/id/bianco-2023` | `Le Crete White@EN` | `12.0^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino of the Plain@EN` | `12.5^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

Two things are worth noticing. The prefixes are declared in the text, as SPARQL wants: the parser keeps them and resolves every name against them. The engine writes every variable name in upper case: the columns are `?WINE`, `?LABEL` and `?ALCOHOL` although the text says `?wine`.

## Printing it back

`ToString()` on any query prints it as SPARQL. For a parsed query that is a way to see what the parser understood:

```csharp
Console.WriteLine(whites);
```

<div class="result" data-label="Printed query" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?WINE ?LABEL ?ALCOHOL
WHERE {
  {
    ?WINE <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> vr:WhiteWine .
    ?WINE vr:alcohol ?ALCOHOL .
    ?WINE rdfs:label ?LABEL .
    FILTER ( ((?ALCOHOL < 13) && (LANGMATCHES(LANG(?LABEL),"en"))) ) 
  }
}
```

</div>

The text is not the one that went in but it says the same thing. The prefixes that you declared are kept and used. `a` is spelled out as the full IRI of `rdf:type`, since no prefix was declared for it. The patterns sit in a group of their own, as in the object model. The filter shows every parenthesis of its expression tree.

The printed text is a faithful form: parsing it again gives a query that prints the same text and finds the same rows.

```csharp
RDFSelectQuery reparsed = RDFSelectQuery.FromString(whites.ToString());
Console.WriteLine(reparsed.ToString() == whites.ToString());
Console.WriteLine(reparsed.ApplyToGraph(catalog).SelectResultsCount);
```

<div class="result" markdown="1">

```text
True
2
```

</div>

This makes `ToString()` and `FromString` a safe way to save a query that you built in C# and to load it back later, or to send it to a service written in any other language.

## When you do not know the form

Each typed `FromString` demands its own form: `RDFSelectQuery.FromString` refuses an ASK. When the text can be of any form, as when the partner shop submits its own questions, call `RDFQueryParserFactory.ParseQuery`. It reads the prologue and the form keyword and returns the matching class as a plain `RDFQuery`. A pattern match on the type tells which one you got:

```csharp
string[] texts =
{
    "ASK { ?wine <https://vallerosa.example/vocab#organic> true }",
    "DESCRIBE <https://vallerosa.example/id/poggio-alto>",
    "SELECT ?wine WHERE { ?wine a <https://vallerosa.example/vocab#SweetWine> }"
};

foreach (string text in texts)
{
    RDFQuery parsed = RDFQueryParserFactory.ParseQuery(text);
    switch (parsed)
    {
        case RDFSelectQuery select:
            Console.WriteLine($"SELECT, rows: {select.ApplyToGraph(catalog).SelectResultsCount}");
            break;
        case RDFAskQuery ask:
            Console.WriteLine($"ASK: {ask.ApplyToGraph(catalog).AskResult}");
            break;
        case RDFDescribeQuery describe:
            Console.WriteLine($"DESCRIBE, triples: {describe.ApplyToGraph(catalog).DescribeResultsCount}");
            break;
        case RDFConstructQuery construct:
            Console.WriteLine($"CONSTRUCT, triples: {construct.ApplyToGraph(catalog).ConstructResultsCount}");
            break;
    }
}
```

<div class="result" markdown="1">

```text
ASK: True
DESCRIBE, triples: 6
SELECT, rows: 1
```

</div>

| Text | Parser | Gives |
|---|---|---|
| a query of known form | `RDFSelectQuery.FromString`, `RDFAskQuery.FromString`, `RDFConstructQuery.FromString`, `RDFDescribeQuery.FromString` | the typed query |
| a query of any form | `RDFQueryParserFactory.ParseQuery` | an `RDFQuery` to match on its type |

## A query with aggregates

Nothing changes for a harder question. The average score of each subzone, from the [Aggregates]({{ '/query/aggregates/' | relative_url }}) chapter, needs the catalog and the guides side by side, so it runs on a store. The text carries the `GROUP BY` and the `AVG`. The parser turns them into a group-by modifier with its aggregator:

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
catalog.SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore catalogStore = new RDFMemoryStore();
catalogStore.MergeGraph(catalog);
RDFMemoryStore guides = RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");
RDFMemoryStore store = catalogStore.UnionWith(guides);

RDFSelectQuery scorePerSubzone = RDFSelectQuery.FromString(@"
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?subzone (AVG(?score) AS ?averageScore)
WHERE {
  ?wine vr:producedBy ?producer ;
        vr:score ?score .
  ?producer vr:locatedIn ?subzone .
}
GROUP BY ?subzone");

RDFSelectQueryResult averaged = scorePerSubzone.ApplyToStore(store);
foreach (DataRow row in averaged.SelectResults.Rows)
    Console.WriteLine($"{row["?SUBZONE"]} | {row["?AVERAGESCORE"]}");
```

</div>

<div class="result" markdown="1">

| ?SUBZONE | ?AVERAGESCORE |
|---|---|
| `https://vallerosa.example/id/poggio` | `91.0^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/colle-nord` | `92.33333333333333333333333333^^http://www.w3.org/2001/XMLSchema#decimal` |
| `https://vallerosa.example/id/piana` | `86.66666666666666666666666667^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The numbers are the ones of the Aggregates chapter, where the same query is built in C#. Printing the parsed query shows the modifier and the aggregator that the text became:

```csharp
Console.WriteLine(scorePerSubzone);
```

<div class="result" data-label="Printed query" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

SELECT ?SUBZONE (AVG(?SCORE) AS ?AVERAGESCORE)
WHERE {
  {
    ?WINE vr:producedBy ?PRODUCER .
    ?WINE vr:score ?SCORE .
    ?PRODUCER vr:locatedIn ?SUBZONE .
  }
}
GROUP BY ?SUBZONE
```

</div>

## Relative IRIs and the base

SPARQL allows an IRI written relative to a base, such as `<rosso-2021>`. The base is declared in the text with `BASE`. When the text has none, the parser has nothing to resolve the name against and refuses it:

```csharp
try
{
    RDFAskQuery.FromString("ASK { <rosso-2021> ?property ?value }");
}
catch (RDFQueryException exception)
{
    Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
}
```

<div class="result" markdown="1">

```text
RDFQueryException: Cannot parse SPARQL term [LINE:1,COL:18]: Found relative Uri reference 'rosso-2021' with no usable base Uri to resolve it against[LINE:1,COL:18]
```

</div>

When the questions come from files that never declare a base, give the parser one through `RDFQueryOptions`. Its only property, `FallbackBaseUri`, is an absolute URI that the parser uses whenever the text does not declare a `BASE` itself. Every `FromString` takes the options as an optional last argument:

```csharp
RDFQueryOptions options = new RDFQueryOptions
{
    FallbackBaseUri = new Uri("https://vallerosa.example/id/")
};

RDFAskQuery relativeAsk = RDFAskQuery.FromString("ASK { <rosso-2021> ?property ?value }", options);
Console.WriteLine(relativeAsk);
Console.WriteLine(relativeAsk.ApplyToGraph(catalog).AskResult);
```

<div class="result" markdown="1">

```text
ASK
WHERE {
  {
    <https://vallerosa.example/id/rosso-2021> ?PROPERTY ?VALUE .
  }
}
True
```

</div>

The relative name became the full IRI of the wine and the query found it. A `BASE` written in the text always wins over the fallback. Here the fallback points elsewhere and the query still finds the wine:

```csharp
RDFQueryOptions elsewhere = new RDFQueryOptions
{
    FallbackBaseUri = new Uri("https://elsewhere.example/")
};

RDFAskQuery relativeAskWithBase = RDFAskQuery.FromString(
    "BASE <https://vallerosa.example/id/> ASK { <rosso-2021> ?property ?value }", elsewhere);
Console.WriteLine(relativeAskWithBase.ApplyToGraph(catalog).AskResult);
```

<div class="result" markdown="1">

```text
True
```

</div>

## Updates

The parser reads SPARQL Update too. Every operation class has its own `FromString`. `RDFOperationParserFactory.ParseOperation` is the counterpart of `ParseQuery` for text of unknown kind. An update is an `RDFOperation`: it applies to a graph or to a store and answers with an `RDFOperationResult`. The [Insert and delete]({{ '/query/insert-delete/' | relative_url }}) chapter builds each one in C#.

| Text | Parser | Gives |
|---|---|---|
| `INSERT DATA`, `DELETE DATA` | `RDFInsertDataOperation.FromString`, `RDFDeleteDataOperation.FromString` | the operation |
| `INSERT … WHERE`, `DELETE … WHERE`, `DELETE … INSERT … WHERE` | `RDFInsertWhereOperation.FromString`, `RDFDeleteWhereOperation.FromString`, `RDFDeleteInsertWhereOperation.FromString` | the operation |
| `LOAD`, `CLEAR`, `CREATE`, `DROP` | `RDFLoadOperation.FromString`, `RDFClearOperation.FromString`, `RDFCreateOperation.FromString`, `RDFDropOperation.FromString` | the operation |
| `COPY`, `MOVE`, `ADD` | `RDFCopyOperation.FromString`, `RDFMoveOperation.FromString`, `RDFAddOperation.FromString` | the operation |
| any single update | `RDFOperationParserFactory.ParseOperation` | an `RDFOperation` to match on its type |
| updates separated by `;` | `RDFOperationSet.FromString` | an `RDFOperationSet` |

A single update of unknown kind goes through the factory. The type of the object tells what it was:

```csharp
RDFOperation operation = RDFOperationParserFactory.ParseOperation(
    "INSERT DATA { <https://vallerosa.example/id/rosato-2024> <https://vallerosa.example/vocab#organic> true }");
Console.WriteLine(operation.GetType().Name);
```

<div class="result" markdown="1">

```text
RDFInsertDataOperation
```

</div>

The cooperative puts the Vermentino on promotion, from 13.00 euros to 12.00. In SPARQL Update that is two operations separated by a semicolon: first the old price goes, then the new one comes in. The text becomes an `RDFOperationSet`, which applies its operations in order and gives back one result for each:

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

DELETE WHERE { vrd:vermentino-2023 vr:price ?price } ;
INSERT DATA { vrd:vermentino-2023 vr:price 12.00 }
```

```csharp
RDFOperationSet promotion = RDFOperationSet.FromString(@"
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

DELETE WHERE { vrd:vermentino-2023 vr:price ?price } ;
INSERT DATA { vrd:vermentino-2023 vr:price 12.00 }");

IReadOnlyList<RDFOperationResult> outcomes = promotion.ApplyToGraph(catalog);
Console.WriteLine(catalog.TriplesCount);
foreach (RDFOperationResult outcome in outcomes)
    Console.WriteLine($"{outcome.DeleteResultsCount} deleted, {outcome.InsertResultsCount} inserted");
```

</div>

<div class="result" markdown="1">

```text
143
1 deleted, 0 inserted
0 deleted, 1 inserted
```

</div>

The graph still holds 143 triples, since one went out and one came in. Printing the set shows the two operations, each with its own prologue:

```csharp
Console.WriteLine(promotion);
```

<div class="result" data-label="Printed operation set" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

DELETE {
  vrd:vermentino-2023 vr:price ?PRICE .
}
WHERE {
  {
    vrd:vermentino-2023 vr:price ?PRICE .
  }
}
;
PREFIX vr: <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

INSERT DATA {
  vrd:vermentino-2023 vr:price "12.00"^^<http://www.w3.org/2001/XMLSchema#decimal> .
}
```

</div>

The operation sets are the subject of [Operation sets]({{ '/query/operation-sets/' | relative_url }}), where the failure of one operation of the chain is discussed.

## When the text is wrong

Every failure of the parser is an `RDFQueryException`. Its message says what the parser expected and where it stopped, with the line and the column in square brackets. A mistyped keyword:

```csharp
try
{
    RDFSelectQuery.FromString("SELEKT ?wine WHERE { ?wine ?property ?value }");
}
catch (RDFQueryException exception)
{
    Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
}
```

<div class="result" markdown="1">

```text
RDFQueryException: Cannot parse SPARQL query: unexpected token 'SELEKT' where a query form keyword was expected [LINE:1,COL:7]
```

</div>

A prefix that the text never declared:

```csharp
try
{
    RDFSelectQuery.FromString("SELECT ?wine WHERE { ?wine vr:price ?price }");
}
catch (RDFQueryException exception)
{
    Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
}
```

<div class="result" markdown="1">

```text
RDFQueryException: Cannot parse SPARQL term [LINE:1,COL:36]: Cannot resolve qname because its namespace prefix 'vr' was never declared[LINE:1,COL:36]
```

</div>

A group that is never closed:

```csharp
try
{
    RDFSelectQuery.FromString("SELECT ?wine WHERE { ?wine <https://vallerosa.example/vocab#price> ?price");
}
catch (RDFQueryException exception)
{
    Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
}
```

<div class="result" markdown="1">

```text
RDFQueryException: Cannot parse SPARQL WHERE clause: expected '}' but found end of input [LINE:1,COL:73]
```

</div>

A text that is correct but of another form than the one that the method expects:

```csharp
try
{
    RDFAskQuery.FromString("SELECT ?wine WHERE { ?wine ?property ?value }");
}
catch (RDFQueryException exception)
{
    Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
}
```

<div class="result" markdown="1">

```text
RDFQueryException: Cannot parse ASK query because the given command represents a different SPARQL query form (RDFSelectQuery)
```

</div>

An empty text gives `Cannot parse SPARQL query because the given command text is null or empty`. The query parser does not read updates: `INSERT DATA { ... }` handed to `ParseQuery` stops at the keyword, with `unexpected token 'INSERT' where a query form keyword was expected`. For a service that accepts text from people the exception is the validation: catch it and send the message back, since it already says where the mistake is.
