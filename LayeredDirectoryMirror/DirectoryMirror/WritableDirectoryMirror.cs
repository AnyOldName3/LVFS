using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

using LVFS.External;

namespace LayeredDirectoryMirror.DirectoryMirror
{
	/// <summary>
	/// An LVFS source which mirrors the contents of a directory, and allows the modification of its contents
	/// </summary>
	public class WritableDirectoryMirror : WritableSource
	{
		/// <summary>
		/// The directory being mirrored
		/// </summary>
		public string DirectoryPath { get; private set; }

		/// <summary>
		/// Construct a <see cref="WritableDirectoryMirror"/> mirroring the specified directory.
		/// </summary>
		/// <param name="path">The path to the directory to mirror.</param>
		public WritableDirectoryMirror(string path)
		{
			DirectoryPath = path;
		}

		private string ConvertPath(string path)
		{
			path = path.Substring(1);
			return Path.Combine(DirectoryPath, path);
		}

		/// <inheritdoc/>
		public override NtStatus CheckDirectoryDeletable(string path)
		{
			if (!Directory.Exists(ConvertPath(path)))
				return PredecessorCheckDirectoryDeletable(path);
			else
			{
				return Directory.EnumerateFileSystemEntries(ConvertPath(path)).Any() || PredecessorHasFilesInDirectory(path) ? DokanResult.DirectoryNotEmpty : DokanResult.Success;
			}
		}

		/// <inheritdoc/>
		public override NtStatus CheckFileDeletable(string path)
		{
			if (Directory.Exists(ConvertPath(path)))
				return DokanResult.AccessDenied;
			else if (!File.Exists(ConvertPath(path)))
				return PredecessorCheckFileDeletable(path);
			else
				return DokanResult.Success;
		}

		/// <inheritdoc/>
		public override bool CleanupFileHandle(string path, LVFSContextInfo info)
		{
			try
			{
				object context;
				if (info.Context.TryGetValue(this, out context))
				{
					(context as FileStream)?.Dispose();
					info.Context.Remove(this);
				}
				return PredecessorCleanupFileHandle(path, info);
			}
			catch (Exception)
			{
				// Because there're no checked exceptions in C#, I can't tell what might go wrong here and catch specific exceptions.
				PredecessorCleanupFileHandle(path, info);
				return false;
			}
		}

		/// <inheritdoc/>
		public override bool CloseFileHandle(string path, LVFSContextInfo info)
		{
			// This is the same as Cleanup as there's only one thing that might need tidying, but according to a comment in the DokanNet Mirror example, there potentially are situations where only one of the two is called.
			try
			{
				object context;
				if (info.Context.TryGetValue(this, out context))
				{
					(context as FileStream)?.Dispose();
					info.Context.Remove(this);
				}
				return PredecessorCleanupFileHandle(path, info);
			}
			catch (Exception)
			{
				// Because there're no checked exceptions in C#, I can't tell what might go wrong here and catch specific exceptions.
				PredecessorCleanupFileHandle(path, info);
				return false;
			}
		}

		/// <inheritdoc/>
		public override bool ControlsFile(string path)
		{
			return File.Exists(ConvertPath(path)) || Directory.Exists(ConvertPath(path));
		}

		/// <inheritdoc/>
		public override NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus FlushBuffers(string path, LVFSContextInfo info)
		{
			NtStatus result = DokanResult.Success;
			if (info.Context.ContainsKey(this))
			{
				try
				{
					FileStream stream = info.Context[this] as FileStream;
					if (stream != null)
						stream.Flush();
				}
				catch (IOException)
				{
					result =  DokanResult.DiskFull;
				}
			}

			NtStatus predecessorResult =  PredecessorFlushBuffers(path, info);

			if (result == DokanResult.Success && predecessorResult != DokanResult.Success)
				return predecessorResult;
			else
				return result;
		}

