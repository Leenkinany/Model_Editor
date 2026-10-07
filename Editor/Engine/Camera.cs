using System.IO;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Editor.Engine
{
    internal class Camera : Iserializable
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Matrix View { get; set; } = Matrix.Identity;
        public Matrix Projection { get; set; } = Matrix.Identity;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000f;
        public float AspectRatio { get; set; } = 16f / 9f;

        public Camera() { }
        public Camera(Vector3 _position, float aspectRatio)
        {
            Update(_position, aspectRatio);
        }

        public void Update(Vector3 _position, float aspectRatio)
        {
            Position = _position;
            AspectRatio = aspectRatio;
            View = Matrix.CreateLookAt(_position,
                new Vector3(0, 0, 0),
                Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f),
                                                              AspectRatio,
                                                                NearPlane,
                                                                FarPlane);
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
