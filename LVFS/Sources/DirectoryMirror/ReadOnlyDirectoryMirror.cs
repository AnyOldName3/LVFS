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
								return DokanResult.Success;
							else if (File.Exists(filePath))
								return NtStatus.NotADirectory;
							else
								return DokanResult.PathNotFound;

						case FileMode.CreateNew:
							if (Directory.Exists(filePath))
								return DokanResult.AlreadyExists;
							else if (File.Exists(filePath))
								return DokanResult.FileExists;
							else
								return DokanResult.AccessDenied;

						default:
							throw new ArgumentException(mode.ToString(), "mode");
					}
				}
				catch (UnauthorizedAccessException)
				{
					return DokanResult.AccessDenied;
				}
			}
			else
			{
				return DokanResult.NotImplemented;
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
