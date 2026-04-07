using AdoToolkit.Config;
using AdoToolkit.Presentation;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class CommandRouter
{
    private readonly AppHost _host;
    private readonly HelpCommand _helpCommand;
    private readonly SetupCommand _setupCommand;
    private readonly ConfigCommands _configCommands;

    public CommandRouter(AppHost host)
    {
        _host = host;
        _helpCommand = new HelpCommand();
        _setupCommand = new SetupCommand(host.SetupWorkflowService);
        _configCommands = new ConfigCommands(host.CurrentContextService);
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _host.HeaderRenderer.Render();

        try
        {
            if (args.Length == 0 || args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                _helpCommand.Execute();
                return 0;
            }

            if (args[0].Equals("--version", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-v", StringComparison.OrdinalIgnoreCase))
            {
                _host.Output.WriteInfo("ado 0.1.0");
                return 0;
            }

            var config = await _host.ConfigService.LoadOrCreateAsync(cancellationToken);

            if (args[0].Equals("setup", StringComparison.OrdinalIgnoreCase))
            {
                await _setupCommand.ExecuteAsync(config, cancellationToken);
                _host.Output.WriteSuccess("Setup completed.");
                return 0;
            }

            if (args.Length >= 2 && args[0].Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleConfigAsync(config, args[1..], cancellationToken);
            }

            throw new InvalidOperationException("Unknown command. Run `ado --help` to see supported commands.");
        }
        catch (Exception ex)
        {
            _host.Output.WriteError(ex.Message);
            return 1;
        }
    }

    private async Task<int> HandleConfigAsync(Models.AppConfig config, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            throw new InvalidOperationException("Missing config subcommand.");
        }

        switch (args[0].ToLowerInvariant())
        {
            case "set-pat":
                if (args.Length < 2)
                {
                    throw new InvalidOperationException("Usage: ado config set-pat <PAT>");
                }

                await _configCommands.SetPatAsync(config, args[1], cancellationToken);
                _host.Output.WriteSuccess("PAT updated.");
                return 0;

            case "set-org":
                await _configCommands.SetOrganizationAsync(config, args.Length > 1 ? args[1] : null, cancellationToken);
                _host.Output.WriteSuccess($"Current organization set to {config.CurrentContext.Organization}.");
                return 0;

            case "set-project":
                await _configCommands.SetProjectAsync(config, args.Length > 1 ? args[1] : null, cancellationToken);
                _host.Output.WriteSuccess($"Current project set to {config.CurrentContext.Project?.Name ?? config.CurrentContext.Project?.Id}.");
                return 0;

            default:
                throw new InvalidOperationException("Unknown config subcommand.");
        }
    }
}

