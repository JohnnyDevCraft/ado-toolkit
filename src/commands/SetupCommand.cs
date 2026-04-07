using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class SetupCommand
{
    private readonly SetupWorkflowService _setupWorkflowService;

    public SetupCommand(SetupWorkflowService setupWorkflowService)
    {
        _setupWorkflowService = setupWorkflowService;
    }

    public Task ExecuteAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        return _setupWorkflowService.RunAsync(config, cancellationToken);
    }
}

