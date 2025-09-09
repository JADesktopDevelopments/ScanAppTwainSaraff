namespace WinFormsScanSaraff
{
    partial class SelectSourceForm
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
            sourceListBox = new ListBox();
            label1 = new Label();
            selectButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // sourceListBox
            // 
            sourceListBox.FormattingEnabled = true;
            sourceListBox.ItemHeight = 15;
            sourceListBox.Location = new Point(33, 89);
            sourceListBox.Name = "sourceListBox";
            sourceListBox.Size = new Size(347, 169);
            sourceListBox.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(33, 47);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // selectButton
            // 
            selectButton.Location = new Point(438, 126);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(75, 23);
            selectButton.TabIndex = 2;
            selectButton.Text = "Select";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += selectButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(438, 193);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // SelectSourceForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(627, 312);
            Controls.Add(cancelButton);
            Controls.Add(selectButton);
            Controls.Add(label1);
            Controls.Add(sourceListBox);
            Name = "SelectSourceForm";
            Text = "SelectSourceForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox sourceListBox;
        private Label label1;
        private Button selectButton;
        private Button cancelButton;
    }
}