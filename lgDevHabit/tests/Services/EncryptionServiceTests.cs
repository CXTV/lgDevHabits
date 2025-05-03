using System.Security.Cryptography;
using lgDevHabit.Api.Services;
using lgDevHabit.Api.Settings;
using Microsoft.Extensions.Options;

namespace lgDevHabit.UnitTests.Services;


public sealed class EncryptionServiceTests
{
    private readonly EncryptionService _encryptionService;

    //在每个测试运行前，自动创建一个带有随机密钥的加密服务，确保测试时服务能正常加解密。
    public EncryptionServiceTests()
    {
        IOptions<EncryptionOptions> options = Options.Create(new EncryptionOptions
        {
            Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        });
        _encryptionService = new EncryptionService(options);
    }

    [Fact]
    public void Decrypt_ShouldReteurnPlainText_WhenDecryptingCorrectCipherText()
    {
        // Arrange
        const string plainText = "sensitive data";
        string cipherText = _encryptionService.Encrypt(plainText);

        // Act
        string decryptedText = _encryptionService.Decrypt(cipherText);

        // Assert
        Assert.Equal(plainText, decryptedText);
    }
}
