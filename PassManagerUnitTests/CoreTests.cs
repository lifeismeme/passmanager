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
		public void SerializeToJson_VaultOfCredential_FileCreated()
		{
			var vault = sampleVault();

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

		[TestMethod]
		public void DeserializeJsonToVault_JsonFile_VaultOfCredential()
		{
			var sampleV = sampleVault();

			try
			{
				Assert.IsTrue(File.Exists(path));

				var savedVault = Core.deserializeJson<Credential>(path);

				Assert.AreEqual(sampleV, savedVault);
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
	}
}
