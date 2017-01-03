using System;
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
		private Selector mSelector;

		private string mVolumeLabel, mFileSystemName;

		public LVFS(Selector selector, string volumeLabel, string fileSystemName)
		{
			mSelector = selector;
			mVolumeLabel = volumeLabel;
			mFileSystemName = fileSystemName;
		}

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
			return mSelector.CreateFileHandle(fileName, access, share, mode, options, attributes, info);
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
			files = mSelector.ListFiles(fileName);

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
			streams = null;
			return DokanResult.NotImplemented;
		}

		public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
		{
			Tuple<long, long, long> sizes = mSelector.GetSpaceInformation();

			if (sizes != null)
			{
				freeBytesAvailable = sizes.Item3;
				totalNumberOfBytes = sizes.Item2;
				totalNumberOfFreeBytes = sizes.Item1;

				return DokanResult.Success;
			}
			else
			{
				freeBytesAvailable = totalNumberOfBytes = totalNumberOfFreeBytes = 0;

				return DokanResult.Unsuccessful;
			}
		}

		public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
		{
			FileInformation? returned = mSelector.GetFileInformation(fileName);

			NtStatus returnCode = returned != null ? DokanResult.Success : DokanResult.FileNotFound;
			fileInfo = returned ?? new FileInformation();

			return returnCode;
		}

		public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
		{
			try
			{
				security = mSelector.GetFileSystemSecurity(fileName, sections);
				return security != null ? DokanResult.Success : DokanResult.FileNotFound;
			}
			catch(UnauthorizedAccessException ex)
			{
				security = null;
				return DokanResult.AccessDenied;
			}
		}

		public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
		{
			volumeLabel = mVolumeLabel;
			fileSystemName = mFileSystemName;

			// Values copied from DokenNet's Mirror example
			features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch | FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage | FileSystemFeatures.UnicodeOnDisk;
			
			if (! mSelector.HasWritableSource)
				features |= FileSystemFeatures.ReadOnlyVolume;

			return DokanResult.Success;
		}

		public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}

		public NtStatus Mounted(DokanFileInfo info)
		{
			return mSelector.OnMount() ? DokanResult.Success : DokanResult.Unsuccessful;
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
			return mSelector.OnUnmount() ? DokanResult.Success : DokanResult.Unsuccessful;
		}

		public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
		{
			throw new NotImplementedException();
		}
	}
}
