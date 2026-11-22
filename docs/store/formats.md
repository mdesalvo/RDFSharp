---
title: Store formats
permalink: /store/formats/
tagline: Saving a store without losing its contexts
lede: A plain triple file loses the context of every fact. N-Quads, TriX and TriG are the three standard formats that keep it. A store reads and writes all of them.
---

> The cooperative wants to hand the scores of the guides to a partner shop. Which file format the partner reads is the partner's choice, so the store has to speak all three.
{: .story}

## Three formats for quadruples

The formats of the [Model]({{ '/model/formats/' | relative_url }}) chapter write a graph. A graph has one context at most. A store has as many as it likes, so it needs formats with room for a fourth element. RDFSharp reads and writes three, chosen with the `RDFStoreEnums.RDFFormats` enumeration:

| Format | Enum value | Media type | Shape |
|---|---|---|---|
| N-Quads | `RDFFormats.NQuads` | `application/n-quads` | one quadruple per line, every IRI written in full |
| TriX | `RDFFormats.TriX` | `application/trix` | XML: one `graph` element for each context |
| TriG | `RDFFormats.TriG` | `application/trig` | Turtle with prefixes, one block for each context |

N-Quads is the easiest for programs to produce and to split into chunks. TriX suits XML tools. TriG is the one people read.

## The store to save

The example of this page is small on purpose: the scores that the two guides gave to the Vallerosa Red and to the Valley Vin Santo. Four quadruples, cut out of the guides file with the indexer. The cooperative's two namespaces go in the register first, so that TriG can use their prefixes.

```csharp
using System.IO;
using System.Text;
using RDFSharp.Model;
using RDFSharp.Store;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFNamespaceRegister.AddNamespace(vr);
RDFNamespaceRegister.AddNamespace(vrd);

RDFMemoryStore guides = RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.TriG, "vallerosa-guides.trig");
RDFMemoryStore scores = guides[s: new RDFResource(vrd + "rosso-2021")]
    .UnionWith(guides[s: new RDFResource(vrd + "vinsanto-2015")]);
Console.WriteLine(scores.QuadruplesCount);
```

<div class="result" markdown="1">

```
4
```

</div>

## N-Quads

`ToFile` writes a store to a file in the format you name. `FromFile` reads it back into a new memory store.

```csharp
scores.ToFile(RDFStoreEnums.RDFFormats.NQuads, "scores.nq");
Console.WriteLine(File.ReadAllText("scores.nq"));
```

<div class="result" data-label="scores.nq" markdown="1">

```
<https://vallerosa.example/id/rosso-2021> <https://vallerosa.example/vocab#score> "88"^^<http://www.w3.org/2001/XMLSchema#integer> <https://guidacalici.example/2025> .
<https://vallerosa.example/id/rosso-2021> <https://vallerosa.example/vocab#score> "90"^^<http://www.w3.org/2001/XMLSchema#integer> <https://winescope.example/2025> .
<https://vallerosa.example/id/vinsanto-2015> <https://vallerosa.example/vocab#score> "95"^^<http://www.w3.org/2001/XMLSchema#integer> <https://guidacalici.example/2025> .
<https://vallerosa.example/id/vinsanto-2015> <https://vallerosa.example/vocab#score> "96"^^<http://www.w3.org/2001/XMLSchema#integer> <https://winescope.example/2025> .
```

</div>

Every line is one quadruple: subject, predicate, object, context, closed by a dot. Note the order, which is the one of the standard: the context comes last, the opposite of the way RDFSharp prints a quadruple.

```csharp
RDFMemoryStore fromNQuads = RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.NQuads, "scores.nq");
Console.WriteLine(fromNQuads.QuadruplesCount);
Console.WriteLine(fromNQuads.ContainsQuadruple(new RDFQuadruple(
    new RDFContext("https://winescope.example/2025"),
    new RDFResource(vrd + "rosso-2021"),
    new RDFResource(vr + "score"),
    new RDFTypedLiteral("90", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
```

<div class="result" markdown="1">

```
4
True
```

</div>

## TriX

TriX is the XML one. Every context becomes a `graph` element that starts with the IRI of the context.

```csharp
scores.ToFile(RDFStoreEnums.RDFFormats.TriX, "scores.trix");
Console.WriteLine(File.ReadAllText("scores.trix"));
```

<div class="result" data-label="scores.trix" markdown="1">

