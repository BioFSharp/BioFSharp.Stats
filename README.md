# BioFSharp.Stats

![Logo](docs/img/Logo_large.png)

![NuGet Version](https://img.shields.io/nuget/v/BioFSharp.Stats?label=nuget(stable))
![NuGet Version](https://img.shields.io/nuget/vpre/BioFSharp.Stats?label=nuget(prerelease))

BioFSharp.Stats contains statistical functions with a clear biological focus such as Gene Set Enrichment Analysis (GSEA).

BioFSharp.Stats is part of the [BioFSharp]() extension package ecosystem.


| Build status (ubuntu and windows) | Test Coverage |
|---|---|
| [![Build and test](https://github.com/BioFSharp/BioFSharp.Stats/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/BioFSharp/BioFSharp.Stats/actions/workflows/build-and-test.yml) | [![codecov](https://codecov.io/gh/BioFSharp/BioFSharp.Stats/branch/main/graph/badge.svg)](https://codecov.io/gh/BioFSharp/BioFSharp.Stats) |


From 2.0.0 onwards, this package will have a decoupled release schedule from the core `BioFSharp` package.
This means that the versioning will be independent and may not follow the same versioning scheme as `BioFSharp`.

The last coupled release was `2.0.0-preview.3`.

## Build

This repo contains a buildproject that can be called either via `build.cmd`, `build.sh`, or directly via `dotnet run`.

Find all build targets in `build/Build.fs`.

Examples:

- run the default build target (`Build`) via build.cmd:
  ```bash
  ./build.cmd
  ```
- run the `RunTests` target in build.sh:     
  ```bash
  ./build.sh RunTests
  ```