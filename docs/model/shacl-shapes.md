---
title: SHACL shapes
permalink: /model/shacl-shapes/
tagline: Write the rules of a graph as data
lede: SHACL describes what a valid graph looks like. You build shapes with targets and constraints and RDFSharp keeps them as a graph of their own that you can store and read back.
---

> The cooperative has rules for its catalog: a wine has one vintage, an alcohol between 5 and 20 degrees, labels in Italian or English, at least one producer and a price above zero. Until now they lived in somebody's head. Now they become a shapes graph that the machine can check.
{: .story}

## What a shape is

**SHACL** (Shapes Constraint Language) is the W3C standard for saying what a graph must look like. A rule is a **shape** and a shape has three parts:
- its **targets**, which choose the nodes that the shape is about (the **focus nodes**)
- its **constraints**, which are the conditions that every focus node must satisfy
- its **severity**, which says how serious a failure is: `Violation` by default, or `Warning` or `Info`

A collection of shapes is a **shapes graph**, an `RDFShapesGraph`. RDFSharp implements SHACL Core with the SPARQL-based targets and constraints. All the types of this chapter live in `RDFSharp.Model`, except the property path that a property shape needs, which comes from `RDFSharp.Query`.

There are two kinds of shape. A **node shape** (`RDFNodeShape`) talks about the focus node itself. A **property shape** (`RDFPropertyShape`) talks about the values that the focus node has along a **path**, usually one property. The usual design is one node shape for the thing and one property shape for each of its properties, joined by a property constraint.

## The shape of a wine

The node shape is about every `vr:Wine`. It is named, so that a report can point to it and it targets a class: the focus nodes are the instances of `vr:Wine`.

```csharp
using RDFSharp.Model;
using RDFSharp.Query;

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");
RDFNamespaceRegister.AddNamespace(vr);
RDFNamespaceRegister.AddNamespace(vrd);

RDFPropertyPath PathOf(RDFResource property)
    => new RDFPropertyPath(new RDFVariable("start"), new RDFVariable("end"))
        .AddStep(RDFPropertyPathStep.Link(property));

RDFNodeShape wineShape = new RDFNodeShape(new RDFResource(vr + "WineShape"));
wineShape.AddTarget(new RDFTargetClass(new RDFResource(vr + "Wine")));
wineShape.AddMessage(new RDFPlainLiteral("A wine of the catalog does not follow the rules of the cooperative", "en"));
Console.WriteLine(wineShape);
Console.WriteLine(wineShape.TargetsCount);
```

<div class="result" markdown="1">

```
https://vallerosa.example/vocab#WineShape
1
```

</div>

`PathOf` is a small function of the example, not of the library. A property shape wants a path and the path of a plain property is a link: a path from a start to an end with one step along the property. Longer paths (a sequence, an alternative, an inverse) are built with the other `RDFPropertyPathStep` methods and are not needed here.

The five rules of the cooperative become five property shapes, one for each property:

```csharp
RDFPropertyShape vintageShape = new RDFPropertyShape(new RDFResource(vr + "WineVintageShape"), PathOf(new RDFResource(vr + "vintage")));
vintageShape.AddConstraint(new RDFMinCountConstraint(1));
vintageShape.AddConstraint(new RDFMaxCountConstraint(1));
vintageShape.AddConstraint(new RDFDatatypeConstraint(RDFModelEnums.RDFDatatypes.XSD_GYEAR));

RDFPropertyShape alcoholShape = new RDFPropertyShape(new RDFResource(vr + "WineAlcoholShape"), PathOf(new RDFResource(vr + "alcohol")));
alcoholShape.AddConstraint(new RDFDatatypeConstraint(RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
alcoholShape.AddConstraint(new RDFMinInclusiveConstraint(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
alcoholShape.AddConstraint(new RDFMaxInclusiveConstraint(new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));

RDFPropertyShape labelShape = new RDFPropertyShape(new RDFResource(vr + "WineLabelShape"), PathOf(RDFVocabulary.RDFS.LABEL));
labelShape.AddConstraint(new RDFLanguageInConstraint(new List<string> { "it", "en" }));
labelShape.AddConstraint(new RDFUniqueLangConstraint(true));

RDFPropertyShape producerShape = new RDFPropertyShape(new RDFResource(vr + "WineProducerShape"), PathOf(new RDFResource(vr + "producedBy")));
producerShape.AddConstraint(new RDFMinCountConstraint(1));
producerShape.AddConstraint(new RDFClassConstraint(new RDFResource(vr + "Producer")));

RDFPropertyShape priceShape = new RDFPropertyShape(new RDFResource(vr + "WinePriceShape"), PathOf(new RDFResource(vr + "price")));
priceShape.AddConstraint(new RDFMinExclusiveConstraint(new RDFTypedLiteral("0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)));
Console.WriteLine(vintageShape.ConstraintsCount);
```

