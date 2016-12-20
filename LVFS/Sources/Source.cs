using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.Sources
{
	/// <summary>
	/// Represents a source (individual layer) in the LVFS.
	/// </summary>
	abstract class Source
	{
		/// <summary>
		/// Gets whether or not this source controls the specified file
		/// </summary>
		/// <param name="path">The file path being queried</param>
		/// <returns>whether or not this source controls the specified file</returns>
		public abstract bool ControlsFile(string path);

		/// <summary>
		/// Lists the files and subdirectories contained within a given directory
		/// </summary>
		/// <param name="path">The directory to list the contents of</param>
		/// <param name="inputFiles">The combined contents of all lower priority layers of the filesystem. This will be null if no lower level contains the requested directory.</param>
		/// <returns>A list of files in the given directory when this source and all lower priority sources have been considered.</returns>
		public abstract IList<FileInformation> ListFiles(string path, IList<FileInformation> inputFiles);
	}
}
