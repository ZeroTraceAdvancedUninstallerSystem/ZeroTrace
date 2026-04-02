namespace ZeroTrace.Core.Crypto;

public interface ICryptoService
{
    Task<byte[]> DeriveKeyAsync(
        string password,
        byte[] salt,
        int keySizeBytes,
        CancellationToken cancellationToken = default);

    Task<string> ComputeBlake3HexAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task<byte[]> EncryptAsync(
        byte[] plaintext,
        byte[] key,
        byte[] nonce,
        byte[]? associatedData = null,
        CancellationToken cancellationToken = default);

    Task<byte[]> DecryptAsync(
        byte[] ciphertext,
        byte[] key,
        byte[] nonce,
        byte[]? associatedData = null,
        CancellationToken cancellationToken = default);
}
