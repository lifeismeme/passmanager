using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PassManager
{
	/// <summary>
	/// Interaction logic for VaultWindow.xaml
	/// </summary>
	/// 
	public partial class VaultWindow : Window
	{
		private Credential selectedCredential = null;

		private ViewModel ViewModel { get; set; } = new ViewModel();

		private _SelectedItemState SelectedItemState { get; set; } = new _SelectedItemState();
		private class _SelectedItemState
		{
			public bool IsEditing { private get; set; } = false;
			public bool HasEdited { private get; set; } = false;

			public bool HasUnsavedChanges()
			{
				return IsEditing && HasEdited;
			}
			public void ResetState()
			{
				IsEditing = false;
				HasEdited = false;
			}
		}
		public VaultWindow()
		{
			InitializeComponent();
			init();
		}

		private void init()
		{

			const string propertyOfCredentialToDisplay = "Title";
			lstTitle.DisplayMemberPath = propertyOfCredentialToDisplay;

			//register eventhandler
			ViewModel.Credentials.CollectionChanged += UpdateLstTitle;

			//add sample
			ViewModel.initSample();

			gridCredential.IsEnabled = false;
		}

		private void UpdateLstTitle(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e == null)
				return;

			foreach (Credential c in e.NewItems)
				lstTitle.Items.Add(c);
		}



		private void TxtSearchTitle_TextChanged(object sender, TextChangedEventArgs e)
		{
			string titleSearch = ((TextBox)sender).Text.Trim().ToLower();
			lstTitle.Items.Clear();
			if (titleSearch == string.Empty)
			{
				foreach (Credential c in ViewModel.Credentials)
				{
					lstTitle.Items.Add(c);
				}
			}
			else
			{
				lstTitle.Items.Clear();
				foreach (Credential c in ViewModel.Credentials)
				{
					string title = c.Title.ToLower();
					if (title.Contains(titleSearch) || titleSearch.Contains(title))
						lstTitle.Items.Add(c);
				}
			}
		}

		private void TxtSearchTitle_GotFocus(object sender, RoutedEventArgs e)
		{
			((TextBox)sender).SelectAll();
		}

		private void MenuNew_Click(object sender, RoutedEventArgs e)
		{

		}
		private void menuOpen_Click(object sender, RoutedEventArgs e)
		{

		}

		private void MenuSave_Click(object sender, RoutedEventArgs e)
		{
			 
		}

		private void MenuExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void LstTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lstTitle.SelectedIndex == -1)
				return;

			if (SelectedItemState.HasUnsavedChanges())
				flushUnsavedCredentialChanges();

			setEditable(false);

			selectedCredential = (Credential)lstTitle.SelectedItem;

			Credential c = selectedCredential;
			txtTitle.Text = c.Title;
			txtUsername.Text = c.Username;
			txtPassword.Text = new String(c.Password);
			txtDescription.Text = c.Description;
			SelectedItemState.ResetState();

			log($"selected Credential.Id: {c.Id}");
		}

		private void flushUnsavedCredentialChanges()
		{
			MessageBoxResult msgBoxResult = MessageBox.Show("Set changes to currently editing credential?", "Unfinish edit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.ServiceNotification);
			switch (msgBoxResult)
			{
				case MessageBoxResult.Yes:
					setChangesToSelectedCredential();
					break;
				case MessageBoxResult.No:
					break;
			}
		}




		private void setChangesToSelectedCredential()
		{
			Credential c = selectedCredential;
			c.Title = txtTitle.Text.Trim();
			c.Username = txtUsername.Text.Trim();
			c.Password = txtPassword.Text.Trim().ToCharArray();
			c.Description = txtDescription.Text.Trim();
			log("set Changes to selected and modified Credential");
		}

		private void BtnAddNewCredential_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.HasUnsavedChanges())
			{
				flushUnsavedCredentialChanges();
				log($"has unsaved changes: {SelectedItemState.HasUnsavedChanges()}");

				SelectedItemState.ResetState();
			}
			setEditable(false);
			int id = ViewModel.Credentials.Count;
			ViewModel.Credentials.Add(new Credential()
			{
				Id = id,
				Title = $"-New Title id ({id})-",
				Username = "",
				Password = new char[0],
				Description = ""
			});

		}

		private void ChkEdit_Click(object sender, RoutedEventArgs e)
		{
			bool? isChecked = ((CheckBox)sender).IsChecked;
			if (isChecked == true)
			{
				setEditable(true);
			}
			else
			{
				if (SelectedItemState.HasUnsavedChanges())
				{
					flushUnsavedCredentialChanges();
					log($"has unsaved changes: {SelectedItemState.HasUnsavedChanges()}");

					SelectedItemState.ResetState();
				}
				setEditable(false);
			}
		}

		private void setEditable(bool isEnable)
		{
			SelectedItemState.IsEditing = isEnable;
			gridCredential.IsEnabled = isEnable;
			chkEdit.IsChecked = isEnable;
			if (isEnable)
				chkEdit.Content = "Editing";
			else
				chkEdit.Content = "Edit";
		}

		private void TxtTitle_TextChanged(object sender, TextChangedEventArgs e)
		{
			setCredentialHasChanged();
		}

		private void TxtUsername_TextChanged(object sender, TextChangedEventArgs e)
		{
			setCredentialHasChanged();
		}

		private void TxtPassword_TextChanged(object sender, TextChangedEventArgs e)
		{
			setCredentialHasChanged();
		}

		private void TxtDescription_TextChanged(object sender, TextChangedEventArgs e)
		{
			setCredentialHasChanged();
		}

		private void setCredentialHasChanged()
		{
			if (chkEdit?.IsEnabled == true)
				SelectedItemState.HasEdited = true;
		}


		private static void log(object x)
		{
			Console.WriteLine(DateTime.UtcNow.Ticks + " " + x);
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			int selectedIndex = lstTitle.SelectedIndex;
			if (selectedIndex == -1)
				return;

			MessageBoxResult msgBoxResult = MessageBox.Show("Save unsaved changes to currently selected credential?", "Unsave Changes", MessageBoxButton.YesNo);
			if (msgBoxResult != MessageBoxResult.Yes)
				return;

			log($"count before delete: { lstTitle.Items.Count}");
			lstTitle.Items.RemoveAt(selectedIndex);
			log($"count after delete: { lstTitle.Items.Count}");
		}

		private Clipboard clipboard = new Clipboard();
		private class Clipboard
		{
			private CancellationTokenSource source = new CancellationTokenSource();
			private string copiedValue = null;
			private TaskCompletionSource<Clipboard> autoClear = new TaskCompletionSource<Clipboard>();
			public int autoClearAfterMilliSecond { get; set; } = 15000;
			private Thread task { get; set; } = new Thread(delegate () { });
			
			public void Set(string text)
			{
				copiedValue = text;
				System.Windows.Clipboard.SetData(DataFormats.Text, text);
				task.Interrupt();
				task = new Thread(delegate ()
				{
					try
					{
						Thread.Sleep(autoClearAfterMilliSecond);
						Clear();
					}
					catch (ThreadInterruptedException)
					{
						//exit
					}
				});
				task.SetApartmentState(ApartmentState.STA);
				task.IsBackground = true;
				task.Start();
			}

			public void Clear()
			{
				if (copiedValue == System.Windows.Clipboard.GetText())
					System.Windows.Clipboard.Clear();
				copiedValue = null;
			}
		}
		private void BtnCopyUsername_Click(object sender, RoutedEventArgs e)
		{
			clipboard.Set(txtUsername.Text);
		}

		private void BtnCopyPassword_Click(object sender, RoutedEventArgs e)
		{
			clipboard.Set(txtPassword.Text);
			var x = btnCopyPassword.Content;
			btnCopyPassword.Content = "-";
			btnCopyPassword.Content = x;
		}

		
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			//eixt
		}
	}
}
