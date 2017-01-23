using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

using LVFS.External;

using LayeredDirectoryMirror.DirectoryMirror;

namespace LVFS
{
	/// <summary>
	/// The main class used to start the layered directory mirror
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The entry point for the application
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			var sourceCount = args.Length - 1;
			Console.WriteLine("There are " + sourceCount + " sources listed.");

			Console.Write("Attempting to mirror ");

			for (var i = 1; i < sourceCount; i++)
				Console.Write("'" + args[i] + "', ");

			Console.WriteLine("and '" + args[sourceCount] + "' to mount point '" + args[0] + "'");

			LVFSInterface lvfs = new LVFSInterface("Mirror");

			for (var i = 1; i < args.Length; i++)
				lvfs.AddSource(new ReadOnlyDirectoryMirror(args[i]));

			try
			{
				lvfs.Mount(args[0]);

				Console.WriteLine("Success!");
			}
			catch (DokanException de)
			{
				Console.Error.WriteLine("Failure :'(");
				Console.Error.WriteLine(de.Message);
				Console.Error.WriteLine(de.StackTrace);
			}
		}
	}
}
