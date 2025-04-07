# PackageTemplate

A template repository for creating an extension package for BioFSharp.

## Content

- `src/BioFSharp.XYZ`: The main project folder. Contains a library with BioFSharp core dependency.
- `tests/BioFSharp.XYZ.Tests`: The test project folder. Contains a XUnit test project
- `build/build.fsproj`: A FAKE build project that handles building, testing, packaging, publishing, etc.
- `docs`: the docs folder contains an example index.fsx file with simple documentation boilerplate.

## Setup

Here is a list of things you should/might want to do after setting up a repo with this template:

> [!IMPORTANT]  
> Whenever you change a project file name or folder, make sure to fix the solution registration afterwards.

- Rename some things: Replace `XYZ` with the name of your package
  - `PackageTemplate.sln`
  - `src/BioFSharp.XYZ`
  - `src/BioFSharp.XYZ/BioFSharp.XYZ.fsproj`
  - `tests/BioFSharp.XYZ`
  - `tests/BioFSharp.XYZ.Tests/BioFSharp.XYZ.Tests.fsproj`
    - Also make sure to fix the project reference to BioFSharp.XYZ when renamed
  - in `build/ProjectInfo.fs`:
    - Set project name: 
      ```fsharp
      let project = "BioFSharp.XYZ"` // replace with the name of your project
      ```
    - Set git owner:
      ```fsharp
      let gitOwner = "BioFSharp" // replace with github account name or organization where repo is hosted if necessary
      ```
    - fix test project path:
      ```fsharp
      let testProjects = 
      [
          "tests/BioFSharp.XYZ.Tests/BioFSharp.XYZ.Tests.fsproj" // replace with the name of your test project
      ]
      ```
- If needed, change the target framework of the project. it currently targets `.net8.0`, might want to target a newer version if you need a specific API, or `netstandard2.0` for increased backwards compatibility.

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