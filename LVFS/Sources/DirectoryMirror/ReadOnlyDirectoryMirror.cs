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
			throw new NotImplementedException();
		}

		public override FileInformation? GetFileInformation(string path)
		{
			var filePath = ConvertPath(path);
			FileSystemInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
			{
				fileInfo = new DirectoryInfo(filePath);
				if (!fileInfo.Exists)
					return null;
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
			throw new NotImplementedException();
		}

		public override Tuple<long, long, long> GetSpaceInformation()
		{
			throw new NotImplementedException();
		}

		public override IList<FileInformation> ListFiles(string path)
		{
			throw new NotImplementedException();
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
