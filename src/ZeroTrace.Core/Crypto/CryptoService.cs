// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Security.Cryptography;
using System.Text;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Crypto;

public sealed class CryptoService
{
    private readonly IZeroTraceLogger _logger;
    private const int KeySize   = 32;
    private const int NonceSize = 12;
    private const int TagSize   = 16;

    public CryptoService(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public byte[] DeriveKey(string password, byte[] salt, int iterations = 100000)
    {
        using var kdf = new Rfc2898DeriveBytes(
            password, salt, iterations, HashAlgorithmName.SHA256);
        return kdf.GetBytes(KeySize);
    }

    public static byte[] GenerateSalt(int length = 16) =>
        RandomNumberGenerator.GetBytes(length);

    public async Task<string> ComputeFileHashAsync(
        string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash);
    }

    public static string ComputeStringHash(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    public byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var cipher = new byte[plaintext.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, cipher, tag);
        var result = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipher, 0, result, NonceSize + TagSize, cipher.Length);
        _logger.Debug($"Verschluesselt: {plaintext.Length} -> {result.Length} Bytes");
        return result;
    }

    public byte[] Decrypt(byte[] combined, byte[] key)
    {
        if (combined.Length < NonceSize + TagSize)
            throw new CryptographicException("Daten zu kurz fuer AES-GCM.");
        var nonce = combined.AsSpan(0, NonceSize).ToArray();
        var tag = combined.AsSpan(NonceSize, TagSize).ToArray();
        var cipher = combined.AsSpan(NonceSize + TagSize).ToArray();
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);
        _logger.Debug($"Entschluesselt: {combined.Length} -> {plain.Length} Bytes");
        return plain;
    }

    public async Task EncryptFileAsync(
        string sourcePath, string destPath, byte[] key, CancellationToken ct = default)
    {
        var data = await File.ReadAllBytesAsync(sourcePath, ct);
        var encrypted = Encrypt(data, key);
        await File.WriteAllBytesAsync(destPath, encrypted, ct);
        _logger.Info($"Datei verschluesselt: {Path.GetFileName(sourcePath)}");
    }

    public async Task DecryptFileAsync(
        string sourcePath, string destPath, byte[] key, CancellationToken ct = default)
    {
        var data = await File.ReadAllBytesAsync(sourcePath, ct);
        var decrypted = Decrypt(data, key);
        await File.WriteAllBytesAsync(destPath, decrypted, ct);
        _logger.Info($"Datei entschluesselt: {Path.GetFileName(sourcePath)}");
    }
}
