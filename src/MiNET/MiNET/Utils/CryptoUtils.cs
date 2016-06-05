using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Jose;

namespace MiNET.Utils
{
	public static class CryptoUtils
	{
		public static byte[] GetDerEncoded(this ECDiffieHellmanPublicKey key)
		{
			byte[] asn = new byte[24]
			{
				0x30, 0x76, 0x30, 0x10, 0x6, 0x7, 0x2a, 0x86, 0x48, 0xce, 0x3d, 0x2,
				0x1, 0x6, 0x5, 0x2b, 0x81, 0x4, 0x0, 0x22, 0x3, 0x62, 0x0, 0x4
			};

			return asn.Concat(key.ToByteArray().Skip(8)).ToArray();
		}

		public static ECDiffieHellmanPublicKey CreateEcDiffieHellmanPublicKey(string clientPubKeyString)
		{
			byte[] clientPublicKeyBlob = Base64Url.Decode(clientPubKeyString);
			clientPublicKeyBlob = FixPublicKey(clientPublicKeyBlob.Skip(23).ToArray());

			ECDiffieHellmanPublicKey clientKey = ECDiffieHellmanCngPublicKey.FromByteArray(clientPublicKeyBlob, CngKeyBlobFormat.EccPublicBlob);
			return clientKey;
		}

		private static byte[] FixPublicKey(byte[] publicKeyBlob)
		{
			var keyType = new byte[] {0x45, 0x43, 0x4b, 0x33};
			var keyLength = new byte[] {0x30, 0x00, 0x00, 0x00};

			return keyType.Concat(keyLength).Concat(publicKeyBlob.Skip(1)).ToArray();
		}

		public static ECDiffieHellmanPublicKey ImportEccPublicKeyFromCertificate(X509Certificate2 cert)
		{
			var keyType = new byte[] {0x45, 0x43, 0x4b, 0x33};
			var keyLength = new byte[] {0x30, 0x00, 0x00, 0x00};
			var key = cert.PublicKey.EncodedKeyValue.RawData.Skip(1);
			var keyImport = keyType.Concat(keyLength).Concat(key).ToArray();

			//Assert.AreEqual(privateKey, keyImport);

			return ECDiffieHellmanCngPublicKey.FromByteArray(keyImport, CngKeyBlobFormat.EccPublicBlob);
		}

		/// <summary>
		/// Used to create a CngKey that can be used to verify JWT content.
		/// </summary>
		/// <param name="clientPubKeyString"></param>
		/// <returns></returns>
		public static CngKey ImportECDsaCngKeyFromString(string clientPubKeyString)
		{
			byte[] clientPublicKeyBlob = Base64Url.Decode(clientPubKeyString);
			byte[] key = clientPublicKeyBlob.Skip(23).ToArray();

			var keyType = new byte[] {0x45, 0x43, 0x53, 0x33};
			var keyLength = new byte[] {0x30, 0x00, 0x00, 0x00};

			var keyImport = keyType.Concat(keyLength).Concat(key.Skip(1)).ToArray();

			var cngKey = CngKey.Import(keyImport, CngKeyBlobFormat.EccPublicBlob);
			var crypto = new ECDsaCng(cngKey);

			return crypto.Key;
		}
	}
}