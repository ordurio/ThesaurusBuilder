using System;
using System.Collections.Generic;

namespace Thesaurus
{
	public interface ICandidateGenerator
	{
		bool GenerateCandidates(string seed, out HashSet<string> newCandidates);
	}
}

