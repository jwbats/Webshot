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

			Webshotter webshotter = new Webshotter(fileName, browserWidth, browserHeight);
			webshotter.CreateWebshots();

			Console.ReadKey();
		}

	}
}
