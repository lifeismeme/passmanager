using System;
using System.Windows;
using System.Windows.Controls;

namespace PassManager
{
	/// <summary>
	/// Interaction logic for PasswordGenerator.xaml
	/// </summary>
	public partial class PasswordGenerator : Window, IDisposable
	{
		public PasswordGenerator(Window Base)
		{
			InitializeComponent();

			Left = Base.Left + Base.Width / 2 - Width / 2;
			Top = Base.Top + Base.Height / 2 - Height / 2;

			lblErrorMessage.Visibility = Visibility.Hidden;
		}

		private bool IsValidPassLen(out int passLen)
		{
			passLen = 0;
			if (!int.TryParse(txtLen.Text.Trim(), out passLen))
				return false;

			if (passLen < 4)
				return false;

			if (passLen > 39)
				return false;

			return true;
		}

		private void ShowErrorMessage(int passLen)
		{
			if (lblErrorMessage == null)
				return;

			if (passLen < 4)
			{
				lblErrorMessage.Visibility = Visibility.Visible;
				lblErrorMessage.Content = "must be 4 or above";
			}

			else if (passLen > 39)
			{
				lblErrorMessage.Visibility = Visibility.Visible;
				lblErrorMessage.Content = "number is too big";

			}
			else
			{
				lblErrorMessage.Visibility = Visibility.Hidden;
			}
		}
		private void TxtLen_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValidPassLen(out int passLen);
			ShowErrorMessage(passLen);
		}
		private void BtnGenerate_Click(object sender, RoutedEventArgs e)
		{
			bool isValidNum = IsValidPassLen(out int passLen);
			ShowErrorMessage(passLen);

			if (!isValidNum)
				return;

			txtPassword.Text = new String(Core.generateRandomPassword(passLen));
		}

		private void BtnCopy_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Clipboard.SetData(DataFormats.Text, txtPassword.Text.Trim());
		}

		private void BtnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		public void Dispose()
		{
			txtPassword.Text = "";
			Close();
		}
	}
}
