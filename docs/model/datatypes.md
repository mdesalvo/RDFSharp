---
title: Datatypes and facets
permalink: /model/datatypes/
tagline: What a typed literal may say and how to narrow it
lede: Every typed literal is checked against its datatype before it exists. You can add datatypes of your own, narrowed by facets, so that impossible values never enter a graph.
---

> A spreadsheet import brings a new wine into the catalog with an alcohol of 135. Somebody dropped the decimal point of 13.5. The cooperative teaches the library what an alcohol content looks like, so that the mistake stops at the door.
{: .story}

## The built-in datatypes

A **datatype** says what kind of value a literal holds and which spellings of it are valid. RDFSharp knows 52 of them, all listed in the enumeration `RDFModelEnums.RDFDatatypes`: the XML Schema ones (strings, booleans, the many kinds of number, dates and times, binary data, IRIs), plus a few of RDF (`rdf:HTML`, `rdf:JSON`, `rdf:XMLLiteral`), GeoSPARQL (`geo:wktLiteral`, `geo:gmlLiteral`), OWL and the time ontology. The register of datatypes holds them all, finds one by enumeration value or by IRI and walks them with `RDFDatatypeRegister.DatatypesEnumerator`.

```csharp
using RDFSharp.Model;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFNamespaceRegister.AddNamespace(vr);
RDFNamespaceRegister.AddNamespace(vrd);

Console.WriteLine(Enum.GetValues<RDFModelEnums.RDFDatatypes>().Length);
Console.WriteLine(RDFDatatypeRegister.DatatypesCount);
Console.WriteLine(RDFDatatypeRegister.GetDatatype(RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
```

<div class="result" markdown="1">

```
52
52
http://www.w3.org/2001/XMLSchema#decimal
```

</div>

## A value that does not fit

You met the check in the [first chapter]({{ '/model/triples/' | relative_url }}): the constructor of `RDFTypedLiteral` validates the value against the datatype and throws an `RDFModelException` when it is not well formed. The message names the value and the datatype:

```csharp
try
{
    RDFTypedLiteral twelveAndAHalf = new RDFTypedLiteral("twelve and a half", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
}
catch (RDFModelException ex)
{
    Console.WriteLine(ex.Message);
}
```

<div class="result" markdown="1">

```
Cannot create RDFTypedLiteral because given "value" parameter (twelve and a half) is not well-formed against given "datatype" parameter (http://www.w3.org/2001/XMLSchema#decimal) which is based on "XSD_DECIMAL" standard datatype
```

</div>

The consequence is a guarantee: a typed literal that exists is a valid one and nothing downstream (a query, a shape, a file) ever meets `"twelve and a half"` as a decimal. The check is only as strict as the datatype, though. To XML Schema 135 is a perfectly good decimal:

```csharp
RDFTypedLiteral excessive = new RDFTypedLiteral("135", RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
Console.WriteLine(excessive);
```

<div class="result" markdown="1">

```
135^^http://www.w3.org/2001/XMLSchema#decimal
```

</div>

A wine with 135 degrees of alcohol would go straight into the catalog. What the cooperative needs is a datatype that means "alcohol by volume" and nothing else.

## A custom datatype

An `RDFDatatype` takes an IRI of your own, a **target datatype** to start from and a list of **facets**. A facet is one more condition on the value: a bound, a length, a pattern. The value must satisfy the target datatype first and then every facet. Alcohol by volume is a decimal from 0 to 20, both included:

```csharp
RDFDatatype abv = new RDFDatatype(new Uri(vr + "abv"), RDFModelEnums.RDFDatatypes.XSD_DECIMAL,
    new List<RDFFacet> { new RDFMinInclusiveFacet(0), new RDFMaxInclusiveFacet(20) });
RDFDatatypeRegister.AddDatatype(abv);

Console.WriteLine(RDFDatatypeRegister.DatatypesCount);
Console.WriteLine(RDFDatatypeRegister.GetDatatype(vr + "abv").TargetDatatype);
```

<div class="result" markdown="1">

```
53
XSD_DECIMAL
```

</div>

Registering the datatype is not needed to use it: you can pass `abv` straight to a typed literal. It is worth doing, because the register is the library's list of known datatypes, found by IRI like the namespaces of the previous chapter. A datatype whose IRI is already registered is not added a second time.

Here are the borders of the new datatype:

```csharp
foreach (string value in new string[] { "0", "13.5", "20", "20.5", "-1", "135" })
{
    try
    {
        RDFTypedLiteral alcohol = new RDFTypedLiteral(value, abv);
        Console.WriteLine($"{value} accepted");
    }
    catch (RDFModelException)
    {
        Console.WriteLine($"{value} refused");
    }
}
```

<div class="result" markdown="1">

```
0 accepted
13.5 accepted
20 accepted
20.5 refused
-1 refused
135 refused
```

</div>

