using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.Filesystem
{
	class LVFSContextInfo
	{
		DokanFileInfo mParent;

		public IDictionary<Sources.Source, object> Context { get; private set; }
		
		public bool DeleteOnClose
		{
			get { return mParent.DeleteOnClose; }
			set { mParent.DeleteOnClose = value; }
		}
		public bool IsDirectory
		{
			get { return mParent.IsDirectory; }
			set { mParent.IsDirectory = value; }
		}

		public bool NoCache { get; private set; }
		public bool PagingIo { get; private set; }
		public int ProcessId { get; private set; }
		public bool SynchronousIo { get; private set; }
		public bool WriteToEndOfFile { get; private set; }

		public Func<WindowsIdentity> GetRequestor { get; private set; }
		public Func<int, bool> TryResetTimeout { get; private set; }

		public LVFSContextInfo(DokanFileInfo info)
		{
			mParent = info;

			Context = (IDictionary<Sources.Source, object>)info.Context;
			if (Context == null)
			{
				Context = new Dictionary<Sources.Source, object>();
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
