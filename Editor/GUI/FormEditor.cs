
using Editor;
using Editor.Engine;
using Editor.Engine.Interfaces;
using Editor.GUI;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AssetTypes = Editor.Editor.AssetTypes;

namespace GUI.Editor
{
    public partial class FormEditor : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GameEditor Game
        {
            get => m_game;
            set
            {
                m_game = value;
                if (m_game != null)
                    HookEvents();
            }
        }

        private GameEditor m_game;
        private IMaterial m_dropped;
        private Process m_MGCBProcess;

        public FormEditor()
        {
            InitializeComponent();
            KeyPreview = true;
            toolStripStatusLabel1.Text = Directory.GetCurrentDirectory();
            listBoxAssets.MouseDown += ListBoxAssets_MouseDown;
        }

        private void HookEvents()
        {
            Form gameForm =
                Control.FromHandle(m_game.Window.Handle) as Form;

            if (gameForm == null)
                return;

            gameForm.MouseDown += GameForm_MouseDown;
            gameForm.MouseUp += GameForm_MouseUp;
            gameForm.MouseWheel += GameForm_MouseWheel;
            gameForm.MouseMove += GameForm_MouseMove;

            gameForm.AllowDrop = true;
            gameForm.DragOver += GameForm_DragOver;
            gameForm.DragDrop += GameForm_DragDrop;

            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;
        }

        private void ListBoxAssets_MouseDown(
            object sender, MouseEventArgs e)
        {
            int index = listBoxAssets.IndexFromPoint(e.Location);

            if (index < 0)
                return;

            ListItemAsset asset =
                listBoxAssets.Items[index] as ListItemAsset;

            if (asset == null)
                return;

            if (asset.Type == AssetTypes.MODEL ||
                asset.Type == AssetTypes.TEXTURE ||
                asset.Type == AssetTypes.EFFECT)
            {
                listBoxAssets.DoDragDrop(
                    asset, DragDropEffects.Copy);
            }
        }

