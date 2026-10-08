using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Editor.Engine
{
    class Models : Iserializable
    {
        public Model Mesh { get; set; }
        public Effect Shader { get; set; }
        public Vector3 Position { get=> m_position; set{ m_position = value; } }
        public Vector3 Rotation { get=>m_rotation; set{ m_rotation = value; } }
        public float Scale { get; set; }

        public float RotationSpeed { get; set; }
        public float OrbitSpeed { get; set; }
        public float OrbitAngle { get; set; }
        public float OrbitRadius { get; set; }
        public Models Parent { get; set; }


        public Texture Texture { get; set; }

        private Vector3 m_position;
        private Vector3 m_rotation;

        public Models() { }

        public Models(ContentManager content,
            string _model, 
            string _texture, 
            string _effect, 
            Vector3 _position, 
            float _scale)
        {
            Create(content, _model, _texture, _effect, _position, _scale);
        }

        public void Create(ContentManager content,
            string _model,
            string _texture,
            string _effect,
            Vector3 _position,
            float _scale)
        {
            Mesh = content.Load<Model>(_model);
            Mesh.Tag = _model;

            Texture = content.Load<Texture>(_texture);
            Texture.Tag = _texture;

            Shader = content.Load<Effect>(_effect);
            Shader.Tag = _effect;

            SetShader(Shader);
            Position = _position;
            Scale = _scale;
        }
        public void SetShader(Effect _effect)
        {
            Shader = _effect;
            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Shader;
                }
            }
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                   Matrix.CreateTranslation(Position);
        }

        public void Render(Matrix _view, Matrix _projection)
        {
            Shader.Parameters["World"].SetValue(GetTransform());
            Shader.Parameters["WorldViewProjection"].SetValue(GetTransform() * _view * _projection);
            Shader.Parameters["Texture"].SetValue(Texture);

            foreach(ModelMesh mesh in Mesh.Meshes)
            {
                mesh.Draw();
            }
        }

        public void Serialize(BinaryWriter _stream)
        { 
            _stream.Write(Mesh.Tag.ToString());
            _stream.Write(Texture.Tag.ToString());
            _stream.Write(Shader.Tag.ToString());
            HelperSerialize.Vec3(_stream, Position);
            HelperSerialize.Vec3(_stream, Rotation);
            _stream.Write(Scale);

            _stream.Write(RotationSpeed);
            _stream.Write(OrbitSpeed);
            _stream.Write(OrbitAngle);
            _stream.Write(OrbitRadius);
        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();
            Position = HelperDeserialize.Vec3(_stream);
            Rotation = HelperDeserialize.Vec3(_stream);
            Scale = _stream.ReadSingle();

            RotationSpeed = _stream.ReadSingle();
            OrbitSpeed = _stream.ReadSingle();
            OrbitAngle = _stream.ReadSingle();
            OrbitRadius = _stream.ReadSingle();

            Create(_content, mesh, texture, shader, Position, Scale);
        }

        public void Update()
        {
            Rotation = new Vector3(Rotation.X, Rotation.Y + RotationSpeed, Rotation.Z);
            if (Parent != null)
            {
                OrbitAngle += OrbitSpeed;

                Position = new Vector3(
                    Parent.Position.X +
                        (float)Math.Cos(OrbitAngle) * OrbitRadius,

                    Position.Y,

                    Parent.Position.Z +
                        (float)Math.Sin(OrbitAngle) * OrbitRadius
                );
            }
        }

        public void SetParent(Models parent)
        {
            Parent = parent;

            float x = Position.X - Parent.Position.X;
            float z = Position.Z - Parent.Position.Z;

            OrbitRadius = (float)Math.Sqrt(x * x + z * z);

            OrbitAngle = (float)Math.Atan2(z, x);
        }
    }
}

