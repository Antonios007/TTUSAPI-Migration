namespace TTUS_Migration
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_Connected = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelAPI = new System.Windows.Forms.ToolStripStatusLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.button_InsertExchangeTraders = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_Connected,
            this.toolStripStatusLabelAPI});
            this.statusStrip1.Location = new System.Drawing.Point(0, 238);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(380, 24);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_Connected
            // 
            this.toolStripStatusLabel_Connected.BackColor = System.Drawing.Color.Red;
            this.toolStripStatusLabel_Connected.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabel_Connected.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel_Connected.Name = "toolStripStatusLabel_Connected";
            this.toolStripStatusLabel_Connected.Size = new System.Drawing.Size(69, 19);
            this.toolStripStatusLabel_Connected.Text = "Connected";
            // 
            // toolStripStatusLabelAPI
            // 
            this.toolStripStatusLabelAPI.BackColor = System.Drawing.Color.Red;
            this.toolStripStatusLabelAPI.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelAPI.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelAPI.Name = "toolStripStatusLabelAPI";
            this.toolStripStatusLabelAPI.Size = new System.Drawing.Size(57, 19);
            this.toolStripStatusLabelAPI.Text = "TTUSAPI";
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(12, 86);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_InsertExchangeTraders
            // 
            this.button_InsertExchangeTraders.Dock = System.Windows.Forms.DockStyle.Top;
            this.button_InsertExchangeTraders.Enabled = false;
            this.button_InsertExchangeTraders.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_InsertExchangeTraders.Location = new System.Drawing.Point(0, 0);
            this.button_InsertExchangeTraders.Name = "button_InsertExchangeTraders";
            this.button_InsertExchangeTraders.Size = new System.Drawing.Size(380, 65);
            this.button_InsertExchangeTraders.TabIndex = 2;
            this.button_InsertExchangeTraders.Text = "Upload Exchange Traders";
            this.button_InsertExchangeTraders.UseVisualStyleBackColor = true;
            this.button_InsertExchangeTraders.Click += new System.EventHandler(this.button_InsertExchangeTraders_Click);
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(0, 143);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(380, 95);
            this.listBox1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 262);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button_InsertExchangeTraders);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "ASG TTUSAPI Gateway Migration Sample Code";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Connected;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelAPI;
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.Button button_InsertExchangeTraders;
        public System.Windows.Forms.ListBox listBox1;
    }
}

