namespace WinFormsWIA
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
            pictureBoxPreview = new PictureBox();
            button1 = new Button();
            comboBoxDevices = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.BorderStyle = BorderStyle.Fixed3D;
            pictureBoxPreview.Location = new Point(364, 26);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new Size(859, 761);
            pictureBoxPreview.TabIndex = 0;
            pictureBoxPreview.TabStop = false;
            // 
            // button1
            // 
            button1.Location = new Point(56, 285);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += buttonScan_Click;
            // 
            // comboBoxDevices
            // 
            comboBoxDevices.FormattingEnabled = true;
            comboBoxDevices.Location = new Point(34, 36);
            comboBoxDevices.Name = "comboBoxDevices";
            comboBoxDevices.Size = new Size(227, 23);
            comboBoxDevices.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1254, 799);
            Controls.Add(comboBoxDevices);
            Controls.Add(button1);
            Controls.Add(pictureBoxPreview);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxPreview;
        private Button button1;
        private ComboBox comboBoxDevices;
    }
}
