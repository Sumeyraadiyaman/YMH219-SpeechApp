using System.Speech.Synthesis;
using System.Text.Json;
using NAudio.Wave;
using Vosk;

namespace YMH219_SpeechApp
{
    public partial class Form1 : Form
    {
        // ---------- Text-to-Speech ----------
        private readonly SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private readonly List<string> voiceNames = new List<string>();
        private bool hasVoices = false;

        // Son vurgulanan kelimenin yeri (vurguyu kaldırmak için)
        private int lastHighlightStart = -1;
        private int lastHighlightLength = 0;

        // ---------- Speech-to-Text (Vosk) ----------
        private const string ModelFolderName = "vosk-model-small-tr-0.3";
        private Model? voskModel;                    // Türkçe dil modeli (bir kez yüklenir)
        private VoskRecognizer? voskRecognizer;      // Dinlerken oluşturulur
        private WaveIn? microphone;                  // Mikrofondan ses alan NAudio nesnesi
        private readonly object recognizerLock = new object();
        private bool isModelLoading = false;
        private bool modelLoadFailed = false;
        private bool isListening = false;

        public Form1()
        {
            InitializeComponent();
            ApplyTheme(); // Renkleri ve buton stillerini uygula

            // Metin kutularının zemini her zaman beyaz kalsın (salt okunurken de)
            txtTts.BackColor = Color.White;
            txtStt.BackColor = Color.White;

            // TTS buton olayları
            btnSpeak.Click += BtnSpeak_Click;
            btnPause.Click += BtnPause_Click;
            btnStopSpeak.Click += BtnStopSpeak_Click;
            btnClearTts.Click += BtnClearTts_Click;
            trkRate.ValueChanged += TrkRate_ValueChanged;

            // TTS motor olayları
            synthesizer.StateChanged += Synthesizer_StateChanged;
            synthesizer.SpeakProgress += Synthesizer_SpeakProgress;
            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;

            // STT buton olayları
            btnListen.Click += BtnListen_Click;
            btnStopListen.Click += BtnStopListen_Click;
            btnSaveStt.Click += BtnSaveStt_Click;
            btnClearStt.Click += BtnClearStt_Click;

            // Dil menüsü: şimdilik tek seçenek
            cmbRecognizers.Items.Add("Türkçe (Vosk - çevrimdışı)");
            cmbRecognizers.SelectedIndex = 0;
            cmbRecognizers.Enabled = false;

            Shown += Form1_Shown;           // Pencere ekranda göründükten sonra
            FormClosing += Form1_FormClosing;

            LoadVoices();
            UpdateTtsControls();
            UpdateSttControls();
        }

        // Pencere açıldıktan sonra modeli arka planda yükle (pencere donmasın)
        private async void Form1_Shown(object? sender, EventArgs e)
        {
            await LoadVoskModelAsync();
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

        private void Synthesizer_StateChanged(object? sender, System.Speech.Synthesis.StateChangedEventArgs e)
        {
            RunOnUi(UpdateTtsControls);
        }

        private void Synthesizer_SpeakProgress(object? sender, SpeakProgressEventArgs e)
        {
            RunOnUi(() => HighlightWord(e.CharacterPosition, e.CharacterCount));
        }

        private void Synthesizer_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            RunOnUi(ClearHighlight);
        }

        // TTS butonlarını motorun durumuna göre ayarlar
        private void UpdateTtsControls()
        {
            bool busy = synthesizer.State != SynthesizerState.Ready;

            btnSpeak.Enabled = hasVoices && !busy;
            btnPause.Enabled = busy;
            btnStopSpeak.Enabled = busy;
            btnPause.Text = synthesizer.State == SynthesizerState.Paused ? "Devam Et" : "Duraklat";
            cmbVoices.Enabled = hasVoices && !busy;
            txtTts.ReadOnly = busy; // Okurken metin değişmesin, vurgu kaymasın

            RefreshStatus();
        }

        // Okunan kelimeyi pembe ile vurgular
        private void HighlightWord(int start, int length)
        {
            ClearHighlight();

            if (start < 0 || start + length > txtTts.TextLength)
                return;

            txtTts.Select(start, length);
            txtTts.SelectionBackColor = Color.FromArgb(255, 200, 220);
            txtTts.Select(start + length, 0);

            lastHighlightStart = start;
            lastHighlightLength = length;
        }

        // Önceki vurguyu kaldırır
        private void ClearHighlight()
        {
            if (lastHighlightStart >= 0 && lastHighlightStart + lastHighlightLength <= txtTts.TextLength)
            {
                txtTts.Select(lastHighlightStart, lastHighlightLength);
                txtTts.SelectionBackColor = Color.White;
                txtTts.Select(lastHighlightStart + lastHighlightLength, 0);
            }

            lastHighlightStart = -1;
        }

        #endregion

        #region Speech-to-Text (Vosk)

