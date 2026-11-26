---
title: Query forms
permalink: /query/query-forms/
tagline: SELECT, ASK, CONSTRUCT and DESCRIBE
lede: A query can answer with a table, a yes or no, a brand new graph or the description of a resource. The form you choose decides what comes back and what you can do with it next.
---

> A partner shop wants the Vallerosa wines in its own catalog, written in the vocabulary that every shop on the Web understands. Before the export goes out, the cooperative checks a few facts about its catalog and learns to ask for the full description of one producer.
{: .story}

SPARQL has four **query forms**. They share everything you have learned so far (patterns, filters, modifiers) and differ in what they return. Every example of this page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}) and needs these namespaces of the library:

```csharp
using System.Data;
using System.IO;
using RDFSharp.Model;   // resources, literals, graphs
using RDFSharp.Query;   // the four query forms and their results
```

## SELECT: a table of answers

`RDFSelectQuery` is the form of the previous chapters: it returns a table with one column for each projected variable. [Variables and patterns]({{ '/query/patterns/' | relative_url }}) explains how to build one. As a reminder and as the starting point of this page, here are the organic white wines with their English name.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?wine ?label
WHERE {
  ?wine a vr:WhiteWine ;
        vr:organic true ;
        rdfs:label ?label .
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFResource whiteWine = new RDFResource(vr + "WhiteWine");
RDFResource isOrganic = new RDFResource(vr + "organic");

RDFTypedLiteral organicTrue = new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
RDFPlainLiteral englishTag = new RDFPlainLiteral("en");

RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");

RDFSelectQuery organicWhites = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, whiteWine))
        .AddPattern(new RDFPattern(wine, isOrganic, organicTrue))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag)))))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(label);

RDFSelectQueryResult organicWhitesResult = await organicWhites.ApplyToGraphAsync(catalog);
foreach (DataRow row in organicWhitesResult.SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?LABEL"]}");
```

</div>

<div class="result" markdown="1">

| ?WINE | ?LABEL |
|---|---|
| `https://vallerosa.example/id/bianco-2023` | `Le Crete White@EN` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino of the Plain@EN` |

</div>

The answer lives in `SelectResults`, a `DataTable`. `SelectResultsCount` tells how many rows it holds.

## ASK: a yes or no

Sometimes the cooperative does not want the wines, only to know whether there are any. `RDFAskQuery` has patterns and filters like a SELECT but no projection: the answer is a single `bool`, true when the patterns match at least once. Is there an organic white wine in the catalog?

<div class="pair" markdown="1">

```sparql
PREFIX vr: <https://vallerosa.example/vocab#>

ASK {
  ?wine a vr:WhiteWine ;
        vr:organic true .
}
```

```csharp
RDFAskQuery organicWhiteExists = new RDFAskQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, whiteWine))
        .AddPattern(new RDFPattern(wine, isOrganic, organicTrue)));

RDFAskQueryResult whiteAnswer = await organicWhiteExists.ApplyToGraphAsync(catalog);
Console.WriteLine(whiteAnswer.AskResult);
```

</div>

<div class="result" markdown="1">

```text
True
```

</div>

The result is an `RDFAskQueryResult` and its `AskResult` property holds the boolean. Changing the class of the wine to `vr:RoseWine` gives the other answer: the only rosé of the catalog is not organic.

```csharp
RDFResource roseWine = new RDFResource(vr + "RoseWine");

RDFAskQuery organicRoseExists = new RDFAskQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, roseWine))
        .AddPattern(new RDFPattern(wine, isOrganic, organicTrue)));

RDFAskQueryResult roseAnswer = await organicRoseExists.ApplyToGraphAsync(catalog);
Console.WriteLine(roseAnswer.AskResult);
```

<div class="result" markdown="1">

```text
False
```

</div>

## CONSTRUCT: a new graph