		/// <inheritdoc/>
		public override FileInformation? GetFileInformation(string path)
		{
			var filePath = ConvertPath(path);
			FileSystemInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
			{
				fileInfo = new DirectoryInfo(filePath);
				if (!fileInfo.Exists)
					// This source has not got the requested file.
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
			string fullPath = ConvertPath(path);
			if (Directory.Exists(fullPath))
				return (FileSystemSecurity)Directory.GetAccessControl(fullPath, sections);
			else if (File.Exists(fullPath))
				return File.GetAccessControl(fullPath, sections);
			else
				return GetPredecessorFileSystemSecurity(path, sections);
		}

		/// <inheritdoc/>
		public override Tuple<long, long, long> GetSpaceInformation()
		{
			var driveInfo = DriveInfo.GetDrives().Single(drive => drive.RootDirectory.Name == Path.GetPathRoot(DirectoryPath + "\\"));

			return new Tuple<long, long, long>(driveInfo.TotalFreeSpace, driveInfo.TotalSize, driveInfo.AvailableFreeSpace);
		}

		/// <inheritdoc/>
		public override bool HasFile(string path)
		{
			return ControlsFile(path) || PredecessorHasFile(path);
		}

		/// <inheritdoc/>
		public override bool HasFilesInDirectory(string path)
		{
			string converted = ConvertPath(path);
			if (Directory.Exists(converted))
			{
				if (Directory.EnumerateFileSystemEntries(converted).Any())
					return true;
			}
			return PredecessorHasFilesInDirectory(path);
		}

		/// <inheritdoc/>
		public override IList<FileInformation> ListFiles(string path)
		{
			IList<FileInformation> predecessorList = ListPredecessorFiles(path);

			HashSet<string> names = new HashSet<string>();
			IList<FileInformation> resultList;
			if (Directory.Exists(ConvertPath(path)))
			{
				IList<FileInformation> thisList = new DirectoryInfo(ConvertPath(path)).GetFileSystemInfos().Select((fileInfo) =>
				{
					names.Add(fileInfo.Name);
					return new FileInformation
					{
						FileName = fileInfo.Name,
						Attributes = fileInfo.Attributes,
						CreationTime = fileInfo.CreationTime,
						LastAccessTime = fileInfo.LastAccessTime,
						LastWriteTime = fileInfo.LastWriteTime,
						Length = (fileInfo as FileInfo)?.Length ?? 0
					};
				}).ToArray();
				resultList = new List<FileInformation>(thisList);
			}
			else
				resultList = new List<FileInformation>();

			foreach (FileInformation fileInfo in predecessorList)
			{
				if (!names.Contains(fileInfo.FileName))
					resultList.Add(fileInfo);
			}

			return resultList;
		}

		/// <inheritdoc/>
		public override NtStatus MoveFile(string currentPath, string newPath, bool replace, LVFSContextInfo info)
		{
			var fullCurrentPath = ConvertPath(currentPath);
			var fullNewPath = ConvertPath(newPath);

			if (ControlsFile(currentPath))
			{
				try
				{
					(info.Context[this] as FileStream)?.Dispose();
					info.Context[this] = null;

					if (!HasFile(newPath))
					{
						if (info.IsDirectory)
							Directory.Move(fullCurrentPath, fullNewPath);
						else
							File.Move(fullCurrentPath, fullNewPath);

						return DokanResult.Success;
					}
					else if (replace)
					{
						// Delete the original if possible, but otherwise ignore it when it's in another layer as it doesn't affect the external behaviour of the VFS

						// replace is incompatible with directories
						if (info.IsDirectory)
							return DokanResult.AccessDenied;

						if (ControlsFile(newPath))
						{
							try
							{
								File.Delete(fullNewPath);
								File.Move(fullCurrentPath, fullNewPath);
								return DokanResult.Success;
							}
							catch (UnauthorizedAccessException)
							{
								return DokanResult.AccessDenied;
							}
						}
						else
						{
							// A predecessor source has the file to be replaced, so we can ignore it.
							try
							{
								File.Move(fullCurrentPath, fullNewPath);
								return DokanResult.Success;
							}
							catch (UnauthorizedAccessException)
							{
								return DokanResult.AccessDenied;
							}
						}
					}
					else
						return DokanResult.FileExists;
				}
				catch (UnauthorizedAccessException)
				{
					return DokanResult.AccessDenied;
				}
			}
			else
				return PredecessorMoveFile(currentPath, newPath, replace, info);
		}

		/// <inheritdoc/>
		public override bool OnMount()
		{
			return Directory.Exists(DirectoryPath);
		}

		/// <inheritdoc/>
		public override bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			if (info.Context.ContainsKey(this))
			{
				var stream = info.Context[this] as FileStream;
				lock (stream)
				{
					stream.Position = offset;
					bytesRead = stream.Read(buffer, 0, buffer.Length);
				}
				return true;
			}
			else if (File.Exists(ConvertPath(path)))
			{
				using (var stream = new FileStream(ConvertPath(path), FileMode.Open, System.IO.FileAccess.Read))
				{
					stream.Position = offset;

					bytesRead = stream.Read(buffer, 0, buffer.Length);
				}
				return true;
			}
			else
				return PredecessorReadFile(path, buffer, out bytesRead, offset, info);
		}

		/// <inheritdoc/>
		public override NtStatus SetAllocatedSize(string path, long allocationSize, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override NtStatus SetFileAttributes(string path, FileAttributes attributes)
		{
			if (ControlsFile(path))
			{
				try
				{
					File.SetAttributes(ConvertPath(path), attributes);
					return DokanResult.Success;
				}
				catch (UnauthorizedAccessException)
				{
					return DokanResult.AccessDenied;
				}
				catch (FileNotFoundException)
				{
					return DokanResult.FileNotFound;
				}
				catch (DirectoryNotFoundException)
				{
					return DokanResult.PathNotFound;
				}
			}
			else
				return PredecessorSetFileAttributes(path, attributes);
		}

		/// <inheritdoc/>
		public override NtStatus SetFileSecurity(string path, FileSystemSecurity security, AccessControlSections sections)
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
			if (info.Context.ContainsKey(this))
			{
				try
				{
					(info.Context[this] as FileStream)?.Lock(startOffset, length);
					return true;
				}
				catch (IOException)
				{
					return false;
				}
			}
			else
				return PredecessorTryLockFileRegion(path, startOffset, length, info);
		}

		/// <inheritdoc/>
		public override bool TryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			if (info.Context.ContainsKey(this))
			{
				try
				{
					(info.Context[this] as FileStream)?.Unlock(startOffset, length);
					return true;
				}
				catch (IOException)
				{
					return false;
				}
			}
			else
				return PredecessorTryUnlockFileRegion(path, startOffset, length, info);
		}

		/// <inheritdoc/>
		public override NtStatus WriteFile(string path, byte[] buffer, out int bytesWritten, long offset, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
