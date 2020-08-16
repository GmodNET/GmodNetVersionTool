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

The resulting version number has the form `[FullVersion]+codename.[codename from version JSON file].[tag or branch or pull].[tag or branch or pull request name].commit.[first 7 symbols of hexadecimal representation of git commit hash].[any additional metadata provided in a "Version" key-value pair of the version JSON file]` where `[FullVersion]` is computed as follows: if the `[version]` value of the `"Version"` key of the version JSON file DOES NOT contain pre-release identifier as described in the [item 9 of the Semantic Versioning 2.0.0 specification](https://semver.org/spec/v2.0.0.html#spec-item-9) then `[FullVersion]` is equal to `[version]`, if the `[version]` value of the `"Version"` key of the version JSON file DOES contain pre-release identifier then `[FullVersion]` equals `[version].[year].[month (numerically)].[day].[hour].[second]` where date and time are date and time of the git commit in Greenwich Mean Time (GMT). `codename.[codename from version JSON file]` can be dropped if there is no codename specified in the version JSON file.

## Examples of resulting version numbers

`0.6.0-beta.1.2020.8.15.13.24.56+branch.master.commit.7a9b35c` provided git repository at the `master` branch and version JSON file
```json
{
  "Version": "0.6.0-beta.1"
}
```

`1.0.0+codename.Ural.tag.1-0-0.commit.788963a.bugfix14` provided git repository at the `1.0.0` tag and version JSON file
```json
{
  "Version": "1.0.0+bugfix14",
  "Codename": "Ural"
}
```
