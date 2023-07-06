using OpenTK.Graphics.OpenGL;

// based on http://www.opentk.com/doc/graphics/frame-buffer-objects

namespace Template
{
    class DepthTarget
    {
        uint fbo;
        public uint depthTexture;

        public uint texture { get { return depthTexture; } }

        int width, height;
        
        
        public DepthTarget(int screenWidth, int screenHeight)
        {
            width = screenWidth;
            height = screenHeight;
            // create color texture
            GL.GenTextures(1, out depthTexture);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)DepthFunction.Lequal);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] clampColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, clampColor);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // bind color and depth textures to fbo
            GL.Ext.GenFramebuffers(1, out fbo);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);

            GL.Ext.FramebufferTexture(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, depthTexture, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            //GL.ReadBuffer(ReadBufferMode.None);
            // test FBO integrity
            bool untestedBoolean = CheckFBOStatus();
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // return to regular framebuffer
        }

        public void Bind()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }
        public void Unbind()
        {
            // return to regular framebuffer
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
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