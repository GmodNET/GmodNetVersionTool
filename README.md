# GmodNET Version Tool
[![MSBuild Target NuGet Link](https://img.shields.io/nuget/v/GmodNET.VersionTool.MSBuild?label=MSBuild%20Target&style=plastic)](https://www.nuget.org/packages/GmodNET.VersionTool.MSBuild/)
[![Class Library NuGet Link](https://img.shields.io/nuget/v/GmodNET.VersionTool.Core?label=Class%20Library&style=plastic)](https://www.nuget.org/packages/GmodNET.VersionTool.Core/)
[![CLI Tool NuGet Link](https://img.shields.io/nuget/v/GmodNET.VersionTool?label=CLI%20Tool&style=plastic)](https://www.nuget.org/packages/GmodNET.VersionTool/)

A git-based [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html) compatible version number generator.

## Description

GmodNET Version Tool generates a version number of the form `[FullVersion]+codename.[codename from version JSON file].head.[git head name].commit.[hexadecimal representation of git commit hash].[any additional metadata provided in a "Version" key-value pair of the version JSON file]` based on the version file and git repository state.

Version file is a JSON file containing key `Version` with corresponding value being a [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html) compatible version base (initial part of the resulting version) and optional key `Codename`.

For example, given that repository HEAD points to `NewFeature` branch and version file
```json
{
  "Version": "0.6.0-beta.1"
}
```
the resulting version will be equal to

`0.6.0-beta.1.943586.NewFeature+head.NewFeature.commit.a3455d90049f17b6ac3bbc54177cfb51b0803851`

or given that git HEAD points to `master` branch and version file
```json
{
  "Version": "1.0.0+bugfix14",
  "Codename": "Ural"
}
```
the resulting version will be equal to

`1.0.0+codename.Ural.head.master.commit.e796d2d62d39256f9c3e55b3648cd2bfb289e7bc.bugfix14`.

You can read about version number generation in more detail in the Version Tool [specification](docs/specification.md).

## Usage

The most straightforward way to use GmodNET Version Tool with .NET projects is to consume [GmodNET.VersionTool.MSBuild NuGet package](https://www.nuget.org/packages/GmodNET.VersionTool.MSBuild/). Just add package reference and specify version file to use by adding similar `ItemGroup` to your MSBuild project:
```xml
<ItemGroup>
  <PackageReference Include="GmodNET.VersionTool.MSBuild" Version="2.0.0" PrivateAssets="All"/>
  <VersionFile Include="../version.json" />
</ItemGroup>
```

[GmodNET.VersionTool.MSBuild](https://www.nuget.org/packages/GmodNET.VersionTool.MSBuild/) will automatically generate version number and set `Version` and `PackageVersion` properties for each build.

There is a cross-platform [.NET command line tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) implementation of GmodNET Version Tool: [GmodNET.VersionTool](https://www.nuget.org/packages/GmodNET.VersionTool/). It can be used from shell or any other build systems like make, cmake, etc.

[GmodNET.VersioTool.Core](https://www.nuget.org/packages/GmodNET.VersionTool.Core/) is a NuGet package containing .NET Standard 2.0 class library which can be used to generate version numbers from other .NET applications.
