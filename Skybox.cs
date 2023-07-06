using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using Template;
using OpenTK.Mathematics;

namespace INFOGR2023TemplateP2
{
    // https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/6.2.cubemaps_environment_mapping/cubemaps_environment_mapping.cpp
    internal class Skybox
    {
        public int texture { get; set; }
        int vertexBufferId;
        float[] SkyboxVertices = new float[] {
            // positions          
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            -1.0f,  1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f
        };

        public void Prepare()
        {
            if (vertexBufferId == 0)
            {
                // generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
                GL.GenBuffers(1, out vertexBufferId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(SkyboxVertices.Length * sizeof(float)), SkyboxVertices, BufferUsageHint.StaticDraw);
            } 
        }

        public Skybox(string imagePath)
        {
            // Load the image
            Image<Bgra32> image = Image.Load<Bgra32>(imagePath);

            // Calculate face dimensions
            int faceWidth = image.Width / 4;
            int faceHeight = image.Height / 3;

            // X and Y offsets for each face in the single image
            int[][] faceOffsets = new int[][] {
                new int[] { 2, 1 }, // +X
                new int[] { 0, 1 }, // -X
                new int[] { 1, 0 }, // +Y
                new int[] { 1, 2 }, // -Y
                new int[] { 1, 1 }, // +Z
                new int[] { 3, 1 }  // -Z
            };


            // Create texture
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);

            // Extract each face and upload to GPU
            for (int face = 0; face < 6; face++)
            {
                // Create array to store pixel data for one face
                int[] pixels = new int[faceWidth * faceHeight];

                // Copy pixel data for one face
                for (int y = 0; y < faceHeight; y++)
                {
                    for (int x = 0; x < faceWidth; x++)
                    {
                        int srcX = x + faceWidth * faceOffsets[face][0];
                        int srcY = y + faceHeight * faceOffsets[face][1];
                        pixels[y * faceWidth + x] = (int)image[srcX, srcY].Bgra;
                    }
                }

                // Upload to GPU
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + face, 0, PixelInternalFormat.Rgba, faceWidth, faceHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            }

            // Set texture parameters
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }


        public Skybox(List<string> faces)
        {
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);

            for(int i = 0; i < faces.Count; i++)
            {
                Image<Bgra32> bmp = Image.Load<Bgra32>(faces[i]);

                int width = bmp.Width;
                int height = bmp.Height;
                int[] pixels = new int[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        pixels[y * width + x] = (int)bmp[x, y].Bgra;

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        }

        public void Render(Shader shader, Matrix4 view, Matrix4 projection)
        {
            Prepare();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);


            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);


            GL.DepthFunc(DepthFunction.Lequal);
            GL.UseProgram(shader.programID);

            GL.Uniform1(shader.GetUniform("skybox"), 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);

            view = new Matrix4(new Matrix3(view));

            GL.UniformMatrix4(shader.GetUniform("view"), false, ref view);
            GL.UniformMatrix4(shader.GetUniform("projection"), false, ref projection);


            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.DepthFunc(DepthFunction.Less);

        }

    }
}
