using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PassManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
			Debug.AutoFlush = true;
			Debug.Indent();
		}


		private void BtnOpen_Click(object sender, RoutedEventArgs e)
		{
			string path = @"C:\Users\mlcmi\Desktop\passmanager\output\A.txt";
			string dest = @"C:\Users\mlcmi\Desktop\passmanager\output\encrypted.txt";
			if (!File.Exists(path))
				return;

			//if (!Directory.Exists(txtDestPath.Text))
			//	return;

			try
			{
				using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
				{
					using (var output = new BufferedStream(new FileStream(dest, FileMode.Create)))
					{
						Core.encrypt(input, output, txtKey.Text );
					}
				}
				Debug.WriteLine("Done ecnrpyting");
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("UnauthorizedAccessException fail to save file.");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}

			try
			{
				new VaultWindow().Show();
			}
			catch (Exception)
			{

				throw;
			}
		}

		private void TxtDestPath_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
		{
			string path = @"C:\Users\mlcmi\Desktop\passmanager\output\encrypted.txt";
			string dest = @"C:\Users\mlcmi\Desktop\passmanager\output\decrypted.txt";
			if (!File.Exists(path))
				return;

			try
			{
				using (var input = new BufferedStream(new FileStream(path, FileMode.Open)))
				{
					using (var output = new BufferedStream(new FileStream(dest, FileMode.Create)))
					{
						Core.decrypt(input, output, txtKey.Text);
					}
				}
				Debug.WriteLine("Done derpyting");
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("UnauthorizedAccessException fail to save file.");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}
	}
}
