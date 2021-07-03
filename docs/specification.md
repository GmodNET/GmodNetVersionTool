# Specification of the GmodNET.VersionTool

## Description

`GmodNET.VersionTool` generates rich, [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html) compatible version number (string) by combining the content of version file and git repository metadata.

## Specification

To generate a version number `GmodNET.VersionTool` takes a version JSON file of the form
```json
{
  "Version": "[version]",
  "Codename": "[codename]"
}
```
within git repository, where `"Version"` key MUST correspond to a [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html) compatible version number value and `"Codename"` key MAY correspond to a string value which, if specified, MUST be a single word of ASCII alphanumerics and hyphens [0-9A-Za-z-].

The resulting version number has the form `[FullVersion]+codename.[codename from version JSON file].head.[git head name].commit.[hexadecimal representation of git commit hash].[any additional metadata provided in a "Version" key-value pair of the version JSON file]` where `[FullVersion]` is computed as follows: if the `[version]` value of the `"Version"` key of the version JSON file DOES NOT contain pre-release identifier as described in the [item 9 of the Semantic Versioning 2.0.0 specification](https://semver.org/spec/v2.0.0.html#spec-item-9) then `[FullVersion]` is equal to `[version]`, if the `[version]` value of the `"Version"` key of the version JSON file DOES contain pre-release identifier then `[FullVersion]` equals `[version].[number of seconds elapsed since January 1st, 2020 UTC].[git head name]`. `codename.[codename from version JSON file]` can be dropped if there is no codename specified in the version JSON file. `[git head name]` is equal to a branch's name if repository HEAD points to one, or is of the form `tag-[git tag name]` if HEAD points to a tagged commit (for example `tag-1-0-1`), or equals to `detached-HEAD` otherwise (checkout at pull request, arbitrary commit, etc).

## Examples of resulting version numbers

`0.6.0-beta.1.943586.NewFeature+head.NewFeature.commit.a3455d90049f17b6ac3bbc54177cfb51b0803851` provided git repository head is at the `NewFeature` branch and version JSON file
```json
{
  "Version": "0.6.0-beta.1"
}
```

`1.0.0+codename.Ural.head.master.commit.e796d2d62d39256f9c3e55b3648cd2bfb289e7bc.bugfix14` provided git repository head is at the `master` branch and version JSON file
```json
{
  "Version": "1.0.0+bugfix14",
  "Codename": "Ural"
}
```
