using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PassManager
{
	public class ClipboardCleaner : IDisposable
	{
		private CancellationTokenSource source = new CancellationTokenSource();
		private TaskCompletionSource<ClipboardCleaner> autoClear = new TaskCompletionSource<ClipboardCleaner>();
		public int autoClearAfterMilliSecond { get; set; } = 15000;
		private Thread Timmer { get; set; } = new Thread(delegate () { });

		public void Set(string text)
		{
			Timmer.Interrupt();
			Timmer = new Thread(() =>
			{
				try
				{
					string copiedValue = text;
					try
					{
						System.Windows.Clipboard.SetData(DataFormats.Text, text);
						Debug.WriteLine("copied");
						Thread.Sleep(autoClearAfterMilliSecond);
					}
					finally
					{
						if (copiedValue == System.Windows.Clipboard.GetText())
							System.Windows.Clipboard.Clear();

						Debug.WriteLine("clear");
					}
				}
				catch (ThreadInterruptedException)
				{
					//exit
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			});
			Timmer.SetApartmentState(ApartmentState.STA);
			Timmer.IsBackground = true;
			Timmer.Start();
		}


		public void Dispose()
		{
			Timmer.Interrupt();
		}

		~ClipboardCleaner()
		{
			Timmer.Interrupt();
		}
	}
}
