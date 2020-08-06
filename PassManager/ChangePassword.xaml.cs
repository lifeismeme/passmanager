using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PassManager
{
	/// <summary>
	/// Interaction logic for ChangePassword.xaml
	/// </summary>
	public partial class ChangePassword : Window, System.IDisposable
	{
		public bool IsNewPasswordsMatch { get; private set; } = false;
		public bool IsOk { get; private set; } = false;
		public ChangePassword(Window Base)
		{
			InitializeComponent();

			Left = Base.Left + Base.Width / 2 - Width / 2;
			Top = Base.Top + Base.Height / 2 - Height / 2;
		}

		private void LblPasswordStrength_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtPasswordNew.Focus();
		}

		private void LblErrorMessage_MouseDown(object sender, MouseButtonEventArgs e)
		{
			txtPasswordNew2.Focus();
		}
		private void TxtPasswordOld_PasswordChanged(object sender, RoutedEventArgs e)
		{
			EnableBtnOkOnMatch();
		}
		private void TxtPasswordNew_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (lblPasswordStrength == null)
				return;
			string password = txtPasswordNew.Password;
			EnableBtnOkOnMatch();

			if (password == "")
			{
				lblPasswordStrength.Visibility = Visibility.Hidden;
				return;
			}

			lblPasswordStrength.Visibility = Visibility.Visible;
			double bits = Core.calcPasswordBits(password);
			lblPasswordStrength.Text = string.Format("{0:F1} bits", bits);
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

		private void TxtPasswordNew2_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (txtPasswordNew == null || txtPasswordNew2 == null)
				return;

			EnableBtnOkOnMatch();
			if (txtPasswordNew.Password == txtPasswordNew2.Password)
				lblErrorMessage.Visibility = Visibility.Hidden;
			else
				lblErrorMessage.Visibility = Visibility.Visible;
		}
		private void EnableBtnOkOnMatch()
		{
			if (txtPasswordOld.Password == "" || txtPasswordNew.Password == "" || txtPasswordNew2.Password == "")
			{
				IsNewPasswordsMatch = false;
				btnOk.IsEnabled = false;
			}
			else
			{
				IsNewPasswordsMatch = txtPasswordNew.Password == txtPasswordNew2.Password;
				btnOk.IsEnabled = IsNewPasswordsMatch;
			}
		}
		private void BtnOk_Click(object sender, RoutedEventArgs e)
		{
			IsOk = true;
			Close();
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			IsNewPasswordsMatch = false;
			IsOk = false;
			Close();
		}

		public void Dispose()
		{
			IsNewPasswordsMatch = false;
			Close();
		}

		private void TxtPasswordOld_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				txtPasswordOld.Password = "";
		}

		private void TxtPasswordNew_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Escape)
				txtPasswordNew.Password = "";
		}

		private void TxtPasswordNew2_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				txtPasswordNew2.Password = "";
		}
	}
}
