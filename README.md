# RDFSharp [![NuGet Badge](https://buildstats.info/nuget/RDFSharp)](https://www.nuget.org/packages/RDFSharp)

RDFSharp has a modular API made up of three layers: 

<ul>
    <li><b>RDFSharp.Model</b></li> 
    <ul>
        <li>Create and manage <i>RDF models</i> (resources, literals, triples, graphs, namespaces, ...)</li>
        <li>Exchange them using standard <i>RDF formats</i> (N-Triples, TriX, Turtle, RDF/Xml)</li>
        <li>Create and validate <i>SHACL shapes</i> (shape graphs, shapes, targets, constraints, ...) <b>-->In Progress</b></li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Store</b></li> 
    <ul>
        <li>Create and manage <i>RDF stores</i> backing data on memory or SQL Server (see extensions)</li>
        <li>Exchange them using standard <i>RDF formats</i> (N-Quads, TriX)</li>
        <li>Create and manage <i>RDF federations</i> giving integrated query access to multiple stores</li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Query</b></li> 
    <ul>
        <li>Create and execute <i>SPARQL queries</i> on graphs, stores, federations and <i>SPARQL endpoints</i></li>
    </ul>
</ul>

RDFSharp also provides a <b>[semantic framework](https://github.com/mdesalvo/RDFSharp.Semantics)</b> for <i>OWL-DL/SKOS ontology</i> modeling, validation and reasoning!
