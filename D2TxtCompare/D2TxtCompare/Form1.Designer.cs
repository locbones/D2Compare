namespace D2TxtCompare
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
            btnBatchLoad = new Button();
            dropSource = new ComboBox();
            dropTarget = new ComboBox();
            labelSource = new Label();
            labelTarget = new Label();
            textColumns = new RichTextBox();
            textRows = new RichTextBox();
            textValues = new RichTextBox();
            labelColumns = new Label();
            labelRows = new Label();
            dropFiles = new ComboBox();
            labelFiles = new Label();
            textSearch = new TextBox();
            btnPrev = new Button();
            btnNext = new Button();
            labelSearch = new Label();
            textFiles = new RichTextBox();
            labelStatus = new Label();
            SuspendLayout();
            // 
            // btnBatchLoad
            // 
            btnBatchLoad.BackColor = Color.Black;
            btnBatchLoad.FlatAppearance.BorderColor = Color.DarkRed;
            btnBatchLoad.FlatAppearance.BorderSize = 3;
            btnBatchLoad.FlatStyle = FlatStyle.Flat;
            btnBatchLoad.Font = new Font("Arial", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            btnBatchLoad.ForeColor = Color.BurlyWood;
            btnBatchLoad.Location = new Point(667, 64);
            btnBatchLoad.Name = "btnBatchLoad";
            btnBatchLoad.Size = new Size(130, 39);
            btnBatchLoad.TabIndex = 1;
            btnBatchLoad.Text = "Batch Load";
            btnBatchLoad.UseVisualStyleBackColor = false;
            btnBatchLoad.Click += btnBatchLoad_Click;
            // 
            // dropSource
            // 
            dropSource.FormattingEnabled = true;
            dropSource.Items.AddRange(new object[] { "Legacy (1.13c+)", "1.0.0.0 (62115)", "1.4.0.0 (64954)", "2.2.0.0 (65890)", "2.3.0.0 (67314)", "2.3.0.1 (67358)", "2.3.1.0 (67554)", "2.4.1.1 (68992)", "2.4.1.2 (69270)", "2.4.3.0 (70161)", "2.5.0.0 (71336)", "2.5.1.0 (71510)", "2.5.2.0 (71776)", "2.6.0.0 (73090)", "2.7.2.0 (77312)", "Custom" });
            dropSource.Location = new Point(526, 29);
            dropSource.Name = "dropSource";
            dropSource.Size = new Size(130, 23);
            dropSource.TabIndex = 2;
            dropSource.SelectedIndexChanged += dropSource_SelectedIndexChanged;
            // 
            // dropTarget
            // 
            dropTarget.FormattingEnabled = true;
            dropTarget.Items.AddRange(new object[] { "Legacy (1.13c+)", "1.0.0.0 (62115)", "1.4.0.0 (64954)", "2.2.0.0 (65890)", "2.3.0.0 (67314)", "2.3.0.1 (67358)", "2.3.1.0 (67554)", "2.4.1.1 (68992)", "2.4.1.2 (69270)", "2.4.3.0 (70161)", "2.5.0.0 (71336)", "2.5.1.0 (71510)", "2.5.2.0 (71776)", "2.6.0.0 (73090)", "2.7.2.0 (77312)", "Custom" });
            dropTarget.Location = new Point(667, 29);
            dropTarget.Name = "dropTarget";
            dropTarget.Size = new Size(130, 23);
            dropTarget.TabIndex = 3;
            dropTarget.SelectedIndexChanged += dropTarget_SelectedIndexChanged;
            // 
            // labelSource
            // 
            labelSource.Location = new Point(526, 4);
            labelSource.Name = "labelSource";
            labelSource.Size = new Size(128, 23);
            labelSource.TabIndex = 4;
            labelSource.Text = "Source Version";
            labelSource.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelTarget
            // 
            labelTarget.Location = new Point(667, 4);
            labelTarget.Name = "labelTarget";
            labelTarget.Size = new Size(130, 23);
            labelTarget.TabIndex = 5;
            labelTarget.Text = "Target Version";
            labelTarget.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textColumns
            // 
            textColumns.Location = new Point(6, 312);
            textColumns.Name = "textColumns";
            textColumns.Size = new Size(349, 421);
            textColumns.TabIndex = 6;
            textColumns.Text = "";
            // 
            // textRows
            // 
            textRows.Location = new Point(361, 312);
            textRows.Name = "textRows";
            textRows.Size = new Size(382, 421);
            textRows.TabIndex = 7;
            textRows.Text = "";
            // 
            // textValues
            // 
            textValues.Location = new Point(749, 312);
            textValues.Name = "textValues";
            textValues.Size = new Size(622, 421);
            textValues.TabIndex = 8;
            textValues.Text = "";
            // 
            // labelColumns
            // 
            labelColumns.Location = new Point(6, 285);
            labelColumns.Name = "labelColumns";
            labelColumns.Size = new Size(349, 24);
            labelColumns.TabIndex = 9;
            labelColumns.Text = "Columns Altered";
            labelColumns.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelRows
            // 
            labelRows.Location = new Point(361, 285);
            labelRows.Name = "labelRows";
            labelRows.Size = new Size(382, 24);
            labelRows.TabIndex = 10;
            labelRows.Text = "Rows Altered";
            labelRows.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dropFiles
            // 
            dropFiles.FormattingEnabled = true;
            dropFiles.Location = new Point(526, 80);
            dropFiles.Name = "dropFiles";
            dropFiles.Size = new Size(134, 23);
            dropFiles.TabIndex = 12;
            dropFiles.SelectedIndexChanged += dropFiles_SelectedIndexChanged;
            // 
            // labelFiles
            // 
            labelFiles.Location = new Point(526, 55);
            labelFiles.Name = "labelFiles";
            labelFiles.Size = new Size(135, 23);
            labelFiles.TabIndex = 13;
            labelFiles.Text = "Comparison File";
            labelFiles.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textSearch
            // 
            textSearch.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic, GraphicsUnit.Point);
            textSearch.ForeColor = SystemColors.WindowFrame;
            textSearch.Location = new Point(1012, 264);
            textSearch.Name = "textSearch";
            textSearch.Size = new Size(100, 25);
            textSearch.TabIndex = 14;
            textSearch.Text = "Search Term(s)";
            textSearch.TextAlign = HorizontalAlignment.Center;
            textSearch.TextChanged += textSearch_TextChanged;
            textSearch.Enter += textSearch_Enter;
            // 
            // btnPrev
            // 
            btnPrev.BackgroundImage = Properties.Resources.arrow_icon_l;
            btnPrev.BackgroundImageLayout = ImageLayout.Stretch;
            btnPrev.FlatAppearance.BorderColor = Color.Black;
            btnPrev.FlatAppearance.BorderSize = 0;
            btnPrev.FlatStyle = FlatStyle.Flat;
            btnPrev.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnPrev.Location = new Point(983, 265);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(23, 21);
            btnPrev.TabIndex = 15;
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // btnNext
            // 
            btnNext.BackgroundImage = Properties.Resources.arrow_icon_r;
            btnNext.BackgroundImageLayout = ImageLayout.Stretch;
            btnNext.FlatAppearance.BorderColor = Color.Black;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btnNext.Location = new Point(1118, 265);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(23, 21);
            btnNext.TabIndex = 16;
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // labelSearch
            // 
            labelSearch.Location = new Point(749, 289);
            labelSearch.Name = "labelSearch";
            labelSearch.Size = new Size(622, 20);
            labelSearch.TabIndex = 17;
            labelSearch.Text = "0 of 0";
            labelSearch.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textFiles
            // 
            textFiles.Location = new Point(526, 114);
            textFiles.Name = "textFiles";
            textFiles.Size = new Size(271, 128);
            textFiles.TabIndex = 19;
            textFiles.Text = "";
            // 
            // labelStatus
            // 
            labelStatus.BackColor = SystemColors.Control;
            labelStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelStatus.ForeColor = Color.Black;
            labelStatus.Location = new Point(526, 245);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(271, 20);
            labelStatus.TabIndex = 20;
            labelStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1376, 739);
            Controls.Add(labelStatus);
            Controls.Add(textFiles);
            Controls.Add(labelSearch);
            Controls.Add(btnNext);
            Controls.Add(btnPrev);
            Controls.Add(textSearch);
            Controls.Add(labelFiles);
            Controls.Add(dropFiles);
            Controls.Add(labelRows);
            Controls.Add(labelColumns);
            Controls.Add(textValues);
            Controls.Add(textRows);
            Controls.Add(textColumns);
            Controls.Add(labelTarget);
            Controls.Add(labelSource);
            Controls.Add(dropTarget);
            Controls.Add(dropSource);
            Controls.Add(btnBatchLoad);
            Name = "Form1";
            Text = "D2Compare v1.0";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnBatchLoad;
        private ComboBox dropSource;
        private ComboBox dropTarget;
        private Label labelSource;
        private Label labelTarget;
        private RichTextBox textColumns;
        private RichTextBox textRows;
        private RichTextBox textValues;
        private Label labelColumns;
        private Label labelRows;
        private ComboBox dropFiles;
        private Label labelFiles;
        private TextBox textSearch;
        private Button btnPrev;
        private Button btnNext;
        private Label labelSearch;
        private RichTextBox textFiles;
        private Label labelStatus;
    }
}
