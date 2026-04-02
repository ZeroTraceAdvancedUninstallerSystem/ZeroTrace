namespace ZeroTrace.Core.Localization;

public sealed class LocalizationSvc
{
    public string CurrentLanguage { get; private set; } = "DE";

    public string GetText(string key) => (CurrentLanguage, key) switch
    {
        ("DE", "ScanBtn") => "System scannen",
        ("EN", "ScanBtn") => "Scan System",
        ("DE", "StatusReady") => "Bereit.",
        ("EN", "StatusReady") => "Ready.",
        _ => key
    };

    public void SetLanguage(string lang) => CurrentLanguage = lang.ToUpper();
}