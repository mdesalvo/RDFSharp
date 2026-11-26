---
title: GeoSPARQL
permalink: /query/geosparql/
tagline: Where things are is part of the data
lede: "RDFSharp understands GeoSPARQL geometries: it measures distances, draws buffers and tests how shapes touch, contain or cross each other, right inside a SPARQL query."
---

> The sales office has a map on the wall and a question every week: which shops can we reach with our own van, which ones need a courier and how many bottles travel each way? The answers are already in the data, as soon as the data knows where things are.
{: .story}

## Geometries in RDF

GeoSPARQL, a standard of the Open Geospatial Consortium, gives RDF a way to say where things are. A thing *has a geometry* and the geometry is *serialized as* a literal, usually in WKT (Well-Known Text) with longitude first, then latitude, in the WGS84 system used by GPS.

The map of Vallerosa lives in [vallerosa-sales.ttl]({{ '/assets/data/vallerosa-sales.ttl' | relative_url }}): the cellar is a point, each subzone a polygon, each partner shop a point in its city.

```turtle
@prefix geo: <http://www.opengis.net/ont/geosparql#> .

vrd:vallerosa geo:hasGeometry vrd:vallerosa-geom .
vrd:vallerosa-geom geo:asWKT "POINT(11.33 43.34)"^^geo:wktLiteral .

vrd:piana geo:hasGeometry vrd:piana-geom .
vrd:piana-geom geo:asWKT "POLYGON((11.29 43.32, 11.37 43.32, 11.37 43.35, 11.30 43.35, 11.29 43.32))"^^geo:wktLiteral .

vrd:bottega-arno a vr:Shop ;
    rdfs:label "Bottega Arno" ;
    geo:hasGeometry vrd:bottega-arno-geom .
vrd:bottega-arno-geom geo:asWKT "POINT(11.2558 43.7696)"^^geo:wktLiteral .
```

<figure class="figure map">
<svg viewBox="0 0 520 290" role="img" aria-labelledby="valley-map-title">
  <title id="valley-map-title">Map of the valley: three subzones sharing their borders, Hilltop to the west, North Hill to the north-east and The Plain to the south-east. The cellar is a point in The Plain; the new plot is a point in North Hill.</title>
  <polygon class="m-zone" points="20,265 180,265 220,145 180,25 20,25"/>
  <polygon class="m-zone" points="220,145 500,145 500,25 180,25"/>
  <polygon class="m-zone" points="180,265 500,265 500,145 220,145"/>
  <path class="m-border" d="M180 265 L220 145 L180 25 M220 145 L500 145"/>
  <text class="m-zone-label" x="70" y="150">Hilltop</text>
  <text class="m-zone-label" x="270" y="70">North Hill</text>
  <text class="m-zone-label" x="290" y="235">The Plain</text>
  <circle class="m-pt" cx="340" cy="185" r="6"/>
  <text class="m-pt-label" x="352" y="190">cellar</text>
  <circle class="m-pt plot" cx="440" cy="85" r="7"/>
  <text class="m-pt-label" x="400" y="115">new plot</text>
</svg>
<figcaption>The valley as the sales file draws it. The teal lines are the borders the subzones share: <code>geof:sfTouches</code> finds them. The ring is the plot offered to the cooperative: <code>geof:sfContains</code> says which subzone it falls in.</figcaption>
</figure>

In C#, the GeoSPARQL terms are ready in `RDFVocabulary.GEOSPARQL` (`HAS_GEOMETRY`, `AS_WKT`) and a WKT literal is a typed literal with the datatype `GEOSPARQL_WKT`:

```csharp
RDFGraph sales = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-sales.ttl");
RDFNamespace vr = new RDFNamespace("vr", "https://vallerosa.example/vocab#");
RDFNamespace vrd = new RDFNamespace("vrd", "https://vallerosa.example/id/");

RDFTypedLiteral cellar = new RDFTypedLiteral("POINT(11.33 43.34)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);

RDFVariable shop = new RDFVariable("shop");
RDFVariable name = new RDFVariable("name");
RDFVariable geometry = new RDFVariable("geom");
RDFVariable geometryWkt = new RDFVariable("wkt");
```

