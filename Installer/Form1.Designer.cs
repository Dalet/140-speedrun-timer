namespace SpeedrunTimerModInstaller
{
	partial class Form1
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
			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.txtGamePath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnInstall = new System.Windows.Forms.Button();
			this.tlpMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tlpMain
			// 
			this.tlpMain.ColumnCount = 1;
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
			this.tlpMain.Controls.Add(this.btnInstall, 0, 1);
			this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMain.Location = new System.Drawing.Point(3, 3);
			this.tlpMain.Name = "tlpMain";
			this.tlpMain.Padding = new System.Windows.Forms.Padding(3);
			this.tlpMain.RowCount = 2;
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.Size = new System.Drawing.Size(378, 65);
			this.tlpMain.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.txtGamePath, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnBrowse, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(372, 29);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// txtGamePath
			// 
			this.txtGamePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.txtGamePath.Location = new System.Drawing.Point(100, 4);
			this.txtGamePath.Name = "txtGamePath";
			this.txtGamePath.Size = new System.Drawing.Size(202, 20);
			this.txtGamePath.TabIndex = 1;
			this.txtGamePath.TextChanged += new System.EventHandler(this.txtGamePath_TextChanged);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Game folder path:";
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(308, 3);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(61, 23);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "Browse...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnInstall
			// 
			this.btnInstall.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.btnInstall.AutoSize = true;
			this.btnInstall.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnInstall.Enabled = false;
			this.btnInstall.Location = new System.Drawing.Point(159, 35);
			this.btnInstall.MinimumSize = new System.Drawing.Size(60, 0);
			this.btnInstall.Name = "btnInstall";
			this.btnInstall.Size = new System.Drawing.Size(60, 23);
			this.btnInstall.TabIndex = 4;
			this.btnInstall.Text = "Install";
			this.btnInstall.UseVisualStyleBackColor = true;
			this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 71);
			this.Controls.Add(this.tlpMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Padding = new System.Windows.Forms.Padding(3);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Speedrun Timer Installer";
			this.tlpMain.ResumeLayout(false);
			this.tlpMain.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tlpMain;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TextBox txtGamePath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnInstall;
	}
}

