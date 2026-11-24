
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
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
        internal Texture2D DefaultGrass { get; set; }
        internal Texture2D DefaultHeightMap { get; set; }
        internal Effect DefaultEffect { get; set; }

        internal ContentManager ProjectContent { get; private set; }

        private GraphicsDeviceManager _graphics;
        private FormEditor m_parent;
        private SpriteBatch m_spriteBatch;
        private FontController m_fonts;

        private RasterizerState m_rasterState;
        private DepthStencilState m_depthStencilState;

        public GameEditor()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            m_rasterState = new RasterizerState();
            m_rasterState.CullMode = CullMode.None;

            m_depthStencilState = new DepthStencilState();
            m_depthStencilState.DepthBufferEnable = true;
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

        protected override void Initialize()
        {
            base.Initialize();
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
                Project.AssetFolder
            );
        }

        private void UpdateSelected()
        {
            if (Project?.CurrentLevel == null)
                return;

            Level level = Project.CurrentLevel;

            // Support both old and new selection systems.
            if (!Models.SelectedDirty && !level.SelectionDirty)
                return;

            var models = level.GetSelectedModels();

            object selectedObject = null;
            object[] selectedObjects = null;

            // Preserve multiple-model selection.
            if (models.Count > 1)
            {
                selectedObjects = new object[models.Count];

                for (int i = 0; i < models.Count; i++)
                {
                    selectedObjects[i] = models[i];
                }
            }
            else if (models.Count == 1)
            {
                selectedObject = models[0];
            }
            else
            {
                // Fallback to the newer Level selection.
                selectedObject = level.SelectedObject;
            }

            if (m_parent != null && !m_parent.IsDisposed)
            {
                var grid = m_parent.propertyGrid;

                if (!grid.IsDisposed)
                {
                    Action updateGrid = () =>
                    {
                        if (grid.IsDisposed)
                            return;

                        if (selectedObjects != null)
                        {
                            grid.SelectedObjects = selectedObjects;
                        }
                        else
                        {
                            grid.SelectedObject = selectedObject;
                        }

                        grid.Refresh();
                    };

                    if (grid.InvokeRequired)
                    {
                        grid.BeginInvoke(updateGrid);
                    }
                    else
                    {
                        updateGrid();
                    }
                }
            }

            Models.SelectedDirty = false;
            level.ClearSelectionDirty();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Project != null)
            {
                Project.Update(
                    (float)gameTime.ElapsedGameTime.TotalSeconds);

                // Update the PropertyGrid after level selection.
                UpdateSelected();

                InputController.Instance.Clear();
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
            if (Project?.CurrentLevel == null)
                return;

            Camera camera =
                Project.CurrentLevel.GetCamera();

            camera.viewport =
                _graphics.GraphicsDevice.Viewport;

            camera.Update(
                camera.Position,
                _graphics.GraphicsDevice.Viewport.AspectRatio
            );
        }

        private void Model_PropertyChanged(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (m_parent == null || m_parent.IsDisposed)
                return;

            var grid = m_parent.propertyGrid;

            if (grid.IsDisposed)
                return;

            if (grid.InvokeRequired)
            {
                grid.BeginInvoke(new Action(() =>
                {
                    if (!grid.IsDisposed)
                        grid.Refresh();
                }));
            }
            else
            {
                grid.Refresh();
            }
        }
    }
}
