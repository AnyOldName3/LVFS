using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVFS
{
	class Test
	{
		static void SwitchThing(FileMode mode)
		{
			Console.WriteLine("Testing " + mode);
			switch (mode)
			{
				case FileMode.Open:
					Console.WriteLine("Open");
					break;
				
				case FileMode.CreateNew:
					Console.WriteLine("CreateNew");
					break;

				case FileMode.Truncate:
					Console.WriteLine("Truncate");
					break;
			}
			Console.WriteLine();
		}

		static void Main(string[] args)
		{
			SwitchThing(FileMode.Append);
			SwitchThing(FileMode.Create);
			SwitchThing(FileMode.CreateNew);
			SwitchThing(FileMode.Open);
			SwitchThing(FileMode.OpenOrCreate);
			SwitchThing(FileMode.Truncate);
		}
	}
}
