using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

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

		/// <summary>
		/// Gets a FileSystemSecurity object representing security information for the requested path, filtered to only include the specified sections. Returns null if the file cannot be found, and throws an UnauthorisedAccessException if the OS denies access to the data requested.
		/// </summary>
		/// <param name="path">The path to the file security information is to be returned for</param>
		/// <param name="sections">The access sections to return</param>
		/// <returns>The requested sections of security information for the requested file</returns>
		/// <exception cref="UnauthorizedAccessException">Thrown if the OS denies access to the data requested.</exception>
		public abstract FileSystemSecurity GetFileSystemSecurity(string path, AccessControlSections sections);

		/// <summary>
		/// Called when a filesystem including the source is mounted.
		/// </summary>
		/// <returns>A boolean representing whether the source was capable of being mounted.</returns>
		public virtual bool OnMount()
		{
			return true;
		}

		/// <summary>
		/// Called when a filesystem including the source is unmounted.
		/// </summary>
		/// <returns>A boolean representing whether the source was capable of being unmounted.</returns>
		public virtual bool OnUnmount()
		{
			return true;
		}

		/// <summary>
		/// Called when a file handle is requested
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="access">The type of access required</param>
		/// <param name="share">The kind of access other filestreams can have</param>
		/// <param name="mode">The mode to open the file in</param>
		/// <param name="options">Advanced options for creating a FileStream</param>
		/// <param name="attributes">The attributes of the file</param>
		/// <param name="info">A LVFSInfo containing the context for the file handle and information on the file.</param>
		/// <returns>An NtStatus explaining the success level of the operation. If mode is OpenOrCreate and Create, and the operation is successful opening an existing file, DokanResult.AlreadyExists must be returned.</returns>
		public abstract NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, Filesystem.LVFSInfo info);

		/// <summary>
		/// Gets the contents of the specified file starting at the specified offset and attempts to fill the buffer.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="buffer">The buffer to fill with the file contents</param>
		/// <param name="bytesRead">The actual number of bytes read from the file. This may be less than the length of the buffer if not enough data is available.</param>
		/// <param name="offset">The byte at which to start reading.</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>A bool indicating whether the operation was successful</returns>
		public abstract bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, Filesystem.LVFSInfo info);
	}
}
