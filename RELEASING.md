# Releasing

## ISB for C# and Unity

### The NuGet package of ISB

[https://www.nuget.org/packages/ISB/](https://www.nuget.org/packages/ISB/)

### Publishing a pre-release build

To publish a pre-release build of the NuGet package:

```shell
cd csharp
dotnet clean
rm ISB/bin/Release/ISB.*.nupkg
dotnet pack -c Release -p:ReleaseTag=preview ISB/
dotnet nuget push ISB/bin/Release/ISB.*.nupkg --api-key <apikey> \
  --source https://api.nuget.org/v3/index.json
```

`ReleaseTag` can be an arbitrary string tag to indicate the build type.
Recommended tags include `alpha`, `beta` and `preview`.

The `<apikey>` to access NuGet.org is maintained by the administrator of the dev
team.

The [semantic](https://semver.org/) version number `<major>.<minor>.<patch>`
will not change when publishing pre-release builds, while the pre-release tag
will be updated, composed of the release tag and an auto-generated timestamp.
For example:

```text
0.1.2-preview20211013074315
```

### Publishing a formal release build

To publish a formal release build of the NuGet package:

Manually assign a new version number `<major>.<minor>.<patch>` to the release
build.

Create a release branch and prepare a PR to update the `VersionPrefix` property
accordingly at:

* [ISB.csproj](./csharp/ISB/ISB.csproj)
* [ISB.Shell.csproj](./csharp/ISB.Shell/ISB.Shell.csproj)

Get the release PR approved by the dev team and merged into main branch. Tag the
main branch with a release tag like `v<major>.<minor>.<patch>`.

Build and publish the package to NuGet:

```shell
cd csharp
dotnet clean
rm ISB/bin/Release/ISB.*.nupkg
dotnet pack -c Release -p:ReleaseTag=release ISB/
dotnet nuget push ISB/bin/Release/ISB.*.nupkg --api-key <apikey> \
 --source https://api.nuget.org/v3/index.json
```

Once `ReleaseTag` is set to `release` as in the above command line, an
auto-generated timestamp will be attached to the [semantic](https://semver.org/)
version number as its build metadata. For example:

```text
0.1.2+20211013074315
```
