﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.Filesystem
{
	class LVFS : IDokanOperations
	{
		private Selector selector;

		public void Cleanup(string fileName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public void CloseFile(string fileName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus DeleteFile(string fileName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
		{
			files = selector.ListFiles(fileName);

			return files != null ? DokanResult.Success : DokanResult.FileNotFound;
		}

		public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, DokanFileInfo info)
		{
			// Note: Implementing this method explicitly is not necessary, as Dokan can wrap FindFiles.

			files = null;
			return DokanResult.NotImplemented;
		}

		public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
		{
			FileInformation? returned = selector.GetFileInformation(fileName);

			NtStatus returnCode = returned != null ? DokanResult.Success : DokanResult.FileNotFound;
			fileInfo = returned ?? new FileInformation();

			return returnCode;
		}

		public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus Mounted(DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus Unmounted(DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