        private void GameForm_DragOver(
            object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            m_dropped = null;

            if (!e.Data.GetDataPresent(typeof(ListItemAsset)))
                return;

            ListItemAsset asset =
                e.Data.GetData(typeof(ListItemAsset))
                as ListItemAsset;

            if (asset == null ||
                Game?.Project?.CurrentLevel == null)
                return;

            if (asset.Type == AssetTypes.MODEL)
            {
                e.Effect = DragDropEffects.Copy;
                return;
            }

            if (asset.Type == AssetTypes.TEXTURE ||
                asset.Type == AssetTypes.EFFECT)
            {
                if (sender is not Form gameForm)
                    return;

                System.Drawing.Point mouse =
                    gameForm.PointToClient(
                        new System.Drawing.Point(e.X, e.Y));

                InputController.Instance.MousePosition =
                    new Vector2(mouse.X, mouse.Y);

                ISelectable selected =
                    Game.Project.CurrentLevel.HandlePick(false);

                if (selected is IMaterial material)
                {
                    m_dropped = material;
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void GameForm_DragDrop(
            object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ListItemAsset)))
                return;

            ListItemAsset asset =
                e.Data.GetData(typeof(ListItemAsset))
                as ListItemAsset;

            if (asset == null ||
                Game?.Project?.CurrentLevel == null)
                return;

            try
            {
                if (asset.Type == AssetTypes.MODEL)
                {
                    AddAssetModel(asset.Name);
                }
                else if (asset.Type == AssetTypes.TEXTURE)
                {
                    m_dropped?.SetTexture(Game, asset.Name);
                }
                else if (asset.Type == AssetTypes.EFFECT)
                {
                    m_dropped?.SetShader(Game, asset.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not apply asset:\n\n" + ex.Message);
            }
            finally
            {
                m_dropped = null;
            }
        }

        private void AddAssetModel(string modelName)
        {
            if (Game?.Project?.CurrentLevel == null)
                return;

            Models model = new Models(
                Game,
                modelName,
                "DefaultTexture",
                "DefaultEffect",
                Vector3.Zero,
                1.0f
            );

            Game.Project.CurrentLevel.AddModel(model);
        }

        private void GameForm_MouseMove(
            object sender, MouseEventArgs e)
        {
            InputController.Instance.MousePosition =
                new Vector2(e.X, e.Y);
        }

        private void GameForm_MouseWheel(
            object sender, MouseEventArgs e)
        {
            InputController.Instance.SetWheel(
                e.Delta /
                SystemInformation.MouseWheelScrollDelta);
        }

        private void GameForm_MouseDown(
            object sender, MouseEventArgs e)
        {
            InputController.Instance.SetButtonDown(e.Button);

            InputController.Instance.DragStart =
                new Vector2(e.X, e.Y);
        }

        private void GameForm_MouseUp(
            object sender, MouseEventArgs e)
        {
            InputController.Instance.SetButtonUp(e.Button);

            InputController.Instance.DragEnd =
                new Vector2(e.X, e.Y);
        }

        private void GameForm_KeyDown(
            object sender, KeyEventArgs e)
        {
            InputController.Instance.SetKeyDown(e.KeyCode);
            e.Handled = true;
        }

        private void GameForm_KeyUp(
            object sender, KeyEventArgs e)
        {
            InputController.Instance.SetKeyUp(e.KeyCode);
            e.Handled = true;
        }

        private void exitToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            Game?.Exit();
        }

        private void exitToolStripMenuItem1_Click(
            object sender, EventArgs e)
        {
            Game?.Exit();
        }

        private void FormEditor_SizeChanged(
            object sender, EventArgs e)
        {
            Game?.AdjustAspectRatio();
        }

        private void splitContainer_Panel1_SizeChanged(
            object sender, EventArgs e)
        {
            Game?.AdjustAspectRatio();
        }

        private void createToolStripMenuItem1_Click_1(
            object sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "OCE Files|*.oce",
                DefaultExt = "oce",
                AddExtension = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (Game?.Project != null)
                Game.Project.OnAssetsUpdated -= Project_OnAssetsUpdated;

            Game.Project = new Project(Game, dialog.FileName);
            Game.Project.OnAssetsUpdated += Project_OnAssetsUpdated;
            Game.Project.AssetMonitor.UpdateAssetsDB();

            Text = "Our Cool Editor - " + Game.Project.Name;

            Game.AdjustAspectRatio();
            RefreshAssetList();

            saveToolStripMenuItem1_Click_1(sender, e);
        }

        private void saveToolStripMenuItem1_Click_1(
            object sender, EventArgs e)
        {
            if (Game?.Project == null)
            {
                MessageBox.Show(
                    "Please create or load a project first.");
                return;
            }

            string fileName = Path.Combine(
                Game.Project.Folder,
                Game.Project.Name);

            using var stream =
                File.Open(fileName, FileMode.Create);

            using var writer =
                new BinaryWriter(stream, Encoding.UTF8, false);

            Game.Project.Serialize(writer);
        }

        private void loadToolStripMenuItem1_Click_1(
            object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "OCE Files|*.oce"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            using var stream =
                File.Open(dialog.FileName, FileMode.Open);

            using var reader =
                new BinaryReader(stream, Encoding.UTF8, false);

            if (Game?.Project != null)
                Game.Project.OnAssetsUpdated -= Project_OnAssetsUpdated;

            Game.Project = new Project();
            Game.Project.Deserialize(reader, Game);

            Game.Project.OnAssetsUpdated += Project_OnAssetsUpdated;
            Game.Project.AssetMonitor.UpdateAssetsDB();

            Text = "Our Cool Editor - " + Game.Project.Name;

            Game.AdjustAspectRatio();
            RefreshAssetList();
        }

        private void Project_OnAssetsUpdated()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
                BeginInvoke(new Action(RefreshAssetList));
            else
                RefreshAssetList();
        }

