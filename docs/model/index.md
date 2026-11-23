---
title: Model at a glance
permalink: /model/
eyebrow: The Model layer
tagline: Describe the data, exchange it, check it
lede: "The Model layer is where RDF data takes shape: resources, literals and triples gathered in graphs, written in the standard formats and validated against SHACL shapes. Everything else in RDFSharp builds on it."
---

> The cooperative starts from nothing: no database, no website, only six wines and the will to describe them well. By the end of this layer its catalog is a graph with its own vocabulary, it travels in the standard RDF formats and nothing reaches the website without passing a quality check.
{: .story}

## What you find here

Everything in this layer lives in the `RDFSharp.Model` namespace.

```csharp
using RDFSharp.Model;
```

The chapters follow the life of the catalog. First the building blocks: what a resource, a literal and a triple are, how triples gather in a graph and how a graph is written to a file and read back. Then the vocabulary around the data: namespaces, datatypes, lists and claims. Last, the rules: SHACL shapes that describe a correct catalog entry and a validator that reports every entry that breaks them.

{% include layer-cards.html group="Model" %}

## Where the data comes from

The examples load [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}) or build pieces of it by hand. [Meet Vallerosa]({{ '/vallerosa/' | relative_url }}) describes what is inside.
