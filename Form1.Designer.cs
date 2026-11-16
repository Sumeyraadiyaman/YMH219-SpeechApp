namespace YMH219_SpeechApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            grpTts = new GroupBox();
            txtTts = new RichTextBox();
            lblVoice = new Label();
            cmbVoices = new ComboBox();
            lblRate = new Label();
            trkRate = new TrackBar();
            lblRateValue = new Label();
            btnSpeak = new Button();
            btnPause = new Button();
            btnStopSpeak = new Button();
            btnClearTts = new Button();
            grpStt = new GroupBox();
            lblLang = new Label();
            cmbRecognizers = new ComboBox();
            btnListen = new Button();
            btnStopListen = new Button();
            txtStt = new RichTextBox();
            btnSaveStt = new Button();
            btnClearStt = new Button();
            lblStatus = new Label();
            grpTts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkRate).BeginInit();
            grpStt.SuspendLayout();
            SuspendLayout();
            // 
            // grpTts
            // 
            grpTts.Controls.Add(txtTts);
            grpTts.Controls.Add(lblVoice);
            grpTts.Controls.Add(cmbVoices);
            grpTts.Controls.Add(lblRate);
            grpTts.Controls.Add(trkRate);
            grpTts.Controls.Add(lblRateValue);
            grpTts.Controls.Add(btnSpeak);
            grpTts.Controls.Add(btnPause);
            grpTts.Controls.Add(btnStopSpeak);
            grpTts.Controls.Add(btnClearTts);
            grpTts.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpTts.Location = new Point(12, 12);
            grpTts.Name = "grpTts";
            grpTts.Size = new Size(390, 440);
            grpTts.TabIndex = 0;
            grpTts.TabStop = false;
            grpTts.Text = "Metni Sese Çevir (Text-to-Speech)";
            // 
            // txtTts
            // 
            txtTts.Font = new Font("Segoe UI", 11F);
            txtTts.Location = new Point(15, 30);
            txtTts.Name = "txtTts";
            txtTts.Size = new Size(360, 170);
            txtTts.TabIndex = 0;
            txtTts.Text = "";
            // 
            // lblVoice
            // 
            lblVoice.AutoSize = true;
            lblVoice.Font = new Font("Segoe UI", 9.5F);
            lblVoice.Location = new Point(15, 215);
            lblVoice.Name = "lblVoice";
            lblVoice.TabIndex = 1;
            lblVoice.Text = "Ses:";
            // 
            // cmbVoices
            // 
            cmbVoices.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbVoices.Font = new Font("Segoe UI", 9.5F);
            cmbVoices.Location = new Point(60, 211);
            cmbVoices.Name = "cmbVoices";
            cmbVoices.Size = new Size(315, 25);
            cmbVoices.TabIndex = 2;
            // 
            // lblRate
            // 
            lblRate.AutoSize = true;
            lblRate.Font = new Font("Segoe UI", 9.5F);
            lblRate.Location = new Point(15, 257);
            lblRate.Name = "lblRate";
            lblRate.TabIndex = 3;
            lblRate.Text = "Hız:";
            // 
            // trkRate
            // 
            trkRate.Location = new Point(55, 250);
            trkRate.Maximum = 10;
            trkRate.Minimum = -10;
            trkRate.Name = "trkRate";
            trkRate.Size = new Size(270, 45);
            trkRate.TabIndex = 4;
            trkRate.TickFrequency = 2;
            trkRate.Value = 0;
            // 
            // lblRateValue
            // 
            lblRateValue.AutoSize = true;
            lblRateValue.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblRateValue.Location = new Point(335, 257);
            lblRateValue.Name = "lblRateValue";
            lblRateValue.TabIndex = 5;
            lblRateValue.Text = "0";
            // 
            // btnSpeak
            // 
            btnSpeak.Location = new Point(15, 305);
            btnSpeak.Name = "btnSpeak";
            btnSpeak.Size = new Size(175, 40);
            btnSpeak.TabIndex = 6;
            btnSpeak.Text = "▶ Oku";
            // 
            // btnPause
            // 
            btnPause.Location = new Point(200, 305);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(175, 40);
            btnPause.TabIndex = 7;
            btnPause.Text = "Duraklat";
            // 
            // btnStopSpeak
            // 
            btnStopSpeak.Location = new Point(15, 355);
            btnStopSpeak.Name = "btnStopSpeak";
            btnStopSpeak.Size = new Size(175, 40);
            btnStopSpeak.TabIndex = 8;
            btnStopSpeak.Text = "■ Durdur";
            // 
            // btnClearTts
            // 
            btnClearTts.Location = new Point(200, 355);
            btnClearTts.Name = "btnClearTts";
            btnClearTts.Size = new Size(175, 40);
            btnClearTts.TabIndex = 9;
            btnClearTts.Text = "Temizle";
            // 
            // grpStt
            // 
            grpStt.Controls.Add(lblLang);
            grpStt.Controls.Add(cmbRecognizers);
            grpStt.Controls.Add(btnListen);
            grpStt.Controls.Add(btnStopListen);
            grpStt.Controls.Add(txtStt);
            grpStt.Controls.Add(btnSaveStt);
            grpStt.Controls.Add(btnClearStt);
            grpStt.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpStt.Location = new Point(414, 12);
            grpStt.Name = "grpStt";
            grpStt.Size = new Size(390, 440);
            grpStt.TabIndex = 1;
            grpStt.TabStop = false;
            grpStt.Text = "Sesi Metne Çevir (Speech-to-Text)";
            // 
            // lblLang
            // 
            lblLang.AutoSize = true;
            lblLang.Font = new Font("Segoe UI", 9.5F);
            lblLang.Location = new Point(15, 35);
            lblLang.Name = "lblLang";
            lblLang.TabIndex = 0;
            lblLang.Text = "Dil:";
            // 
            // cmbRecognizers
            // 
            cmbRecognizers.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRecognizers.Font = new Font("Segoe UI", 9.5F);
            cmbRecognizers.Location = new Point(60, 31);
            cmbRecognizers.Name = "cmbRecognizers";
            cmbRecognizers.Size = new Size(315, 25);
            cmbRecognizers.TabIndex = 1;
            // 
            // btnListen
            // 
            btnListen.Location = new Point(15, 70);
            btnListen.Name = "btnListen";
            btnListen.Size = new Size(175, 40);
            btnListen.TabIndex = 2;
            btnListen.Text = "Dinlemeye Başla";
            // 
            // btnStopListen
            // 
            btnStopListen.Location = new Point(200, 70);
            btnStopListen.Name = "btnStopListen";
            btnStopListen.Size = new Size(175, 40);
            btnStopListen.TabIndex = 3;
            btnStopListen.Text = "Dinlemeyi Durdur";
            // 
            // txtStt
            // 
            txtStt.Font = new Font("Segoe UI", 11F);
            txtStt.Location = new Point(15, 125);
            txtStt.Name = "txtStt";
            txtStt.Size = new Size(360, 220);
            txtStt.TabIndex = 4;
            txtStt.Text = "";
            // 
            // btnSaveStt
            // 
            btnSaveStt.Location = new Point(15, 355);
            btnSaveStt.Name = "btnSaveStt";
            btnSaveStt.Size = new Size(175, 40);
            btnSaveStt.TabIndex = 5;
            btnSaveStt.Text = "Kaydet (.txt)";
            // 
            // btnClearStt
            // 
            btnClearStt.Location = new Point(200, 355);
            btnClearStt.Name = "btnClearStt";
            btnClearStt.Size = new Size(175, 40);
            btnClearStt.TabIndex = 6;
            btnClearStt.Text = "Temizle";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatus.Location = new Point(12, 465);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Durum: Hazır";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(818, 500);
            Controls.Add(grpTts);
            Controls.Add(grpStt);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Speech App";
            grpTts.ResumeLayout(false);
            grpTts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trkRate).EndInit();
            grpStt.ResumeLayout(false);
            grpStt.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpTts;
        private RichTextBox txtTts;
        private Label lblVoice;
        private ComboBox cmbVoices;
        private Label lblRate;
        private TrackBar trkRate;
        private Label lblRateValue;
        private Button btnSpeak;
        private Button btnPause;
        private Button btnStopSpeak;
        private Button btnClearTts;
        private GroupBox grpStt;
        private Label lblLang;
        private ComboBox cmbRecognizers;
        private Button btnListen;
        private Button btnStopListen;
        private RichTextBox txtStt;
        private Button btnSaveStt;
        private Button btnClearStt;
        private Label lblStatus;
    }
}