        private void RefreshAssetList()
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshAssetList));
                return;
            }

            listBoxAssets.Items.Clear();

            if (Game?.Project?.AssetMonitor == null)
                return;

            var assets = Game.Project.AssetMonitor.Assets;

            foreach (AssetTypes type in
                     Enum.GetValues(typeof(AssetTypes)))
            {
                if (!assets.ContainsKey(type))
                    continue;

                listBoxAssets.Items.Add(new ListItemAsset
                {
                    Name = type.ToString().ToUpper() + "S:",
                    Type = AssetTypes.NONE
                });

                foreach (string name in assets[type])
                {
                    listBoxAssets.Items.Add(new ListItemAsset
                    {
                        Name = name,
                        Type = type
                    });
                }

                listBoxAssets.Items.Add(new ListItemAsset
                {
                    Name = " ",
                    Type = AssetTypes.NONE
                });
            }
        }

        private void importToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (Game?.Project == null)
            {
                MessageBox.Show(
                    "Please create or load a project first.");
                return;
            }

            string mgcbEditorPath =
                ConfigurationManager.AppSettings["MGCB_EditorPath"];

            if (string.IsNullOrWhiteSpace(mgcbEditorPath))
            {
                MessageBox.Show(
                    "MGCB_EditorPath is not configured in App.config.");
                return;
            }

            string executablePath = Path.Combine(
                mgcbEditorPath,
                "mgcb-editor-windows.exe");

            string contentPath = Path.Combine(
                Game.Project.ContentFolder,
                "Content.mgcb");

            if (!File.Exists(executablePath) ||
                !File.Exists(contentPath))
            {
                MessageBox.Show(
                    "MGCB editor or Content.mgcb not found.\n" +
                    executablePath + "\n" + contentPath);
                return;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = executablePath,
                Arguments = "\"" + contentPath + "\"",
                WorkingDirectory = Game.Project.ContentFolder,
                UseShellExecute = true
            };

            m_MGCBProcess = Process.Start(startInfo);

            if (m_MGCBProcess == null)
                return;

            m_MGCBProcess.EnableRaisingEvents = true;

            m_MGCBProcess.Exited += (s, args) =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (IsDisposed ||
                            Game?.Project?.AssetMonitor == null)
                            return;

                        Game.Project.AssetMonitor.UpdateAssetsDB();
                        RefreshAssetList();
                    }));
                }
            };
        }

        private void listBoxAssets_DoubleClick(
            object sender, EventArgs e)
        {
            if (Game?.Project?.CurrentLevel == null)
                return;

            ListItemAsset asset =
                listBoxAssets.SelectedItem as ListItemAsset;

            if (asset == null ||
                asset.Type != AssetTypes.MODEL)
                return;

            try
            {
                AddAssetModel(asset.Name);

                MessageBox.Show(
                    asset.Name + " added to the scene!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not add this model:\n\n" +
                    ex.Message);
            }
        }

        private void FormEditor_FormClosing(
            object sender, FormClosingEventArgs e)
        {
            if (m_MGCBProcess == null)
                return;

            if (!m_MGCBProcess.HasExited)
                m_MGCBProcess.Kill();

            m_MGCBProcess.Dispose();
        }

        // Keep these handlers if used by FormEditor.Designer.cs.

        private void FormEditor_Load(
            object sender, EventArgs e)
        {
        }

        private void createToolStripMenuItem1_Click(
            object sender, EventArgs e)
        {
        }

        private void saveToolStripMenuItem1_Click(
            object sender, EventArgs e)
        {
        }

        private void loadToolStripMenuItem1_Click(
            object sender, EventArgs e)
        {
        }

        private void propertyGrid_Click(
            object sender, EventArgs e)
        {
        }

        private void splitContainer_Panel1_Paint(
            object sender, PaintEventArgs e)
        {
        }

        private void listBoxAssets_SelectedIndexChanged(
            object sender, EventArgs e)
        {
        }
    }
}
