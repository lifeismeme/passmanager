﻿using System;
using System.Collections.Generic;
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
	/// Interaction logic for PasswordDialog.xaml
	/// </summary>
	public partial class PasswordDialog : Window, IDisposable
	{
		public bool IsOk { get; set; } = false;
		public PasswordDialog()
		{
			InitializeComponent();
			txtPassword.Focus();
		}
		public PasswordDialog(Window Base)
		{
			InitializeComponent();
			txtPassword.Focus();

			Left = Base.Left + Base.Width / 2 - Width / 2;
			Top = Base.Top + Base.Height / 2 - Height / 2;
		}

		public void Dispose()
		{
			txtPassword.Password = "";
			this.Close();
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e)
		{
			IsOk = true;
			this.Close();
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			txtPassword.Password = "";
			this.Close();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				if (txtPassword.Password == "")
				{
					IsOk = false;
					this.Close();
				}
				txtPassword.Password = "";
			}
			else if (e.Key == Key.Enter)
			{
				IsOk = true;
				this.Close();
			}
		}
	}
}
