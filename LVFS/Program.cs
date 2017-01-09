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
			Console.WriteLine("Attempting to mirror '" + args[1] + "' and '" + args[2] + "' to mount point '" + args[0] + "'");

			Source source = new ReadOnlyDirectoryMirror(args[1], null);
			Source sourceB = new ReadOnlyDirectoryMirror(args[2], source);
			Selector selector = new Selector();
			selector.AddSource(source);
			selector.AddSource(sourceB);
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