        // Türkçe modeli diskten yükler (birkaç saniye sürebilir)
        private async Task LoadVoskModelAsync()
        {
            string modelPath = Path.Combine(Application.StartupPath, "Models", ModelFolderName);

            if (!Directory.Exists(modelPath))
            {
                modelLoadFailed = true;
                UpdateSttControls();
                MessageBox.Show($"Konuşma modeli bulunamadı:\n{modelPath}\n\nModeli README'deki adımlarla indirip Models klasörüne koyun.",
                    "Model bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isModelLoading = true;
            UpdateSttControls();

            try
            {
                Vosk.Vosk.SetLogLevel(-1); // Vosk'un konsola yazdığı teknik mesajları kapat
                voskModel = await Task.Run(() => new Model(modelPath));
            }
            catch (Exception ex)
            {
                modelLoadFailed = true;
                ShowError("Konuşma modeli yüklenemedi", ex);
            }
            finally
            {
                isModelLoading = false;
                UpdateSttControls();
            }
        }

        private void BtnListen_Click(object? sender, EventArgs e)
        {
            if (voskModel == null)
                return;

            try
            {
                lock (recognizerLock)
                {
                    voskRecognizer = new VoskRecognizer(voskModel, 16000.0f);
                }

                // Vosk'un beklediği ses biçimi: saniyede 16000 örnek, 16 bit, tek kanal (mono)
                microphone = new WaveIn
                {
                    WaveFormat = new WaveFormat(16000, 16, 1),
                    BufferMilliseconds = 100
                };
                microphone.DataAvailable += Microphone_DataAvailable;
                microphone.RecordingStopped += Microphone_RecordingStopped;
                microphone.StartRecording(); // Mikrofon yoksa veya izin kapalıysa burada hata verir

                isListening = true;
                UpdateSttControls();
            }
            catch (Exception ex)
            {
                CleanupRecognizer();
                ShowError("Mikrofona erişilemedi. Mikrofonunuzun bağlı ve gizlilik ayarlarında izinli olduğundan emin olun", ex);
            }
        }

        private void BtnStopListen_Click(object? sender, EventArgs e)
        {
            StopListening();
        }

        // Mikrofondan her 100 ms'lik ses parçası geldiğinde çalışır (arka plan iş parçacığında)
        private void Microphone_DataAvailable(object? sender, WaveInEventArgs e)
        {
            string? finalText = null;
            string? partialText = null;

            lock (recognizerLock)
            {
                if (voskRecognizer == null)
                    return;

                // AcceptWaveform true dönerse bir cümle tamamlanmıştır
                if (voskRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                    finalText = ReadJson(voskRecognizer.Result(), "text");
                else
                    partialText = ReadJson(voskRecognizer.PartialResult(), "partial");
            }

            if (!string.IsNullOrWhiteSpace(finalText))
            {
                string text = finalText;
                RunOnUi(() => AppendResult(text));
            }
            else if (!string.IsNullOrWhiteSpace(partialText))
            {
                string guess = partialText;
                RunOnUi(() => SetStatus($"Dinleniyor... (duyulan: {guess})", Color.FromArgb(156, 39, 176)));
            }
        }

        // Kayıt beklenmedik şekilde durursa (ör. mikrofon çıkarıldı)
        private void Microphone_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception == null)
                return;

            Exception error = e.Exception;
            RunOnUi(() =>
            {
                StopListening();
                ShowError("Mikrofon bağlantısı kesildi", error);
            });
        }

        // Tanınan cümleyi zaman damgasıyla metin kutusuna ekler
        private void AppendResult(string text)
        {
            txtStt.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            txtStt.ScrollToCaret();
            RefreshStatus();
        }

        // Vosk sonuçları JSON biçiminde gelir: {"text" : "merhaba dünya"}
        private static string ReadJson(string json, string key)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(key, out JsonElement value))
                    return value.GetString() ?? "";
            }
            catch (JsonException)
            {
                // Bozuk sonuç gelirse yok say
            }