<div class="result" markdown="1">

```
3
```

</div>

Read them in order:
- the vintage: exactly one (a minimum and a maximum of one) and of datatype `xsd:gYear`
- the alcohol: a decimal that is at least 5 and at most 20
- the label: only in `it` or `en` and at most one for each language
- the producer: at least one and each of them an instance of `vr:Producer`
- the price: strictly greater than 0

Every `AddConstraint` is one SHACL constraint. The bounds are typed literals, because a bound of `5` as text would not be comparable to a decimal. Then the property shapes are attached to the node shape and everything goes into the shapes graph:

```csharp
wineShape.AddConstraint(new RDFPropertyConstraint(vintageShape));
wineShape.AddConstraint(new RDFPropertyConstraint(alcoholShape));
wineShape.AddConstraint(new RDFPropertyConstraint(labelShape));
wineShape.AddConstraint(new RDFPropertyConstraint(producerShape));
wineShape.AddConstraint(new RDFPropertyConstraint(priceShape));

RDFShapesGraph shapes = new RDFShapesGraph(new RDFResource(vr + "WineShapes"));
shapes.AddShape(wineShape);
shapes.AddShape(vintageShape);
shapes.AddShape(alcoholShape);
shapes.AddShape(labelShape);
shapes.AddShape(producerShape);
shapes.AddShape(priceShape);
Console.WriteLine(shapes.ShapesCount);
```

<div class="result" markdown="1">

```
6
```

</div>

The `RDFPropertyConstraint` is the link between the two kinds of shape: the focus node of the node shape must conform to the property shape. The property shapes go into the shapes graph as well, as the node shape refers to them by name.

> The `Add...` methods of a shape return the base type `RDFShape`, which is why the example calls them as statements and does not chain them into the variable of the derived type.
{: .tip}

## The shapes graph as RDF

SHACL shapes are RDF and `ToRDFGraph` shows it: the shapes graph becomes an ordinary graph that every serializer of the [formats chapter]({{ '/model/formats/' | relative_url }}) can write. `RDFShapesGraph.FromRDFGraph` goes the other way, so the rules can be kept in a file, read by anybody and loaded at startup.

```csharp
RDFGraph shapesGraph = shapes.ToRDFGraph().SetContext(new Uri("https://vallerosa.example/shapes"));
Console.WriteLine(shapesGraph.TriplesCount);
shapesGraph.ToFile(RDFModelEnums.RDFFormats.Turtle, "shapes-graph.ttl");

RDFShapesGraph fromFile = RDFShapesGraph.FromRDFGraph(RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, "shapes-graph.ttl"));
Console.WriteLine(fromFile.ShapesCount);
```

<div class="result" markdown="1">

```
47
6
```

</div>

<div class="result" data-label="shapes-graph.ttl" markdown="1">

```turtle
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix sh: <http://www.w3.org/ns/shacl#>.
@prefix vr: <https://vallerosa.example/vocab#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@base <https://vallerosa.example/shapes>.

_:6b7ff071fedb4088ab800f2857cafc47 rdf:first "EN"; 
                                   rdf:rest rdf:nil; 
                                   a rdf:List. 
_:a6efe218a9e84c0cb7dbb23266315650 rdf:first "IT"; 
                                   rdf:rest _:6b7ff071fedb4088ab800f2857cafc47; 
                                   a rdf:List. 
vr:WineAlcoholShape a sh:PropertyShape; 
                    sh:datatype xsd:decimal; 
                    sh:deactivated "false"^^xsd:boolean; 
                    sh:maxInclusive "20"^^xsd:decimal; 
                    sh:minInclusive "5"^^xsd:decimal; 
                    sh:path vr:alcohol; 
                    sh:severity sh:Violation. 
vr:WineLabelShape a sh:PropertyShape; 
                  sh:deactivated "false"^^xsd:boolean; 
                  sh:languageIn _:a6efe218a9e84c0cb7dbb23266315650; 
                  sh:path rdfs:label; 
                  sh:severity sh:Violation; 
                  sh:uniqueLang "true"^^xsd:boolean. 
vr:WinePriceShape a sh:PropertyShape; 
                  sh:deactivated "false"^^xsd:boolean; 
                  sh:minExclusive "0"^^xsd:decimal; 
                  sh:path vr:price; 
                  sh:severity sh:Violation. 
vr:WineProducerShape a sh:PropertyShape; 
                     sh:class vr:Producer; 
                     sh:deactivated "false"^^xsd:boolean; 
                     sh:minCount "1"^^xsd:integer; 
                     sh:path vr:producedBy; 
                     sh:severity sh:Violation. 
vr:WineShape a sh:NodeShape; 
             sh:deactivated "false"^^xsd:boolean; 
             sh:message "A wine of the catalog does not follow the rules of the cooperative"@EN; 
             sh:property vr:WineVintageShape, vr:WineAlcoholShape, vr:WineLabelShape, vr:WineProducerShape, vr:WinePriceShape; 
             sh:severity sh:Violation; 
             sh:targetClass vr:Wine. 
vr:WineVintageShape a sh:PropertyShape; 
                    sh:datatype xsd:gYear; 
                    sh:deactivated "false"^^xsd:boolean; 
                    sh:maxCount "1"^^xsd:integer; 
                    sh:minCount "1"^^xsd:integer; 
                    sh:path vr:vintage; 
                    sh:severity sh:Violation. 
```

