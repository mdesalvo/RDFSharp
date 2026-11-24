---
title: RDF formats
permalink: /model/formats/
tagline: Write a graph to a file or a stream and read it back
lede: RDFSharp reads and writes four RDF formats. The register of namespaces decides how readable the result is.
---

> Cantina dei Tre Ulivi sends its wines to the cooperative as N-Triples, the format its software produces. A partner shop that sells the Vallerosa wines asks for the same data as RDF/XML, so the cooperative reads one format and writes the other.
{: .story}

## One graph, four formats

A **format** is a way to write a graph as text. The library reads and writes four of them, named by the enumeration `RDFModelEnums.RDFFormats`:

| Format | Value | What it is good for |
|---|---|---|
| N-Triples | `NTriples` | one triple for each line: simple for programs to produce and to split |
| Turtle | `Turtle` | the compact form for people, with prefixes and grouped subjects |
| TriX | `TriX` | RDF as XML with every term spelled out: verbose and unambiguous |
| RDF/XML | `RdfXml` | the oldest standard form, the one that XML tools understand |

Every example of the chapter writes the same small graph: three triples of the Vermentino. Before writing, the two namespaces of the cooperative go into the register of the library, which the Turtle and RDF/XML writers use to shorten IRIs (more on that at the end):

```csharp
using RDFSharp.Model;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFNamespaceRegister.AddNamespace(vr);
RDFNamespaceRegister.AddNamespace(vrd);

RDFResource producedBy = new RDFResource(vr + "producedBy");
RDFResource hasVintage = new RDFResource(vr + "vintage");

RDFResource vermentino2023 = new RDFResource(vrd + "vermentino-2023");
RDFResource treUlivi = new RDFResource(vrd + "tre-ulivi");

RDFPlainLiteral vermentinoLabel = new RDFPlainLiteral("Vermentino of the Plain", "en");
RDFTypedLiteral vermentinoVintage = new RDFTypedLiteral("2023", RDFModelEnums.RDFDatatypes.XSD_GYEAR);

RDFGraph wine = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/catalog"))
    .AddTriple(new RDFTriple(vermentino2023, RDFVocabulary.RDFS.LABEL, vermentinoLabel))
    .AddTriple(new RDFTriple(vermentino2023, producedBy, treUlivi))
    .AddTriple(new RDFTriple(vermentino2023, hasVintage, vermentinoVintage));
```

## Write to a file

`ToFileAsync` takes the format and a path and creates or replaces the file. One call for each format:

```csharp
await wine.ToFileAsync(RDFModelEnums.RDFFormats.NTriples, "vermentino.nt");
await wine.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "vermentino.ttl");
await wine.ToFileAsync(RDFModelEnums.RDFFormats.TriX, "vermentino.trix");
await wine.ToFileAsync(RDFModelEnums.RDFFormats.RdfXml, "vermentino.rdf");
```

These are the four files. Pick a tab for each of them:

<div class="result" data-label="vermentino.*" markdown="1">

<div class="pair" data-labels="N-Triples,Turtle,TriX,RDF/XML" markdown="1">

```text
<https://vallerosa.example/id/vermentino-2023> <http://www.w3.org/2000/01/rdf-schema#label> "Vermentino of the Plain"@EN .
<https://vallerosa.example/id/vermentino-2023> <https://vallerosa.example/vocab#producedBy> <https://vallerosa.example/id/tre-ulivi> .
<https://vallerosa.example/id/vermentino-2023> <https://vallerosa.example/vocab#vintage> "2023Z"^^<http://www.w3.org/2001/XMLSchema#gYear> .
```

```turtle
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix vrd: <https://vallerosa.example/id/>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@base <https://vallerosa.example/catalog>.

vrd:vermentino-2023 rdfs:label "Vermentino of the Plain"@EN; 
                    vr:producedBy vrd:tre-ulivi; 
                    vr:vintage "2023Z"^^xsd:gYear. 
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<TriX xmlns="http://www.w3.org/2004/03/trix/trix-1/">
  <graph>
    <uri>https://vallerosa.example/catalog</uri>
    <triple>
      <uri>https://vallerosa.example/id/vermentino-2023</uri>
      <uri>http://www.w3.org/2000/01/rdf-schema#label</uri>
      <plainLiteral xml:lang="EN">Vermentino of the Plain</plainLiteral>
    </triple>
    <triple>
      <uri>https://vallerosa.example/id/vermentino-2023</uri>
      <uri>https://vallerosa.example/vocab#producedBy</uri>
      <uri>https://vallerosa.example/id/tre-ulivi</uri>
    </triple>
    <triple>
      <uri>https://vallerosa.example/id/vermentino-2023</uri>
      <uri>https://vallerosa.example/vocab#vintage</uri>
      <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#gYear">2023Z</typedLiteral>
    </triple>
  </graph>
</TriX>
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<rdf:RDF xmlns:vrd="https://vallerosa.example/id/" xmlns:rdfs="http://www.w3.org/2000/01/rdf-schema#" xmlns:vr="https://vallerosa.example/vocab#" xmlns:xsd="http://www.w3.org/2001/XMLSchema#" xml:base="https://vallerosa.example/catalog" xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
  <rdf:Description rdf:about="https://vallerosa.example/id/vermentino-2023">
    <rdfs:label xml:lang="EN">Vermentino of the Plain</rdfs:label>
    <vr:producedBy rdf:resource="https://vallerosa.example/id/tre-ulivi" />
    <vr:vintage rdf:datatype="http://www.w3.org/2001/XMLSchema#gYear">2023Z</vr:vintage>
  </rdf:Description>
</rdf:RDF>
```

</div>

