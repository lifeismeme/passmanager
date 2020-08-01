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
		public Vault<Vault> Vault { get; set; }
		 
 

		public ViewModel()
		{
			Vault = new Vault<Vault>();
		}

		public void initSample()
		{
			Vault.Add(new Vault
			{
				Id = 1,
				Username = "hello world",
				Password = new char[] { 'B', 'B', 'C' },
				Description = "good by.\nh",
				Title = "o0o"
			});
			Vault.Add(new Vault
			{
				Id = 2,
				Password = new char[] { 'A', 'b', 'a' },
				Title = "pp"
			});
		}

		
		void createNewValut(string path, byte[] bytes)
		{
			//if (File.Exists(path))
			//	throw new Exception();
			//File.WriteAllBytes(path, bytes);
		}
	}
}
