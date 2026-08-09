using TranslatorTrayApp.History;
using TranslatorTrayApp.Translation;

namespace TranslatorTrayApp.UI;

internal sealed class OverlayForm : Form
{
    private readonly TranslationService _translationService;
    private readonly TranslationHistoryStore _historyStore;

    private readonly TextBox _input;
    private readonly TextBox _output;
    private readonly Label _status;
    private readonly Button _copyButton;

    private CancellationTokenSource? _requestCts;
    private long _requestVersion;

    public OverlayForm(TranslationService translationService, TranslationHistoryStore historyStore)
    {
        _translationService = translationService;
        _historyStore = historyStore;

        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.Manual;
        Width = AppConfig.OverlayWidth;
        Height = AppConfig.OverlayHeight;
        Text = "Tradução rápida";

        _input = new TextBox
        {
            Left = 12,
            Top = 12,
            Width = ClientSize.Width - 24,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 10f)
        };

        _status = new Label
        {
            Left = 12,
            Top = 44,
            Width = ClientSize.Width - 24,
            Height = 18,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = "",
            ForeColor = Color.DimGray
        };

        _output = new TextBox
        {
            Left = 12,
            Top = 66,
            Width = ClientSize.Width - 100,
            Height = 74,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 10f)
        };

        _copyButton = new Button
        {
            Left = ClientSize.Width - 80,
            Top = 66,
            Width = 68,
            Height = 28,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Text = "Copiar"
        };

        Controls.Add(_input);
        Controls.Add(_status);
        Controls.Add(_output);
        Controls.Add(_copyButton);

        _copyButton.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_output.Text))
                Clipboard.SetText(_output.Text);
        };

        _input.TextChanged += OnInputTextChanged;
        _input.KeyDown += OnInputKeyDown;
        KeyDown += OnFormKeyDown;

        Deactivate += (_, _) => HideOverlay();
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

    public void SetInputAndTranslate(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
            return;

        if (!string.Equals(_input.Text, normalized, StringComparison.Ordinal))
            _input.Text = normalized;

        FocusInput(selectAll: true);
        RequestTranslation(immediate: true);
    }

    private void CenterOnPrimaryScreen()
    {
        var area = Screen.PrimaryScreen?.WorkingArea ?? Screen.FromControl(this).WorkingArea;
        Left = area.Left + (area.Width - Width) / 2;
        Top = area.Top + (area.Height - Height) / 2;
    }

    private async void OnInputTextChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_input.Text))
        {
            CancelCurrentRequest();
            _output.Clear();
            _status.Text = "";
            return;
        }

        RequestTranslation(immediate: false);
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

            var result = await _translationService.TranslateAsync(text, AppConfig.TargetLanguage, cancellationToken);

            if (requestVersion != Interlocked.Read(ref _requestVersion))
                return;

            _output.Text = FormatResult(result);
            _status.Text = string.Empty;
            _historyStore.Add(text, result.TranslatedText);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancelamentos esperados.
        }
        catch
        {
            if (requestVersion == Interlocked.Read(ref _requestVersion))
                _status.Text = "Falha ao traduzir.";
        }
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
