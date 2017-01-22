using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

using DokanNet;

namespace LVFS.External
{
	/// <summary>
	/// Represents a source (individual layer) in the LVFS.
	/// </summary>
	public abstract class Source
	{
		internal Source mPredecessor;

		/// <summary>
		/// True if this source has no predecessor. False otherwise.
		/// </summary>
		public bool IsFirst { get { return mPredecessor == null; } }

		/// <summary>
		/// True if the source is writable
		/// </summary>
		public bool IsWritable { get { return this is WritableSource; } }

		/// <summary>
		/// True if a predecessor source exists and is writable
		/// </summary>
		public bool HasWritablePredecessor { get { return !IsFirst && mPredecessor.IsWritable; } }

		/// <summary>
		/// Construct a source.
		/// </summary>
		protected Source()
		{
			mPredecessor = null;
		}

		/// <summary>
		/// Sets the predecessor
		/// </summary>
		/// <param name="predecessor">The new predecessor of this source</param>
		internal void SetPredecessor(Source predecessor)
		{
			mPredecessor = predecessor;
		}

		/// <summary>
		/// As with <see cref="HasFile(string)"/>, but for the predecessor source
		/// </summary>
		/// <param name="path">The path being queried</param>
		/// <returns>Whether the predecessor or any of its predecessors has the file.</returns>
		protected bool PredecessorHasFile(string path)
		{
			return mPredecessor?.HasFile(path) ?? false;
		}

		/// <summary>
		/// As with ListFiles, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path of the directory to list the contents of</param>
		/// <returns>A list of file information structs.</returns>
		protected IList<FileInformation> ListPredecessorFiles(string path)
		{
			if (IsFirst)
				return new List<FileInformation>();
			else
				return mPredecessor.ListFiles(path);
		}

		/// <summary>
		/// As with GetFileInformation, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to get file information for</param>
		/// <returns>A file information struct if the path corresponds to a file, or null if not.</returns>
		protected FileInformation? GetPredecessorFileInformation(string path)
		{
			return mPredecessor?.GetFileInformation(path) ?? null;
		}

		/// <summary>
		/// As with GetFileSystemSecurity, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file security information is to be returned for</param>
		/// <param name="sections">The access sections to return</param>
		/// <returns>The requested sections of security information for the requested file. Null if it does not exist.</returns>
		/// <exception cref="UnauthorizedAccessException">Thrown if the OS denies access to the data requested.</exception>
		protected FileSystemSecurity GetPredecessorFileSystemSecurity(string path, AccessControlSections sections)
		{
			return mPredecessor?.GetFileSystemSecurity(path, sections) ?? null;
		}

		/// <summary>
		/// As with CreateFileHandle, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="access">The type of access required</param>
		/// <param name="share">The kind of access other filestreams can have</param>
		/// <param name="mode">The mode to open the file in</param>
		/// <param name="options">Advanced options for creating a FileStream</param>
		/// <param name="attributes">The attributes of the file</param>
		/// <param name="info">A LVFSInfo containing the context for the file handle and information on the file.</param>
		/// <returns>An NtStatus explaining the success level of the operation. If mode is OpenOrCreate and Create, and the operation is successful opening an existing file, DokanResult.AlreadyExists must be returned.</returns>
		protected NtStatus PredecessorCreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info)
		{
			if (mPredecessor != null)
				return mPredecessor.CreateFileHandle(path, access, share, mode, options, attributes, info);
			else
			{
				switch (mode)
				{
					case FileMode.Create:
					case FileMode.CreateNew:
					case FileMode.OpenOrCreate:
						return DokanResult.AccessDenied;
					default:
						return DokanResult.PathNotFound;
				}
			}
		}

		/// <summary>
		/// As with CleanupFileHandle, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="info">The information for the context</param>
		/// <returns>True if the operation is successful</returns>
		protected bool PredecessorCleanupFileHandle(string path, LVFSContextInfo info)
		{
			return mPredecessor?.CleanupFileHandle(path, info) ?? true;
		}

		/// <summary>
		/// As with CloseFileHandle, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="info">The information for the context</param>
		/// <returns>True if the operation is successful</returns>
		protected bool PredecessorCloseFileHandle(string path, LVFSContextInfo info)
		{
			return mPredecessor?.CloseFileHandle(path, info) ?? true;
		}

