using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

using LVFS.External;

namespace LayeredDirectoryMirror.OneWay
{
	/// <summary>
	/// An LVFS source which allows all operations on its entire contents, but causes no changes to be propagated to predecessor sources. Achieves this by copying files from lower priority sources when they are modified.
	/// </summary>
	public class SimpleOneWayContentMirror : WritableSource
	{
		/// <summary>
		/// The directory being mirrored
		/// </summary>
		public string DirectoryPath { get; private set; }

		/// <summary>
		/// Constructs the class to mirror the specified path, creating it if it does not exist and <paramref name="create"/> is set.
		/// </summary>
		/// <param name="path">The path to mirror</param>
		/// <param name="create">Whether to create the path if it does not exist</param>
		public SimpleOneWayContentMirror(string path, bool create)
		{
			if (create && !Directory.Exists(path))
				Directory.CreateDirectory(path);
			DirectoryPath = path;
		}

		/// <summary>
		/// Constructs the class to mirror the specified path if it exists.
		/// </summary>
		/// <param name="path">The path to mirror</param>
		public SimpleOneWayContentMirror(string path) : this(path, false)
		{
		}

		private string ConvertPath(string path)
		{
			path = path.Substring(1);
			return Path.Combine(DirectoryPath, path);
		}

		/// <inheritdoc/>
		public override NtStatus CheckDirectoryDeletable(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus CheckFileDeletable(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool CleanupFileHandle(string path, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool CloseFileHandle(string path, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool ControlsFile(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus FlushBuffers(string path, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override FileInformation? GetFileInformation(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override FileSystemSecurity GetFileSystemSecurity(string path, AccessControlSections sections)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override Tuple<long, long, long> GetSpaceInformation()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool HasFile(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool HasFilesInDirectory(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override IList<FileInformation> ListFiles(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus MoveFile(string currentPath, string newPath, bool replace, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool OnMount()
		{
			return Directory.Exists(DirectoryPath);
		}

		public override bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus SetAllocatedSize(string path, long allocationSize, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus SetFileAttributes(string path, FileAttributes attributes)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus SetFileSecurity(string path, FileSystemSecurity security, AccessControlSections sections, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus SetFileTimes(string path, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus SetLength(string path, long length, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool TryLockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override bool TryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus WriteFile(string path, byte[] buffer, out int bytesWritten, long offset, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
