using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassManager;
using System.Linq;

namespace PassManagerUnitTests
{
	/// <summary>
	/// Summary description for CredentialTests
	/// </summary>
	[TestClass]
	public class CredentialTests
	{
		public CredentialTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void PropertyChanged_IdChange_NotifiedAndEqual()
		{
			var credential = new Vault()
			{
				Id = 1,
			};
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender, e) => {
				actualSender = sender;
				actualEvent = e;
			};
			int newId = 442;

			credential.Id = newId;

			Assert.AreEqual(credential, actualSender);
			Assert.IsTrue(actualEvent != null);
			Assert.IsTrue(credential.Id == newId);
		}
		[TestMethod]
		public void PropertyChanged_TitleChange_NotifiedAndEqual()
		{
			var credential = new Vault() {
				Title="title",
			};
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender,e) => {
				actualSender = sender;
				actualEvent = e;
			};
			string newTitle = "title changed";

			credential.Title = newTitle;

			Assert.AreEqual(credential, actualSender);
			Assert.IsTrue(actualEvent != null);
			Assert.IsTrue(credential.Title == newTitle);
		}

		[TestMethod]
		public void PropertyChanged_UsernameChange_NotifiedAndEqual()
		{
			var credential = new Vault()
			{
				Username = "username",
			};
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender, e) => {
				actualSender = sender;
				actualEvent = e;
			};
			string newUsername = "username changed";

			credential.Username = newUsername;

			Assert.IsTrue(actualEvent != null);
			Assert.AreEqual(credential, actualSender);
			Assert.IsTrue(credential.Username == newUsername);
		}

		[TestMethod]
		public void PropertyChanged_PasswordChange_NotifiedAndEqual()
		{
			var credential = new Vault()
			{
				Password = new char[] { 'p', '@', 's', 's', 'W', '0', 'r', 'd' },
			};
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender, e) => {
				actualSender = sender;
				actualEvent = e;
			};
			char[] oldPass = credential.Password;
			char[] newPass = new char[] { 'n'};

			credential.Password = newPass;

			Assert.IsTrue(actualEvent != null);
			Assert.AreEqual(credential, actualSender);
			Assert.IsTrue(Enumerable.SequenceEqual(credential.Password, newPass));
			Assert.IsTrue(credential.Password != oldPass);
		}

		[TestMethod]
		public void PropertyChanged_DescriptionChange_NotifiedAndEqual()
		{
			var credential = new Vault()
			{
				Description = "description..."
			};
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender, e) => {
				actualSender = sender;
				actualEvent = e;
			};
			string newDescription="description change.";

			credential.Description = newDescription;

			Assert.IsTrue(actualEvent != null);
			Assert.AreEqual(credential, actualSender); 
			Assert.IsTrue(credential.Description == newDescription);
		}

		[TestMethod]
		public void PropertyChanged_CreationChange_NotifiedAndEqual()
		{
			var credential = new Vault()
			{
				Creation = new DateTime(1992,1,31)
			};
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender, e) => {
				actualSender = sender;
				actualEvent = e;
			};
			DateTime newCreationDate = DateTime.Now;

			credential.Creation = newCreationDate;
			
			Assert.IsTrue(actualEvent != null);
			Assert.AreEqual(credential, actualSender);
			Assert.IsTrue(credential.Creation == newCreationDate);
			Assert.IsTrue(DateTime.Compare(credential.Creation, newCreationDate) == 0);
		}
		[TestMethod]
		public void Creation_DefaultDateTime_IsNow()
		{
			DateTime now = DateTime.Now;
			var credential = new Vault();
			DateTime secLater = DateTime.Now;
			
			
			Assert.IsTrue(DateTime.Compare(credential.Creation, now) >= 0);
			Assert.IsTrue(DateTime.Compare(credential.Creation, secLater) <= 0);
		}

		[TestMethod]
		public void LastModified_Changes_LastModifiedIsUpdated()
		{
			DateTime oldLastModified = new DateTime(1999, 1, 31);
			var credential = new Vault() { LastModified = oldLastModified };
			object actualSender = null;
			EventArgs actualEvent = null;
			credential.PropertyChanged += (sender, e) => {
				actualSender = sender;
				actualEvent = e;
			};


			Assert.IsTrue(DateTime.Compare(credential.LastModified, oldLastModified) == 0);

			credential.Id = 994;
			Assert.IsTrue(DateTime.Compare(credential.LastModified, DateTime.Now) <= 0);
			Assert.IsTrue(DateTime.Compare(credential.LastModified, oldLastModified) > 0);
			oldLastModified = credential.LastModified;

			credential.Title = "a";
			Assert.IsTrue(DateTime.Compare(credential.LastModified, DateTime.Now) <= 0);
			Assert.IsTrue(DateTime.Compare(credential.LastModified, oldLastModified) >= 0);
			oldLastModified = credential.LastModified;

			credential.Username = "usenr";
			Assert.IsTrue(DateTime.Compare(credential.LastModified, DateTime.Now) <= 0);
			Assert.IsTrue(DateTime.Compare(credential.LastModified, oldLastModified) >= 0);
			oldLastModified = credential.LastModified;

			credential.Password = new char[] { 's' };
			Assert.IsTrue(DateTime.Compare(credential.LastModified, DateTime.Now) <= 0);
			Assert.IsTrue(DateTime.Compare(credential.LastModified, oldLastModified) >= 0);
			oldLastModified = credential.LastModified;

			credential.Description = "description..";
			Assert.IsTrue(DateTime.Compare(credential.LastModified, DateTime.Now) <= 0);
			Assert.IsTrue(DateTime.Compare(credential.LastModified, oldLastModified) >= 0);
			oldLastModified = credential.LastModified;


			Assert.IsTrue(actualEvent != null);
			Assert.AreEqual(credential, actualSender);
		}

		[TestMethod]
		public void Password_Changes_OldIsDestroyed()
		{
			char[] original = new char[] { '1', 'A', 'b', '%' };
			char[] copy = new char[] { '1', 'A', 'b', '%' };
			var credential = new Vault() {
				Password = original
			};

			Assert.IsTrue(Enumerable.SequenceEqual(credential.Password, copy));
			credential.Password = new char[] { 'x' };

			Assert.IsFalse(Enumerable.SequenceEqual(original, copy));
			Assert.IsTrue(Enumerable.SequenceEqual(original, new char[original.Length]));

		}
	}
}
