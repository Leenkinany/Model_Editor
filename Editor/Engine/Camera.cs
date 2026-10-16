using System.IO;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Engine
{
    internal class Camera : Iserializable
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Vector3 Target { get; set; } = new Vector3(0, 0, 0);
        public Matrix View { get; set; } = Matrix.Identity;
        public Matrix Projection { get; set; } = Matrix.Identity;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000f;
        public float AspectRatio { get; set; } = 16f / 9f;
        public Viewport viewport { get; set; }

        public Camera() { }
        public Camera(Vector3 _position, float aspectRatio)
        {
            Update(_position, aspectRatio);
        }

        public void Update(Vector3 _position, float aspectRatio)
        {
            Position = _position;
            AspectRatio = aspectRatio;
            View = Matrix.CreateLookAt(_position, Target, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f),
                                                              AspectRatio,
                                                                NearPlane,
                                                                FarPlane);
        }

        public void Translate (Vector3 _translate)
        {
            float distance = Vector3.Distance(Target, Position);
            Vector3 forward = Target - Position;
            forward.Normalize();
            Vector3 left = Vector3.Cross(forward, Vector3.Up);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, forward);
            up.Normalize();
            Position += left * _translate.X * distance;
            Position += up * _translate.Y * distance;
            Position += forward * _translate.Z * 100f;
            Target += left * _translate.X * distance;
            Target += up * _translate.Y * distance;

            Update(Position, AspectRatio);
        }

        public void Rotate(Vector3 _rotate)
        {
            Position = Vector3.Transform(Position - Target, Matrix.CreateRotationY(_rotate.Y));
            Position += Target;
            Update(Position, AspectRatio);
        }

        public override string ToString()
        {
            string s = "Camera Position: " + Position.ToString();
            return s;
        }

        public void Serialize(BinaryWriter _stream)
        {
            HelperSerialize.Vec3(_stream, Position);
            _stream.Write(NearPlane);
            _stream.Write(FarPlane);
            _stream.Write(AspectRatio);
        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            Position = HelperDeserialize.Vec3(_stream);
            NearPlane = _stream.ReadSingle();
            FarPlane = _stream.ReadSingle();
            AspectRatio = _stream.ReadSingle();
            Update(Position, AspectRatio);
        }
    }
}
