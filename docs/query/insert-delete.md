---
title: Insert and delete
permalink: /query/insert-delete/
tagline: Changing the catalog with SPARQL Update, one operation at a time
lede: A query reads data and an update operation changes it. Five operations add and remove triples, from a fixed list of facts up to a rule applied to every match.
---

> The 2022 vintage of the Rosso reaches the cellar, the rosé is sold out and the cooperative raises its prices by five percent. Each change is one update operation, run on the catalog and answered with the list of what it did.
{: .story}

## Operations and their results

SPARQL Update is the half of SPARQL that writes. RDFSharp models every update as a class that derives from `RDFOperation`, built with the same fluent calls as a query and it can be applied to a graph, to a store or to a SPARQL Update endpoint. A federation is read-only, so it accepts no operation.

This page covers the five operations that add and remove triples. The ones that move whole graphs around are on the [next page]({{ '/query/graph-management/' | relative_url }}).

| SPARQL Update | RDFSharp class | What it does |
|---|---|---|
| `INSERT DATA` | `RDFInsertDataOperation` | adds a fixed list of triples |
| `INSERT … WHERE` | `RDFInsertWhereOperation` | adds the triples that a template builds for every match of a pattern |
| `DELETE DATA` | `RDFDeleteDataOperation` | removes a fixed list of triples |
| `DELETE … WHERE` | `RDFDeleteWhereOperation` | removes the triples that a template builds for every match of a pattern |
| `DELETE … INSERT … WHERE` | `RDFDeleteInsertWhereOperation` | does both for the same matches: delete first and insert after |

Applied to a graph or a store, an operation returns an `RDFOperationResult` with two tables: `DeleteResults` holds the triples that were removed and `InsertResults` the triples that were added, with their counts in `DeleteResultsCount` and `InsertResultsCount`. A triple that was already there is not added again and a triple that was not there is not removed, so neither shows up in the result. Every operation also reads back from its SPARQL text with its static `FromString` method, which is how the SPARQL side of each example below was checked against the C# side.

The examples run on the catalog as a graph, in order, each one starting from what the one before left behind.

```csharp
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFGraph catalog = (await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl"))
    .SetContext(new Uri("https://vallerosa.example/catalog"));
Console.WriteLine(catalog.TriplesCount);   // 143
```

## Insert data

`INSERT DATA` takes triples written out in full, with no variables. The new vintage of the Rosso is `vrd:rosso-2022`: same producer as the 2021, 13.5 degrees, 15.00 euros, not organic, Sangiovese and Canaiolo.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX vrd:  <https://vallerosa.example/id/>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX xsd:  <http://www.w3.org/2001/XMLSchema#>

INSERT DATA {
  vrd:rosso-2022 a vr:RedWine ;
      rdfs:label "Rosso di Vallerosa"@it, "Vallerosa Red"@en ;
      vr:producedBy vrd:poggio-alto ;
      vr:vintage "2022"^^xsd:gYear ;
      vr:alcohol 13.5 ;
      vr:price 15.00 ;
      vr:organic false ;
      vr:grape vrd:sangiovese, vrd:canaiolo .
}
```

```csharp
RDFResource rosso2022 = new RDFResource(vrd + "rosso-2022");

