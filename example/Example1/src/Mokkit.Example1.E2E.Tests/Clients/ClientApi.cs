using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Mokkit.Example1.E2E.Tests.Contracts;

namespace Mokkit.Example1.E2E.Tests.Clients;

/// <summary>
/// The observable artifact a write operation hands from the Act to the Inspect: the HTTP status plus the
/// client id and message the API returned.
/// </summary>
public sealed record ClientWriteResult(HttpStatusCode Status, Guid? ClientId, string? Message);

/// <summary>
/// Raw HTTP calls against the client API, shared by arranges (when creating a precondition) and acts
/// (when the write is the operation under test). Each returns a <see cref="ClientWriteResult"/> artifact.
/// </summary>
public static class ClientApi
{
    public static async Task<ClientWriteResult> CreateAsync(HttpClient http, SaveClientRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/v1/clients", request);
        return await ToResult(response);
    }

    public static async Task<ClientWriteResult> UpdateAsync(HttpClient http, Guid clientId, SaveClientRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/v1/clients/{clientId}", request);
        var result = await ToResult(response);

        // A PUT targets a known id; fall back to it if the body doesn't echo one.
        return result with { ClientId = result.ClientId ?? clientId };
    }

    private static async Task<ClientWriteResult> ToResult(HttpResponseMessage response)
    {
        SaveClientResponse? body = null;
        if (response.Content.Headers.ContentLength is > 0)
        {
            try
            {
                body = await response.Content.ReadFromJsonAsync<SaveClientResponse>();
            }
            catch (JsonException)
            {
                // Non-JSON error body (e.g. the global exception handler) — status is enough.
            }
        }

        var clientId = body is { ClientId: var id } && id != Guid.Empty ? id : (Guid?)null;
        return new ClientWriteResult(response.StatusCode, clientId, body?.Message);
    }
}
