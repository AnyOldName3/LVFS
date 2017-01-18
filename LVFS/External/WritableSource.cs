using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.External
{
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
				return GetPredecessorFileInformation(path) != null ? DokanResult.AccessDenied : DokanResult.PathNotFound;
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
				return GetPredecessorFileInformation(path) != null ? DokanResult.AccessDenied : DokanResult.FileNotFound;
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
	}
}