The partner shop does not know the `vr:` vocabulary. It reads **schema.org**, the vocabulary that search engines and shops share, where a wine is a `schema:Product` with a name, a manufacturer and an offer. `CONSTRUCT` reads the catalog like a SELECT and then writes triples instead of a table: for every match of the patterns it fills a **template** and keeps the result. The query is a translation from one vocabulary to the other.

| In the catalog | In the export |
|---|---|
| a wine | a `schema:Product` |
| its English label | `schema:name` of the product |
| its producer | `schema:manufacturer`, a resource with its own `schema:name` |
| its price | `schema:price` of a `schema:Offer`, linked from the product with `schema:offers` |
| nothing | `schema:priceCurrency "EUR"`, a constant of the export |

The library has no schema.org vocabulary built in, so the namespace is declared by hand. The offer is a new resource that the query mints for each wine, by appending `-offer` to the IRI of the wine with `BIND` (see [Bind and expressions]({{ '/query/expressions/' | relative_url }})). The templates go in with `AddTemplate`, the patterns that feed them in a pattern group as usual:

<div class="pair" markdown="1">

```sparql
PREFIX vr:     <https://vallerosa.example/vocab#>
PREFIX rdfs:   <http://www.w3.org/2000/01/rdf-schema#>
PREFIX schema: <https://schema.org/>

CONSTRUCT {
  ?wine a schema:Product ;
        schema:name ?label ;
        schema:manufacturer ?producer ;
        schema:offers ?offer .
  ?producer schema:name ?producerName .
  ?offer a schema:Offer ;
         schema:price ?price ;
         schema:priceCurrency "EUR" .
}
WHERE {
  ?wine rdfs:label ?label ;
        vr:price ?price ;
        vr:producedBy ?producer .
  ?producer rdfs:label ?producerName .
  FILTER (LANGMATCHES(LANG(?label), "en"))
  BIND (IRI(CONCAT(STR(?wine), "-offer")) AS ?offer)
}
```

```csharp
RDFNamespace schema = new RDFNamespace("schema", "https://schema.org/");

RDFResource producedBy = new RDFResource(vr + "producedBy");
RDFResource hasPrice = new RDFResource(vr + "price");
RDFResource schemaProduct = new RDFResource(schema + "Product");
RDFResource schemaOffer = new RDFResource(schema + "Offer");
RDFResource schemaName = new RDFResource(schema + "name");
RDFResource schemaManufacturer = new RDFResource(schema + "manufacturer");
RDFResource schemaOffers = new RDFResource(schema + "offers");
RDFResource schemaPrice = new RDFResource(schema + "price");
RDFResource schemaPriceCurrency = new RDFResource(schema + "priceCurrency");

RDFPlainLiteral euroCurrency = new RDFPlainLiteral("EUR");
RDFPlainLiteral offerSuffix = new RDFPlainLiteral("-offer");

RDFVariable producer = new RDFVariable("producer");
RDFVariable producerName = new RDFVariable("producerName");
RDFVariable price = new RDFVariable("price");
RDFVariable offer = new RDFVariable("offer");

RDFConstructQuery partnerExport = new RDFConstructQuery()
    .AddTemplate(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, schemaProduct))
    .AddTemplate(new RDFPattern(wine, schemaName, label))
    .AddTemplate(new RDFPattern(wine, schemaManufacturer, producer))
    .AddTemplate(new RDFPattern(wine, schemaOffers, offer))
    .AddTemplate(new RDFPattern(producer, schemaName, producerName))
    .AddTemplate(new RDFPattern(offer, RDFVocabulary.RDF.TYPE, schemaOffer))
    .AddTemplate(new RDFPattern(offer, schemaPrice, price))
    .AddTemplate(new RDFPattern(offer, schemaPriceCurrency, euroCurrency))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, hasPrice, price))
        .AddPattern(new RDFPattern(wine, producedBy, producer))
        .AddPattern(new RDFPattern(producer, RDFVocabulary.RDFS.LABEL, producerName))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag))))
        .AddBind(new RDFBind(new RDFIriExpression(new RDFConcatExpression(
            new RDFStrExpression(wine),
            new RDFConstantExpression(offerSuffix))), offer)));

RDFConstructQueryResult exported = await partnerExport.ApplyToGraphAsync(catalog);
Console.WriteLine(exported.ConstructResultsCount);
```

