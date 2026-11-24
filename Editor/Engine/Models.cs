
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Editor.Engine
{
    class Models : Iserializable, INotifyPropertyChanged,
                   ISelectable, IRenderable
    {
        [Browsable(false)]
        public Model Mesh { get; set; }

        public Material Material { get; private set; }

        [Browsable(false)]
        public Texture Texture { get; set; }

        [Browsable(false)]
        public Effect Shader { get; set; }

        private Vector3 m_position;
        private Vector3 m_rotation;
        private float m_scale;
        private bool m_selected;

        private ContentManager m_content;
        private GameEditor m_game;

        public static bool SelectedDirty { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public Models() { }

        public Models(
            GameEditor game,
            string _model,
            string _texture,
            string _effect,
            Vector3 _position,
            float _scale)
        {
            Create(game, _model, _texture,
                   _effect, _position, _scale);
        }

        public void Create(
            GameEditor game,
            string _model,
            string _texture,
            string _effect,
            Vector3 _position,
            float _scale)
        {
            // Store GameEditor for PropertyGrid texture changes.
            m_game = game;

            // Use project assets when available.
            // Otherwise use the editor's default content.
            m_content = game.ProjectContent ?? game.Content;

            Mesh = m_content.Load<Model>(_model);
            Mesh.Tag = _model;

            Material = new Material();

            SetTexture(game, _texture);
            SetShader(game, _effect);

            Position = _position;
            Scale = _scale;
        }

        // PROPERTY GRID - TRANSFORMATIONS

        [Category("Transformation")]
        [Description("Position of the model in world space.")]
        public Vector3 Position
        {
            get => m_position;
            set
            {
                if (m_position != value)
                {
                    m_position = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Transformation")]
        [Description("Rotation of the model.")]
        public Vector3 Rotation
        {
            get => m_rotation;
            set
            {
                if (m_rotation != value)
                {
                    m_rotation = value;
                    OnPropertyChanged();
                }
            }
        }

        [Category("Transformation")]
        [Description("Scale of the model.")]
        public float Scale
        {
            get => m_scale;
            set
            {
                if (m_scale != value)
                {
                    m_scale = value;
                    OnPropertyChanged();
                }
            }
        }

        // PROPERTY GRID - APPEARANCE

        [Category("Appearance")]
        [Description("Diffuse texture of the model.")]
        [TypeConverter(typeof(DiffuseTextureConverter))]
        public string DiffuseTexture
        {
            get => Material?.Diffuse?.Tag?.ToString() ?? "";

            set
            {
                if (m_content == null ||
                    m_game == null ||
                    DiffuseTexture == value)
                    return;

                SetTexture(m_game, value);
            }
        }

        // PROPERTY GRID - SELECTION

        [Category("State")]
        [Description("Selection status.")]
        public bool Selected
        {
            get { return m_selected; }
            set
            {
                if (m_selected != value)
                {
                    m_selected = value;
                    SelectedDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        // TEXTURE

        public void SetTexture(
            GameEditor game,
            string texture)
        {
            m_game = game;
            m_content = game.ProjectContent ?? game.Content;

            if (texture == "DefaultTexture")
            {
                Material.Diffuse = game.DefaultTexture;
            }
            else
            {
                Material.Diffuse =
                    m_content.Load<Texture>(texture);
            }

            Material.Diffuse.Tag = texture;

            Texture = Material.Diffuse;

            OnPropertyChanged(nameof(DiffuseTexture));
        }

        // SHADER

        public void SetShader(Effect effect)
        {
            Material.Effect = effect;
            Shader = effect;

            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Material.Effect;
                }
            }
        }

        public void SetShader(
            GameEditor game,
            string effect)
        {
            m_game = game;
            m_content = game.ProjectContent ?? game.Content;

            if (effect == "DefaultEffect")
            {
                Material.Effect = game.DefaultEffect;
            }
            else
            {
                Material.Effect =
                    m_content.Load<Effect>(effect);
            }

            Material.Effect.Tag = effect;

            SetShader(Material.Effect);
        }

        // TRANSLATION

        public void Translate(
            Vector3 _translate,
            Camera _camera)
        {
            float distance = Vector3.Distance(
                _camera.Target,
                _camera.Position
            );

            Vector3 forward =
                _camera.Target - _camera.Position;

            forward.Normalize();

            Vector3 left =
                Vector3.Cross(forward, Vector3.Up);

            left.Normalize();

            Vector3 up =
                Vector3.Cross(left, forward);

            up.Normalize();

            Position += left * _translate.X * distance;
            Position += up * _translate.Y * distance;
            Position += forward * _translate.Z * 100f;
        }

        // ROTATION

        public void Rotate(Vector3 _rotate)
        {
            Rotation += _rotate;
        }

        // TRANSFORMATION MATRIX

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(
                       Rotation.Y,
                       Rotation.X,
                       Rotation.Z
                   ) *
                   Matrix.CreateTranslation(Position);
        }

        // RENDERING

        public void Render()
        {
            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Material.Effect;
                }

                mesh.Draw();
            }
        }

        // SERIALIZATION

        public void Serialize(BinaryWriter _stream)
        {
            _stream.Write(Mesh.Tag.ToString());
            _stream.Write(Material.Diffuse.Tag.ToString());
            _stream.Write(Material.Effect.Tag.ToString());

            HelpSerialize.Vec3(_stream, Position);
            HelpSerialize.Vec3(_stream, Rotation);

            _stream.Write(Scale);
        }

        // DESERIALIZATION

        public void Deserialize(
            BinaryReader _stream,
            GameEditor game)
        {
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();

            Vector3 position =
                HelpDeserialize.Vec3(_stream);

            Vector3 rotation =
                HelpDeserialize.Vec3(_stream);

            float scale = _stream.ReadSingle();

            Create(
                game,
                mesh,
                texture,
                shader,
                position,
                scale
            );

            Rotation = rotation;
        }

        // PROPERTY CHANGED EVENT

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}
