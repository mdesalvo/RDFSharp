---
title: Store at a glance
permalink: /store/
eyebrow: The Store layer
tagline: Remember where every fact came from
lede: "A graph knows what is said, a store also knows who said it. The Store layer adds a context to every triple, keeps many sources side by side and moves them from memory to a database when they grow."
---

> The wine guides have started to write about Vallerosa. Their scores are worth publishing next to the catalog, but they must never be confused with it, nor with each other.
{: .story}

## What you find here

Everything in this layer lives in the `RDFSharp.Store` namespace, on top of `RDFSharp.Model`.

```csharp
using RDFSharp.Model;
using RDFSharp.Store;
```

A quadruple is a triple with a fourth element, its context: the name of the source it comes from. A store holds quadruples, so the same statement told by two guides stays two statements. The chapters go from the quadruple to the store, then to the formats that carry contexts and last to the databases that can hold a store.

{% include layer-cards.html group="Store" %}

## Where the data comes from

The examples load [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}), with one named graph per guide, and put the catalog in its own context next to them.
