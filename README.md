# RDFSharp [![NuGet Badge](https://buildstats.info/nuget/RDFSharp)](https://www.nuget.org/packages/RDFSharp) [![codecov](https://codecov.io/gh/mdesalvo/RDFSharp/branch/master/graph/badge.svg?token=wtP1B77d3e)](https://codecov.io/gh/mdesalvo/RDFSharp)

RDFSharp has a modular API made up of 3 layers: 

<b><a href="https://github.com/mdesalvo/RDFSharp/releases/download/v3.1.0/RDFSharp.Model-3.1.0.pdf">RDFSharp.Model</a></b>
<ul>
    <li>Create and manage <b>RDF models</b> (resources, literals, triples, graphs, namespaces, ...)</li>
    <li>Exchange them using standard <b>RDF formats</b> (N-Triples, TriX, Turtle, RDF/Xml)</li>
    <li>Create and validate <b>SHACL shapes</b> (shape graphs, shapes, targets, constraints, reports, ...)</b></li>
</ul>

<b><a href="https://github.com/mdesalvo/RDFSharp/releases/download/v3.1.0/RDFSharp.Store-3.1.0.pdf">RDFSharp.Store</a></b>
<ul>
    <li>Create and manage <b>RDF stores</b> for context-aware modeling of RDF data (contexts, quadruples, ...)</li>
    <li>Exchange them using standard <b>RDF formats</b> (N-Quads, TriX, TriG)</li>
    <li>Pick an <a href="https://github.com/mdesalvo/RDFSharp.Extensions">extension</a> to store RDF data on many supported providers</li>
</ul>

<b><a href="https://github.com/mdesalvo/RDFSharp/releases/download/v3.1.0/RDFSharp.Query-3.1.0.pdf">RDFSharp.Query</a></b>
<ul>
    <li>Create and execute <b>SPARQL queries</b> on graphs, stores, federations and <i>SPARQL endpoints</i></li>
    <li>Create and execute <b>SPARQL operations</b> on graphs, stores and <i>SPARQL UPDATE endpoints</i></li>
</ul>

<hr/>
An additional <a href="https://github.com/mdesalvo/RDFSharp.Semantics">layer</a> is also available for working in a powerful and natural way with <b>OWL-DL ontologies</b>
