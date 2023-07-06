using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Template;

namespace INFOGR2023TemplateP2
{
    internal class Camera
    {
        public Vector3 position { get; set; }
        public Vector3 direction { get; set; }
        public Vector3 tempDir { get; set; }
        public Vector3 up { get; set; }
        public Vector3 right { get; set; }

        public float yaw, pitch;

        float fov = 45.0f;
        public float FOV { get; set; } = 45f;
        public float focalDistance { get; set; } = 1.0f;

        public float aspectRatio { get; set; }

        public Camera(Vector3 position, float aspectRatio)
        {
            this.position = position;
            direction = new Vector3(0, 0, 1);
            tempDir = new Vector3(0, 0, 1);
            up = new Vector3(0, 1, 0);
            this.right = Vector3.Cross(this.up, this.direction).Normalized();
            this.aspectRatio = aspectRatio;
        }

        public void MoveForward(float delta)
        {
            this.position += this.direction * delta;
        }

        public void MoveSide(float delta)
        {
            this.position += this.right * delta;
        }

        public void Rotate(float deltaX, float deltaY)
        {
            this.pitch += deltaX;
            this.yaw += deltaY;

            var rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(this.pitch));

            var rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(this.yaw));

            var temp = rotationY * rotationX * new Vector4(this.tempDir.X, this.tempDir.Y, this.tempDir.Z, 1);

            this.direction = new Vector3(temp.X, temp.Y, temp.Z).Normalized();

            this.right = Vector3.Cross(this.up, this.direction).Normalized();

        }

        public Matrix4 GetMatrix()
        {
            return Matrix4.LookAt(this.position, this.position + this.direction, this.up);
        }
    }
}
