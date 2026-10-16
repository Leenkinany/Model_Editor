namespace GUI.Editor
{
    partial class FormEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            splitContainer = new System.Windows.Forms.SplitContainer();
            projectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            createToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem1 });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(800, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem1
            // 
            fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exitToolStripMenuItem1, projectToolStripMenuItem1 });
            fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            fileToolStripMenuItem1.Size = new System.Drawing.Size(46, 24);
            fileToolStripMenuItem1.Text = "File";
            // 
            // exitToolStripMenuItem1
            // 
            exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            exitToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            exitToolStripMenuItem1.Text = "Exit";
            exitToolStripMenuItem1.Click += exitToolStripMenuItem1_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(138, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exitToolStripMenuItem, projectToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // projectToolStripMenuItem
            // 
            projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { createToolStripMenuItem, saveToolStripMenuItem, loadToolStripMenuItem });
            projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            projectToolStripMenuItem.Size = new System.Drawing.Size(138, 26);
            projectToolStripMenuItem.Text = "Project";
            // 
            // createToolStripMenuItem
            // 
            createToolStripMenuItem.Name = "createToolStripMenuItem";
            createToolStripMenuItem.Size = new System.Drawing.Size(83, 26);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(83, 26);
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(83, 26);
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new System.Drawing.Point(0, 424);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(800, 26);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(67, 20);
            toolStripStatusLabel1.Text = "Toolstrip";
            // 
            // splitContainer
            // 
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 28);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.SizeChanged += splitContainer_Panel1_SizeChanged;
            splitContainer.Size = new System.Drawing.Size(800, 396);
            splitContainer.SplitterDistance = 586;
            splitContainer.TabIndex = 2;
            // 
            // projectToolStripMenuItem1
            // 
            projectToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { createToolStripMenuItem1, saveToolStripMenuItem1, loadToolStripMenuItem1 });
            projectToolStripMenuItem1.Name = "projectToolStripMenuItem1";
            projectToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            projectToolStripMenuItem1.Text = "Project";
            // 
            // createToolStripMenuItem1
            // 
            createToolStripMenuItem1.Name = "createToolStripMenuItem1";
            createToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            createToolStripMenuItem1.Text = "Create";
            createToolStripMenuItem1.Click += createToolStripMenuItem1_Click_1;
            // 
            // saveToolStripMenuItem1
            // 
            saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            saveToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            saveToolStripMenuItem1.Text = "Save";
            saveToolStripMenuItem1.Click += saveToolStripMenuItem1_Click_1;
            // 
            // loadToolStripMenuItem1
            // 
            loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            loadToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            loadToolStripMenuItem1.Text = "Load";
            loadToolStripMenuItem1.Click += loadToolStripMenuItem1_Click_1;
            // 
            // FormEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(splitContainer);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "FormEditor";
            Text = "FormEditor";
            Load += FormEditor_Load;
            SizeChanged += FormEditor_SizeChanged;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem1;
    }
}   