</div>

<div class="result" markdown="1">

```text
45
```

</div>

The `RDFConstructQueryResult` keeps the filled templates in the `ConstructResults` table, one row for each triple. Six wines make six products, six offers and three producers: 45 triples in all. A table of triples is not what a partner shop wants, so `ToRDFGraphAsync()` turns it into an `RDFGraph` and from there it is a file in any of the [RDF formats]({{ '/model/formats/' | relative_url }}). Register the namespaces first so that the prefixes appear. Give the graph a context with `SetContext`: it becomes the `@base` of the Turtle file.

```csharp
RDFNamespaceRegister.AddNamespace(schema);
RDFNamespaceRegister.AddNamespace(vr);
RDFNamespaceRegister.AddNamespace(vrd);

RDFGraph partner = (await exported.ToRDFGraphAsync())
    .SetContext(new Uri("https://vallerosa.example/export/partner"));
Console.WriteLine(partner.TriplesCount);

await partner.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "partner-export.ttl");
```

<div class="result" markdown="1">

```text
45
```

</div>

<div class="result" data-label="partner-export.ttl" markdown="1">

```turtle
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix schema: <https://schema.org/>.
@prefix vrd: <https://vallerosa.example/id/>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@base <https://vallerosa.example/export/partner>.

vrd:bianco-2023 a schema:Product; 
                schema:manufacturer vrd:le-crete; 
                schema:name "Le Crete White"@EN; 
                schema:offers vrd:bianco-2023-offer. 
vrd:bianco-2023-offer a schema:Offer; 
                      schema:price "11.00"^^xsd:decimal; 
                      schema:priceCurrency "EUR". 
vrd:le-crete schema:name "Tenuta Le Crete". 
vrd:poggio-alto schema:name "Fattoria Poggio Alto". 
vrd:riserva-2019 a schema:Product; 
                 schema:manufacturer vrd:poggio-alto; 
                 schema:name "Vallerosa Reserve"@EN; 
                 schema:offers vrd:riserva-2019-offer. 
vrd:riserva-2019-offer a schema:Offer; 
                       schema:price "32.00"^^xsd:decimal; 
                       schema:priceCurrency "EUR". 
vrd:rosato-2024 a schema:Product; 
                schema:manufacturer vrd:tre-ulivi; 
                schema:name "Valley Rosé"@EN; 
                schema:offers vrd:rosato-2024-offer. 
vrd:rosato-2024-offer a schema:Offer; 
                      schema:price "10.00"^^xsd:decimal; 
                      schema:priceCurrency "EUR". 
vrd:rosso-2021 a schema:Product; 
               schema:manufacturer vrd:poggio-alto; 
               schema:name "Vallerosa Red"@EN; 
               schema:offers vrd:rosso-2021-offer. 
vrd:rosso-2021-offer a schema:Offer; 
                     schema:price "14.50"^^xsd:decimal; 
                     schema:priceCurrency "EUR". 
vrd:tre-ulivi schema:name "Cantina dei Tre Ulivi". 
vrd:vermentino-2023 a schema:Product; 
                    schema:manufacturer vrd:tre-ulivi; 
                    schema:name "Vermentino of the Plain"@EN; 
                    schema:offers vrd:vermentino-2023-offer. 
vrd:vermentino-2023-offer a schema:Offer; 
                          schema:price "13.00"^^xsd:decimal; 
                          schema:priceCurrency "EUR". 
vrd:vinsanto-2015 a schema:Product; 
                  schema:manufacturer vrd:le-crete; 
                  schema:name "Valley Vin Santo"@EN; 
                  schema:offers vrd:vinsanto-2015-offer. 
vrd:vinsanto-2015-offer a schema:Offer; 
                        schema:price "38.00"^^xsd:decimal; 
                        schema:priceCurrency "EUR". 
```

</div>