</div>

The same three triples look different in each file:
- N-Triples repeats the full IRIs on every line and has no place for anything else
- Turtle declares prefixes once and groups the three triples under their subject
- TriX writes each term as an element of its own, with the graph's context at the top
- RDF/XML turns the predicates into element names, which is why it needs the prefixes of the namespaces

Two details come from the library, not from the formats. The language tag is upper case and the year carries the `Z` of UTC, as explained in [Resources, literals and triples]({{ '/model/triples/' | relative_url }}). The context of the graph shows up as `@base` in Turtle, as `xml:base` in RDF/XML and as the `uri` of the graph in TriX. N-Triples cannot hold it.

## Read from a file

`RDFGraph.FromFileAsync` is the mirror of `ToFileAsync`: the format you name must be the one of the file. The winery's N-Triples arrive like this:

```csharp
RDFGraph received = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.NTriples, "vermentino.nt");
Console.WriteLine(received.TriplesCount);
Console.WriteLine(received.Context);
Console.WriteLine(received.Equals(wine));
```

<div class="result" markdown="1">

```
3
https://rdfsharp.codeplex.com/
True
```

</div>

All three triples are there and the graph is equal to the original, but its context is the default one: a file of N-Triples carries no name for the graph, so a graph that comes from one has to be named by whoever reads it. The three other formats bring the context back:

```csharp
RDFGraph fromTurtle = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vermentino.ttl");
RDFGraph fromTrix = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.TriX, "vermentino.trix");
RDFGraph fromRdfXml = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.RdfXml, "vermentino.rdf");
Console.WriteLine(fromTurtle.Context);
Console.WriteLine(fromTrix.Context);
Console.WriteLine(fromRdfXml.Context);
```

<div class="result" markdown="1">

```
https://vallerosa.example/catalog
https://vallerosa.example/catalog
https://vallerosa.example/catalog
```

</div>

A path that does not point to a file is refused with a message that names it:

```csharp
try
{
    await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "missing.ttl");
}
catch (RDFModelException exception)
{
    Console.WriteLine(exception.Message);
}
```

<div class="result" markdown="1">

```
Cannot read RDF graph from file because given "filepath" parameter (missing.ttl) does not indicate an existing file.
```

</div>

That is the whole exchange with the partner shop: the cooperative reads the winery's file, names the graph and writes the format that the shop asked for.

```csharp
received.SetContext(new Uri("https://vallerosa.example/catalog"));
await received.ToFileAsync(RDFModelEnums.RDFFormats.RdfXml, "for-the-shop.rdf");
```

## Streams

A file is only one place to put a graph. `ToStreamAsync` and `FromStreamAsync` work on any `Stream`: a memory buffer to send over a connection, a file opened by hand, the body of a web response.

```csharp
using (MemoryStream buffer = new MemoryStream())
{
    await wine.ToStreamAsync(RDFModelEnums.RDFFormats.RdfXml, buffer);
    using (MemoryStream input = new MemoryStream(buffer.ToArray()))
    {
        RDFGraph fromStream = await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.RdfXml, input);
        Console.WriteLine(fromStream.TriplesCount);
    }
}

using (FileStream output = new FileStream("stream.nt", FileMode.Create))
    await wine.ToStreamAsync(RDFModelEnums.RDFFormats.NTriples, output);
using (FileStream input = new FileStream("stream.nt", FileMode.Open))
    Console.WriteLine((await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.NTriples, input)).TriplesCount);
```

<div class="result" markdown="1">

```
3
3
```

</div>

Each method also has a synchronous form without the suffix: `ToFile`, `FromFile`, `ToStream` and `FromStream`.

## Read from a URI

`RDFGraph.FromUri(new Uri("https://..."))` dereferences an address: the library asks the server for an RDF representation, looks at the content type of the answer and picks the matching reader. It understands `application/rdf+xml`, `text/turtle` (and the two older Turtle names), `application/n-triples` and `application/trix`. The call waits 20 seconds by default and `FromUriAsync` does not block. Any failure of the network or of the parsing comes out as an `RDFModelException` whose message starts with `Cannot read RDF graph from Uri`.

One fact helps in practice. A namespace of the register can carry a **dereference URI**, an address to use instead of its own when the namespace IRI does not serve RDF: `FromUriAsync` looks the address up in the register before it asks.

This chapter does not call the network, so no example shows one. The cooperative has no use for it either: its partners send files.

## Prefixes come from the register

The Turtle and RDF/XML writers choose their prefixes from the register of namespaces: the standard vocabularies are already there and the two lines at the top of this page added the cooperative's. A namespace that the register does not know is not an error, only less readable. This is the same Turtle after taking the two prefixes out again:

```csharp
RDFNamespaceRegister.RemoveByPrefix("vr");
RDFNamespaceRegister.RemoveByPrefix("vrd");
await wine.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "unprefixed.ttl");
```

<div class="result" data-label="unprefixed.ttl" markdown="1">

```turtle
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@base <https://vallerosa.example/catalog>.

<https://vallerosa.example/id/vermentino-2023> rdfs:label "Vermentino of the Plain"@EN; 
                                               <https://vallerosa.example/vocab#producedBy> <https://vallerosa.example/id/tre-ulivi>; 
                                               <https://vallerosa.example/vocab#vintage> "2023Z"^^xsd:gYear. 
```

</div>

Turtle falls back to the complete IRIs. RDF/XML cannot do that, because an element name needs a prefix, so it invents one (`autoNS1`) for the namespace that it does not know. Registering the namespaces first, as at the top of this page, keeps both files readable. The [next chapter]({{ '/model/namespaces/' | relative_url }}) is about that register.
