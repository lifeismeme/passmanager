using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PassManager
{
	public class Credential : INotifyPropertyChanged, IComparable<Credential>
	{
		private int id;
		private string title;
		private string username;
		private char[] password = new char[0];
		private string description;
		private DateTime creation;
		private DateTime lastModified;

		public Credential()
		{
			Creation = DateTime.Now;
		}
		public static Credential CreateNew()
		{
			DateTime now = DateTime.Now;
			return new Credential()
			{
				Id = 1,
				Title = "",
				Username = "",
				Password = new char[0],
				Description = "",
				Creation = now,
				LastModified = now
			};
		}
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			lastModified = DateTime.Now;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public int CompareTo(Credential other)
		{
			var otherId = other.Id;
			if (id == otherId)
				return 0;
			else if (id > otherId)
				return 1;
			else
				return -1;
		}

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;
			var c = obj as Credential;
			return (c.Id == Id
				&& c.Title == Title
				&& c.Username == username
				&& Enumerable.SequenceEqual(c.Password, Password)
				&& c.Description == Description
				&& DateTime.Equals(c.Creation, Creation)
				&& DateTime.Equals(c.LastModified, LastModified));
		}

		public override int GetHashCode()
		{
			var hashCode = 165930658;
			hashCode *= -1521134295 + Id.GetHashCode();
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Username);
			hashCode *= -1521134295 + EqualityComparer<char[]>.Default.GetHashCode(Password);
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
			hashCode *= -1521134295 + Creation.GetHashCode();
			hashCode *= -1521134295 + LastModified.GetHashCode();
			return hashCode;
		}

		public int Id { get => id; set { id = Math.Abs(value); NotifyPropertyChanged(); } }
		public string Title
		{
			get => title; set
			{
				title = value;
				NotifyPropertyChanged();
			}
		}
		public string Username { get => username; set { username = value; NotifyPropertyChanged(); } }
		public char[] Password
		{
			get => password; set
			{
				//manually clear and destroy char[] before set its reference to other char[]
				if (password != null)
				{
					for (int i = 0; i < password.Length; ++i)
					{
						password[i] = '\0';
					}
				}
				password = value;
				NotifyPropertyChanged();
			}
		}
		public string Description { get => description; set { description = value; NotifyPropertyChanged(); } }
		public DateTime Creation { get => creation; set { creation = value; NotifyPropertyChanged(); } }
		public DateTime LastModified { get => lastModified; set => lastModified = value; }

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
