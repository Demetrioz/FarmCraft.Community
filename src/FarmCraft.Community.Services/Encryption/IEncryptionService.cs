namespace FarmCraft.Community.Services.Encryption
{
    public interface IEncryptionService
    {
        string Encrypt(string value);
        string Decrypt(string encryptedValue);
    }
}