> RDFSharp registers the GeoSPARQL namespace under the prefix `geosparql`, because `geo` already belongs to the older W3C WGS84 vocabulary. The SPARQL on this page follows the OGC habit and writes `geo:`; in C# the constants of `RDFVocabulary.GEOSPARQL` remove any doubt.
{: .tip}

## How far is each shop

`geof:distance` measures the distance between two geometries. RDFSharp computes it in metres, on a projection suited to the area, so the numbers are real distances on the ground. Divided by 1000 and rounded, they become kilometres.

In SPARQL text the unit argument of `geof:distance` and `geof:buffer` may be `uom:metre` or the QUDT metre with its decimal multiples and submultiples (`unit:M`, `unit:DecaM`, `unit:HectoM`, `unit:KiloM`, `unit:DeciM`, `unit:CentiM`, `unit:MilliM` and `unit:MicroM`): the parser converts between them and the metres of the engine, so a distance asked in `unit:KiloM` comes back in kilometres and a radius given in `unit:KiloM` becomes 1000 times as many metres. Any other unit is rejected by the parser. `RDFGeoDistanceExpression` and `RDFGeoBufferExpression` built in C# always work in metres. The units of the vocabulary are `RDFVocabulary.GEOSPARQL.UOM.METRE` and `RDFVocabulary.GEOSPARQL.UNIT.*`.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX geo:  <http://www.opengis.net/ont/geosparql#>
PREFIX geof: <http://www.opengis.net/def/function/geosparql/>
PREFIX uom:  <http://www.opengis.net/def/uom/OGC/1.0/>

SELECT ?name ?km
WHERE {
  ?shop a vr:Shop ;
        rdfs:label ?name ;
        geo:hasGeometry ?geom .
  ?geom geo:asWKT ?wkt .
  BIND (ROUND(geof:distance(?wkt, "POINT(11.33 43.34)"^^geo:wktLiteral, uom:metre) / 1000) AS ?km)
}
ORDER BY ?km
```

```csharp
RDFResource shopClass = new RDFResource(vr + "Shop");

RDFTypedLiteral metresPerKilometre = new RDFTypedLiteral("1000", RDFModelEnums.RDFDatatypes.XSD_INTEGER);

RDFVariable kilometres = new RDFVariable("km");

RDFSelectQuery distances = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(shop, RDFVocabulary.RDF.TYPE, shopClass))
        .AddPattern(new RDFPattern(shop, RDFVocabulary.RDFS.LABEL, name))
        .AddPattern(new RDFPattern(shop, RDFVocabulary.GEOSPARQL.HAS_GEOMETRY, geometry))
        .AddPattern(new RDFPattern(geometry, RDFVocabulary.GEOSPARQL.AS_WKT, geometryWkt))
        .AddBind(new RDFBind(
            new RDFRoundExpression(new RDFDivideExpression(
                new RDFGeoDistanceExpression(geometryWkt, cellar),
                new RDFConstantExpression(metresPerKilometre))),
            kilometres)))
    .AddModifier(new RDFOrderByModifier(kilometres, RDFQueryEnums.RDFOrderByFlavors.ASC))
    .AddProjectionVariable(name)
    .AddProjectionVariable(kilometres);

RDFSelectQueryResult result = await distances.ApplyToGraphAsync(sales);
```

</div>

<div class="result" markdown="1">

| ?NAME | ?KM |
|---|---|
| `Enoteca del Campo` | `2^^http://www.w3.org/2001/XMLSchema#double` |
| `Bottega Arno` | `45^^http://www.w3.org/2001/XMLSchema#double` |
| `Cantinetta della Torre` | `87^^http://www.w3.org/2001/XMLSchema#double` |
| `Vineria Due Torri` | `122^^http://www.w3.org/2001/XMLSchema#double` |
| `Enoteca Sette Colli` | `178^^http://www.w3.org/2001/XMLSchema#double` |
| `Enoteca dei Navigli` | `278^^http://www.w3.org/2001/XMLSchema#double` |

</div>

The shop in Siena is two kilometres down the road; the one in Milan is a day's drive.

## Who is inside the delivery radius

