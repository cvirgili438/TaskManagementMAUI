using Microsoft.Maui.Storage;
using TaskCalendar.Application.DTOs.Auth;

namespace TaskCalendar.App.Services;

public sealed class SessionService(CalendarApiClient apiClient)
{
    private const string TokenKey = "taskcalendar_token";
    private const string LanguageKey = "taskcalendar_lang";
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private bool _isInitialized;

    public event Action? Changed;

    public string? Token { get; private set; }
    public UserProfileResponse? CurrentUser { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
    public string Language { get; private set; } = Preferences.Default.Get(LanguageKey, "es");
    public int LanguageVersion { get; private set; }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializeLock.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                return;
            }

            Token ??= Preferences.Default.ContainsKey(TokenKey)
                ? Preferences.Default.Get(TokenKey, string.Empty)
                : null;

            if (!string.IsNullOrWhiteSpace(Token))
            {
                apiClient.SetToken(Token);
                try
                {
                    CurrentUser = await apiClient.GetProfileAsync();
                    SyncLanguageFromProfile();
                }
                catch
                {
                    await LogoutAsync();
                }
            }

            _isInitialized = true;
            Notify();
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var response = await apiClient.LoginAsync(request);
        ApplyAuth(response);
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var response = await apiClient.RegisterAsync(request);
        ApplyAuth(response);
    }

    public Task LogoutAsync()
    {
        Token = null;
        CurrentUser = null;
        _isInitialized = true;
        apiClient.SetToken(null);
        Preferences.Default.Remove(TokenKey);
        Notify();
        return Task.CompletedTask;
    }

    public void SetLanguage(string language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;
        LanguageVersion++;
        Preferences.Default.Set(LanguageKey, language);
        Notify();
    }

    private void ApplyAuth(AuthResponse response)
    {
        Token = response.Token;
        CurrentUser = response.User;
        _isInitialized = true;
        apiClient.SetToken(Token);
        Preferences.Default.Set(TokenKey, response.Token);
        SyncLanguageFromProfile();
        Notify();
    }

    private void SyncLanguageFromProfile()
    {
        if (CurrentUser is null)
        {
            return;
        }

        var newLanguage = CurrentUser.PreferredCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "es";
        if (Language != newLanguage)
        {
            Language = newLanguage;
            LanguageVersion++;
            Preferences.Default.Set(LanguageKey, Language);
        }
    }

    private void Notify() => Changed?.Invoke();
}
