using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

using LVFS.Filesystem;

namespace LVFS.External
{
	/// <summary>
	/// A class allowing LVFS implementations to set up and mount virtual drives
	/// </summary>
	public class LVFSInterface
	{
		private LVFSEngine mEngine;
		private Selector mSelector;

		/// <summary>
		/// Constructs an instance of the class
		/// </summary>
		/// <param name="volumeLabel">The volume label to assign to the filesystem</param>
		/// <param name="filesystemName">The filesystem name to expose to the operating system.</param>
		public LVFSInterface(string volumeLabel = "LVFS", string filesystemName = "LVFS")
		{
			mSelector = new Selector();
			mEngine = new LVFSEngine(mSelector, volumeLabel, filesystemName);
		}

		/// <summary>
		/// Add a source to the LVFS as the highest priority
		/// </summary>
		/// <param name="source">The source to add</param>
		public void AddSource(Source source)
		{
			mSelector.AddSource(source);
		}

		/// <summary>
		/// Mount the LVFS to the specified mount point with the default options
		/// </summary>
		/// <param name="mountPoint">The mount point to use.</param>
		public void Mount(string mountPoint)
		{
#if DEBUG
			mEngine.Mount(mountPoint, new DokanNet.Logging.ConsoleLogger());
#else
			mEngine.Mount(mountPoint);
#endif
		}
	}
}
