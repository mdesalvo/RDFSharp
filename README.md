# RDFSharp [![NuGet Badge](https://buildstats.info/nuget/RDFSharp)](https://www.nuget.org/packages/RDFSharp)

RDFSharp has a modular API made up of four layers ([Model-Store-Query](https://github.com/mdesalvo/RDFSharp/releases/download/v2.24.1/RDFSharp-2.24.1.pdf), [Semantics](https://github.com/mdesalvo/RDFSharp/releases/download/v2.24.1/RDFSharp.Semantics-2.24.1.pdf)): 

<ul>
    <li><b>RDFSharp.Model</b></li> 
    <ul>
        <li>Create and manage <b>RDF models</b> (resources, literals, triples, graphs, namespaces, ...)</li>
        <li>Exchange them using standard <b>RDF formats</b> (N-Triples, TriX, Turtle, RDF/Xml)</li>
        <li>Create and validate <b>SHACL shapes</b> (shape graphs, shapes, targets, constraints, reports, ...)</b></li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Store</b></li> 
    <ul>
        <li>Create and manage <b>RDF stores</b> for context-aware modeling of RDF data (quadruples)</li>
        <li>Exchange them using standard <b>RDF formats</b> (N-Quads, TriX)</li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Query</b></li> 
    <ul>
        <li>Create and execute <b>SPARQL queries</b> on graphs, stores, federations and <i>SPARQL endpoints</i></li>
        <li>Create and execute <b>SPARQL operations</b> on graphs, stores and <i>SPARQL UPDATE endpoints</i></li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Semantics</b></li> 
    <ul>
        <li>Create and manage <b>OWL-DL ontologies</b> (classes, restrictions, properties, facts, assertions, annotations, ...)</li>
        <li>Validate ontology <b>T-BOX</b> and <b>A-BOX</b> against a wide set of intelligent semantic rules</li>
        <li>Create and execute <b>SWRL reasoners</b> on ontologies, with forward chaining inference materialization</li>
        <li>Create and manage <b>SKOS schemes</b> (concepts, collections, relations, annotations, labels, ...)</li>
    </ul>
</ul>
