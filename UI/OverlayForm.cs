using TranslatorTrayApp.Translation;

namespace TranslatorTrayApp.UI;

internal sealed class OverlayForm : Form
{
    private readonly TranslationService _translationService;

    private readonly ComboBox _sourceLanguage;
    private readonly ComboBox _targetLanguage;
    private readonly TextBox _input;
    private readonly TextBox _output;
    private readonly Label _status;
    private readonly Button _copyButton;
    private readonly Button _clearButton;
    private readonly Button _clearAllButton;
    private readonly Label _copyInfo;
    private readonly ToolTip _copyToolTip;

    private CancellationTokenSource? _requestCts;
    private long _requestVersion;
    private string _lastTranslatedText = string.Empty;

    public OverlayForm(TranslationService translationService)
    {
        _translationService = translationService;

        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(AppConfig.OverlayWidth, AppConfig.OverlayHeight);
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.Manual;
        Width = AppConfig.OverlayWidth;
        Height = AppConfig.OverlayHeight;
        Text = "Tradução rápida";

        var sourceLabel = new Label { Left = 12, Top = 14, Width = 24, Height = 22, Text = "De:" };
        _sourceLanguage = CreateLanguageSelector(38, AppConfig.DefaultSourceLanguage);

        var targetLabel = new Label { Left = 274, Top = 14, Width = 36, Height = 22, Text = "Para:" };
        _targetLanguage = CreateLanguageSelector(314, AppConfig.DefaultTargetLanguage);

        _input = new TextBox
        {
            Left = 12,
            Top = 48,
            Width = ClientSize.Width - 24,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 10f)
        };

        _status = new Label
        {
            Left = 12,
            Top = 80,
            Width = ClientSize.Width - 24,
            Height = 36,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = "",
            ForeColor = Color.DimGray
        };

