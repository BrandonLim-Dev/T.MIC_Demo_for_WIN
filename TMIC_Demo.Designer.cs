
namespace T.MIC_Demo_for_WIN
{
    partial class TMIC_Demo
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbMICList = new System.Windows.Forms.ComboBox();
            this.cbProtocol = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbIPAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConnection = new System.Windows.Forms.Button();
            this.btnMute = new System.Windows.Forms.Button();
            this.statusMngTimer = new System.Windows.Forms.Timer(this.components);
            this.lbLog = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbSttText = new System.Windows.Forms.ListBox();
            this.mtbPortNumber = new System.Windows.Forms.MaskedTextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mtbPortNumber);
            this.groupBox1.Controls.Add(this.cbMICList);
            this.groupBox1.Controls.Add(this.cbProtocol);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tbIPAddress);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(432, 82);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "접속정보 및 마이크 정보";
            // 
            // cbMICList
            // 
            this.cbMICList.FormattingEnabled = true;
            this.cbMICList.Location = new System.Drawing.Point(281, 47);
            this.cbMICList.Name = "cbMICList";
            this.cbMICList.Size = new System.Drawing.Size(139, 20);
            this.cbMICList.TabIndex = 7;
            // 
            // cbProtocol
            // 
            this.cbProtocol.FormattingEnabled = true;
            this.cbProtocol.Items.AddRange(new object[] {
            "TCP/IP",
            "UDP"});
            this.cbProtocol.Location = new System.Drawing.Point(281, 21);
            this.cbProtocol.Name = "cbProtocol";
            this.cbProtocol.Size = new System.Drawing.Size(139, 20);
            this.cbProtocol.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(213, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "사용 MIC";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(214, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Protocol";
            // 
            // tbIPAddress
            // 
            this.tbIPAddress.Location = new System.Drawing.Point(71, 20);
            this.tbIPAddress.MaxLength = 15;
            this.tbIPAddress.Name = "tbIPAddress";
            this.tbIPAddress.Size = new System.Drawing.Size(128, 21);
            this.tbIPAddress.TabIndex = 2;
            this.tbIPAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbIPAddress_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Port 번호";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP 주소";
            // 
            // btnConnection
            // 
            this.btnConnection.BackColor = System.Drawing.Color.LightPink;
            this.btnConnection.Location = new System.Drawing.Point(450, 17);
            this.btnConnection.Name = "btnConnection";
            this.btnConnection.Size = new System.Drawing.Size(120, 77);
            this.btnConnection.TabIndex = 1;
            this.btnConnection.Text = "연결 요청";
            this.btnConnection.UseVisualStyleBackColor = false;
            this.btnConnection.Click += new System.EventHandler(this.btnConnection_Click);
            // 
            // btnMute
            // 
            this.btnMute.BackColor = System.Drawing.Color.LightPink;
            this.btnMute.Location = new System.Drawing.Point(576, 17);
            this.btnMute.Name = "btnMute";
            this.btnMute.Size = new System.Drawing.Size(120, 77);
            this.btnMute.TabIndex = 2;
            this.btnMute.Text = "마이크 꺼짐";
            this.btnMute.UseVisualStyleBackColor = false;
            this.btnMute.Click += new System.EventHandler(this.btnMute_Click);
            // 
            // statusMngTimer
            // 
            this.statusMngTimer.Enabled = true;
            this.statusMngTimer.Tick += new System.EventHandler(this.statusMngTimer_Tick);
            // 
            // lbLog
            // 
            this.lbLog.FormattingEnabled = true;
            this.lbLog.HorizontalScrollbar = true;
            this.lbLog.ItemHeight = 12;
            this.lbLog.Location = new System.Drawing.Point(12, 420);
            this.lbLog.Name = "lbLog";
            this.lbLog.Size = new System.Drawing.Size(682, 124);
            this.lbLog.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbSttText);
            this.groupBox2.Location = new System.Drawing.Point(12, 100);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(682, 314);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "STT 변환 결과";
            // 
            // lbSttText
            // 
            this.lbSttText.FormattingEnabled = true;
            this.lbSttText.HorizontalScrollbar = true;
            this.lbSttText.ItemHeight = 12;
            this.lbSttText.Location = new System.Drawing.Point(8, 20);
            this.lbSttText.Name = "lbSttText";
            this.lbSttText.Size = new System.Drawing.Size(668, 280);
            this.lbSttText.TabIndex = 4;
            // 
            // mtbPortNumber
            // 
            this.mtbPortNumber.Location = new System.Drawing.Point(71, 46);
            this.mtbPortNumber.Mask = "99999";
            this.mtbPortNumber.Name = "mtbPortNumber";
            this.mtbPortNumber.Size = new System.Drawing.Size(128, 21);
            this.mtbPortNumber.TabIndex = 6;
            this.mtbPortNumber.ValidatingType = typeof(int);
            // 
            // TMIC_Demo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 556);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lbLog);
            this.Controls.Add(this.btnMute);
            this.Controls.Add(this.btnConnection);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TMIC_Demo";
            this.Text = "T.MIC Demo v0.9";
            this.Load += new System.EventHandler(this.TMIC_Demo_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbMICList;
        private System.Windows.Forms.ComboBox cbProtocol;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbIPAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnection;
        private System.Windows.Forms.Button btnMute;
        private System.Windows.Forms.Timer statusMngTimer;
        private System.Windows.Forms.ListBox lbLog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lbSttText;
        private System.Windows.Forms.MaskedTextBox mtbPortNumber;
    }
}

