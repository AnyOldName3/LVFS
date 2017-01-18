using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.External
{
	/// <summary>
	/// A class giving access to information regarding the context of an operation.
	/// </summary>
	public class LVFSContextInfo
	{
		DokanFileInfo mParent;

		/// <summary>
		/// A dictionary holding arbitrary context data for each source. A source can store any or no data it wishes here, and it will be preserved for each file handle.
		/// </summary>
		public IDictionary<Source, object> Context { get; private set; }
		
		/// <summary>
		/// Holds whether the file should be deleted when it is closed.
		/// </summary>
		public bool DeleteOnClose
		{
			get { return mParent.DeleteOnClose; }
			set { mParent.DeleteOnClose = value; }
		}

		/// <summary>
		/// Holds whether the application software regards the file as a directory or regular file. If not done beforehand, this must be set in <see cref="Source.CreateFileHandle"/>
		/// </summary>
		public bool IsDirectory
		{
			get { return mParent.IsDirectory; }
			set { mParent.IsDirectory = value; }
		}

		/// <summary>
		/// True if the file should not be cached, i.e. all reads come directly from the backing store, and all writes are immediately written through.
		/// </summary>
		public bool NoCache { get; private set; }
		/// <summary>
		/// True if reads/writes are paging IO.
		/// </summary>
		public bool PagingIo { get; private set; }
		/// <summary>
		/// The process ID of the thread that originally requested the IO operation.
		/// </summary>
		public int ProcessId { get; private set; }
		/// <summary>
		/// True if the operation is synchronous IO.
		/// </summary>
		public bool SynchronousIo { get; private set; }
		/// <summary>
		/// True if the end of the file should be written to instead of the requested offset from the start.
		/// </summary>
		public bool WriteToEndOfFile { get; private set; }

		/// <summary>
		/// Gets the Windows user requesting the operation. According to the <see cref="DokanNet"/> documentation, this method needs to be called in <see cref="Source.CreateFileHandle(string, FileAccess, System.IO.FileShare, System.IO.FileMode, System.IO.FileOptions, System.IO.FileAttributes, LVFSContextInfo)"/>
		/// </summary>
		/// <remarks>
		/// Returns a <see cref="WindowsIdentity"/> with the access token, or <c>null</c> if the operation was not successful.
		/// </remarks>
		public Func<WindowsIdentity> GetRequestor { get; private set; }
		/// <summary>
		/// Extends the timeout period for the current operation.
		/// </summary>
		/// <remarks>
		/// <c>milliseconds</c>: The number of milliseconds to extend the timeout period by.
		/// Returns <c>true</c> if the operation was successful.
		/// </remarks>
		public Func<int, bool> TryResetTimeout { get; private set; }

		/// <summary>
		/// Constructs an LVFSContextInfo instance wrapping the specified Dokan version.
		/// </summary>
		/// <param name="info">The <see cref="DokanFileInfo"/> to wrap</param>
		internal LVFSContextInfo(DokanFileInfo info)
		{
			mParent = info;

			Context = info.Context as IDictionary<Source, object>;
			if (Context == null)
			{
				Context = new Dictionary<Source, object>();
				mParent.Context = Context;
			}

			NoCache = info.NoCache;
			PagingIo = info.PagingIo;
			ProcessId = info.ProcessId;
			SynchronousIo = info.SynchronousIo;
			WriteToEndOfFile = info.WriteToEndOfFile;

			GetRequestor = info.GetRequestor;
			TryResetTimeout = info.TryResetTimeout;
		}
	}
}
