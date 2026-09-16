# Casos de teste temporários — Tradutor Tray App

Data: 2026-09-16  
Versão: Release local

## Legenda

- **Aprovado**: verificado nesta sessão com evidência suficiente.
- **Reprovado (não executado)**: ainda depende de interação manual ou de uma condição que não foi validada.

| ID | Caso de teste | Critério de aceite | Resultado esperado | Status |
|---|---|---|---|---|
| CT-001 | Compilação Release | O projeto deve compilar sem erros ou avisos | `dotnet build -c Release --no-restore` termina com 0 erros e 0 avisos | Aprovado |
| CT-002 | Inicialização em segundo plano | O app não deve abrir uma janela automaticamente | O processo inicia responsivo e aparece somente na bandeja | **Aprovado** |
| CT-003 | Ícone e menu da bandeja | O app deve oferecer acesso pela bandeja | O ícone fica visível e o menu contém “Abrir” e “Sair” | **Reprovado (não executado)** |
| CT-004 | Atalho global `ALT + D` | O atalho deve abrir o overlay mesmo com outro programa em primeiro plano | A janela pequena abre centralizada e recebe foco | **Reprovado (não executado)** |
| CT-005 | Foco automático | O campo de entrada deve receber o foco ao abrir | O texto digitado aparece diretamente no campo | **Reprovado (não executado)** |
| CT-006 | Tradução automática | Uma alteração no campo deve iniciar tradução após o debounce | A resposta aparece sem clicar em botão | **Reprovado (não executado)** |
| CT-007 | Tradução com `ENTER` | `ENTER` deve traduzir imediatamente | A tradução é solicitada sem aguardar o debounce completo | **Reprovado (não executado)** |
| CT-008 | Detecção automática de idioma | O serviço deve aceitar idiomas de entrada diferentes | `good morning` retorna uma tradução em português com `TargetLanguage = "pt"` | **Reprovado (não executado)** |
| CT-009 | Cancelamento por nova entrada | Apenas a última requisição deve atualizar a tela | Digitação rápida não deixa resultado antigo sobrescrever o atual | **Reprovado (não executado)** |
| CT-010 | Tratamento de falha de rede | A interface não deve travar quando o endpoint falhar | O status mostra falha e o app continua utilizável | **Reprovado (não executado)** |
| CT-011 | Cópia do resultado | O botão deve copiar somente o texto traduzido | Ao colar em outro programa, não aparecem rótulo, contexto ou exemplo | **Reprovado (não executado)** |
| CT-012 | Fechamento com `ESC` | `ESC` deve ocultar o overlay imediatamente | A janela desaparece e uma requisição pendente é cancelada | **Reprovado (não executado)** |
| CT-013 | Encerramento pelo menu | “Sair” deve liberar os recursos do app | Ícone, hotkey e processo são encerrados sem erro | **Reprovado (não executado)** |
| CT-014 | Consumo em repouso | O app deve permanecer leve em segundo plano | CPU permanece próxima de zero e RAM deve ser registrada durante o teste | **Reprovado (não executado)** |
| CT-015 | Fallback após bloqueio do provedor principal | HTTP 429/403/503 do Google não deve impedir a tradução | O app usa o MyMemory e exibe o resultado quando o provedor principal bloqueia a requisição | **Reprovado (não executado)** |
| CT-016 | Fechamento pelo botão `X` | Fechar o overlay não deve destruir a janela reutilizada pela bandeja | O overlay apenas se oculta; `ALT + D` e “Abrir” continuam funcionando depois | **Reprovado (não executado)** |
| CT-017 | Seleção de idioma de origem | O usuário deve escolher a língua em que está escrevendo | O seletor “De” oferece “Detectar idioma” e os idiomas disponíveis | **Reprovado (não executado)** |
| CT-018 | Seleção de idioma de destino | O usuário deve escolher a língua da tradução | O seletor “Para” altera o idioma de saída | **Reprovado (não executado)** |
| CT-019 | Nova tradução ao trocar idioma | Alterar “De” ou “Para” com texto preenchido deve atualizar o resultado | Uma nova tradução é solicitada imediatamente usando as duas seleções | **Reprovado (não executado)** |
| CT-020 | Botão `clear` | Limpar os dados da tradução sem alterar os idiomas | Entrada, resultado, status e requisições pendentes são limpos; “De” e “Para” permanecem iguais | **Reprovado (não executado)** |
| CT-021 | Botão `clear all` | Limpar os dados e restaurar as preferências de idioma | Entrada, resultado e status são limpos; “De” volta para “Detectar idioma” e “Para” volta para português | **Reprovado (não executado)** |
| CT-022 | Informativo do botão de copiar | O usuário deve entender que copiar leva somente a tradução | Ao passar o mouse no ícone `ⓘ`, aparece a explicação; ao sair, ela desaparece | **Reprovado (não executado)** |
| CT-023 | Redimensionamento do HUD | O usuário deve aumentar a janela para ler respostas longas | Arrastar uma borda ou canto aumenta o overlay e a área de resultado acompanha o tamanho | **Reprovado (não executado)** |

## Execução manual

Na pasta do projeto:

```powershell
dotnet run -c Release
```

Depois, executar CT-003 a CT-014 na ordem. Para cada caso, substituir o status por **Aprovado** quando o critério for atendido; manter **Reprovado** quando falhar e anotar o comportamento observado abaixo.

## Observações

- O idioma padrão está definido como português em `AppConfig.cs`.
- O endpoint de tradução exige conexão com a internet.
- O MyMemory é usado como fallback gratuito quando o endpoint principal bloqueia requisições automáticas.
- Este arquivo é temporário e pode ser removido depois da primeira rodada de testes.
