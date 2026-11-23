---
title: Query at a glance
permalink: /query/
eyebrow: The Query layer
tagline: Ask questions, change answers
lede: "The Query layer is Mirella, the SPARQL 1.1 engine of RDFSharp. It answers queries and applies updates over graphs, stores, federations and remote endpoints, whether you compose them in C# or write them as SPARQL text."
---


## What you find here

Everything in this layer lives in the `RDFSharp.Query` namespace, on top of the other two.

```csharp
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;
```

Every query can be composed in C#, with one class per SPARQL construct, or parsed from a SPARQL string; both produce the same object and the same answers. The chapters start from patterns and build up through filters, paths, aggregates and expressions to the four query forms. Then the data sources beyond a single graph: federations and remote endpoints. Last, the updates: SPARQL operations that insert, delete and reorganize data.

{% include layer-cards.html group="Query" %}

## Where the data comes from

Most examples query [vallerosa-catalog.ttl]({{ '/assets/data/vallerosa-catalog.ttl' | relative_url }}). Those about scores use the store of the Store chapters; those about shops and sales use [vallerosa-sales.ttl]({{ '/assets/data/vallerosa-sales.ttl' | relative_url }}). Each page says which.
