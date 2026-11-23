---
title: Meet Vallerosa
permalink: /vallerosa/
tagline: One cooperative, one catalog, every feature
lede: Every example of this manual works on the same data, the catalog of a small wine cooperative in Tuscany. Here are its people, its places, its wines and the vocabulary that describes them.
---

> Three family wineries in a Tuscan valley sell their wines under one name, Vallerosa. They want their catalog on the Web as linked data: open to partner shops, readable by the wine guides, checked before anything goes out. This manual follows them as they build it.
{: .story}

Vallerosa, its producers and its wines are fictional; the grape varieties and the two regions are real. Every IRI lives under `vallerosa.example`, a domain reserved for examples that will never resolve to a real site.

## The files

The whole case fits in five files. Every chapter loads one of them or builds a piece of them by hand.

<div class="cards">
  <a class="card" href="{{ '/assets/data/vallerosa-vocab.ttl' | relative_url }}"><span class="kind">Turtle · 135 triples</span><h3>vallerosa-vocab.ttl</h3><p>The vocabulary: classes such as <code>vr:Wine</code> and properties such as <code>vr:producedBy</code>, with their domains and ranges.</p></a>
  <a class="card" href="{{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}"><span class="kind">Turtle · 143 triples</span><h3>vallerosa-catalog.ttl</h3><p>The catalog: the cooperative, its areas, producers, grape varieties and six wines.</p></a>
  <a class="card" href="{{ '/assets/data/vallerosa-guides.trig' | relative_url }}"><span class="kind">TriG · 10 quadruples</span><h3>vallerosa-guides.trig</h3><p>What two wine guides scored in 2025, one named graph per guide. The Store chapters start here.</p></a>
  <a class="card" href="{{ '/assets/data/vallerosa-sales.ttl' | relative_url }}"><span class="kind">Turtle · 108 triples</span><h3>vallerosa-sales.ttl</h3><p>The map and the ledger: the cellar and the subzones as geometries, six partner shops and what they bought in 2025.</p></a>
  <a class="card" href="{{ '/assets/data/vallerosa-shapes.ttl' | relative_url }}"><span class="kind">Turtle · 47 triples</span><h3>vallerosa-shapes.ttl</h3><p>The rules of the catalog as SHACL shapes: what every wine must state. Written in the SHACL shapes chapter, applied in the validation chapter.</p></a>
</div>

Two namespaces cover everything of the cooperative's own:

| Prefix | Namespace | Holds |
|---|---|---|
| `vr:` | `https://vallerosa.example/vocab#` | the vocabulary: classes and properties |
| `vrd:` | `https://vallerosa.example/id/` | the data: wines, producers, areas, grapes |

Keeping the vocabulary apart from the data is a habit worth having: the vocabulary changes rarely and is shared, the data changes every harvest.

## The places

The valley sits in Tuscany and is split into three subzones, one for each winery. Areas are linked by `vr:partOf`, from the smallest to the largest: a chain that the [property paths](../query/property-paths/) chapter walks in a single query.

```mermaid
flowchart BT
  poggio([vrd:poggio]):::res -- vr:partOf --> valle([vrd:valle-rosa]):::res
  colle([vrd:colle-nord]):::res -- vr:partOf --> valle
  piana([vrd:piana]):::res -- vr:partOf --> valle
  valle -- vr:partOf --> tuscany([vrd:tuscany]):::res
  tuscany -- vr:partOf --> italy([vrd:italy]):::res
```

Every area has a label in English and in Italian: *Rose Valley* and *Valle Rosa*, *North Hill* and *Colle Nord*.

## The people

| Producer | Subzone | Makes |
|---|---|---|
| Fattoria Poggio Alto (`vrd:poggio-alto`) | Hilltop (`vrd:poggio`) | the two reds |
| Tenuta Le Crete (`vrd:le-crete`) | North Hill (`vrd:colle-nord`) | the white blend and the Vin Santo |
| Cantina dei Tre Ulivi (`vrd:tre-ulivi`) | The Plain (`vrd:piana`) | the Vermentino and the rosé |

