using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PassManager
{
	public class ViewModel : IDisposable
	{
		public string LoadedVaultPath { get; private set; } = "";
		public string VaultPath { get; set; }
		public Vault<Credential> Vault { get; private set; }

		public ViewModel()
		{
			Vault = new Vault<Credential>();
		}

		public ClipboardCleaner Clipboard { get; } = new ClipboardCleaner();


		public void initSample()
		{
			Vault.Add(new Credential
			{
				Id = 1,
				Username = "hello world",
				Password = new char[] { 'B', 'B', 'C' },
				Description = "good by.\nh",
				Title = "o0o"
			});
			Vault.Add(new Credential
			{
				Id = 2,
				Password = new char[] { 'A', 'b', 'a' },
				Title = "pp"
			});
			Vault.Add(new Credential
			{
				Id = 3,
				Username = "hello world",
				Password = new char[] { 'x', 'x', 'C' },
				Description = "goodcccc by.\nsh",
				Title = "xAsd23 sd"
			});
		}


		public bool IsValidVaultPassword(string path, string password, Vault<Credential> OfVault)
		{
			try
			{
				string vaultData = null;
				using (var buffer = new MemoryStream())
				{
					using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
					{
						Core.decrypt(input, buffer, password);
					}

					buffer.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(buffer))
					{
						vaultData = reader.ReadToEnd();
					}
				}
				if (vaultData == null || vaultData == "")
					throw new IOException("invalid file, no data");

				var tempVault = JsonSerializer.Deserialize<Vault<Credential>>(vaultData);
				if (!DateTime.Equals(tempVault.Creation, OfVault.Creation)
					|| DateTime.Compare(tempVault.LastModified, OfVault.LastModified) > 0)
					throw new IOException("vault is different?\n" + vaultData);

				return true;
			}
			catch (CryptographicException ex)
			{
				//invalid pass or file
				Logger.Log(ex);
			}
			catch (JsonException ex)
			{
				//invalid file
				Logger.Log(ex);
			}
			catch (IOException ex)
			{
				Logger.Log(ex);
			}

			return false;
		}

		public void EncryptAndSaveVault(string path, string password, Vault<Credential> VaultToEncryptAndSave)
		{
			string tempLockPath = $"{path}.lock";

			try
			{
				using (var input = new MemoryStream(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(VaultToEncryptAndSave)), false))
				{
					using (var output = new BufferedStream(new FileStream(tempLockPath, FileMode.Create)))
					{
						Core.encrypt(input, output, password);
					}
				}

				if (!File.Exists(tempLockPath))
					throw new IOException("failed to save file");

				//begin to swap [.lock] to replace old file 
				//overwriting existing will require check if password is correct for existing Vault
				string oldToDelete = $"{path}.deleted";
				if (File.Exists(path))
				{
					if (!IsValidVaultPassword(path, password, VaultToEncryptAndSave))
						throw new ArgumentException("invalid password!");

					File.Move(path, oldToDelete);
					if (!File.Exists(oldToDelete))
					{
						File.Delete(tempLockPath);
						throw new IOException("failed to save file, cannot move old file.");
					}
				}

				File.Move(tempLockPath, path); //completing save
				if (!File.Exists(path))
				{
					File.Move(oldToDelete, path); //reset
					throw new IOException("failed to save file");
				}

				File.Delete(oldToDelete);
				if (File.Exists(oldToDelete))
					throw new IOException("failed to save file, cannot remove old file.");
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}

		public void LoadVault(string path, string password)
		{
			try
			{
				string vaultData = null;
				using (var buffer = new MemoryStream())
				{
					using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
						Core.decrypt(input, buffer, password);

					buffer.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(buffer))
						vaultData = reader.ReadToEnd();
				}
				if (vaultData == null || vaultData == "")
					throw new InvalidDataException("Invalid file, no data.");

				var tempVault = JsonSerializer.Deserialize<Vault<Credential>>(vaultData);
				VaultPath = path;
				Vault.Dispose();
				Vault = tempVault;

				LoadedVaultPath = path;
			}
			catch (CryptographicException ex)
			{
				Logger.Log(ex);
				throw new IOException("Fail to decrypt the vault. Invalid file or password");
			}
			catch (JsonException ex)
			{
				Logger.Log(ex);
				throw new IOException("Invalid file data");
			}
			catch (InvalidDataException)
			{
				throw new IOException("Invalid file, no data");
			}
			catch (IOException ex)
			{
				Logger.Log(ex);
				throw new IOException("Invalid file");
			}
		}

		public void ChangeVaultPasswordThenEncryptAndSave(string pathToOldVault, string oldPassword, string newPassword, Vault<Credential> oldLoadedVault)
		{
			//only can be done via overwritten existing vault with correct existing old password
			if (!File.Exists(pathToOldVault))
				throw new IOException("Vault does not exists ");
			if (!IsValidVaultPassword(pathToOldVault, oldPassword, oldLoadedVault))
				throw new ArgumentException("invalid old password!");

			string tempLockPath = $"{pathToOldVault}.lock";
			try
			{
				using (var input = new MemoryStream(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(oldLoadedVault)), false))
				{
					using (var output = new BufferedStream(new FileStream(tempLockPath, FileMode.Create)))
					{
						Core.encrypt(input, output, newPassword);
					}
				}

				if (!File.Exists(tempLockPath))
					throw new IOException("failed to save file");

				//begin to swap [.lock] to replace old file 
				string oldToDelete = $"{pathToOldVault}.deleted";
				
					File.Move(pathToOldVault, oldToDelete);
					if (!File.Exists(oldToDelete))
					{
						File.Delete(tempLockPath);
						throw new IOException("failed to save file, cannot move old file.");
					}

				File.Move(tempLockPath, pathToOldVault); //completing save
				if (!File.Exists(pathToOldVault))
				{
					File.Move(oldToDelete, pathToOldVault); //reset
					throw new IOException("failed to save file");
				}

				File.Delete(oldToDelete);
				if (File.Exists(oldToDelete))
					throw new IOException("failed to save file, cannot remove old file.");
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				throw;
			}
		}

		public void Dispose()
		{
			Vault.Dispose();
			Clipboard.Dispose();
		}
	}
}