            return "";
        }

        // Dinlemeyi durdurur, yarım kalan son cümleyi de ekler
        private void StopListening()
        {
            string? lastText = CleanupRecognizer();
            isListening = false;

            if (!string.IsNullOrWhiteSpace(lastText))
                AppendResult(lastText);

            UpdateSttControls();
        }

        // Mikrofonu ve tanıyıcıyı kapatır; tanıyıcının elinde kalan son metni döndürür
        private string? CleanupRecognizer()
        {
            if (microphone != null)
            {
                microphone.DataAvailable -= Microphone_DataAvailable;
                microphone.RecordingStopped -= Microphone_RecordingStopped;

                try
                {
                    microphone.StopRecording();
                }
                catch
                {
                    // Zaten durmuşsa sorun değil
                }

                microphone.Dispose();
                microphone = null;
            }

            string? lastText = null;

            lock (recognizerLock)
            {
                if (voskRecognizer != null)
                {
                    lastText = ReadJson(voskRecognizer.FinalResult(), "text");
                    voskRecognizer.Dispose();
                    voskRecognizer = null;
                }
            }

            return lastText;
        }

        private void BtnSaveStt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStt.Text))
            {
                MessageBox.Show("Kaydedilecek metin yok.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Metin dosyası (*.txt)|*.txt";
            dialog.FileName = $"konusma_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(dialog.FileName, txtStt.Text);
                MessageBox.Show("Metin kaydedildi:\n" + dialog.FileName, "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Dosya kaydedilemedi", ex);
            }
        }

        private void BtnClearStt_Click(object? sender, EventArgs e)
        {
            txtStt.Clear();
        }

        // STT butonlarını duruma göre ayarlar
        private void UpdateSttControls()
        {
            btnListen.Enabled = voskModel != null && !isListening;
            btnStopListen.Enabled = isListening;

            RefreshStatus();
        }

        #endregion

        #region Görünüm

        // Pasif butonların rengi
        private static readonly Color DisabledColor = Color.FromArgb(215, 215, 215);

        // Pembe-mint temasını uygular
        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(253, 246, 249); // Çok açık pembe zemin

            // Bölüm başlıkları: sol pembe, sağ mint
            grpTts.ForeColor = Color.FromArgb(173, 20, 87);
            grpStt.ForeColor = Color.FromArgb(0, 121, 107);

            // Metin kutuları: koyu gri yazı, ince kenar
            txtTts.ForeColor = Color.FromArgb(55, 45, 50);
            txtStt.ForeColor = Color.FromArgb(55, 45, 50);
            txtTts.BorderStyle = BorderStyle.FixedSingle;
            txtStt.BorderStyle = BorderStyle.FixedSingle;

            // Açılır menüler düz görünümlü olsun
            cmbVoices.FlatStyle = FlatStyle.Flat;
            cmbRecognizers.FlatStyle = FlatStyle.Flat;

            // Kaydırma çubuğunun zemini formla aynı olsun
            trkRate.BackColor = BackColor;

            // TTS butonları (pembe tonlar)
            StyleButton(btnSpeak, Color.FromArgb(236, 64, 122));      // Pembe
            StyleButton(btnPause, Color.FromArgb(171, 130, 220));     // Lavanta
            StyleButton(btnStopSpeak, Color.FromArgb(239, 83, 80));   // Kırmızımsı
            StyleButton(btnClearTts, Color.FromArgb(176, 176, 176));  // Gri

            // STT butonları (mint tonlar)
            StyleButton(btnListen, Color.FromArgb(38, 166, 154));     // Mint
            StyleButton(btnStopListen, Color.FromArgb(239, 83, 80));  // Kırmızımsı
            StyleButton(btnSaveStt, Color.FromArgb(77, 182, 172));    // Açık mint
            StyleButton(btnClearStt, Color.FromArgb(176, 176, 176));  // Gri
        }

        // Bir butona düz, renkli görünüm verir; pasifken otomatik griye döner
        private void StyleButton(Button button, Color color)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            button.BackColor = button.Enabled ? color : DisabledColor;

            // Buton aktif/pasif olduğunda rengini kendiliğinden değiştirsin
            button.EnabledChanged += (sender, e) =>
            {
                button.BackColor = button.Enabled ? color : DisabledColor;
            };
        }

        #endregion
        #region Ortak

        // Durum yazısını okuma, dinleme ve model durumuna göre günceller
        private void RefreshStatus()
        {
            List<string> parts = new List<string>();
            Color color = Color.Black;

            if (isModelLoading)
            {
                parts.Add("Türkçe konuşma modeli yükleniyor...");
                color = Color.DimGray;
            }
            else if (modelLoadFailed)
            {
                parts.Add("Konuşma modeli yüklenemedi");
                color = Color.Red;
            }

            if (synthesizer.State == SynthesizerState.Speaking)
            {
                parts.Add("Metin okunuyor");
                color = Color.Green;
            }
            else if (synthesizer.State == SynthesizerState.Paused)
            {
                parts.Add("Okuma duraklatıldı");
                color = Color.DarkOrange;
            }

            if (isListening)
            {
                parts.Add("Mikrofon dinleniyor, konuşabilirsiniz");
                color = Color.FromArgb(156, 39, 176); // Mor
            }

            if (parts.Count == 0)
                SetStatus("Hazır", Color.Black);
            else
                SetStatus(string.Join("  |  ", parts), color);
        }

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

        // Pencere kapanırken her şeyi kapat ve kaynakları serbest bırak
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            synthesizer.StateChanged -= Synthesizer_StateChanged;
            synthesizer.SpeakProgress -= Synthesizer_SpeakProgress;
            synthesizer.SpeakCompleted -= Synthesizer_SpeakCompleted;

            StopSpeaking();
            synthesizer.Dispose();

            CleanupRecognizer();
            voskModel?.Dispose();
        }

        #endregion
    }
}