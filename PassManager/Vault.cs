using System;
using System.Collections.Generic;


namespace PassManager
{
	public class Vault<T>
	{
		public DateTime Creation { get; set; }
		public DateTime LastModified { get; set; }
		public string Sha256sum { get; set; }
		public int Count { get { return Items.Count; } }
		//serializing Dictionary<?,?> to json can be either string or Object with public properties only, other types in an Dictionary/Hastable are not supported to be serialize to json.
		public Dictionary<string, T> Items { get; set; } //<id, Item> 
		public int LastAddedItemId { get; private set; }

		public delegate void ItemAddedHandler (Vault<T> sender, T addedItem);
		public event ItemAddedHandler OnItemAdded;
		public void Add(T item)
		{
			int id = Items.Count + 1;
			while (Items.ContainsKey(id.ToString()))
				id += 1;

			Items.Add(id.ToString(), item);
			LastAddedItemId = id;
			OnItemAdded?.Invoke(this, item);
		}

		public Vault()
		{
			DateTime NOW = DateTime.Now;
			Creation = NOW;
			LastModified = NOW;
			Sha256sum = null;
			Items = new Dictionary<string, T>();
		}

		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		public static Vault<T> CreateNew()
		{
			return new Vault<T>();
		}


		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;

			var theirItems = ((Vault<T>)obj).Items;
			var smallerSet = theirItems.Count < Items.Count ? theirItems : Items;
			var largerSet = theirItems.Count > Items.Count ? theirItems : Items;
			foreach (var i in smallerSet)
			{
				T item;
				if (largerSet.TryGetValue(i.Key, out item))
					if (item.Equals(i.Value))
						return true;
			}

			return false;
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
