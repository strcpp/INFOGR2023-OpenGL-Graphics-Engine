using OpenTK.Graphics.OpenGL;

// based on http://www.opentk.com/doc/graphics/frame-buffer-objects

namespace Template
{
    class RenderTarget
    {
        uint fbo, resolveFbo;
        int colorTexture;
        uint colorRenderbuffer, depthRenderbuffer;
        int width, height;
        int samples = 16; // number of samples for multisampling

        public RenderTarget(int screenWidth, int screenHeight)
        {
            width = screenWidth;
            height = screenHeight;

            GL.GenRenderbuffers(1, out colorRenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRenderbuffer);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Rgba8, width, height);

            GL.GenRenderbuffers(1, out depthRenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.DepthComponent24, width, height);

            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, colorRenderbuffer);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);

            GL.GenTextures(1, out colorTexture);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.GenFramebuffers(1, out resolveFbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, resolveFbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);

            bool untestedBoolean = CheckFBOStatus();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public int GetTextureID()
        {
            return colorTexture;
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Unbind()
        {
            // resolve
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, resolveFbo);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    
        private bool CheckFBOStatus()
            {
                switch (GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
                {
                    case FramebufferErrorCode.FramebufferCompleteExt:
                        Console.WriteLine("FBO: The framebuffer is complete and valid for rendering.");
                        return true;
                    case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
                        Console.WriteLine("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                        break;
                    case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
                        Console.WriteLine("FBO: There are no attachments.");
                        break;
                    case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
                        Console.WriteLine("FBO: Attachments are of different size. All attachments must have the same width and height.");
                        break;
                    case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
                        Console.WriteLine("FBO: The color attachments have different format. All color attachments must have the same format.");
                        break;
                    case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
                        Console.WriteLine("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                        break;
                    case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
                        Console.WriteLine("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                        break;
                    case FramebufferErrorCode.FramebufferUnsupportedExt:
                        Console.WriteLine("FBO: This particular FBO configuration is not supported by the implementation.");
                        break;
                    default:
                        Console.WriteLine("FBO: Status unknown.");
                        break;
                }
                return false;
            }
    }
}