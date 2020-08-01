using System;
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
		private static Vault<Vault> sampleVault()
		{
			return new Vault<Vault>() {
				new Vault()
				{
					Id =1,
					Title="xx new tittle",
					Username="xx username",
					Password=new char[]{'p','@','S','s','W','0','r','d','?'},
					Description="some description....",
					Creation=NOW,
					LastModified = NOW
				},
				new Vault()
				{
					Id =2,
					Title="xx new tittle2",
					Username="xx username2",
					Password=new char[]{'x','@','2','s','W','0','r','d','?'},
					Description="some description2....",
					Creation = NOW,
					LastModified = NOW
				}
			};
		}
		const string fileName = "hello.txt";
		const string path = @"C:\Users\mlcmi\Desktop\passmanager\output\" + fileName;
		[TestMethod]
		public void SerializeToJson_VaultOfCredential_FileCreated()
		{
			var vault = sampleVault();

			try
			{
				Core.serializeToJson(vault, path);
				Assert.IsTrue(File.Exists(path));

				var savedVault = Core.deserializeJson<Vault>(path);
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

				var savedVault = Core.deserializeJson<Vault>(path);

				Assert.AreEqual(sampleV, savedVault);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}
		}



			const string pathEnc = path + ".encrypted.txt";
		[TestMethod]
		public void Encrypt()
		{
			Assert.IsTrue(File.Exists(path));
			try
			{
				byte[] key = new byte[32];
				using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
				{
					using (var output = new BufferedStream(new FileStream(pathEnc, FileMode.Create)))
					{
						Core.encrypt(input, output, key);
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
		public void Decrypt()
		{
			Assert.IsTrue(File.Exists(pathEnc));
			string pathDec = path + ".decrypted.txt";
			try
			{
				byte[] key = new byte[32];
				using (var input = new BufferedStream(new FileStream(pathEnc, FileMode.Open)))
				{
					using (var output = new BufferedStream(new FileStream(pathDec, FileMode.Create)))
					{
						Core.decrypt(input, output, key);
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
	}
}