Each wine is now a product with a manufacturer and an offer. The producers carry their names. Nothing of the `vr:` vocabulary is left: the shop can load the file without knowing that Vallerosa exists.

## DESCRIBE: everything about one resource

`CONSTRUCT` makes you choose the triples. `DESCRIBE` leaves the choice to the engine: you name a resource and you get back what the data says about it. RDFSharp returns every triple that has the resource as subject or as object. The cooperative asks for the description of Fattoria Poggio Alto.

<div class="pair" markdown="1">

```sparql
PREFIX vrd: <https://vallerosa.example/id/>

DESCRIBE vrd:poggio-alto
```

```csharp
RDFResource poggioAlto = new RDFResource(vrd + "poggio-alto");

RDFDescribeQuery aboutPoggioAlto = new RDFDescribeQuery()
    .AddDescribeTerm(poggioAlto);

RDFDescribeQueryResult described = await aboutPoggioAlto.ApplyToGraphAsync(catalog);
Console.WriteLine(described.DescribeResultsCount);

RDFGraph producerGraph = (await described.ToRDFGraphAsync())
    .SetContext(new Uri("https://vallerosa.example/export/poggio-alto"));
await producerGraph.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "poggio-alto.ttl");
```

</div>

<div class="result" markdown="1">

```text
6
```

</div>

<div class="result" data-label="poggio-alto.ttl" markdown="1">

```turtle
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix vrd: <https://vallerosa.example/id/>.
@base <https://vallerosa.example/export/poggio-alto>.

vrd:poggio-alto a vr:Producer; 
                rdfs:label "Fattoria Poggio Alto"; 
                vr:locatedIn vrd:poggio; 
                vr:memberOf vrd:vallerosa. 
vrd:riserva-2019 vr:producedBy vrd:poggio-alto. 
vrd:rosso-2021 vr:producedBy vrd:poggio-alto. 
```

</div>

The first four triples are what the catalog says about the producer: its class, its name, where it is and the cooperative it belongs to. The last two are what the catalog says of it from the other side, the two wines that name it as their producer. `RDFDescribeQueryResult` keeps them in `DescribeResults` and gives the same `ToRDFGraphAsync()` as a CONSTRUCT.

The term to describe can also be a variable that patterns bind, as in `DESCRIBE ?producer WHERE { ... }`. Describing the producers of the Hilltop subzone gives the same six rows, because only one producer sits there:

```csharp
RDFResource locatedIn = new RDFResource(vr + "locatedIn");

RDFResource poggio = new RDFResource(vrd + "poggio");

RDFDescribeQuery aboutHilltop = new RDFDescribeQuery()
    .AddDescribeTerm(producer)
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(producer, locatedIn, poggio)));

Console.WriteLine((await aboutHilltop.ApplyToGraphAsync(catalog)).DescribeResultsCount);
```

<div class="result" markdown="1">

```text
6
```

</div>

## Exchanging results: the SPARQL Query Results XML Format

CONSTRUCT and DESCRIBE return graphs, which travel in the RDF formats. The answers of a SELECT and of an ASK are not graphs: they are a table and a boolean that need a format of their own. The W3C standardized it as the **SPARQL Query Results XML Format**. It is how RDFSharp exchanges SPARQL results with other tools and with remote endpoints: a result written by the library opens in any SPARQL tool and a result produced by a triple store reads back into a `DataTable`. It is the only results format that the library writes and reads: there is no JSON or CSV serializer for results.

Both result types have the same four methods, for a file path and for a stream, each with an `Async` twin.

| Method | On | Does |
|---|---|---|
| `ToSparqlXmlResult(string filepath)` | `RDFSelectQueryResult`, `RDFAskQueryResult` | writes the result to a file |
| `ToSparqlXmlResult(Stream outputStream)` | `RDFSelectQueryResult`, `RDFAskQueryResult` | writes the result to a stream |
| `FromSparqlXmlResult(string filepath)` | `RDFSelectQueryResult`, `RDFAskQueryResult` | reads a result from a file |
| `FromSparqlXmlResult(Stream inputStream)` | `RDFSelectQueryResult`, `RDFAskQueryResult` | reads a result from a stream |

