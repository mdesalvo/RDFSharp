# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

RDFSharp is a .NET library for Semantic Web applications (RDF, SPARQL, SHACL). It is published as a NuGet package and targets both `netstandard2.0` and `net8.0` from a single source tree (see `RDFSharp/RDFSharp.csproj`). The test project targets `net8.0` only.

The codebase is logically split into three layers, all inside the single `RDFSharp` assembly:

- `RDFSharp.Model` — RDF primitives (`RDFResource`, `RDFLiteral`, `RDFTriple`, `RDFGraph`), namespaces, datatypes, RDF format serializers (N-Triples, TriX, Turtle, RDF/Xml), and the SHACL validation engine (`Model/Validation`, `Model/Facets`).
- `RDFSharp.Store` — context-aware quadruple storage. `RDFStore` is the abstract base; `RDFMemoryStore` is the in-process implementation. Out-of-process providers live in the separate `RDFSharp.Extensions` repo.
- `RDFSharp.Query` — SPARQL query/operation engine (codename **Mirella**, see `Query/Mirella/RDFQueryEngine.cs` and `RDFOperationEngine.cs`). Hosts query types (`RDFSelectQuery`, `RDFAskQuery`, `RDFConstructQuery`, `RDFDescribeQuery`), SPARQL UPDATE operations, expressions, filters, modifiers, aggregators, federation (`RDFFederation`), and SPARQL endpoint client (`RDFSPARQLEndpoint`).

Anchor abstractions to know before changing things:

- `RDFDataSource` (`Model/RDFDataSource.cs`) is the common base for `RDFGraph`, `RDFStore`, `RDFFederation`, and `RDFSPARQLEndpoint`. The query engine dispatches against this type via the `IsGraph()/IsStore()/IsFederation()/IsSPARQLEndpoint()` internal probes.
- `RDFGraphIndex` / `RDFStoreIndex` are in-memory hashed indexes (subjects/predicates/objects/literals → hash sets of triple IDs). All triple/quadruple ops go through the index — when adding new mutation paths, keep the index in sync (see how `RDFGraph.AddTriple` and `RemoveTriple` do it).
- `RDFNamespaceRegister` and `RDFDatatypeRegister` are singletons (`Instance` property). Most code reaches built-in vocabularies via `RDFVocabulary` (e.g. `RDFVocabulary.RDF.TYPE`); that class is `[ExcludeFromCodeCoverage]` because it's static constants.
- `RDFShims` (`RDFSharp/RDFShims.cs`) centralizes every regex pattern. On `net8.0` it uses source-generated `[GeneratedRegex]`; on `netstandard2.0` it falls back to `Lazy<Regex>` with `RegexOptions.Compiled`. **Add new regexes here**, not inline, and provide both branches under the existing `#if NET8_0_OR_GREATER` guard.
- `[InternalsVisibleTo]` in `RDFSharp/Properties/AssemblyInfo.cs` exposes internals to `RDFSharp.Test`, `RDFSharp.Extensions`, `OWLSharp`, and `OWLSharp.Extensions`. Be aware that internal API changes can break those downstream repos.

## Build, test, version

The CI workflows in `.github/workflows/{linux,windows}.yml` are authoritative for the canonical commands:

```
dotnet build -c Release
dotnet test --no-build -c Release --verbosity normal
```

Linux CI additionally runs `--collect:"XPlat Code Coverage"` and uploads to Codecov. There is no separate lint step — `EnableNETAnalyzers` is on in `RDFSharp.csproj`, so warnings come from the build.

Run a single test class or method:

```
dotnet test --filter "FullyQualifiedName~RDFGraphTest"
dotnet test --filter "Name=ShouldCreateEmptyGraph"
```

The package version is set in `RDFSharp/RDFSharp.csproj` via `<Version>` (currently `3.24.0`). `GeneratePackageOnBuild` is true, so a Release build emits the `.nupkg` + `.snupkg` into `bin/Release`.

## Conventions worth preserving

- **File header**: every `.cs` file begins with the Apache-2.0 copyright block (`Copyright 2012-2026 Marco De Salvo`). New files must include it.
- **Naming**: every public type is prefixed `RDF` (e.g. `RDFGraph`, `RDFSelectQuery`). Test classes mirror this with a `Test` suffix.
- **Namespaces**: library code uses traditional `namespace X { ... }` blocks; test code uses file-scoped `namespace X;`. Match the file you're editing.
- **Multi-target gating**: when adding APIs that depend on `IAsyncEnumerable`, `IAsyncDisposable`, source-generated regex, etc., guard them with `#if NET8_0_OR_GREATER` so the `netstandard2.0` build still compiles. `RDFGraph`, `RDFMemoryStore`, and `RDFShims` are the canonical examples.
- **EOL-sensitive assertions**: serializer/printer output uses `Environment.NewLine` (CRLF on Windows), but expected strings in test files are stored with LF. Always wrap both operands of string equality assertions with `RDFTestUtilities.NormalizeEOL(...)` (defined in `RDFSharp.Test/RDFTestUtilities.cs`). Commit `5ada8a55` fixed 264 such assertions — don't reintroduce the bug.

## Test infrastructure notes

- Framework: MSTest v4 (`MSTest.TestAdapter` + `MSTest.TestFramework`), coverage via `coverlet.collector`.
- HTTP-dependent tests (SPARQL endpoint client, `RDFLoadOperation`, etc.) use `WireMock.Net.Minimal` for in-process mocking — prefer that over real network calls.
- Test files mirror the production tree: `RDFSharp/Foo/Bar.cs` ↔ `RDFSharp.Test/Foo/BarTest.cs`. New types should ship with a matching test file in the same relative path.
