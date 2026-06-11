using ArcDaemon;
using ArcDaemon.CommandHandlers;
using ArcShared;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService();
builder.Services.AddSingleton<ConnectionHandler>();
builder.Services.AddSingleton<TaskDispatcher>();
builder.Services.AddHostedService<PipeListenerService>();

var host = builder.Build();
var dispatcher = host.Services.GetRequiredService<TaskDispatcher>();
dispatcher.RegisterActionHandler(ArcConstants.ActionPing, new PingHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionParameterTest, new ParameterTestHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionInstall, new InstallHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionUninstall, new UninstallHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionWhoAmI, new whoIsHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionProcesses, new ProcessesHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionSystemReport, new systemReportHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionDrives, new DiskHandler());
host.Run();