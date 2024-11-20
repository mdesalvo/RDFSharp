# RDFSharp <a href="https://github.com/mdesalvo/RDFSharp/releases"><img src="https://img.shields.io/nuget/v/RDFSharp?style=flat-square&color=abcdef&logo=nuget&label=version"/></a> <a href="https://www.nuget.org/packages/RDFSharp"><img src="https://img.shields.io/nuget/dt/RDFSharp?style=flat-square&color=abcdef&logo=nuget&label=downloads"/></a> <a href="https://app.codecov.io/gh/mdesalvo/RDFSharp"><img src="https://img.shields.io/codecov/c/github/mdesalvo/RDFSharp?style=flat-square&color=04aa6d&logo=codecov&label=coverage"/></a>

RDFSharp is a modular API made up of 3 layers: 

<b>RDFSharp.Model</b>
<ul>
    <li>Create and manage <b><a href="https://www.w3.org/TR/rdf11-primer/">RDF models</a></b> (resources, literals, triples, graphs, namespaces, datatypes, ...)</li>
    <li>Exchange them using standard <b>RDF formats</b> (N-Triples, TriX, Turtle, RDF/Xml)</li>
    <li>Create and validate <b><a href="https://www.w3.org/TR/shacl/">SHACL shapes</a></b> (shape graphs, shapes, targets, constraints, reports, ...)</b></li>
</ul>

<b>RDFSharp.Store</b>
<ul>
    <li>Create and manage <b>RDF stores</b> for context-aware modeling of RDF data (contexts, quadruples, ...)</li>
    <li>Exchange them using standard <b>RDF formats</b> (N-Quads, TriX, TriG)</li>
    <li>Pick a store <a href="https://github.com/mdesalvo/RDFSharp.Extensions">extension</a> to save and query RDF data on many supported providers</li>
</ul>

<b>RDFSharp.Query</b>
<ul>
    <li>Create and execute <b><a href="https://www.w3.org/TR/sparql11-query/">SPARQL queries</a></b> on graphs, stores, federations and SPARQL endpoints</li>
    <li>Create and execute <b><a href="https://www.w3.org/TR/sparql11-update/">SPARQL operations</a></b> on graphs, stores and SPARQL UPDATE endpoints</li>
</ul>
<hr/>
An additional <a href="https://github.com/mdesalvo/OWLSharp">API</a> is available for working in expressive way with <b>OWL2 ontologies</b>
