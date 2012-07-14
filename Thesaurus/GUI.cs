using System;
using System.Text;
using System.Web;

namespace Thesaurus
{
	public class GUI
	{
		private ThesaurusExpander _thesaurusExpander;
		private string _thesaurusExpanderPath;
		private static string NEED_SEED_MESSAGE = "gimme seeds man!";
		
		#region constructor
		public GUI (string thesaurusExpanderPath)
		{
			_thesaurusExpanderPath = thesaurusExpanderPath;
			// load or create thesaurusExpander
			if (!ThesaurusExpander.TryLoad(thesaurusExpanderPath, out _thesaurusExpander))
				_thesaurusExpander = new ThesaurusExpander();
		}
		#endregion
		
		#region main processing methods defining different commands
		public void Start()
		{
			string userResp = "";
			string message = "";
			while(userResp != "q")
			{
				message = GetNextMessage(userResp, message);
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
		private string GetNextMessage(string userResp, string previousMessage)
		{
			switch (userResp)
			{
			case "": // confirm candidate
				TryConfirmCandidate();
				string newCandidate;
				double score = ProposeNewCandidate(out newCandidate);
				return HttpUtility.UrlDecode(newCandidate) + "\t" + score;
				break;
			case "n": // reject candidate
				TryRejectCandidate();
				score = ProposeNewCandidate(out newCandidate);
				return HttpUtility.UrlDecode(newCandidate) + "\t" + score;
				break;
			case "s":
				_thesaurusExpander.Save(_thesaurusExpanderPath);
				return previousMessage;
				break;
			case "us":
				_thesaurusExpander.InitScores();
				_thesaurusExpander.UpdateScores();
				return previousMessage;
				break;
			default: // new seed to add
				var seeds = userResp.Split(new char[] {' '});
				foreach(var seed in seeds)
					_thesaurusExpander.AddSeed(seed);
				if (previousMessage != NEED_SEED_MESSAGE)
					return previousMessage;
				else
				{
					score = ProposeNewCandidate(out newCandidate);
					return HttpUtility.UrlDecode(newCandidate) + "\t" + score;
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