```
<?xml version="1.0" encoding="utf-8"?>
<TriX xmlns="http://www.w3.org/2004/03/trix/trix-1/">
  <graph>
    <uri>https://guidacalici.example/2025</uri>
    <triple>
      <uri>https://vallerosa.example/id/rosso-2021</uri>
      <uri>https://vallerosa.example/vocab#score</uri>
      <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#integer">88</typedLiteral>
    </triple>
    <triple>
      <uri>https://vallerosa.example/id/vinsanto-2015</uri>
      <uri>https://vallerosa.example/vocab#score</uri>
      <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#integer">95</typedLiteral>
    </triple>
  </graph>
  <graph>
    <uri>https://winescope.example/2025</uri>
    <triple>
      <uri>https://vallerosa.example/id/rosso-2021</uri>
      <uri>https://vallerosa.example/vocab#score</uri>
      <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#integer">90</typedLiteral>
    </triple>
    <triple>
      <uri>https://vallerosa.example/id/vinsanto-2015</uri>
      <uri>https://vallerosa.example/vocab#score</uri>
      <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#integer">96</typedLiteral>
    </triple>
  </graph>
</TriX>
```

</div>

Resources are `uri` elements, literals are `plainLiteral` or `typedLiteral` according to their kind. A blank node would be an `id`. Reading is the same call with the same enumeration value:

```csharp
RDFMemoryStore fromTriX = RDFMemoryStore.FromFile(RDFStoreEnums.RDFFormats.TriX, "scores.trix");
Console.WriteLine(fromTriX.QuadruplesCount);
```

<div class="result" markdown="1">

```
4
```

</div>

## TriG

TriG is Turtle with a block for each context. The registered prefixes shorten every IRI of the data. The context itself stays in full before its block.

```csharp
scores.ToFile(RDFStoreEnums.RDFFormats.TriG, "scores.trig");
Console.WriteLine(File.ReadAllText("scores.trig"));
```

<div class="result" data-label="scores.trig" markdown="1">

```
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix vrd: <https://vallerosa.example/id/>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.

GRAPH <https://guidacalici.example/2025>
{
  vrd:rosso-2021 vr:score "88"^^xsd:integer. 
  vrd:vinsanto-2015 vr:score "95"^^xsd:integer. 
}

GRAPH <https://winescope.example/2025>
{
  vrd:rosso-2021 vr:score "90"^^xsd:integer. 
  vrd:vinsanto-2015 vr:score "96"^^xsd:integer. 
}
```

</div>

The file the cooperative started from, [vallerosa-guides.trig]({{ '/assets/data/vallerosa-guides.trig' | relative_url }}), is TriG too, written by hand: it uses the shorthand `88` for an integer. The library writes the long form, which every reader understands.

## Streams

Files are the common case but not the only one: a web response or a message queue gives a stream and does not give a path. `ToStream` and `FromStream` take the place of `ToFile` and `FromFile`. Both close the stream when they finish, so keep the bytes if you need them again.

```csharp
MemoryStream outgoing = new MemoryStream();
scores.ToStream(RDFStoreEnums.RDFFormats.NQuads, outgoing);
byte[] bytes = outgoing.ToArray();
Console.WriteLine(Encoding.ASCII.GetString(bytes).Split('\n').Length - 1);

MemoryStream incoming = new MemoryStream(bytes);
RDFMemoryStore fromStream = RDFMemoryStore.FromStream(RDFStoreEnums.RDFFormats.NQuads, incoming);
Console.WriteLine(fromStream.QuadruplesCount);
```

<div class="result" markdown="1">

```
4
4
```

</div>

The first number is the count of lines of the N-Quads text, one for each quadruple. Each method has an `Async` twin: `ToFileAsync`, `FromFileAsync`, `ToStreamAsync` and `FromStreamAsync`.

## From the Web

A partner can also publish its store at an address. `RDFMemoryStore.FromUri` asks that address for a store and reads whatever comes back. It sends an `Accept` header that lists the three media types of the table above and picks the parser from the `Content-Type` of the answer. When the server states none, it reads the body as N-Quads. The second parameter is a timeout in milliseconds, 20 seconds by default.

```csharp
try
{
    RDFMemoryStore partner = RDFMemoryStore.FromUri(new Uri("https://partner.example/vallerosa-scores"), 5000);
    Console.WriteLine(partner.QuadruplesCount);
}
catch (RDFStoreException)
{
    Console.WriteLine("The partner does not answer.");
}
```

<div class="result" markdown="1">

```
The partner does not answer.
```

</div>

The address `partner.example` is reserved for examples and never answers, so the call fails with an `RDFStoreException` as it should whenever the network, the server or the content cannot be read. `FromUriAsync` is the asynchronous form.

> `FromFile`, `FromStream` and `FromUri` take an optional last parameter, `enableDatatypeDiscovery`. When it is `true`, the datatypes that the data declares are registered in the library as the data is read, which matters for the custom datatypes of the [Datatypes and facets]({{ '/model/datatypes/' | relative_url }}) chapter.
{: .tip}

> **Where next.** [Persistent stores]({{ '/store/extensions/' | relative_url }}) moves the cooperative's store from memory onto a database.
{: .tip}
