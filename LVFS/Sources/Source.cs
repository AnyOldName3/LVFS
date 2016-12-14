using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVFS.Sources
{
	abstract class Source
	{
		public abstract bool HasFile(string fileName);
	}
}
