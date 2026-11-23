---
title: Federations
permalink: /query/federations/
tagline: Several sources, one question
lede: A federation keeps data sources logically separate and lets you query them as one. Each source stays where it is with its own owner while a single question reaches all of them.
---

> The wines of Vallerosa are described in the catalog and scored by the guides, two sources that no single query reaches. Then the cooperative wants to know where its grapes come from. The catalog links each of them to DBpedia and only DBpedia knows the answer.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}) and [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}), loaded as in [Memory stores]({{ '/store/stores/' | relative_url }}). At the end it reaches out to DBpedia, which needs a network connection. It uses the same namespaces of the library as the previous pages:

```csharp
using System.Data;
using RDFSharp.Model;   // resources, literals, graphs
using RDFSharp.Query;   // federations, endpoints, queries
using RDFSharp.Store;   // memory stores
```

## Why a federation

The obvious way to answer a question across several sources is to copy them into one graph. That works until the sources stop agreeing on who owns what. At Vallerosa the catalog graph belongs to the cooperative and changes at every harvest. The guides store belongs to the two guides and is re-issued every year. DBpedia belongs to its own community and changes every day. Merged into one graph they would share a single lifecycle: someone would have to refresh the copy and the origin of each fact would blur.

A federation takes the opposite road. The sources stay logically separate, each in its own place and under its own owner. The federation is only a list of them, so the cooperative can still ask one question across all of them as if they were a single source. Reach for a federation when the data has different owners or different rhythms and you want one query over it all. Merge into one graph when the data is yours and moves together.

## One question, two sources

A **federation** is an `RDFFederation`: you build it empty and add the members one by one. A member can be a graph, a store, another federation or a SPARQL endpoint. The first two are what the cooperative has: the catalog in a graph and the two guides in a store.

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");
RDFMemoryStore guides = await RDFMemoryStore.FromFileAsync(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");

RDFFederation cellar = new RDFFederation()
    .AddGraph(catalog)
    .AddStore(guides);
Console.WriteLine(cellar.DataSourcesCount);
```

<div class="result" markdown="1">

```
2
```

</div>

`DataSourcesEnumerator` walks the members of a federation one by one.

The question is: which wines have an English label and what scores did the guides give them? The label lives in the catalog and the score in the guides. A federation has no query language of its own: you write an ordinary query and apply it with `ApplyToFederationAsync`.

```csharp
RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");
RDFVariable score = new RDFVariable("score");

RDFSelectQuery scores = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "score"), score))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(new RDFPlainLiteral("en"))))))
    .AddProjectionVariable(wine)
    .AddProjectionVariable(label)
    .AddProjectionVariable(score)
    .AddModifier(new RDFOrderByModifier(label, RDFQueryEnums.RDFOrderByFlavors.ASC))
    .AddModifier(new RDFOrderByModifier(score, RDFQueryEnums.RDFOrderByFlavors.ASC));

RDFSelectQueryResult answer = await scores.ApplyToFederationAsync(cellar);
foreach (DataRow row in answer.SelectResults.Rows)
    Console.WriteLine($"{row["?WINE"]} | {row["?LABEL"]} | {row["?SCORE"]}");
```

<div class="result" markdown="1">

| ?WINE | ?LABEL | ?SCORE |
|---|---|---|
| `https://vallerosa.example/id/bianco-2023` | `Le Crete White@EN` | `86^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/rosso-2021` | `Vallerosa Red@EN` | `88^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/rosso-2021` | `Vallerosa Red@EN` | `90^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/riserva-2019` | `Vallerosa Reserve@EN` | `92^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/riserva-2019` | `Vallerosa Reserve@EN` | `94^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/rosato-2024` | `Valley Rosé@EN` | `84^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vinsanto-2015` | `Valley Vin Santo@EN` | `95^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vinsanto-2015` | `Valley Vin Santo@EN` | `96^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino of the Plain@EN` | `87^^http://www.w3.org/2001/XMLSchema#integer` |
| `https://vallerosa.example/id/vermentino-2023` | `Vermentino of the Plain@EN` | `89^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

Ten rows: one for each score of the guides. Four wines are scored by both guides and two by a single guide. Neither source can say as much by itself. Ask the same query of the catalog alone or of the guides alone and each answers with nothing:

```csharp
Console.WriteLine((await scores.ApplyToGraphAsync(catalog)).SelectResultsCount);
Console.WriteLine((await scores.ApplyToStoreAsync(guides)).SelectResultsCount);
```

<div class="result" markdown="1">

```
0
0
```

</div>

The catalog has labels and no scores and the guides have scores and no labels. The federation is what brings the two halves of the question together.

## Federations of federations

A federation is a data source like the others, so it can be a member of another federation with `AddFederation`. Each team of the cooperative can keep its own and a larger one can gather them.

```csharp
RDFFederation yearbook = new RDFFederation().AddFederation(cellar);
Console.WriteLine(yearbook.DataSourcesCount);
Console.WriteLine((await scores.ApplyToFederationAsync(yearbook)).SelectResultsCount);
cellar.AddFederation(yearbook);
Console.WriteLine(cellar.DataSourcesCount);
```

<div class="result" markdown="1">

```
1
10
2
```

</div>

The yearbook has one member and answers with the same ten rows, because it passes the question on to the `cellar`. The last statement tries to close a circle: the `cellar` receiving the yearbook that already contains it. The library ignores it in silence and the `cellar` keeps its two members.

## How the engine answers

A federation does not merge its members into one big graph. The engine takes the query one pattern at a time and the same steps repeat for each of them:

1. it asks every member for the rows of the pattern, whatever the rest of the query says
2. it puts together the rows of all members in one table for that pattern
3. once every pattern has its table, it joins the tables on the variables they share

This is why the example above works: the table of `?wine rdfs:label ?label` comes from the catalog and the table of `?wine vr:score ?score` from the guides. They meet in the join on `?wine`. It also explains the rule that matters most with remote members. A pattern travels to a SPARQL endpoint as a query of its own, a `SELECT *` that knows nothing about the rows found so far. You can see the exact text that the engine builds:

```csharp
RDFNamespace dbr = new RDFNamespace("dbr", "http://dbpedia.org/resource/");
RDFNamespace dbo = new RDFNamespace("dbo", "http://dbpedia.org/ontology/");
RDFResource originProperty = new RDFResource(dbo + "origin");
RDFVariable origin = new RDFVariable("origin");

