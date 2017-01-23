using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

using DokanNet;

using LVFS.External;

namespace LVFS.Filesystem
{
	/// <summary>
	/// A class responsible for fetching the Source(s) responsible for files and files available from the collection of Sources
	/// </summary>
	class Selector
	{
		private IList<Source> mSources;
		private Source Last { get { return mSources.Count >= 1 ? mSources.Last<Source>() : null; } }

		/// <summary>
		/// Constructs a new Selector with a list of Sources
		/// </summary>
		/// <param name="sources">The list of Sources to use. They must already have their predecessors set up correctly.</param>
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
				throw new ArgumentNullException(nameof(source));
			source.SetPredecessor(Last);
			mSources.Add(source);
		}

		public bool IsWritable { get { return Last is WritableSource; } }

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
			var list = Last.ListFiles(path);

			if (list != null && !IsWritable)
			{
				var list2 = new List<FileInformation>();
				foreach (var info in list)
				{
					var infoCopy = info;
					infoCopy.Attributes |= System.IO.FileAttributes.ReadOnly;
					list2.Add(infoCopy);
				}
				return list2;
			}

			return list;
		}

		/// <summary>
		/// Gets file information for the file with the specified path (if it exists), or null otherwise.
		/// </summary>
		/// <param name="path">The file path to get the information for</param>
		/// <returns>A nullable FileInformation struct for the requested file.</returns>
		public FileInformation? GetFileInformation(string path)
		{
			var info = Last.GetFileInformation(path);
			if (info.HasValue && !IsWritable)
			{
				var value = info.Value;
				value.Attributes |= System.IO.FileAttributes.ReadOnly;
				return value;
			}
			else
				return info;
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

		/// <summary>
		/// Checks if a directory can be deleted, but doesn't actually do so. Must not return a success result if the directory is non-empty.
		/// </summary>
		/// <param name="path">The path to the directory to check</param>
		/// <returns><see cref="DokanResult.Success"/> if the directory can be deleted. If not, an appropriate status message to explain why.</returns>
		public NtStatus CheckDirectoryDeletable(string path)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.CheckDirectoryDeletable(path);
			else if (Last != null && Last.GetFileInformation(path) != null)
				return DokanResult.AccessDenied;
			else
				return DokanResult.PathNotFound;
		}

		/// <summary>
		/// Checks if a file can be deleted, but doesn't actually do so. For the purpose of this method, directories do not count as files.
		/// </summary>
		/// <param name="path">The path to the file to check</param>
		/// <returns><see cref="DokanResult.Success"/> if the file can be delted. If not, an appropriate error status.</returns>
		public NtStatus CheckFileDeletable(string path)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.CheckFileDeletable(path);
			else if (Last != null && Last.GetFileInformation(path) != null)
				return DokanResult.AccessDenied;
			else
				return DokanResult.FileNotFound;
		}

		/// <summary>
		/// Clears any buffers for the context, and ensures any buffered data is written to the actual file.
		/// </summary>
		/// <param name="path">The path to the file whose buffers to flush</param>
		/// <param name="info">Information concerning the context for the operation</param>
		/// <returns><see cref="DokanResult.Success"/> if all buffers were flushed, If not, an appropriate error status.</returns>
		public NtStatus FlushBuffers(string path, LVFSContextInfo info)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.FlushBuffers(path, info);
			else
				return DokanResult.NotImplemented;
		}

		/// <summary>
		/// Moves the file/directory from its current path to a new one, replacing any existing files only if replace is set.
		/// </summary>
		/// <param name="currentPath">The current path of the file/directory</param>
		/// <param name="newPath">The new path of the file/directory</param>
		/// <param name="replace">Whether to replace any existing file occupying the new path</param>
		/// <param name="info">Information concerning the context for this operation.</param>
		/// <returns><see cref="DokanResult.Success"/> if the file was moved. Otherwise, an appropriate error status.</returns>
		public NtStatus MoveFile(string currentPath, string newPath, bool replace, LVFSContextInfo info)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.MoveFile(currentPath, newPath, replace, info);
			else
				return DokanResult.AccessDenied;
		}

		/// <summary>
		/// Sets the allocated size for the file. If this is less than the current length, trucate the file. If the file does not grow to fill this space before the handle is released, it may be freed.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="allocationSize">The new size to allocate</param>
		/// <param name="info">Information concerning the context for this operation</param>
		/// <returns><see cref="DokanResult.Success"/> if the allocation size was changed or already the correct value. If not, an appropriate error status.</returns>
		public NtStatus SetAllocatedSize(string path, long allocationSize, LVFSContextInfo info)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.SetAllocatedSize(path, allocationSize, info);
			else
				return DokanResult.AccessDenied;
		}

		/// <summary>
		/// Sets the length of the file.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="length">The new length of the file</param>
		/// <param name="info">Information concerning the context of this operation</param>
		/// <returns><see cref="DokanResult.Success"/> if the requested length is now the length of the file. If not, an appropriate error status.</returns>
		public NtStatus SetLength(string path, long length, LVFSContextInfo info)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.SetLength(path, length, info);
			else
				return DokanResult.AccessDenied;
		}

		/// <summary>
		/// Sets the attributes of a file or directory.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="attributes">The attributes to set</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public NtStatus SetFileAttributes(string path, System.IO.FileAttributes attributes)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.SetFileAttributes(path, attributes);
			else
				return DokanResult.AccessDenied;
		}

		/// <summary>
		/// Sets the security attributes for the specified sections of the specified file or directory.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="security">The security to set</param>
		/// <param name="sections">The access control sections to change</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public NtStatus SetFileSecurity(string path, FileSystemSecurity security, AccessControlSections sections)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.SetFileSecurity(path, security, sections);
			else
				return DokanResult.AccessDenied;
		}

		/// <summary>
		/// Sets the creation, last access, and last modification times for a file if they are specified. Any null values mean the value will not be changed.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="creationTime">The new creation time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <param name="lastAccessTime">The new last access time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <param name="lastWriteTime">The new last write time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public NtStatus SetFileTimes(string path, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
		{
			WritableSource writable = Last as WritableSource;
			if (writable != null)
				return writable.SetFileTimes(path, creationTime, lastAccessTime, lastWriteTime);
			else
				return DokanResult.AccessDenied;
		}
	}
}
