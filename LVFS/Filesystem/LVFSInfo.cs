using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using DokanNet;

namespace LVFS.Filesystem
{
	class LVFSInfo
	{
		public IDictionary<Sources.Source, object> Context { get; private set; }
		public bool DeleteOnClose { get; private set; }
		public bool IsDirectory { get; private set; }
		public bool NoCache { get; private set; }
		public bool PagingIo { get; private set; }
		public int ProcessId { get; private set; }
		public bool SynchronousIo { get; private set; }
		public bool WriteToEndOfFile { get; private set; }

		public Func<WindowsIdentity> GetRequestor { get; private set; }
		public Func<int, bool> TryResetTimeout { get; private set; }

		public LVFSInfo(DokanFileInfo info)
		{
			Context = (IDictionary<Sources.Source, object>)info.Context;
			DeleteOnClose = info.DeleteOnClose;
			IsDirectory = info.IsDirectory;
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
