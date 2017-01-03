using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;

using DokanNet;

using LVFS.Sources;

namespace LVFS.Filesystem
{
	/// <summary>
	/// A class responsible for fetching the Source(s) responsible for files and files available from the collection of Sources
	/// </summary>
	class Selector
	{
		private IList<Source> mSources;
		private Source mOutputSource;

		/// <summary>
		/// Constructs a new Selector with a list of Sources
		/// </summary>
		/// <param name="sources">The list of Sources to use</param>
		public Selector(IList<Source> sources)
		{
			mSources = new List<Source>(sources);
			mOutputSource = mSources.Last<Source>();
		}

		/// <summary>
		/// Constructs a new Selector with no Sources
		/// </summary>
		public Selector()
		{
			mSources = new List<Source>();
			mOutputSource = null;
		}

		/// <summary>
		/// Adds an additional Source to the LVFS as the highest priority Source
		/// </summary>
		/// <param name="source">The Source to add</param>
		public void AddSource(Source source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			mSources.Add(source);
		}

		public bool HasWritableSource { get { return mOutputSource != null; } }

		/// <summary>
		/// Gets the source responsible for a file/directory
		/// </summary>
		/// <param name="fileName">The path to the file/directory</param>
		/// <returns>The Source responsible for the file/directory</returns>
		public Source SourceOf(string fileName)
		{
			for (int i = mSources.Count - 1; i >= 0; i--)
			{
				if (mSources[i].ControlsFile(fileName))
					return mSources[i];
			}
			return null;
		}

		/// <summary>
		/// Lists the files in the directory represented by the input.
		/// </summary>
		/// <param name="path">The directory to list the contents of</param>
		/// <returns>A list of file information about the directory contents, or null if the directory does not exist within the VFS</returns>
		public IList<FileInformation> ListFiles(string path)
		{
			return mSources.Last<Source>().ListFiles(path);
		}

		public FileInformation? GetFileInformation(string path)
		{
			return mSources.Last<Source>().GetFileInformation(path);
		}

		/// <summary>
		/// If there exists a writable source, returns a tuple of the free, total and available space for the storage medium holding the output source. Otherwise, returns null.
		/// </summary>
		/// <returns>A tuple of the free, total and available bytes of space for the output source's storage medium</returns>
		public Tuple<long, long, long> GetSpaceInformation()
		{
			return mOutputSource != null? mOutputSource.GetSpaceInformation() : null;
		}

		/// <summary>
		/// Gets a FileSystemSecurity object representing security information for the requested path, filtered to only include the specified sections. Returns null if the file cannot be found, and throws an UnauthorisedAccessException if the OS denies access to the data requested.
		/// </summary>
		/// <param name="path">The path to get security data for</param>
		/// <param name="sections">The sections of security data to get</param>
		/// <returns>The security data</returns>
		/// <exception cref="UnauthorizedAccessException">Thrown if the OS denies access to the data requested.</exception>
		public FileSystemSecurity GetFileSystemSecurity(string path, AccessControlSections sections)
		{
			return mSources.Last<Source>().GetFileSystemSecurity(path, sections);
		}

		/// <summary>
		/// Called when a filesystem using this selector is mounted
		/// </summary>
		/// <returns>A boolean representing whether all sources were capable of being mounted.</returns>
		public bool OnMount()
		{
			bool success = true;
			for (int i = mSources.Count - 1; i >= 0; i--)
				success &= mSources[i].OnMount();
			return success;
		}

		/// <summary>
		/// Called when a filesystem using this selector is unmounted
		/// </summary>
		/// <returns>A boolean representing whether all sources were capable of being unmounted.</returns>
		public bool OnUnmount()
		{
			bool success = true;
			for (int i = mSources.Count - 1; i >= 0; i--)
				success &= mSources[i].OnUnmount();
			return success;
		}

		/// <summary>
		/// Called when a file handle is requested
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="access">The type of access required</param>
		/// <param name="share">The kind of access other filestreams can have</param>
		/// <param name="mode">The mode to open the file in</param>
		/// <param name="options">Advanced options for creating a FileStream</param>
		/// <param name="attributes">The attributes of the file</param>
		/// <param name="info">An LVFSinfo containing the context for the file handle and information on the file</param>
		/// <returns>An NtStatus explaining the success level of the operation. If mode is OpenOrCreate and Create, and the operation is successful opening an existing file, DokanResult.AlreadyExists is returned.</returns>
		public NtStatus CreateFileHandle(string path, DokanNet.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, System.IO.FileAttributes attributes, LVFSInfo info)
		{
			return mSources.Last<Source>().CreateFileHandle(path, access, share, mode, options, attributes, info);
		}
	}
}
