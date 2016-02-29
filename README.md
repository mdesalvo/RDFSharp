# RDFSharp - Start playing with RDF!
RDFSharp is a lightweight C# framework designed to ease the creation of .NET applications based on the <b>RDF model</b>, representing a straightforward <b>didactic</b> solution for start playing with RDF and Semantic Web concepts. 

With RDFSharp it is possible to realize .NET applications capable of modeling, storing and querying RDF data.
<hr>
RDFSharp has a modular API made up of three layers: 

<ul>
    <li><b>RDFSharp.Model</b></li> 
    <ul>
        <li>Create and manage <i>RDF models</i> (resources, literals, triples, graphs, namespaces, datatypes, ...)</li>
        <li>Exchange them using standard <i>RDF formats</i> (N-Triples, TriX, Turtle, Xml)</li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Store</b></li> 
    <ul>
        <li>Create and manage <i>RDF stores</i> backing data on memory or SQL Server</li>
        <li>Exchange them using standard RDF formats (N-Quads)</li>
        <li>Create and manage <i>RDF federations</i> giving integrated query access to multiple stores</li>
    </ul>
</ul>
<ul>
    <li><b>RDFSharp.Query</b></li> 
    <ul>
        <li>Create and execute <i>SPARQL queries</i> on graphs, stores and federations to fluently query RDF data</li>
    </ul>
</ul>
<hr>
Plugins are also available, to extend the capabilities of the core library with specific layers and functionalities:

<ul>
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.Semantics">RDFSharp.Semantics</a></b></li> 
    <ul>
        <li>Create and manage <i>OWL-DL ontologies</i> (classes, properties, restrictions, facts, relations, ...)</li> 
        <li>Validate them against <i>RDFS/OWL-DL specifications</i> to detect warning/error evidences</li>
		<li>Create and execute <i>RDFS/OWL-DL/Custom reasoners</i> and materialize inferred knowledge</li>
    </ul>
</ul>
<ul>
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.RDFFirebirdStore">RDFSharp.RDFFirebirdStore</a></b></li> 
    <ul>
        <li>Create and manage RDF stores backing data on <i>Firebird</i></li>
    </ul>
</ul>
<ul>
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.RDFMySQLStore">RDFSharp.RDFMySQLStore</a></b></li> 
    <ul>
        <li>Create and manage RDF stores backing data on <i>MySQL</i></li>
    </ul>
</ul>
<ul>
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.RDFPostgreSQLStore">RDFSharp.RDFPostgreSQLStore</a></b></li> 
    <ul>
        <li>Create and manage RDF stores backing data on <i>PostgreSQL</i></li>
    </ul>
</ul>
<ul>
    <li><b><a href="https://github.com/mdesalvo/RDFSharp.RDFSQLiteStore">RDFSharp.RDFSQLiteStore</a></b></li> 
    <ul>
        <li>Create and manage RDF stores backing data on <i>SQLite</i></li>
    </ul>
</ul>
<hr>
There are many ways that you can contribute to the RDFSharp project: 

<ul>
    <li>Submit a bug</li> 
    <li>Submit a code fix for a bug</li>  
    <li>Submit a feature request</li>
    <li>Submit code for a feature request</li>
    <li>Tell others about the RDFSharp project :)</li>  
    <li><i>Kindly donate to non-profit italian charity organization <b><a href="http://www.soleterre.org/en/">Soleterre</a></b> :)</i></li> 
</ul>
<hr>
RDFSharp is also available on <b><a href="http://www.nuget.org/packages?q=rdfsharp">NuGet</a></b> and <b><a href="https://rdfsharp.codeplex.com/">Codeplex</a></b>!
<hr>
