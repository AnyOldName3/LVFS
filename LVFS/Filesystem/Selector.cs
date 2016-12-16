using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public void addSource(Source source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			mSources.Add(source);
			mOutputSource = source;
		}

        /// <summary>
        /// Gets the source responsible for a file/directory
        /// </summary>
        /// <param name="fileName">The path to the file/directory</param>
        /// <returns>The Source responsible for the file/directory</returns>
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
