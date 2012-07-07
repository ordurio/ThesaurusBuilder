using System;
using System.IO;
using System.Collections.Generic;

namespace Thesaurus
{
	public class Utils
	{
		public Utils ()
		{
		}
		
		public static HashSet<string> ReadTerms(string path)
		{
			HashSet<string> output = new HashSet<string>();
			foreach(string line in File.ReadAllLines(path))
			{
				foreach( string token in line.Split(' '))
					output.Add(token.Trim());
			}
			return output;
		}
		
		public static void SaveTerms(IEnumerable<string> terms, string path)
		{
			StreamWriter sw = new StreamWriter(path);
			foreach(var t in terms)
				sw.WriteLine(t);
			sw.Close();
		}
	}
}

