using System.Text.Json;
using System.Windows;
using ArcShared;
using Microsoft.Web.WebView2.Core;

namespace ArcConsole;

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
                string commandIdStr = message.CommandId.ToString();

                if (message.Type == ArcConstants.MessageTypeResult)
                {
                    string statusSymbol = message.IsSuccess ? "✓" : "✕";
                    string statusClass = message.IsSuccess ? "status-success" : "status-error";

                    await OutputWebView.ExecuteScriptAsync($@"
                        var spinner = document.getElementById('spinner-' + '{commandIdStr}');
                        if (spinner) {{ 
                            spinner.className = '{statusClass}';
                            spinner.innerText = '{statusSymbol}';
                        }}
                    ");
                }

                // Hides the callback ("command received") ack for UI
                if (message.Type == ArcConstants.MessageTypeCallback)
                {
                    return; // Exit early without creating a UI element
                }

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
                    body { margin: 0; background-color: #050508; color: #ffffff; font-family: sans-serif; height: 100vh; display: flex; flex-direction: column; overflow: hidden; }
                    .ambient-bg { position: fixed; inset: 0; pointer-events: none; z-index: 0; overflow: hidden; }
                    .circle { position: absolute; border-radius: 50%; mix-blend-mode: screen; background: radial-gradient(circle, rgba(13, 110, 253, 0.3) 0%, transparent 70%); width: 40vw; height: 40vw; left: 10vw; top: 10vh; }
                    .blur-overlay { position: absolute; inset: 0; backdrop-filter: blur(80px); background: rgba(5, 5, 8, 0.6); z-index: 1; }
                    #output { position: relative; z-index: 2; flex-grow: 1; overflow-y: auto; padding: 20px; font-family: monospace; font-size: 13px; }
                    .input-area { position: relative; z-index: 2; padding: 20px; border-top: 1px solid #333; display: flex; gap: 10px; background: rgba(10, 10, 15, 0.8); }
                    input { flex-grow: 1; background: #1a1a24; color: white; border: 1px solid #444; padding: 8px; border-radius: 4px; }
                    button { padding: 8px 16px; cursor: pointer; background: #252526; color: white; border: 1px solid #444; border-radius: 4px; }
                    
                    /* CSS Spinner Styles */
                    .command-row { 
                        display: flex; 
                        align-items: center; 
                        justify-content: space-between;
                        width: 100%;
                        color: #888888; 
                        white-space: pre-wrap; 
                        border-top: 1px solid rgba(255, 255, 255, 0.1); 
                        margin-top: 12px;                               
                        padding-top: 8px;                               
                    }
                    .spinner {
                        width: 12px;
                        height: 12px;
                        border: 2px solid rgba(255,255,255,0.2);
                        border-top-color: #00fffa;
                        border-radius: 50%;
                        animation: spin 0.8s linear infinite, fadeIn 0.2s ease-in forwards;
                        animation-delay: 0s, 0.1s;
                        opacity: 0;
                        display: inline-block;
                    }

                    /* End Status Styles */
                    .status-success {
                        color: #00ff00;
                        font-weight: bold;
                        display: inline-block;
                        animation: fadeIn 0.15s ease-out forwards;
                    }
                    .status-error {
                        color: #ff4444;
                        font-weight: bold;
                        display: inline-block;
                        animation: fadeIn 0.15s ease-out forwards;
                    }

                    @keyframes spin {
                        to { transform: rotate(360deg); }
                    }

                    @keyframes fadeIn {
                        to { opacity: 1; }
                    }
                </style>
            </head>
            <body>
                <div class='ambient-bg'><div class='circle'></div><div class='blur-overlay'></div></div>
                <div id='output'></div>
                <div class='input-area'>
                    <input type='text' id='cmdInput' placeholder='Enter command...' />
                    <button onclick='send()'>Send</button>
                </div>
                <script>
                    function send() {
                        const input = document.getElementById('cmdInput');
                        if(input.value.trim()){
                            window.chrome.webview.postMessage(input.value);
                            input.value = '';
                        }
                    }
                    document.getElementById('cmdInput').addEventListener('keypress', (e) => { if(e.key === 'Enter') send(); });
                </script>
            </body>
        </html>";

        OutputWebView.CoreWebView2.NavigateToString(html);
        OutputWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        await OutputWebView.ExecuteScriptAsync($@"
            var div = document.createElement('div');
            div.style.color = '#00fffa';
            div.style.whiteSpace = 'pre-wrap';
            div.innerText = 'Welcome to the ARC console! Type HELP for a list of commands.';
            document.getElementById('output').appendChild(div);
            document.getElementById('output').scrollTop = document.getElementById('output').scrollHeight;
        ");
    }

    private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string commandText = e.TryGetWebMessageAsString();
        if (string.IsNullOrWhiteSpace(commandText)) return;

        ArcCommand command = CommandParser.Parse(commandText);
        string commandIdStr = command.Id.ToString();
        string escapedCommand = JsonSerializer.Serialize(commandText);

        await OutputWebView.ExecuteScriptAsync($@"
            var row = document.createElement('div');
            row.className = 'command-row';
            row.id = 'cmd-row-' + '{commandIdStr}';
            
            var textSpan = document.createElement('span');
            textSpan.innerText = '> ' + {escapedCommand};
            
            var spinnerSpan = document.createElement('span');
            spinnerSpan.className = 'spinner';
            spinnerSpan.id = 'spinner-' + '{commandIdStr}';
            
            row.appendChild(textSpan);
            row.appendChild(spinnerSpan);
            
            document.getElementById('output').appendChild(row);
            document.getElementById('output').scrollTop = document.getElementById('output').scrollHeight;
        ");

        await _client.SendCommandAsync(command);
    }
}