RDFPattern sangioveseOrigin = new RDFPattern(new RDFResource(dbr + "Sangiovese"), originProperty, origin);
Console.WriteLine(new RDFSelectQuery().AddPatternGroup(new RDFPatternGroup().AddPattern(sangioveseOrigin)));
```

<div class="result" markdown="1">

```
SELECT *
WHERE {
  {
    <http://dbpedia.org/resource/Sangiovese> <http://dbpedia.org/ontology/origin> ?ORIGIN .
  }
}
```

</div>

A pattern with a constant in the subject is a small question. A pattern such as `?grape dbo:origin ?origin` would ask the endpoint for every origin it knows and the join with the local rows would happen only after all of them have arrived. On a federation with a remote member, give every pattern that goes remote a constant. Two more facts complete the picture. The members that are endpoints are contacted at the same time for each pattern. The [query optimizer]({{ '/query/tips/' | relative_url }}) does not act on federations: the patterns run in the order you declared them.

## Adding DBpedia

The catalog links each grape to its DBpedia resource with `owl:sameAs`. An endpoint joins the federation with `AddSPARQLEndpoint`, which takes optional query options: here a timeout of thirty seconds and an exception when something goes wrong. The options are covered in [SPARQL endpoints]({{ '/query/endpoints/' | relative_url }}). The new federation reuses the `cellar` as a member.

```csharp
RDFSPARQLEndpoint dbpedia = new RDFSPARQLEndpoint(new Uri("https://dbpedia.org/sparql"));
RDFSPARQLEndpointQueryOptions dbpediaOptions = new RDFSPARQLEndpointQueryOptions(
    30000, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException);
RDFFederation withDbpedia = new RDFFederation()
    .AddFederation(cellar)
    .AddSPARQLEndpoint(dbpedia, dbpediaOptions);
Console.WriteLine(withDbpedia.DataSourcesCount);
```

<div class="result" markdown="1">

```
2
```

</div>

Following the rule above, the cooperative asks the local members first for the DBpedia resource of each grape. Then it asks the federation for the origin of each resource, with the resource as a constant in the pattern.

```csharp
RDFVariable grape = new RDFVariable("grape");
RDFVariable dbpediaGrape = new RDFVariable("dbpedia");

RDFSelectQuery sameAs = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(grape, RDFVocabulary.OWL.SAME_AS, dbpediaGrape)))
    .AddProjectionVariable(dbpediaGrape);
RDFSelectQueryResult sameAsResult = await sameAs.ApplyToFederationAsync(cellar);

foreach (DataRow sameAsRow in sameAsResult.SelectResults.Rows)
{
    RDFResource dbpediaResource = new RDFResource(sameAsRow["?DBPEDIA"].ToString());
    RDFSelectQuery originQuery = new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(dbpediaResource, originProperty, origin)))
        .AddProjectionVariable(origin);
    RDFSelectQueryResult originResult = await originQuery.ApplyToFederationAsync(withDbpedia);
    Console.WriteLine($"{dbpediaResource} | {originResult.SelectResults.Rows[0]["?ORIGIN"]}");
}
```

<div class="result" markdown="1">

| Grape on DBpedia | Origin |
|---|---|
| `http://dbpedia.org/resource/Sangiovese` | `http://dbpedia.org/resource/Italy` |
| `http://dbpedia.org/resource/Canaiolo` | `http://dbpedia.org/resource/Italy` |
| `http://dbpedia.org/resource/Trebbiano` | `http://dbpedia.org/resource/Italy` |
| `http://dbpedia.org/resource/Malvasia` | `http://dbpedia.org/resource/Greece` |
| `http://dbpedia.org/resource/Vermentino` | `http://dbpedia.org/resource/Italy` |

