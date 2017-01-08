using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using LVFS.Filesystem;

namespace LVFS.Sources.DirectoryMirror
{
	class ReadOnlyDirectoryMirror : Source
	{
		public string DirectoryPath { get; private set; }

		public ReadOnlyDirectoryMirror(string path, Source predecessor) : base(predecessor)
		{
			DirectoryPath = path;
		}

		private string ConvertPath(string path)
		{
			return Path.Combine(DirectoryPath, path);
		}

		public override bool CleanupFileHandle(string path, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		public override bool CloseFileHandle(string path, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		public override bool ControlsFile(string path)
		{
			return File.Exists(ConvertPath(path)) || Directory.Exists(ConvertPath(path));
		}

		public override NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info)
		{
			var filePath = ConvertPath(path);

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
								return DokanResult.PathNotFound;

						case FileMode.OpenOrCreate:
							if (Directory.Exists(filePath))
								return DokanResult.Success;
							else if (File.Exists(filePath))
								return DokanResult.FileExists;
							else
								return DokanResult.AccessDenied;

						case FileMode.CreateNew:
							if (Directory.Exists(filePath))
								return DokanResult.AlreadyExists;
							else if (File.Exists(filePath))
								return DokanResult.FileExists;
							else
								return DokanResult.AccessDenied;

						default:
							return DokanResult.AccessDenied;
					}
				}
				catch (UnauthorizedAccessException)
				{
					return DokanResult.AccessDenied;
				}
			}
			else
			{
				var isDirectory = Directory.Exists(filePath);
				var pathExists = isDirectory || File.Exists(filePath);

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
							return DokanResult.FileNotFound;

					case FileMode.OpenOrCreate:
						if (pathExists)
							// Go to the regular handler
							break;
						else
							return DokanResult.AccessDenied;

					case FileMode.CreateNew:
						if (pathExists)
							return DokanResult.FileExists;
						else
							return DokanResult.AccessDenied;

					case FileMode.Truncate:
						if (pathExists)
							return DokanResult.AccessDenied;
						else
							return DokanResult.FileNotFound;

					case FileMode.Create:
						return DokanResult.AccessDenied;

					case FileMode.Append:
						if (pathExists)
							return DokanResult.AccessDenied;
						else
							return DokanResult.FileNotFound;

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

		public override Tuple<long, long, long> GetSpaceInformation()
		{
			throw new NotImplementedException();
		}

		public override IList<FileInformation> ListFiles(string path)
		{
			IList<FileInformation> predecessorList = ListPredecessorFiles(path);

			HashSet<string> names = new HashSet<string>();
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

			IList<FileInformation> resultList = new List<FileInformation>(thisList);
			foreach (FileInformation fileInfo in predecessorList)
			{
				if (!names.Contains(fileInfo.FileName))
					resultList.Add(fileInfo);
			}

			return resultList;
		}

		public override bool OnMount()
		{
			return Directory.Exists(DirectoryPath);
		}

		public override bool ReadFile(string path, byte[] buffer, out int bytesRead, long offset, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		public override bool TryLockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		public override bool TryUnlockFileRegion(string path, long startOffset, long length, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
