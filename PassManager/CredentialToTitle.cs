using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassManager
{
	class CredentialToTitle : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			IEnumerable list = (IEnumerable)value;
			var titles = new List<string>();
			var e = list.GetEnumerator();
			while (e.MoveNext())
			{
				titles.Add(((Vault)e.Current).Title);
			}

			return titles;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
