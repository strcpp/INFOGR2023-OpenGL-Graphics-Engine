using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{
    public class ScreenQuad
    {
        // data members
        int vao = 0, vbo_idx = 0, vbo_vert = 0;
        float[] vertices =
        {
            -1,  1, 0, 0, 1,
             1,  1, 0, 1, 1,
            -1, -1, 0, 0, 0,
             1, -1, 0, 1, 0,
        };
        int[] indices = { 0, 1, 2, 3 };
        // constructor
        public ScreenQuad()
        {
        }

        // initialization; called during first render
        public void Prepare(Shader shader)
        {
            if (vbo_vert == 0)
            {
                vao = GL.GenVertexArray();
                GL.BindVertexArray(vao);
                // prepare VBO for quad rendering
                GL.GenBuffers(1, out vbo_vert);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vert);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 * 5 * 4), vertices, BufferUsageHint.StaticDraw);
                GL.GenBuffers(1, out vbo_idx);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_idx);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(16), indices, BufferUsageHint.StaticDraw);
            }
        }

        // render the mesh using the supplied shader and matrix
        public void Render(Shader shader, int textureID)
        {
            // on first run, prepare buffers
            Prepare(shader);

            // enable shader
            GL.UseProgram(shader.programID);

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.UseProgram(shader.programID);
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // enable position and uv attributes
            GL.EnableVertexAttribArray(shader.GetAttribute("vPosition"));
            GL.EnableVertexAttribArray(shader.GetAttribute("vUV"));

            // bind vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vert);

            // link vertex attributes to shader parameters 
            GL.VertexAttribPointer(shader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 20, 0);
            GL.VertexAttribPointer(shader.GetAttribute("vUV"), 2, VertexAttribPointerType.Float, false, 20, 3 * 4);

            // bind triangle index data and render
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo_idx);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            // disable shader
            GL.UseProgram(0);
        }
    }
}