using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;


namespace WikiCrawler
{
	public class WikiFetcher
	{
		public Uri BaseUri {get; set;}
		public Regex ContentRE {get; set;}
		public Regex HyperlinkRE {get; set;}
		public Regex[] NonHyperlinkREs {get; set;}
	
		public WikiFetcher (string country = "fr")
			:this("http://"+country+".wikipedia.org",
			      @"<p>(?<chunk>.*?)</p>",
			      @"<a href=""/wiki/(?<link>.*?)"" ",
			      new string[] {"<span (?<kickout>.*?)</span>", "<small (?<kickout>.*?)</small>", "<div (?<kickout>.*?)</div>", "<sup (?<kickout>.*?)</sup>"})
		{
		}
		
		public WikiFetcher(string baseUri, string contentPattern, string hyperlinkPattern, string[] nonHyperlinkPatterns)
		{
			BaseUri = new Uri(baseUri);
			ContentRE = new Regex(contentPattern, RegexOptions.Compiled);
			HyperlinkRE = new Regex(hyperlinkPattern, RegexOptions.Compiled);
			NonHyperlinkREs = new Regex[nonHyperlinkPatterns.Length];
			for(int k = 0; k < nonHyperlinkPatterns.Length; ++k)
				NonHyperlinkREs[k] = new Regex(nonHyperlinkPatterns[k], RegexOptions.Compiled);
		}
		
		
		
		public string FetchContent(string term)
		{
			Uri url = new Uri(BaseUri, string.Format("/wiki/{0}",term.ToLower().Replace(" ","_")));
			HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
			myRequest.UserAgent = "chrome";
			myRequest.Method = "GET";
			WebResponse myResponse;
			try
			{
				myResponse = myRequest.GetResponse();
				Thread.Sleep(100);
			}
			catch (WebException e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
			StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
			string result = sr.ReadToEnd();
			sr.Close();
			myResponse.Close();
			return result;
		}
		
		public string ParseContent(string content)
		{
			string result = "";
			var matches = ContentRE.Matches(content);
			foreach(Match m in matches)
			{
				result += m.Groups["chunk"].Value;
				result += " ";
			}
			return result;
		}
		
		
		public string RemoveNonHyperlinks(string content)
		{
			string filteredContent = content;
			foreach(Regex re in NonHyperlinkREs)
			{
				filteredContent = re.Replace(filteredContent, " ");
			}
			return filteredContent;
		}
		
		
		public HashSet<string> ExtractLinks(string content)
		{
			string filteredContent = RemoveNonHyperlinks(content);
			HashSet<string> links = new HashSet<string>();
			var matches = HyperlinkRE.Matches(filteredContent);
			foreach(Match m in matches)
				links.Add(m.Groups["link"].Value);
			return links;
		}
		
		public HashSet<string> GetFriends(string term)
		{
			string content = FetchContent(term);
			if (content == null)
				return null;
			string parsedContent = ParseContent(content);
			HashSet<string> friends = ExtractLinks(parsedContent);
			return friends;
		}
		
		public Dictionary<string,HashSet<string>> GetFriends(IEnumerable<string> terms)
		{
			Dictionary<string,HashSet<string>> output = new Dictionary<string, HashSet<string>>();
			foreach(string term in terms)
				output[term] = GetFriends(term);
			return output;
		}
	}
}

