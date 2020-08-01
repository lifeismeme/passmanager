using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassManager
{
	public class Core
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
		public static void encrypt(Stream input, Stream output, byte[] Key)
		{
			const int blockSize = 16;
			byte[] IV = new byte[blockSize];
			using(var random = new RNGCryptoServiceProvider())
				random.GetBytes(IV);
			
			output.Write(IV, 0, blockSize);
			encrypt(input, output, Key, IV);
		}

		public static void decrypt(Stream input, Stream output, byte[] Key)
		{
			const int blockSize = 16;
			byte[] IV = new byte[blockSize];
			
			//first 16bytes is the stored IV
			int bytesRead = input.Read(IV, 0, blockSize);
			if (bytesRead != 16)
				throw new FileFormatException("cannot read first 16 bytes, possibility invalid file");

			decrypt(input, output, Key, IV);
		}

		public static void encrypt(Stream input, Stream output, byte[] Key, byte[] IV)
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

				using (CryptoStream csEncrypt = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
				{
					int b;
					while ((b = input.ReadByte()) != -1)
					{
						csEncrypt.WriteByte((byte)b);
					}
					csEncrypt.FlushFinalBlock();
				}


			}

		}

		public static void decrypt(Stream input, Stream output, byte[] Key, byte[] IV)
		{
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("blank agruement Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("blank agruement IV");

			using (Aes aes = Aes.Create())
			{
				aes.Key = Key;
				aes.IV = IV;


				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);


				using (var csDecrypt = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
				{

					int b;
					while ((b = csDecrypt.ReadByte()) != -1)
					{
						output.WriteByte((byte)b);
					}
				}

			}

		}

		public static void serializeToJson(Object data, string path)
		{
			var x = new JsonSerializerOptions();
			x.WriteIndented = true;
			x.Converters.Add(new VaultConverter());
			string json = JsonSerializer.Serialize(data,x);
			File.WriteAllText(path, json);
		}

		public static Vault<T> deserializeJson<T>(string path)
		{
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<Vault<T>>(json);
		}
	}
}