The cooperative's van delivers for free within 100 km of the cellar. Rather than comparing distances, GeoSPARQL can draw the area itself: `geof:buffer` grows the cellar's point into a disc of 100 km, `geof:sfWithin` keeps the shops that fall inside it.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX geo:  <http://www.opengis.net/ont/geosparql#>
PREFIX geof: <http://www.opengis.net/def/function/geosparql/>
PREFIX uom:  <http://www.opengis.net/def/uom/OGC/1.0/>

SELECT ?name
WHERE {
  ?shop a vr:Shop ;
        rdfs:label ?name ;
        geo:hasGeometry ?geom .
  ?geom geo:asWKT ?wkt .
  FILTER (geof:sfWithin(?wkt, geof:buffer("POINT(11.33 43.34)"^^geo:wktLiteral, 100000, uom:metre)))
}
```

```csharp
RDFGeoBufferExpression deliveryArea = new RDFGeoBufferExpression(new RDFConstantExpression(cellar), 100000);

RDFSelectQuery nearby = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(shop, RDFVocabulary.RDF.TYPE, shopClass))
        .AddPattern(new RDFPattern(shop, RDFVocabulary.RDFS.LABEL, name))
        .AddPattern(new RDFPattern(shop, RDFVocabulary.GEOSPARQL.HAS_GEOMETRY, geometry))
        .AddPattern(new RDFPattern(geometry, RDFVocabulary.GEOSPARQL.AS_WKT, geometryWkt))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFGeoWithinExpression(geometryWkt, deliveryArea),
            new RDFConstantExpression(RDFTypedLiteral.True)))))
    .AddProjectionVariable(name);
```

</div>

<div class="result" markdown="1">

| ?NAME |
|---|
| `Enoteca del Campo` |
| `Bottega Arno` |
| `Cantinetta della Torre` |

</div>

The topological functions such as `geof:sfWithin` answer true or false. In C#, a filter on one of them compares it with `true`, which is exactly what the parser builds when it reads `FILTER (geof:sfWithin(...))`.

## How many bottles travel each way

A spatial test can feed any other part of a query. Here `IF` turns "inside the buffer" into a delivery zone and the zone becomes the key of an aggregate over all the sales of 2025.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX geo:  <http://www.opengis.net/ont/geosparql#>
PREFIX geof: <http://www.opengis.net/def/function/geosparql/>
PREFIX uom:  <http://www.opengis.net/def/uom/OGC/1.0/>

SELECT ?zone (SUM(?bottles) AS ?total)
WHERE {
  ?sale vr:soldTo ?shop ;
        vr:bottles ?bottles .
  ?shop geo:hasGeometry ?geom .
  ?geom geo:asWKT ?wkt .
  BIND (IF(geof:sfWithin(?wkt, geof:buffer("POINT(11.33 43.34)"^^geo:wktLiteral, 100000, uom:metre)),
           "free delivery", "courier") AS ?zone)
}
GROUP BY ?zone
```

```csharp
RDFResource soldTo = new RDFResource(vr + "soldTo");
RDFResource hasBottles = new RDFResource(vr + "bottles");

RDFPlainLiteral freeDelivery = new RDFPlainLiteral("free delivery");
RDFPlainLiteral courier = new RDFPlainLiteral("courier");

RDFVariable sale = new RDFVariable("sale");
RDFVariable bottles = new RDFVariable("bottles");
RDFVariable zone = new RDFVariable("zone");
RDFVariable total = new RDFVariable("total");

RDFSelectQuery byZone = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(sale, soldTo, shop))
        .AddPattern(new RDFPattern(sale, hasBottles, bottles))
        .AddPattern(new RDFPattern(shop, RDFVocabulary.GEOSPARQL.HAS_GEOMETRY, geometry))
        .AddPattern(new RDFPattern(geometry, RDFVocabulary.GEOSPARQL.AS_WKT, geometryWkt))
        .AddBind(new RDFBind(
            new RDFIfExpression(new RDFGeoWithinExpression(geometryWkt, deliveryArea),
                new RDFConstantExpression(freeDelivery),
                new RDFConstantExpression(courier)),
            zone)))
    .AddModifier(new RDFGroupByModifier(new List<RDFVariable> { zone })
        .AddAggregator(new RDFSumAggregator(bottles, total)));
```

</div>

