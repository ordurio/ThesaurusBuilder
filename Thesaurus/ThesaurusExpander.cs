using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Thesaurus
{
	public class ThesaurusExpander
	{
		
		#region static constants
		static string TERMS_FILENAME = "terms.txt";
		static string SEED_TERMS_FILENAME = "seed_terms.txt";
		static string CANDIDATE_TERMS_FILENAME = "candidate_terms.txt";
		static string ANTI_TERMS_FILENAME = "anti_terms.txt";
		static string TERM_FRIENDS_FILENAME = "term_friends.txt";
		static string TERM_SCORES_FILENAME = "term_scores.txt";
		#endregion
		
		#region main thesaurus components
		public HashSet<string> Terms {get; set;} // the thesaurus à proprement parlé
		public HashSet<string> AntiTerms {get; set;} // terms excluded from thesaurus
		public HashSet<string> CandidateTerms {get; set;} // terms candidate for being in the thesaurus
		public Queue<string> SeedTerms {get; set;} // all terms whose friends will be fetched in pushed into candidates
		public string PendingCandidate {get; set;} // each time a candidate is dequeued from candidate, it remains pending until caller's feedback
		#endregion
		
		#region internal useful members
		private WikiCandidateGenerator _candidateGenerator;
		private HashSet<string> _alreadySeen;
		private Dictionary<string, HashSet<string>> _termFriends;
		private Dictionary<string,double> _termScores;
		#endregion
		
		#region constructors
		public ThesaurusExpander(Queue<string> seedTerms)
		{
			Terms = new HashSet<string>();
			AntiTerms = new HashSet<string>();
			CandidateTerms = new HashSet<string>();
			SeedTerms = seedTerms;
			_candidateGenerator = new WikiCandidateGenerator();
			_alreadySeen = new HashSet<string>(seedTerms);
			_termScores = new Dictionary<string, double>();
			_termFriends = new Dictionary<string, HashSet<string>>();
		}

		public ThesaurusExpander()
			:this(new Queue<string>()) {}
		#endregion
		
		#region thesaurus component modifiers (add/remove)
		private void AddTerm(string term)
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
			_termScores[antiTerm] = 0.0;
		}
		
		public void AddSeed(string seed)
		{
			_termScores[seed] = 1.0;
			AddTerm(seed);
			SeedTerms.Enqueue(seed);
			GenerateCandidates(SeedTerms.Dequeue());
		}
		
		
		private string ComputeBestCandidate()
		{
			InitScores();
			UpdateScores();
			string bestCandidate = "";
			double bestScore = -1.0;
			foreach(var c in CandidateTerms)
			{
				if (_termScores.ContainsKey(c) && _termScores[c] > bestScore)
				{
					bestScore = _termScores[c];
					bestCandidate = c;
				}
			}
			return bestCandidate;
		}
		
		public void InitScores()
		{
			foreach(var t in Terms)
				_termScores[t] = 1.0;
			foreach(var at in AntiTerms)
				_termScores[at] = 0.0;
			foreach(var ct in CandidateTerms)
				_termScores[ct] = 0.0;
		}
		
		
		public void UpdateScore(string term)
		{
			if (!_termScores.ContainsKey(term))
				_termScores[term] = 0.0;

			if (!_termFriends.ContainsKey(term) || _termFriends[term] == null )
				return;
			
			foreach(var f in _termFriends[term])
			{
				if (_termScores.ContainsKey(f))
					_termScores[term] += _termScores[f];
			}
		}
		
		public void UpdateScores(IEnumerable<string> terms)
		{
			foreach(var term in terms)
			{
				UpdateScore(term);
			}
		}
		
		public void UpdateScores()
		{
			UpdateScores(Terms);
			UpdateScores(CandidateTerms);
		}
		
		public double GetCandidate(out string candidate)
		{
			if (string.IsNullOrEmpty(PendingCandidate))
			{
				try
				{
				    PendingCandidate = ComputeBestCandidate();
				}
				catch
				{
					// just nothing
				}
			}
			CandidateTerms.Remove(PendingCandidate);
			candidate = PendingCandidate;
			return _termScores.ContainsKey(candidate) ? _termScores[candidate] : 0.0;
		}
		
		public void ConfirmCandidate()
		{
			if(!string.IsNullOrEmpty(PendingCandidate))
				AddSeed(PendingCandidate);
			_termScores[PendingCandidate] = 1.0;
			PendingCandidate = string.Empty;
		}
		
		public void RejectCandidate()
		{
			if(!string.IsNullOrEmpty(PendingCandidate))
			{
				AntiTerms.Add(PendingCandidate);
				Terms.Remove(PendingCandidate);
				_termScores[PendingCandidate] = 0.0;
			}
			PendingCandidate = string.Empty;
		}
		
		
		private HashSet<string> GetFriends(string term)
		{
			if (_termFriends.ContainsKey(term))
				return _termFriends[term];
			
			HashSet<string> friends;
			_candidateGenerator.GenerateCandidates(term, out friends);
			return friends;
		}
		
		public bool GenerateCandidates(string seed)
		{
			if (string.IsNullOrEmpty(seed))
				return false;
			bool bOut = false;
			_termFriends[seed] = GetFriends(seed);
			if (_termFriends[seed] == null || _termFriends[seed].Count == 0)
				return false;
			foreach(var nc in _termFriends[seed])
			{
				if (!_alreadySeen.Contains(nc))
				{
					CandidateTerms.Add(nc);
					_alreadySeen.Add(nc);
					bOut = true;
				}
				// compute friends of new candidate and store them in _termFriends, in order to score candidate
				_termFriends[nc] = GetFriends(nc);
			}
			return bOut;
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
				thesaurusExpander.CandidateTerms = Utils.ReadTerms(candidatePath);;
	
			string termPath = thesaurusExpanderPath + "/" + TERMS_FILENAME;
			if(File.Exists(termPath))
				thesaurusExpander.Terms = Utils.ReadTerms(termPath);
			
			string scoresPath = thesaurusExpanderPath + "/" + TERM_SCORES_FILENAME;
			if(File.Exists(scoresPath))
				thesaurusExpander._termScores = ReadScores(scoresPath);

			string termFriendsPath = thesaurusExpanderPath + "/" + TERM_FRIENDS_FILENAME;
			if(File.Exists(termFriendsPath))
				thesaurusExpander._termFriends = ReadFriends(termFriendsPath);

			return true;
		}
		
		public void Save(string path)
		{
			Directory.CreateDirectory(path);
			Utils.SaveTerms(SeedTerms, path +"/" + SEED_TERMS_FILENAME);
			Utils.SaveTerms(AntiTerms, path +"/" + ANTI_TERMS_FILENAME);
			Utils.SaveTerms(CandidateTerms, path +"/" + CANDIDATE_TERMS_FILENAME);
			Utils.SaveTerms(Terms, path +"/" + TERMS_FILENAME);
			SaveScores(path + "/" + TERM_SCORES_FILENAME);
			SaveTermFriends(path + "/" + TERM_FRIENDS_FILENAME);
		}
		
		private void SaveScores(string path)
		{
			StreamWriter sw = new StreamWriter(path);
			foreach(var ts in _termScores)
				sw.WriteLine("{0}\t{1}", ts.Key, ts.Value);
			sw.Close();
		}
			
		private void SaveTermFriends(string path)
		{
			StreamWriter sw = new StreamWriter(path);
			foreach(var tf in _termFriends)
			{
				if (tf.Key == null || tf.Value == null)
					continue;
				sw.Write(tf.Key);
				foreach(var f in tf.Value)
					sw.Write("\t" + f.Trim());
				sw.WriteLine();
			}
			sw.Close();
		}
		
		public static Dictionary<string, double> ReadScores(string path)
		{
			Dictionary<string, double> output = new Dictionary<string, double>();
			foreach(string line in File.ReadAllLines(path, Encoding.UTF8))
			{
				var termScore = line.Split('\t');
				if (termScore.Length < 2)
					continue;
				output[termScore[0].Trim()] = Convert.ToDouble(termScore[1]);
			}
			return output;
		}
	
		public static Dictionary<string, HashSet<string>> ReadFriends(string path)
		{
			Dictionary<string, HashSet<string>> output = new Dictionary<string, HashSet<string>>();
			foreach(string line in File.ReadAllLines(path, Encoding.UTF8))
			{
				var termFriends = line.Split('\t');
				if (termFriends.Length < 2)
					continue;
				var term = termFriends[0].Trim();
				output[term] = new HashSet<string>();
				for(int k=1; k < termFriends.Length; ++k)
					output[term].Add(termFriends[k].Trim());
			}
			return output;
		}
		
		#endregion
	}
}

