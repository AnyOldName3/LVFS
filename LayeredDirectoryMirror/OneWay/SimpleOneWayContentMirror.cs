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

		private bool CopyFromPredecessor(string path)
		{
			//Content
			//Security
			//Information
			// - attributes
			// - times
			// - name
			// - length

			try
			{
				var fileInfo = GetPredecessorFileInformation(path).Value;
				var fileSecurity = GetPredecessorFileSystemSecurity(path, AccessControlSections.All);
				var convertedPath = ConvertPath(path);
				using (var stream = new FileStream(convertedPath, FileMode.CreateNew, FileSystemRights.FullControl, FileShare.Read, 4096, FileOptions.SequentialScan, fileSecurity as FileSecurity))
				{
					var currentOffset = 0L;
					var end = fileInfo.Length;
					var buffer = new byte[end < int.MaxValue ? end : int.MaxValue];

					while (currentOffset < end)
					{
						int bytesRead;
						PredecessorReadFile(path, buffer, out bytesRead, currentOffset, null);
						stream.Write(buffer, 0, bytesRead);
						currentOffset += bytesRead;
					}
				}
				File.SetAttributes(convertedPath, fileInfo.Attributes);
				File.SetCreationTime(convertedPath, fileInfo.CreationTime.Value);
				File.SetLastAccessTime(convertedPath, fileInfo.LastAccessTime.Value);
				File.SetLastWriteTime(convertedPath, fileInfo.LastWriteTime.Value);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private bool IsDirectoryVisible(string path)
		{
			return !IsDirectoryShadowed(path) && Directory.Exists(path);
		}

		private bool IsFileVisible(string path)
		{
			return !IsFileShadowed(path) && File.Exists(path);
		}

		private bool IsPredecessorDirectoryVisible(string path)
		{
			return !IsDirectoryShadowed(ConvertPath(path)) && PredecessorHasDirectory(path);
		}

		private bool IsPredecessorFileVisible(string path)
		{
			return !IsFileShadowed(ConvertPath(path)) && PredecessorHasRegularFile(path);
		}

		private bool IsDirectoryShadowed(string path)
		{
			// TODO
			return false;
		}

		private bool IsFileShadowed(string path)
		{
			// TODO
			return false;
		}

		private void ShadowDirectory(string path)
		{
			// TODO
		}

		private void ShadowFile(string path)
		{
			// TODO
		}

		/// <inheritdoc/>
		public override NtStatus CheckDirectoryDeletable(string path)
		{
			if (HasFilesInDirectory(path))
				return DokanResult.DirectoryNotEmpty;
			else
				return DokanResult.Success;
		}

		/// <inheritdoc/>
		public override NtStatus CheckFileDeletable(string path)
		{
			if (HasDirectory(path))
				return DokanResult.AccessDenied;
			else if (HasRegularFile(path))
				return DokanResult.Success;
			else
				return DokanResult.FileNotFound;
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
			var convertedPath = ConvertPath(path);
			return File.Exists(convertedPath) || Directory.Exists(convertedPath) || IsFileShadowed(convertedPath) || IsDirectoryShadowed(convertedPath);
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
			var convertedPath = ConvertPath(path);

			if (IsFileShadowed(convertedPath) || IsDirectoryShadowed(convertedPath))
				return null;

			FileSystemInfo fileInfo = new FileInfo(convertedPath);
			if (!fileInfo.Exists)
			{
				fileInfo = new DirectoryInfo(convertedPath);
				if (!fileInfo.Exists)
					return GetPredecessorFileInformation(path);
			}

			return new FileInformation
			{
				FileName = path,
				Attributes = fileInfo.Attributes,
				CreationTime = fileInfo.CreationTime,
				LastAccessTime = fileInfo.LastAccessTime,
				LastWriteTime = fileInfo.LastWriteTime,
				Length = (fileInfo as FileInfo)?.Length ?? 0
			};
		}

		/// <inheritdoc/>
		public override FileSystemSecurity GetFileSystemSecurity(string path, AccessControlSections sections)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override Tuple<long, long, long> GetSpaceInformation()
		{
			var driveInfo = DriveInfo.GetDrives().Single(drive => string.Equals(drive.RootDirectory.Name, Path.GetPathRoot(DirectoryPath + "\\"), StringComparison.OrdinalIgnoreCase));
			
			return new Tuple<long, long, long>(driveInfo.TotalFreeSpace, driveInfo.TotalSize, driveInfo.AvailableFreeSpace);
		}

		/// <inheritdoc/>
		public override bool HasDirectory(string path)
		{
			return IsDirectoryVisible(ConvertPath(path)) || IsPredecessorDirectoryVisible(path);
		}

		/// <inheritdoc/>
		public override bool HasFile(string path)
		{
			return HasDirectory(path) || HasRegularFile(path);
		}

		/// <inheritdoc/>
		public override bool HasFilesInDirectory(string path)
		{
			var convertedPath = ConvertPath(path);
			if (Directory.Exists(convertedPath) && !IsDirectoryShadowed(convertedPath))
			{
				var rawFiles = Directory.EnumerateFiles(convertedPath);
				foreach (var file in rawFiles)
				{
					if (!IsFileShadowed(file))
						return true;
				}
				rawFiles = Directory.EnumerateDirectories(convertedPath);
				foreach (var dir in rawFiles)
				{
					if (!IsDirectoryShadowed(dir))
						return true;
				}
			}
			return PredecessorHasFilesInDirectory(path);
		}

		/// <inheritdoc/>
		public override bool HasRegularFile(string path)
		{
			return IsFileVisible(ConvertPath(path)) || IsPredecessorFileVisible(path);
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

		/// <inheritdoc/>
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
