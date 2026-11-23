---
title: RDFSharp
permalink: /
lede: "RDFSharp is a lightweight .NET library for RDF data: model it, store it with its provenance, query it with SPARQL and validate it with SHACL."
---

<div class="hero" markdown="0">
  <h1>RDF, SPARQL and SHACL, <em>in plain C#</em></h1>
  <p>RDFSharp lets a .NET application speak RDF. You build graphs from resources and literals, keep them in stores that remember where each fact came from, ask them questions in SPARQL and check them against SHACL shapes. Every page of this manual shows it on one real case, the catalog of a wine cooperative that grows chapter after chapter.</p>
</div>

## A first taste

The cooperative wants its red wines under 20 euros, with their English names. Here is the question built in C# with RDFSharp's query engine, next to the same question in SPARQL.

<div class="pair" markdown="1">

```csharp
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");

RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFVariable wine = new RDFVariable("wine");
RDFVariable label = new RDFVariable("label");
RDFVariable price = new RDFVariable("price");

RDFSelectQuery query = new RDFSelectQuery()
    .AddPrefix(vr)
    .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDF.TYPE, new RDFResource(vr + "RedWine")))
        .AddPattern(new RDFPattern(wine, RDFVocabulary.RDFS.LABEL, label))
        .AddPattern(new RDFPattern(wine, new RDFResource(vr + "price"), price))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(new RDFPlainLiteral("en")))))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            price, new RDFConstantExpression(new RDFTypedLiteral("20", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))))))
    .AddProjectionVariable(label)
    .AddProjectionVariable(price);

RDFSelectQueryResult result = await query.ApplyToGraphAsync(catalog);
```

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?label ?price
WHERE {
  ?wine a vr:RedWine ;
        rdfs:label ?label ;
        vr:price ?price .
  FILTER (LANGMATCHES(LANG(?label), "en"))
  FILTER (?price < 20)
}
```

</div>

<div class="result" markdown="1">

| ?LABEL | ?PRICE |
|---|---|
| `Vallerosa Red@EN` | `14.50^^http://www.w3.org/2001/XMLSchema#decimal` |

</div>

The Reserve costs 32 euros and the other wines are not red, so one row comes back. [Getting started](getting-started/) walks through this example from an empty project.

## Three layers, one library

RDFSharp ships as a single NuGet package targeting `netstandard2.0`, `net8.0` and `net10.0`. Inside it, three layers build on each other.

<div class="cards three">
  <a class="card" href="{{ '/model/' | relative_url }}"><span class="kind">Model</span><h3>Describe the data</h3><p>Resources, literals, triples and graphs. Namespaces and datatypes. The standard RDF formats. SHACL shapes to say what good data looks like.</p></a>
  <a class="card" href="{{ '/store/' | relative_url }}"><span class="kind">Store</span><h3>Remember where it came from</h3><p>Quadruples add a context to every triple, so the same fact told by two sources stays two facts. In memory or on a database.</p></a>
  <a class="card" href="{{ '/query/' | relative_url }}"><span class="kind">Query</span><h3>Ask and change it</h3><p>SPARQL 1.1 queries and updates, composed in C# or parsed from text, over graphs, stores, federations and remote endpoints.</p></a>
</div>

## The family

<div class="cards">
  <a class="card" href="https://github.com/mdesalvo/RDFSharp.Extensions"><span class="kind">Storage</span><h3>RDFSharp.Extensions</h3><p>Store providers that keep quadruples on real databases, from SQLite, PostgreSQL and SQL Server to MongoDB, Neo4j and Azure Table, behind the same <code>RDFStore</code> API.</p></a>
  <a class="card" href="https://github.com/mdesalvo/OWLSharp"><span class="kind">Ontologies</span><h3>OWLSharp</h3><p>OWL2 ontologies, reasoning and SWRL rules, built on top of RDFSharp. Its own manual picks up the Vallerosa case where this one leaves it.</p></a>
</div>
