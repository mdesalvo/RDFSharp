---
title: Namespaces and vocabularies
permalink: /model/namespaces/
tagline: Short names for long IRIs and a register that remembers them
lede: A namespace gives an IRI prefix a short name. The register of namespaces keeps those names for the serializers. The built-in vocabularies save you from typing the standard IRIs.
---

> The cooperative's two namespaces, `vr:` for the vocabulary and `vrd:` for the data, have been typed in every example so far. Now they become official: they go into the register of the library and every file the cooperative writes from then on speaks in prefixes.
{: .story}

## A namespace

A **namespace** is the first part of a family of IRIs, given a short name called **prefix**. The vocabulary of the cooperative lives at `https://vallerosa.example/vocab#` and goes by `vr`. An `RDFNamespace` is the pair and adding a local name to it gives you the IRI of a resource:

```csharp
using RDFSharp.Model;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

Console.WriteLine(vr.NamespacePrefix);
Console.WriteLine(vr.NamespaceUri);
Console.WriteLine(vrd + "vermentino");
```

<div class="result" markdown="1">

```
vr
https://vallerosa.example/vocab#
https://vallerosa.example/id/vermentino
```

</div>

The library checks both parts. A prefix holds letters and digits only (and cannot be `bnode` or `xmlns`, which RDFSharp keeps for itself) and the IRI must be an absolute address:

```csharp
try
{
    RDFNamespace brokenPrefix = new RDFNamespace("vallerosa wine", "https://vallerosa.example/");
}
catch (RDFModelException exception)
{
    Console.WriteLine(exception.Message);
}

try
{
    RDFNamespace brokenUri = new RDFNamespace("vr", "not an address");
}
catch (RDFModelException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
Cannot create RDFNamespace because "prefix" parameter contains unallowed characters
Cannot create RDFNamespace because "uri" parameter is not a valid Uri
```

</div>

## What a serializer does without the register

Take the Vermentino grape of the catalog and write it as Turtle. Its type comes from the cooperative's vocabulary, its label from RDF Schema and its link to DBpedia from OWL:

```csharp
RDFResource grapeClass = new RDFResource(vr + "Grape");

RDFResource vermentino = new RDFResource(vrd + "vermentino");
RDFResource dbpediaVermentino = new RDFResource("http://dbpedia.org/resource/Vermentino");

RDFPlainLiteral vermentinoLabel = new RDFPlainLiteral("Vermentino");

RDFGraph grape = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/catalog"))
    .AddTriple(new RDFTriple(vermentino, RDFVocabulary.RDF.TYPE, grapeClass))
    .AddTriple(new RDFTriple(vermentino, RDFVocabulary.RDFS.LABEL, vermentinoLabel))
    .AddTriple(new RDFTriple(vermentino, RDFVocabulary.OWL.SAME_AS, dbpediaVermentino));

await grape.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "grape-before.ttl");
```

<div class="result" data-label="grape-before.ttl" markdown="1">

```turtle
@prefix owl: <http://www.w3.org/2002/07/owl#>.
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@base <https://vallerosa.example/catalog>.

<https://vallerosa.example/id/vermentino> a <https://vallerosa.example/vocab#Grape>; 
                                          rdfs:label "Vermentino"; 
                                          owl:sameAs <http://dbpedia.org/resource/Vermentino>. 
```

</div>

The standard vocabularies come out as `rdf:`, `rdfs:` and `owl:`, because the library knows them. The two IRIs of the cooperative are written in full: nobody told the library about `vr` and `vrd`.

## The register

`RDFNamespaceRegister` is the library's single list of known namespaces. It is a static class: you never create one, you call `AddNamespace` on it. A new library starts with 25 entries: the 24 standard vocabularies and `rdfsharp`, the default namespace.

```csharp
Console.WriteLine(RDFNamespaceRegister.NamespacesCount);
RDFNamespaceRegister.AddNamespace(vr);
RDFNamespaceRegister.AddNamespace(vrd);
Console.WriteLine(RDFNamespaceRegister.NamespacesCount);
RDFNamespaceRegister.AddNamespace(new RDFNamespace("vr", "https://vallerosa.example/other#"));
Console.WriteLine(RDFNamespaceRegister.NamespacesCount);
```