<div class="result" markdown="1">

| ?ZONE | ?TOTAL |
|---|---|
| `free delivery` | `510^^http://www.w3.org/2001/XMLSchema#integer` |
| `courier` | `156^^http://www.w3.org/2001/XMLSchema#integer` |

</div>

More than three bottles out of four never leave the van's radius.

## Which subzones share a border

Topology works between shapes too. `geof:sfTouches` is true when two geometries meet on their boundary without overlapping, which is what neighbouring subzones do. The subzones come from the catalog (they are `vr:partOf` the valley), their shapes from the sales file, so the query runs on the union of the two graphs; comparing the IRIs as strings keeps each pair once.

<div class="pair" markdown="1">

```sparql
PREFIX vr:   <https://vallerosa.example/vocab#>
PREFIX vrd:  <https://vallerosa.example/id/>
PREFIX geo:  <http://www.opengis.net/ont/geosparql#>
PREFIX geof: <http://www.opengis.net/def/function/geosparql/>

SELECT ?a ?b
WHERE {
  ?a vr:partOf vrd:valle-rosa ;
     geo:hasGeometry ?ga .
  ?ga geo:asWKT ?wa .
  ?b vr:partOf vrd:valle-rosa ;
     geo:hasGeometry ?gb .
  ?gb geo:asWKT ?wb .
  FILTER (geof:sfTouches(?wa, ?wb))
  FILTER (STR(?a) < STR(?b))
}
```

```csharp
RDFGraph catalog = await RDFGraph.FromFileAsync(RDFModelEnums.RDFFormats.Turtle, "vallerosa-catalog.ttl");
RDFGraph map = await catalog.UnionWithAsync(sales);

RDFResource partOf = new RDFResource(vr + "partOf");

RDFResource valleRosa = new RDFResource(vrd + "valle-rosa");

RDFVariable firstSubzone = new RDFVariable("a");
RDFVariable secondSubzone = new RDFVariable("b");
RDFVariable firstGeometry = new RDFVariable("ga");
RDFVariable secondGeometry = new RDFVariable("gb");
RDFVariable firstWkt = new RDFVariable("wa");
RDFVariable secondWkt = new RDFVariable("wb");

RDFSelectQuery neighbours = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(firstSubzone, partOf, valleRosa))
        .AddPattern(new RDFPattern(secondSubzone, partOf, valleRosa))
        .AddPattern(new RDFPattern(firstSubzone, RDFVocabulary.GEOSPARQL.HAS_GEOMETRY, firstGeometry))
        .AddPattern(new RDFPattern(firstGeometry, RDFVocabulary.GEOSPARQL.AS_WKT, firstWkt))
        .AddPattern(new RDFPattern(secondSubzone, RDFVocabulary.GEOSPARQL.HAS_GEOMETRY, secondGeometry))
        .AddPattern(new RDFPattern(secondGeometry, RDFVocabulary.GEOSPARQL.AS_WKT, secondWkt))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFGeoTouchesExpression(firstWkt, secondWkt),
            new RDFConstantExpression(RDFTypedLiteral.True))))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
            new RDFStrExpression(firstSubzone), new RDFStrExpression(secondSubzone)))))
    .AddProjectionVariable(firstSubzone)
    .AddProjectionVariable(secondSubzone);
```

</div>

<div class="result" markdown="1">

| ?A | ?B |
|---|---|
| `https://vallerosa.example/id/colle-nord` | `https://vallerosa.example/id/poggio` |
| `https://vallerosa.example/id/colle-nord` | `https://vallerosa.example/id/piana` |
| `https://vallerosa.example/id/piana` | `https://vallerosa.example/id/poggio` |

</div>

Each subzone borders the other two, as the map shows.

## Where does a new plot fall

A farmer offers the cooperative a vineyard at longitude 11.355, latitude 43.365. Which subzone would it join? `geof:sfContains` asks each polygon whether it contains the point.

<div class="pair" markdown="1">

```sparql
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX geo:  <http://www.opengis.net/ont/geosparql#>
PREFIX geof: <http://www.opengis.net/def/function/geosparql/>

SELECT ?subzone ?label
WHERE {
  ?subzone geo:hasGeometry ?geom ;
           rdfs:label ?label .
  ?geom geo:asWKT ?wkt .
  FILTER (geof:sfContains(?wkt, "POINT(11.355 43.365)"^^geo:wktLiteral))
  FILTER (LANGMATCHES(LANG(?label), "en"))
}
```

