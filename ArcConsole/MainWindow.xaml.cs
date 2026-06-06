using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using ArcShared;

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
                string escapedPayload = JsonSerializer.Serialize(message.Payload);
                string color = message.IsSuccess ? "#00ff00" : "#ff4444";
                if (message.Type == ArcConstants.MessageTypeProgress) color = "#ffffff";
                
                await OutputWebView.ExecuteScriptAsync($@"
            var div = document.createElement('div');
            div.style.color = '{color}';
            div.style.whiteSpace = 'pre-wrap'; // Add this here too!
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
    }

    private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string commandText = e.TryGetWebMessageAsString();
        if (string.IsNullOrWhiteSpace(commandText)) return;
    
        // Call your existing command logic
        string escapedCommand = JsonSerializer.Serialize(commandText);
        await OutputWebView.ExecuteScriptAsync($@"
    var div = document.createElement('div');
    div.style.color = '#888888';
    // Add this line to preserve your spaces and line breaks
    div.style.whiteSpace = 'pre-wrap'; 
    div.innerText = '> ' + {escapedCommand};
    document.getElementById('output').appendChild(div);
    document.getElementById('output').scrollTop = document.getElementById('output').scrollHeight;
");
        ArcCommand command = CommandParser.Parse(commandText);
        await _client.SendCommandAsync(command);
    }
}