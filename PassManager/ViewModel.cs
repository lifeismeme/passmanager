using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PassManager
{
    class ViewModel
    {
		public Vault<Credential> Credentials { get; set; }
		 
 

		public ViewModel()
		{
			Credentials = new Vault<Credential>();
		}

		public void initSample()
		{
			Credentials.Add(new Credential
			{
				Id = 1,
				Username = "hello world",
				Password = new char[] { 'B', 'B', 'C' },
				Description = "good by.\nh",
				Title = "o0o"
			});
			Credentials.Add(new Credential
			{
				Id = 2,
				Password = new char[] { 'A', 'b', 'a' },
				Title = "pp"
			});
		}

		internal void AddCredential(Credential credential)
		{
			Credentials.Add(credential);
		}
		internal int CredentialCount()
		{
			return Credentials.Count;
		}

		void createNewValut(string path, byte[] bytes)
		{
			//if (File.Exists(path))
			//	throw new Exception();
			//File.WriteAllBytes(path, bytes);
		}
	}
}