```csharp
RDFPlainLiteral englishTag = new RDFPlainLiteral("en");
RDFTypedLiteral plot = new RDFTypedLiteral("POINT(11.355 43.365)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT);

RDFVariable subzone = new RDFVariable("subzone");
RDFVariable label = new RDFVariable("label");

RDFSelectQuery whereIsIt = new RDFSelectQuery()
    .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(subzone, RDFVocabulary.GEOSPARQL.HAS_GEOMETRY, geometry))
        .AddPattern(new RDFPattern(geometry, RDFVocabulary.GEOSPARQL.AS_WKT, geometryWkt))
        .AddPattern(new RDFPattern(subzone, RDFVocabulary.RDFS.LABEL, label))
        .AddFilter(new RDFFilter(new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
            new RDFGeoContainsExpression(geometryWkt, plot),
            new RDFConstantExpression(RDFTypedLiteral.True))))
        .AddFilter(new RDFFilter(new RDFLangMatchesExpression(label,
            new RDFConstantExpression(englishTag)))))
    .AddProjectionVariable(subzone)
    .AddProjectionVariable(label);

RDFSelectQueryResult found = await whereIsIt.ApplyToGraphAsync(map);
```

</div>

<div class="result" markdown="1">

| ?SUBZONE | ?LABEL |
|---|---|
| `https://vallerosa.example/id/colle-nord` | `North Hill@EN` |

</div>

## Every function at a glance

Each GeoSPARQL function has its expression class. The ones answering true or false go in a filter compared with `true`, the others produce a value for a `BIND` or a projection.

| Expression class | GeoSPARQL function | Answers |
|---|---|---|
| `RDFGeoDistanceExpression` | `geof:distance` | the distance in metres |
| `RDFGeoBufferExpression` | `geof:buffer` | the geometry grown by a radius in metres |
| `RDFGeoConvexHullExpression` | `geof:convexHull` | the smallest convex shape around the geometry |
| `RDFGeoEnvelopeExpression` | `geof:envelope` | the bounding rectangle |
| `RDFGeoBoundaryExpression` | `geof:boundary` | the outline of the geometry |
| `RDFGeoIntersectionExpression` | `geof:intersection` | the part two geometries share |
| `RDFGeoUnionExpression` | `geof:union` | the two geometries merged |
| `RDFGeoDifferenceExpression` | `geof:difference` | the first geometry minus the second |
| `RDFGeoSymDifferenceExpression` | `geof:symDifference` | what belongs to only one of the two |
| `RDFGeoDimensionExpression` | `geof:dimension` | 0 for points, 1 for lines, 2 for areas |
| `RDFGeoGetSRIDExpression` | `geof:getSRID` | the coordinate reference system |
| `RDFGeoIsEmptyExpression`, `RDFGeoIsSimpleExpression` | `geof:isEmpty`, `geof:isSimple` | true or false |
| `RDFGeoEqualsExpression`, `RDFGeoDisjointExpression`, `RDFGeoIntersectsExpression`, `RDFGeoTouchesExpression`, `RDFGeoCrossesExpression`, `RDFGeoWithinExpression`, `RDFGeoContainsExpression`, `RDFGeoOverlapsExpression` | the Simple Features relations `geof:sfEquals` … `geof:sfOverlaps` | true or false |
| `RDFGeoEgenhoferExpression` | the Egenhofer relations `geof:ehEquals` … `geof:ehCoveredBy` | true or false, the relation chosen with `RDFQueryEnums.RDFGeoEgenhoferRelations` |
| `RDFGeoRCC8Expression` | the RCC8 relations `geof:rcc8eq` … `geof:rcc8ntppi` | true or false, the relation chosen with `RDFQueryEnums.RDFGeoRCC8Relations` |
| `RDFGeoRelateExpression` | `geof:relate` | true or false against a DE-9IM matrix pattern |

Geometries can also be written in GML (`geo:asGML` with `geo:gmlLiteral`); every function accepts both.
