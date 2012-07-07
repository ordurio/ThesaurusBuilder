using System;
using System.IO;
using System.Text;

namespace WikiCrawler
{
	class MainClass
	{
		public static void Main (string[] args)
		{
/*			WikiFetcher wf = new WikiFetcher();
			string term;
			while((term = Console.ReadLine()) != "stop")
			{
				var friends = wf.GetFriends(term);
				if (friends == null)
					continue;
				foreach(string s in friends)
					Console.WriteLine(s);
				Console.WriteLine("*********************************************");
			}
*/
/*			Thesaurus thesaurus = new Thesaurus();
			string term;
			Console.WriteLine("please enter pilot terms (one by line, then stop)");
			while((term = Console.ReadLine()) != "")
			{
				thesaurus.AddTerm(term);
			}
			
			Console.WriteLine("please enter pilot antiterms (one by line, then stop)");
			while((term = Console.ReadLine()) != "")
			{
				thesaurus.AddAntiTerm(term);
			}
			
			string stopouencore = "";
			while(stopouencore != "stop")
			{
				thesaurus.ExpandThesaurusManual();
				Console.WriteLine("*********** TERMS *************");
				foreach(string t in thesaurus.Terms)
					Console.WriteLine(t);
				Console.WriteLine("*********** ANTITERMS *************");
				foreach(string t in thesaurus.AntiTerms)
					Console.WriteLine(t);
				stopouencore = Console.ReadLine();
			}*/
		}
	}
}
