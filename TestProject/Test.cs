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
			FileSecurity fSecurity = File.GetAccessControl(args[0], AccessControlSections.Owner);

			var owner = fSecurity.GetOwner(typeof(SecurityIdentifier));
			Console.WriteLine(owner);
			if (owner == null)
				Console.WriteLine("Null owner!");
			else
				Console.WriteLine(owner.Value);
		}
	}
}
