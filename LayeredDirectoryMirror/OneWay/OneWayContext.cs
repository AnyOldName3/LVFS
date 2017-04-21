using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayeredDirectoryMirror.OneWay
{
	class OneWayContext
	{
		public bool OneWayControls { get; set; }

		#region HandleCreationParameters
		public string CreationPath { get; set; }
		public DokanNet.FileAccess CreationAccess { get; set; }
		public FileShare CreationShare { get; set; }
		public FileMode CreationMode { get; set; }
		public FileOptions CreationOptions { get; set; }
		public FileAttributes CreationAttributes { get; set; }
		#endregion HandleCreationParameters

		public object Context { get; set; }

		public OneWayContext(string creationPath, DokanNet.FileAccess creationAccess, FileShare creationShare, FileMode creationMode, FileOptions creationOptions, FileAttributes creationAttributes)
		{
			CreationPath = creationPath;
			CreationAccess = creationAccess;
			CreationShare = creationShare;
			CreationMode = creationMode;
			CreationOptions = creationOptions;
			CreationAttributes = creationAttributes;

			OneWayControls = false;
		}
	}
}
