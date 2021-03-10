
namespace AzureBackupInstaller.Desktop
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chk_portalalias = new System.Windows.Forms.CheckBox();
            this.btn_iisfolder_browse = new System.Windows.Forms.Button();
            this.btn_zip_browse = new System.Windows.Forms.Button();
            this.txt_zip = new System.Windows.Forms.TextBox();
            this.txt_websitename = new System.Windows.Forms.TextBox();
            this.txt_websiteurl = new System.Windows.Forms.TextBox();
            this.txt_iisfolder = new System.Windows.Forms.TextBox();
            this.txt_portalalias = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_package_location = new System.Windows.Forms.Button();
            this.txt_sqlpackage = new System.Windows.Forms.TextBox();
            this.txt_servername = new System.Windows.Forms.TextBox();
            this.txt_databasename = new System.Windows.Forms.TextBox();
            this.txt_serverusername = new System.Windows.Forms.TextBox();
            this.txt_serverpassword = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btn_install = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.rtb_logs = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(238, 49);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(329, 15);
            this.linkLabel1.TabIndex = 23;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://github.com/emimontesdeoca/azure-backup-installer";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label11.Location = new System.Drawing.Point(212, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(371, 37);
            this.label11.TabIndex = 22;
            this.label11.Text = "Azure DNN backup installer";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chk_portalalias);
            this.groupBox1.Controls.Add(this.btn_iisfolder_browse);
            this.groupBox1.Controls.Add(this.btn_zip_browse);
            this.groupBox1.Controls.Add(this.txt_zip);
            this.groupBox1.Controls.Add(this.txt_websitename);
            this.groupBox1.Controls.Add(this.txt_websiteurl);
            this.groupBox1.Controls.Add(this.txt_iisfolder);
            this.groupBox1.Controls.Add(this.txt_portalalias);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Location = new System.Drawing.Point(11, 73);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(385, 171);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Backup settings";
            // 
            // chk_portalalias
            // 
            this.chk_portalalias.AutoSize = true;
            this.chk_portalalias.Location = new System.Drawing.Point(361, 140);
            this.chk_portalalias.Name = "chk_portalalias";
            this.chk_portalalias.Size = new System.Drawing.Size(15, 14);
            this.chk_portalalias.TabIndex = 15;
            this.chk_portalalias.UseVisualStyleBackColor = true;
            // 
            // btn_iisfolder_browse
            // 
            this.btn_iisfolder_browse.Location = new System.Drawing.Point(301, 106);
            this.btn_iisfolder_browse.Name = "btn_iisfolder_browse";
            this.btn_iisfolder_browse.Size = new System.Drawing.Size(75, 24);
            this.btn_iisfolder_browse.TabIndex = 14;
            this.btn_iisfolder_browse.Text = "Browse";
            this.btn_iisfolder_browse.UseVisualStyleBackColor = true;
            this.btn_iisfolder_browse.Click += new System.EventHandler(this.btn_iisfolder_browse_Click);
            // 
            // btn_zip_browse
            // 
            this.btn_zip_browse.Location = new System.Drawing.Point(301, 18);
            this.btn_zip_browse.Name = "btn_zip_browse";
            this.btn_zip_browse.Size = new System.Drawing.Size(75, 24);
            this.btn_zip_browse.TabIndex = 13;
            this.btn_zip_browse.Text = "Browse";
            this.btn_zip_browse.UseVisualStyleBackColor = true;
            this.btn_zip_browse.Click += new System.EventHandler(this.btn_zip_browse_Click);
            // 
            // txt_zip
            // 
            this.txt_zip.Location = new System.Drawing.Point(101, 19);
            this.txt_zip.Name = "txt_zip";
            this.txt_zip.Size = new System.Drawing.Size(194, 23);
            this.txt_zip.TabIndex = 12;
            // 
            // txt_websitename
            // 
            this.txt_websitename.Location = new System.Drawing.Point(101, 49);
            this.txt_websitename.Name = "txt_websitename";
            this.txt_websitename.Size = new System.Drawing.Size(275, 23);
            this.txt_websitename.TabIndex = 11;
            // 
            // txt_websiteurl
            // 
            this.txt_websiteurl.Location = new System.Drawing.Point(101, 78);
            this.txt_websiteurl.Name = "txt_websiteurl";
            this.txt_websiteurl.Size = new System.Drawing.Size(275, 23);
            this.txt_websiteurl.TabIndex = 10;
            // 
            // txt_iisfolder
            // 
            this.txt_iisfolder.Location = new System.Drawing.Point(101, 107);
            this.txt_iisfolder.Name = "txt_iisfolder";
            this.txt_iisfolder.Size = new System.Drawing.Size(194, 23);
            this.txt_iisfolder.TabIndex = 9;
            // 
            // txt_portalalias
            // 
            this.txt_portalalias.Location = new System.Drawing.Point(101, 136);
            this.txt_portalalias.Name = "txt_portalalias";
            this.txt_portalalias.Size = new System.Drawing.Size(254, 23);
            this.txt_portalalias.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Website url";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Website folder";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Zip package";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "PortalAlias word";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 52);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 15);
            this.label10.TabIndex = 4;
            this.label10.Text = "Website name";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_package_location);
            this.groupBox2.Controls.Add(this.txt_sqlpackage);
            this.groupBox2.Controls.Add(this.txt_servername);
            this.groupBox2.Controls.Add(this.txt_databasename);
            this.groupBox2.Controls.Add(this.txt_serverusername);
            this.groupBox2.Controls.Add(this.txt_serverpassword);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(402, 73);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(385, 171);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SQL settings";
            // 
            // btn_package_location
            // 
            this.btn_package_location.Location = new System.Drawing.Point(301, 18);
            this.btn_package_location.Name = "btn_package_location";
            this.btn_package_location.Size = new System.Drawing.Size(75, 24);
            this.btn_package_location.TabIndex = 13;
            this.btn_package_location.Text = "Browse";
            this.btn_package_location.UseVisualStyleBackColor = true;
            this.btn_package_location.Click += new System.EventHandler(this.btn_package_location_Click);
            // 
            // txt_sqlpackage
            // 
            this.txt_sqlpackage.Location = new System.Drawing.Point(130, 19);
            this.txt_sqlpackage.Name = "txt_sqlpackage";
            this.txt_sqlpackage.Size = new System.Drawing.Size(165, 23);
            this.txt_sqlpackage.TabIndex = 12;
            // 
            // txt_servername
            // 
            this.txt_servername.Location = new System.Drawing.Point(101, 49);
            this.txt_servername.Name = "txt_servername";
            this.txt_servername.Size = new System.Drawing.Size(275, 23);
            this.txt_servername.TabIndex = 11;
            // 
            // txt_databasename
            // 
            this.txt_databasename.Location = new System.Drawing.Point(101, 78);
            this.txt_databasename.Name = "txt_databasename";
            this.txt_databasename.Size = new System.Drawing.Size(275, 23);
            this.txt_databasename.TabIndex = 10;
            // 
            // txt_serverusername
            // 
            this.txt_serverusername.Location = new System.Drawing.Point(101, 107);
            this.txt_serverusername.Name = "txt_serverusername";
            this.txt_serverusername.Size = new System.Drawing.Size(275, 23);
            this.txt_serverusername.TabIndex = 9;
            // 
            // txt_serverpassword
            // 
            this.txt_serverpassword.Location = new System.Drawing.Point(101, 136);
            this.txt_serverpassword.Name = "txt_serverpassword";
            this.txt_serverpassword.Size = new System.Drawing.Size(275, 23);
            this.txt_serverpassword.TabIndex = 8;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 81);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(88, 15);
            this.label9.TabIndex = 7;
            this.label9.Text = "Database name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 110);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "Server username";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 15);
            this.label6.TabIndex = 3;
            this.label6.Text = "SQL Package location";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 139);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 15);
            this.label7.TabIndex = 6;
            this.label7.Text = "Server password";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 52);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 15);
            this.label8.TabIndex = 4;
            this.label8.Text = "Server name";
            // 
            // btn_install
            // 
            this.btn_install.Enabled = false;
            this.btn_install.Location = new System.Drawing.Point(712, 503);
            this.btn_install.Name = "btn_install";
            this.btn_install.Size = new System.Drawing.Size(75, 23);
            this.btn_install.TabIndex = 19;
            this.btn_install.Text = "Install";
            this.btn_install.UseVisualStyleBackColor = true;
            this.btn_install.Click += new System.EventHandler(this.btn_install_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(11, 503);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(533, 23);
            this.progressBar.TabIndex = 18;
            // 
            // rtb_logs
            // 
            this.rtb_logs.Enabled = false;
            this.rtb_logs.Location = new System.Drawing.Point(11, 250);
            this.rtb_logs.Name = "rtb_logs";
            this.rtb_logs.Size = new System.Drawing.Size(776, 247);
            this.rtb_logs.TabIndex = 17;
            this.rtb_logs.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(631, 503);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 24;
            this.button1.Text = "Check";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(550, 503);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 25;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 539);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btn_install);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.rtb_logs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Azure DNN backup installer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_iisfolder_browse;
        private System.Windows.Forms.Button btn_zip_browse;
        private System.Windows.Forms.TextBox txt_zip;
        private System.Windows.Forms.TextBox txt_websitename;
        private System.Windows.Forms.TextBox txt_websiteurl;
        private System.Windows.Forms.TextBox txt_iisfolder;
        private System.Windows.Forms.TextBox txt_portalalias;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_package_location;
        private System.Windows.Forms.TextBox txt_sqlpackage;
        private System.Windows.Forms.TextBox txt_servername;
        private System.Windows.Forms.TextBox txt_databasename;
        private System.Windows.Forms.TextBox txt_serverusername;
        private System.Windows.Forms.TextBox txt_serverpassword;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btn_install;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.RichTextBox rtb_logs;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chk_portalalias;
        private System.Windows.Forms.Button btn_cancel;
    }
}

