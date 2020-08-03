# netcore-opengl

[![NuGet Badge](https://buildstats.info/nuget/netcore-opengl)](https://www.nuget.org/packages/netcore-opengl/)

.NET core opengl

<hr/>

- [API Documentation](https://devel0.github.io/netcore-opengl/api/SearchAThing.html)
- [Recent changes](#recent-changes)
- [Quickstart](#quickstart)
- [Examples](#examples)
- [How this project was built](#how-this-project-was-built)

<hr/>

## Recent changes

## Quickstart

- [nuget package](https://www.nuget.org/packages/netcore-opengl/)

in .cs file

```csharp
using SearchAThing;
```

in .xaml file specify namespace

```
xmlns:opengl="clr-namespace:SearchAThing;assembly=netcore-opengl"
```

to run examples

```sh
cd netcore-opengl
code .
```

hit F5 to start example ( change by edit [.vscode/launch.json](.vscode/launch.json) )

## Examples

#### 0001

Triangle

![](data/img/example-0001.gif)

## How this project was built

```sh
mkdir netcore-opengl
cd netcore-opengl

dotnet new sln
dotnet new classlib -n netcore-opengl

cd netcore-opengl
dotnet add package netcore-util --version 1.6.1
dotnet add package netcore-sci --version 1.8.0
dotnet add package Silk.NET --version 1.4.0
dotnet add package QuantumConcepts.Formats.STL.netcore --version 1.3.1
dotnet add package Avalonia --version 0.10.0-preview2
cd ..

dotnet sln add netcore-opengl
dotnet restore
dotnet build
```
