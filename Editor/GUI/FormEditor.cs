using Editor;
using Editor.Engine;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;



namespace GUI.Editor
{
    public partial class FormEditor : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GameEditor Game { get => m_game; set { m_game = value; HookEvents(); } }
        private GameEditor m_game = null;
        private IMaterial m_dropped = null;
        private Process m_MGCBProcess = null;

        public FormEditor()
        {
            InitializeComponent();
            KeyPreview = true;
            listBoxAssets.MouseDown += ListBoxAssets_MouseDown;
        }

        private void ListBoxAssets_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBoxAssets.Items.Count == 0)
                return;

            int index = listBoxAssets.IndexFromPoint(e.X, e.Y);

            if (index < 0)
                return;

            var lia = listBoxAssets.Items[index] as ListItemAsset;

            if (lia == null)
                return;

            if (lia.Type == AssetTypes.MODEL ||
                lia.Type == AssetTypes.TEXTURE ||
                lia.Type == AssetTypes.EFFECT)
            {
                listBoxAssets.DoDragDrop(lia, DragDropEffects.Copy);
            }
        }


        private void HookEvents()
        {
            Form gameForm = Control.FromHandle(m_game.Window.Handle) as Form;
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

        private void GameForm_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            m_dropped = null;

            if (!e.Data.GetDataPresent(typeof(ListItemAsset)))
                return;

            ListItemAsset lia =
                e.Data.GetData(typeof(ListItemAsset)) as ListItemAsset;

            if (lia == null)
                return;

            if (lia.Type == AssetTypes.MODEL)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (lia.Type == AssetTypes.TEXTURE ||
                     lia.Type == AssetTypes.EFFECT)
            {
                if (m_game?.Project?.CurrentLevel == null)
                    return;

                // DragEventArgs uses screen coordinates.
                Form gameForm = sender as Form;
                if (gameForm == null)
                    return;

                System.Drawing.Point mouse =
                    gameForm.PointToClient(
                        new System.Drawing.Point(e.X, e.Y));

                InputController.Instance.MousePosition =
                    new Vector2(mouse.X, mouse.Y);

                ISelectable obj =
                    m_game.Project.CurrentLevel.HandlePick(false);

                if (obj is IMaterial)
                {
                    m_dropped = (IMaterial)obj;
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void GameForm_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ListItemAsset)))
                return;

            ListItemAsset lia =
                e.Data.GetData(typeof(ListItemAsset)) as ListItemAsset;

            if (lia == null)
                return;

            if (m_game?.Project?.CurrentLevel == null)
                return;

            if (lia.Type == AssetTypes.MODEL)
            {
                if (m_game.ProjectContent == null)
                    return;

                Models model = new Models(
                    m_game,
                    lia.Name,
                    Vector3.Zero,
                    1.0f
                );

                m_game.Project.CurrentLevel.AddModel(model);
            }
            else if (lia.Type == AssetTypes.TEXTURE)
            {
                m_dropped?.SetTexture(m_game, lia.Name);
            }
            else if (lia.Type == AssetTypes.EFFECT)
            {
                m_dropped?.SetShader(m_game, lia.Name);
            }

            m_dropped = null;
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
                Game.Project = new(Game, sfd.FileName);
                Game.InitializeProjectContent();
                Game.Project.AssetMonitor.UpdateAssetsDB();

                Text = "Our Cool Editor - " + Game.Project.Name;
                Game.AdjustAspectRatio();
                RefreshAssetList();
            }
            saveToolStripMenuItem1_Click_1(sender, e);
        }

        private void saveToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            if (Game?.Project == null)
            {
                MessageBox.Show("Please create or load a project first.");
                return;
            }
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

                Game.Project.Deserialize(
                    reader,
                    Game.Content,
                    folder =>
                    {
                        // The folder is initialized before models are loaded.
                        Game.Project.Folder = folder;

                        // Prepare the imported asset content manager.
                        Game.InitializeProjectContent();

                        return Game.ProjectContent;
                    }
                );

                Text = "Our Cool Editor - " + Game.Project.Name;
                Game.AdjustAspectRatio();
                Game.Project.AssetMonitor.UpdateAssetsDB();
                RefreshAssetList();
            }
        }



        private void propertyGrid_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Game?.Project == null)
            {
                MessageBox.Show("Please create or open a project first.");
                return;
            }

            string mgcbEditorPath =
                ConfigurationManager.AppSettings["MGCB_EditorPath"];

            if (string.IsNullOrWhiteSpace(mgcbEditorPath))
            {
                MessageBox.Show("MGCB editor path is not configured.");
                return;
            }

            string executablePath =
                Path.Combine(mgcbEditorPath, "mgcb-editor-windows.exe");

            string contentPath =
                Path.Combine(Game.Project.Folder, "Content.mgcb");

            if (!File.Exists(executablePath))
            {
                MessageBox.Show(
                    "MGCB executable NOT FOUND:\n\n" + executablePath
                );
                return;
            }

            if (!File.Exists(contentPath))
            {
                MessageBox.Show(
                    "Content.mgcb NOT FOUND:\n\n" + contentPath
                );
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = "\"" + contentPath + "\"",
                WorkingDirectory = Game.Project.Folder,
                UseShellExecute = true
            };

            m_MGCBProcess = Process.Start(startInfo);
            m_MGCBProcess.EnableRaisingEvents = true;

            m_MGCBProcess.Exited += (s, args) =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!IsDisposed)
                            RefreshAssetList();
                    }));
                }
            };
        }
        private void RefreshAssetList()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshAssetList));
                return;
            }

            listBoxAssets.Items.Clear();

            if (Game?.Project?.AssetMonitor == null)
                return;

            var assets = Game.Project.AssetMonitor.Assets;

            foreach (AssetTypes assetType in Enum.GetValues(typeof(AssetTypes)))
            {
                if (!assets.ContainsKey(assetType))
                    continue;

                listBoxAssets.Items.Add(new ListItemAsset
                {
                    Name = assetType.ToString().ToUpper() + "S:",
                    Type = AssetTypes.NONE
                });

                foreach (string asset in assets[assetType])
                {
                    listBoxAssets.Items.Add(new ListItemAsset
                    {
                        Name = asset,
                        Type = assetType
                    });
                }

                listBoxAssets.Items.Add(new ListItemAsset
                {
                    Name = "",
                    Type = AssetTypes.NONE
                });
            }
        }
        private void listBoxAssets_DoubleClick(object sender, EventArgs e)
        {
            if (Game?.Project?.CurrentLevel == null ||
                Game.ProjectContent == null)
                return;

            if (listBoxAssets.SelectedItem == null)
                return;

            string assetName = listBoxAssets.SelectedItem.ToString();
            if (assetName != "Fighter")
            {
                MessageBox.Show("Please select the Fighter model.");
                return;
            }

            // Only attempt to add assets that are compiled 3D models.
            try
            {
                Game.Project.CurrentLevel.AddImportedModel(
                    Game.ProjectContent,
                    Game.Content,
                    assetName,
                    "FighterDiffuse"
                );

                MessageBox.Show(assetName + " added to the scene!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not add this asset as a 3D model:\n\n" +
                    ex.Message
                );
            }
        }
        private void listBoxAssets_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
