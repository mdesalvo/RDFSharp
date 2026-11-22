---
title: Property paths
permalink: /query/property-paths/
tagline: Walking chains of links in one step
lede: A property path describes a route through the graph: follow this link, then that one, as many times as needed. It replaces a pile of patterns with one line and it reaches things that a fixed number of patterns cannot.
---

> The cooperative wants every wine of Tuscany. A wine knows its producer, a producer knows its subzone and a subzone is part of a valley that is part of a region: the answer is at the end of a chain of unknown length.
{: .story}

This page runs on [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}) and, for the second example, on the catalog joined to [vallerosa-vocab.ttl]({{ '/assets/data/vallerosa-vocab.ttl' | relative_url }}).

## Anatomy of a path

A path connects a **start** and an **end**, each one a resource or a variable, through a route made of **steps**. `RDFPropertyPath` takes the two ends. `AddStep` adds one step at a time and the steps are followed in the order you add them, so three calls make a sequence. A path is added to a pattern group with `AddPropertyPath`, as a pattern would be.

A step comes from one of four factories of `RDFPropertyPathStep`, which can then be decorated:

| Factory or decorator | SPARQL | Meaning |
|---|---|---|
| `Link(p)` | `p` | follow the predicate `p` once |
| `Sequence([a, b])` | `a/b` | follow `a`, then `b` |
| `Alternative([a, b])` | `a\|b` | follow `a` or follow `b` |
| `NegatedPropertySet(forward, inverse)` | `!(a\|^b)` | follow any predicate except the listed ones |
| `.Inverse()` | `^p` | follow `p` backwards, from object to subject |
| `.ZeroOrOne()` | `p?` | follow it once or not at all |
| `.OneOrMore()` | `p+` | follow it one time or more |
| `.ZeroOrMore()` | `p*` | follow it any number of times, none included |

## Every wine of Tuscany

The route from a wine to Tuscany goes by its producer, then by the area where the producer sits, then up the chain of `vr:partOf` as far as it goes. The last link can be repeated any number of times, which is what the star says.

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?wine
WHERE {
  ?wine vr:producedBy/vr:locatedIn/vr:partOf* vrd:tuscany .
}
```

```csharp
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFGraph catalog = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFVariable wine = new RDFVariable("wine");
RDFResource tuscany = new RDFResource(vrd + "tuscany");

RDFPropertyPath winesOfTuscany = new RDFPropertyPath(wine, tuscany)
    .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "producedBy")))
    .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "locatedIn")))
    .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "partOf")).ZeroOrMore());

RDFSelectQuery tuscanWines = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(winesOfTuscany))
    .AddProjectionVariable(wine);

RDFSelectQueryResult tuscan = tuscanWines.ApplyToGraph(catalog);
foreach (DataRow row in tuscan.SelectResults.Rows)
    Console.WriteLine(row["?WINE"]);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` |
| `https://vallerosa.example/id/bianco-2023` |
| `https://vallerosa.example/id/vinsanto-2015` |
| `https://vallerosa.example/id/vermentino-2023` |
| `https://vallerosa.example/id/rosato-2024` |

</div>

All six wines, as expected: every producer is in the valley and the valley is in Tuscany. The star is what makes the query robust. Poggio is two links away from Tuscany and a producer in a larger area could be one link away and the same path answers for both. The same route to the North Hill instead of Tuscany finds only what lives there, because the star accepts zero links:

```csharp
RDFPropertyPath winesOfNorthHill = new RDFPropertyPath(wine, new RDFResource(vrd + "colle-nord"))
    .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "producedBy")))
    .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "locatedIn")))
    .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "partOf")).ZeroOrMore());

RDFSelectQuery northHillWines = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(winesOfNorthHill))
    .AddProjectionVariable(wine);
```

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/bianco-2023` |
| `https://vallerosa.example/id/vinsanto-2015` |

</div>

## One or more, zero or more

The difference between the plus and the star is the starting point. `vr:partOf+` needs at least one link, so it gives the areas above Poggio and not Poggio itself. `vr:partOf*` also accepts zero links, so the start counts as its own answer. The plus first:

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?area
WHERE {
  vrd:poggio vr:partOf+ ?area .
}
```

```csharp
RDFVariable area = new RDFVariable("area");

RDFSelectQuery above = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(
        new RDFPropertyPath(new RDFResource(vrd + "poggio"), area)
            .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "partOf")).OneOrMore())))
    .AddProjectionVariable(area);

RDFSelectQueryResult aboveResult = above.ApplyToGraph(catalog);
```

</div>

<div class="result" markdown="1">

| ?AREA |
|---|
| `https://vallerosa.example/id/valle-rosa` |
| `https://vallerosa.example/id/tuscany` |
| `https://vallerosa.example/id/italy` |

</div>

With `ZeroOrMore()` in place of `OneOrMore()` the table gains a first row, `https://vallerosa.example/id/poggio` and the other three stay. Note that the path starts from a resource and ends at a variable: either end can be fixed or free.

## Inverse and alternative

The catalog says `vr:producedBy`, from the wine to its producer. The wines of a producer are on the way back and `Inverse()` walks a link backwards, so the producer is the start:

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?wine
WHERE {
  vrd:poggio-alto ^vr:producedBy ?wine .
}
```

```csharp
RDFSelectQuery winesOfProducer = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(
        new RDFPropertyPath(new RDFResource(vrd + "poggio-alto"), wine)
            .AddStep(RDFPropertyPathStep.Link(new RDFResource(vr + "producedBy")).Inverse())))
    .AddProjectionVariable(wine);

