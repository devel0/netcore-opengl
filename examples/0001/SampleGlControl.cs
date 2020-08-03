using System;
using System.Numerics;
using System.Threading;
using Avalonia;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;

namespace SearchAThing.SciExamples
{

    public class SampleGlControl : OpenGlControl
    {

        public SampleGlControl()
        {
        }

        protected override void OnModelAttached()
        {
            InvalidateVisual();
        }

    }


}