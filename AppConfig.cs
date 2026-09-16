namespace TranslatorTrayApp;

internal static class AppConfig
{
    // Troque para "en" se quiser sempre traduzir para inglês.
    internal sealed record LanguageOption(string Code, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    public static readonly LanguageOption[] Languages =
    {
        new("auto", "Detectar idioma"),
        new("pt", "Português"),
        new("en", "Inglês"),
        new("es", "Espanhol"),
        new("fr", "Francês"),
        new("de", "Alemão"),
        new("it", "Italiano"),
        new("ja", "Japonês"),
        new("zh-CN", "Chinês"),
        new("ko", "Coreano"),
        new("ru", "Russo")
    };

    public const string DefaultSourceLanguage = "auto";
    public const string DefaultTargetLanguage = "pt";

    // ALT + D
    public const uint HotkeyModifiers = NativeMethods.MOD_ALT;
    public const uint HotkeyVirtualKey = (uint)Keys.D;

    // Janela de overlay
    public const int OverlayWidth = 520;
    // A altura inclui a barra de título; 270 mantém os botões inferiores visíveis.
    public const int OverlayHeight = 270;

    // Pequeno debounce para reduzir chamadas enquanto digita
    public const int AutoTranslateDebounceMs = 180;
}
