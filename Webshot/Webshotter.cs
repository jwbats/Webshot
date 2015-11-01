using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webshot
{
	class Webshotter
	{

		#region ================================================== Constructor & Members ==================================================

		private string fileName;
		private int browserWidth;
		private int browserHeight;
		private int timeOut;
		private string[] urls;

		private ConcurrentQueue<string> cqSuccess;
		private ConcurrentQueue<string> cqTimeout;
		private ConcurrentQueue<string> cqException;

		public Webshotter(string fileName, int browserWidth, int browserHeight, int timeOut)
		{
			this.fileName      = fileName;
			this.browserWidth  = browserWidth;
			this.browserHeight = browserHeight;
			this.timeOut       = timeOut;
			this.urls          = CreateUrls();
		}

		#endregion ================================================== Constructor & Members ==================================================




		#region ================================================== Private Methods ==================================================

		private void InitConcurrentQueues()
		{
			cqSuccess   = new ConcurrentQueue<string>();
			cqTimeout   = new ConcurrentQueue<string>();
			cqException = new ConcurrentQueue<string>();
		}


		private string[] CreateUrls()
		{
			return File.ReadAllLines(this.fileName)
				.Where(x => !String.IsNullOrWhiteSpace(x))
				.Select(x => x.Trim())
				.Where(x => x.StartsWith("http"))
				.ToArray();
		}


		private Bitmap _GenerateScreenshot(string url, int width, int height)
		{
			// Load the webpage into a WebBrowser control
			WebBrowser webBrowser = new WebBrowser();
			webBrowser.ScrollBarsEnabled = false;
			webBrowser.ScriptErrorsSuppressed = true;
			webBrowser.Navigate(url);

			// wait for iiiiiiiiiiiit...
			while (webBrowser.ReadyState != WebBrowserReadyState.Complete) { Application.DoEvents(); }


			// Set the size of the WebBrowser control
			webBrowser.Width  = width;
			webBrowser.Height = height;

			// Get a Bitmap representation of the webpage as it's rendered in the WebBrowser control
			Bitmap bitmap = new Bitmap(webBrowser.Width, webBrowser.Height);
			webBrowser.DrawToBitmap(bitmap, new Rectangle(0, 0, webBrowser.Width, webBrowser.Height));
			webBrowser.Dispose();

			return bitmap;
		}


		private Bitmap GenerateScreenshot(string url, int width, int height)
		{
			try
			{
				return _GenerateScreenshot(url, width, height);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.ToString());
				return null;
			}
		}


		private string CreateFileName(string url, int index)
		{
			string strippedUrl = url.Replace("http://", "").Replace("https://", "");

			return $"{index:D3} - {strippedUrl}.bmp";
		}


		private void CreateWebshot(string url, int index)
		{
			Bitmap bitmap   = GenerateScreenshot(url, this.browserWidth, this.browserHeight);
			string fileName = CreateFileName(url, index);

			bitmap?.Save(fileName);
		}


		/// <summary>
		/// Thread entry point.
		/// </summary>
		private void CreateWebshot_TryCatch(string url, int index)
		{
			try
			{
				CreateWebshot(url, index);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.ToString());
				this.cqException.Enqueue(url);
			}
		}


		private void CreateWebshot_Task(string url, int index)
		{
			ThreadStart threadStart = new ThreadStart(
				() => { CreateWebshot(url, index); }	
			);

			Thread thread = new Thread(threadStart);

			thread.SetApartmentState(ApartmentState.STA); // required by WebBrowser component

			thread.Start();

			bool success = thread.Join(this.timeOut);

			if (success)
			{
				this.cqSuccess.Enqueue(url);
			}
			else
			{
				this.cqTimeout.Enqueue(url);
				thread.Abort();
			}
		}

		#endregion ================================================== Private Methods ==================================================




		#region ================================================== Public Methods ==================================================

		public void CreateWebshots(out List<string> success, out List<string> timeout, out List<string> exception)
		{
			InitConcurrentQueues();

			int nrUrls = this.urls.Length;

			Console.WriteLine($"Webshotting {nrUrls} urls.");

			for (int i = 0; i < nrUrls; i++)
			{
				string url = urls[i];
				Console.WriteLine($"Webshotting {url}...");
				CreateWebshot_Task(url, i);
			}

			Console.WriteLine($"Webshotted {nrUrls} urls.");

			success   = this.cqSuccess.ToList();
			timeout   = this.cqTimeout.ToList();
			exception = this.cqException.ToList();
		}

		#endregion ================================================== Public Methods ==================================================

	}
}