RDFSelectQueryResult winesResult = winesOfProducer.ApplyToGraph(catalog);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` |

</div>

This is the answer that the vocabulary would give with `vr:produces`, the inverse property that nobody writes in the catalog. Here the path computes it on the spot.

An alternative follows one route or the other. The areas next to the Rose Valley, the ones it belongs to and the ones that belong to it, are the links `vr:partOf` in both directions:

<div class="pair" markdown="1">

```sparql
PREFIX vr:  <https://vallerosa.example/vocab#>
PREFIX vrd: <https://vallerosa.example/id/>

SELECT ?neighbour
WHERE {
  vrd:valle-rosa (vr:partOf|^vr:partOf) ?neighbour .
}
```

```csharp
RDFResource partOf = new RDFResource(vr + "partOf");
RDFVariable neighbour = new RDFVariable("neighbour");

RDFSelectQuery neighbours = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(
        new RDFPropertyPath(new RDFResource(vrd + "valle-rosa"), neighbour)
            .AddStep(RDFPropertyPathStep.Alternative(new List<RDFPropertyPathStep>
            {
                RDFPropertyPathStep.Link(partOf),
                RDFPropertyPathStep.Link(partOf).Inverse()
            }))))
    .AddProjectionVariable(neighbour);

RDFSelectQueryResult neighboursResult = neighbours.ApplyToGraph(catalog);
```

</div>

<div class="result" markdown="1">

| ?NEIGHBOUR |
|---|
| `https://vallerosa.example/id/tuscany` |
| `https://vallerosa.example/id/poggio` |
| `https://vallerosa.example/id/colle-nord` |
| `https://vallerosa.example/id/piana` |

</div>

> Several `AddStep` calls and one `Sequence` step are two ways to say the same thing: `AddStep(a).AddStep(b)` is `a/b`. Use `Sequence` when the whole sequence needs a decorator, for example `(a/b)+`.
{: .tip}

## Every wine, whatever its subclass

The vocabulary says that `vr:RedWine`, `vr:WhiteWine`, `vr:RoseWine` and `vr:SweetWine` are subclasses of `vr:Wine` and the catalog says only what each wine is: a red, a sweet one. A plain pattern for `?wine a vr:Wine` finds nothing, because no wine is declared of that class directly. Walking the type and then the subclass links by hand finds them all.

The graph for this query is the union of the vocabulary and the catalog, which together have 265 triples:

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?wine
WHERE {
  ?wine rdf:type/rdfs:subClassOf* vr:Wine .
}
```

```csharp
RDFGraph vocab = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "vallerosa-vocab.ttl");
RDFGraph vocabAndCatalog = vocab.UnionWith(catalog);

RDFSelectQuery allWines = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(
        new RDFPropertyPath(wine, new RDFResource(vr + "Wine"))
            .AddStep(RDFPropertyPathStep.Link(RDFVocabulary.RDF.TYPE))
            .AddStep(RDFPropertyPathStep.Link(RDFVocabulary.RDFS.SUB_CLASS_OF).ZeroOrMore())))
    .AddProjectionVariable(wine);

RDFSelectQueryResult allWinesResult = allWines.ApplyToGraph(vocabAndCatalog);
Console.WriteLine(vocabAndCatalog.TriplesCount);
```

</div>

<div class="result" markdown="1">

| ?WINE |
|---|
| `https://vallerosa.example/id/rosso-2021` |
| `https://vallerosa.example/id/riserva-2019` |
| `https://vallerosa.example/id/bianco-2023` |
| `https://vallerosa.example/id/vinsanto-2015` |
| `https://vallerosa.example/id/vermentino-2023` |
| `https://vallerosa.example/id/rosato-2024` |

</div>

What this path does by hand is what a reasoner deduces on its own: once `vr:SweetWine` is declared a subclass of `vr:Wine`, a reasoner knows that every sweet wine is a wine, without you writing the walk. That is the job of the [OWLSharp manual](https://github.com/mdesalvo/OWLSharp), which picks up these same files.

## Negation

`NegatedPropertySet` follows any predicate except those you list, separately for the forward and the inverse direction. All that the catalog says of the Vallerosa Red apart from its type, its labels and its blend:

```csharp
RDFVariable value = new RDFVariable("value");
RDFSelectQuery otherFacts = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup().AddPropertyPath(
        new RDFPropertyPath(new RDFResource(vrd + "rosso-2021"), value)
            .AddStep(RDFPropertyPathStep.NegatedPropertySet(new List<RDFResource>
            {
                RDFVocabulary.RDF.TYPE,
                RDFVocabulary.RDFS.LABEL,
                new RDFResource(vr + "blend")
            }))))
    .AddProjectionVariable(value);

RDFSelectQueryResult otherFactsResult = otherFacts.ApplyToGraph(catalog);
```

<div class="result" markdown="1">

| ?VALUE |
|---|
| `https://vallerosa.example/id/poggio-alto` |
| `2021Z^^http://www.w3.org/2001/XMLSchema#gYear` |
| `13.5^^http://www.w3.org/2001/XMLSchema#decimal` |
| `14.50^^http://www.w3.org/2001/XMLSchema#decimal` |
| `false^^http://www.w3.org/2001/XMLSchema#boolean` |
| `https://vallerosa.example/id/sangiovese` |
| `https://vallerosa.example/id/canaiolo` |

</div>

Paths can also be combined with the other members of a group: `Optional()` works on a path as on a pattern and so do `Union` and `Minus`, as described in [Variables and patterns]({{ '/query/patterns/' | relative_url }}). The next page nests a whole query inside another: [Subqueries]({{ '/query/subqueries/' | relative_url }}).
