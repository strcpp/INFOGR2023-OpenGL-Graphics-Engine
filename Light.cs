using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOGR2023TemplateP2
{
    public class Light
    {
        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }
        public Vector3 Ia { get; set; }
        public Vector3 Id { get; set; }
        public Vector3 Is { get; set; }

        public Light(Vector3 position, Vector3 color, float ia, float id, float @is)
        {
            Position = position;
            Color = color;
            Ia = ia * color;
            Id = id * color;
            Is = @is * color;
        }   
    }
}
