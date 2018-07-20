namespace UART_Demo
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.comport = new System.Windows.Forms.ComboBox();
            this.open_btn = new System.Windows.Forms.Button();
            this.FILEPATH = new System.Windows.Forms.TextBox();
            this.openfile_btn = new System.Windows.Forms.Button();
            this.rec_box = new System.Windows.Forms.RichTextBox();
            this.send_box = new System.Windows.Forms.TextBox();
            this.baudRate_combox = new System.Windows.Forms.ComboBox();
            this.sendfile_btn = new System.Windows.Forms.Button();
            this.FileSize_lab = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.err_label = new System.Windows.Forms.Label();
            this.label_packet = new System.Windows.Forms.Label();
            this.RecFilebtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ParameterGroup = new System.Windows.Forms.GroupBox();
            this.packet_combox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pic_panel = new System.Windows.Forms.Panel();
            this.Ex_btn = new System.Windows.Forms.Button();
            this.savepic_btn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_download = new System.Windows.Forms.Button();
            this.labelTips = new System.Windows.Forms.Label();
            this.btn_save1BitBin = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开图像ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.重新搜索串口ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ParameterGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pic_panel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comport
            // 
            this.comport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.comport, "comport");
            this.comport.FormattingEnabled = true;
            this.comport.Name = "comport";
            // 
            // open_btn
            // 
            resources.ApplyResources(this.open_btn, "open_btn");
            this.open_btn.Name = "open_btn";
            this.open_btn.Tag = "close";
            this.open_btn.UseVisualStyleBackColor = true;
            this.open_btn.Click += new System.EventHandler(this.open_btn_Click);
            // 
            // FILEPATH
            // 
            resources.ApplyResources(this.FILEPATH, "FILEPATH");
            this.FILEPATH.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.FILEPATH.Name = "FILEPATH";
            this.FILEPATH.Enter += new System.EventHandler(this.FILEPATH__Enter);
            this.FILEPATH.Leave += new System.EventHandler(this.FILEPATH__Leave);
            // 
            // openfile_btn
            // 
            resources.ApplyResources(this.openfile_btn, "openfile_btn");
            this.openfile_btn.Name = "openfile_btn";
            this.openfile_btn.UseVisualStyleBackColor = true;
            this.openfile_btn.Click += new System.EventHandler(this.openfile_btn_Click);
            // 
            // rec_box
            // 
            resources.ApplyResources(this.rec_box, "rec_box");
            this.rec_box.Name = "rec_box";
            this.rec_box.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.rec_box_MouseDoubleClick);
            // 
            // send_box
            // 
            resources.ApplyResources(this.send_box, "send_box");
            this.send_box.Name = "send_box";
            this.send_box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.send_box_KeyPress);
            // 
            // baudRate_combox
            // 
            this.baudRate_combox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.baudRate_combox, "baudRate_combox");
            this.baudRate_combox.FormattingEnabled = true;
            this.baudRate_combox.Items.AddRange(new object[] {
            resources.GetString("baudRate_combox.Items"),
            resources.GetString("baudRate_combox.Items1"),
            resources.GetString("baudRate_combox.Items2"),
            resources.GetString("baudRate_combox.Items3")});
            this.baudRate_combox.Name = "baudRate_combox";
            // 
            // sendfile_btn
            // 
            resources.ApplyResources(this.sendfile_btn, "sendfile_btn");
            this.sendfile_btn.Name = "sendfile_btn";
            this.sendfile_btn.UseVisualStyleBackColor = true;
            this.sendfile_btn.Click += new System.EventHandler(this.sendfile_btn_Click);
            // 
            // FileSize_lab
            // 
            resources.ApplyResources(this.FileSize_lab, "FileSize_lab");
            this.FileSize_lab.Name = "FileSize_lab";
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // err_label
            // 
            resources.ApplyResources(this.err_label, "err_label");
            this.err_label.ForeColor = System.Drawing.Color.Red;
            this.err_label.Name = "err_label";
            // 
            // label_packet
            // 
            resources.ApplyResources(this.label_packet, "label_packet");
            this.label_packet.Name = "label_packet";
            // 
            // RecFilebtn
            // 
            resources.ApplyResources(this.RecFilebtn, "RecFilebtn");
            this.RecFilebtn.Name = "RecFilebtn";
            this.RecFilebtn.UseVisualStyleBackColor = true;
            this.RecFilebtn.Click += new System.EventHandler(this.RecFilebtn_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // ParameterGroup
            // 
            this.ParameterGroup.Controls.Add(this.packet_combox);
            this.ParameterGroup.Controls.Add(this.comport);
            this.ParameterGroup.Controls.Add(this.label1);
            this.ParameterGroup.Controls.Add(this.baudRate_combox);
            resources.ApplyResources(this.ParameterGroup, "ParameterGroup");
            this.ParameterGroup.Name = "ParameterGroup";
            this.ParameterGroup.TabStop = false;
            // 
            // packet_combox
            // 
            this.packet_combox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.packet_combox, "packet_combox");
            this.packet_combox.FormattingEnabled = true;
            this.packet_combox.Name = "packet_combox";
            this.packet_combox.SelectedIndexChanged += new System.EventHandler(this.packet_combox_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_DoubleClick);
            // 
            // pic_panel
            // 
            resources.ApplyResources(this.pic_panel, "pic_panel");
            this.pic_panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pic_panel.Controls.Add(this.pictureBox1);
            this.pic_panel.Name = "pic_panel";
            // 
            // Ex_btn
            // 
            resources.ApplyResources(this.Ex_btn, "Ex_btn");
            this.Ex_btn.Name = "Ex_btn";
            this.Ex_btn.UseVisualStyleBackColor = true;
            this.Ex_btn.TextChanged += new System.EventHandler(this.Ex_btn_TextChanged);
            this.Ex_btn.Click += new System.EventHandler(this.Ex_btn_Click);
            // 
            // savepic_btn
            // 
            resources.ApplyResources(this.savepic_btn, "savepic_btn");
            this.savepic_btn.Name = "savepic_btn";
            this.savepic_btn.UseVisualStyleBackColor = true;
            this.savepic_btn.Click += new System.EventHandler(this.savepic_btn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_save1BitBin);
            this.groupBox1.Controls.Add(this.btn_download);
            this.groupBox1.Controls.Add(this.labelTips);
            this.groupBox1.Controls.Add(this.savepic_btn);
            this.groupBox1.Controls.Add(this.pic_panel);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btn_download
            // 
            resources.ApplyResources(this.btn_download, "btn_download");
            this.btn_download.Name = "btn_download";
            this.btn_download.UseVisualStyleBackColor = true;
            this.btn_download.Click += new System.EventHandler(this.btn_download_Click);
            // 
            // labelTips
            // 
            resources.ApplyResources(this.labelTips, "labelTips");
            this.labelTips.Name = "labelTips";
            this.labelTips.Click += new System.EventHandler(this.label3_Click);
            // 
            // btn_save1BitBin
            // 
            resources.ApplyResources(this.btn_save1BitBin, "btn_save1BitBin");
            this.btn_save1BitBin.Name = "btn_save1BitBin";
            this.btn_save1BitBin.UseVisualStyleBackColor = true;
            this.btn_save1BitBin.Click += new System.EventHandler(this.btn_save1BitBin_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.关于ToolStripMenuItem,
            this.重新搜索串口ToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开图像ToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            // 
            // 打开图像ToolStripMenuItem
            // 
            this.打开图像ToolStripMenuItem.Name = "打开图像ToolStripMenuItem";
            resources.ApplyResources(this.打开图像ToolStripMenuItem, "打开图像ToolStripMenuItem");
            this.打开图像ToolStripMenuItem.Click += new System.EventHandler(this.打开图像ToolStripMenuItem_Click);
            // 
            // 关于ToolStripMenuItem
            // 
            this.关于ToolStripMenuItem.Name = "关于ToolStripMenuItem";
            resources.ApplyResources(this.关于ToolStripMenuItem, "关于ToolStripMenuItem");
            this.关于ToolStripMenuItem.Click += new System.EventHandler(this.关于ToolStripMenuItem_Click);
            // 
            // 重新搜索串口ToolStripMenuItem
            // 
            this.重新搜索串口ToolStripMenuItem.Name = "重新搜索串口ToolStripMenuItem";
            resources.ApplyResources(this.重新搜索串口ToolStripMenuItem, "重新搜索串口ToolStripMenuItem");
            this.重新搜索串口ToolStripMenuItem.Click += new System.EventHandler(this.重新搜索串口ToolStripMenuItem_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Ex_btn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ParameterGroup);
            this.Controls.Add(this.RecFilebtn);
            this.Controls.Add(this.label_packet);
            this.Controls.Add(this.err_label);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.FileSize_lab);
            this.Controls.Add(this.sendfile_btn);
            this.Controls.Add(this.send_box);
            this.Controls.Add(this.rec_box);
            this.Controls.Add(this.openfile_btn);
            this.Controls.Add(this.FILEPATH);
            this.Controls.Add(this.open_btn);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ParameterGroup.ResumeLayout(false);
            this.ParameterGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pic_panel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comport;
        private System.Windows.Forms.Button open_btn;
        private System.Windows.Forms.TextBox FILEPATH;
        private System.Windows.Forms.Button openfile_btn;
        private System.Windows.Forms.RichTextBox rec_box;
        private System.Windows.Forms.TextBox send_box;
        private System.Windows.Forms.ComboBox baudRate_combox;
        private System.Windows.Forms.Button sendfile_btn;
        private System.Windows.Forms.Label FileSize_lab;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label err_label;
        private System.Windows.Forms.Label label_packet;
        private System.Windows.Forms.Button RecFilebtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox ParameterGroup;
        private System.Windows.Forms.ComboBox packet_combox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pic_panel;
        private System.Windows.Forms.Button Ex_btn;
        private System.Windows.Forms.Button savepic_btn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelTips;
        private System.Windows.Forms.Button btn_download;
        private System.Windows.Forms.Button btn_save1BitBin;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关于ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开图像ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 重新搜索串口ToolStripMenuItem;
    }
}

