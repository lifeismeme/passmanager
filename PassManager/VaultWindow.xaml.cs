﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace PassManager
{
	/// <summary>
	/// Interaction logic for VaultWindow.xaml
	/// </summary>
	/// 
	public partial class VaultWindow : Window
	{
		private readonly string defaultTitle = "Password Manager =]";
		private ViewModel ViewModel { get; set; } = new ViewModel();

		private SelectedItem<Credential> SelectedItemState { get; set; } = new SelectedItem<Credential>();

		private class SelectedItem<T>
		{
			private T item;
			public T Item { get => item; set { item = value; ResetState(); } }
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
			init();
		}

		public VaultWindow(Window Base)
		{
			init();
			Left = Base.Left + Base.Width / 2 - Width / 2;
			Top = Base.Top + Base.Height / 2 - Height / 2;
		}

		private void init()
		{
			InitializeComponent();

			const string propertyOfCredentialToDisplay = "Title";
			lstTitle.DisplayMemberPath = propertyOfCredentialToDisplay;
			registerVaultItemsChangeHandler();

			//add sample
			ViewModel.initSample();

			setEditable(false);

			if (ViewModel.LoadedVaultPath == "")
			{
				menuSave.IsEnabled = false;
				menuChangePassword.IsEnabled = false;
			}

			//placeholders
			registerTxtPlaceholderShowHide();
		}

		private void registerTxtPlaceholderShowHide()
		{

			txtSearchTitle.TextChanged += (sender, e) =>
			{
				TextBox txtbox = (TextBox)sender;
				if (txtbox.Text == "")
					lblSearchTitle.Visibility = Visibility.Visible;
				else
					lblSearchTitle.Visibility = Visibility.Hidden;
			};
			txtTitle.TextChanged += (sender, e) =>
			{
				TextBox txtbox = (TextBox)sender;
				if (txtbox.Text == "")
					lblTitle.Visibility = Visibility.Visible;
				else
					lblTitle.Visibility = Visibility.Hidden;
			};
			txtUsername.TextChanged += (sender, e) =>
			{
				TextBox txtbox = (TextBox)sender;
				if (txtbox.Text == "")
					lblUsername.Visibility = Visibility.Visible;
				else
					lblUsername.Visibility = Visibility.Hidden;
			};
			txtPassword.TextChanged += (sender, e) =>
			{
				TextBox txtbox = (TextBox)sender;
				if (txtbox.Text == "")
					lblPassword.Visibility = Visibility.Visible;
				else
					lblPassword.Visibility = Visibility.Hidden;
			};
			txtDescription.TextChanged += (sender, e) =>
			{
				TextBox txtbox = (TextBox)sender;
				if (txtbox.Text == "")
					lblDescription.Visibility = Visibility.Visible;
				else
					lblDescription.Visibility = Visibility.Hidden;
			};
		}

		private void registerVaultItemsChangeHandler()
		{
			var vault = ViewModel.Vault;
			if (vault == null)
				return;

			vault.OnItemAdded += (sender, itemAdded) => lstTitle.Items.Add(itemAdded);
			vault.OnItemRemoved += (sender, itemRemoved) => lstTitle.Items.Remove(itemRemoved);
			vault.OnDisposed += (sender) => lstTitle.Items.Clear();
		}

		private void TxtSearchTitle_TextChanged(object sender, TextChangedEventArgs e)
		{
			string titleSearch = ((TextBox)sender).Text.Trim().ToLower();
			lstTitle.Items.Clear();
			if (titleSearch == string.Empty)
			{
				foreach (var item in ViewModel.Vault)
				{
					var c = item.Value;
					lstTitle.Items.Add(c);
				}
			}
			else
			{
				lstTitle.Items.Clear();
				foreach (var item in ViewModel.Vault)
				{
					var c = item.Value;
					string title = c.Title.ToLower();
					if (title.Contains(titleSearch) || titleSearch.Contains(title))
						lstTitle.Items.Add(c);
				}
			}
		}

		private void TxtSearchTitle_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				if (txtSearchTitle.Text == "")
				{
					this.Focus();
				}
				txtSearchTitle.Text = "";
			}
		}

		private void TxtSearchTitle_GotFocus(object sender, RoutedEventArgs e)
		{
			((TextBox)sender).SelectAll(); //doesn't work, might be a bug.
		}

		private static readonly string dialogFilter = "Encrypted Vault (*.encryptedvault)|*.encryptedvault";
		private void MenuNew_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.HasUnsavedChanges())
				flushUnsavedCredentialChanges();
			setEditable(false);
			clearDisplayedCredential();
			try
			{
				var saveDialog = new SaveFileDialog();
				saveDialog.Filter = dialogFilter;
				saveDialog.ShowDialog();
				if (saveDialog.SafeFileName == "") return;
				if (File.Exists(saveDialog.FileName))
				{
					MessageBox.Show($"Cannot overwrite existing file: {saveDialog.SafeFileName}", "File exists", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				using (var dialog = new ChangePassword(this))
				{
					//override the default GUI
					dialog.txtPasswordOld.Password = "noNeedToFill";
					dialog.txtPasswordOld.Visibility = Visibility.Hidden;
					dialog.lblPasswordOld.Visibility = Visibility.Hidden;
					dialog.Title = "New Vault";
					dialog.lblMessage.Text = "Set a master password for the new vault";
					dialog.txtPasswordNew.Focus();
					dialog.ShowDialog();
					if (!dialog.IsOk) return;
					if (!dialog.IsNewPasswordsMatch) { Logger.Log("new passwords not match."); return; }
					if (File.Exists(saveDialog.FileName))
					{
						MessageBox.Show($"Cannot overwrite existing file: {saveDialog.SafeFileName}", "File exists", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					ViewModel.EncryptAndSaveVault(saveDialog.FileName, dialog.txtPasswordNew.Password, new Vault<Credential>());
					loadVault(saveDialog.FileName, dialog.txtPasswordNew.Password);
					Title = $"{defaultTitle} : {saveDialog.SafeFileName}";
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				MessageBox.Show($"Error!: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		private void clearDisplayedCredential()
		{
			txtTitle.Text = "";
			txtUsername.Text = "";
			txtPassword.Text = "";
			txtDescription.Text = "";
			lblDateCreated.Content = "Date Created:";
			lblDateLastModified.Content = "Last Modified:";
			lblDateCreated.ToolTip = "";
			lblDateLastModified.ToolTip = "";
			SelectedItemState.ResetState();
			SelectedItemState.Item = null;
			lstTitle.SelectedIndex = -1;
		}

		private void loadVault(string path, string password)
		{
			try
			{
				Visibility = Visibility.Hidden;

				clearDisplayedCredential();
				lstTitle.Items.Clear();

				ViewModel.LoadVault(path, password);

				registerVaultItemsChangeHandler();
				ViewModel.Vault.pushAllItemsToHandlers();
			}
			finally
			{
				Visibility = Visibility.Visible; //show refresh behaviour
				menuSave.IsEnabled = (ViewModel.LoadedVaultPath != "");
				menuChangePassword.IsEnabled = menuSave.IsEnabled;
			}
		}
		private void menuOpen_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.HasUnsavedChanges())
				flushUnsavedCredentialChanges();
			setEditable(false);
			clearDisplayedCredential();
			try
			{
				OpenFileDialog openDialog = new OpenFileDialog() { Filter = dialogFilter };
				openDialog.ShowDialog();
				if (openDialog.SafeFileName == "")
					return;
				var passwordDialog = new PasswordDialog(this);
				passwordDialog.ShowDialog();
				if (!passwordDialog.IsOk)
					return;

				loadVault(openDialog.FileName, passwordDialog.txtPassword.Password);
				Title = $"{defaultTitle} {openDialog.SafeFileName}";
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
			}
		}

		private void MenuSave_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.HasUnsavedChanges())
				flushUnsavedCredentialChanges();
			setEditable(false);
			try
			{
				using (var dialog = new PasswordDialog(this))
				{
					dialog.ShowDialog();
					if (!dialog.IsOk)
						return;

					ViewModel.EncryptAndSaveVault(ViewModel.LoadedVaultPath, dialog.txtPassword.Password, ViewModel.Vault);
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
			}
		}

		private void MenuExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuChangePassword_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.HasUnsavedChanges())
				flushUnsavedCredentialChanges();
			setEditable(false);
			displaySelectedCredential(SelectedItemState.Item);

			try
			{
				using (var dialog = new ChangePassword(this))
				{
					dialog.ShowDialog();
					if (!dialog.IsOk)
						return;
					if (!dialog.IsNewPasswordsMatch)
						return;

					ViewModel.ChangeVaultPasswordThenEncryptAndSave(ViewModel.LoadedVaultPath, dialog.txtPasswordOld.Password, dialog.txtPasswordNew.Password, ViewModel.Vault);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
			}
		}

		private void MenuGeneratePassword_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new PasswordGenerator(this))
			{
				dialog.ShowDialog();
			}
		}

		private void LstTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lstTitle.SelectedIndex == -1)
				return;

			if (SelectedItemState.HasUnsavedChanges())
				flushUnsavedCredentialChanges();

			setEditable(false);
			SelectedItemState.Item = (Credential)lstTitle.SelectedItem;
			displaySelectedCredential(SelectedItemState.Item);
			Logger.Log($"selected Credential.Id: {SelectedItemState.Item?.Id}");
		}

		private void flushUnsavedCredentialChanges()
		{
			MessageBoxResult msgBoxResult = MessageBox.Show("Set unfinish changes to currently editing credential first?", "Unfinish edit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.ServiceNotification);

			if (msgBoxResult == MessageBoxResult.Yes)
				setChangesToCredential();
		}

		private void setChangesToCredential()
		{
			//unselect first before modifying the item ListBox has ref to, 
			//changes to underlying item result different GetHashCode, 
			//where ListBox SelectItem ref no longer exists, and get stuck
			lstTitle.SelectedIndex = -1;

			Credential c = SelectedItemState.Item;
			c.Title = txtTitle.Text.Trim();
			c.Username = txtUsername.Text.Trim();
			c.Password = txtPassword.Text.Trim().ToCharArray();
			c.Description = txtDescription.Text.Trim();

			Logger.Log("set Changes to selected and modified Credential");
		}

		private void BtnAddNewCredential_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.HasUnsavedChanges())
			{
				flushUnsavedCredentialChanges();
				Logger.Log($"has unsaved changes: {SelectedItemState.HasUnsavedChanges()}");

				SelectedItemState.ResetState();
			}
			setEditable(false);

			using (var dialog = new InputDialog(this))
			{
				dialog.Title = "New Credential";
				dialog.txtMessage.Text = "New title:";
				dialog.ShowDialog();
				if (dialog.IsOk)
				{
					string title = dialog.txtInput.Text.Trim();
					if (title == "")
						title = $"-New Title id ({ViewModel.Vault.Count})-";
					var newCredential = new Credential()
					{
						Title = title,
						Username = "",
						Password = new char[0],
						Description = ""
					};
					ViewModel.Vault.Add(newCredential);
					newCredential.Id = ViewModel.Vault.LastAddedItemId();

					lstTitle.Focus();
					lstTitle.SelectedItem = newCredential;
				}
			}
		}

		private void ChkEdit_Click(object sender, RoutedEventArgs e)
		{
			var titlelist = lstTitle;
			var items = lstTitle.SelectedItems;
			var selecteditemstate = SelectedItemState;
			var vault = ViewModel.Vault;

			bool? isChecked = ((CheckBox)sender).IsChecked;
			if (isChecked == true)
			{
				setEditable(true);
			}
			else
			{
				if (SelectedItemState.HasUnsavedChanges())
				{
					Logger.Log($"chkEdit clicked to uncheck, has unsaved changes");
					flushUnsavedCredentialChanges();

					SelectedItemState.ResetState();
				}
				setEditable(false);
				lstTitle.SelectedItem = SelectedItemState.Item; // select back unselected item
				displaySelectedCredential(SelectedItemState.Item);
			}
		}

		private void setEditable(bool isEnable)
		{
			SelectedItemState.IsEditing = isEnable;
			gridCredential.IsEnabled = isEnable;
			chkEdit.IsChecked = isEnable;
			if (isEnable)
			{
				chkEdit.Content = "Editing";
				txtPasswordHider.Visibility = Visibility.Hidden;
				txtPassword.Visibility = Visibility.Visible;
				btnSave.Visibility = Visibility.Visible;
			}
			else
			{
				chkEdit.Content = "Edit";
				chkEdit.IsEnabled = lstTitle.SelectedItem != null;
				txtPasswordHider.Visibility = Visibility.Visible;
				txtPassword.Visibility = Visibility.Hidden;
				btnSave.Visibility = Visibility.Hidden;
			}
		}

		private void displaySelectedCredential(Credential c)
		{
			if (c == null)
			{
				Logger.Log("error: selected credential is Null, cannot display.");
				return;
			}

			txtTitle.Text = c.Title;
			txtUsername.Text = c.Username;
			txtPassword.Text = new String(c.Password);
			txtDescription.Text = c.Description;
			lblDateCreated.Content = $"Date Created: {c.Creation.ToString("dd-MMM-yy")}";
			lblDateLastModified.Content = $"Last Modified: {c.LastModified.ToString("dd-MMM-yy HH:mm:ss")}";
			lblDateCreated.ToolTip = c.Creation.ToString("dd-MMM-yy HH:mm:ss");
			lblDateLastModified.ToolTip = c.LastModified.ToString("dd-MMM-yy HH:mm:ss");
			SelectedItemState.ResetState();

			txtPasswordHider.Password = txtPassword.Text;
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

			if (lblPasswordStrength != null)
			{
				string password = txtPassword.Text;
				lblPasswordStrength.Visibility = Visibility.Visible;
				double bits = Core.calcPasswordBits(password);
				lblPasswordStrength.Content = string.Format("{0:F1} bits", bits);
				if (bits > 64)
					lblPasswordStrength.Foreground = Brushes.Green;
				else if (bits > 48)
					lblPasswordStrength.Foreground = Brushes.YellowGreen;
				else if (bits > 40)
					lblPasswordStrength.Foreground = Brushes.Orange;
				else if (bits > 32)
					lblPasswordStrength.Foreground = Brushes.OrangeRed;
				else if (bits > 16)
					lblPasswordStrength.Foreground = Brushes.Red;
			}
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

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedItemState.Item == null)
				return;

			MessageBoxResult msgBoxResult = MessageBox.Show("Confirm to delete this credential? This action cannot be undone", "Delete Credential", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.ServiceNotification);
			if (msgBoxResult == MessageBoxResult.Yes)
			{
				var c = SelectedItemState.Item;
				clearDisplayedCredential();
				setEditable(false);

				Logger.Log($"count before delete: { ViewModel.Vault.Count}");
				ViewModel.Vault.Remove(c.Id.ToString());
				Logger.Log($"count after delete: { ViewModel.Vault.Count}");
			}
		}

		private void BtnCopyUsername_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Clipboard.Set(txtUsername.Text);
		}

		private void BtnCopyPassword_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Clipboard.Set(txtPassword.Text);
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			ViewModel.Dispose();
		}

		private void GridCredential_LostFocus(object sender, RoutedEventArgs e)
		{
			Logger.Log("##Edit lost focus!##");
		}

		private void LblSearchTitle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtSearchTitle.Focus();
		}

		private void LblTitle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtTitle.Focus();
		}

		private void LblUsername_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtUsername.Focus();
		}

		private void LblPassword_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtPassword.Focus();
		}

		private void LblDescription_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtDescription.Focus();
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			setChangesToCredential();
			setEditable(false);
			SelectedItemState.ResetState();
			lstTitle.SelectedItem = SelectedItemState.Item;
		}
	}
}
