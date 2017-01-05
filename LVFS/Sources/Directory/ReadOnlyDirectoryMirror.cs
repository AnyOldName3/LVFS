using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using LVFS.Filesystem;

namespace LVFS.Sources.Directory
{
	class ReadOnlyDirectoryMirror : Source
	{
		private string mDirectoryPath;

		public ReadOnlyDirectoryMirror(string path, Source predecessor) : base(predecessor)
		{
			mDirectoryPath = path;
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
			throw new NotImplementedException();
		}

		public override NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, LVFSContextInfo info)
		{
			throw new NotImplementedException();
		}

		public override FileInformation? GetFileInformation(string path)
		{
			throw new NotImplementedException();
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