The bounds are inclusive, so 0 and 20 pass. A target datatype with no facets, passed as `null` instead of a list, makes an **alias**: a new name for an existing datatype, with the same rules.

## The import that failed

Back to the spreadsheet. The new wine is `vrd:nero-2024` and its alcohol arrives as the text `135`. The import builds the literal with the datatype of the cooperative and the library refuses it. The row is kept out of the graph and the next line, with the corrected value, goes in:

```csharp
RDFResource nero = new RDFResource(vrd + "nero-2024");
RDFResource hasAlcohol = new RDFResource(vr + "alcohol");
RDFGraph delivery = new RDFGraph().SetContext(new Uri("https://vallerosa.example/catalog"));

try
{
    delivery.AddTriple(new RDFTriple(nero, hasAlcohol, new RDFTypedLiteral("135", abv)));
}
catch (RDFModelException ex)
{
    Console.WriteLine(ex.Message);
}
Console.WriteLine(delivery.TriplesCount);

delivery.AddTriple(new RDFTriple(nero, hasAlcohol, new RDFTypedLiteral("13.5", abv)));
Console.WriteLine(delivery.TriplesCount);
Console.WriteLine((await delivery.SelectTriplesAsync(s: nero))[0].Object);
```

<div class="result" markdown="1">

```
Cannot create RDFTypedLiteral because given "value" parameter (135) is not well-formed against given "datatype" parameter (https://vallerosa.example/vocab#abv) which is based on "XSD_DECIMAL" standard datatype
0
1
13.5^^https://vallerosa.example/vocab#abv
```

</div>

The error arrives at the earliest possible moment, when the wrong value is turned into a literal and it names the datatype of the cooperative. The graph never held the bad value.

> The wine now has `vr:abv` as the datatype of its alcohol, while the catalog so far says `xsd:decimal`. The [validation chapters]({{ '/model/shacl-shapes/' | relative_url }}) check the data that is already in a graph, which is the other half of the guard.
{: .tip}

## The facets

RDFSharp has eight facets, all following the XML Schema ones. Each is an `RDFFacet` that you pass in the list of a datatype:

| Facet | Requires the value to |
|---|---|
| `RDFMinInclusiveFacet(double)` | be a number greater than or equal to the bound |
| `RDFMinExclusiveFacet(double)` | be a number greater than the bound |
| `RDFMaxInclusiveFacet(double)` | be a number less than or equal to the bound |
| `RDFMaxExclusiveFacet(double)` | be a number less than the bound |
| `RDFLengthFacet(uint)` | have exactly the given number of characters |
| `RDFMinLengthFacet(uint)` | have at least the given number of characters |
| `RDFMaxLengthFacet(uint)` | have at most the given number of characters |
| `RDFPatternFacet(string)` | match the given regular expression |

The four numeric bounds read the value as a number, so they suit the numeric datatypes. The length facets count the characters of the written value and the pattern is a .NET regular expression. Several facets in one list all have to hold.

## A datatype as RDF

A datatype is knowledge too and `AddDatatype` writes it into a graph: its IRI as an `rdfs:Datatype`, the target it narrows and the facets as a list of restrictions. The cooperative would publish this next to its vocabulary:

```csharp
RDFGraph vocabulary = new RDFGraph()
    .SetContext(new Uri("https://vallerosa.example/vocab"))
    .AddDatatype(abv);
await vocabulary.ToFileAsync(RDFModelEnums.RDFFormats.Turtle, "abv.ttl");
Console.WriteLine(vocabulary.TriplesCount);
```

<div class="result" markdown="1">

```
11
```

</div>

<div class="result" data-label="abv.ttl" markdown="1">

```turtle
@prefix owl: <http://www.w3.org/2002/07/owl#>.
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@base <https://vallerosa.example/vocab>.

_:7206b92c18f04c0ebcf56e5ad9480b0c xsd:maxInclusive "20"^^xsd:double. 
_:772c2abbe159431c9840deb0f465181c xsd:minInclusive "0"^^xsd:double. 
_:a3c07fe993c2476b862eac03e0146b49 rdf:first _:7206b92c18f04c0ebcf56e5ad9480b0c; 
                                   rdf:rest rdf:nil; 
                                   a rdf:List. 
_:f9933c4b0684406ab168ac3888c23115 rdf:first _:772c2abbe159431c9840deb0f465181c; 
                                   rdf:rest _:a3c07fe993c2476b862eac03e0146b49; 
                                   a rdf:List. 
vr:abv a rdfs:Datatype; 
       owl:onDatatype xsd:decimal; 
       owl:withRestrictions _:f9933c4b0684406ab168ac3888c23115. 
```

</div>

The names that start with `_:` are blank resources, invented anew at every run. The reader of the file does not need the datatype in its register: `RDFGraph.FromFileAsync`, `FromStreamAsync` and `FromUriAsync` take a last argument, `enableDatatypeDiscovery` and when it is `true` the library finds the datatype definitions in the data and registers them before it moves on.
