using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace PassManager
{
	public class Vault<T> : ObservableCollection<T>
	{

		private DateTime creation;
		private DateTime lastModified;
		private string sha256sum;


		public DateTime Creation { get => creation; set => creation = value; }
		public DateTime LastModified { get => lastModified; set => lastModified = value; }
		public string Sha256sum { get => sha256sum; set => sha256sum = value; }



		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;

			var vault = (Vault<T>)obj;
			IOrderedEnumerable<T> sortedVault = from c in vault
												orderby c
												select c;
			int count = this.Count;
			var en = sortedVault.GetEnumerator();
			int i = 0;
			for (; en.MoveNext() && i < count; ++i)
			{
				if (!en.Current.Equals(this[i]))
					return false;
			}

			if (!en.MoveNext() && i == count)
				return true;
			else
				return false;
		}

		public override int GetHashCode()
		{
			var hashCode = 1636230790;
			hashCode *= -1521134295 + creation.GetHashCode();
			hashCode *= -1521134295 + lastModified.GetHashCode();
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(sha256sum);
			hashCode *= -1521134295 + Creation.GetHashCode();
			hashCode *= -1521134295 + LastModified.GetHashCode();
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Sha256sum);
			return hashCode;
		}
	}
}
