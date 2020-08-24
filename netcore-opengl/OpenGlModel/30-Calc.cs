using Avalonia;
using System;
using System.Numerics;
using SearchAThing;

namespace SearchAThing
{

    public abstract partial class OpenGlModelBase : AvaloniaObject, IDisposable
    {

        /// <summary>
        /// convert given x,y coord ( origin left-bottom ) to normalized device coord
        /// </summary>
        /// <param name="ctl">control to retrieve info about screen size</param>
        /// <param name="pt">2d point in pixel ( origin left-bottom )</param>
        /// <returns>ndc point</returns>
        public Vector2 ToNDC(OpenGlControl ctl, Vector2 pt)
        {
            var w = ctl.Bounds.Width;
            var h = ctl.Bounds.Height;

            var ndcx = (float)(2f * pt.X / w - 1f);
            var ndcy = (float)(2f * pt.Y / h - 1f);

            return new Vector2(ndcx, ndcy);
        }

        public Line3D RayWorld(OpenGlControl ctl, float mouse_x, float mouse_y)
        {
            var pm = ctl.ProjectionMatrix;
            var vm = ctl.ViewMatrix;
            var mm = ctl.ModelMatrix;

            var w = ctl.Bounds.Width;
            var h = ctl.Bounds.Height;

            var mouse_ndc = ToNDC(ctl, new Vector2(mouse_x, mouse_y));

            var x = mouse_ndc.X;
            var y = mouse_ndc.Y;

            // TODO:
            return null;
        }

    }

}