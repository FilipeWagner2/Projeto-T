# Tradutor rápido

Um tradutor simples para Windows, feito em C# com WinForms. Ele fica na bandeja do sistema e abre uma janela pequena com o atalho `ALT + D`.

## O que ele faz

- roda em segundo plano pela bandeja do Windows;
- abre o campo de tradução com `ALT + D`;
- traduz enquanto você digita ou quando pressiona `ENTER`;
- permite escolher idioma de origem e destino;
- copia somente o texto traduzido para a área de transferência.

## Como rodar

Você precisa do .NET 8 instalado.

Pelo terminal, entre na pasta do projeto e rode:

```powershell
cd "C:\caminho\para\Projeto-T"
dotnet run -c Release
```

No Windows, também dá para abrir com clique duplo em:

```text
abrir-tradutor.bat
```

Se já existir uma build em `bin\Release`, o script abre o executável direto. Se não existir, ele roda o projeto com `dotnet run`.

## Como usar

1. Abra o app.
2. Use `ALT + D` para mostrar a janela.
3. Digite ou cole o texto.
4. Use `ENTER` para traduzir na hora.
5. Use `ESC` para ocultar a janela.

Para encerrar o app, use o menu do ícone na bandeja do sistema.

## Publicar uma versão local

Para gerar uma versão para Windows que abre direto pelo `.exe`, execute:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Ou use o arquivo:

```text
publicar-win-x64.bat
```

Antes de publicar ou compilar em Release, feche o app pela bandeja. Se ele estiver aberto, o Windows pode bloquear a substituição do `.exe`.

Depois da publicação, o executável fica em:

```text
bin\Release\net8.0-windows\win-x64\publish\
```

## Privacidade

O texto digitado é enviado para serviços externos de tradução. O app usa o endpoint público do Google Translate e, se houver bloqueio ou indisponibilidade, tenta o MyMemory como alternativa.

Não use este app para traduzir senhas, documentos confidenciais ou qualquer conteúdo que não deva sair do seu computador.

## Estrutura

- `Program.cs`: ponto de entrada do app.
- `TrayApplicationContext.cs`: bandeja do sistema, menu e ciclo de vida.
- `HotkeyWindow.cs`: registro do atalho global.
- `UI/OverlayForm.cs`: janela de tradução.
- `Translation/TranslationService.cs`: chamadas HTTP e cache simples.
