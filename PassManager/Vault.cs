using System;
using System.Collections.Generic;


namespace PassManager
{
	public class Vault<T> : IDisposable
	{
		public DateTime Creation { get; set; }
		public DateTime LastModified { get; set; }
		public string Sha256sum { get; set; }
		public int Count { get { return Items.Count; } }
		//serializing Dictionary<?,?> to json can be either string or Object with public properties only, other types in an Dictionary/Hastable are not supported to be serialize to json.
		public Dictionary<string, T> Items { get; set; } //<id, Item> 
		private int lastAddedItemId = -1;

		public delegate void ItemsChangeHandler(Vault<T> sender, T item);
		public event ItemsChangeHandler OnItemAdded;
		public event ItemsChangeHandler OnItemRemoved;

		public delegate void DisposedHandler(Vault<T> sender);
		public event DisposedHandler OnDisposed;
		public void Add(T item)
		{
			int id = Items.Count + 1;
			while (Items.ContainsKey(id.ToString()))
				id += 1;

			Items.Add(id.ToString(), item);
			lastAddedItemId = id;
			OnItemAdded?.Invoke(this, item);
		}

		public bool Remove(string id)
		{
			bool isRemoved = false;
			if (Items.TryGetValue(id, out T item))
			{
				isRemoved = Items.Remove(id);
				OnItemRemoved?.Invoke(this, item);
			}

			return isRemoved;
		}

		public void pushAllItemsToHandlers()
		{
			if (OnItemAdded == null)
				return;
			foreach (var item in Items)
				OnItemAdded.Invoke(this, item.Value);
		}

		public int LastAddedItemId()
		{
			return lastAddedItemId;
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

			var theirs = (Vault<T>)obj;

			if (Sha256sum != theirs.Sha256sum) return false;
			if (!DateTime.Equals(Creation, theirs.Creation)) return false;
			if (!DateTime.Equals(LastModified, theirs.LastModified)) return false;

			if (Count != theirs.Count)
				return false;

			foreach (var entry in Items)
			{
				if (!theirs.Items.TryGetValue(entry.Key, out T theirItem))
					return false;
				if (!theirItem.Equals(entry.Value))
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			var hashCode = 1636230790;
			hashCode *= -1521134295 + Creation.GetHashCode();
			hashCode *= -1521134295 + LastModified.GetHashCode();
			hashCode *= -1521134295 + EqualityComparer<string>.Default.GetHashCode(Sha256sum);
			return hashCode;
		}

		public void Dispose()
		{
			Items?.Clear();
			OnDisposed?.Invoke(this);
		}
	}
}
