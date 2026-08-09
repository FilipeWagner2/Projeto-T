namespace TranslatorTrayApp;

internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    private readonly int _id;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public HotkeyWindow(int id)
    {
        _id = id;
        CreateHandle(new CreateParams());
    }

    public bool Register(uint modifiers, uint virtualKey)
    {
        if (_registered)
            return true;

        _registered = NativeMethods.RegisterHotKey(Handle, _id, modifiers | NativeMethods.MOD_NOREPEAT, virtualKey);
        return _registered;
    }

    public void Unregister()
    {
        if (!_registered)
            return;

        NativeMethods.UnregisterHotKey(Handle, _id);
        _registered = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == _id)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        Unregister();
        DestroyHandle();
        GC.SuppressFinalize(this);
    }
}
