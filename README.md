# RDFSharp [![NuGet Badge](https://buildstats.info/nuget/RDFSharp)](https://www.nuget.org/packages/RDFSharp)

RDFSharp has a modular API made up of four layers ([RDF/SPARQL/SHACL docs](https://github.com/mdesalvo/RDFSharp/releases/download/v2.23.0/RDFSharp-2.23.0.pdf), [OWL/SKOS docs](https://github.com/mdesalvo/RDFSharp/releases/download/v2.23.0/RDFSharp.Semantics-2.23.0.pdf)): 

<ul>
    <li><b>RDFSharp.Model</b></li> 
    <ul>
        <li>Create and manage <i>RDF models</i> (resources, literals, triples, graphs, namespaces, ...)</li>
        <li>Exchange them using standard <i>RDF formats</i> (N-Triples, TriX, Turtle, RDF/Xml)</li>
        <li>Create and validate <i>SHACL shapes</i> (shape graphs, shapes, targets, constraints, reports, ...)</b></li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Store</b></li> 
    <ul>
        <li>Create and manage <i>RDF stores</i> for context-aware modeling of RDF data (quadruples)</li>
        <li>Exchange them using standard <i>RDF formats</i> (N-Quads, TriX)</li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Query</b></li> 
    <ul>
        <li>Create and manage <i>RDF federations</i> giving integrated query access to multiple data sources</li>
        <li>Create and execute <i>SPARQL queries</i> on graphs, stores, federations and <i>SPARQL endpoints</i></li>        
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Semantics</b></li> 
    <ul>
        <li>Create and manage <i>OWL-DL ontologies</i> (classes, restrictions, properties, facts, assertions, annotations, ...)</li>
        <li>Validate ontology <i>T-BOX</i> and <i>A-BOX</i> against a standard set of <i>RDFS/OWL-DL</i> constraint rules</li>
        <li>Create <i>OWL-DL reasoners</i> exploiting an extensible set of <i>RDFS/OWL-DL</i> inference rules</li>
        <li>Create and manage <i>SKOS concept schemes</i> (concepts, collections, relations, annotations, labels, ...)</li>
    </ul>
</ul>
