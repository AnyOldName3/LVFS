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
			var path = args[0];
			var isDirectory = Directory.Exists(path);
			var sections = AccessControlSections.Access;

			if (args[1] == "display")
				sections = AccessControlSections.All;

			FileSystemSecurity actualSecurity;
			if (isDirectory)
				actualSecurity = Directory.GetAccessControl(path, sections);
			else
				actualSecurity = File.GetAccessControl(path, sections);

			Console.WriteLine("Original SDDL string: " + actualSecurity.GetSecurityDescriptorSddlForm(sections));

			if (args[1] == "display")
				return;

			var desiredSddlForm = args[1];
			actualSecurity.SetSecurityDescriptorSddlForm(desiredSddlForm, sections);
			var actualSddlForm = actualSecurity.GetSecurityDescriptorSddlForm(sections);

			if (desiredSddlForm != actualSddlForm)
			{
				ConsoleColor originalColour = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;

				Console.Error.WriteLine("SDDL strings differ:");
				Console.Error.WriteLine("Desired: " + desiredSddlForm);
				Console.Error.WriteLine("Actual:  " + actualSddlForm);

				Console.Error.Flush();
				Console.ForegroundColor = originalColour;
			}

			if (isDirectory)
				Directory.SetAccessControl(path, actualSecurity as DirectorySecurity);
			else
				File.SetAccessControl(path, actualSecurity as FileSecurity);

			if (isDirectory)
				actualSecurity = Directory.GetAccessControl(path, sections);
			else
				actualSecurity = File.GetAccessControl(path, sections);

			if (actualSddlForm != actualSecurity.GetSecurityDescriptorSddlForm(sections))
			{
				ConsoleColor originalColour = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;

				Console.Error.WriteLine("SDDL strings differ:");
				Console.Error.WriteLine("Read back: " + actualSecurity.GetSecurityDescriptorSddlForm(sections));
				Console.Error.WriteLine("Intended:  " + actualSddlForm);

				Console.Error.Flush();
				Console.ForegroundColor = originalColour;
			}

			Console.WriteLine("Final SDDL string: " + actualSecurity.GetSecurityDescriptorSddlForm(sections));
		}
	}
}
