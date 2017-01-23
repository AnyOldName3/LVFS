using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.External
{
	/// <summary>
	/// Represents an LVFS source which supports write operations.
	/// </summary>
	public abstract class WritableSource : Source
	{
		/// <summary>
		/// Constructor for WritableSource
		/// </summary>
		protected WritableSource() : base()
		{
		}

		/// <summary>
		/// As with <see cref="CheckDirectoryDeletable"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the directory to check</param>
		/// <returns>The result for the predecessor if it supports the operation, or a suitable error status if not.</returns>
		protected NtStatus PredecessorCheckDirectoryDeletable(string path)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.CheckDirectoryDeletable(path);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.PathNotFound;
		}

		/// <summary>
		/// As with <see cref="CheckFileDeletable(string)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file to check</param>
		/// <returns>The result for the predecessor if it supports the operation, or a suitable error status if not.</returns>
		protected NtStatus PredecessorCheckFileDeletable(string path)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.CheckFileDeletable(path);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="SetFileAttributes(string, FileAttributes)"/>, but for the predecessor source.
		/// </summary>
		///<param name="path">The path to the file</param>
		/// <param name="attributes">The attributes to set</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		protected NtStatus PredecessorSetFileAttributes(string path, FileAttributes attributes)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.SetFileAttributes(path, attributes);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="FlushBuffers(string, LVFSContextInfo)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file whose buffers to flush</param>
		/// <param name="info">Information concerning the context for the operation</param>
		/// <returns><see cref="DokanResult.Success"/> if all buffers were flushed, If not, an appropriate error status.</returns>
		protected NtStatus PredecessorFlushBuffers(string path, LVFSContextInfo info)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.FlushBuffers(path, info);
			else
				return DokanResult.Success;
		}

		/// <summary>
		/// As with <see cref="MoveFile(string, string, bool, LVFSContextInfo)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="currentPath">The current path of the file/directory</param>
		/// <param name="newPath">The new path of the file/directory</param>
		/// <param name="replace">Whether to replace any existing file occupying the new path</param>
		/// <param name="info">Information concerning the context for this operation.</param>
		/// <returns><see cref="DokanResult.Success"/> if the file was moved. Otherwise, an appropriate error status.</returns>
		protected NtStatus PredecessorMoveFile(string currentPath, string newPath, bool replace, LVFSContextInfo info)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.MoveFile(currentPath, newPath, replace, info);
			else
				return PredecessorHasFile(currentPath) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="SetAllocatedSize(string, long, LVFSContextInfo)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="allocationSize">The new size to allocate</param>
		/// <param name="info">Information concerning the context for this operation</param>
		/// <returns><see cref="DokanResult.Success"/> if the allocation size was changed or already the correct value. If not, an appropriate error status.</returns>
		protected NtStatus PredecessorSetAllocatedSize(string path, long allocationSize, LVFSContextInfo info)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.SetAllocatedSize(path, allocationSize, info);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="SetLength(string, long, LVFSContextInfo)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="length">The new length of the file</param>
		/// <param name="info">Information concerning the context of this operation</param>
		/// <returns><see cref="DokanResult.Success"/> if the requested length is now the length of the file. If not, an appropriate error status.</returns>
		protected NtStatus PredecessorSetLength(string path, long length, LVFSContextInfo info)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.SetLength(path, length, info);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="SetFileSecurity(string, FileSystemSecurity, AccessControlSections)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="security">The security to set</param>
		/// <param name="sections">The access control sections to change</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		protected NtStatus PredecessorSetFileSecurity(string path, FileSystemSecurity security, AccessControlSections sections)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.SetFileSecurity(path, security, sections);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="SetFileTimes(string, DateTime?, DateTime?, DateTime?)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="creationTime">The new creation time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <param name="lastAccessTime">The new last access time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <param name="lastWriteTime">The new last write time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		protected NtStatus PredecessorSetFileTimes(string path, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.SetFileTimes(path, creationTime, lastAccessTime, lastWriteTime);
			else
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
		}

		/// <summary>
		/// As with <see cref="WriteFile(string, byte[], out int, long, LVFSContextInfo)"/>, but for the predecessor source.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="buffer">A buffer containing the data to write</param>
		/// <param name="bytesWritten">The number of bytes transferred from the buffer to the file</param>
		/// <param name="offset">The offset at which to start the write</param>
		/// <param name="info">Information concerning the context of this operation.</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		protected NtStatus PredecessorWriteFile(string path, byte[] buffer, out int bytesWritten, long offset, LVFSContextInfo info)
		{
			WritableSource predecessor = mPredecessor as WritableSource;
			if (predecessor != null)
				return predecessor.WriteFile(path, buffer, out bytesWritten, offset, info);
			else
			{
				bytesWritten = 0;
				return PredecessorHasFile(path) ? DokanResult.AccessDenied : DokanResult.FileNotFound;
			}
		}

		/// <summary>
		/// Checks if a directory can be deleted, but doesn't actually do so. Must not return a success result if the directory is non-empty.
		/// </summary>
		/// <param name="path">The path to the directory to check</param>
		/// <returns><see cref="DokanResult.Success"/> if the directory can be deleted. If not, an appropriate status message to explain why.</returns>
		public abstract NtStatus CheckDirectoryDeletable(string path);

		/// <summary>
		/// Checks if a file can be deleted, but doesn't actually do so. For the purpose of this method, directories do not count as files.
		/// </summary>
		/// <param name="path">The path to the file to check</param>
		/// <returns><see cref="DokanResult.Success"/> if the file can be delted. If not, an appropriate error status.</returns>
		public abstract NtStatus CheckFileDeletable(string path);

		/// <summary>
		/// Clears any buffers for the context, and ensures any buffered data is written to the actual file.
		/// </summary>
		/// <param name="path">The path to the file whose buffers to flush</param>
		/// <param name="info">Information concerning the context for the operation</param>
		/// <returns><see cref="DokanResult.Success"/> if all buffers were flushed, If not, an appropriate error status.</returns>
		public abstract NtStatus FlushBuffers(string path, LVFSContextInfo info);

		/// <summary>
		/// Moves the file/directory from its current path to a new one, replacing any existing files only if replace is set.
		/// </summary>
		/// <param name="currentPath">The current path of the file/directory</param>
		/// <param name="newPath">The new path of the file/directory</param>
		/// <param name="replace">Whether to replace any existing file occupying the new path</param>
		/// <param name="info">Information concerning the context for this operation.</param>
		/// <returns><see cref="DokanResult.Success"/> if the file was moved. Otherwise, an appropriate error status.</returns>
		public abstract NtStatus MoveFile(string currentPath, string newPath, bool replace, LVFSContextInfo info);

		/// <summary>
		/// Sets the allocated size for the file. If this is less than the current length, trucate the file. If the file does not grow to fill this space before the handle is released, it may be freed.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="allocationSize">The new size to allocate</param>
		/// <param name="info">Information concerning the context for this operation</param>
		/// <returns><see cref="DokanResult.Success"/> if the allocation size was changed or already the correct value. If not, an appropriate error status.</returns>
		public abstract NtStatus SetAllocatedSize(string path, long allocationSize, LVFSContextInfo info);

		/// <summary>
		/// Sets the length of the file.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="length">The new length of the file</param>
		/// <param name="info">Information concerning the context of this operation</param>
		/// <returns><see cref="DokanResult.Success"/> if the requested length is now the length of the file. If not, an appropriate error status.</returns>
		public abstract NtStatus SetLength(string path, long length, LVFSContextInfo info);

		/// <summary>
		/// Sets the attributes of a file or directory.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="attributes">The attributes to set</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public abstract NtStatus SetFileAttributes(string path, FileAttributes attributes);

		/// <summary>
		/// Sets the security attributes for the specified sections of the specified file or directory.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="security">The security to set</param>
		/// <param name="sections">The access control sections to change</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public abstract NtStatus SetFileSecurity(string path, FileSystemSecurity security, AccessControlSections sections);

		/// <summary>
		/// Sets the creation, last access, and last modification times for a file if they are specified. Any null values mean the value will not be changed.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="creationTime">The new creation time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <param name="lastAccessTime">The new last access time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <param name="lastWriteTime">The new last write time for the file, or <c>null</c> if it is not to be changed.</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public abstract NtStatus SetFileTimes(string path, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime);

		/// <summary>
		/// Writes the contents of the buffer to the requested file, starting at the requested offset, and sets the bytes written value to the number of bytes successfully written to the file.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="buffer">A buffer containing the data to write</param>
		/// <param name="bytesWritten">The number of bytes transferred from the buffer to the file</param>
		/// <param name="offset">The offset at which to start the write</param>
		/// <param name="info">Information concerning the context of this operation.</param>
		/// <returns><see cref="DokanResult.Success"/> if the operation was successful. If not, an appropriate error status.</returns>
		public abstract NtStatus WriteFile(string path, byte[] buffer, out int bytesWritten, long offset, LVFSContextInfo info);
	}
}
