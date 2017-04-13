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
			Console.WriteLine(Path.GetDirectoryName(args[0]));
		}
	}
}
