using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

using LVFS.Filesystem;
using LVFS.Sources;
using LVFS.Sources.DirectoryMirror;

namespace LVFS
{
	class Program
	{
		static void Main(string[] args)
		{
			var sourceCount = args.Length - 1;
			Console.WriteLine("There are " + sourceCount + " sources listed.");

			Console.Write("Attempting to mirror ");

			for (var i = 1; i < sourceCount; i++)
				Console.Write("'" + args[i] + "', ");

			Console.WriteLine("and '" + args[sourceCount] + "' to mount point '" + args[0] + "'");

			Selector selector = new Selector();

			Source sourceA = null;
			Source sourceB;

			for (var i = 1; i <= sourceCount; i++)
			{
				sourceB = new ReadOnlyDirectoryMirror(args[i], sourceA);
				sourceA = sourceB;
				selector.AddSource(sourceA);
			}

			LVFSEngine filesystem = new LVFSEngine(selector, "VolumeLabel", "FSName");

			try
			{
				filesystem.Mount(args[0]);
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
