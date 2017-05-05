using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LVFS
{
	class Test
	{
		static void Main(string[] args)
		{
			Console.WriteLine(File.Exists(args[0]));
			using (var stream = new FileStream(args[0], FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Delete))
			{
				Console.WriteLine(File.Exists(args[0]));
				File.Delete(args[0]);
				Console.WriteLine(File.Exists(args[0]));
			}
			Console.WriteLine(File.Exists(args[0]));
		}
	}
}
