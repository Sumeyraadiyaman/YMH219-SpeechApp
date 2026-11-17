using System.Speech.Synthesis;

namespace YMH219_SpeechApp
{
    public partial class Form1 : Form
    {
        // Metni sese çeviren Windows bileşeni
        private readonly SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        // Açılır menüdeki seslerin gerçek adları (menüde dil koduyla birlikte görünüyorlar)
        private readonly List<string> voiceNames = new List<string>();
        private bool hasVoices = false;

        // Son vurgulanan kelimenin yeri (vurguyu kaldırmak için)
        private int lastHighlightStart = -1;
        private int lastHighlightLength = 0;

        public Form1()
        {
            InitializeComponent();

            // Buton olayları
            btnSpeak.Click += BtnSpeak_Click;
            btnPause.Click += BtnPause_Click;
            btnStopSpeak.Click += BtnStopSpeak_Click;
            btnClearTts.Click += BtnClearTts_Click;
            trkRate.ValueChanged += TrkRate_ValueChanged;

            // Konuşma motorunun olayları
            synthesizer.StateChanged += Synthesizer_StateChanged;
            synthesizer.SpeakProgress += Synthesizer_SpeakProgress;
            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;

            FormClosing += Form1_FormClosing;

            LoadVoices();
            UpdateTtsControls();
        }

        #region Text-to-Speech

        // Bilgisayarda yüklü sesleri açılır menüye doldurur
        private void LoadVoices()
        {
            try
            {
                foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
                {
                    if (!voice.Enabled)
                        continue;

                    VoiceInfo info = voice.VoiceInfo;
                    voiceNames.Add(info.Name);
                    cmbVoices.Items.Add($"{info.Name} ({info.Culture.Name})");

                    // Türkçe ses varsa onu varsayılan olarak seç
                    if (info.Culture.Name == "tr-TR" && cmbVoices.SelectedIndex == -1)
                        cmbVoices.SelectedIndex = cmbVoices.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                ShowError("Sesler yüklenemedi", ex);
            }

            hasVoices = voiceNames.Count > 0;

            if (!hasVoices)
            {
                cmbVoices.Items.Add("Yüklü ses bulunamadı");
                cmbVoices.SelectedIndex = 0;
            }
            else if (cmbVoices.SelectedIndex == -1)
            {
                cmbVoices.SelectedIndex = 0; // Türkçe yoksa ilk sesi seç
            }
        }

        private void BtnSpeak_Click(object? sender, EventArgs e)
        {
            string text = txtTts.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Lütfen okunacak bir metin yazın.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                StopSpeaking();
                synthesizer.SelectVoice(voiceNames[cmbVoices.SelectedIndex]);
                synthesizer.Rate = trkRate.Value;
                synthesizer.SetOutputToDefaultAudioDevice();
                synthesizer.SpeakAsync(text); // Arka planda okur, arayüz donmaz
            }
            catch (Exception ex)
            {
                ShowError("Metin okunamadı. Hoparlörünüzü veya ses aygıtınızı kontrol edin", ex);
            }
        }

        // Duraklat / Devam Et (aynı buton iki işi yapıyor)
        private void BtnPause_Click(object? sender, EventArgs e)
        {
            if (synthesizer.State == SynthesizerState.Speaking)
                synthesizer.Pause();
            else if (synthesizer.State == SynthesizerState.Paused)
                synthesizer.Resume();
        }

        private void BtnStopSpeak_Click(object? sender, EventArgs e)
        {
            StopSpeaking();
        }

        private void BtnClearTts_Click(object? sender, EventArgs e)
        {
            StopSpeaking();
            txtTts.Clear();
        }

        private void TrkRate_ValueChanged(object? sender, EventArgs e)
        {
            lblRateValue.Text = trkRate.Value.ToString();
            synthesizer.Rate = trkRate.Value;
        }

        // Okumayı tamamen durdurur
        private void StopSpeaking()
        {
            // Duraklatılmış okuma iptal edilemiyor, önce devam ettirmek gerekiyor
            if (synthesizer.State == SynthesizerState.Paused)
                synthesizer.Resume();

            synthesizer.SpeakAsyncCancelAll();
        }

        // Motorun durumu değişince (okuyor / duraklatıldı / hazır) arayüzü güncelle
        private void Synthesizer_StateChanged(object? sender, StateChangedEventArgs e)
        {
            RunOnUi(UpdateTtsControls);
        }

        // Her kelime okunmaya başladığında çalışır
        private void Synthesizer_SpeakProgress(object? sender, SpeakProgressEventArgs e)
        {
            RunOnUi(() => HighlightWord(e.CharacterPosition, e.CharacterCount));
        }

        // Okuma bittiğinde veya durdurulduğunda
        private void Synthesizer_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            RunOnUi(ClearHighlight);
        }

        // Butonları ve durum yazısını motorun durumuna göre ayarlar
        private void UpdateTtsControls()
        {
            SynthesizerState state = synthesizer.State;
            bool busy = state != SynthesizerState.Ready;

            btnSpeak.Enabled = hasVoices && !busy;
            btnPause.Enabled = busy;
            btnStopSpeak.Enabled = busy;
            btnPause.Text = state == SynthesizerState.Paused ? "Devam Et" : "Duraklat";
            cmbVoices.Enabled = hasVoices && !busy;
            txtTts.ReadOnly = busy; // Okurken metin değişmesin, vurgu kaymasın

            if (state == SynthesizerState.Speaking)
                SetStatus("Metin okunuyor...", Color.Green);
            else if (state == SynthesizerState.Paused)
                SetStatus("Okuma duraklatıldı", Color.DarkOrange);
            else
                SetStatus(hasVoices ? "Hazır" : "Bu bilgisayarda konuşma sesi bulunamadı",
                          hasVoices ? Color.Black : Color.Red);
        }

        // Okunan kelimeyi pembe ile vurgular
        private void HighlightWord(int start, int length)
        {
            ClearHighlight();

            if (start < 0 || start + length > txtTts.TextLength)
                return;

            txtTts.Select(start, length);
            txtTts.SelectionBackColor = Color.FromArgb(255, 200, 220);
            txtTts.Select(start + length, 0); // Seçimi kaldır, imleci kelimenin sonuna koy

            lastHighlightStart = start;
            lastHighlightLength = length;
        }

        // Önceki vurguyu kaldırır
        private void ClearHighlight()
        {
            if (lastHighlightStart >= 0 && lastHighlightStart + lastHighlightLength <= txtTts.TextLength)
            {
                txtTts.Select(lastHighlightStart, lastHighlightLength);
                txtTts.SelectionBackColor = txtTts.BackColor;
                txtTts.Select(lastHighlightStart + lastHighlightLength, 0);
            }

            lastHighlightStart = -1;
        }

        #endregion

        #region Yardımcı metotlar

        // Arayüzü değiştiren kodu güvenli şekilde arayüz iş parçacığında çalıştırır
        private void RunOnUi(Action action)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = "Durum: " + text;
            lblStatus.ForeColor = color;
        }

        private void ShowError(string message, Exception ex)
        {
            MessageBox.Show($"{message}.\n\nAyrıntı: {ex.Message}", "Hata",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus(message, Color.Red);
        }

        // Pencere kapanırken konuşma motorunu kapat ve kaynaklarını serbest bırak
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            synthesizer.StateChanged -= Synthesizer_StateChanged;
            synthesizer.SpeakProgress -= Synthesizer_SpeakProgress;
            synthesizer.SpeakCompleted -= Synthesizer_SpeakCompleted;

            StopSpeaking();
            synthesizer.Dispose();
        }

        #endregion
    }
}