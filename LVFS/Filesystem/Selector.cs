using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

using DokanNet;

using LVFS.Sources;

namespace LVFS.Filesystem
{
	/// <summary>
	/// A class responsible for fetching the Source(s) responsible for files and files available from the collection of Sources
	/// </summary>
	class Selector
	{
		private IList<Source> mSources;
		private Source Last { get { return mSources.Last<Source>(); } }

		/// <summary>
		/// Constructs a new Selector with a list of Sources
		/// </summary>
		/// <param name="sources">The list of Sources to use</param>
		public Selector(IList<Source> sources)
		{
			mSources = new List<Source>(sources);
		}

		/// <summary>
		/// Constructs a new Selector with no Sources
		/// </summary>
		public Selector()
		{
			mSources = new List<Source>();
		}

		/// <summary>
		/// Adds an additional Source to the LVFS as the highest priority Source
		/// </summary>
		/// <param name="source">The Source to add</param>
		public void AddSource(Source source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			mSources.Add(source);
		}

		public bool HasWritableSource { get { return Last.GetType().IsSubclassOf(typeof(WritableSource)); } }

		/// <summary>
		/// Gets the source responsible for a file/directory
		/// </summary>
		/// <param name="fileName">The path to the file/directory</param>
		/// <returns>The Source responsible for the file/directory</returns>
		public Source SourceOf(string fileName)
		{
			for (int i = mSources.Count - 1; i >= 0; i--)
			{
				if (mSources[i].ControlsFile(fileName))
					return mSources[i];
			}
			return null;
		}

		/// <summary>
		/// Lists the files in the directory represented by the input.
		/// </summary>
		/// <param name="path">The directory to list the contents of</param>
		/// <returns>A list of file information about the directory contents, or null if the directory does not exist within the VFS</returns>
		public IList<FileInformation> ListFiles(string path)
		{
			return Last.ListFiles(path);
		}

		/// <summary>
		/// Gets file information for the file with the specified path (if it exists), or null otherwise.
		/// </summary>
		/// <param name="path">The file path to get the information for</param>
		/// <returns>A nullable FileInformation struct for the requested file.</returns>
		public FileInformation? GetFileInformation(string path)
		{
			return Last.GetFileInformation(path);
		}

		/// <summary>
		/// If possible, returns a tuple of the free, total and available space for the storage medium holding the output source. Otherwise, returns null.
		/// </summary>
		/// <returns>A tuple of the free, total and available bytes of space for the output source's storage medium</returns>
		public Tuple<long, long, long> GetSpaceInformation()
		{
			return Last.GetSpaceInformation();
		}

		/// <summary>
		/// Gets a FileSystemSecurity object representing security information for the requested path, filtered to only include the specified sections. Returns null if the file cannot be found, and throws an UnauthorisedAccessException if the OS denies access to the data requested.
		/// </summary>
		/// <param name="path">The path to get security data for</param>
		/// <param name="sections">The sections of security data to get</param>
		/// <returns>The security data</returns>
		/// <exception cref="UnauthorizedAccessException">Thrown if the OS denies access to the data requested.</exception>
		public FileSystemSecurity GetFileSystemSecurity(string path, AccessControlSections sections)
		{
			return Last.GetFileSystemSecurity(path, sections);
		}

		/// <summary>
		/// Called when a filesystem using this selector is mounted
		/// </summary>
		/// <returns>A boolean representing whether all sources were capable of being mounted.</returns>
		public bool OnMount()
		{
			bool success = true;
			for (int i = mSources.Count - 1; i >= 0; i--)
				success &= mSources[i].OnMount();
			return success;
		}

		/// <summary>
		/// Called when a filesystem using this selector is unmounted
		/// </summary>
		/// <returns>A boolean representing whether all sources were capable of being unmounted.</returns>
		public bool OnUnmount()
		{
			bool success = true;
			for (int i = mSources.Count - 1; i >= 0; i--)
				success &= mSources[i].OnUnmount();
			return success;
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
		/// <param name="info">An LVFSinfo containing the context for the file handle and information on the file</param>
		/// <returns>An NtStatus explaining the success level of the operation. If mode is OpenOrCreate and Create, and the operation is successful opening an existing file, DokanResult.AlreadyExists is returned.</returns>
		public NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, System.IO.FileAttributes attributes, LVFSContextInfo info)
		{
			return Last.CreateFileHandle(path, access, share, mode, options, attributes, info);
		}

		/// <summary>
		/// To be called when a file handle and context have been closed, but not necessarily released. If <paramref name="info"/>.DeleteOnClose is true, then this is where the file is actually deleted.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="info">The information for the context of this operation</param>
		/// <returns>True if the operation was successful</returns>
		public bool CleanupFileHandle(string path, LVFSContextInfo info)
		{
			return Last.CleanupFileHandle(path, info);
		}

		/// <summary>
		/// To be called once all file handles for this context have been closed and released.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="info">The information for the context of this operation</param>
		/// <returns>True if the operation was successful</returns>
		public bool CloseFileHandle(string path, LVFSContextInfo info)
		{
			return Last.CloseFileHandle(path, info);
		}

		/// <summary>
		/// Gets the contents of the specified file starting at the specified offset and attempts to fill the buffer.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="buffer">The buffer to fill with the file contents</param>
		/// <param name="bytesRead">The actual number of bytes read from the file. This may be less than the length of the buffer if not enough data is available.</param>
		/// <param name="offset">The byte at which to start reading.</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>A bool indicating whether the operation was successful</returns>
		public bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			return Last.ReadFile(path, buffer, out bytesRead, offset, info);
		}

		/// <summary>
		/// Locks a region of the specified file from the specified offset with the specified length if possible. The region is either entirely locked or entirely unlocked at the end of the operation.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="startOffset">The offset at which the region to lock starts</param>
		/// <param name="length">The length of the region to lock</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>True if the operation was successful, false if access was denied</returns>
		public bool TryLockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			return Last.TryLockFileRegion(path, startOffset, length, info);
		}

		/// <summary>
		/// Unlocks a region of the specified file from the specified offset with the specified length if possible.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="startOffset">The offset at which the region to unlock starts</param>
		/// <param name="length">The length of the region to unlock</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>True if the operation was successful, false if access was denied</returns>
		public bool TryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			return Last.TryUnlockFileRegion(path, startOffset, length, info);
		}
	}
}
