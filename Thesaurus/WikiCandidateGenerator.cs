using System;
using WikiCrawler;
using System.Collections.Generic;

namespace Thesaurus
{
	public class WikiCandidateGenerator : ICandidateGenerator
	{
		private WikiFetcher _wikiFetcher;
		public WikiCandidateGenerator ()
		{
			 _wikiFetcher = new WikiFetcher();
		}
		
		public bool GenerateCandidates(string seed, out HashSet<string> newCandidates)
		{
			newCandidates = _wikiFetcher.GetFriends(seed);
			return (newCandidates != null && newCandidates.Count > 0);
		}
	}
}

