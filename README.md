ZeroTrace - Forensic Uninstaller System 🛡️

Author: Mario B. | License: MIT | .NET 8.0 | Windows 10/11
🇩🇪 Deutsch: Projektbeschreibung

ZeroTrace ist eine professionelle Open-Source-Anwendung für die rückstandslose Softwareentfernung unter Windows. Im Gegensatz zu Standard-Uninstallern nutzt ZeroTrace fortschrittliche forensische Methoden, um sicherzustellen, dass keine digitalen Fußabdrücke auf dem System verbleiben.
Kernfunktionen

    Deep Scan Engine (RAE): Die Residual Analysis Engine nutzt heuristisches Scoring, um verwaiste Dateien, Ordner und Registry-Leichen aufzuspüren.

    Forensisches Schreddern: Speziell für SSD & NVMe optimierte Löschalgorithmen überschreiben Daten mit kryptografischem Rauschen, um eine Wiederherstellung zu verhindern.

    VSS-Purge (Anti-Forensik): Vernichtet gezielt Windows-Schattenkopien (Volume Shadow Copies), damit gelöschte Programme nicht über Systemwiederherstellungspunkte reaktiviert werden können.

    Sicherheits-Vault: Automatische Backups vor jeder Löschung. Backups sind mit AES-256-GCM verschlüsselt und via SHA-256 integritätsgeprüft.

    Service-Management: Erkennt und stoppt aktive Hintergrunddienste und Prozesse, die Datei-Löschungen blockieren könnten.

    System-Hygiene: Reinigung von Browser-Daten, temporären Verzeichnissen und Verwaltung von Autostart-Einträgen.

Anforderungen

    Windows 10/11 (64-bit)

    .NET 8.0 Runtime

    Administratorrechte (zwingend erforderlich für VSS-Purge und Registry-Zugriff)

🇺🇸 English: Project Description

ZeroTrace is a professional open-source utility designed for the complete removal of software on Windows. Unlike standard uninstallers, ZeroTrace employs advanced forensic methods to ensure that no digital footprints remain on your system.
Key Features

    Deep Scan Engine (RAE): The Residual Analysis Engine uses confidence-based scoring to detect orphaned files, folders, and registry bloat.

    Forensic Shredding: Specifically optimized for SSD & NVMe storage, using cryptographic noise overwriting to prevent data recovery.

    VSS Purge (Anti-Forensics): Targets and destroys Windows Volume Shadow Copies to ensure that deleted applications cannot be restored via system restore points.

    Safety Vault: Automatic atomic backups before any deletion. Backups are encrypted using AES-256-GCM with SHA-256 integrity verification.

    Service Management: Detects and terminates background services and processes that typically lock files during uninstallation.

    System Hygiene: Cleans browser data, temporary directories, and manages startup items.

Requirements

    Windows 10/11 (64-bit)

    .NET 8.0 Runtime

    Administrator Privileges (strictly required for VSS Purge and Registry access)
