using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PassManager
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class InputDialog : Window, IDisposable
	{
		public bool IsOk { get; set; } = false;

		private void init()
		{
			InitializeComponent();
			txtInput.Focus();
		}
		public InputDialog()
		{
			init();
		}

		public InputDialog(Window Base)
		{
			InitializeComponent();
			txtInput.Focus();
			Left = Base.Left + Base.Width / 2 - Width / 2;
			Top = Base.Top + Base.Height / 2 - Height / 2;
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e)
		{
			IsOk = true;
			this.Close();
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		public void Dispose()
		{
			this.Close();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				if (txtInput.Text == "")
				{
					IsOk = false;
					this.Close();
				}
				txtInput.Text = "";
			}
			else if (e.Key == Key.Enter)
			{
				IsOk = true;
				this.Close();
			}
		}
	}
}
