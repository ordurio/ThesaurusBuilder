using System;
using System.Text;
using System.Web;

namespace Thesaurus
{
	public class GUI
	{
		private ThesaurusExpander _thesaurusExpander;
		private string _thesaurusExpanderPath;
		public static string NEED_SEED_MESSAGE = "gimme seeds man!";
		
		#region constructor
		public GUI (string thesaurusExpanderPath)
		{
			_thesaurusExpanderPath = thesaurusExpanderPath;
			_thesaurusExpander = new ThesaurusExpander();
			// load or create thesaurusExpander
			ThesaurusExpander.Load(thesaurusExpanderPath, out _thesaurusExpander);
		}
		#endregion
		
		#region main processing methods defining different commands
		public void Start()
		{
			string userResp = "";
			string message = NEED_SEED_MESSAGE;
	
			while(userResp != "q")
			{
				GetNextMessage(userResp.Trim(), ref message);
				Console.WriteLine(message);
				userResp = Console.ReadLine();
			}
		}
		
		/// <summary>
		/// Gets the next message taking input user response into account.
		/// </summary>
		/// <returns>
		/// The next message.
		/// </returns>
		/// <param name='userResp'>
		/// User resp.
		/// </param>
		private void GetNextMessage(string userResp, ref string message)
		{
			string newCandidate;
			double score;
			switch (userResp)
			{
			case "": // confirm candidate
				TryConfirmCandidate();
				score = ProposeNewCandidate(out newCandidate);
				message = HttpUtility.UrlDecode(newCandidate) + "\t" + score;
				break;
			case "n": // reject candidate
				TryRejectCandidate();
				score = ProposeNewCandidate(out newCandidate);
				message = HttpUtility.UrlDecode(newCandidate) + "\t" + score;
				break;
			case "s":
				_thesaurusExpander.Save(_thesaurusExpanderPath);
				break;
			case "us":
				_thesaurusExpander.InitScores();
				_thesaurusExpander.UpdateScores();
				break;
			default: // new seed to add
				Console.WriteLine("toto");
				var seeds = userResp.Split(new char[] {' '});
				foreach(var seed in seeds)
					_thesaurusExpander.AddSeed(seed);
				if (message != NEED_SEED_MESSAGE)
				{
					Console.WriteLine("tintin");
				}
				else
				{
					score = ProposeNewCandidate(out newCandidate);
					Console.WriteLine("new candidate: " + newCandidate);
					message = HttpUtility.UrlDecode(newCandidate) + "\t" + score;
				}
				break;
			}
		}
		
		private void TryConfirmCandidate()
		{
			// confirm candidate if there was a pending candidate
			if (!string.IsNullOrEmpty(_thesaurusExpander.PendingCandidate))
			{
				_thesaurusExpander.ConfirmCandidate();
			}
		}
		
		private void TryRejectCandidate()
		{
			// confirm candidate if there was a pending candidate
			if (_thesaurusExpander.PendingCandidate != "")
			{
				_thesaurusExpander.RejectCandidate();
			}
		}

		private double ProposeNewCandidate(out string newCandidate)
		{
			var score = _thesaurusExpander.GetCandidate(out newCandidate);
			if (newCandidate != "")
				 return score;
			else //if no more candidate can be generated, only new seeds given by the user could do
			{
				newCandidate = NEED_SEED_MESSAGE;
				return -1.0;
			}
		}
		#endregion
	}
}

