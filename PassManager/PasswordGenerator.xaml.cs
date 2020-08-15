using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
				btnGenerate.IsEnabled = false;
			}

			else if (passLen > 39)
			{
				lblErrorMessage.Visibility = Visibility.Visible;
				lblErrorMessage.Content = "number is too big";
				btnGenerate.IsEnabled = false;
			}
			else
			{
				lblErrorMessage.Visibility = Visibility.Hidden;
				btnGenerate.IsEnabled = true;
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

		private void TxtPassword_TextChanged(object sender, TextChangedEventArgs e)
		{
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

		private void TxtLen_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
				txtLen.Text = "";
		}
	}
}
