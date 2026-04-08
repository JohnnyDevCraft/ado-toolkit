namespace AdoToolkit.Presentation;

public interface ISetupInteraction
{
    string PromptSecret(string prompt);

    string PromptText(string prompt);

    void WaitForAcknowledgement(string message);
}
