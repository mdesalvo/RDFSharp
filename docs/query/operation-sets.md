---
title: Operation sets
permalink: /query/operation-sets/
tagline: Several updates applied in order as one request
lede: An operation set chains update operations, runs them in order on the same data source and hands back one result for each. Sent to an endpoint it travels as a single request.
---

> It is the end of the year at Vallerosa. The guides of 2025 go to the archive, the 2022 vintage of the Rosso comes in, the rosé leaves the catalog and the prices rise by five percent: four jobs that make sense only together and in this order.
{: .story}

## One unit of work

A single operation is a good tool for a single change. The year-end rollover is made of several and each depends on what the one before left behind. An `RDFOperationSet` is an ordered list of operations: you add them one after the other with `AddOperation` and apply the set to a graph, to a store or to a SPARQL Update endpoint, exactly as you would an operation.

The operations are the ones of the two previous pages: [insert and delete]({{ '/query/insert-delete/' | relative_url }}) and [load and manage graphs]({{ '/query/graph-management/' | relative_url }}). In SPARQL Update the same thing is a request with several operations separated by a semicolon and an operation set prints itself that way.

The set below works on the store of the Store chapters: the catalog in its context, the two guides in theirs, 153 quadruples. Because a store has contexts, every pattern of the set names the one it works on, the `GRAPH <https://vallerosa.example/catalog>` of SPARQL.

```csharp
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFGraph catalog = (await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl"))
    .SetContext(new Uri("https://vallerosa.example/catalog"));
RDFMemoryStore catalogStore = new RDFMemoryStore();
await catalogStore.MergeGraphAsync(catalog);
RDFMemoryStore store = await catalogStore.UnionWithAsync(
    await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig"));

RDFContext catalogContext = new RDFContext("https://vallerosa.example/catalog");
Console.WriteLine(store.QuadruplesCount);
```

<div class="result" markdown="1">

```
153
```

</div>

## The year-end rollover

The rollover is four steps and five operations, in this order:

1. `MOVE` the 2025 edition of each guide to its archive context, which frees the original contexts for the 2026 editions: two operations
2. `INSERT DATA` the new vintage in the catalog
3. `DELETE … WHERE` the rosé in the catalog, with the three triples of its blend list
4. `DELETE … INSERT … WHERE` to raise every price of the catalog by five percent

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

MOVE <https://guidacalici.example/2025>
  TO <https://vallerosa.example/archive/guidacalici-2025> ;

MOVE <https://winescope.example/2025>
  TO <https://vallerosa.example/archive/winescope-2025> ;

INSERT DATA {
  GRAPH <https://vallerosa.example/catalog> {
    vrd:rosso-2022 a vr:RedWine ;
        rdfs:label "Vallerosa Red"@en ;
        vr:producedBy vrd:poggio-alto ;
        vr:vintage "2022"^^xsd:gYear ;
        vr:price 15.00 ;
        vr:grape vrd:sangiovese, vrd:canaiolo .
  }
} ;

DELETE {
  GRAPH <https://vallerosa.example/catalog> {
    vrd:rosato-2024 ?p ?o .
    ?list a ?listType ;
          rdf:first ?head ;
          rdf:rest ?tail .
  }
}
WHERE {
  GRAPH <https://vallerosa.example/catalog> {
    vrd:rosato-2024 ?p ?o ;
                    vr:blend ?list .
    ?list a ?listType ;
          rdf:first ?head ;
          rdf:rest ?tail .
  }
} ;

