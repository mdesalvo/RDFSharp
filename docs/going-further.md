---
title: Going further
permalink: /going-further/
tagline: Where the story continues
lede: The Vallerosa catalog is on the Web, validated, stored with its sources and queried from every side. Here is where to go next.
---

> At year end the cooperative looks at what it has built: a catalog that anyone can read, two guides that sit side by side without overwriting each other and a set of queries that answer the questions it used to answer by hand. It now wants the data to reason on its own.
{: .story}

## Ontologies and reasoning: OWLSharp

RDF Schema says what classes and properties exist. It cannot say that `vr:produces` is the inverse of `vr:producedBy`, that `vr:partOf` is transitive, that a red wine is never a white wine or that a wine above 15 degrees made from dried grapes is a sweet wine. That is the job of OWL2 and of SWRL rules, with a reasoner that deduces what nobody wrote down.

[OWLSharp](https://github.com/mdesalvo/OWLSharp) does this on top of RDFSharp. Its manual starts from the same Vallerosa files and grows the vocabulary into an ontology.

## Databases: RDFSharp.Extensions

When the catalog outgrows memory, [RDFSharp.Extensions](https://github.com/mdesalvo/RDFSharp.Extensions) keeps quadruples on Azure Table, Firebird, MongoDB, MySQL, Neo4j, Oracle, PostgreSQL, SQLite or SQL Server, behind the same `RDFStore` API. The [Persistent stores](../store/extensions/) chapter shows the switch.

## The standards

RDFSharp implements W3C recommendations; when in doubt about what something means, they are the final word.

| Standard | What it defines |
|---|---|
| [RDF 1.1 Concepts](https://www.w3.org/TR/rdf11-concepts/) | resources, literals, triples, graphs, datasets |
| [RDF Schema 1.1](https://www.w3.org/TR/rdf-schema/) | classes, properties, domains and ranges |
| [Turtle](https://www.w3.org/TR/turtle/), [N-Triples](https://www.w3.org/TR/n-triples/), [TriG](https://www.w3.org/TR/trig/), [N-Quads](https://www.w3.org/TR/n-quads/), [RDF/XML](https://www.w3.org/TR/rdf-syntax-grammar/) | the formats |
| [SPARQL 1.1 Query](https://www.w3.org/TR/sparql11-query/) and [Update](https://www.w3.org/TR/sparql11-update/) | queries and operations |
| [SHACL](https://www.w3.org/TR/shacl/) | shapes and validation |
| [GeoSPARQL](https://www.ogc.org/standard/geosparql/) (OGC) | geometries and spatial functions |

## Help and contributions

Questions, bugs and ideas are welcome as [GitHub issues](https://github.com/mdesalvo/RDFSharp/issues). Every page of this manual has an *Improve this page* link at its foot: a correction is one pull request away.
