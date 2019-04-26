# RDFSharp - Start playing with RDF!
[![NuGet Badge](https://buildstats.info/nuget/RDFSharp)](https://www.nuget.org/packages/RDFSharp)

RDFSharp is a lightweight C# framework designed to ease the creation of .NET applications based on the <b>RDF model</b>, representing a straightforward <b>didactic</b> solution for start playing with RDF, SPARQL and Semantic Web concepts. 

With RDFSharp it is possible to realize .NET applications capable of modeling, storing and querying RDF data.
<hr>
RDFSharp has a modular API made up of three layers: 

<ul>
    <li><b>RDFSharp.Model</b></li> 
    <ul>
        <li>Create and manage <i>RDF models</i> (resources, literals, triples, graphs, namespaces, ...)</li>
        <li>Exchange them using standard <i>RDF formats</i> (N-Triples, TriX, Turtle, RDF/Xml)</li>
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
<hr>
RDFSharp also delivers a set of extension projects enhancing its features and capabilities:

<ul>
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.Semantics">Ontology</a></b> modeling, validation and reasoning with full support for <i>RDFS/OWL-DL/SKOS</i></li>  
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.Stores">Storage</a></b> of RDF data on different SQL providers</li>
</ul>
<hr>
