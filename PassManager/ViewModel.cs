using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassManager
{
	class ViewModel
	{
		public Vault<Credential> Vault { get; set; }



		public ViewModel()
		{
			Vault = new Vault<Credential>();
		}

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

		public bool loadVault(string path, string password)
		{
			try
			{
				using (var buffer = new MemoryStream())
				{
					using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
					{
						Core.decrypt(input, buffer, password);
					}

					buffer.Seek(0, SeekOrigin.Begin);
					string vaultData = "";
					using (var reader = new StreamReader(buffer))
					{
						vaultData = reader.ReadToEnd();
					}

					Vault = JsonSerializer.Deserialize<Vault<Credential>>(vaultData);
				}
				return true;
			}
			catch ( CryptographicException )
			{
				//invalid pass
			}
			catch (JsonException)
			{
				//invalid file
			}

			return false;
		}
		void createNewValut(string path, byte[] bytes)
		{

			if (File.Exists(path))
				throw new Exception();
			File.WriteAllBytes(path, bytes);
		}
	}
}
