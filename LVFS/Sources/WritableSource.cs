using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVFS.Sources
{
	abstract class WritableSource : Source
	{
		protected WritableSource(Source predecessor) : base(predecessor)
		{

		}
	}
}
