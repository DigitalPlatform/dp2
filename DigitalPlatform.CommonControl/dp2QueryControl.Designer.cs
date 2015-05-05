namespace DigitalPlatform.CommonControl
{
    partial class dp2QueryControl
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(dp2QueryControl));
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.label_logic = new System.Windows.Forms.Label();
            this.label_word = new System.Windows.Forms.Label();
            this.label_from = new System.Windows.Forms.Label();
            this.label_serverName = new System.Windows.Forms.Label();
            this.label_database = new System.Windows.Forms.Label();
            this.label_matchStyle = new System.Windows.Forms.Label();
            this.imageList_states = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.AutoScroll = true;
            this.tableLayoutPanel_main.AutoSize = true;
            this.tableLayoutPanel_main.ColumnCount = 7;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.label_logic, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_word, 4, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_from, 5, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_serverName, 2, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_database, 3, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_matchStyle, 6, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(382, 132);
            this.tableLayoutPanel_main.TabIndex = 1;
            this.tableLayoutPanel_main.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel_main_MouseUp);
            // 
            // label_logic
            // 
            this.label_logic.AutoSize = true;
            this.label_logic.Location = new System.Drawing.Point(26, 0);
            this.label_logic.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_logic.Name = "label_logic";
            this.label_logic.Size = new System.Drawing.Size(53, 12);
            this.label_logic.TabIndex = 0;
            this.label_logic.Text = "逻辑算符";
            this.label_logic.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_logic_MouseUp);
            // 
            // label_word
            // 
            this.label_word.AutoSize = true;
            this.label_word.Location = new System.Drawing.Point(177, 0);
            this.label_word.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_word.MinimumSize = new System.Drawing.Size(75, 0);
            this.label_word.Name = "label_word";
            this.label_word.Size = new System.Drawing.Size(75, 12);
            this.label_word.TabIndex = 1;
            this.label_word.Text = "检索词";
            this.label_word.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_word_MouseUp);
            // 
            // label_from
            // 
            this.label_from.AutoSize = true;
            this.label_from.Location = new System.Drawing.Point(256, 0);
            this.label_from.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_from.Name = "label_from";
            this.label_from.Size = new System.Drawing.Size(53, 12);
            this.label_from.TabIndex = 2;
            this.label_from.Text = "检索途径";
            this.label_from.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_from_MouseUp);
            // 
            // label_serverName
            // 
            this.label_serverName.AutoSize = true;
            this.label_serverName.Location = new System.Drawing.Point(84, 0);
            this.label_serverName.Name = "label_serverName";
            this.label_serverName.Size = new System.Drawing.Size(41, 12);
            this.label_serverName.TabIndex = 3;
            this.label_serverName.Text = "服务器";
            this.label_serverName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_serverName_MouseUp);
            // 
            // label_database
            // 
            this.label_database.AutoSize = true;
            this.label_database.Location = new System.Drawing.Point(131, 0);
            this.label_database.Name = "label_database";
            this.label_database.Size = new System.Drawing.Size(41, 12);
            this.label_database.TabIndex = 4;
            this.label_database.Text = "数据库";
            this.label_database.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_database_MouseUp);
            // 
            // label_matchStyle
            // 
            this.label_matchStyle.AutoSize = true;
            this.label_matchStyle.Location = new System.Drawing.Point(314, 0);
            this.label_matchStyle.Name = "label_matchStyle";
            this.label_matchStyle.Size = new System.Drawing.Size(53, 12);
            this.label_matchStyle.TabIndex = 5;
            this.label_matchStyle.Text = "匹配方式";
            this.label_matchStyle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_matchStyle_MouseUp);
            // 
            // imageList_states
            // 
            this.imageList_states.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_states.ImageStream")));
            this.imageList_states.TransparentColor = System.Drawing.Color.White;
            this.imageList_states.Images.SetKeyName(0, "query_blank.bmp");
            this.imageList_states.Images.SetKeyName(1, "query_ok.bmp");
            // 
            // dp2QueryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Name = "dp2QueryControl";
            this.Size = new System.Drawing.Size(382, 132);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label_logic;
        private System.Windows.Forms.Label label_word;
        private System.Windows.Forms.Label label_from;
        private System.Windows.Forms.Label label_serverName;
        private System.Windows.Forms.Label label_database;
        private System.Windows.Forms.Label label_matchStyle;
        internal System.Windows.Forms.ImageList imageList_states;
        internal System.Windows.Forms.ToolTip toolTip1;
    }
}
