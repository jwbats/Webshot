using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webshot
{
	class Program
	{


		private static void Output(List<string> success, List<string> timeout, List<string> exception)
		{
			Console.WriteLine($"SUCCESS: {success.Count}");
			foreach (string url in success)
			{
				Console.WriteLine(url);
			}

			Console.WriteLine();

			Console.WriteLine($"TIMEOUT: {timeout.Count}");
			foreach (string url in timeout)
			{
				Console.WriteLine(url);
			}

			Console.WriteLine();

			Console.WriteLine($"EXCEPTION: {exception.Count}");
			foreach (string url in exception)
			{
				Console.WriteLine(url);
			}

			Console.WriteLine();
		}


		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Arg0: [textfile with single url per line]");
				return;
			}

			string fileName   = args[0];
			int browserWidth  = Convert.ToInt32(ConfigurationManager.AppSettings["browserWidth"]);
			int browserHeight = Convert.ToInt32(ConfigurationManager.AppSettings["browserHeight"]);
			int timeOut       = Convert.ToInt32(ConfigurationManager.AppSettings["timeOut"]);

			List<string> success, timeout, exception;

			Webshotter webshotter = new Webshotter(fileName, browserWidth, browserHeight, timeOut);
			webshotter.CreateWebshots(out success, out timeout, out exception);

			Output(success, timeout, exception);

			Console.ReadKey();
		}

	}
}