        _output = new TextBox
        {
            Left = 12,
            Top = 120,
            Width = ClientSize.Width - 24,
            Height = 56,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 10f)
        };

        _copyButton = new Button
        {
            Left = 12,
            Top = 188,
            Width = 76,
            Height = 28,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Text = "Copiar"
        };

        _clearButton = new Button
        {
            Left = 96,
            Top = 188,
            Width = 76,
            Height = 28,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Text = "clear"
        };

        _clearAllButton = new Button
        {
            Left = 180,
            Top = 188,
            Width = 96,
            Height = 28,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Text = "clear all"
        };

        _copyInfo = new Label
        {
            Left = 284,
            Top = 188,
            Width = 24,
            Height = 28,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Text = "ⓘ",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Symbol", 12f, FontStyle.Bold),
            ForeColor = Color.DimGray,
            Cursor = Cursors.Help
        };

        _copyToolTip = new ToolTip
        {
            AutomaticDelay = 0,
            AutoPopDelay = 10000,
            InitialDelay = 0,
            ReshowDelay = 0,
            ShowAlways = true
        };
        _copyToolTip.SetToolTip(_copyInfo, "Copiar envia somente a frase traduzida para a área de transferência.");

        Controls.Add(sourceLabel);
        Controls.Add(_sourceLanguage);
        Controls.Add(targetLabel);
        Controls.Add(_targetLanguage);
        Controls.Add(_input);
        Controls.Add(_status);
        Controls.Add(_output);
        Controls.Add(_copyButton);
        Controls.Add(_clearButton);
        Controls.Add(_clearAllButton);
        Controls.Add(_copyInfo);

        _copyButton.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_lastTranslatedText))
                Clipboard.SetText(_lastTranslatedText);
        };

        _clearButton.Click += (_, _) => ClearTranslation();
        _clearAllButton.Click += (_, _) => ClearAll();
        _copyInfo.MouseEnter += (_, _) => _copyToolTip.Show(
            "Copiar envia somente a frase traduzida para a área de transferência.",
            _copyInfo,
            _copyInfo.Width + 4,
            0,
            _copyToolTip.AutoPopDelay);
        _copyInfo.MouseLeave += (_, _) => _copyToolTip.Hide(_copyInfo);

        _input.TextChanged += OnInputTextChanged;
        _sourceLanguage.SelectedIndexChanged += OnLanguageChanged;
        _targetLanguage.SelectedIndexChanged += OnLanguageChanged;
        _input.KeyDown += OnInputKeyDown;
        KeyDown += OnFormKeyDown;

        FormClosing += OnFormClosing;
        Deactivate += (_, _) => HideOverlay();
    }

    private static ComboBox CreateLanguageSelector(int left, string selectedCode)
    {
        var selector = new ComboBox
        {
            Left = left,
            Top = 10,
            Width = 190,
            Height = 24,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        selector.Items.AddRange(AppConfig.Languages);
        selector.SelectedItem = AppConfig.Languages.First(language => language.Code == selectedCode);
        return selector;
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideOverlay();
        }
    }

    public void ShowOverlay()
    {
        CenterOnPrimaryScreen();
        if (!Visible)
            Show();

        BringToFront();
        Activate();
        FocusInput(selectAll: true);
    }

    public void FocusInput(bool selectAll)
    {
        _input.Focus();
        if (selectAll)
            _input.SelectAll();
    }

    private void CenterOnPrimaryScreen()
    {
        var area = Screen.PrimaryScreen?.WorkingArea ?? Screen.FromControl(this).WorkingArea;
        Left = area.Left + (area.Width - Width) / 2;
        Top = area.Top + (area.Height - Height) / 2;
    }

    private void OnInputTextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_input.Text))
        {
            CancelCurrentRequest();
            _output.Clear();
            _lastTranslatedText = string.Empty;
            _status.Text = "";
            return;
        }

        RequestTranslation(immediate: false);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_input.Text))
            RequestTranslation(immediate: true);
    }

    private void ClearTranslation()
    {
        CancelCurrentRequest();
        Interlocked.Increment(ref _requestVersion);
        _input.Clear();
        _output.Clear();
        _lastTranslatedText = string.Empty;
        _status.Text = string.Empty;
        _status.ForeColor = Color.DimGray;
        FocusInput(selectAll: false);
    }

    private void ClearAll()
    {
        ClearTranslation();
        SelectLanguage(_sourceLanguage, AppConfig.DefaultSourceLanguage);
        SelectLanguage(_targetLanguage, AppConfig.DefaultTargetLanguage);
    }

    private static void SelectLanguage(ComboBox selector, string languageCode)
    {
        var index = Array.FindIndex(AppConfig.Languages, language => language.Code == languageCode);
        if (index >= 0)
            selector.SelectedIndex = index;
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            RequestTranslation(immediate: true);
        }
        else if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            HideOverlay();
        }
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true;
            HideOverlay();
        }
    }

    private void RequestTranslation(bool immediate)
    {
        var text = _input.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        CancelCurrentRequest();

        _status.Text = "Translating...";
        _status.ForeColor = Color.DimGray;
        _output.Clear();
        _lastTranslatedText = string.Empty;

        var cts = new CancellationTokenSource();
        _requestCts = cts;
        var requestVersion = Interlocked.Increment(ref _requestVersion);

        _ = TranslatePipelineAsync(text, immediate ? 0 : AppConfig.AutoTranslateDebounceMs, requestVersion, cts.Token);
    }

    private async Task TranslatePipelineAsync(string text, int delayMs, long requestVersion, CancellationToken cancellationToken)
    {
        try
        {
            if (delayMs > 0)
                await Task.Delay(delayMs, cancellationToken);

            var sourceLanguage = ((AppConfig.LanguageOption)_sourceLanguage.SelectedItem!).Code;
            var targetLanguage = ((AppConfig.LanguageOption)_targetLanguage.SelectedItem!).Code;
            var result = await _translationService.TranslateAsync(
                text,
                sourceLanguage,
                targetLanguage,
                cancellationToken);

            if (requestVersion != Interlocked.Read(ref _requestVersion))
                return;

            _lastTranslatedText = result.TranslatedText;
            _output.Text = FormatResult(result);
            _status.Text = string.Empty;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ignore cancelamentos esperados.
        }
        catch (Exception ex)
        {
            if (requestVersion == Interlocked.Read(ref _requestVersion))
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = "Falha ao traduzir. Veja os detalhes abaixo.";
                _output.Text = FormatFailure(ex);
                _lastTranslatedText = string.Empty;
            }
        }
    }

    private static string FormatFailure(Exception exception)
    {
        var details = $"Falha: {exception.GetType().Name}: {exception.Message}";
        if (exception.InnerException is not null)
            details += $" | Interno: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}";

        return details;
    }

    private static string FormatResult(TranslationResult result)
    {
        if (string.IsNullOrWhiteSpace(result.Context) && string.IsNullOrWhiteSpace(result.Example))
            return $"Tradução: {result.TranslatedText}";

        return $"Tradução: {result.TranslatedText}{Environment.NewLine}"
            + $"Contexto: {result.Context}{Environment.NewLine}"
            + $"Exemplo: {result.Example}";
    }

    private void CancelCurrentRequest()
    {
        _requestCts?.Cancel();
        _requestCts?.Dispose();
        _requestCts = null;
    }

    private void HideOverlay()
    {
        CancelCurrentRequest();
        Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            CancelCurrentRequest();

        base.Dispose(disposing);
    }
}
