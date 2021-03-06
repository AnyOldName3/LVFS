﻿using System;
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
	/// An LVFS source which mirrors the contents of a directory, but does not allow any changes to be made to that directory's contents.
	/// </summary>
	public class ReadOnlyDirectoryMirror : Source
	{
		/// <summary>
		/// The directory being mirrored
		/// </summary>
		public string DirectoryPath { get; private set; }

		/// <summary>
		/// Construct a <see cref="ReadOnlyDirectoryMirror"/> mirroring the specified directory.
		/// </summary>
		/// <param name="path">The path to the directory to mirror.</param>
		public ReadOnlyDirectoryMirror(string path)
		{
			DirectoryPath = path;
		}

		private string ConvertPath(string path)
		{
			path = path.Substring(1);
			return Path.Combine(DirectoryPath, path);
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
				return true;
			}
			catch (Exception)
			{
				// Because there're no checked exceptions in C#, I can't tell what might go wrong here and catch specific exceptions.
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
				return true;
			}
			catch (Exception)
			{
				// Because there're no checked exceptions in C#, I can't tell what might go wrong here and catch specific exceptions.
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
			var filePath = ConvertPath(path);

			var isDirectory = Directory.Exists(filePath);
			var pathExists = isDirectory || File.Exists(filePath);

			// Check this first for performance reasons. Keep checking it later in case anything's been meddled with
			if(!pathExists)
				return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

			if (info.IsDirectory)
			{
				try
				{
					switch (mode)
					{
						case FileMode.Open:
							if (Directory.Exists(filePath))
								// The original Dokan mirror this is based on called new DirectoryInfo(filePath).EnumerateFileSystemInfos().Any();, but did nothing with its result at this point
								return DokanResult.Success;
							else if (File.Exists(filePath))
								return NtStatus.NotADirectory;
							else
								return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

						case FileMode.OpenOrCreate:
							if (Directory.Exists(filePath))
								return DokanResult.Success;
							else if (File.Exists(filePath))
								return DokanResult.FileExists;
							else
								return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

						case FileMode.CreateNew:
							if (Directory.Exists(filePath))
								return DokanResult.AlreadyExists;
							else if (File.Exists(filePath))
								return DokanResult.FileExists;
							else
								return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

						default:
							if (Directory.Exists(filePath) || File.Exists(filePath))
								return DokanResult.AccessDenied;
							else
								return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);
					}
				}
				catch (UnauthorizedAccessException)
				{
					return DokanResult.AccessDenied;
				}
			}
			else
			{
				switch (mode)
				{
					case FileMode.Open:
						if (pathExists)
						{
							var dataAccess = DokanNet.FileAccess.ReadData | DokanNet.FileAccess.WriteData | DokanNet.FileAccess.AppendData | DokanNet.FileAccess.Execute | DokanNet.FileAccess.GenericExecute | DokanNet.FileAccess.GenericWrite | DokanNet.FileAccess.GenericRead;
							var readWriteOnlyAttributes = (access & dataAccess) == 0;
							if (isDirectory || readWriteOnlyAttributes)
							{
								if (isDirectory && access.HasFlag(DokanNet.FileAccess.Delete) && !access.HasFlag(DokanNet.FileAccess.Synchronize))
									// It's a delete request on a directory.
									return DokanResult.AccessDenied;

								info.IsDirectory = isDirectory;

								return DokanResult.Success;
							}
							else
								// Go to the regular handler
								break;
						}
						else
							return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

					case FileMode.OpenOrCreate:
						if (pathExists)
							// Go to the regular handler
							break;
						else
							return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

					case FileMode.CreateNew:
						if (pathExists)
							return DokanResult.FileExists;
						else
							return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

					case FileMode.Truncate:
						if (pathExists)
							return DokanResult.AccessDenied;
						else
							return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

					case FileMode.Create:
						if (pathExists)
							return DokanResult.AccessDenied;
						else
							return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

					case FileMode.Append:
						if (pathExists)
							return DokanResult.AccessDenied;
						else
							return PredecessorCreateFileHandle(path, access, share, mode, options, attributes, info);

					default:
						// This code should never be reached
						throw new ArgumentException("Unknown file mode: " + mode, nameof(mode));
				}

				try
				{
					info.Context[this] = new FileStream(filePath, mode, System.IO.FileAccess.Read, share, 4096, options);

					if (mode == FileMode.Open)
						return DokanResult.Success;
					else
						return DokanResult.AlreadyExists;
				}
				catch (UnauthorizedAccessException)
				{
					return DokanResult.AccessDenied;
				}
				catch (DirectoryNotFoundException)
				{
					return DokanResult.PathNotFound;
				}
				catch (Exception ex)
				{
					var hr = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(ex);
					switch (hr)
					{
						case 0x80070020: //Sharing violation
							return DokanResult.SharingViolation;
						default:
							throw;
					}
				}
			}
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
		public override bool HasDirectory(string path)
		{
			return Directory.Exists(ConvertPath(path)) || PredecessorHasDirectory(path);
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
		public override bool HasRegularFile(string path)
		{
			return File.Exists(ConvertPath(path)) || PredecessorHasRegularFile(path);
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
		public override bool OnMount()
		{
			return Directory.Exists(DirectoryPath);
		}

		/// <inheritdoc/>
		public override bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			if (info != null && info.Context.ContainsKey(this))
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
	}
}
