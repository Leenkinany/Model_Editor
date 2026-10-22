using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using Editor.Engine;
using GUI.Editor;

namespace Editor
{
    public class GameEditor : Game
    {
        private SpriteBatch m_spriteBatch;
        private FontController m_fonts; 
        private GraphicsDeviceManager _graphics;
        private FormEditor m_parent;
        private Models m_observedModel;
        internal Project Project { get; set; }
        RasterizerState m_rasterState = new RasterizerState();
        DepthStencilState m_depthStencilState = new DepthStencilState();


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
            Form gameForm = Control.FromHandle(Window.Handle) as Form;
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
            m_spriteBatch = new(GraphicsDevice);
            m_fonts = new();
            m_fonts.LoadContent(Content);
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
                    Models selectedModel = level.SelectedModel;

                    // Subscribe to the selected model
                    ObserveModel(selectedModel);

                    if (m_parent != null && !m_parent.IsDisposed)
                    {
                        if (m_parent.propertyGrid.InvokeRequired)
                        {
                            m_parent.propertyGrid.BeginInvoke(
                                new System.Action(() =>
                                {
                                    if (!m_parent.propertyGrid.IsDisposed)
                                    {
                                        m_parent.propertyGrid.SelectedObject =
                                            selectedModel;
                                    }
                                }));
                        }
                        else
                        {
                            m_parent.propertyGrid.SelectedObject =
                                selectedModel;
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
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.RasterizerState = m_rasterState;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                Project.Render();

                m_spriteBatch.Begin();
                m_fonts.Draw(
                    m_spriteBatch, 20,
                    InputController.Instance.ToString(),
                    new Vector2(20, 20),
                    Color.White
                );
                m_fonts.Draw(
                    m_spriteBatch, 16,
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
            if (Project == null) return;
            Camera c = Project.CurrentLevel.GetCamera();
            c.viewport = _graphics.GraphicsDevice.Viewport;
            c.Update(c.Position, _graphics.GraphicsDevice.Viewport.AspectRatio);
        }
        private void ObserveModel(Models model)
        {
            if (m_observedModel != null)
                m_observedModel.PropertyChanged -= Model_PropertyChanged;

            m_observedModel = model;

            if (m_observedModel != null)
                m_observedModel.PropertyChanged += Model_PropertyChanged;
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
                grid.BeginInvoke(new System.Action(() =>
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