# netcore-opengl

.NET core opengl

- [API Documentation][api]
- [Changelog][changelog]

<hr/>

- [Introduction](#introduction)
- [Requirements](#requirements)
- [Quickstart](#quickstart)
- [Build solution](#build-solution)
- [Examples](#examples)
  - [Running examples from console](#running-examples-from-console)
  - [Running examples from vscode](#running-examples-from-vscode)
  - [List of examples](#list-of-examples)
- [Development key notes](#development-key-notes)
  - [Coordinate spaces](#coordinate-spaces)
  - [GL Dev inspect tool](#gl-dev-inspect-tool)
  - [VsCode settings](#vscode-settings)
  - [Primitives, figures interaction](#primitives-figures-interaction)
    - [Selection and coord identify](#selection-and-coord-identify)
    - [Removal](#removal)
    - [SimpleCmd](#simplecmd)
    - [Change rotation center](#change-rotation-center)
  - [Send notification](#send-notification)
  - [View invalidation model](#view-invalidation-model)
  - [Opengl debugging tools](#opengl-debugging-tools)
  - [Multiplatform](#multiplatform)
  - [Docker (mesa)](#docker-mesa)
  - [Software rendered (mesa)](#software-rendered-mesa)
  - [C# global usings (full)](#c-global-usings-full)
  - [Gestures](#gestures)
    - [Mouse gestures](#mouse-gestures)
    - [Keybindings](#keybindings)
- [Unit tests](#unit-tests)
- [How this project was built](#how-this-project-was-built)
  - [Documentation (github pages)](#documentation-github-pages)
    - [Build and view locally](#build-and-view-locally)
    - [Build and commit into docs branch](#build-and-commit-into-docs-branch)
- [References](#references)

<hr/>

![](data/images/example-0006.gif)

## Introduction

**netcore-opengl** library provides a multiplatform framework for 3D rendering, visualization and interactions.

The library is composed by following modules:

| module                               | framework        | dependencies                                                                                                           | description                                                       |
| ------------------------------------ | ---------------- | ---------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| **core** [![core-badge]][core]       | NET Standard 2.0 | [netcore-ext], [netcore-sci], [System.Drawing.Common], [Silk.NET], [Magick.NET], [SkiaSharp.HarfBuzz], [netdxf-devel0] | gl calculations, render abstraction over Silk.NET opengl library. |
| **gui** [![gui-badge]][gui]          | NET Standard 2.0 | [core], [Avalonia], [netcore-desktop]                                                                                  | desktop gl widget                                                 |
| **shapes** [![shapes-badge]][shapes] | NET Standard 2.0 | [core]                                                                                                                 | box, cone, sphere, arrow shapes                                   |
| **nurbs** [![nurbs-badge]][nurbs]    | NET Standard 2.0 | [core], [G-Shark]                                                                                                      | nurbs figures                                                     |

## Requirements

```sh
apt install libglfw3
```

## Quickstart

```sh
dotnet new console --use-program-main -n sample
cd sample
dotnet add package netcore-opengl-gui
dotnet add package netcore-opengl-shapes
code .
# accept to install vscode required assets
```

edit [`Program.cs`](examples/example-9000/Program.cs) as follows ( consider to create a separate global [usings.cs](#c-global-usings-full) file ):

```csharp
namespace sample;

// quickstart

using Vector3 = System.Numerics.Vector3;
using Color = System.Drawing.Color;
using SearchAThing.OpenGL.Core;
using static SearchAThing.OpenGL.Core.Constants;
using SearchAThing.OpenGL.GUI;
using static SearchAThing.OpenGL.GUI.Toolkit;
using SearchAThing.OpenGL.Shapes;

class Program
{
    static void Main(string[] args)
    {
        // this must called for console application to enable Avalonia framework
        // and must called before any other Avalonia control usage
        InitAvalonia();

        // create standalone Avalonia window for Silk.NET opengl rendering
        var w = GLWindow.Create();

        // define the GLModel build function
        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            // clear the model
            glModel.Clear();

            // place a point light at xyz=(2,2,2)
            glModel.PointLights.Add(new GLPointLight(2, 2, 2));

            // create and add a sphere centered at (0,0,0) with radius=1 and meshed as uvsphere with N=20 divisions
            var sphere = new UVSphere(center: Vector3.Zero, radius: 1);
            glModel.AddFigure(sphere.Figure(divisions: 20).SetColor(Color.Cyan));

            // place a base box which receive sphere shadow centered like the sphere but 2*z lower ( out of sphere )
            // with size xyz=(5, 5, .1f) larger than sphere but with small thickness
            var basebox = new Box(cs: WCS.Move(sphere.Center - Vector3.UnitZ * 2), csSize: new Vector3(5, 5, .1f));
            glModel.AddFigure(basebox.Sides);

            glCtl.CameraView(CameraViewType.Right);
        };

        // show the gl window
        w.ShowSync();
    }
}
```

```sh
dotnet run # or hit F5 from vscode
```

results ( control can manipulated with [gestures](#gestures) ):

[![img][sample]][sample]

[sample]: data/images/sample.png

## Build solution

```sh
cd netcore-opengl
# git submodule update --init
dotnet build
```

## Examples

### Running examples from console

```sh
cd netcore-opengl
dotnet run --project examples/example-0000
```

### Running examples from vscode

```sh
cd netcore-opengl
code .
```

C-S-p -> `NET: Generate Assets for Build and Debug`

choose an example.

Tip: to change startup example from bash `./set-startup-example xxxx` where xxxx is the nr of one of the examples provided.

This will update `.vscode/launch.json` then hit F5 to start.

### List of examples

Click on the `example code` link to open source code of the example, read top tour instructions contained in each example to test functions, for example following in the top comment of example 0000:

```cs
// example-0000
// draw a triangle with 3 colors (one for each vertex)
//
// use gesture such as:
// - 'w' to toggle wireframe
// - ctrl right/left to change tilt
// - mouse wheel to zoom
// - 'z' to zoomfit
// - ctrl + x to show bbox
```

<hr/>

Code: [0000][es0]

Draw a triangle with 3 colors (one for each vertex).

[![img][e0]][e0]

<hr/>

Code: [0001][es1]

Random lines ( console program ).

[![img][e1]][e1]

<hr/>

Code: [0002][es2] (mvvm)

Random lines ( avalonia AXAML program ).

[![img][e2]][e2]

<hr/>

Code: [0003][es3]

Render stl terrain map varying vertex colors by height ; presence of a point light makes shadows.

[![img][e3]][e3]

<hr/>

Code: [0004][es4]

Draw text.

[![img][e4]][e4]

<hr/>

Code: [0005][es5]

Draw box with keyboard face toggler.

[![img][e5]][e5]

<hr/>

Code: [0006][es6]

Draw nurb surface with triangles normal and animation, layout loaded from saved file.

[![img][e6]][e6]

<hr/>

Code: [0007][es7]

Draw nurb tube with triangle selection on click through raycast in perspective mode; generate gl split layout programmtically generated.

[![img][e7]][e7]

<hr/>

Code: [0008][es8] (mvvm)

Draw nurb tube with lighting tunable from mvvm interface.

[![img][e8]][e8]

<hr/>

Code: [0009][es9]

Generate two captures of different sizes from the same scene.

<hr/>

Code: [0010][es10]

Draw 3d shapes on a textured cube face.

[![img][e10]][e10]

<hr/>

Code: [0011][es11]

Texture, light and text transparency.

[![img][e11]][e11]

<hr/>

Code: [0012][es12]

Show text alignment types with their bounding box.

[![img][e12]][e12]

<hr/>

Code: [0013][es13]

Multiline text.

[![img][e13]][e13]

<hr/>

Code: [0014][es14]

Scalability benchmark for text.

[![img][e14]][e14]

<hr/>

Code: [0015][es15]

Raycast in orthogonal mode for snapping test.

[![img][e15]][e15]

<hr/>

Code: [0016][es16] (mvvm)

Invalidate control on vertex change.

[![img][e16]][e16]

<hr/>

Code: [0017][es17]

Figure using screen coord.

[![img][e17]][e17]

<hr/>

Code: [0018][es18]

Illusion of rotating base box model while its the camera that's rotating around. A small box rotates using object matrix in all scenes ; show camera frustum.

[![img][e18]][e18]

<hr/>

Code: [0019][es19] (mvvm)

Sphere vertex render and hittest scalability test.

[![img][e19]][e19]

<hr/>

Code: [0020][es20]

Customize key gesture.

<hr/>

Code: [0021][es21]

Use of raycast to pick vertexes and define a new ucs.

[![img][e21]][e21]

<hr/>

Code: [0022][es22]

Nurb surface join on two tubes.

[![img][e22]][e22]

<hr/>

Code: [0023][es23]

Show 1-D fem element displacement using. Dependency: [BriefFiniteElement].

[![img][e23]][e23]

<hr/>

Code: [0024][es24]

Show 3-D fem element displacement with countour and legend visible only in one of the split views using control and figure custom tag data. Dependency: [BriefFiniteElement]

[![img][e24]][e24]

<hr/>

Code: [0025][es25]

Nurb surface intersection generating nurb curves using [FeasibleTriIntersectionTests] extension method.

[![img][e25]][e25]

<hr/>

Code: [0026][es26]

Shows 2 triangle intersection and SimpleCmd management.

[![img][e26]][e26]

<hr/>

Code: [0027][es27]

Shows earth representation through a textured uv sphere.

[![img][e27]][e27]

<hr/>

Code: [0100][es100]

Use of raycast to pick vertexes over a sphere.

[![img][e100]][e100]

## Development key notes

Most of technical documentation is directly integrated with [API documentation](https://devel0.github.io/netcore-opengl/html/annotated.html).

### Coordinate spaces

![](data/notes/SpaceCoord.svg)

- **Object**, **Model**, **View** and **Projection** matrixes are used by the gl pipeline at the [vertex shader set gl position].
- **Clip**, **NDC** and finally **Screen** spaces are handled by the gl pipeline as further stages where size of render device was set by the [gl control render][set of gl viewport].
- Forward and backward coordinate transform can be done through provided core helper functions; these methods exists to preview gl transformations done by the gpu on client side and are used forwardly by the zoomfit in [compute screen bbox] or in the backward case to detect local point from screen through [local ray cast].
  - [forward transform](src/core/calc/ForwardTransform.cs)
  - [backward transform](src/core/calc/BackwardTransform.cs)

### GL Dev inspect tool

Hit `F1` gesture to open gl dev tool useful to understand how conversion translates between spaces; it provides some basic support such as:

- show render count
- show/edit GlView title
- toggle control perspective, shadow, texture, wireframe, shade with edge, show normals
- override light ambient, diffuse, specular strength
- change fovdeg, show camera coordinates and frustum
- toggle autoadjust near/far with near,far edit
- show bbox size and model/view/projection matrixes
- add, remove, set position, color of lights

![](data/images/gldevtool.gif)

### VsCode settings

relevant glsl settings

```json
{
  "[glsl]": {
      "editor.defaultFormatter": "xaver.clang-format"
  },
  "glsllint.additionalStageAssociations": {
      ".fs": "frag",
      ".vs": "vert",
      ".gs": "geom"
  },
  "clang-format.language.glsl.style": "WebKit",
}
```

### Primitives, figures interaction

#### Selection and coord identify

| cursor                   | type      | hotkey | description                                   |
| ------------------------ | --------- | ------ | --------------------------------------------- |
| ![img][normal-cursor]    | normal    | `s`    | Normal pan/zoom/rotate [gestures](#gestures). |
| ![img][primitive-cursor] | primitive | `s`    | Primitive selection toggler.                  |
| ![img][figure-cursor]    | figure    | `s`    | Figure selection toggler.                     |
| ![img][identify-cursor]  | identify  | `i`    | Identify coord.                               |

#### Removal

Select a primitive/figures then hit `d` key to delete from the model.

#### SimpleCmd

Each primitive/figure selected can be copied to clipboard by the `ctrl+c` key and can be pasted within `ctrl+v`.

Actual implementation doesn't support color information but only geometric data:

| Primitive | SimpleCmd                          |
| --------- | ---------------------------------- |
| point     | `p` x1,y1,z1;...                   |
| line      | `l` x1,y1,z1,x2,y2,z2;...          |
| triangle  | `t` x1,y1,z1,x2,y2,z2,x3,y3,z3;... |

For example a WCS object figure composed of 3 lines is expressed as follow SimpleCmd:

```
l 0,0,0,1,0,0;0,0,0,0,1,0;0,0,0,0,0,1
```

#### Change rotation center

- select a primitive ( `s` to enable selection )
- hit `ctrl+r`

![](data/images/change-rotation.gif)

To return at default rotation center hit `ctrl+r` again that is with no selection.

### Send notification

Use gl model send notification to display a message with following properties:

- _title_
- _message_
- _level_ : Information, Success, Warning, Error

### View invalidation model

The view invalidation follow these rules:

- View is refreshed automatically as a result of scale/rotate/pan predefined interactions.
- Gl model changes doesn't imply an invalidation of the view and user have to request view update through gl model Invalidate method.

### Opengl debugging tools

To implement some technical part of this library the [RenderDoc](https://renderdoc.org/) tool was a useful to investigate the content of the gl pipeline and to see the cube depth map generated to handle point light shadow rendering.

### Multiplatform

- The same binary compiled in a platform can run in others.
- For example compile the solution then try to copy an example `bin` folder to other machine, then issue `dotnet bin/Debug/net7.0/example-xxxx.dll`.

### Docker (mesa)

Unit tests of this projects can run in docker ( see [this folder](src/test/docker) ).

For a simple program execution there is an example in the offscreen rendering [example-0009](examples/example-0009/docker-example/), to execute it:

```sh
cd examples/example-0009/docker-example
./build.sh
./run.sh
```

that executes with follow output:

```
GL VERSION = 4.5 (Core Profile) Mesa 22.2.5
finished
Generated file:
total 52K
drwxr-xr-x 2 devel0 devel0 4.0K Mar 29 13:46 .
drwxrwxr-x 3 devel0 devel0 4.0K Mar 29 13:06 ..
-rw-r--r-- 1 devel0 devel0  21K Apr  3 10:33 example-0009-1024x768.png
-rw-r--r-- 1 devel0 devel0  18K Apr  3 10:33 example-0009-640x480.png
```

generated files can be found in the `examples/example-0009/docker-example/output` generated folder.

### Software rendered (mesa)

For example if you try to run the binary or sources from Windows guest in VirtualBox linux host you can receive follow error:

```
C:\Users\devel0\Downloads\bin\Debug\net7.0>dotnet example-0010.dll
Unhandled exception. System.AggregateException: One or more errors occurred. (ApiUnavailable: WGL: The driver does not appear to support OpenGL)
 ---> Silk.NET.GLFW.GlfwException: ApiUnavailable: WGL: The driver does not appear to support OpenGL
   at Silk.NET.GLFW.Glfw.<>c.<.cctor>b__141_0(ErrorCode errorCode, String description)
   at Silk.NET.GLFW.Glfw.CreateWindow(Int32 width, Int32 height, String title, Monitor* monitor, WindowHandle* share)
   at Silk.NET.Windowing.Glfw.GlfwWindow.CoreInitialize(WindowOptions opts)
   at Silk.NET.Windowing.Internals.WindowImplementationBase.CoreInitialize(ViewOptions opts)
   at Silk.NET.Windowing.Internals.ViewImplementationBase.Initialize()
   at SearchAThing.OpenGL.Core.GLContext..ctor() in /home/devel0/Documents/opensource/netcore-opengl/src/render/GLContext.cs:line 183
```

To overcome the issue you can execute with software rendered mesa graphics driver.

To install:

- Download [mesa library](https://fdossena.com/?p=mesa/index.frag).
- Unpack `MesaForWindows-x64-20.1.8.7z` in a folder.
- Set the environment variable `OPENGL_LIBRARY_PATH` to the path of the folder containing `opengl32.dll`.

![](data/images/mesa-software-rendered.png)

_Technical note:_

Mesa 20.1.8.7 doesn't expose glsl support for 4.6 regardless of that it contains effective implementation for that. To fix the problem `netcore-opengl` automatically set [two other environment variables](https://github.com/devel0/netcore-opengl/blob/37ad075f4bd983e9bfbeaa86d606fc25f3430eb5/src/render/GLContext.cs#L158-L159) when mesa is used.

### C# global usings (full)

Following is the list of global usings for app using gui and shapes modules.
Just create a global.cs file and put into your solution to avoid `using` on each single .cs file.

```cs
// core
global using System;
global using System.Linq;
global using System.Globalization;
global using System.Collections;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.IO;
global using System.Diagnostics;
global using System.Threading.Tasks;
global using System.Numerics;
global using System.ComponentModel;
global using System.Runtime.CompilerServices;
global using static System.Math;
global using static System.FormattableString;
global using Vector3 = System.Numerics.Vector3;
global using Color = System.Drawing.Color;
global using Size = System.Drawing.Size;
global using ColorTranslator = System.Drawing.ColorTranslator;
global using System.Reflection;

global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;
global using SkiaSharp;

global using Silk.NET.OpenGL;

global using SearchAThing.Ext;
global using static SearchAThing.Ext.Toolkit;

global using SearchAThing.Sci;
global using static SearchAThing.Sci.Toolkit;

global using SearchAThing.OpenGL.Core;
global using static SearchAThing.OpenGL.Core.Toolkit;
global using static SearchAThing.OpenGL.Core.Constants;

// gui
global using System.Threading;
global using Avalonia;
global using Avalonia.Input;
global using Point = Avalonia.Point;
global using Avalonia.Media;
global using AColor = Avalonia.Media.Color;
global using ABrush = Avalonia.Media.Brush;
global using Avalonia.Data.Converters;

global using SearchAThing.Desktop;

global using SearchAThing.OpenGL.GUI;
global using static SearchAThing.OpenGL.GUI.Toolkit;
global using static SearchAThing.OpenGL.GUI.Constants;

// shapes
global using SearchAThing.OpenGL.Shapes;
global using static SearchAThing.OpenGL.Shapes.Toolkit;
global using static SearchAThing.OpenGL.Shapes.Constants;

// nurbs
global using SearchAThing.OpenGL.Nurbs;
global using static SearchAThing.OpenGL.Nurbs.Toolkit;
```

### Gestures

#### Mouse gestures

| Key                 | Description                       |
| ------------------- | --------------------------------- |
| Left + Move         | Rotate the model over bbox middle |
| Middle + Move       | Pan                               |
| Middle double click | Zoom fit                          |

#### Keybindings

Key gesture can be overriden ( see [example-0020](https://github.com/devel0/netcore-opengl/blob/3943766b7cb98ae46149fbf14e54497f84ecf41f/examples/example-0020/Program.cs#L19-L23) ).

| Key              | Description                                                |
| ---------------- | ---------------------------------------------------------- |
| o                | View bOttom                                                |
| t                | View Top                                                   |
| l                | View Left                                                  |
| r                | View Right                                                 |
| f                | View Front                                                 |
| b                | View Back                                                  |
| i                | Toggle Identify coord                                      |
| s                | Toggle selection mode                                      |
| Ctrl + r         | Change rotation center                                     |
| Ctrl + ⬆         | Camera zoom in                                             |
| Ctrl + ⬇         | Camera zoom out                                            |
| Shift + ⬅        | Camera pan left                                            |
| Shift + ➡        | Camera pan right                                           |
| Shift + ⬆        | Camera pan up                                              |
| Shift + ⬇        | Camera pan up                                              |
| ⬅                | Model rotate left                                          |
| ➡                | Model rotate right                                         |
| ⬆                | Model rotate up                                            |
| ⬇                | Model rotate down                                          |
| Ctrl + ⬅         | Camera tilt left                                           |
| Ctrl + ➡         | Camera tilt right                                          |
| Alt + ⬅          | Camera rotate left                                         |
| Alt + ➡          | Camera rotate right                                        |
| Alt + ⬆          | Camera rotate up                                           |
| Alt + ⬇          | Camera rotate down                                         |
| h                | Split view horizontal                                      |
| v                | Split view vertical                                        |
| c                | Close current view                                         |
| w                | Toggle wireframe                                           |
| Ctrl + w         | Toggle (geom shader) shade with edges                      |
| Alt + v          | Toggle (geom shader) vertex visibility                     |
| n                | Toggle show normals                                        |
| p                | Toggle perspective                                         |
| x                | Toggle texture                                             |
| Ctrl + s         | Toggle shadow                                              |
| Ctrl + x         | Toggle model bbox                                          |
| Ctrl + Shift + c | Toggle camera object                                       |
| z                | Zoom fit                                                   |
| Ctrl + c         | Copy selected primitives/figures to clipboard as SimpleCmd |
| Ctrl + v         | Paste primitives/figures from clipboard SimpleCmd          |
| Delete           | Delete selected primitives/figures                         |
| Escape           | Cancel selection and back to view cursor mode              |
| F1               | Open dev tool                                              |
| F2               | Save current view                                          |
| F3               | Restore last saved view                                    |
| Shift + F2       | Save current view layout                                   |
| Shift + F3       | Restore last saved view layout                             |
| Ctrl + i         | Invalidate view                                            |

## Unit tests

- debugging unit tests
  - from vscode click `debug test` on codelens button
- executing all tests
  - from solution root folder `dotnet test`
- testing coverage
  - from vscode run task ( ctrl+shift+p ) `Tasks: Run Task` then `test with coverage` or use provided script `./generate-coverage.sh`
  - extensions required to watch coverage ( `Coverage Gutters` )

![](data/images/unit-tests-coverage-gutters.png)

## How this project was built

```sh
mkdir netcore-opengl
cd netcore-opengl

mkdir src examples

cd src
dotnet new classlib -n netcore-opengl-core
mv netcore-opengl-core core
cd ..

cd examples
dotnet new console --use-program-main -n example
mv example/example.csproj example/example-0001.csproj
mv example example-0001

dotnet new --install Avalonia.Templates
dotnet new avalonia.mvvm -n example
mv example/example.csproj example/example-0002.csproj
mv example example-0002

dotnet new classlib -n example-figures

dotnet new xunit -n test
cd test
dotnet add reference ../ext/netcore-ext.csproj
# enable test coverage collector
# to view in vscode ( "Coverage Gutters" ext ) run `./test-coverage` then `C-S-p` Coverage Gutters: Watch
dotnet add package coverlet.collector
dotnet add package coverlet.msbuild
cd ..

cd ..

dotnet new sln
dotnet sln add src/core src/test
dotnet sln add examples/example-0001 examples/example-0002 examples/example-figures
dotnet build
```

### Documentation (github pages)

Configured through Settings/Pages on Branch docs ( path /docs ).

- while master branch exclude "docs" with .gitignore the docs branch doesn't

#### Build and view locally

```sh
./doc build
./doc serve
./doc view
```

#### Build and commit into docs branch

```sh
./doc commit
```

## References

- [OpenGL Transformation](http://www.songho.ca/opengl/gl_transform.html)
- [The Perspective and Orthographic Projection Matrix](https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix/projection-matrix-GPU-rendering-pipeline-clipping.html)

<!-- LINKS -->

[api]: https://devel0.github.io/netcore-opengl/html/annotated.html
[changelog]: https://github.com/devel0/netcore-opengl/commits/master
[core-badge]: https://buildstats.info/nuget/netcore-opengl-core
[gui-badge]: https://buildstats.info/nuget/netcore-opengl-gui
[shapes-badge]: https://buildstats.info/nuget/netcore-opengl-shapes
[nurbs-badge]: https://buildstats.info/nuget/netcore-opengl-nurbs
[core]: https://www.nuget.org/packages/netcore-opengl-core
[gui]: https://www.nuget.org/packages/netcore-opengl-gui
[shapes]: https://www.nuget.org/packages/netcore-opengl-shapes
[nurbs]: https://www.nuget.org/packages/netcore-opengl-nurbs
[netcore-ext]: https://www.nuget.org/packages/netcore-ext
[netcore-sci]: https://www.nuget.org/packages/netcore-sci
[netdxf-devel0]: https://www.nuget.org/packages/netDxf-devel0
[netcore-desktop]: https://www.nuget.org/packages/netcore-desktop
[system.drawing.common]: https://www.nuget.org/packages/System.Drawing.Common
[silk.net]: https://www.nuget.org/packages/Silk.NET
[magick.net]: https://www.nuget.org/packages/Magick.NET-Q8-AnyCPU
[skiasharp.harfbuzz]: https://www.nuget.org/packages/SkiaSharp.HarfBuzz
[avalonia]: https://www.nuget.org/packages/Avalonia
[g-shark]: https://www.nuget.org/packages/GShark
[normal-cursor]: data/images/cursors/normal.png
[primitive-cursor]: data/images/cursors/primitive.png
[figure-cursor]: data/images/cursors/figure.png
[identify-cursor]: data/images/cursors/identify.png
[vertex shader set gl position]: https://github.com/devel0/netcore-opengl/blob/37ad075f4bd983e9bfbeaa86d606fc25f3430eb5/src/render/shaders/4.main.vs#L74
[set of gl viewport]: https://github.com/devel0/netcore-opengl/blob/3dbf0e483007c9eea979d091547e5fa08a85e082/src/render/GLControl.cs#L855
[local ray cast]: https://github.com/devel0/netcore-opengl/blob/37ad075f4bd983e9bfbeaa86d606fc25f3430eb5/src/core/calc/BackwardTransform.cs#L176
[compute screen bbox]: https://github.com/devel0/netcore-opengl/blob/ca1237b9b3b16c1a7fc2c673c9bf2de87160f12b/src/core/calc/Util.cs#L17
[brieffiniteelement]: https://github.com/BriefFiniteElementNet/BriefFiniteElement.Net
[es0]: examples/example-0000/Program.cs
[es1]: examples/example-0001/Program.cs
[es2]: examples/example-0002/Views/MainWindow.axaml.cs
[es3]: examples/example-0003/Program.cs
[es4]: examples/example-0004/Program.cs
[es5]: examples/example-0005/Program.cs
[es6]: examples/example-0006/Program.cs
[es7]: examples/example-0007/Program.cs
[es8]: examples/example-0008/Views/MainWindow.axaml.cs
[es9]: examples/example-0009/Program.cs
[es10]: examples/example-0010/Program.cs
[es11]: examples/example-0011/Program.cs
[es12]: examples/example-0012/Program.cs
[es13]: examples/example-0013/Program.cs
[es14]: examples/example-0014/Program.cs
[es15]: examples/example-0015/Program.cs
[es16]: examples/example-0016/Views/MainWindow.axaml.cs
[es17]: examples/example-0017/Program.cs
[es18]: examples/example-0018/Program.cs
[es19]: examples/example-0019/Views/MainWindow.axaml.cs
[es20]: examples/example-0020/Program.cs
[es21]: examples/example-0021/Views/MainWindow.axaml.cs
[es22]: examples/example-0022/Program.cs
[es23]: examples/example-0023/Program.cs
[es24]: examples/example-0024/Program.cs
[es25]: examples/example-0025/Program.cs
[es26]: examples/example-0026/Views/MainWindow.axaml.cs
[es27]: examples/example-0027/Program.cs
[es100]: examples/example-0100/Program.cs

[e0]: data/images/examples/0000.png
[e1]: data/images/examples/0001.png
[e2]: data/images/examples/0002.png
[e3]: data/images/examples/0003.png
[e4]: data/images/examples/0004.png
[e5]: data/images/examples/0005.png
[e6]: data/images/examples/0006.png
[e7]: data/images/examples/0007.png
[e8]: data/images/examples/0008.png
[e9]: data/images/examples/0009.png
[e10]: data/images/examples/0010.png
[e11]: data/images/examples/0011.png
[e12]: data/images/examples/0012.png
[e13]: data/images/examples/0013.png
[e14]: data/images/examples/0014.png
[e15]: data/images/examples/0015.png
[e16]: data/images/examples/0016.png
[e17]: data/images/examples/0017.png
[e18]: data/images/examples/0018.png
[e19]: data/images/examples/0019.png
[e21]: data/images/examples/0021.gif
[e22]: data/images/examples/0022.png
[e23]: data/images/examples/0023.png
[e24]: data/images/examples/0024.png
[e25]: data/images/examples/0025.png
[e26]: data/images/examples/0026.png
[e27]: data/images/examples/0027.png
[e100]: data/images/examples/0100.gif

[FeasibleTriIntersectionTests]: https://github.com/devel0/netcore-opengl/blob/0ce534f6fcbb62279d814b0eca08f5be97ec8f98/src/core/primitive/IGLTriangle.cs#L113
