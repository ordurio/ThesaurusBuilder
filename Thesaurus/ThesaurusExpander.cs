using System;
using System.Collections.Generic;
using System.IO;

namespace Thesaurus
{
	public class ThesaurusExpander
	{
		
		#region static constants
		static string TERMS_FILENAME = "terms.csv";
		static string SEED_TERMS_FILENAME = "seed_terms.csv";
		static string CANDIDATE_TERMS_FILENAME = "candidate_terms.csv";
		static string ANTI_TERMS_FILENAME = "anti_terms.csv";
		#endregion
		
		#region main thesaurus components
		public HashSet<string> Terms {get; set;} // the thesaurus à proprement parlé
		public HashSet<string> AntiTerms {get; set;} // terms excluded from thesaurus
		public Queue<string> CandidateTerms {get; set;} // terms candidate for being in the thesaurus
		public Queue<string> SeedTerms {get; set;} // all terms whose friends will be fetched in pushed into candidates
		public string PendingCandidate {get; set;} // each time a candidate is dequeued from candidate, it remains pending until caller's feedback
		#endregion
		
		#region internal useful members
		private WikiCandidateGenerator _candidateGenerator;
		private HashSet<string> _alreadySeen;
		#endregion
		
		#region constructors
		public ThesaurusExpander(Queue<string> seedTerms)
		{
			Terms = new HashSet<string>();
			AntiTerms = new HashSet<string>();
			CandidateTerms = new Queue<string>();
			SeedTerms = seedTerms;
			_candidateGenerator = new WikiCandidateGenerator();
			_alreadySeen = new HashSet<string>(seedTerms);
		}

		public ThesaurusExpander()
			:this(new Queue<string>()) {}
		#endregion
		
		#region thesaurus component modifiers (add/remove)
		public void AddTerm(string term)
		{
			AntiTerms.Remove(term);
			Terms.Add(term);
			_alreadySeen.Add(term);
		}
		
		public void AddAntiTerm(string antiTerm)
		{
			Terms.Remove(antiTerm);
			AntiTerms.Add(antiTerm);
			_alreadySeen.Add(antiTerm);
		}
		
		public void AddSeed(string seed)
		{
			AddTerm(seed);
			SeedTerms.Enqueue(seed);
		}

		public string GetCandidate()
		{
			if (string.IsNullOrEmpty(PendingCandidate))
			{
				try
				{
				    PendingCandidate = CandidateTerms.Dequeue();
				}
				catch
				{
					// just nothing
				}
			}
			return PendingCandidate;
		}
		
		public void ConfirmCandidate()
		{
			if(!string.IsNullOrEmpty(PendingCandidate))
				AddSeed(PendingCandidate);
			PendingCandidate = string.Empty;
		}
		
		public void RejectCandidate()
		{
			if(!string.IsNullOrEmpty(PendingCandidate))
			{
				AntiTerms.Add(PendingCandidate);
				Terms.Remove(PendingCandidate);
			}
			PendingCandidate = string.Empty;
		}
		
		public bool GenerateCandidates(string seed)
		{
			if (string.IsNullOrEmpty(seed))
				return false;
			bool bOut = false;
			HashSet<string> newCandidates;
			if( _candidateGenerator.GenerateCandidates(seed, out newCandidates) )
			{
				foreach(var nc in newCandidates)
				{
					if (!_alreadySeen.Contains(nc))
					{
						CandidateTerms.Enqueue(nc);
						_alreadySeen.Add(nc);
						bOut = true;
					}
				}
			}
			return bOut;
		}
		
		public bool GenerateCandidates()
		{
			while(CandidateTerms.Count < 1 && SeedTerms.Count > 0)
			{
				if ( GenerateCandidates(SeedTerms.Dequeue()) )
					return true;
			}
			return false;
		}
		#endregion
		
		#region thesaurus serialization		
		public static bool TryLoad(string thesaurusExpanderPath, out ThesaurusExpander thesaurusExpander)
		{
			thesaurusExpander = null;
			// main condition for a load to happen: the availability of seed_terms.csv
			string seedPath = thesaurusExpanderPath + "/" + SEED_TERMS_FILENAME;
			if(!File.Exists(seedPath))
				return false;
			thesaurusExpander = new ThesaurusExpander();
			foreach(string term in Utils.ReadTerms(seedPath))
				thesaurusExpander.SeedTerms.Enqueue(term);
			
			// fill other components if available
			string antiPath = thesaurusExpanderPath + "/" + ANTI_TERMS_FILENAME;
			if(File.Exists(antiPath))
				thesaurusExpander.AntiTerms = Utils.ReadTerms(antiPath);

			string candidatePath = thesaurusExpanderPath + "/" + CANDIDATE_TERMS_FILENAME;
			if(File.Exists(candidatePath))
			{
				foreach(var c in Utils.ReadTerms(candidatePath))
					thesaurusExpander.CandidateTerms.Enqueue(c);
			}
	
			string termPath = thesaurusExpanderPath + "/" + TERMS_FILENAME;
			if(File.Exists(termPath))
				thesaurusExpander.Terms = Utils.ReadTerms(termPath);
			
			return true;
		}
		
		public void Save(string path)
		{
			Directory.CreateDirectory(path);
			Utils.SaveTerms(SeedTerms, path +"/" + SEED_TERMS_FILENAME);
			Utils.SaveTerms(AntiTerms, path +"/" + ANTI_TERMS_FILENAME);
			Utils.SaveTerms(CandidateTerms, path +"/" + CANDIDATE_TERMS_FILENAME);
			Utils.SaveTerms(Terms, path +"/" + TERMS_FILENAME);
		}
		#endregion
	}
}

