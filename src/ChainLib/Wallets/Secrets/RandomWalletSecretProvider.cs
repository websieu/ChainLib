using System.Security.Cryptography;

namespace ChainLib.Wallets.Secrets
{
    /// <summary>
    /// Generates a wallet using high-entropy, cryptographically secure random data.
    /// 
    /// This type of wallet is more secure, since the private key must be obtained in addition
    /// to any additional security, and cannot be re-created by any other means, such as a 
    /// passphrase.
    /// 
    /// The downside is the wallet user has to know what they are doing.
    /// </summary>
    public class RandomWalletSecretProvider : IWalletSecretProvider
    {
        private readonly ushort _buffer;

        public RandomWalletSecretProvider(ushort bitsOfEntropy = 256)
        {
            _buffer = (ushort) (bitsOfEntropy / 8);
        }

        public byte[] GenerateSecret(params object[] args)
	    {
			byte[] randomBytes = new byte[_buffer];
		    using (var rng = RandomNumberGenerator.Create())
			    rng.GetBytes(randomBytes);
		    return randomBytes;
		}
    }    
}