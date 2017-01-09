using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVFS
{
	class Test
	{
		static string DirectoryPath = "G:\\Chris\\Documents";

		private static string ConvertPath(string path)
		{
			path = path.Substring(1);
			return Path.Combine(DirectoryPath, path);
		}

		static void Main(string[] args)
		{

			Console.WriteLine(ConvertPath("\\TestFolder\\"));
			;
		}
	}
}
