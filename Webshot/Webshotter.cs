using System;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webshot
{
	class Webshotter
	{

		// ---------- Constructor & Members

		private string fileName;
		private int browserWidth;
		private int browserHeight;
		private string[] urls;

		public Webshotter(string fileName, int browserWidth, int browserHeight)
		{
			this.fileName      = fileName;
			this.browserWidth  = browserWidth;
			this.browserHeight = browserHeight;
			this.urls          = CreateUrls();
		}


		// ---------- Private Methods

		private string[] CreateUrls()
		{
			return File.ReadAllLines(this.fileName)
				.Where(x => !String.IsNullOrWhiteSpace(x))
				.Select(x => x.Trim())
				.Where(x => x.StartsWith("http"))
				.ToArray();
		}


		private Bitmap GenerateScreenshot(string url, int width, int height)
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


		private string CreateFileName(string url, int index)
		{
			string strippedUrl = url.Replace("http://", "").Replace("https://", "");

			return $"{index:D3} - {strippedUrl}.bmp";
		}


		private void CreateWebshot(string url, int index)
		{
			Bitmap bitmap   = GenerateScreenshot(url, this.browserWidth, this.browserHeight);
			string fileName = CreateFileName(url, index);

			bitmap.Save(fileName);
		}


		// ---------- Public Methods

		public void CreateWebshots()
		{
			for (int i = 0; i < urls.Length; i++)
			{
				string url = urls[i];
				Console.WriteLine($"Webshotting {url}...");
				CreateWebshot(url, i);
			}
		}
	}
}
