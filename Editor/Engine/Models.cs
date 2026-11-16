using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Editor.Engine
{
    class Models : Iserializable, IRenderable, ISelectable, IMaterial
    {
        [Browsable(false)]
        public Model Mesh { get; set; }

        [Browsable(false)]
        public Texture Texture { get; set; }

        [Browsable(false)]
        public Effect Shader { get; set; }

        [Category("Appearance")]
        [Description("Diffuse texture of the model.")]
        [TypeConverter(typeof(DiffuseTextureConverter))]

        public string DiffuseTexture
        {
            get => Texture?.Tag?.ToString() ?? "";

            set
            {
                if (m_content == null || DiffuseTexture == value)
                    return;

                Texture = m_content.Load<Texture>(value);
                Texture.Tag = value;
                if (Material != null)
                    Material.Diffuse = Texture;

                OnPropertyChanged();
            }
        }


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
                }
            }
        }
        public static bool SelectedDirty { get; set; } = false;
        private Vector3 m_position;
        private Vector3 m_rotation;
        private bool m_selected;
        private float m_scale;
        private ContentManager m_content;

        [Browsable(false)]
        public bool IsImported { get; set; } = false;
        public Material Material { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

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

        public Models(
            ContentManager content,
            string model,
            string texture,
            string effect,
            Vector3 position,
            float scale)
        {
            m_content = content;

            Mesh = content.Load<Model>(model);
            Mesh.Tag = model;

            Texture = content.Load<Texture>(texture);
            Texture.Tag = texture;

            Shader = content.Load<Effect>(
                effect == "DefaultEffect" ? "DefaultShader" : effect);
            Shader.Tag = effect;
            Material = new Material
            {
                Diffuse = Texture,
                Effect = Shader
            };
           
            SetShader(Shader);

            Position = position;
            Scale = scale;
        }

        public Models(
            ContentManager projectContent,
            ContentManager editorContent,
            string modelName,
            string textureName,
            string effectName,
            Vector3 position,
            float scale)
        {
            // Use the project's content manager for imported assets.
            m_content = projectContent;

            Mesh = projectContent.Load<Model>(modelName);
            Mesh.Tag = modelName;

            if (textureName == "DefaultTexture")
            {
                Texture = editorContent.Load<Texture>("DefaultTexture");
                m_content = editorContent;
            }
            else
            {
                Texture = projectContent.Load<Texture>(textureName);
                m_content = projectContent;
            }
            Texture.Tag = textureName;

            Shader = editorContent.Load<Effect>(
                effectName == "DefaultEffect"
                    ? "DefaultShader"
                    : effectName
            );
            Shader.Tag = effectName;

            Material = new Material
            {
                Diffuse = Texture,
                Effect = Shader
            };
            SetShader(Shader);

            Position = position;
            Scale = scale;

            IsImported = true;
        }

        public Models(
            GameEditor game,
            string modelName,
            Vector3 position,
            float scale)
        {
            m_content = game.Content;

            Mesh = game.ProjectContent.Load<Model>(modelName);
            Mesh.Tag = modelName;

            Texture = game.DefaultTexture;
            Texture.Tag = "DefaultTexture";

            Shader = game.DefaultEffect;
            Shader.Tag = "DefaultEffect";

            SetShader(Shader);

            Position = position;
            Scale = scale;
            IsImported = true;
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
            Material ??= new Material();

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

        public void SetTexture(GameEditor game, string texture)
        {
            Material ??= new Material();

            if (texture == "DefaultTexture")
            {
                Material.Diffuse = game.DefaultTexture;
            }
            else
            {
                Material.Diffuse = game.Content.Load<Texture>(texture);
            }

            Material.Diffuse.Tag = texture;
            Texture = Material.Diffuse;
        }

        public void SetShader(GameEditor game, string effect)
        {
            Material ??= new Material();

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

        public void Rotate(Vector3 _rotate)
        {
            Rotation += _rotate;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
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

            // Keep this for your existing version 2 project files.
            _stream.Write(IsImported);
        }

        public void Deserialize(BinaryReader stream, ContentManager content)
        {
            Deserialize(stream, content, null, false);
        }

        public void Deserialize(BinaryReader _stream, GameEditor _content)
        {
            string mesh = _stream.ReadString();
            string texture = _stream.ReadString();
            string shader = _stream.ReadString();
            Position = HelpDeserialize.Vec3(_stream);
            Rotation = HelpDeserialize.Vec3(_stream);
            Scale = _stream.ReadSingle();

            Create(_content, mesh, texture, shader, Position, Scale);
        }


        public void Deserialize(
            BinaryReader stream,
            ContentManager editorContent,
            ContentManager projectContent,
            bool hasImportFlag)
        {
            string mesh = stream.ReadString();
            string texture = stream.ReadString();
            string shader = stream.ReadString();

            Vector3 position = HelpDeserialize.Vec3(stream);
            Vector3 rotation = HelpDeserialize.Vec3(stream);
            float scale = stream.ReadSingle();

            bool isImported = hasImportFlag && stream.ReadBoolean();

            if (isImported)
            {
                if (projectContent == null)
                    throw new InvalidDataException(
                        "Project content manager is not initialized.");

                Models imported = new Models(
                    projectContent,
                    editorContent,
                    mesh,
                    texture,
                    shader,
                    position,
                    scale
                );

                Mesh = imported.Mesh;
                Texture = imported.Texture;
                Shader = imported.Shader;
                m_content = imported.m_content;
                Material = imported.Material;
                IsImported = true;
            }
            else
            {
                m_content = editorContent;

                Mesh = editorContent.Load<Model>(mesh);
                Mesh.Tag = mesh;

                Texture = editorContent.Load<Texture>(texture);
                Texture.Tag = texture;

                Shader = editorContent.Load<Effect>(
                    shader == "DefaultEffect" ? "DefaultShader" : shader);
                Shader.Tag = shader;
                Material = new Material
                {
                    Diffuse = Texture,
                    Effect = Shader
                };
                SetShader(Shader);
                IsImported = false;
            }

            Position = position;
            Rotation = rotation;
            Scale = scale;
        }


        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

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

    }
}

    