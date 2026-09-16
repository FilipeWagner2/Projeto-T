# Tradutor rápido (MVP leve)

Aplicativo desktop em WinForms focado em:
- baixo uso de CPU/RAM
- inicialização rápida
- execução em segundo plano (tray)
- atalho global `ALT + D`

## Como executar

1. Abra a pasta do projeto.
2. Build/Run em Release:
   - `dotnet run -c Release`

## Fluxo

1. O app inicia direto na bandeja do sistema.
2. Pressione `ALT + D` para abrir o overlay.
3. Digite ou cole texto.
4. Tradução automática (debounce curto) ou `ENTER` para traduzir na hora.
5. `ESC` fecha o overlay instantaneamente.

## Idiomas

O overlay permite escolher o idioma de origem e o idioma de destino.
`Detectar idioma` usa detecção automática para a origem. A lista inicial inclui português, inglês, espanhol, francês, alemão, italiano, japonês, chinês, coreano e russo.

## Arquitetura

- `TrayApplicationContext`: ciclo de vida do app + tray + hotkey global
- `HotkeyWindow`: registra/escuta `WM_HOTKEY`
- `UI/OverlayForm`: janela leve de entrada/resultado
- `Translation/TranslationService`: chamada HTTP assíncrona + cache simples

## Fonte de tradução

Endpoint gratuito sem autenticação:
- `translate.googleapis.com/translate_a/single`