<div class="result" markdown="1">

```
25
27
27
```

</div>

A prefix and an IRI each appear once in the register. The third call tries to give the prefix `vr` to another IRI and the register keeps the first one without an error. You search it by prefix or by IRI and the answer is `null` when the namespace is unknown:

```csharp
Console.WriteLine(RDFNamespaceRegister.GetByPrefix("vrd").NamespaceUri);
Console.WriteLine(RDFNamespaceRegister.GetByUri("https://vallerosa.example/vocab#").NamespacePrefix);
Console.WriteLine(RDFNamespaceRegister.GetByPrefix("dbr") == null);
```

<div class="result" markdown="1">

```
https://vallerosa.example/id/
vr
True
```

</div>

The register can also ask the `prefix.cc` service, a public directory of prefixes, for a namespace it does not know: pass `true` as the second argument of `GetByPrefix` or `GetByUri`. The answer, when there is one, is added to the register. The call goes to the network and fails quietly with `null` when the service does not answer, so use it at development time and not in the middle of an import. This manual does not run it.

## The register drives the serializers

Write the same grape again. Nothing changed in the graph, only in the register:

```csharp
await grape.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "grape-after.ttl");
```

<div class="result" data-label="grape-after.ttl" markdown="1">

```turtle
@prefix owl: <http://www.w3.org/2002/07/owl#>.
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix vrd: <https://vallerosa.example/id/>.
@base <https://vallerosa.example/catalog>.

vrd:vermentino a vr:Grape; 
               rdfs:label "Vermentino"; 
               owl:sameAs <http://dbpedia.org/resource/Vermentino>. 
```

</div>

Now `vr:` and `vrd:` appear and only the prefixes that the file really uses are declared. The DBpedia address is still in full. Registering it fixes that too:

```csharp
RDFNamespaceRegister.AddNamespace(new RDFNamespace("dbr", "http://dbpedia.org/resource/"));
await grape.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "grape-dbr.ttl");
```

<div class="result" data-label="grape-dbr.ttl" markdown="1">

```turtle
@prefix dbr: <http://dbpedia.org/resource/>.
@prefix owl: <http://www.w3.org/2002/07/owl#>.
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix vrd: <https://vallerosa.example/id/>.
@base <https://vallerosa.example/catalog>.

vrd:vermentino a vr:Grape; 
               rdfs:label "Vermentino"; 
               owl:sameAs dbr:Vermentino. 
```

</div>

The register is global to the process, so what you add once serves every graph you write afterwards, in Turtle and in RDF/XML.

To see the whole register, enumerate it or walk `RDFNamespaceRegister.NamespacesEnumerator`. To remove a namespace, use `RemoveByPrefix` or `RemoveByUri`. The standard ones are protected and a removal of them does nothing:

```csharp
foreach (RDFNamespace registeredNamespace in RDFNamespaceRegister.Instance)
    Console.Write(registeredNamespace.NamespacePrefix + " ");
Console.WriteLine();

RDFNamespaceRegister.RemoveByPrefix("rdf");
Console.WriteLine(RDFNamespaceRegister.GetByPrefix("rdf") != null);
RDFNamespaceRegister.RemoveByUri("http://dbpedia.org/resource/");
Console.WriteLine(RDFNamespaceRegister.GetByPrefix("dbr") == null);
```

<div class="result" markdown="1">

```
rdfsharp rdf rdfs xsd owl sh swrl swrlb xml dc dcam dcterms dctype foaf geo geosparql sf geof uom unit skos skosxl time greg thors vr vrd dbr
True
True
```

</div>

## The built-in vocabularies

The IRIs of RDF, RDF Schema, XML Schema and the other well-known vocabularies are ready as constants in the static class `RDFVocabulary`, one nested class for each vocabulary. Each has its `PREFIX` and its `BASE_URI` and a resource for every term it defines:

```csharp
RDFResource rdfType = new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

Console.WriteLine(RDFVocabulary.RDF.TYPE);
Console.WriteLine(RDFVocabulary.RDFS.LABEL);
Console.WriteLine(RDFVocabulary.RDFS.SUB_CLASS_OF);
Console.WriteLine(RDFVocabulary.OWL.SAME_AS);
Console.WriteLine(RDFVocabulary.XSD.PREFIX);
Console.WriteLine(RDFVocabulary.SHACL.BASE_URI);
Console.WriteLine(RDFVocabulary.RDF.TYPE.Equals(rdfType));
```

<div class="result" markdown="1">

```
http://www.w3.org/1999/02/22-rdf-syntax-ns#type
http://www.w3.org/2000/01/rdf-schema#label
http://www.w3.org/2000/01/rdf-schema#subClassOf
http://www.w3.org/2002/07/owl#sameAs
xsd
http://www.w3.org/ns/shacl#
True
```

</div>

A term is an `RDFResource` like any other, so `RDFVocabulary.RDF.TYPE` and the resource you build from the IRI are the same. These are the vocabularies that the library carries and the register holds every one of them under its prefix:

| Class | Prefix | Vocabulary |
|---|---|---|
| `RDF` | `rdf` | RDF itself: types, lists, containers, reification |
| `RDFS` | `rdfs` | RDF Schema: classes, labels, domains and ranges |
| `XSD` | `xsd` | XML Schema datatypes and facets |
| `OWL` | `owl` | the Web Ontology Language |
| `SHACL` | `sh` | shapes and validation reports |
| `DC` | `dc` | Dublin Core elements, with `DCAM`, `DCTERMS` and `DCTYPE` |
| `FOAF` | `foaf` | people and their relations |
| `GEO` | `geo` | points on the map |
| `GEOSPARQL` | `geosparql` | geometries and spatial functions, with `SF`, `GEOF`, `UOM` and `UNIT` |
| `SKOS` | `skos` | thesauri and classification schemes, with `SKOSXL` |
| `SWRL` | `swrl` | rules, with `SWRLB` for the built-in functions |
| `TIME` | `time` | instants and intervals, with `GREG` and `THORS` |
| `XML` | `xml` | the `xml:` attributes such as the language |
| `RDFSHARP` | `rdfsharp` | the default namespace of the library |

The vocabulary of the cooperative is not on the list and does not need to be: `vr` is a namespace you build and its terms are the resources you create from it, as you did in every example.

## The default namespace

One namespace of the register has a special job. A graph that was never given a context gets the default namespace as its context and that is the `https://rdfsharp.codeplex.com/` that showed up in the earlier chapters. You can change it for every graph created afterwards:

```csharp
Console.WriteLine(new RDFGraph().Context);
RDFNamespaceRegister.SetDefaultNamespace(vrd);
Console.WriteLine(new RDFGraph().Context);
RDFNamespaceRegister.ResetDefaultNamespace();
Console.WriteLine(new RDFGraph().Context);
```

<div class="result" markdown="1">

```
https://rdfsharp.codeplex.com/
https://vallerosa.example/id/
https://rdfsharp.codeplex.com/
```

</div>

The cooperative does not change the default: a graph that has to be named gets `SetContext`, so nothing relies on a global.

## Where a namespace can be read

The IRI of a namespace is a name, not always an address that serves RDF. A **dereference URI** is the address to use when somebody asks for the namespace as RDF, for example through `RDFGraph.FromUriAsync`. By default it is the IRI itself. The vocabulary of the cooperative is published as a file of this manual, so that is the address to give it:

```csharp
vr.SetDereferenceUri(new Uri("https://mdesalvo.github.io/RDFSharp/assets/data/vallerosa-vocab.ttl"));
Console.WriteLine(vr.DereferenceUri);
Console.WriteLine(vrd.DereferenceUri);
```

<div class="result" markdown="1">

```
https://mdesalvo.github.io/RDFSharp/assets/data/vallerosa-vocab.ttl
https://vallerosa.example/id/
```

</div>

The [vocabulary file]({{ '/assets/data/vallerosa-vocab.ttl' | relative_url }}) is the one described in [Meet Vallerosa]({{ '/vallerosa/' | relative_url }}). The next chapter turns to the other half of what the register of the library holds: the datatypes.