</div>

Four of the five grapes of the cooperative are Italian and the Malvasia comes from Greece. The answer comes from the live DBpedia, so it follows what DBpedia says on the day you run it.

## The SERVICE clause

The loop above works but asks DBpedia five times. SPARQL has a standard way to hand a part of a query to an endpoint: the `SERVICE` clause. In RDFSharp it is an `RDFService`, made of the endpoint and the group of patterns it must evaluate. The group travels to the endpoint whole as one query. The rows that come back are joined with the local ones. Here the local patterns find each grape and its DBpedia resource and the remote group finds the origin of every grape that DBpedia knows.

<div class="pair" markdown="1">

```sparql
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl:  <http://www.w3.org/2002/07/owl#>
PREFIX dbo:  <http://dbpedia.org/ontology/>

SELECT DISTINCT ?name ?origin
WHERE {
  ?grape rdfs:label ?name ;
         owl:sameAs ?dbpedia .
  SERVICE <https://dbpedia.org/sparql> {
    ?dbpedia a dbo:Grape ;
             dbo:origin ?origin .
  }
}
ORDER BY ?name
```

```csharp
RDFVariable name = new RDFVariable("name");

RDFSelectQuery origins = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(grape, RDFVocabulary.RDFS.LABEL, name))
        .AddPattern(new RDFPattern(grape, RDFVocabulary.OWL.SAME_AS, dbpediaGrape)))
    .AddService(new RDFService(dbpedia, new RDFPatternGroup()
        .AddPattern(new RDFPattern(dbpediaGrape, RDFVocabulary.RDF.TYPE, new RDFResource(dbo + "Grape")))
        .AddPattern(new RDFPattern(dbpediaGrape, originProperty, origin)), dbpediaOptions))
    .AddProjectionVariable(name)
    .AddProjectionVariable(origin)
    .AddModifier(new RDFDistinctModifier())
    .AddModifier(new RDFOrderByModifier(name, RDFQueryEnums.RDFOrderByFlavors.ASC));

RDFSelectQueryResult originsResult = await origins.ApplyToGraphAsync(catalog);
foreach (DataRow row in originsResult.SelectResults.Rows)
    Console.WriteLine($"{row["?NAME"]} | {row["?ORIGIN"]}");
```

</div>

<div class="result" markdown="1">

| ?NAME | ?ORIGIN |
|---|---|
| `Canaiolo` | `http://dbpedia.org/resource/Italy` |
| `Malvasia` | `http://dbpedia.org/resource/Greece` |
| `Sangiovese` | `http://dbpedia.org/resource/Italy` |
| `Trebbiano` | `http://dbpedia.org/resource/Italy` |
| `Vermentino` | `http://dbpedia.org/resource/Italy` |

</div>

Two things to notice. First, the query is applied to the catalog alone: a `SERVICE` needs no federation, because the endpoint is named inside the query. Second, the `RDFDistinctModifier` is not decoration. DBpedia states the typing `?dbpedia a dbo:Grape` three times for each grape, so without it every grape comes back three times:

```csharp
RDFSelectQuery allOrigins = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(grape, RDFVocabulary.RDFS.LABEL, name))
        .AddPattern(new RDFPattern(grape, RDFVocabulary.OWL.SAME_AS, dbpediaGrape)))
    .AddService(new RDFService(dbpedia, new RDFPatternGroup()
        .AddPattern(new RDFPattern(dbpediaGrape, RDFVocabulary.RDF.TYPE, new RDFResource(dbo + "Grape")))
        .AddPattern(new RDFPattern(dbpediaGrape, originProperty, origin)), dbpediaOptions))
    .AddProjectionVariable(name)
    .AddProjectionVariable(origin);
Console.WriteLine((await allOrigins.ApplyToGraphAsync(catalog)).SelectResultsCount);
Console.WriteLine((await origins.ApplyToFederationAsync(cellar)).SelectResultsCount);
```

<div class="result" markdown="1">

```
15
5
```

</div>

Fifteen rows without the modifier and five with it. The last line applies the same query to the federation of the catalog and the guides: a `SERVICE` works on a graph as well as on a federation and gives the same five rows.

## Federation or SERVICE

Both reach DBpedia and they answer different needs.

| | Federation member | `SERVICE` clause |
|---|---|---|
| What reaches the endpoint | each pattern on its own | the whole group of patterns |
| Where the endpoint is named | when you build the federation | inside the query |
| Typical use | one question across local and remote sources | a remote part with its own joins |
| Rule of thumb | patterns that go remote carry a constant | the remote group should be selective by itself |

A federation fits when the sources are many and the patterns small and precise. A `SERVICE` fits when a coherent piece of the question is better answered in one go by the remote side.
