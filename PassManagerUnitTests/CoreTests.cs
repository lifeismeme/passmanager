using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassManager;

namespace PassManagerUnitTests
{
	[TestClass]
	public class CoreTests
	{
		[TestMethod]
		public void SerializeToJson_VaultOfCredential_FileCreated()
		{
			const string fileName = "hello.txt";

			const string path = @"C:\Users\mlcmi\Desktop\passmanager\output\" + fileName;
			var vault = new Vault<Credential>() {
				new Credential()
				{
					Id =1,
					Title="xx new tittle",
					Username="xx username",
					Password=new char[]{'p','@','S','s','W','0','r','d','?'},
					Description="some description...."
				},
				new Credential()
				{
					Id =2,
					Title="xx new tittle2",
					Username="xx username2",
					Password=new char[]{'x','@','2','s','W','0','r','d','?'},
					Description="some description2...."
				}
			};

			try
			{
				Core.serializeToJson(vault, path);
				Assert.IsTrue(File.Exists(path));

				var savedVault = Core.deserializeJson<Credential>(path);
				Assert.AreEqual(vault, savedVault);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.ToString());
			}


		}
	}
}
