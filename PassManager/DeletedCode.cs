using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassManager
{
	class DeletedCode
	{
		static void copyPast(string path, string destinationPath, string fileFullName)
		{
			if (!File.Exists(path))
				return;

			byte[] data = File.ReadAllBytes(path);

			if (!Directory.Exists(destinationPath))
				return;

			File.WriteAllBytes(destinationPath + $"/{fileFullName}", data);

		}

		static byte[] encrypt(byte[] inputData, byte[] Key, byte[] IV)
		{
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("blank agruement Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("blank agruement IV");

			using (Aes aes = Aes.Create())
			{
				aes.Key = Key;
				aes.IV = IV;

				// Create an encryptor to perform the stream transform.
				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
				using (var outputBuffer = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(outputBuffer, encryptor, CryptoStreamMode.Write))
					{
						csEncrypt.Write(inputData, 0, inputData.Length);

						csEncrypt.FlushFinalBlock();
						return outputBuffer.ToArray();

					}
				}

			}

		}
		static byte[] decrypt(byte[] inputCipher, byte[] Key, byte[] IV)
		{
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("blank agruement Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("blank agruement IV");

			using (Aes aes = Aes.Create())
			{
				aes.Key = Key;
				aes.IV = IV;

				// Create a decryptor to perform the stream transform.
				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
				using (var inbuffer = new MemoryStream(inputCipher))
				{

					using (var csDecrypt = new CryptoStream(inbuffer, decryptor, CryptoStreamMode.Read))
					{
						byte[] data = new byte[inputCipher.Length];

						csDecrypt.Read(data, 0, data.Length);

						return data;
					}
				}
			}
		}
	}
}
