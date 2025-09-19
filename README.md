# RDFSharp <a href="https://www.nuget.org/packages/RDFSharp"><img src="https://img.shields.io/nuget/dt/RDFSharp?style=flat&color=abcdef&logo=nuget&label=downloads"/></a> [![codecov](https://codecov.io/gh/mdesalvo/RDFSharp/graph/badge.svg?token=J3KNYAXTZC)](https://codecov.io/gh/mdesalvo/RDFSharp)

RDFSharp is a modular .NET library made up of 3 layers: 

<b><a href="https://github.com/mdesalvo/RDFSharp/releases/download/v3.22.0/RDFSharp.Model-3.22.pdf">RDFSharp.Model</a></b>
<ul>
    <li>Create and manage <b>RDF models</b> (resources, literals, triples, graphs, namespaces, datatypes, ...)</li>
    <li>Exchange them using standard <b>RDF formats</b> (N-Triples, TriX, Turtle, RDF/Xml)</li>
    <li>Create and validate <b>SHACL shapes</b> (shape graphs, shapes, targets, constraints, reports, ...)</b></li>
</ul>

<b><a href="https://github.com/mdesalvo/RDFSharp/releases/download/v3.22.0/RDFSharp.Store-3.22.pdf">RDFSharp.Store</a></b>
<ul>
    <li>Create and manage <b>RDF stores</b> for context-aware modeling of RDF data (contexts, quadruples, ...)</li>
    <li>Exchange them using standard <b>RDF formats</b> (N-Quads, TriX, TriG)</li>
    <li>Pick a store <a href="https://github.com/mdesalvo/RDFSharp.Extensions">extension</a> to save and query big RDF data on many supported providers</li>
</ul>

<b><a href="https://github.com/mdesalvo/RDFSharp/releases/download/v3.22.0/RDFSharp.Query-3.22.pdf">RDFSharp.Query</a></b>
<ul>
    <li>Create and execute <b>SPARQL queries</b> on graphs, stores, federations and SPARQL endpoints</li>
    <li>Create and execute <b>SPARQL operations</b> on graphs, stores and SPARQL UPDATE endpoints</li>
</ul>
<hr/>
The project also delivers a <a href="https://github.com/mdesalvo/OWLSharp">library</a> for working with <b>OWL2 ontologies</b>!
