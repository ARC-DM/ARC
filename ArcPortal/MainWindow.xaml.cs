using System.Text.Json;
using System.Windows;
using ArcShared;
using Microsoft.Web.WebView2.Core;

namespace ArcPortal;

public partial class MainWindow : Window
{
    private readonly DaemonClient _client;
    private bool _isWebViewReady = false;
    public MainWindow()
    {
        InitializeComponent();
        InitializeWebView();

        _client = new DaemonClient(message =>
        {
          Dispatcher.Invoke(async () =>
          {
            string escapedPayload = JsonSerializer.Serialize(message.Payload);
            string color = message.IsSuccess ? "#00ff00" : "#ff4444";
            if (message.Type == ArcConstants.MessageTypeProgress) color = "#ffffff";

            await OutputWebView.ExecuteScriptAsync($@"
            var div = document.createElement('div');
            div.style.color = '{color}';
            div.style.whiteSpace = 'pre-wrap';
            div.innerText = {escapedPayload};
            document.getElementById('output').appendChild(div);
            document.getElementById('output').scrollTop = document.getElementById('output').scrollHeight;
        ");
          });
        });
    }

    private async void InitializeWebView()
    {
        await OutputWebView.EnsureCoreWebView2Async();
        
        string html = @"
<html>
<head>
<style>
   @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap');
   *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
   body {
     margin: 0;
     background-color: #050508;
     color: #ffffff;
     font-family: 'Inter', sans-serif;
     min-height: 100vh;
     display: flex;
     flex-direction: column;
     overflow: hidden;
   }
   /* ── Ambient background ── */
   .ambient-bg {
     position: fixed;
     inset: 0;
     pointer-events: none;
     z-index: 0;
     overflow: hidden;
   }
   .circle {
     position: absolute;
     border-radius: 50%;
     mix-blend-mode: screen;
     background: radial-gradient(circle, rgba(13, 110, 253, 0.35) 0%, transparent 70%);
     width: 40vw;
     height: 40vw;
     left: 10vw;
     top: 10vh;
   }
   .circle-2 {
     position: absolute;
     border-radius: 50%;
     mix-blend-mode: screen;
     background: radial-gradient(circle, rgba(99, 40, 220, 0.2) 0%, transparent 70%);
     width: 30vw;
     height: 30vw;
     right: 5vw;
     bottom: 10vh;
   }
   .blur-overlay {
     position: absolute;
     inset: 0;
     backdrop-filter: blur(80px);
     background: rgba(5, 5, 8, 0.55);
     z-index: 1;
   }
   /* ── Layout ── */
   #options {
     position: relative;
     z-index: 2;
     padding: 32px 24px 24px;
     display: flex;
     flex-wrap: wrap;
     gap: 16px;
   }
   #output {
     position: relative;
     z-index: 2;
     flex-grow: 1;
     overflow-y: auto;
     padding: 20px;
     font-family: monospace;
     font-size: 13px;
   }
   .input-area {
     position: relative;
     z-index: 2;
     padding: 20px;
     border-top: 1px solid rgba(255,255,255,0.08);
     display: flex;
     gap: 10px;
     background: rgba(10, 10, 15, 0.85);
   }
   input {
     flex-grow: 1;
     background: #1a1a24;
     color: white;
     border: 1px solid #333;
     padding: 8px 12px;
     border-radius: 6px;
     font-family: inherit;
     font-size: 13px;
     outline: none;
   }
   input:focus { border-color: #4a7aff; }
   /* ── Card ── */
   .card {
     width: 260px;
     background: linear-gradient(145deg, rgba(255,255,255,0.07) 0%, rgba(255,255,255,0.03) 100%);
     border: 1px solid rgba(255,255,255,0.1);
     border-radius: 16px;
     padding: 20px;
     display: flex;
     flex-direction: column;
     gap: 12px;
     box-shadow:
       0 4px 24px rgba(0,0,0,0.4),
       inset 0 1px 0 rgba(255,255,255,0.08);
     backdrop-filter: blur(12px);
     transition: box-shadow 0.2s ease, border-color 0.2s ease, transform 0.2s ease;
   }
   .card:hover {
     border-color: rgba(255,255,255,0.18);
     box-shadow:
       0 8px 32px rgba(0,0,0,0.5),
       0 0 0 1px rgba(74,122,255,0.15),
       inset 0 1px 0 rgba(255,255,255,0.1);
   }
   /* Card icon badge */
   .card-icon {
     width: 36px;
     height: 36px;
     border-radius: 10px;
     background: linear-gradient(135deg, rgba(74,122,255,0.3), rgba(99,40,220,0.3));
     border: 1px solid rgba(74,122,255,0.25);
     display: flex;
     align-items: center;
     justify-content: center;
     font-size: 16px;
     flex-shrink: 0;
   }
   .card-header {
     display: flex;
     align-items: center;
     gap: 12px;
   }
   .card-title {
     font-size: 15px;
     font-weight: 600;
     color: #f0f0f5;
     letter-spacing: -0.01em;
   }
   .card-contents {
     font-size: 13px;
     color: rgba(255,255,255,0.5);
     line-height: 1.55;
     flex-grow: 1;
   }
   .card-footer {
     display: flex;
     align-items: center;
     justify-content: space-between;
     padding-top: 4px;
     border-top: 1px solid rgba(255,255,255,0.06);
   }
   .card-meta {
     font-size: 11px;
     color: rgba(255,255,255,0.28);
     letter-spacing: 0.03em;
   }
   /* Install button */
   .btn-install {
     padding: 7px 16px;
     cursor: pointer;
     background: linear-gradient(135deg, #2a5ce6, #1e44b8);
     color: #e8eeff;
     border: 1px solid rgba(100,140,255,0.35);
     border-radius: 8px;
     font-family: inherit;
     font-size: 12px;
     font-weight: 500;
     letter-spacing: 0.02em;
     box-shadow: 0 2px 8px rgba(30, 68, 184, 0.35);
     transition: background 0.15s ease, box-shadow 0.15s ease, transform 0.1s ease;
   }
   .btn-install:hover {
     background: linear-gradient(135deg, #3568f5, #2a50d0);
   }
   .btn-install:active {
     transform: scale(0.97);
   }
   /* Generic send button */
   button:not(.btn-install) {
     padding: 8px 16px;
     cursor: pointer;
     background: #1e1e2a;
     color: #ccc;
     border: 1px solid #333;
     border-radius: 6px;
     font-family: inherit;
     font-size: 13px;
     transition: background 0.15s, border-color 0.15s;
   }
   button:not(.btn-install):hover {
     background: #252535;
     border-color: #555;
   }

.bar {
    user-select: none;
    width: 100%;
  height: 3px;
  position: relative;
    align-items: center;
	border-radius: 5px;
  background: rgba(255, 255, 255, 0.5)
}

.bar-progress {
  position: absolute;
  top: 0;
  left: 0;
	height: 100%;
	cursor: pointer;
  animation: progressAnimation 1000ms infinite ease;
    background: rgb(255, 255, 255);
}

@keyframes progressAnimation {
  0% {
    width: 0;
  }
  50% {
    left: 25%;
    width: 50%;
  }
  100% {
    left: 100%;
    width: 0;
  }
}
</style>
</head>
<body>
<div class=""ambient-bg"">
<div class=""circle""></div>
<div class=""circle-2""></div>
<div class=""blur-overlay""></div>
</div>
<div id=""options"">
<div class=""card"">
<div class=""card-header"">
<div class=""card-icon"">🛠️</div>
<span class=""card-title"">PING</span>
</div>
<div class=""card-contents"">Run a PING test on your device.</div>
<div class=""card-footer"">
<span class=""card-meta"">v2.4.1 · 14 MB</span>
<button class=""btn-install"" onclick=""send('WHOAMI')"">Run</button>
</div>
</div>
<div class=""card"">
<div class=""card-header"">
<div class=""card-icon"">🛠️</div>
<span class=""card-title"">DevTools</span>
</div>
<div class=""card-contents"">A suite of debugging and profiling utilities for local development environments.</div>
    <div class=""bar"">
      <div class=""bar-progress"">
      </div>
    </div>
<div class=""card-footer"">
<span class=""card-meta"">v1.0.0 · 8 MB</span>
<button class=""btn-install"" onclick=""send('install-devtools')"">Install</button>
</div>
</div>
</div>
<div id=""output""></div>
</div>
<script>
   function send(command) {
      window.chrome.webview.postMessage(command)
   }
</script>
</body>
</html>";
        
        OutputWebView.CoreWebView2.NavigateToString(html);
        OutputWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
    }

    private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string commandText = e.TryGetWebMessageAsString();
        if (string.IsNullOrWhiteSpace(commandText)) return;
        ArcCommand command = CommandParser.Parse(commandText);
        await _client.SendCommandAsync(command);
    }
}