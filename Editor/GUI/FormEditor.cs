using System;
using Editor;
using Editor.Engine;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace GUI.Editor
{
    public partial class FormEditor : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GameEditor Game { get => m_game; set { m_game = value; HookEvents(); } }
        private GameEditor m_game = null;

        public FormEditor()
        {
            InitializeComponent();
            KeyPreview = true;
        }

        private void HookEvents()
        {
            Form gameForm = Control.FromHandle(m_game.Window.Handle) as Form;
            gameForm.MouseDown += GameForm_MouseDown;
            gameForm.MouseUp += GameForm_MouseUp;
            gameForm.MouseWheel += GameForm_MouseWheel;
            gameForm.MouseMove += GameForm_MouseMove;
            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;
        }
        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            var p = new Vector2(e.Location.X, e.Location.Y);
            InputController.Instance.MousePosition = p;
        }
        private void GameForm_MouseWheel(object sender, MouseEventArgs e)
        {
            InputController.Instance.SetWheel(e.Delta / SystemInformation.MouseWheelScrollDelta);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.Exit();
        }
        private void FormEditor_SizeChanged(object sender, EventArgs e)
        {
            if (Game == null) return;
            Game.AdjustAspectRatio();
        }

        private void splitContainer_Panel1_SizeChanged(object sender, EventArgs e)
        {
            if (Game == null) return;
            Game.AdjustAspectRatio();
        }

        private void FormEditor_Load(object sender, EventArgs e)
        {

        }


        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            InputController.Instance.SetButtonDown(e.Button);
            var p = new Vector2(e.Location.X, e.Location.Y);
            InputController.Instance.DragStart = p;
        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            InputController.Instance.SetButtonUp(e.Button);
            var p = new Vector2(e.Location.X, e.Location.Y);
            InputController.Instance.DragEnd = p;
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            InputController.Instance.SetKeyDown(e.KeyCode);
            e.Handled = true;
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            InputController.Instance.SetKeyUp(e.KeyCode);
            e.Handled = true;
        }

        private void createToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Game.Exit();
        }

        private void createToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Game.Project = new(Game.Content, sfd.FileName);
                Text = "Our Cool Editor - " + Game.Project.Name;
                Game.AdjustAspectRatio();
            }
            saveToolStripMenuItem1_Click_1(sender, e);
        }

        private void saveToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            string fname = Path.Combine(Game.Project.Folder, Game.Project.Name);
            using var stream = File.Open(fname, FileMode.Create);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
            Game.Project.Serialize(writer);
        }

        private void loadToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Filter = "OCE Files|*.oce";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using var stream = File.Open(ofd.FileName, FileMode.Open);
                using var reader = new BinaryReader(stream, Encoding.UTF8, false);
                Game.Project = new();
                Game.Project.Deserialize(reader, Game.Content);
                Text = "Our Cool Editor - " + Game.Project.Name;
                Game.AdjustAspectRatio();
            }
        }

        private void propertyGrid_Click(object sender, EventArgs e)
        {

        }
    }
}