The partner shop wants the list of organic whites too. The result of the SELECT of this page goes to a file:

```csharp
await organicWhitesResult.ToSparqlXmlResultAsync("organic-whites.srx");
```

<div class="result" data-label="organic-whites.srx" markdown="1">

```xml
<?xml version="1.0" encoding="utf-8"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
  <head>
    <variable name="WINE" />
    <variable name="LABEL" />
  </head>
  <results>
    <result>
      <binding name="WINE">
        <uri>https://vallerosa.example/id/bianco-2023</uri>
      </binding>
      <binding name="LABEL">
        <literal xml:lang="EN">Le Crete White</literal>
      </binding>
    </result>
    <result>
      <binding name="WINE">
        <uri>https://vallerosa.example/id/vermentino-2023</uri>
      </binding>
      <binding name="LABEL">
        <literal xml:lang="EN">Vermentino of the Plain</literal>
      </binding>
    </result>
  </results>
</sparql>
```

</div>

The `head` lists the variables and each `result` carries one `binding` for each of them: a `uri` for a resource and a `literal` for a value, with its `datatype` or its `xml:lang`. Reading the file gives back a table with the same columns and the same rows:

```csharp
RDFSelectQueryResult reread = await RDFSelectQueryResult.FromSparqlXmlResultAsync("organic-whites.srx");
foreach (DataRow row in reread.SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?LABEL"]}");
```

<div class="result" markdown="1">

| ?WINE | ?LABEL |
|---|---|
| `https://vallerosa.example/id/bianco-2023` | `Le Crete White@EN` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino of the Plain@EN` |

</div>

An ASK result is the same document with a `boolean` in place of the `results`. It reads back as an `RDFAskQueryResult`:

```csharp
await whiteAnswer.ToSparqlXmlResultAsync("organic-white-exists.srx");
bool again = (await RDFAskQueryResult.FromSparqlXmlResultAsync("organic-white-exists.srx")).AskResult;
Console.WriteLine(again);
```

<div class="result" markdown="1">

```text
True
```

</div>

A stream is how a result travels without touching the disk, for example in the body of an HTTP response. The writer closes the stream that it is given, so take the bytes out as soon as it returns:

```csharp
byte[] payload;
using (MemoryStream buffer = new MemoryStream())
{
    await organicWhitesResult.ToSparqlXmlResultAsync(buffer);
    payload = buffer.ToArray();
}

RDFSelectQueryResult received = await RDFSelectQueryResult.FromSparqlXmlResultAsync(new MemoryStream(payload));
Console.WriteLine(received.SelectResultsCount);
```

<div class="result" markdown="1">

```text
2
```

</div>

When the library queries a remote service, it asks for this format in the request and reads the answer with `FromSparqlXmlResult`: the [SPARQL endpoints]({{ '/query/endpoints/' | relative_url }}) chapter builds on that.

## What each form returns

| Form | Class | Result | Where the answer is | As a graph |
|---|---|---|---|---|
| SELECT | `RDFSelectQuery` | `RDFSelectQueryResult` | `SelectResults`, a table | no |
| ASK | `RDFAskQuery` | `RDFAskQueryResult` | `AskResult`, a `bool` | no |
| CONSTRUCT | `RDFConstructQuery` | `RDFConstructQueryResult` | `ConstructResults`, a table of triples | `ToRDFGraphAsync()` |
| DESCRIBE | `RDFDescribeQuery` | `RDFDescribeQueryResult` | `DescribeResults`, a table of triples | `ToRDFGraphAsync()` |

Every form applies to a graph with `ApplyToGraphAsync`, to a store with `ApplyToStoreAsync`, to a federation with `ApplyToFederationAsync` and to a SPARQL endpoint with `ApplyToSPARQLEndpointAsync`. The next chapter shows how to build any of the four from SPARQL text instead of C#.
