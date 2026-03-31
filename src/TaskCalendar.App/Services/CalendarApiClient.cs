using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskCalendar.Application.DTOs.Auth;
using TaskCalendar.Application.DTOs.Calendar;

namespace TaskCalendar.App.Services;

public sealed class CalendarApiClient(HttpClient httpClient)
{
    public void SetToken(string? token)
    {
        httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
        => await SendAsync<AuthResponse>(() => httpClient.PostAsJsonAsync("api/auth/login", request));

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        => await SendAsync<AuthResponse>(() => httpClient.PostAsJsonAsync("api/auth/register", request));

    public async Task<UserProfileResponse> GetProfileAsync()
        => await GetRequiredAsync<UserProfileResponse>("api/auth/me");

    public async Task<IReadOnlyList<TaskOccurrenceResponse>> GetOccurrencesAsync(DateTimeOffset from, DateTimeOffset to)
        => await GetRequiredAsync<List<TaskOccurrenceResponse>>($"api/tasks?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}");

    public async Task<IReadOnlyList<TaskItemResponse>> GetTaskDefinitionsAsync()
        => await GetRequiredAsync<List<TaskItemResponse>>("api/tasks/definitions");

    public async Task<TaskItemResponse> CreateTaskAsync(TaskItemRequest request)
        => await SendAsync<TaskItemResponse>(() => httpClient.PostAsJsonAsync("api/tasks", request));

    public async Task<TaskItemResponse> UpdateTaskAsync(Guid id, TaskItemRequest request)
        => await SendAsync<TaskItemResponse>(() => httpClient.PutAsJsonAsync($"api/tasks/{id}", request));

    public async Task DeleteTaskAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"api/tasks/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<IReadOnlyList<UserOperatingHourDto>> GetOperatingHoursAsync()
        => await GetRequiredAsync<List<UserOperatingHourDto>>("api/operatinghours");

    public async Task SaveOperatingHoursAsync(UpdateOperatingHoursRequest request)
    {
        var response = await httpClient.PutAsJsonAsync("api/operatinghours", request);
        await EnsureSuccessAsync(response);
    }

    private async Task<T> GetRequiredAsync<T>(string path)
    {
        var response = await httpClient.GetAsync(path);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action();
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var payload = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(payload) ? response.ReasonPhrase : payload);
    }
}