</div>

The file tells everything the C# says. Each shape is a resource of class `sh:NodeShape` or `sh:PropertyShape`, each constraint is a property of it (`sh:minCount`, `sh:datatype`, `sh:languageIn`...) and the node shape points to its property shapes with `sh:property`. The list of languages is an RDF list of the kind that the [containers chapter]({{ '/model/containers/' | relative_url }}) shows, with its blank cells. The language codes come out in upper case as everywhere in the library and the check ignores the case of a tag.

The file also writes down what the library fills in by default: `sh:deactivated` is false and `sh:severity` is `sh:Violation`. A shape can be switched off with `Deactivate()`, given another severity with `SetSeverity` and given messages with `AddMessage`, the text for the people who read a report, as the `sh:message` of the wine shape above. A property shape can also be described with `AddName`, `AddDescription`, `SetOrder` and `SetGroup`, which help the tools that build forms out of shapes.

## The targets

A target decides which nodes a shape looks at. RDFSharp has five:

| Target | Chooses |
|---|---|
| `RDFTargetClass(class)` | the instances of the class, as in the wine shape |
| `RDFTargetNode(resource)` | exactly the given resource |
| `RDFTargetSubjectsOf(property)` | every node that is the subject of a triple with the property |
| `RDFTargetObjectsOf(property)` | every node that is the object of a triple with the property |
| `RDFTargetSPARQL(query)` | the nodes that a SPARQL `SELECT` returns in its `?this` variable |

A shape may have several targets and the focus nodes are the union of them. A property shape that is only used through a node shape needs none of its own.

## The constraints

The constraints come in a few families. The page uses ten of the 29 that RDFSharp offers. The other nineteen follow the same pattern: build the object, pass it to `AddConstraint`.

| Family | Constraint | Requires |
|---|---|---|
| Cardinality | `RDFMinCountConstraint`, `RDFMaxCountConstraint` | a minimum and a maximum number of values (used above) |
| Value type | `RDFDatatypeConstraint`, `RDFClassConstraint` | literals of a datatype and resources of a class (used above) |
| | `RDFNodeKindConstraint` | values of a kind: IRI, blank node, literal or a combination |
| Value range | `RDFMinInclusiveConstraint`, `RDFMaxInclusiveConstraint`, `RDFMinExclusiveConstraint` | bounds (used above) |
| | `RDFMaxExclusiveConstraint` | values strictly below a bound |
| Strings | `RDFLanguageInConstraint`, `RDFUniqueLangConstraint` | tags from a list and one value for each tag (used above) |
| | `RDFPatternConstraint` | values that match a .NET regular expression |
| | `RDFMinLengthConstraint`, `RDFMaxLengthConstraint` | a minimum and a maximum length of the text |
| Pairs of properties | `RDFEqualsConstraint` | the same values as another property |
| | `RDFDisjointConstraint` | no value in common with another property |
| | `RDFLessThanConstraint`, `RDFLessThanOrEqualsConstraint` | values less than (or equal to) those of another property |
| Values | `RDFHasValueConstraint` | one given value among the values |
| | `RDFInConstraint` | values taken from a closed list |
| Shapes | `RDFPropertyConstraint` | conformance to a property shape (used above) |
| | `RDFNodeConstraint` | conformance to a node shape |
| | `RDFQualifiedValueShapeConstraint` | a minimum and a maximum number of values that conform to a shape |
| Logic | `RDFNotConstraint` | not to conform to a shape |
| | `RDFAndConstraint`, `RDFOrConstraint`, `RDFXoneConstraint` | all, at least one or exactly one of a list of shapes |
| Closure | `RDFClosedConstraint` | no property beyond those of the property shapes and an ignored list |
| SPARQL | `RDFSPARQLConstraint` | a `SELECT` with `?this` that returns no row for a conforming node |

The shapes of the cooperative are ready. [The next chapter]({{ '/model/shacl-validation/' | relative_url }}) applies them to the catalog.
