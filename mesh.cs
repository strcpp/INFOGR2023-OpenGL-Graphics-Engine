using System.Runtime.InteropServices;
using INFOGR2023TemplateP2;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Template
{
    // mesh and loader based on work by JTalton; http://www.opentk.com/node/642

    public class Mesh
    {
        // data members
        public ObjVertex[]? vertices;            // vertex positions, model space
        public ObjTriangle[]? triangles;         // triangles (3 vertex indices)
        public ObjQuad[]? quads;                 // quads (4 vertex indices)
        int vertexBufferId;                     // vertex buffer
        int triangleBufferId;                   // triangle buffer
        int quadBufferId;                       // quad buffer (not in Modern OpenGL)
        public Matrix4 model { get; set; }
        public Texture texture { get; set; }
        public Texture? normalMap { get; set; }

        public AABB aabb { get; set; }

        public bool mirror { get; set; }

        Matrix4 projectionLight = Matrix4.CreateOrthographicOffCenter(-10, 10, -10, 10, 0.1f, 75f);

        // constructor
        public Mesh(string fileName, Matrix4 model, Texture texture, bool mirror, Texture? normalMap = null)
        {
            MeshLoader loader = new();
            loader.Load(this, fileName);
            this.model = model;
            this.texture = texture;


            //Calculate initial AABB of mesh.
            if (vertices != null && vertices.Length > 0)
            {
                Vector3 firstVertex = vertices[0].Vertex;
                float minX = firstVertex.X, minY = firstVertex.Y, minZ = firstVertex.Z;
                float maxX = firstVertex.X, maxY = firstVertex.Y, maxZ = firstVertex.Z;

                for (int i = 1; i < vertices.Length; i++)
                {
                    Vector3 vertex = vertices[i].Vertex;
                    minX = Math.Min(minX, vertex.X);
                    minY = Math.Min(minY, vertex.Y);
                    minZ = Math.Min(minZ, vertex.Z);
                    maxX = Math.Max(maxX, vertex.X);
                    maxY = Math.Max(maxY, vertex.Y);
                    maxZ = Math.Max(maxZ, vertex.Z);
                }

                aabb = new AABB
                {
                    min = new Vector3(minX, minY, minZ),
                    max = new Vector3(maxX, maxY, maxZ),
                };


            }

            this.mirror = mirror;
            this.normalMap = normalMap;
        }

        // initialization; called during first render
        public void Prepare()
        {
            if (vertexBufferId == 0 && vertices != null && triangles != null && quads != null)
            {
                // generate interleaved vertex data (uv/normal/position/tangent/bitangent per vertex)
                GL.GenBuffers(1, out vertexBufferId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf(typeof(ObjVertex))), vertices, BufferUsageHint.StaticDraw);

                // generate triangle index array
                GL.GenBuffers(1, out triangleBufferId);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangleBufferId);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf(typeof(ObjTriangle))), triangles, BufferUsageHint.StaticDraw);

                if (OpenTKApp.allowPrehistoricOpenGL)
                {
                    // generate quad index array
                    GL.GenBuffers(1, out quadBufferId);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, quadBufferId);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf(typeof(ObjQuad))), quads, BufferUsageHint.StaticDraw);
                }
            } 
        }

        // render the mesh using the supplied shader and matrix
        public void Render(Shader shader, Matrix4 model, Matrix4 view, Matrix4 projection, Light light, Vector3 cameraPos,  Texture texture, uint shadowMapTexture,uint skyboxTexture)
        {
            // on first run, prepare buffers
            Prepare();

            Matrix4 viewLight = Matrix4.LookAt(light.Position, new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 1.0f, 0.0f));

            var lightMatrix = viewLight * projectionLight;


            // bind vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);

            // enable shader
            GL.UseProgram(shader.programID);

            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);
            
            GL.Uniform1(shader.GetUniform("shadowMap"), 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, shadowMapTexture);

            GL.Uniform1(shader.GetUniform("skybox"), 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.TextureCubeMap, skyboxTexture);

            if (normalMap != null)
            {
                GL.Uniform1(shader.GetUniform("normalMap"), 3);
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, normalMap.id);
                GL.Uniform1(shader.GetUniform("useNormalMap"), 1.0f);
            } else
            {
                GL.Uniform1(shader.GetUniform("useNormalMap"), 0.0f);
            }


            // pass transform to vertex shader
            GL.UniformMatrix4(shader.GetUniform("model"), false, ref model);
            GL.UniformMatrix4(shader.GetUniform("view"), false, ref view);
            GL.UniformMatrix4(shader.GetUniform("projection"), false, ref projection);
            GL.UniformMatrix4(shader.GetUniform("lightMatrix"), false, ref lightMatrix);
            GL.Uniform1(shader.GetUniform("mirror"), this.mirror ? 1.0f: 0.0f);

            GL.Uniform3(shader.GetUniform("light.position"), light.Position);
            GL.Uniform3(shader.GetUniform("light.Ia"), light.Ia);
            GL.Uniform3(shader.GetUniform("light.Id"), light.Id);
            GL.Uniform3(shader.GetUniform("light.Is"), light.Is);

            GL.Uniform3(shader.GetUniform("camPos"), cameraPos);

            GL.EnableVertexAttribArray(shader.GetAttribute("vPosition"));
            GL.EnableVertexAttribArray(shader.GetAttribute("vNormal"));
            GL.EnableVertexAttribArray(shader.GetAttribute("vUV"));
            GL.EnableVertexAttribArray(shader.GetAttribute("vTangent"));
            GL.EnableVertexAttribArray(shader.GetAttribute("vBitangent"));

            // link vertex attributes to shader parameters 
            GL.VertexAttribPointer(shader.GetAttribute("vUV"), 2, VertexAttribPointerType.Float, false, 56, 0);
            GL.VertexAttribPointer(shader.GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, 56, 2 * 4);
            GL.VertexAttribPointer(shader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 56, 5 * 4);
            GL.VertexAttribPointer(shader.GetAttribute("vTangent"), 3, VertexAttribPointerType.Float, false, 56, 8 * 4);
            GL.VertexAttribPointer(shader.GetAttribute("vBitangent"), 3, VertexAttribPointerType.Float, false, 56, 11 * 4);

            // bind triangle index data and render
            if (triangles != null && triangles.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangleBufferId);
                GL.DrawArrays(PrimitiveType.Triangles, 0, triangles.Length * 3);
            }

            // bind quad index data and render
            if (quads != null && quads.Length > 0)
            {
                if (OpenTKApp.allowPrehistoricOpenGL)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, quadBufferId);
                    GL.DrawArrays(PrimitiveType.Quads, 0, quads.Length * 4);
                }
                else throw new Exception("Quads not supported in Modern OpenGL");
            }

            // restore previous OpenGL state
            GL.UseProgram(0);
        }

        // layout of a single vertex
        [StructLayout(LayoutKind.Sequential)]
        public struct ObjVertex
        {
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Vertex;
            public Vector3 Tangent;
            public Vector3 Bitangent;
        }

        // layout of a single triangle
        [StructLayout(LayoutKind.Sequential)]
        public struct ObjTriangle
        {
            public int Index0, Index1, Index2;
        }

        // layout of a single quad
        [StructLayout(LayoutKind.Sequential)]
        public struct ObjQuad
        {
            public int Index0, Index1, Index2, Index3;
        }
    }
}