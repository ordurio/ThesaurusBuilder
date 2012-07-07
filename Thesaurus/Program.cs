using System;

namespace Thesaurus
{
	public class Program
	{
		public Program ()
		{
		}
		
		public static void Main(string[] args)
		{
			// build GUI
			GUI gui = new GUI(args[0]);
			gui.Start();
		}
	}
}

