using System;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Silk.NET.OpenGL;

namespace SearchAThing
{

    public partial class OpenGlControl
    {

        /// <summary>
        /// render builtin graphics such as orbit etc in NDC system
        /// </summary>
        /// <param name="GL">gl</param>
        /// <param name="context">avalonia drawing context</param>
        /// <param name="ps">control size</param>
        internal void RenderOverlay(GL GL, DrawingContext context, PixelSize ps)
        {
            if (ShowOrbit)
            {
                var vtxmgr = new VertexManager(1e-6);

                Func<Vector4> colfn = () => Colors.White.ToVector4();

                var pw = 1; // pixel width                

                var sw = Bounds.Width;
                var sh = Bounds.Height;

                orbitGeometry.Recompute(sw, sh);

                vtxmgr.AddCircle(orbitGeometry.Main.ToCircle3D(1e-6), colfn, pw, 360, this);
                vtxmgr.AddCircle(orbitGeometry.Top.ToCircle3D(1e-6), colfn, pw, 360, this);
                vtxmgr.AddCircle(orbitGeometry.Bottom.ToCircle3D(1e-6), colfn, pw, 360, this);
                vtxmgr.AddCircle(orbitGeometry.Left.ToCircle3D(1e-6), colfn, pw, 360, this);
                vtxmgr.AddCircle(orbitGeometry.Right.ToCircle3D(1e-6), colfn, pw, 360, this);

                if (OrbitShowCross)
                {
                    vtxmgr.AddLine(new Line3D(orbitGeometry.Bottom.Center, orbitGeometry.Top.Center), colfn, pw, 8, false, this);
                    vtxmgr.AddLine(new Line3D(orbitGeometry.Left.Center, orbitGeometry.Right.Center), colfn, pw, 8, false, this);
                }

                var pts = vtxmgr.Vtxs.Select(w => new GLVertex { Position = new Vector3(w.Position.X, w.Position.Y, -1) }).ToArray();

                Vector4? color = null;

                using (var VboTmp = new VertexBufferObject<GLVertex>(GL, BufferTargetARB.ArrayBuffer, pts))
                {
                    using (var VaoTmp = new VertexArrayObject<GLVertex>(GL))
                    {
                        VaoTmp.AttribPointer(Model.overlayPositionLocation, 0);

                        foreach (var figure in vtxmgr.FigureNames)
                        {
                            var idxs = vtxmgr.GetIdxs(figure);
                            color = vtxmgr.GetColor(figure);
                            using (var EboTmp = new VertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, idxs))
                            {
                                GL.Uniform3(Model.overlayUObjColLocation, color.HasValue ? color.Value.ToVector3() : new Vector3(1f, 1f, 1f));

                                unsafe
                                {
                                    GL.DrawElements(PrimitiveType.Triangles,
                                        (uint)idxs.Length, DrawElementsType.UnsignedInt, null);
                                }
                            }
                        }
                    }
                }

            }

        }

    }

}