using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Template
{
    public class Texture
    {
        // data members
        public int id;

        // constructor
        public Texture(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentException(filename);
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            Image<Bgra32> bmp = Image.Load<Bgra32>(filename);
            int width = bmp.Width;
            int height = bmp.Height;
            int[] pixels = new int[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = (int)bmp[x, y].Bgra;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
        }
    }
}
