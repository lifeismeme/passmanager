using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassManager;

namespace PassManagerUnitTests
{
	[TestClass]
	public class CoreTests
	{
		private readonly static DateTime NOW = DateTime.Now;
		private static Vault<Credential> sampleVault()
		{
			var v = new Vault<Credential>();
			v.Add(new Credential()
			{
				Id = 1,
				Title = "xx new tittle",
				Username = "xx username",
				Password = new char[] { 'p', '@', 'S', 's', 'W', '0', 'r', 'd', '?' },
				Description = "some description....",
				Creation = NOW,
				LastModified = NOW
			});
			v.Add(new Credential()
			{
				Id = 2,
				Title = "xx new tittle2",
				Username = "xx username2",
				Password = new char[] { 'x', '@', '2', 's', 'W', '0', 'r', 'd', '?' },
				Description = "some description2....",
				Creation = NOW,
				LastModified = NOW
			});
			return v;
		}
		const string fileName = "hello.txt";
		const string path = @"C:\Users\mlcmi\Desktop\passmanager\output\" + fileName;
		[TestMethod]
		public void SerializeToAndDeserializeJson_Vault_SameVault()
		{
			var vault = sampleVault();
			SerializeToJson_Vault_FileCreated(vault);
			DeserializeJsonToVault_JsonFile_Vault(vault);
		}
		private void SerializeToJson_Vault_FileCreated(Vault<Credential> vault)
		{
			try
			{
				Core.serializeToJsonNiceFormat(vault, path);
				Assert.IsTrue(File.Exists(path));

				var savedVault = Core.deserializeJson<Credential>(path);
				Assert.AreEqual(vault, savedVault);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		private void DeserializeJsonToVault_JsonFile_Vault(Vault<Credential> previousVault)
		{
			try
			{
				Assert.IsTrue(File.Exists(path));

				var savedVault = Core.deserializeJson<Credential>(path);

				Assert.AreEqual(previousVault, savedVault);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}
		}



		const string pathEnc = path + ".encrypted.txt";
		const string samplePassword = "somepassword";
		[TestMethod]
		public void Encrypt_File_EncryptedFile()
		{
			Assert.IsTrue(File.Exists(path));
			try
			{
				using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
				{
					using (var output = new BufferedStream(new FileStream(pathEnc, FileMode.Create)))
					{
						Core.encrypt(input, output, samplePassword);
					}
				}

				Assert.IsTrue(File.Exists(pathEnc));
				Assert.IsFalse(Enumerable.SequenceEqual(File.ReadAllBytes(path), File.ReadAllBytes(pathEnc)));
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		[TestMethod]
		public void Decrypt_EncryptedFile_OriginalFile()
		{
			Assert.IsTrue(File.Exists(pathEnc));
			string pathDec = path + ".decrypted.txt";
			try
			{
				using (var input = new BufferedStream(new FileStream(pathEnc, FileMode.Open)))
				{
					using (var output = new BufferedStream(new FileStream(pathDec, FileMode.Create)))
					{
						Core.decrypt(input, output, samplePassword);
					}
				}

				Assert.IsTrue(File.Exists(pathDec));
				Assert.IsFalse(Enumerable.SequenceEqual(File.ReadAllBytes(pathEnc), File.ReadAllBytes(pathDec)));
				Assert.IsTrue(Enumerable.SequenceEqual(File.ReadAllBytes(path), File.ReadAllBytes(pathDec)));
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		[TestMethod]
		public void GenerateKey_String_ByteKey()
		{
			string password = "!@#$%^&*()1234567890Abzc[]{}-=\\`~<>,.;':\"";
			byte[] emptyKey = new byte[Core.KEY_SIZE_BYTE];
			byte[] emptySalt = new byte[Core.SALT_SIZE_BYTE];

			//act1
			byte[] key = new byte[Core.KEY_SIZE_BYTE];
			byte[] salt = new byte[Core.SALT_SIZE_BYTE];
			Core.setGeneratedKeyAndSalt(password, ref key, ref salt);

			Assert.IsTrue(key.Length == Core.KEY_SIZE_BYTE);
			Assert.IsTrue(salt.Length == Core.SALT_SIZE_BYTE);
			Assert.IsFalse(Enumerable.SequenceEqual(emptyKey, key));
			Assert.IsFalse(Enumerable.SequenceEqual(emptySalt, salt));
			Assert.IsFalse(Enumerable.SequenceEqual(key, salt));


			//act2
			string password2 = "somepassword";

			byte[] key2 = new byte[Core.KEY_SIZE_BYTE];
			byte[] salt2 = new byte[Core.SALT_SIZE_BYTE];
			Core.setGeneratedKeyAndSalt(password2, ref key2, ref salt2);

			Assert.IsTrue(key2.Length == Core.KEY_SIZE_BYTE);
			Assert.IsTrue(salt2.Length == Core.SALT_SIZE_BYTE);
			Assert.IsFalse(Enumerable.SequenceEqual(emptyKey, key2));
			Assert.IsFalse(Enumerable.SequenceEqual(emptySalt, salt2));
			Assert.IsFalse(Enumerable.SequenceEqual(key2, salt2));

			Assert.IsFalse(Enumerable.SequenceEqual(key2, key));
			Assert.IsFalse(Enumerable.SequenceEqual(salt2, salt));
		}

		[TestMethod]
		public void GenerateRandomPassword_LengthN_RandomPrintableASCIIwithNcharacters()
		{
			const int len = 30;
			const int minPrintableASIIC = 33;
			const int maxPrintableASIIC = 126;

			var password = Core.generateRandomPassword(len);
			var password2 = Core.generateRandomPassword(len);
			Debug.WriteLine(System.Text.Encoding.ASCII.GetString(password));

			foreach (byte c in password)
				Assert.IsTrue(c >= minPrintableASIIC && c <= maxPrintableASIIC);
			foreach (byte c in password2)
				Assert.IsTrue(c >= minPrintableASIIC && c <= maxPrintableASIIC);

			Assert.IsTrue(password.Length == len);
			Assert.IsTrue(password2.Length == len);
			Assert.IsFalse(Enumerable.SequenceEqual(password, password2));
		}


		[TestMethod]
		public void CalcPassStrength_PasswordByCharBase_PossibleComposition()
		{
			const string lower10 = "abcdefghij";
			const string upper10 = "KLMNOPQRST";
			const string digit10 = "0123456789";
			const string symbols10 = @"!#\[',.""_-";
			Assert.IsTrue(lower10.Length == 10);
			Assert.IsTrue(upper10.Length == 10);
			Assert.IsTrue(digit10.Length == 10);
			Assert.IsTrue(symbols10.Length == 10);

			double lower10Bits = Core.calcPasswordBits(lower10);
			double upper10Bits = Core.calcPasswordBits(upper10);
			double digit10Bits = Core.calcPasswordBits(digit10);
			double symbolsBits = Core.calcPasswordBits(symbols10);

			Assert.IsTrue(lower10Bits == Math.Log(Math.Pow(26, lower10.Length), 2));
			Assert.IsTrue(upper10Bits == Math.Log(Math.Pow(26, upper10.Length), 2));
			Assert.IsTrue(digit10Bits == Math.Log(Math.Pow(10, digit10.Length), 2));
			Assert.IsTrue(symbolsBits == Math.Log(Math.Pow(32, symbols10.Length), 2));
		}

		[TestMethod]
		public void CalcPassStrength_MixCharBase_PossibleComposition()
		{
			const string alphabet20 = "abcdefghijKLMNOPQRST";
			const string digit10 = "0987654321";
			const string symbols10 = @"\.""[]/^*(`";
			Assert.IsTrue(alphabet20.Length == 20);
			Assert.IsTrue(digit10.Length == 10);
			Assert.IsTrue(symbols10.Length == 10);

			const string alphanumeric30 = alphabet20 + digit10;
			const string alphaSymbols30 = alphabet20 + symbols10;
			const string alphaNumSymb40 = alphabet20 + digit10 + symbols10 + alphabet20;
			const string digitSymbols20 = digit10 + symbols10;

			//act
			double bits_alphanumeric30 = Core.calcPasswordBits(alphanumeric30);
			double bits_alphaSymbols30 = Core.calcPasswordBits(alphaSymbols30);
			double bits_alphaNumSymb40 = Core.calcPasswordBits(alphaNumSymb40);
			double bits_digitSymbols20 = Core.calcPasswordBits(digitSymbols20);

			//assert
			double bitForCharBase62 = Math.Log(Math.Pow(52 + 10, alphanumeric30.Length), 2);
			double bitForCharBase84 = Math.Log(Math.Pow(52 + 32, alphaSymbols30.Length), 2);
			double bitForCharBase94 = Math.Log(Math.Pow(52 + 10 + 32, alphaNumSymb40.Length), 2);
			double bitForCharBase42 = Math.Log(Math.Pow(10 + 32, digitSymbols20.Length), 2);
			Assert.IsTrue(bits_alphanumeric30 == bitForCharBase62);
			Assert.IsTrue(bits_alphaSymbols30 == bitForCharBase84);
			Assert.IsTrue(bits_alphaNumSymb40 == bitForCharBase94);
			Assert.IsTrue(bits_digitSymbols20 == bitForCharBase42);
		}
	}
}
