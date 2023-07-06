using System.Diagnostics;
using INFOGR2023TemplateP2;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Template
{
    class MyApplication
    {
        // member variables
        public Surface screen;                  // background surface for printing etc.
        float a = 0;                            // teapot rotation angle
        readonly Stopwatch timer = new();       // timer for measuring frame duration
        Shader? baseShader;                     // shader to use for rendering
        Shader? skyboxShader;                   // skybox shader   
        Shader? shadowShader;                   // shadow map shader
        Shader? depthShader;                   // debug depth view

        Shader? postproc;                       // shader to use for post processing
        RenderTarget? target;                   // intermediate render target
        ScreenQuad? quad;                       // screen filling quad for post processing
        Camera camera;
        Light light;
        Skybox skybox;
        ShadowMap shadowMap;
        SceneGraph sceneGraph;
        float orbitAngle;
        float fov = 1.2f;

        // constructor
        public MyApplication(Surface screen)
        {
            this.screen = screen;
        }
        // initialize
        public void Init()
        {
            // load teapot
            // initialize stopwatch
            timer.Reset();
            timer.Start();

            // create shaders
            baseShader = new Shader("../../../shaders/vs.glsl", "../../../shaders/fs.glsl");
            skyboxShader = new Shader("../../../shaders/skybox_vs.glsl", "../../../shaders/skybox_fs.glsl");
            shadowShader = new Shader("../../../shaders/shadow_vs.glsl", "../../../shaders/shadow_fs.glsl");
            depthShader = new Shader("../../../shaders/depthQuad_vs.glsl", "../../../shaders/depthQuad_fs.glsl");

            postproc = new Shader("../../../shaders/vs_post.glsl", "../../../shaders/fs_post.glsl");
            // load a texture
            var lava = new Texture("../../../assets/lava.jpg");
            var lavalNormalMap = new Texture("../../../assets/lava-normal.jpg");

            var abs = new Texture("../../../assets/abs.jpg");
            var absNormalMap = new Texture("../../../assets/abs-normal.jpg");

            // create the render target
            target = new RenderTarget(screen.width, screen.height);
            quad = new ScreenQuad();

            this.camera = new Camera(new Vector3(0, 3, -20), screen.width / screen.height);
            this.light = new Light(new Vector3(-30,24, 0), new Vector3(2,2,2), 0.1f, 0.8f, 0.5f);

            //List<string> faces = new List<string>
            //{
            //    "../../../assets/right.jpg",
            //    "../../../assets/left.jpg",
            //    "../../../assets/top.jpg",
            //    "../../../assets/bottom.jpg",
            //    "../../../assets/front.jpg",
            //    "../../../assets/back.jpg"
            //};
            //this.skybox = new Skybox(faces);
            this.skybox = new Skybox("../../../assets/night-skybox.png");
            this.shadowMap = new ShadowMap(screen.width, screen.height);

            Matrix4 Tpot = Matrix4.CreateScale(0.75f) * Matrix4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f));
            Matrix4 Tpot2 = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(new Vector3(10.0f, 0.0f, 0.0f));


            Matrix4 Tfloor = Matrix4.CreateScale(1.0f) * Matrix4.CreateTranslation(new Vector3(0, 0, 0));

            SceneNode tpot = new SceneNode(new Mesh("../../../assets/teapot.obj", Tpot, abs, false, absNormalMap));
            SceneNode tpot2 = new SceneNode(new Mesh("../../../assets/teapot.obj", Tpot2, abs, true));
            SceneNode tpot3 = new SceneNode(new Mesh("../../../assets/teapot.obj", Tpot2, abs, true));
            SceneNode tpot4 = new SceneNode(new Mesh("../../../assets/teapot.obj", Tpot2, abs, true));
            SceneNode tpot5 = new SceneNode(new Mesh("../../../assets/teapot.obj", Tpot2, abs, true));

            SceneNode floor = new SceneNode(new Mesh("../../../assets/floor.obj", Tfloor, lava, false, lavalNormalMap));
                

            this.sceneGraph = new SceneGraph(camera, light, 1.2f);
            tpot.AddChild(tpot2);
            tpot.AddChild(tpot3);
            tpot.AddChild(tpot4);
            tpot.AddChild(tpot5);


            this.sceneGraph.AttachToRoot(tpot);


            this.sceneGraph.AttachToRoot(floor);
        }

        // tick for background surface
        public void Tick(KeyboardState input)
        {
            this.moveCamera(input);
            float fovDelta = 0.1f;

            if (input.IsKeyDown(Keys.D1))
            {
                this.sceneGraph.frustumFov = (float) Math.Min(Math.PI - 0.1f, this.sceneGraph.frustumFov + fovDelta);
            }

            if (input.IsKeyDown(Keys.D2))
            {
                this.sceneGraph.frustumFov = Math.Max(0.1f, this.sceneGraph.frustumFov - fovDelta);
            }

            screen.Clear(0);
        }


        private void moveCamera(KeyboardState input)
        {
            float delta = 0.2f;
            float rotateDelta = 3.0f;
            float fovDelta = 0.1f;

            if (input.IsKeyDown(Keys.W))
            {
                this.camera.MoveForward(delta);
            }

            if (input.IsKeyDown(Keys.A))
            {
                this.camera.MoveSide(delta);
            }

            if (input.IsKeyDown(Keys.S))
            {
                this.camera.MoveForward(-delta);

            }

            if (input.IsKeyDown(Keys.D))
            {
                this.camera.MoveSide(-delta);
            }


            if (input.IsKeyDown(Keys.Up))
            {
                this.camera.Rotate(rotateDelta, 0);
            }

            if (input.IsKeyDown(Keys.Left))
            {
                this.camera.Rotate(0, rotateDelta);
            }

            if (input.IsKeyDown(Keys.Down))
            {
                this.camera.Rotate(-rotateDelta, 0);

            }

            if (input.IsKeyDown(Keys.Right))
            {
                this.camera.Rotate(0, -rotateDelta);
            }

            if (input.IsKeyDown(Keys.Q))
            {
                this.camera.FOV += fovDelta;


            }

            if (input.IsKeyDown(Keys.E))
            {
                this.camera.FOV -= fovDelta;

            }
        }
        // tick for OpenGL rendering code
        public void RenderGL()
        {

            // measure frame duration
            float frameDuration = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();


            float orbitRadius = 10.0f;

            float lightOrbitRadius = 30.0f;

            float lightX = MathF.Cos(a) * lightOrbitRadius - 30;
            float lightY = 24.0f; 
            float lightZ = MathF.Sin(a) * lightOrbitRadius;

            this.light.Position = new Vector3(lightX, lightY, lightZ);

            var tpot = this.sceneGraph.root.children[0];

            tpot.mesh.model = Matrix4.CreateScale(0.75f) * Matrix4.CreateTranslation(new Vector3(0.0f, MathF.Sin(a) * 3.0f, 0.0f));

            for (int i = 0; i < 4; i++)
            {
                if (i < this.sceneGraph.root.children[0].children.Count)
                {
                    float angle = i * (MathF.PI / 2);

                    float x = MathF.Cos(angle) * orbitRadius;
                    float z = MathF.Sin(angle) * orbitRadius;

                    Matrix4 initialPosition = Matrix4.CreateTranslation(x, 0.0f, z);

                    Matrix4 selfRotation = Matrix4.CreateRotationY(a); 

                    Matrix4 model = Matrix4.CreateScale(0.5f) * initialPosition * selfRotation;

                    this.sceneGraph.root.children[0].children[i].mesh.model = model;
                }
            }

            Matrix4 view = this.camera.GetMatrix();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(this.fov, this.camera.aspectRatio, .1f, 1000);

            // update rotation
            a += 0.001f * frameDuration;
            if (a > 2 * MathF.PI) a -= 2 * MathF.PI;

            // render scene to shadow map
            this.shadowMap.Bind();
            this.sceneGraph.Render(shadowShader, view, projection, shadowMap.GetTextureId(), (uint)this.skybox.texture);
            this.shadowMap.Unbind();

            //if (depthShader != null)
            //    quad.Render(depthShader, (int)shadowMap.GetTextureId());

            GL.Viewport(0, 0, screen.width, screen.height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            // enable render target
            target.Bind();

            this.sceneGraph.Render(baseShader, view, projection, shadowMap.GetTextureId(), (uint)this.skybox.texture);

            if (skyboxShader != null)
            {
                this.skybox.Render(skyboxShader, view, projection);
            }

            // render quad
            target.Unbind();
            if (postproc != null)
                quad.Render(postproc, target.GetTextureID());

        }
    }
}