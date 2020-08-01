using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public const int SALT_SIZE_BYTE = 8;
		public const int KEY_SIZE_BYTE = 32;
		const int ITERATION = 1000;

		public static byte[] regenerateKey(string password, byte[] salt)
		{
			if (salt.Length != SALT_SIZE_BYTE)
				throw new ArgumentException("invalid salt size");

			Rfc2898DeriveBytes pbdkf2 = new Rfc2898DeriveBytes(password, salt, ITERATION);

			return pbdkf2.GetBytes(KEY_SIZE_BYTE);

		}
		public static void setGeneratedKeyAndSalt(string password, ref byte[] key, ref byte[] salt)
		{

			if (key.Length != KEY_SIZE_BYTE)
				throw new ArgumentException("invalid key size");
			if (salt.Length != SALT_SIZE_BYTE)
				throw new ArgumentException("invalid salt size");

			using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
			{
				rngCsp.GetBytes(salt);
				Rfc2898DeriveBytes pbdkf2 = new Rfc2898DeriveBytes(password, salt, ITERATION);

				key = pbdkf2.GetBytes(KEY_SIZE_BYTE);
			}
		}

		const int BLOCK_SIZE_BYTE = 16;
		public static void encrypt(Stream input, Stream output, string password)
		{
			byte[] IV = new byte[BLOCK_SIZE_BYTE];
			using (var random = new RNGCryptoServiceProvider())
				random.GetBytes(IV);

			output.Write(IV, 0, IV.Length);

			byte[] key = new byte[KEY_SIZE_BYTE];
			byte[] salt = new byte[SALT_SIZE_BYTE];
			setGeneratedKeyAndSalt(password, ref key, ref salt);
			try
			{
				//after IV, append the salt
				output.Write(salt, 0, salt.Length);

				encrypt(input, output, key, IV);
			}
			finally
			{
				for (int i = 0; i < key.Length; ++i)
					key[i] = 0;
			}
		}

		public static void decrypt(Stream input, Stream output, string password)
		{
			//first 16bytes is the stored IV
			byte[] IV = new byte[BLOCK_SIZE_BYTE];
			int bytesRead = input.Read(IV, 0, BLOCK_SIZE_BYTE);
			if (bytesRead != BLOCK_SIZE_BYTE)
				throw new FileFormatException($"cannot read first {BLOCK_SIZE_BYTE} bytes, possibility invalid file");
			
			//then 8bytes is the stored Salt that used to derive the key
			byte[] salt = new byte[SALT_SIZE_BYTE];
			bytesRead = input.Read(salt, 0, salt.Length);
			if (bytesRead != SALT_SIZE_BYTE)
				throw new FileFormatException($"cannot read salt of {SALT_SIZE_BYTE} bytes, possibility invalid file");

			byte[] key = regenerateKey(password, salt);
			
			try
			{
				decrypt(input, output, key, IV);
			}
			finally
			{
				for (int i = 0; i < key.Length; ++i)
					key[i] = 0;
			}
		}

		private static void encrypt(Stream input, Stream output, byte[] Key, byte[] IV)
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
						csEncrypt.WriteByte((byte)b);

					csEncrypt.FlushFinalBlock();
				}
			}

		}

		private static void decrypt(Stream input, Stream output, byte[] Key, byte[] IV)
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
						output.WriteByte((byte)b);
				}

			}

		}

		public static void serializeToJson(Object data, string path)
		{
			var x = new JsonSerializerOptions();
			x.WriteIndented = true;
			string json = JsonSerializer.Serialize(data, x);
			File.WriteAllText(path, json);
		}

		public static Vault<T> deserializeJson<T>(string path)
		{
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<Vault<T>>(json);
		}
	}
}
