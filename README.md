# netcore-opengl

[![NuGet Badge](https://buildstats.info/nuget/netcore-opengl)](https://www.nuget.org/packages/netcore-opengl/)

.NET core opengl

- [API Documentation](https://devel0.github.io/netcore-opengl/html/annotated.html)
- [Changelog](https://github.com/devel0/netcore-opengl/commits/master)

<hr/>

<!-- TOC -->
* [Quickstart](#quickstart)
* [build](#build)
* [Keynotes](#keynotes)
* [Examples](#examples)
    - [0001](#0001)
    - [0002](#0002)
    - [0003](#0003)
    - [0004](#0004)
* [How this project was built](#how-this-project-was-built)
<!-- TOCEND -->

<hr/>

## Quickstart

- prerequisite
```sh
apt install libglfw3
```

- [nuget package](https://www.nuget.org/packages/netcore-opengl/)

- [extension methods](https://devel0.github.io/netcore-opengl/html/class_search_a_thing_1_1_open_gl_ext.html)

```csharp
using SearchAThing;
```

- controls xaml

```
xmlns:opengl="clr-namespace:SearchAThing;assembly=netcore-opengl"
```

- [toolkit methods](https://devel0.github.io/netcore-opengl/html/class_search_a_thing_1_1_open_gl_toolkit.html)

```cs
using static SearchAThing.OpenGlToolkit;
```

- run examples

```sh
cd netcore-opengl
code .
```

hit F5 to start example ( change by edit [.vscode/launch.json](.vscode/launch.json) )

## build

```sh
mkdir ~/opensource
git clone https://github.com/devel0/netcore-util.git
git clone https://github.com/devel0/netcore-sci.git
git clone https://github.com/devel0/netcore-opengl.git

cd netcore-opengl
dotnet build
```

:point_right: To make dependency netcore-util/netcore-sci debuggable comment `PackageReference` and uncomment `ProjectReference` for corresponding reference from [csproj](netcore-opengl/netcore-opengl.csproj)

## Keynotes

- create a derived class from [OpenGlModelBase][1]:
    - *OnInitialized* can be used to init VertexArray and VertexBuffer objects for static data; compile shaders; retrieve attrib and uniform locations; define attrib pointers
    - *Render* can be used to draw frame
- create a derived class from [OpenGlControl][2] or use directly in xaml if not need to specialize
- glue [Model][3] to the [control][4]
- [draw something][5] using [vertex manager][6] ; it will rendered as [coloured triangles][7]
- set Debug in [OpenGlModelOptions][8] to get notified on console about GL error and [break debugger][9] in DEBUG mode

[1]: https://github.com/devel0/netcore-opengl/blob/7d54fd507c60c20e1a95183f071a8e4c04f19921/examples/0001/SampleGlModel.cs#L10
[2]: https://github.com/devel0/netcore-opengl/blob/7d54fd507c60c20e1a95183f071a8e4c04f19921/examples/0001/SampleGlControl.cs#L6
[3]: https://github.com/devel0/netcore-opengl/blob/7d54fd507c60c20e1a95183f071a8e4c04f19921/examples/0001/MainWindow.xaml.cs#L36
[4]: https://github.com/devel0/netcore-opengl/blob/7d54fd507c60c20e1a95183f071a8e4c04f19921/examples/0001/MainWindow.xaml#L28
[5]: https://github.com/devel0/netcore-opengl/blob/6f38e6d78aab1e89507e76265ba2a4a7b0a65610/examples/0003/SampleGlModel.cs#L185-L312
[6]: https://github.com/devel0/netcore-opengl/blob/0fae8b7cebae277283e8d7e48ab2c9a02e5f517d/netcore-opengl/VertexManager/VertexManager.cs#L17
[7]: https://github.com/devel0/netcore-opengl/blob/6f38e6d78aab1e89507e76265ba2a4a7b0a65610/examples/0003/SampleGlModel.cs#L153
[8]: https://github.com/devel0/netcore-opengl/blob/7d54fd507c60c20e1a95183f071a8e4c04f19921/examples/0001/MainWindow.xaml.cs#L38
[9]: https://github.com/devel0/netcore-opengl/blob/5a7fde43360408fad7407f1e1f20c2606d8b683d/netcore-opengl/OpenGlModel/00-OpenGlModelBase.cs#L299

## Examples

#### 0001

Triangle

![](data/img/example-0001.gif)

#### 0002

STL map

- OrthoFit

![](data/img/example-0002a.gif)

- GridSplitManager

![](data/img/example-0002b.gif)

#### 0003

Show how to highlight mouse hovered 3d elements even in perspective mode

![](data/img/example-0003.gif)

<img width=400 src="data/img/example-0003b.gif"/>

#### 0004

[Model animation][100]

![](data/img/example-0004.gif)

[100]: https://github.com/devel0/netcore-opengl/blob/5a7fde43360408fad7407f1e1f20c2606d8b683d/examples/0004/SampleGlModel.cs#L204

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
