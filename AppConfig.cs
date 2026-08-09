namespace TranslatorTrayApp;

internal static class AppConfig
{
    // Troque para "en" se quiser sempre traduzir para inglês.
    public const string TargetLanguage = "pt";

    // ALT + D
    public const uint HotkeyModifiers = NativeMethods.MOD_ALT;
    public const uint HotkeyVirtualKey = (uint)Keys.D;

    // Janela de overlay
    public const int OverlayWidth = 520;
    public const int OverlayHeight = 190;

    // Pequeno debounce para reduzir chamadas enquanto digita
    public const int AutoTranslateDebounceMs = 180;
}
