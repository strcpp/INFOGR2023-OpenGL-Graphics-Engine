using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace INFOGR2023TemplateP2
{
    internal class ShadowMap
    {
        public int width { get; set; }
        public int height { get; set; }

        DepthTarget depthTarget;

        public ShadowMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            this.depthTarget = new DepthTarget(width, height);
        }

        public uint GetTextureId()
        {
            return depthTarget.texture;
        }
        public void Bind()
        {
            GL.Viewport(0, 0, width, height);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            this.depthTarget.Bind();
            GL.CullFace(CullFaceMode.Front);

        }

        public void Unbind()
        {
            this.depthTarget.Unbind();
            GL.CullFace(CullFaceMode.Back);

        }
    }
}
