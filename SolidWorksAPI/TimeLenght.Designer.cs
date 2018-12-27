namespace SolidWorksAPI
{
    partial class TimeLenght
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.OperationName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ToolpathTotalTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ToolpathTotalLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OperationName,
            this.ToolpathTotalTime,
            this.ToolpathTotalLength});
            this.dataGridView1.Location = new System.Drawing.Point(1, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(662, 482);
            this.dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(547, 510);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 33);
            this.button1.TabIndex = 1;
            this.button1.Text = "关闭";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // OperationName
            // 
            this.OperationName.DataPropertyName = "OperationName";
            this.OperationName.HeaderText = "工序名称";
            this.OperationName.Name = "OperationName";
            // 
            // ToolpathTotalTime
            // 
            this.ToolpathTotalTime.DataPropertyName = "ToolpathTotalTime";
            this.ToolpathTotalTime.HeaderText = "加工用时";
            this.ToolpathTotalTime.Name = "ToolpathTotalTime";
            // 
            // ToolpathTotalLength
            // 
            this.ToolpathTotalLength.DataPropertyName = "ToolpathTotalLength";
            this.ToolpathTotalLength.HeaderText = "刀具轨迹长度";
            this.ToolpathTotalLength.Name = "ToolpathTotalLength";
            this.ToolpathTotalLength.Width = 200;
            // 
            // TimeLenght
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 554);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "TimeLenght";
            this.Text = "获取特征时间";
            this.Load += new System.EventHandler(this.TimeLenght_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn OperationName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ToolpathTotalTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ToolpathTotalLength;
    }
}