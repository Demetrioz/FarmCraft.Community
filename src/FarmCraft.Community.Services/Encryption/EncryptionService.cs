using FarmCraft.Community.Utilities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace FarmCraft.Community.Services.Encryption
{
    public class EncryptionService : IEncryptionService
    {
        private readonly EncryptionSettings _settings;

        public EncryptionService(IOptions<EncryptionSettings> settings)
        {
            _settings = settings.Value;
        }

        public string Encrypt(string key)
        {
            using (RSACryptoServiceProvider publicProvider = new())
            {
                byte[] plainKeyBytes = Encoding.ASCII.GetBytes(key);

                RSAUtility.FromXmlString(publicProvider, _settings.PublicKey ?? "");

                byte[] encryptedKeyBytes = publicProvider.Encrypt(plainKeyBytes, false);
                return Convert.ToBase64String(encryptedKeyBytes);
            }
        }

        public string Decrypt(string key)
        {
            using (RSACryptoServiceProvider privateProvider = new())
            {
                byte[] encryptedKeyBytes = Convert.FromBase64String(key);

                RSAUtility.FromXmlString(privateProvider, _settings.PrivateKey ?? "");

                byte[] decryptedKeyBytes = privateProvider.Decrypt(encryptedKeyBytes, false);
                return Encoding.ASCII.GetString(decryptedKeyBytes);
            }
        }
    }
}