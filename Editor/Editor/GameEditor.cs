using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using Editor.Engine;
using GUI.Editor;

namespace Editor
{
    public class GameEditor : Game
    {
        private GraphicsDeviceManager _graphics;
        private FormEditor m_parent;
        internal Project Project { get; set; }

        public GameEditor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = state;

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Project != null)
                Project.CurrentLevel.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (Project != null) Project.Render();

            base.Draw(gameTime);
        }

        public void AdjustAspectRatio()
        {
            if (Project == null) return;
            Camera c = Project.CurrentLevel.GetCamera();
            c.Update(c.Position, _graphics.GraphicsDevice.Viewport.AspectRatio);
        }
    }
}
