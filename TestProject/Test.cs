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
			using (var fs = new FileStream(args[0], FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
			{
				Console.WriteLine("Created?");
			}
		}
	}
}
