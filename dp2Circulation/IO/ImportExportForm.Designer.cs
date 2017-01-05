namespace dp2Circulation
{
    partial class ImportExportForm
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_source = new System.Windows.Forms.TabPage();
            this.textBox_source_range = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_getObjectDirectoryName = new System.Windows.Forms.Button();
            this.textBox_objectDirectoryName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_source_findFileName = new System.Windows.Forms.Button();
            this.textBox_source_fileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_subRecords_object = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_comment = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_issue = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_order = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_entity = new System.Windows.Forms.CheckBox();
            this.tabPage_convert = new System.Windows.Forms.TabPage();
            this.textBox_convert_itemBatchNo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_convert_addBiblioToItem = new System.Windows.Forms.CheckBox();
            this.panel_map = new System.Windows.Forms.Panel();
            this.button_convert_initialMapString = new System.Windows.Forms.Button();
            this.checkBox_target_newRefID = new System.Windows.Forms.CheckBox();
            this.checkBox_target_randomItemBarcode = new System.Windows.Forms.CheckBox();
            this.tabPage_target = new System.Windows.Forms.TabPage();
            this.checkBox_target_dontChangeOperations = new System.Windows.Forms.CheckBox();
            this.checkBox_target_suppressOperLog = new System.Windows.Forms.CheckBox();
            this.checkBox_target_dontSearchDup = new System.Windows.Forms.CheckBox();
            this.checkBox_target_restoreOldID = new System.Windows.Forms.CheckBox();
            this.button_target_simulateImport = new System.Windows.Forms.Button();
            this.comboBox_target_targetBiblioDbName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_run = new System.Windows.Forms.TabPage();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_next = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_target_dbNameList = new System.Windows.Forms.TextBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_source.SuspendLayout();
            this.tabPage_convert.SuspendLayout();
            this.tabPage_target.SuspendLayout();
            this.tabPage_run.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_source);
            this.tabControl_main.Controls.Add(this.tabPage_convert);
            this.tabControl_main.Controls.Add(this.tabPage_target);
            this.tabControl_main.Controls.Add(this.tabPage_run);
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(443, 283);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_source
            // 
            this.tabPage_source.AutoScroll = true;
            this.tabPage_source.Controls.Add(this.textBox_source_range);
            this.tabPage_source.Controls.Add(this.label4);
            this.tabPage_source.Controls.Add(this.button_getObjectDirectoryName);
            this.tabPage_source.Controls.Add(this.textBox_objectDirectoryName);
            this.tabPage_source.Controls.Add(this.label3);
            this.tabPage_source.Controls.Add(this.button_source_findFileName);
            this.tabPage_source.Controls.Add(this.textBox_source_fileName);
            this.tabPage_source.Controls.Add(this.label2);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_object);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_comment);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_issue);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_order);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_entity);
            this.tabPage_source.Location = new System.Drawing.Point(4, 22);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_source.Size = new System.Drawing.Size(435, 257);
            this.tabPage_source.TabIndex = 1;
            this.tabPage_source.Text = "源文件";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // textBox_source_range
            // 
            this.textBox_source_range.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_source_range.Location = new System.Drawing.Point(125, 179);
            this.textBox_source_range.Name = "textBox_source_range";
            this.textBox_source_range.Size = new System.Drawing.Size(258, 21);
            this.textBox_source_range.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 182);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "导入记录范围(&R):";
            // 
            // button_getObjectDirectoryName
            // 
            this.button_getObjectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getObjectDirectoryName.Location = new System.Drawing.Point(389, 128);
            this.button_getObjectDirectoryName.Name = "button_getObjectDirectoryName";
            this.button_getObjectDirectoryName.Size = new System.Drawing.Size(39, 23);
            this.button_getObjectDirectoryName.TabIndex = 13;
            this.button_getObjectDirectoryName.Text = "...";
            this.button_getObjectDirectoryName.UseVisualStyleBackColor = true;
            this.button_getObjectDirectoryName.Click += new System.EventHandler(this.button_getObjectDirectoryName_Click);
            // 
            // textBox_objectDirectoryName
            // 
            this.textBox_objectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_objectDirectoryName.Location = new System.Drawing.Point(27, 130);
            this.textBox_objectDirectoryName.Name = "textBox_objectDirectoryName";
            this.textBox_objectDirectoryName.Size = new System.Drawing.Size(356, 21);
            this.textBox_objectDirectoryName.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "对象文件目录(&O):";
            // 
            // button_source_findFileName
            // 
            this.button_source_findFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_source_findFileName.Location = new System.Drawing.Point(389, 19);
            this.button_source_findFileName.Name = "button_source_findFileName";
            this.button_source_findFileName.Size = new System.Drawing.Size(39, 23);
            this.button_source_findFileName.TabIndex = 7;
            this.button_source_findFileName.Text = "...";
            this.button_source_findFileName.UseVisualStyleBackColor = true;
            this.button_source_findFileName.Click += new System.EventHandler(this.button_source_findFileName_Click);
            // 
            // textBox_source_fileName
            // 
            this.textBox_source_fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_source_fileName.Location = new System.Drawing.Point(125, 21);
            this.textBox_source_fileName.Name = "textBox_source_fileName";
            this.textBox_source_fileName.Size = new System.Drawing.Size(258, 21);
            this.textBox_source_fileName.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "书目转储文件名(&D):";
            // 
            // checkBox_subRecords_object
            // 
            this.checkBox_subRecords_object.AutoSize = true;
            this.checkBox_subRecords_object.Checked = true;
            this.checkBox_subRecords_object.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_object.Location = new System.Drawing.Point(8, 96);
            this.checkBox_subRecords_object.Name = "checkBox_subRecords_object";
            this.checkBox_subRecords_object.Size = new System.Drawing.Size(66, 16);
            this.checkBox_subRecords_object.TabIndex = 4;
            this.checkBox_subRecords_object.Text = "对象(&O)";
            this.checkBox_subRecords_object.UseVisualStyleBackColor = true;
            this.checkBox_subRecords_object.CheckedChanged += new System.EventHandler(this.checkBox_subRecords_object_CheckedChanged);
            // 
            // checkBox_subRecords_comment
            // 
            this.checkBox_subRecords_comment.AutoSize = true;
            this.checkBox_subRecords_comment.Checked = true;
            this.checkBox_subRecords_comment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_comment.Location = new System.Drawing.Point(276, 54);
            this.checkBox_subRecords_comment.Name = "checkBox_subRecords_comment";
            this.checkBox_subRecords_comment.Size = new System.Drawing.Size(66, 16);
            this.checkBox_subRecords_comment.TabIndex = 3;
            this.checkBox_subRecords_comment.Text = "评注(&C)";
            this.checkBox_subRecords_comment.UseVisualStyleBackColor = true;
            // 
            // checkBox_subRecords_issue
            // 
            this.checkBox_subRecords_issue.AutoSize = true;
            this.checkBox_subRecords_issue.Checked = true;
            this.checkBox_subRecords_issue.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_issue.Location = new System.Drawing.Point(194, 54);
            this.checkBox_subRecords_issue.Name = "checkBox_subRecords_issue";
            this.checkBox_subRecords_issue.Size = new System.Drawing.Size(54, 16);
            this.checkBox_subRecords_issue.TabIndex = 2;
            this.checkBox_subRecords_issue.Text = "期(&I)";
            this.checkBox_subRecords_issue.UseVisualStyleBackColor = true;
            // 
            // checkBox_subRecords_order
            // 
            this.checkBox_subRecords_order.AutoSize = true;
            this.checkBox_subRecords_order.Checked = true;
            this.checkBox_subRecords_order.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_order.Location = new System.Drawing.Point(98, 54);
            this.checkBox_subRecords_order.Name = "checkBox_subRecords_order";
            this.checkBox_subRecords_order.Size = new System.Drawing.Size(66, 16);
            this.checkBox_subRecords_order.TabIndex = 1;
            this.checkBox_subRecords_order.Text = "订购(&O)";
            this.checkBox_subRecords_order.UseVisualStyleBackColor = true;
            // 
            // checkBox_subRecords_entity
            // 
            this.checkBox_subRecords_entity.AutoSize = true;
            this.checkBox_subRecords_entity.Checked = true;
            this.checkBox_subRecords_entity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_entity.Location = new System.Drawing.Point(8, 54);
            this.checkBox_subRecords_entity.Name = "checkBox_subRecords_entity";
            this.checkBox_subRecords_entity.Size = new System.Drawing.Size(54, 16);
            this.checkBox_subRecords_entity.TabIndex = 0;
            this.checkBox_subRecords_entity.Text = "册(&E)";
            this.checkBox_subRecords_entity.UseVisualStyleBackColor = true;
            // 
            // tabPage_convert
            // 
            this.tabPage_convert.AutoScroll = true;
            this.tabPage_convert.Controls.Add(this.textBox_convert_itemBatchNo);
            this.tabPage_convert.Controls.Add(this.label5);
            this.tabPage_convert.Controls.Add(this.checkBox_convert_addBiblioToItem);
            this.tabPage_convert.Controls.Add(this.panel_map);
            this.tabPage_convert.Controls.Add(this.button_convert_initialMapString);
            this.tabPage_convert.Controls.Add(this.checkBox_target_newRefID);
            this.tabPage_convert.Controls.Add(this.checkBox_target_randomItemBarcode);
            this.tabPage_convert.Location = new System.Drawing.Point(4, 22);
            this.tabPage_convert.Name = "tabPage_convert";
            this.tabPage_convert.Size = new System.Drawing.Size(435, 257);
            this.tabPage_convert.TabIndex = 3;
            this.tabPage_convert.Text = "转换";
            this.tabPage_convert.UseVisualStyleBackColor = true;
            // 
            // textBox_convert_itemBatchNo
            // 
            this.textBox_convert_itemBatchNo.Location = new System.Drawing.Point(103, 82);
            this.textBox_convert_itemBatchNo.Name = "textBox_convert_itemBatchNo";
            this.textBox_convert_itemBatchNo.Size = new System.Drawing.Size(186, 21);
            this.textBox_convert_itemBatchNo.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 85);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "册记录批次号:";
            // 
            // checkBox_convert_addBiblioToItem
            // 
            this.checkBox_convert_addBiblioToItem.AutoSize = true;
            this.checkBox_convert_addBiblioToItem.Location = new System.Drawing.Point(3, 38);
            this.checkBox_convert_addBiblioToItem.Name = "checkBox_convert_addBiblioToItem";
            this.checkBox_convert_addBiblioToItem.Size = new System.Drawing.Size(168, 16);
            this.checkBox_convert_addBiblioToItem.TabIndex = 13;
            this.checkBox_convert_addBiblioToItem.Text = "为册记录添加书目信息元素";
            this.checkBox_convert_addBiblioToItem.UseVisualStyleBackColor = true;
            // 
            // panel_map
            // 
            this.panel_map.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_map.Location = new System.Drawing.Point(3, 138);
            this.panel_map.Name = "panel_map";
            this.panel_map.Size = new System.Drawing.Size(327, 122);
            this.panel_map.TabIndex = 12;
            // 
            // button_convert_initialMapString
            // 
            this.button_convert_initialMapString.Location = new System.Drawing.Point(3, 109);
            this.button_convert_initialMapString.Name = "button_convert_initialMapString";
            this.button_convert_initialMapString.Size = new System.Drawing.Size(286, 23);
            this.button_convert_initialMapString.TabIndex = 10;
            this.button_convert_initialMapString.Text = "从数据中获取馆藏地，初始化馆藏地转换表 ...";
            this.button_convert_initialMapString.UseVisualStyleBackColor = true;
            this.button_convert_initialMapString.Click += new System.EventHandler(this.button_convert_initialMapString_Click);
            // 
            // checkBox_target_newRefID
            // 
            this.checkBox_target_newRefID.AutoSize = true;
            this.checkBox_target_newRefID.Checked = true;
            this.checkBox_target_newRefID.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_target_newRefID.Location = new System.Drawing.Point(3, 60);
            this.checkBox_target_newRefID.Name = "checkBox_target_newRefID";
            this.checkBox_target_newRefID.Size = new System.Drawing.Size(108, 16);
            this.checkBox_target_newRefID.TabIndex = 9;
            this.checkBox_target_newRefID.Text = "重新生成参考ID";
            this.checkBox_target_newRefID.UseVisualStyleBackColor = true;
            this.checkBox_target_newRefID.Visible = false;
            // 
            // checkBox_target_randomItemBarcode
            // 
            this.checkBox_target_randomItemBarcode.AutoSize = true;
            this.checkBox_target_randomItemBarcode.Location = new System.Drawing.Point(3, 16);
            this.checkBox_target_randomItemBarcode.Name = "checkBox_target_randomItemBarcode";
            this.checkBox_target_randomItemBarcode.Size = new System.Drawing.Size(360, 16);
            this.checkBox_target_randomItemBarcode.TabIndex = 8;
            this.checkBox_target_randomItemBarcode.Text = "为册条码号增加随机后缀(以避免转入的册条码号和系统内重复)";
            this.checkBox_target_randomItemBarcode.UseVisualStyleBackColor = true;
            // 
            // tabPage_target
            // 
            this.tabPage_target.AutoScroll = true;
            this.tabPage_target.Controls.Add(this.textBox_target_dbNameList);
            this.tabPage_target.Controls.Add(this.label6);
            this.tabPage_target.Controls.Add(this.checkBox_target_dontChangeOperations);
            this.tabPage_target.Controls.Add(this.checkBox_target_suppressOperLog);
            this.tabPage_target.Controls.Add(this.checkBox_target_dontSearchDup);
            this.tabPage_target.Controls.Add(this.checkBox_target_restoreOldID);
            this.tabPage_target.Controls.Add(this.button_target_simulateImport);
            this.tabPage_target.Controls.Add(this.comboBox_target_targetBiblioDbName);
            this.tabPage_target.Controls.Add(this.label1);
            this.tabPage_target.Location = new System.Drawing.Point(4, 22);
            this.tabPage_target.Name = "tabPage_target";
            this.tabPage_target.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_target.Size = new System.Drawing.Size(435, 257);
            this.tabPage_target.TabIndex = 0;
            this.tabPage_target.Text = "目标库";
            this.tabPage_target.UseVisualStyleBackColor = true;
            // 
            // checkBox_target_dontChangeOperations
            // 
            this.checkBox_target_dontChangeOperations.AutoSize = true;
            this.checkBox_target_dontChangeOperations.Location = new System.Drawing.Point(9, 144);
            this.checkBox_target_dontChangeOperations.Name = "checkBox_target_dontChangeOperations";
            this.checkBox_target_dontChangeOperations.Size = new System.Drawing.Size(150, 16);
            this.checkBox_target_dontChangeOperations.TabIndex = 9;
            this.checkBox_target_dontChangeOperations.Text = "不修改 operation 元素";
            this.checkBox_target_dontChangeOperations.UseVisualStyleBackColor = true;
            // 
            // checkBox_target_suppressOperLog
            // 
            this.checkBox_target_suppressOperLog.AutoSize = true;
            this.checkBox_target_suppressOperLog.Location = new System.Drawing.Point(9, 122);
            this.checkBox_target_suppressOperLog.Name = "checkBox_target_suppressOperLog";
            this.checkBox_target_suppressOperLog.Size = new System.Drawing.Size(108, 16);
            this.checkBox_target_suppressOperLog.TabIndex = 8;
            this.checkBox_target_suppressOperLog.Text = "不写入操作日志";
            this.checkBox_target_suppressOperLog.UseVisualStyleBackColor = true;
            // 
            // checkBox_target_dontSearchDup
            // 
            this.checkBox_target_dontSearchDup.AutoSize = true;
            this.checkBox_target_dontSearchDup.Location = new System.Drawing.Point(9, 100);
            this.checkBox_target_dontSearchDup.Name = "checkBox_target_dontSearchDup";
            this.checkBox_target_dontSearchDup.Size = new System.Drawing.Size(60, 16);
            this.checkBox_target_dontSearchDup.TabIndex = 7;
            this.checkBox_target_dontSearchDup.Text = "不查重";
            this.checkBox_target_dontSearchDup.UseVisualStyleBackColor = true;
            // 
            // checkBox_target_restoreOldID
            // 
            this.checkBox_target_restoreOldID.AutoSize = true;
            this.checkBox_target_restoreOldID.Location = new System.Drawing.Point(9, 64);
            this.checkBox_target_restoreOldID.Name = "checkBox_target_restoreOldID";
            this.checkBox_target_restoreOldID.Size = new System.Drawing.Size(138, 16);
            this.checkBox_target_restoreOldID.TabIndex = 6;
            this.checkBox_target_restoreOldID.Text = "恢复到原先的记录 ID";
            this.checkBox_target_restoreOldID.UseVisualStyleBackColor = true;
            // 
            // button_target_simulateImport
            // 
            this.button_target_simulateImport.Location = new System.Drawing.Point(9, 209);
            this.button_target_simulateImport.Name = "button_target_simulateImport";
            this.button_target_simulateImport.Size = new System.Drawing.Size(113, 23);
            this.button_target_simulateImport.TabIndex = 5;
            this.button_target_simulateImport.Text = "模拟导入";
            this.button_target_simulateImport.UseVisualStyleBackColor = true;
            this.button_target_simulateImport.Click += new System.EventHandler(this.button_target_simulateImport_Click);
            // 
            // comboBox_target_targetBiblioDbName
            // 
            this.comboBox_target_targetBiblioDbName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_target_targetBiblioDbName.FormattingEnabled = true;
            this.comboBox_target_targetBiblioDbName.Location = new System.Drawing.Point(126, 22);
            this.comboBox_target_targetBiblioDbName.Name = "comboBox_target_targetBiblioDbName";
            this.comboBox_target_targetBiblioDbName.Size = new System.Drawing.Size(182, 20);
            this.comboBox_target_targetBiblioDbName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "目标书目库名(&B):";
            // 
            // tabPage_run
            // 
            this.tabPage_run.Controls.Add(this.webBrowser1);
            this.tabPage_run.Location = new System.Drawing.Point(4, 22);
            this.tabPage_run.Name = "tabPage_run";
            this.tabPage_run.Size = new System.Drawing.Size(435, 257);
            this.tabPage_run.TabIndex = 2;
            this.tabPage_run.Text = "导入";
            this.tabPage_run.UseVisualStyleBackColor = true;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(435, 257);
            this.webBrowser1.TabIndex = 0;
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(369, 301);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(83, 22);
            this.button_next.TabIndex = 2;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(243, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(143, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "自动选择目标数据库顺序:";
            // 
            // textBox_target_dbNameList
            // 
            this.textBox_target_dbNameList.AcceptsReturn = true;
            this.textBox_target_dbNameList.Location = new System.Drawing.Point(245, 83);
            this.textBox_target_dbNameList.Multiline = true;
            this.textBox_target_dbNameList.Name = "textBox_target_dbNameList";
            this.textBox_target_dbNameList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_target_dbNameList.Size = new System.Drawing.Size(176, 149);
            this.textBox_target_dbNameList.TabIndex = 11;
            // 
            // ImportExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 334);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Name = "ImportExportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "从书目转储文件导入";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportExportForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImportExportForm_FormClosed);
            this.Load += new System.EventHandler(this.ImportExportForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_source.ResumeLayout(false);
            this.tabPage_source.PerformLayout();
            this.tabPage_convert.ResumeLayout(false);
            this.tabPage_convert.PerformLayout();
            this.tabPage_target.ResumeLayout(false);
            this.tabPage_target.PerformLayout();
            this.tabPage_run.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_target;
        private System.Windows.Forms.TabPage tabPage_source;
        private System.Windows.Forms.ComboBox comboBox_target_targetBiblioDbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_subRecords_object;
        private System.Windows.Forms.CheckBox checkBox_subRecords_comment;
        private System.Windows.Forms.CheckBox checkBox_subRecords_issue;
        private System.Windows.Forms.CheckBox checkBox_subRecords_order;
        private System.Windows.Forms.CheckBox checkBox_subRecords_entity;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_source_findFileName;
        private System.Windows.Forms.TextBox textBox_source_fileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getObjectDirectoryName;
        private System.Windows.Forms.TextBox textBox_objectDirectoryName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_run;
        private System.Windows.Forms.Button button_target_simulateImport;
        private System.Windows.Forms.TabPage tabPage_convert;
        private System.Windows.Forms.Button button_convert_initialMapString;
        private System.Windows.Forms.CheckBox checkBox_target_newRefID;
        private System.Windows.Forms.CheckBox checkBox_target_randomItemBarcode;
        private System.Windows.Forms.Panel panel_map;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.CheckBox checkBox_convert_addBiblioToItem;
        private System.Windows.Forms.CheckBox checkBox_target_restoreOldID;
        private System.Windows.Forms.CheckBox checkBox_target_dontSearchDup;
        private System.Windows.Forms.CheckBox checkBox_target_dontChangeOperations;
        private System.Windows.Forms.CheckBox checkBox_target_suppressOperLog;
        private System.Windows.Forms.TextBox textBox_source_range;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_convert_itemBatchNo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_target_dbNameList;
        private System.Windows.Forms.Label label6;
    }
}