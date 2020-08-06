using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PassManager
{
	public class Core
	{
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

			using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
			{
				random.GetBytes(salt);
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

		public static void serializeToJsonNiceFormat(Object data, string path)
		{
			var Option = new JsonSerializerOptions();
			Option.WriteIndented = true;
			string json = JsonSerializer.Serialize(data, Option);
			File.WriteAllText(path, json);
		}

		public static void serializeToJson(Object data, string path)
		{
			string json = JsonSerializer.Serialize(data);
			File.WriteAllText(path, json);
		}

		public static Vault<T> deserializeJson<T>(string path)
		{
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<Vault<T>>(json);
		}

		public static byte[] generateRandomPassword(int length)
		{
			const int minPrintableASIIC = 33;
			const int overPrintableASIIC = 127;
			byte[] oneByte = new byte[1];
			byte[] password = new byte[length];
			using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
			{
				for (int i = 0; i < length; ++i)
				{
					random.GetBytes(oneByte);
					int printableASIIC;
					while ((printableASIIC = (oneByte[0] + minPrintableASIIC) % overPrintableASIIC) < minPrintableASIIC)
					{
						random.GetBytes(oneByte);
					}
					password[i] = (byte)printableASIIC;
				}
			}
			return password;
		}

		public static double calcPasswordBits(string password)
		{
			Regex Lower = new Regex(@"[a-z]");
			Regex Upper = new Regex(@"[A-Z]");
			Regex Digit = new Regex(@"[0-9]");
			Regex Symbol = new Regex(@"[\~\!\@\#\$\%\^\&\*\(\)_\+\|\{\}\:\""\<\>\?\`\-\=\\\[\]\;\'\,\.\/]");

			int charBase = 0;
			if (Lower.IsMatch(password))
				charBase += 26;
			if (Upper.IsMatch(password))
				charBase += 26;
			if (Digit.IsMatch(password))
				charBase += 10;
			if (Symbol.IsMatch(password))
				charBase += 32;

			return  Math.Log( Math.Pow(charBase, password.Length), 2);
		}
	}
}
