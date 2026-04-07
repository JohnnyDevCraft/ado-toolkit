using AdoToolkit.Commands;
using AdoToolkit.Services;

var host = new AppHost();
var router = new CommandRouter(host);
return await router.ExecuteAsync(args);

