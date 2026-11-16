
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Windows.Forms;
using Editor.Engine;
using GUI.Editor;

namespace Editor
{
    public class GameEditor : Game
    {
        internal Project Project { get; set; }
        internal Texture DefaultTexture { get; set; }
        internal Effect DefaultEffect { get; set; }
        internal ContentManager ProjectContent { get; private set; }
        internal Texture2D DefaultGrass { get; set; }
        internal Texture2D DefaultHeightMap { get; set; }

        private SpriteBatch m_spriteBatch;
        private FontController m_fonts;
        private GraphicsDeviceManager _graphics;
        private FormEditor m_parent;

        private RasterizerState m_rasterState;

        public GameEditor()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            m_rasterState = new RasterizerState
            {
                CullMode = CullMode.None
            };
        }

        public GameEditor(FormEditor parent) : this()
        {
            m_parent = parent;

            Form gameForm =
                Control.FromHandle(Window.Handle) as Form;

            gameForm.TopLevel = false;
            gameForm.Dock = DockStyle.Fill;
            gameForm.FormBorderStyle = FormBorderStyle.None;

            m_parent.splitContainer.Panel1.Controls.Add(gameForm);
        }

        protected override void LoadContent()
        {
            m_spriteBatch = new SpriteBatch(GraphicsDevice);

            m_fonts = new FontController();
            m_fonts.LoadContent(Content);

            DefaultTexture =
                Content.Load<Texture>("DefaultTexture");

            DefaultGrass =
                Content.Load<Texture2D>("DefaultGrass");

            DefaultHeightMap =
                Content.Load<Texture2D>("DefaultHeightMap");

            DefaultEffect =
                Content.Load<Effect>("MyShader");
        }

        public void InitializeProjectContent()
        {
            ProjectContent?.Unload();
            ProjectContent?.Dispose();

            if (Project == null)
            {
                ProjectContent = null;
                return;
            }

            ProjectContent = new ContentManager(
                Services,
                Path.Combine(Project.Folder, "bin")
            );
        }

        protected override void Update(GameTime gameTime)
        {
            if (Project != null)
            {
                Project.Update(
                    (float)gameTime.ElapsedGameTime.TotalSeconds);

                InputController.Instance.Clear();

                Level level = Project.CurrentLevel;

                if (level.SelectionDirty)
                {
                    object selectedObject =
                        level.SelectedObject;

                    if (m_parent != null &&
                        !m_parent.IsDisposed)
                    {
                        var grid = m_parent.propertyGrid;

                        if (grid.InvokeRequired)
                        {
                            grid.BeginInvoke(
                                new System.Action(() =>
                                {
                                    if (!grid.IsDisposed)
                                    {
                                        grid.SelectedObject =
                                            selectedObject;
                                    }
                                }));
                        }
                        else
                        {
                            grid.SelectedObject =
                                selectedObject;
                        }
                    }

                    level.ClearSelectionDirty();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (Project != null)
            {
                GraphicsDevice.DepthStencilState =
                    DepthStencilState.Default;

                GraphicsDevice.RasterizerState =
                    m_rasterState;

                GraphicsDevice.SamplerStates[0] =
                    SamplerState.LinearWrap;

                Project.Render();

                m_spriteBatch.Begin();

                m_fonts.Draw(
                    m_spriteBatch,
                    20,
                    InputController.Instance.ToString(),
                    new Vector2(20, 20),
                    Color.White
                );

                m_fonts.Draw(
                    m_spriteBatch,
                    16,
                    Project.CurrentLevel.ToString(),
                    new Vector2(20, 80),
                    Color.Yellow
                );

                m_spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public void AdjustAspectRatio()
        {
            if (Project == null)
                return;

            Camera camera = Project.CurrentLevel.GetCamera();

            camera.viewport = _graphics.GraphicsDevice.Viewport;

            camera.Update(
                camera.Position,
                _graphics.GraphicsDevice.Viewport.AspectRatio
            );
        }
    }
}
