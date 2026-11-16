using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Editor.Engine
{
    class Models : Iserializable, IRenderable, ISelectable
    {
        [Browsable(false)]
        public Model Mesh { get; set; }
        public Material Material { get; private set;}
        public  Vector3 Position { get=> m_position; set{ m_position = value; } }
        public Vector3 Rotation { get => m_rotation; set { m_rotation = value; } }
        public float Scale { get; set; }

        public bool Selected
        {
            get { return m_selected; }
            set
            {
                if (m_selected != value)
                {
                    m_selected = value;
                    SelectedDirty = true;
                }
            }
        }
        public static bool SelectedDirty { get; set; } = false;
        private Vector3 m_position;
        private Vector3 m_rotation;
        private bool m_selected;
        private float m_scale;

        public Models() { }

        public Models(GameEditor content,
            string _model,
            string _texture,
            string _effect,
            Vector3 _position,
            float _scale)
        {
            Create(content, _model, _texture, _effect, _position, _scale);
        }

        public void Create(
            GameEditor game,
            string model,
            string texture,
            string effect,
            Vector3 position,
            float scale)
        {
            Mesh = game.Content.Load<Model>(model);
            Mesh.Tag = model;
            Material = new Material();
            SetTexture(game, texture);
            SetShader(game, effect);
            Position = position;
            Scale = scale;
        }


        public void SetShader(Effect effect)
        {
            Material.Effect = effect;

            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Material.Effect;
                }
            }
        }

        public void SetTexture(GameEditor game, string texture)
        {
            if (texture == "DefaultTexture")
            {
                Material.Diffuse = game.DefaultTexture;
            }
            else
            {
                Material.Diffuse = game.Content.Load<Texture>(texture);
            }

            Material.Diffuse.Tag = texture;
        }

        public void SetShader(GameEditor game, string effect)
        {
            if (effect == "DefaultEffect")
            {
                Material.Effect = game.DefaultEffect;
            }
            else
            {
                Material.Effect = game.Content.Load<Effect>(effect);
            }

            Material.Effect.Tag = effect;
            SetShader(Material.Effect);
        }

        public void Translate(Vector3 _translate, Camera _camera)
        {
            float distance = Vector3.Distance(
                _camera.Target, _camera.Position);

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

        public void Rotate(Vector3 _rotate)
        {
            Rotation += _rotate;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(
                       Rotation.Y, Rotation.X, Rotation.Z) *
                   Matrix.CreateTranslation(Position);
        }

        public void Render()
        {
            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                mesh.Draw();
            }
        }

        public void Serialize(BinaryWriter _stream)
        {
            _stream.Write(Mesh.Tag.ToString());
            _stream.Write(Material.Diffuse.Tag.ToString());
            _stream.Write(Material.Effect.Tag.ToString());

            HelpSerialize.Vec3(_stream, Position);
            HelpSerialize.Vec3(_stream, Rotation);
            _stream.Write(Scale);
        }

        public void Deserialize(
            BinaryReader _stream,
            GameEditor _content)
        {
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();

            Vector3 position = HelpDeserialize.Vec3(_stream);
            Vector3 rotation = HelpDeserialize.Vec3(_stream);
            float scale = _stream.ReadSingle();

            Create(_content, mesh, texture, shader, position, scale);
            Rotation = rotation;
        }
    }
}
