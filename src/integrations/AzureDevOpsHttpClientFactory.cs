using System.Net.Http.Headers;
using System.Text;

namespace AdoToolkit.Integrations;

public sealed class AzureDevOpsHttpClientFactory
{
    public HttpClient Create(string pat)
    {
        var client = new HttpClient();
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}

