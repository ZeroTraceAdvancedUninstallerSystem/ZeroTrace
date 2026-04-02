// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Models;

/// <summary>Where a program was discovered.</summary>
public enum ProgramSource
{
    Unknown              = 0,
    RegistryLocalMachine = 1,
    RegistryCurrentUser  = 2,
    Msi                  = 3,
    Store                = 4,
    Portable             = 5,
    Custom               = 6
}