All three are `vr:memberOf` the cooperative, `vrd:vallerosa`. Their names are proper names, so their labels carry no language tag.

## The wines

| Wine | Type | Vintage | Alcohol | Price (€) | Organic | Blend |
|---|---|---|---|---|---|---|
| `vrd:rosso-2021` Vallerosa Red | `vr:RedWine` | 2021 | 13.5 | 14.50 | no | Sangiovese, Canaiolo |
| `vrd:riserva-2019` Vallerosa Reserve | `vr:RedWine` | 2019 | 14.0 | 32.00 | no | Sangiovese |
| `vrd:bianco-2023` Le Crete White | `vr:WhiteWine` | 2023 | 12.0 | 11.00 | yes | Trebbiano, Malvasia |
| `vrd:vinsanto-2015` Valley Vin Santo | `vr:SweetWine` | 2015 | 16.0 | 38.00 | no | Malvasia, Trebbiano |
| `vrd:vermentino-2023` Vermentino of the Plain | `vr:WhiteWine` | 2023 | 12.5 | 13.00 | yes | Vermentino |
| `vrd:rosato-2024` Valley Rosé | `vr:RoseWine` | 2024 | 12.0 | 10.00 | no | Sangiovese |

Each wine says who made it, its vintage as an `xsd:gYear`, its alcohol and price as `xsd:decimal`, whether it is organic as an `xsd:boolean`, its grapes one by one with `vr:grape` and the same grapes in order of share as an RDF list with `vr:blend`. Each grape variety is linked with `owl:sameAs` to its DBpedia resource, which is what lets the [federations](../query/federations/) chapter ask DBpedia about it.

Here is one wine in full, as the catalog file writes it:

```turtle
vrd:rosso-2021 a vr:RedWine ;
    rdfs:label "Rosso di Vallerosa"@it, "Vallerosa Red"@en ;
    vr:producedBy vrd:poggio-alto ;
    vr:vintage "2021"^^xsd:gYear ;
    vr:alcohol 13.5 ;
    vr:price 14.50 ;
    vr:organic false ;
    vr:grape vrd:sangiovese, vrd:canaiolo ;
    vr:blend ( vrd:sangiovese vrd:canaiolo ) .
```

## The vocabulary

The vocabulary stays at the level of RDF Schema: classes, a small hierarchy, properties with a domain and a range.

| Class | Is a | Describes |
|---|---|---|
| `vr:Cooperative` | | a group of producers selling under one name |
| `vr:Producer` | | a winery |
| `vr:Area` | | a country, region, valley or subzone |
| `vr:Grape` | | a grape variety |
| `vr:Wine` | | a wine of a given vintage |
| `vr:RedWine`, `vr:WhiteWine`, `vr:RoseWine`, `vr:SweetWine` | `vr:Wine` | the four kinds the cooperative sells |
| `vr:Dish` | | a dish a wine goes with |
| `vr:Person` | | someone who says something about a wine |
| `vr:Shop` | | a partner wine shop |
| `vr:Sale` | | one delivery of one wine to one shop |

| Property | From | To |
|---|---|---|
| `vr:memberOf` | `vr:Producer` | `vr:Cooperative` |
| `vr:locatedIn` | `vr:Producer` | `vr:Area` |
| `vr:partOf` | `vr:Area` | `vr:Area` |
| `vr:producedBy` | `vr:Wine` | `vr:Producer` |
| `vr:produces` | `vr:Producer` | `vr:Wine` |
| `vr:grape` | `vr:Wine` | `vr:Grape` |
| `vr:blend` | `vr:Wine` | `rdf:List` |
| `vr:vintage` | `vr:Wine` | `xsd:gYear` |
| `vr:alcohol` | `vr:Wine` | `xsd:decimal` |
| `vr:price` | `vr:Wine` | `xsd:decimal` |
| `vr:organic` | `vr:Wine` | `xsd:boolean` |
| `vr:score` | `vr:Wine` | `xsd:integer` |
| `vr:pairsWith` | `vr:Wine` | `vr:Dish` |
| `vr:awards` | `vr:Wine` | `rdf:Bag` |
| `vr:claims` | `vr:Person` | `rdf:Statement` |
| `vr:soldTo` | `vr:Sale` | `vr:Shop` |
| `vr:soldWine` | `vr:Sale` | `vr:Wine` |
| `vr:bottles` | `vr:Sale` | `xsd:integer` |
| `vr:date` | `vr:Sale` | `xsd:date` |

