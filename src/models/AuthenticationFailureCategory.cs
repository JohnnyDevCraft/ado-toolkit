namespace AdoToolkit.Models;

public enum AuthenticationFailureCategory
{
    None = 0,
    InvalidCredentials = 1,
    InsufficientPermissions = 2,
    ConnectivityFailure = 3,
    ServiceFailure = 4
}
