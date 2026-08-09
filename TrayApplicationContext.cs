using TranslatorTrayApp.History;
using TranslatorTrayApp.Translation;
using TranslatorTrayApp.UI;

namespace TranslatorTrayApp;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly HotkeyWindow _hotkeyWindow;
    private readonly OverlayForm _overlayForm;

    public TrayApplicationContext()
    {
        var translationService = new TranslationService();
        var historyStore = new TranslationHistoryStore();
        _overlayForm = new OverlayForm(translationService, historyStore);

        _hotkeyWindow = new HotkeyWindow(id: 1);
        _hotkeyWindow.HotkeyPressed += (_, _) => OnHotkeyPressed();

        if (!_hotkeyWindow.Register(AppConfig.HotkeyModifiers, AppConfig.HotkeyVirtualKey))
        {
            MessageBox.Show("Não foi possível registrar ALT + D.", "Tradutor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        var menu = new ContextMenuStrip();
        menu.Items.Add("Abrir", null, (_, _) => OnHotkeyPressed());
        menu.Items.Add("Sair", null, (_, _) => ExitThread());

        _notifyIcon = new NotifyIcon
        {
            Text = "Tradutor rápido",
            Icon = SystemIcons.Information,
            Visible = true,
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => OnHotkeyPressed();
    }

    private void OnHotkeyPressed()
    {
        _overlayForm.ShowOverlay();

        var clipboardText = TryGetClipboardText();
        if (!string.IsNullOrWhiteSpace(clipboardText))
            _overlayForm.SetInputAndTranslate(clipboardText!);
        else
            _overlayForm.FocusInput(selectAll: true);
    }

    private static string? TryGetClipboardText()
    {
        try
        {
            if (!Clipboard.ContainsText())
                return null;

            var text = Clipboard.GetText();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    protected override void ExitThreadCore()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();

        _hotkeyWindow.Dispose();
        _overlayForm.Dispose();

        base.ExitThreadCore();
    }
}
