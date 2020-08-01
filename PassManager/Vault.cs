using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections;

namespace PassManager
{
	public class Vault<T> : IEnumerable<T>
	{

		public DateTime Creation { get; set; }
		public DateTime LastModified { get; set; }
		public string Sha256sum { get; set; }
		public List<T> Credentials { get; set; }
		public int Count { get { return Credentials.Count; } }
		public Vault()
		{
			DateTime NOW = DateTime.Now;
			Creation = NOW;
			LastModified = NOW;
			Sha256sum = null;
			Credentials = new List<T>();
		}
		public static Vault<T> CreateNew()
		{
			DateTime NOW = DateTime.Now;
			return new Vault<T>() {
				Creation = NOW,
				LastModified = NOW,
				Sha256sum = null,
				Credentials = new List<T>()
			};
		}

		public void Add(T item)
		{
			Credentials.Add(item);
		}
		
		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;

			var vault = (Vault<T>)obj;
			IOrderedEnumerable<T> sortedVault = from c in vault
												orderby c
												select c;
			var Credentials = this.Credentials;
			int count = Credentials.Count;
			var en = sortedVault.GetEnumerator();
			int i = 0;
			for (; en.MoveNext() && i < count; ++i)
			{
				if (!en.Current.Equals(Credentials[i]))
					return false;
			}

			return (!en.MoveNext() && i == count);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Credentials.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Credentials.GetEnumerator();
		}

		public override int GetHashCode()
		{
			var hashCode = 1636230790;
			hashCode *= -1521134295 + Creation.GetHashCode();
			hashCode *= -1521134295 + LastModified.GetHashCode();
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Sha256sum);
			return hashCode;
		}


	}
}
