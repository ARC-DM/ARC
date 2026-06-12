using ArcDaemon;
using ArcDaemon.CommandHandlers;
using ArcShared;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService();
builder.Services.AddSingleton<ConnectionHandler>();
builder.Services.AddSingleton<TaskDispatcher>();
builder.Services.AddSingleton<AuthenticationHandler>();
builder.Services.AddHostedService<PipeListenerService>();

var host = builder.Build();
var dispatcher = host.Services.GetRequiredService<TaskDispatcher>();
var auth = host.Services.GetRequiredService<AuthenticationHandler>();
dispatcher.RegisterActionHandler(ArcConstants.ActionPing, new PingHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionInstall, new InstallHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionUninstall, new UninstallHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionWhoAmI, new WhoIsHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionProcesses, new ProcessesHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionSystemReport, new SystemReportHandler());
dispatcher.RegisterActionHandler(ArcConstants.ActionDrives, new DiskHandler());

dispatcher.RegisterActionHandler(ArcConstants.ActionHelp, new HelpHandler(dispatcher));

auth.CreateDatabase();
auth.AddCommand(ArcConstants.ActionPing, "NONE");
auth.AddCommand(ArcConstants.ActionInstall, "USER");
auth.AddCommand(ArcConstants.ActionUninstall, "USER");
auth.AddCommand(ArcConstants.ActionWhoAmI, "USER");
auth.AddCommand(ArcConstants.ActionProcesses, "ADMIN");
auth.AddCommand(ArcConstants.ActionSystemReport, "ADMIN");
auth.AddCommand(ArcConstants.ActionDrives, "ADMIN");
host.Run();