RDFInsertDataOperation insertVintage = new RDFInsertDataOperation()
    .AddPrefix(vr)
    .AddPrefix(vrd)
    .AddInsertTemplate(new RDFPattern(rosso2022, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RedWine")))
    .AddInsertTemplate(new RDFPattern(rosso2022, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Rosso di Vallerosa", "it")))
    .AddInsertTemplate(new RDFPattern(rosso2022, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Vallerosa Red", "en")))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "producedBy"), new RDFResource(vrd + "poggio-alto")))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "vintage"),
        new RDFTypedLiteral("2022", RDFModelEnums.RDFDatatypes.XSD_GYEAR)))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "alcohol"),
        new RDFTypedLiteral("13.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "price"),
        new RDFTypedLiteral("15.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "organic"),
        new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "grape"), new RDFResource(vrd + "sangiovese")))
    .AddInsertTemplate(new RDFPattern(rosso2022, new RDFResource(vr + "grape"), new RDFResource(vrd + "canaiolo")));

RDFOperationResult vintageResult = await insertVintage.ApplyToGraphAsync(catalog);
Console.WriteLine($"{vintageResult.InsertResultsCount} {catalog.TriplesCount}");
foreach (DataRow row in vintageResult.InsertResults.Rows)
    Console.WriteLine($"{row["?PREDICATE"]} | {row["?OBJECT"]}");
Console.WriteLine((await insertVintage.ApplyToGraphAsync(catalog)).InsertResultsCount);
```

</div>

<div class="result" markdown="1">

```
10 153
http://www.w3.org/1999/02/22-rdf-syntax-ns#type | https://vallerosa.example/vocab#RedWine
http://www.w3.org/2000/01/rdf-schema#label | Rosso di Vallerosa@IT
http://www.w3.org/2000/01/rdf-schema#label | Vallerosa Red@EN
https://vallerosa.example/vocab#producedBy | https://vallerosa.example/id/poggio-alto
https://vallerosa.example/vocab#vintage | 2022Z^^http://www.w3.org/2001/XMLSchema#gYear
https://vallerosa.example/vocab#alcohol | 13.5^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/vocab#price | 15.00^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/vocab#organic | false^^http://www.w3.org/2001/XMLSchema#boolean
https://vallerosa.example/vocab#grape | https://vallerosa.example/id/sangiovese
https://vallerosa.example/vocab#grape | https://vallerosa.example/id/canaiolo
0
```

</div>

Ten triples went in and the catalog grew from 143 to 153. The last line applies the same operation a second time: nothing is added, because a graph is a set and every one of the ten triples is already there. The blend of the new wine is left out on purpose: an RDF list is made of blank nodes and `INSERT DATA` accepts only ground triples.

## Insert where

`INSERT … WHERE` finds its matches first and builds the new triples from them. Poggio Alto wants every one of its wines to carry a pairing: grilled steak, a new dish of the catalog (`vrd:grilled-steak`). The pattern finds the wines of the producer and the template says what to add for each of them.

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

INSERT {
  vrd:grilled-steak a vr:Dish .
  ?wine vr:pairsWith vrd:grilled-steak .
}
WHERE {
  ?wine vr:producedBy vrd:poggio-alto .
}
```

```csharp
RDFResource grilledSteak = new RDFResource(vrd + "grilled-steak");
RDFVariable wine = new RDFVariable("wine");

RDFInsertWhereOperation pairWithSteak = new RDFInsertWhereOperation()
    .AddPrefix(vr)
    .AddPrefix(vrd)
    .AddInsertTemplate(new RDFPattern(grilledSteak, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "Dish")))
    .AddInsertTemplate(new RDFPattern(wine, new RDFResource(vr + "pairsWith"), grilledSteak))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "producedBy"), new RDFResource(vrd + "poggio-alto"))));

RDFOperationResult pairingResult = await pairWithSteak.ApplyToGraphAsync(catalog);
Console.WriteLine($"{pairingResult.InsertResultsCount} {catalog.TriplesCount}");
foreach (DataRow row in pairingResult.InsertResults.Rows)
    Console.WriteLine($"{row["?SUBJECT"]} | {row["?PREDICATE"]} | {row["?OBJECT"]}");
```

</div>

<div class="result" markdown="1">

```
4 157
https://vallerosa.example/id/grilled-steak | http://www.w3.org/1999/02/22-rdf-syntax-ns#type | https://vallerosa.example/vocab#Dish
https://vallerosa.example/id/rosso-2021 | https://vallerosa.example/vocab#pairsWith | https://vallerosa.example/id/grilled-steak
https://vallerosa.example/id/riserva-2019 | https://vallerosa.example/vocab#pairsWith | https://vallerosa.example/id/grilled-steak
https://vallerosa.example/id/rosso-2022 | https://vallerosa.example/vocab#pairsWith | https://vallerosa.example/id/grilled-steak
```

</div>

The pattern matches three wines: the two that were there and the 2022 that came in a moment ago. The template without variables, the one that declares the dish, is added once. Four triples in total.

## Delete data

`DELETE DATA` is the mirror of `INSERT DATA`: triples written out in full. The Valley Rosé is almost gone and the cooperative stops quoting its price.

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

DELETE DATA {
  vrd:rosato-2024 vr:price 10.00 .
}
```

```csharp
RDFResource rosato = new RDFResource(vrd + "rosato-2024");

RDFDeleteDataOperation stopQuoting = new RDFDeleteDataOperation()
    .AddPrefix(vr)
    .AddPrefix(vrd)
    .AddDeleteTemplate(new RDFPattern(rosato, new RDFResource(vr + "price"),
        new RDFTypedLiteral("10.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));

RDFOperationResult quotingResult = await stopQuoting.ApplyToGraphAsync(catalog);
Console.WriteLine($"{quotingResult.DeleteResultsCount} {catalog.TriplesCount}");
```

</div>

<div class="result" markdown="1">

```
1 156
```

</div>

The triple must match exactly: `10.00` is a decimal of that lexical form and a price written `10` or `"10.00"` as a plain string would remove nothing.

## Delete where

Then the rosé sells out for good and the whole wine has to leave the catalog. `DELETE … WHERE` removes whatever its template builds from the matches. Here the template names two things: every remaining property of the rosé and the one-item list that holds its blend, which sits in three triples of blank nodes.

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

DELETE {
  vrd:rosato-2024 ?p ?o .
  ?list a ?listType ;
        rdf:first ?head ;
        rdf:rest ?tail .
}
WHERE {
  vrd:rosato-2024 ?p ?o ;
                  vr:blend ?list .
  ?list a ?listType ;
        rdf:first ?head ;
        rdf:rest ?tail .
}
```

```csharp
RDFVariable property = new RDFVariable("p");
RDFVariable value = new RDFVariable("o");
RDFVariable list = new RDFVariable("list");
RDFVariable listType = new RDFVariable("listType");
RDFVariable head = new RDFVariable("head");
RDFVariable tail = new RDFVariable("tail");

RDFDeleteWhereOperation forgetRosato = new RDFDeleteWhereOperation()
    .AddPrefix(vr)
    .AddPrefix(vrd)
    .AddDeleteTemplate(new RDFPattern(rosato, property, value))
    .AddDeleteTemplate(new RDFPattern(list, RDFVocabulary.RDF.TYPE, listType))
    .AddDeleteTemplate(new RDFPattern(list, RDFVocabulary.RDF.FIRST, head))
    .AddDeleteTemplate(new RDFPattern(list, RDFVocabulary.RDF.REST, tail))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(rosato, property, value))
        .AddPattern(new RDFPattern(rosato, new RDFResource(vr + "blend"), list))
        .AddPattern(new RDFPattern(list, RDFVocabulary.RDF.TYPE, listType))
        .AddPattern(new RDFPattern(list, RDFVocabulary.RDF.FIRST, head))
        .AddPattern(new RDFPattern(list, RDFVocabulary.RDF.REST, tail)));

RDFOperationResult forgetResult = await forgetRosato.ApplyToGraphAsync(catalog);
Console.WriteLine($"{forgetResult.DeleteResultsCount} {catalog.TriplesCount}");
foreach (DataRow row in forgetResult.DeleteResults.Rows)
    Console.WriteLine(row["?PREDICATE"]);
```

</div>

<div class="result" markdown="1">

```
12 144
http://www.w3.org/1999/02/22-rdf-syntax-ns#type
http://www.w3.org/2000/01/rdf-schema#label
http://www.w3.org/2000/01/rdf-schema#label
https://vallerosa.example/vocab#producedBy
https://vallerosa.example/vocab#vintage
https://vallerosa.example/vocab#alcohol
https://vallerosa.example/vocab#organic
https://vallerosa.example/vocab#grape
https://vallerosa.example/vocab#blend
http://www.w3.org/1999/02/22-rdf-syntax-ns#type
http://www.w3.org/1999/02/22-rdf-syntax-ns#first
http://www.w3.org/1999/02/22-rdf-syntax-ns#rest
```

</div>

Twelve triples left: nine that belonged to the wine directly and three that belonged to its list. The blank node has a different name on every load, so the list could not be removed by naming it: the pattern reaches it through `vr:blend` instead. SPARQL also has a short form, `DELETE WHERE { vrd:rosato-2024 ?p ?o }`, where the pattern doubles as the template. `RDFDeleteWhereOperation.FromString` reads it and gives the same operation as the long form.

## Delete and insert where

Prices rise by five percent. A price cannot be edited, only replaced: the old triple goes and a new one with the new value comes in. `DELETE … INSERT … WHERE` does both in one operation, from the same matches, deleting first. The new value is computed in the pattern with a `BIND`.

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

DELETE { ?wine vr:price ?price }
INSERT { ?wine vr:price ?newPrice }
WHERE {
  ?wine vr:price ?price .
  BIND(?price * 1.05 AS ?newPrice)
}
```

```csharp
RDFResource priceProperty = new RDFResource(vr + "price");
RDFVariable price = new RDFVariable("price");
RDFVariable newPrice = new RDFVariable("newPrice");

RDFDeleteInsertWhereOperation raisePrices = new RDFDeleteInsertWhereOperation()
    .AddPrefix(vr)
    .AddDeleteTemplate(new RDFPattern(wine, priceProperty, price))
    .AddInsertTemplate(new RDFPattern(wine, priceProperty, newPrice))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, priceProperty, price))
        .AddBind(new RDFBind(new RDFMultiplyExpression(price,
            new RDFTypedLiteral("1.05", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)), newPrice)));

RDFOperationResult raiseResult = await raisePrices.ApplyToGraphAsync(catalog);
Console.WriteLine($"{raiseResult.DeleteResultsCount} {raiseResult.InsertResultsCount} {catalog.TriplesCount}");
foreach (DataRow row in raiseResult.InsertResults.Rows)
    Console.WriteLine($"{row["?SUBJECT"]} | {row["?OBJECT"]}");
Console.WriteLine(raisePrices);
```

</div>

<div class="result" markdown="1">

```
6 6 144
https://vallerosa.example/id/rosso-2021 | 15.225^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/id/riserva-2019 | 33.6^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/id/bianco-2023 | 11.55^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/id/vinsanto-2015 | 39.9^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/id/vermentino-2023 | 13.65^^http://www.w3.org/2001/XMLSchema#decimal
https://vallerosa.example/id/rosso-2022 | 15.75^^http://www.w3.org/2001/XMLSchema#decimal
PREFIX vr: <https://vallerosa.example/vocab#>

DELETE {
  ?WINE vr:price ?PRICE .
}
INSERT {
  ?WINE vr:price ?NEWPRICE .
}
WHERE {
  {
    ?WINE vr:price ?PRICE .
    BIND((?PRICE * 1.05) AS ?NEWPRICE) .
  }
}
```

</div>

Six prices out and six in, so the catalog holds the same 144 triples as before. The Vallerosa Red goes from 14.50 to 15.225: the product is exact and rounding it to the cent is a choice the cooperative makes when it prints the price list. The last line shows that an operation prints itself as SPARQL Update, with the variable names in upper case as the engine writes them.

## The same on a store

On a store every triple belongs to a context, so a pattern can name the context with a fourth term, a variable or a fixed one: the `GRAPH ?guide { … }` of SPARQL. With the rosé gone, the scores it received have no wine to talk about. The store is built as in the [Store chapters]({{ '/store/stores/' | relative_url }}): the original catalog merged into its context and the guides read from their file.

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

DELETE WHERE {
  GRAPH ?guide { vrd:rosato-2024 vr:score ?score }
}
```

```csharp
RDFMemoryStore catalogStore = new RDFMemoryStore();
await catalogStore.MergeGraphAsync((await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl"))
    .SetContext(new Uri("https://vallerosa.example/catalog")));
RDFMemoryStore store = await catalogStore.UnionWithAsync(
    await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig"));

RDFVariable guide = new RDFVariable("guide");
RDFVariable score = new RDFVariable("score");
RDFResource scoreProperty = new RDFResource(vr + "score");

RDFDeleteWhereOperation forgetScores = new RDFDeleteWhereOperation()
    .AddPrefix(vr)
    .AddPrefix(vrd)
    .AddDeleteTemplate(new RDFPattern(guide, rosato, scoreProperty, score))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(guide, rosato, scoreProperty, score)));

RDFOperationResult scoresResult = await forgetScores.ApplyToStoreAsync(store);
Console.WriteLine($"{scoresResult.DeleteResultsCount} {store.QuadruplesCount}");
foreach (DataRow row in scoresResult.DeleteResults.Rows)
    Console.WriteLine($"{row["?CONTEXT"]} | {row["?OBJECT"]}");
```

</div>

<div class="result" markdown="1">

```
1 152
https://winescope.example/2025 | 84^^http://www.w3.org/2001/XMLSchema#integer
```

</div>

The result tables of a store carry a `?CONTEXT` column as well. Only WineScope had tasted the rosé, so one quadruple goes: the store drops from 153 to 152. On a store a template or a pattern without a context belongs to the default context of the library, which is not the one of the catalog, so name the context whenever you work on a store.

> Every operation also has the synchronous forms `ApplyToGraph` and `ApplyToStore`. Against a SPARQL Update endpoint it becomes a request instead of a result: the [endpoints]({{ '/query/endpoints/' | relative_url }}) page explains how.
{: .tip}
