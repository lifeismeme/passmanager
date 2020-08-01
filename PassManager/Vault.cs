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



		public DateTime Creation { get  ; set; }
		public DateTime LastModified { get ; set ; }
		public string Sha256sum { get; set  ; }



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
		
			return (!en.MoveNext() && i == count);
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
