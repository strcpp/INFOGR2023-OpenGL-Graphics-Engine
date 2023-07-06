using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template;

namespace INFOGR2023TemplateP2
{

    internal class SceneNode
    {
        public List<SceneNode> children { get; set; }
        public Mesh mesh { get; set; }

        public SceneNode(Mesh mesh)
        {
            this.mesh = mesh;
            this.children = new List<SceneNode>();
        }

        public SceneNode AddChild(SceneNode child)
        {
            children.Add(child);
            return child;
        }
    }

    internal class SceneGraph
    {
        public SceneNode root { get; set; }
        Light light;
        Camera camera;
        
        public float frustumFov { get; set; }

        public SceneGraph(Camera camera, Light light, float frustumFov)
        {
            this.root = new SceneNode(null);
            this.light = light;
            this.camera = camera;
            this.frustumFov = frustumFov;
        }

        public void AttachToRoot(SceneNode node)
        {
            this.root.AddChild(node);
        }

        public void Render(Shader shader, Matrix4 view, Matrix4 projection, uint shadowMapTexture = 0, uint skyboxTexture = 0)
        {
            Matrix4 frustumProjection = Matrix4.CreatePerspectiveFieldOfView(this.frustumFov, this.camera.aspectRatio, .1f, 100);


            Matrix4 fviewProjectionMatrix =  view  * frustumProjection;

            RenderRecursive(shader, root, view, projection, fviewProjectionMatrix, shadowMapTexture, skyboxTexture);
        }

        public void RenderRecursive(Shader shader, SceneNode parent, Matrix4 view, Matrix4 projection, Matrix4 frustumVP,  uint shadowMapTexture, uint skyboxTexture)
        {
            foreach (var child in parent.children)
            {
                Matrix4 parentTransform = parent.mesh == null ? Matrix4.Identity : parent.mesh.model;


                child.mesh.Render(shader, parentTransform * child.mesh.model, view, projection, this.light,
                    this.camera.position, child.mesh.texture, shadowMapTexture, skyboxTexture);

                RenderRecursive(shader, child, view, projection, frustumVP, shadowMapTexture, skyboxTexture);

            }
        }
    }
}
