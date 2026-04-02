// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Localization;

/// <summary>
/// Multi-language support for ZeroTrace UI.
/// Currently supports German (DE) and English (EN).
/// New languages can be added via AddTranslation().
/// </summary>
public sealed class LocalizationService
{
    private readonly IZeroTraceLogger _logger;
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLanguage = "DE";

    public string CurrentLanguage => _currentLanguage;
    public event EventHandler<string>? LanguageChanged;

    public LocalizationService(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoadDefaultTranslations();
    }

    /// <summary>Get translated text for a key. Returns key if not found.</summary>
    public string Get(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var dict)
            && dict.TryGetValue(key, out var value))
            return value;

        // Fallback to English
        if (_currentLanguage != "EN"
            && _translations.TryGetValue("EN", out var enDict)
            && enDict.TryGetValue(key, out var enValue))
            return enValue;

        return key; // Return key itself as last resort
    }

    /// <summary>Shorthand indexer: localization["Key"]</summary>
    public string this[string key] => Get(key);

    /// <summary>Switch the active language.</summary>
    public void SetLanguage(string languageCode)
    {
        var code = languageCode.ToUpperInvariant();
        if (!_translations.ContainsKey(code))
        {
            _logger.Warning($"Sprache nicht verfuegbar: {code}. Verfuegbar: {string.Join(", ", GetAvailableLanguages())}");
            return;
        }

        _currentLanguage = code;
        _logger.Info($"Sprache gewechselt: {code}");
        LanguageChanged?.Invoke(this, code);
    }

    /// <summary>Get list of available language codes.</summary>
    public IReadOnlyList<string> GetAvailableLanguages() =>
        _translations.Keys.ToList().AsReadOnly();

    /// <summary>Add or overwrite a single translation.</summary>
    public void AddTranslation(string language, string key, string value)
    {
        var lang = language.ToUpperInvariant();
        if (!_translations.TryGetValue(lang, out var dict))
        {
            dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _translations[lang] = dict;
        }
        dict[key] = value;
    }

    /// <summary>Add a complete language dictionary.</summary>
    public void AddLanguage(string language, Dictionary<string, string> translations)
    {
        _translations[language.ToUpperInvariant()] = new Dictionary<string, string>(
            translations, StringComparer.OrdinalIgnoreCase);
        _logger.Info($"Sprache hinzugefuegt: {language} ({translations.Count} Eintraege)");
    }

    private void LoadDefaultTranslations()
    {
        // German (Default)
        _translations["DE"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // App
            ["AppTitle"]           = "ZeroTrace - Advanced Uninstaller System",
            ["AppSubtitle"]        = "Professionelle Software-Entfernung",

            // Buttons
            ["BtnScan"]            = "System scannen",
            ["BtnUninstall"]       = "Deinstallieren",
            ["BtnDeepClean"]       = "Tiefenreinigung",
            ["BtnVault"]           = "Vault anzeigen",
            ["BtnRestore"]         = "Wiederherstellen",
            ["BtnSettings"]        = "Einstellungen",
            ["BtnAdmin"]           = "Admin-Bereich",
            ["BtnCancel"]          = "Abbrechen",
            ["BtnClose"]           = "Schliessen",
            ["BtnApply"]           = "Uebernehmen",
            ["BtnSelectAll"]       = "Alle auswaehlen",
            ["BtnDeselectAll"]     = "Alle abwaehlen",

            // Status
            ["StatusReady"]        = "Bereit.",
            ["StatusScanning"]     = "Scanne System...",
            ["StatusCleaning"]     = "Bereinigung laeuft...",
            ["StatusDone"]         = "Abgeschlossen.",
            ["StatusError"]        = "Fehler aufgetreten.",
            ["StatusNoResults"]    = "Keine Ergebnisse gefunden.",

            // Scan
            ["ScanTitle"]          = "Scan-Ergebnisse",
            ["ScanPrograms"]       = "Programme gefunden",
            ["ScanResiduals"]      = "Reste gefunden",
            ["ScanConfidence"]     = "Konfidenz",
            ["ScanSize"]           = "Groesse",
            ["ScanPath"]           = "Pfad",

            // Tabs
            ["TabPrograms"]        = "Programme",
            ["TabCleanup"]         = "Bereinigung",
            ["TabVault"]           = "Sicherung",
            ["TabSystem"]          = "System",
            ["TabAdmin"]           = "Admin",
            ["TabSettings"]        = "Einstellungen",

            // Messages
            ["MsgConfirmDelete"]   = "Sollen die ausgewaehlten Elemente wirklich geloescht werden?",
            ["MsgBackupCreated"]   = "Backup erfolgreich erstellt.",
            ["MsgRestoreComplete"] = "Wiederherstellung abgeschlossen.",
            ["MsgUpdateAvailable"] = "Ein Update ist verfuegbar!",
            ["MsgNoUpdate"]        = "Software ist aktuell.",
            ["MsgAdminRequired"]   = "Administratorrechte erforderlich.",
        };

        // English
        _translations["EN"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AppTitle"]           = "ZeroTrace - Advanced Uninstaller System",
            ["AppSubtitle"]        = "Professional Software Removal",

            ["BtnScan"]            = "Scan System",
            ["BtnUninstall"]       = "Uninstall",
            ["BtnDeepClean"]       = "Deep Clean",
            ["BtnVault"]           = "View Vault",
            ["BtnRestore"]         = "Restore",
            ["BtnSettings"]        = "Settings",
            ["BtnAdmin"]           = "Admin Panel",
            ["BtnCancel"]          = "Cancel",
            ["BtnClose"]           = "Close",
            ["BtnApply"]           = "Apply",
            ["BtnSelectAll"]       = "Select All",
            ["BtnDeselectAll"]     = "Deselect All",

            ["StatusReady"]        = "Ready.",
            ["StatusScanning"]     = "Scanning system...",
            ["StatusCleaning"]     = "Cleaning in progress...",
            ["StatusDone"]         = "Complete.",
            ["StatusError"]        = "An error occurred.",
            ["StatusNoResults"]    = "No results found.",

            ["ScanTitle"]          = "Scan Results",
            ["ScanPrograms"]       = "Programs found",
            ["ScanResiduals"]      = "Residuals found",
            ["ScanConfidence"]     = "Confidence",
            ["ScanSize"]           = "Size",
            ["ScanPath"]           = "Path",

            ["TabPrograms"]        = "Programs",
            ["TabCleanup"]         = "Cleanup",
            ["TabVault"]           = "Vault",
            ["TabSystem"]          = "System",
            ["TabAdmin"]           = "Admin",
            ["TabSettings"]        = "Settings",

            ["MsgConfirmDelete"]   = "Are you sure you want to delete the selected items?",
            ["MsgBackupCreated"]   = "Backup created successfully.",
            ["MsgRestoreComplete"] = "Restore completed.",
            ["MsgUpdateAvailable"] = "An update is available!",
            ["MsgNoUpdate"]        = "Software is up to date.",
            ["MsgAdminRequired"]   = "Administrator privileges required.",
        };

        _logger.Info($"Lokalisierung geladen: {_translations.Count} Sprachen, {_translations["DE"].Count} Schluessel");
    }
}
