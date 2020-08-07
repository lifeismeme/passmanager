using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassManager;

namespace PassManagerUnitTests
{
	[TestClass]
	public class ViewModelTests
	{
		const string path = @"C:\Users\mlcmi\Desktop\passmanager\output\testnewvault.encryptedvault";
		const string samplePass = "somepassword";
		[TestMethod]
		public void EncryptAndSaveVaultAndLoadVault_NewPasswordAndNewVault_NewEncryptedFile()
		{
			try
			{
				File.Delete(path);
				var vmodel = new ViewModel();
				vmodel.EncryptAndSaveVault(path, samplePass, vmodel.Vault);

				Assert.IsTrue(File.Exists(path));

				var vmodel2 = new ViewModel();
				vmodel2.LoadVault(path, samplePass);

				Assert.AreEqual(vmodel.Vault, vmodel2.Vault);
			}
			catch (IOException ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		[TestMethod]
		public void EncryptAndSave_OldVaultWithChanges_NewVault()
		{
			try
			{
				var vmodelOld = new ViewModel();
				vmodelOld.LoadVault(path, samplePass);

				var vmodelToModify = new ViewModel();
				vmodelToModify.LoadVault(path, samplePass);

				vmodelToModify.Vault.Add(new Credential());
				vmodelToModify.EncryptAndSaveVault(path, samplePass, vmodelToModify.Vault);

				var vmodelModified = new ViewModel();
				vmodelModified.LoadVault(path, samplePass);

				Assert.AreNotEqual(vmodelOld.Vault, vmodelModified.Vault);
				Assert.AreEqual(vmodelToModify.Vault, vmodelModified.Vault);
			}
			catch (IOException ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		[TestMethod]
		public void EncryptAndSave_WrongPassword_NotSaving()
		{
			string wrongPass = samplePass + "ohla";
			try
			{
				var vmodelOld = new ViewModel();
				vmodelOld.LoadVault(path, samplePass);

				var vmodelToModify = new ViewModel();
				vmodelToModify.LoadVault(path, samplePass);

				vmodelToModify.Vault.Add(new Credential());

				Assert.AreNotEqual(vmodelOld.Vault, vmodelToModify.Vault);
				try
				{
					vmodelToModify.EncryptAndSaveVault(path, wrongPass, vmodelToModify.Vault);

					Assert.Fail("vault saved and overwritten with wrong a password");
				}
				catch (ArgumentException)
				{
					Assert.IsTrue(true);
				}
				var vmodelReloaded = new ViewModel();
				vmodelReloaded.LoadVault(path, samplePass);
				Assert.AreEqual(vmodelOld.Vault, vmodelReloaded.Vault);
				Assert.AreNotEqual(vmodelToModify.Vault, vmodelReloaded.Vault);
			}
			catch (IOException ex)
			{
				Assert.Fail(ex.ToString());
			}
		}

		[TestMethod]
		public void ChangeVaultPasswordThenEncryptAndSave_oldAndNewPassword_VaultRequireNewPasswordOnly()
		{
			string oldPass = samplePass;
			string newPass = samplePass + "new1";
			try
			{
				var vmodel = new ViewModel();
				vmodel.LoadVault(path, oldPass);

				vmodel.ChangeVaultPasswordThenEncryptAndSave(path, samplePass, newPass, vmodel.Vault);

				try
				{
					new ViewModel().LoadVault(path, samplePass);
					Assert.Fail("vault still uses old password");
				}
				catch (IOException)
				{
					Assert.IsTrue(true);
				}

				var vmodelReloaded = new ViewModel();
				vmodelReloaded.LoadVault(path, newPass);
				Assert.AreEqual(vmodelReloaded.Vault, vmodel.Vault);
			}
			catch (IOException ex)
			{
				Assert.Fail(ex.ToString());
			}
		}
	}
}
