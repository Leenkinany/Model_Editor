using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Editor.Engine
{
    class Models : Iserializable
    {
        public Model Mesh { get; set; }
        public Texture Texture { get; set; }
        public Effect Shader { get; set; }
        public Vector3 Position { get=> m_position; set{ m_position = value; } }
        public Vector3 Rotation { get=>m_rotation; set{ m_rotation = value; } }
        public float Scale { get; set; }
        public bool Selected { get; set; } = false;

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

        public void Translate(Vector3 _translate, Camera _camera)
        {
            float distance = Vector3.Distance(_camera.Target, _camera.Position);
            Vector3 forward = _camera.Target - _camera.Position;
            forward.Normalize();
            Vector3 left = Vector3.Cross(forward, Vector3.Up);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, forward);
            up.Normalize();
            Position += left * _translate.X * distance;
            Position += up * _translate.Y * distance;
            Position += forward * _translate.Z * 100f;
        }

        public void Rotate (Vector3 _rotate)
        {
            Rotation += _rotate;
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
            Shader.Parameters["Tint"].SetValue(Selected);

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
        }

        public void Deserialize(BinaryReader _stream, ContentManager _content)
        {
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();
            Position = HelperDeserialize.Vec3(_stream);
            Rotation = HelperDeserialize.Vec3(_stream);
            Scale = _stream.ReadSingle();

            Create(_content, mesh, texture, shader, Position, Scale);
        }

    }
}

