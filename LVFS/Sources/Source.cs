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
		private Source mPredecessor;

		public bool IsFirst { get { return mPredecessor == null; } }

		/// <summary>
		/// Construct a source with the specified predecessor.
		/// </summary>
		/// <param name="predecessor">The predecessor of the current source. It should be null if there is no predecessor.</param>
		protected Source(Source predecessor)
		{
			mPredecessor = predecessor;
		}

		/// <summary>
		/// As with ListFiles, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path of the directory to list the contents of</param>
		/// <returns>A list of file information structs.</returns>
		protected IList<FileInformation> ListPredecessorFiles(string path)
		{
			return mPredecessor.ListFiles(path);
		}

		/// <summary>
		/// As with GetFileInformation, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to get file information for</param>
		/// <returns>A file information struct if the path corresponds to a file, or null if not.</returns>
		protected FileInformation? GetPredecessorFileInformation(string path)
		{
			return mPredecessor.GetFileInformation(path);
		}
		
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
		/// <returns>A list of files in the given directory when this source and all lower priority sources have been considered.</returns>
		public abstract IList<FileInformation> ListFiles(string path);

		/// <summary>
		/// Gets file information for the file with the specified path (if it exists), or null otherwise.
		/// </summary>
		/// <param name="path">The file path to get the information for</param>
		/// <returns>A nullable FileInformation struct for the requested file.</returns>
		public abstract FileInformation? GetFileInformation(string path);

		/// <summary>
		/// If the source is writable, returns a tuple of the free, total and available space for the storage medium holding the current source. Otherwise, returns null.
		/// </summary>
		/// <returns>A tuple of the free, total and available bytes of space for the source's storage medium</returns>
		public abstract Tuple<long, long, long> GetSpaceInformation();
	}
}
