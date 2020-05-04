namespace Goombratlas
{
    partial class WorldmapEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldmapEditorForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fUUUUUUUUNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.sceneListView1 = new GL_EditorFramework.SceneListView();
            this.objectUIControl1 = new GL_EditorFramework.ObjectUIControl();
            this.gL_ControlModern1 = new GL_EditorFramework.GL_Core.GL_ControlModern();
            this.straightenRouteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1158, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.fUUUUUUUUNToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // fUUUUUUUUNToolStripMenuItem
            // 
            this.fUUUUUUUUNToolStripMenuItem.Name = "fUUUUUUUUNToolStripMenuItem";
            this.fUUUUUUUUNToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.fUUUUUUUUNToolStripMenuItem.Text = "FUUUUUUUUN";
            this.fUUUUUUUUNToolStripMenuItem.Click += new System.EventHandler(this.FUNToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteSelectedToolStripMenuItem,
            this.straightenRouteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // deleteSelectedToolStripMenuItem
            // 
            this.deleteSelectedToolStripMenuItem.Name = "deleteSelectedToolStripMenuItem";
            this.deleteSelectedToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteSelectedToolStripMenuItem.Text = "Delete Selected";
            this.deleteSelectedToolStripMenuItem.Click += new System.EventHandler(this.DeleteSelectedToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gL_ControlModern1);
            this.splitContainer1.Size = new System.Drawing.Size(1134, 572);
            this.splitContainer1.SplitterDistance = 349;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.sceneListView1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.objectUIControl1);
            this.splitContainer2.Size = new System.Drawing.Size(349, 572);
            this.splitContainer2.SplitterDistance = 246;
            this.splitContainer2.TabIndex = 3;
            // 
            // sceneListView1
            // 
            this.sceneListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneListView1.Location = new System.Drawing.Point(0, 0);
            this.sceneListView1.Name = "sceneListView1";
            this.sceneListView1.RootLists = ((System.Collections.Generic.Dictionary<string, System.Collections.IList>)(resources.GetObject("sceneListView1.RootLists")));
            this.sceneListView1.Size = new System.Drawing.Size(349, 246);
            this.sceneListView1.TabIndex = 0;
            this.sceneListView1.SelectionChanged += new GL_EditorFramework.SelectionChangedEventHandler(this.SceneListView1_SelectionChanged);
            // 
            // objectUIControl1
            // 
            this.objectUIControl1.BackColor = System.Drawing.SystemColors.Control;
            this.objectUIControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectUIControl1.Location = new System.Drawing.Point(0, 0);
            this.objectUIControl1.Name = "objectUIControl1";
            this.objectUIControl1.Size = new System.Drawing.Size(349, 322);
            this.objectUIControl1.TabIndex = 0;
            // 
            // gL_ControlModern1
            // 
            this.gL_ControlModern1.BackColor = System.Drawing.Color.Black;
            this.gL_ControlModern1.CameraDistance = 10F;
            this.gL_ControlModern1.CameraTarget = ((OpenTK.Vector3)(resources.GetObject("gL_ControlModern1.CameraTarget")));
            this.gL_ControlModern1.CamRotX = 0F;
            this.gL_ControlModern1.CamRotY = 0F;
            this.gL_ControlModern1.CurrentShader = null;
            this.gL_ControlModern1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gL_ControlModern1.Fov = 0.7853982F;
            this.gL_ControlModern1.Location = new System.Drawing.Point(0, 0);
            this.gL_ControlModern1.Name = "gL_ControlModern1";
            this.gL_ControlModern1.NormPickingDepth = 0F;
            this.gL_ControlModern1.ShowOrientationCube = true;
            this.gL_ControlModern1.Size = new System.Drawing.Size(781, 572);
            this.gL_ControlModern1.Stereoscopy = false;
            this.gL_ControlModern1.TabIndex = 0;
            this.gL_ControlModern1.VSync = false;
            this.gL_ControlModern1.ZFar = 32000F;
            this.gL_ControlModern1.ZNear = 0.32F;
            // 
            // straightenRouteToolStripMenuItem
            // 
            this.straightenRouteToolStripMenuItem.Name = "straightenRouteToolStripMenuItem";
            this.straightenRouteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.straightenRouteToolStripMenuItem.Text = "Straighten Route";
            this.straightenRouteToolStripMenuItem.Click += new System.EventHandler(this.StraightenRouteToolStripMenuItem_Click);
            // 
            // WorldmapEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1158, 611);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "WorldmapEditorForm";
            this.Text = "Goombratlas";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GL_EditorFramework.GL_Core.GL_ControlModern gL_ControlModern1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fUUUUUUUUNToolStripMenuItem;
        private GL_EditorFramework.ObjectUIControl objectUIControl1;
        private GL_EditorFramework.SceneListView sceneListView1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem straightenRouteToolStripMenuItem;
    }
}