		/// <summary>
		/// As with ReadFile, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="buffer">The buffer to fill with the file contents</param>
		/// <param name="bytesRead">The actual number of bytes read from the file. This may be less than the length of the buffer if not enough data is available.</param>
		/// <param name="offset">The byte at which to start reading.</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>A bool indicating whether the operation was successful</returns>
		protected bool PredecessorReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			if (IsFirst)
			{
				bytesRead = 0;
				return false;
			}
			else
				return mPredecessor.ReadFile(path, buffer, out bytesRead, offset, info);
		}

		/// <summary>
		/// As with TryLockFileRegion, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="startOffset">The offset at which the region to lock starts</param>
		/// <param name="length">The length of the region to lock</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>True if the operation was successful, false if access was denied</returns>
		protected bool PredecessorTryLockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			return mPredecessor?.TryLockFileRegion(path, startOffset, length, info) ?? false;
		}

		/// <summary>
		/// As with TryUnlockFileRegion, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="startOffset">The offset at which the region to unlock starts</param>
		/// <param name="length">The length of the region to unlock</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>True if the operation was successful, false if access was denied</returns>
		protected bool PredecessorTryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			return mPredecessor?.TryUnlockFileRegion(path, startOffset, length, info) ?? false;
		}

		/// <summary>
		/// Gets whether or not this source controls the specified file
		/// </summary>
		/// <param name="path">The file path being queried</param>
		/// <returns>Whether or not this source controls the specified file</returns>
		public abstract bool ControlsFile(string path);

		/// <summary>
		/// Gets whether or not this source or a predecessor controls the specified file.
		/// </summary>
		/// <param name="path">The file path being queried</param>
		/// <returns>Whether or not this source or a predecessor controls the specified file.</returns>
		public abstract bool HasFile(string path);

		/// <summary>
		/// Lists the files and subdirectories contained within a given directory, including those of the predecessor source.
		/// </summary>
		/// <param name="path">The directory to list the contents of</param>
		/// <returns>A list of files in the given directory when this source and all lower priority sources have been considered.</returns>
		public abstract IList<FileInformation> ListFiles(string path);

		/// <summary>
		/// Gets file information for the file with the specified path (if it exists in this source or a predecessor), or null otherwise.
		/// </summary>
		/// <param name="path">The file path to get the information for</param>
		/// <returns>A nullable FileInformation struct for the requested file.</returns>
		public abstract FileInformation? GetFileInformation(string path);

		/// <summary>
		/// If possible, returns a tuple of the free, total and available space for the storage medium holding the current source. Otherwise, returns null.
		/// </summary>
		/// <returns>A tuple of the free, total and available bytes of space for the source's storage medium</returns>
		public abstract Tuple<long, long, long> GetSpaceInformation();

		/// <summary>
		/// Gets a FileSystemSecurity object representing security information for the requested path, filtered to only include the specified sections. Returns null if the file cannot be found in this or a predecessor source, and throws an UnauthorisedAccessException if the OS denies access to the data requested.
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
		/// Called when a file handle is requested. If this is an inappropriate action for this source, the request is passed on to the predecessor.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="access">The type of access required</param>
		/// <param name="share">The kind of access other filestreams can have</param>
		/// <param name="mode">The mode to open the file in</param>
		/// <param name="options">Advanced options for creating a FileStream</param>
		/// <param name="attributes">The attributes of the file</param>
		/// <param name="info">A LVFSInfo containing the context for the file handle and information on the file.</param>
		/// <returns>An NtStatus explaining the success level of the operation. If mode is OpenOrCreate and Create, and the operation is successful opening an existing file, DokanResult.AlreadyExists must be returned.</returns>
		public abstract NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info);

		/// <summary>
		/// Called when the last handle for a file has been closed, but not necessarily released. This is an appropriate place to delete the file if DeleteOnClose is set. This must recursively call the predecessor version if the predecessor source may have been invloved in any operations with this context.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="info">The information for the context</param>
		/// <returns>True if the operation is successful</returns>
		public abstract bool CleanupFileHandle(string path, LVFSContextInfo info);

		/// <summary>
		/// Called once the last handle for a file has been closed and released. <paramref name="info"/>.Context will be lost after this method returns, so must be ready for this. This must recursively call the predecessor version if the predecessor source may have been invloved in any operations with this context.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="info">The information for the context</param>
		/// <returns>True if the operation was successful</returns>
		public abstract bool CloseFileHandle(string path, LVFSContextInfo info);

		/// <summary>
		/// Gets the contents of the specified file starting at the specified offset and attempts to fill the buffer. If this is an inappropriate action for this source, the request is passed on to the predecessor.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="buffer">The buffer to fill with the file contents</param>
		/// <param name="bytesRead">The actual number of bytes read from the file. This may be less than the length of the buffer if not enough data is available.</param>
		/// <param name="offset">The byte at which to start reading.</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>A bool indicating whether the operation was successful</returns>
		public abstract bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info);

		/// <summary>
		/// Locks a region of the specified file from the specified offset with the specified length if possible. The region is either entirely locked or entirely unlocked at the end of the operation. If this is an inappropriate action for this source, the request is passed on to the predecessor.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="startOffset">The offset at which the region to lock starts</param>
		/// <param name="length">The length of the region to lock</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>True if the operation was successful, false if access was denied</returns>
		public abstract bool TryLockFileRegion(string path, long startOffset, long length, LVFSContextInfo info);

		/// <summary>
		/// Unlocks a region of the specified file from the specified offset with the specified length if possible. If this is an inappropriate action for this source, the request is passed on to the predecessor.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="startOffset">The offset at which the region to unlock starts</param>
		/// <param name="length">The length of the region to unlock</param>
		/// <param name="info">Holds the context for the operation and relevant information</param>
		/// <returns>True if the operation was successful, false if access was denied</returns>
		public abstract bool TryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info);
	}
}