One datatype of its own completes the vocabulary: `vr:abv`, alcohol by volume, a decimal from 0 to 20 that the [datatypes chapter](../model/datatypes/) builds with its facets.

The catalog only ever says `vr:producedBy`; nobody writes `vr:produces`, its mirror image. Saying that one is the inverse of the other, or that `vr:partOf` is transitive, is a job for OWL and lies beyond RDF Schema. Here, when the cooperative needs those answers, it asks SPARQL to walk the links.

## The map and the shops

Where things are is part of the data too. The cellar is a point, each subzone a polygon sharing its borders with the others, each partner shop a point in its city: all of them GeoSPARQL geometries in WKT, longitude first.

| Shop | City | Bottles bought in 2025 |
|---|---|---|
| Enoteca del Campo (`vrd:enoteca-del-campo`) | Siena | 180 |
| Bottega Arno (`vrd:bottega-arno`) | Florence | 198 |
| Cantinetta della Torre (`vrd:cantinetta-torre`) | Pisa | 132 |
| Vineria Due Torri (`vrd:due-torri`) | Bologna | 72 |
| Enoteca Sette Colli (`vrd:sette-colli`) | Rome | 42 |
| Enoteca dei Navigli (`vrd:navigli`) | Milan | 42 |

Fourteen sales in all, each one a `vr:Sale` with its shop, its wine, its bottles and its date. The [GeoSPARQL](../query/geosparql/) chapter asks which shops lie inside the free delivery radius and how many bottles travel by courier.

## The sources

From the [Store](../store/quadruples/) chapters on, the cooperative is no longer the only one talking about its wines. Each source gets its own context, so every fact keeps track of who said it.

| Context | Who |
|---|---|
| `https://vallerosa.example/catalog` | the cooperative's own catalog |
| `https://guidacalici.example/2025` | *Guida ai Calici*, an Italian wine guide, 2025 edition |
| `https://winescope.example/2025` | *WineScope*, an international guide, 2025 edition |

The two guides do not always agree. WineScope gives the Vallerosa Red 90 where Guida ai Calici gives it 88. Calici never tasted the rosé and WineScope never tasted the white blend.

## The story, chapter by chapter

| Layer | What happens at Vallerosa |
|---|---|
| Model | The first wines are written down by hand, saved as Turtle and exchanged with the producers in other formats. The cooperative gets its own namespaces and an alcohol datatype that refuses impossible values. Blends become ordered lists, awards a bag. A sommelier's pairing advice is recorded as a claim rather than a fact. SHACL shapes catch a sloppy import before it reaches the website. |
| Store | The catalog and the two guides move into one store, each in its own context. The store is saved as TriG and N-Quads, then moved onto a database. |
| Query | The cooperative asks questions of growing difficulty: whites under 13 degrees, every wine of Tuscany at any depth, the average score per subzone, the best wine of each producer, prices with VAT. It measures how far each shop is from the cellar, builds an export for a partner shop, asks DBpedia about its grapes and closes the year with a batch of updates. |

When the cooperative's needs outgrow RDF Schema, with inverse and transitive properties, classes defined by rules and a reasoner deducing what nobody wrote down, the story moves on to the [OWLSharp manual](https://github.com/mdesalvo/OWLSharp), which picks up these same files.