DELETE { GRAPH <https://vallerosa.example/catalog> { ?wine vr:price ?price } }
INSERT { GRAPH <https://vallerosa.example/catalog> { ?wine vr:price ?newPrice } }
WHERE {
  GRAPH <https://vallerosa.example/catalog> {
    ?wine vr:price ?price .
    BIND(?price * 1.05 AS ?newPrice)
  }
}
```

```csharp
RDFResource rosso2022 = new RDFResource(vrd + "rosso-2022");
RDFResource rosato = new RDFResource(vrd + "rosato-2024");
RDFResource priceProperty = new RDFResource(vr + "price");
RDFVariable wine = new RDFVariable("wine");
RDFVariable price = new RDFVariable("price");
RDFVariable newPrice = new RDFVariable("newPrice");
RDFVariable property = new RDFVariable("p");
RDFVariable value = new RDFVariable("o");
RDFVariable list = new RDFVariable("list");
RDFVariable listType = new RDFVariable("listType");
RDFVariable head = new RDFVariable("head");
RDFVariable tail = new RDFVariable("tail");

RDFMoveOperation archiveCalici = new RDFMoveOperation()
    .SetFromContext(new Uri("https://guidacalici.example/2025"))
    .SetToContext(new Uri("https://vallerosa.example/archive/guidacalici-2025"));
RDFMoveOperation archiveWinescope = new RDFMoveOperation()
    .SetFromContext(new Uri("https://winescope.example/2025"))
    .SetToContext(new Uri("https://vallerosa.example/archive/winescope-2025"));

RDFInsertDataOperation addVintage = new RDFInsertDataOperation()
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RedWine")))
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("Vallerosa Red", "en")))
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, new RDFResource(vr + "producedBy"), new RDFResource(vrd + "poggio-alto")))
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, new RDFResource(vr + "vintage"),
        new RDFTypedLiteral("2022", RDFModelEnums.RDFDatatypes.XSD_GYEAR)))
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, priceProperty,
        new RDFTypedLiteral("15.00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, new RDFResource(vr + "grape"), new RDFResource(vrd + "sangiovese")))
    .AddInsertTemplate(new RDFPattern(catalogContext, rosso2022, new RDFResource(vr + "grape"), new RDFResource(vrd + "canaiolo")));

RDFDeleteWhereOperation removeRosato = new RDFDeleteWhereOperation()
    .AddDeleteTemplate(new RDFPattern(catalogContext, rosato, property, value))
    .AddDeleteTemplate(new RDFPattern(catalogContext, list, RDFVocabulary.RDF.TYPE, listType))
    .AddDeleteTemplate(new RDFPattern(catalogContext, list, RDFVocabulary.RDF.FIRST, head))
    .AddDeleteTemplate(new RDFPattern(catalogContext, list, RDFVocabulary.RDF.REST, tail))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(catalogContext, rosato, property, value))
        .AddPattern(new RDFPattern(catalogContext, rosato, new RDFResource(vr + "blend"), list))
        .AddPattern(new RDFPattern(catalogContext, list, RDFVocabulary.RDF.TYPE, listType))
        .AddPattern(new RDFPattern(catalogContext, list, RDFVocabulary.RDF.FIRST, head))
        .AddPattern(new RDFPattern(catalogContext, list, RDFVocabulary.RDF.REST, tail)));

RDFDeleteInsertWhereOperation raisePrices = new RDFDeleteInsertWhereOperation()
    .AddDeleteTemplate(new RDFPattern(catalogContext, wine, priceProperty, price))
    .AddInsertTemplate(new RDFPattern(catalogContext, wine, priceProperty, newPrice))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(catalogContext, wine, priceProperty, price))
        .AddBind(new RDFBind(new RDFMultiplyExpression(price,
            new RDFTypedLiteral("1.05", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)), newPrice)));

RDFOperationSet rollover = new RDFOperationSet()
    .AddOperation(archiveCalici)
    .AddOperation(archiveWinescope)
    .AddOperation(addVintage)
    .AddOperation(removeRosato)
    .AddOperation(raisePrices);
