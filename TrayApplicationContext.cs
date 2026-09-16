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
        _overlayForm = new OverlayForm(translationService);

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
