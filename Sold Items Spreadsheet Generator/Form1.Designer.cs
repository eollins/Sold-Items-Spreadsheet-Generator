namespace Sold_Items_Spreadsheet_Generator
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
            this.selectType = new System.Windows.Forms.ComboBox();
            this.Begin = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.Log = new System.Windows.Forms.ListBox();
            this.openSpreadsheet = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.progressBar3 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // selectType
            // 
            this.selectType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectType.FormattingEnabled = true;
            this.selectType.Items.AddRange(new object[] {
            "Graded Cards",
            "Non-Graded Cards"});
            this.selectType.Location = new System.Drawing.Point(13, 13);
            this.selectType.Name = "selectType";
            this.selectType.Size = new System.Drawing.Size(282, 24);
            this.selectType.TabIndex = 0;
            // 
            // Begin
            // 
            this.Begin.Location = new System.Drawing.Point(301, 14);
            this.Begin.Name = "Begin";
            this.Begin.Size = new System.Drawing.Size(73, 24);
            this.Begin.TabIndex = 1;
            this.Begin.Text = "Begin";
            this.Begin.UseVisualStyleBackColor = true;
            this.Begin.Click += new System.EventHandler(this.Begin_Click);
            // 
            // Log
            // 
            this.Log.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Log.FormattingEnabled = true;
            this.Log.ItemHeight = 16;
            this.Log.Location = new System.Drawing.Point(13, 44);
            this.Log.Name = "Log";
            this.Log.Size = new System.Drawing.Size(361, 84);
            this.Log.TabIndex = 2;
            // 
            // openSpreadsheet
            // 
            this.openSpreadsheet.Filter = "Excel Workbook|*.xlsx|CSV files|*.csv";
            this.openSpreadsheet.Title = "Open Sales Spreadsheet";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 165);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(361, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 249);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(361, 18);
            this.label1.TabIndex = 4;
            this.label1.Text = "0 items grouped";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(13, 194);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(361, 23);
            this.progressBar2.TabIndex = 8;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 135);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(361, 20);
            this.textBox1.TabIndex = 9;
            this.textBox1.Text = "Not Running";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(13, 223);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(361, 23);
            this.progressBar3.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 275);
            this.Controls.Add(this.progressBar3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.Log);
            this.Controls.Add(this.Begin);
            this.Controls.Add(this.selectType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox selectType;
        private System.Windows.Forms.Button Begin;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ListBox Log;
        private System.Windows.Forms.OpenFileDialog openSpreadsheet;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ProgressBar progressBar3;
    }
}