```

</div>

Nothing has run yet: building the set only collects its operations, which stay available in `rollover.Operations`. Applying it runs them one after the other on the same store and returns the results in a list that follows the same order: the first result belongs to the first operation.

```csharp
IReadOnlyList<RDFOperationResult> results = await rollover.ApplyToStoreAsync(store);
for (int index = 0; index < results.Count; index++)
    Console.WriteLine($"{rollover.Operations[index].GetType().Name} {results[index].InsertResultsCount} {results[index].DeleteResultsCount}");
Console.WriteLine(store.QuadruplesCount);
foreach (RDFGraph graph in await store.ExtractGraphsAsync())
    Console.WriteLine($"{graph.Context} {graph.TriplesCount}");
```

<div class="result" markdown="1">

```

```

</div>

Each line reads: the operation, the quadruples it inserted and the quadruples it removed. The two `MOVE` operations insert and remove five each. The new vintage adds seven and the rosé takes away thirteen: its ten own triples and the three of its blend list. The price rise replaces six prices: the five wines left from before and the new one. This is what the order of the set does. The raise ran after the vintage came in, so it saw `vrd:rosso-2022` too and the new Rosso went from 15.00 to 15.75. Had the insertion been the last operation, it would have stayed at 15.00. The store ends with 147 quadruples: 153 plus 7 minus 13. Its contexts are the catalog and the two archives.

The results can be read like those of single operations: `results[3].DeleteResults` is the table of the triples the rosé took with it.

## Reading a set from SPARQL

A request written in SPARQL Update becomes a set with `RDFOperationSet.FromString`: it splits the semicolon-separated operations and keeps their order. A small request works well for a check.

```csharp
RDFOperationSet archiveOnly = RDFOperationSet.FromString(@"
    MOVE <https://guidacalici.example/2026> TO <https://vallerosa.example/archive/guidacalici-2026> ;
    MOVE <https://winescope.example/2026> TO <https://vallerosa.example/archive/winescope-2026>");
Console.WriteLine(archiveOnly.Operations.Count);
Console.WriteLine(archiveOnly);
```

<div class="result" markdown="1">

```
RDFMoveOperation 5 5
RDFMoveOperation 5 5
RDFInsertDataOperation 7 0
RDFDeleteWhereOperation 0 13
RDFDeleteInsertWhereOperation 6 6
147
https://vallerosa.example/catalog 137
https://vallerosa.example/archive/winescope-2025 5
https://vallerosa.example/archive/guidacalici-2025 5
```

</div>

The printed form is what an endpoint receives.

## On an endpoint

`rollover.ApplyToSPARQLUpdateEndpoint(endpoint)` sends the printed set as one request and returns `true` when the endpoint accepts it. Unlike the in-memory case, the endpoint decides whether the request is atomic: the server runs the chain as a unit and the library cannot give a `SILENT` operation its own error handling inside it. A failure of the request raises an `RDFQueryException`. The [endpoints]({{ '/query/endpoints/' | relative_url }}) page covers the options.

## When an operation fails

An operation set is not a transaction. If an operation throws, the ones before it have already changed the data source and stay changed: there is no rollback. Moving data between contexts makes no sense on a graph, so the set below fails at its second operation and shows what is left behind.

```csharp
RDFGraph scratch = new RDFGraph();
RDFOperationSet failing = new RDFOperationSet()
    .AddOperation(new RDFInsertDataOperation()
        .AddInsertTemplate(new RDFPattern(rosso2022, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RedWine"))))
    .AddOperation(archiveCalici);

try
{
    await failing.ApplyToGraphAsync(scratch);
}
catch (RDFQueryException)
{
    Console.WriteLine(scratch.TriplesCount);
}
```

<div class="result" markdown="1">

```
2
MOVE <https://guidacalici.example/2026> TO <https://vallerosa.example/archive/guidacalici-2026>
;
MOVE <https://winescope.example/2026> TO <https://vallerosa.example/archive/winescope-2026>
```

</div>

The insertion of the first operation survived the failure of the second. When you need all or nothing, work on a copy of the data source and swap it in when the whole set has succeeded.
