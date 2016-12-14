using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LVFS.Sources;

namespace LVFS.Filesystem
{
	class Selector
	{
		private IList<Source> mSources;

		public Selector(IList<Source> sources)
		{
			mSources = new List<Source>(sources);
		}

		public Selector()
		{
			mSources = new List<Source>();
		}

		public Source SourceOf(string fileName)
		{
			for (int i = mSources.Count - 1; i >= 0; i--)
			{
				if (mSources[i].HasFile(fileName))
					return mSources[i];
			}
			return null;
		}
	}
}
