using System.Globalization;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

// The template provides you with a window which displays a 'linear frame buffer', i.e.
// a 1D array of pixels that represents the graphical contents of the window.

// Under the hood, this array is encapsulated in a 'Surface' object, and copied once per
// frame to an OpenGL texture, which is then used to texture 2 triangles that exactly
// cover the window. This is all handled automatically by the template code.

// Before drawing the two triangles, the template calls the Tick method in MyApplication,
// in which you are expected to modify the contents of the linear frame buffer.

// After (or instead of) rendering the triangles you can add your own OpenGL code.

// We will use both the pure pixel rendering as well as straight OpenGL code in the
// tutorial. After the tutorial you can throw away this template code, or modify it at
// will, or maybe it simply suits your needs.

namespace Template
{
    public class OpenTKApp : GameWindow
    {

        public const bool allowPrehistoricOpenGL = false;

        static int screenID;            // unique integer identifier of the OpenGL texture
        static MyApplication? app;       // instance of the application
        static bool terminated = false; // application terminates gracefully when this is true

        ScreenQuad quad;
        Shader screenShader;

        public OpenTKApp()
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Profile = allowPrehistoricOpenGL ? ContextProfile.Compatability : ContextProfile.Core,  // required for fixed-function, which is probably not supported on MacOS
            })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            // called during application initialization
            GL.ClearColor(0, 0, 0, 0);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Multisample);


            Surface screen = new(ClientSize.X, ClientSize.Y);
            app = new MyApplication(screen);
            screenID = app.screen.GenTexture();
            if(allowPrehistoricOpenGL)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            }
            else
            {
                quad = new ScreenQuad();
                screenShader = new Shader("../../../shaders/screen_vs.glsl", "../../../shaders/screen_fs.glsl");
            }
            app.Init();
        }
        protected override void OnUnload()
        {
            base.OnUnload();
            // called upon app close
            GL.DeleteTextures(1, ref screenID);
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            // called upon window resize. Note: does not change the size of the pixel buffer.
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            if (allowPrehistoricOpenGL)
            {
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
            }
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // called once per frame; app logic
            var keyboard = KeyboardState;
            if (keyboard[Keys.Escape]) terminated = true;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            // called once per frame; render
            if (app != null) app.Tick(KeyboardState);
            if (terminated)
            {
                Close();
                return;
            }
            // convert MyApplication.screen to OpenGL texture
            if (app != null)
            {
                GL.ClearColor(Color4.Black);
                GL.Disable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, screenID);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                               app.screen.width, app.screen.height, 0,
                               PixelFormat.Bgra,
                               PixelType.UnsignedByte, app.screen.pixels
                             );
                if (allowPrehistoricOpenGL)
                {
                    GL.Enable(EnableCap.Texture2D);
                    GL.Color3(1.0f, 1.0f, 1.0f);
                    // draw screen filling quad
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Begin(PrimitiveType.Quads);
                    GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
                    GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
                    GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
                    GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
                    GL.End();
                    GL.Disable(EnableCap.Texture2D);
                }
                else
                {
                    quad.Render(screenShader, screenID);
                }
                // prepare for generic OpenGL rendering
                GL.Enable(EnableCap.DepthTest);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                // do OpenGL rendering
                app.RenderGL();
            }
            // tell OpenTK we're done rendering
            SwapBuffers();
        }
        public static void Main()
        {
            // entry point
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            using OpenTKApp app = new();
            app.RenderFrequency = 30.0;
            app.Run();
        }
    }
}