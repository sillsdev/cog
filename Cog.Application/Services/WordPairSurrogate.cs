using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;
using SIL.Extensions;

namespace SIL.Cog.Application.Services
{
	[ProtoContract]
	internal class WordPairSurrogate
	{
		private readonly List<string> _alignmentNotes; 

		public WordPairSurrogate()
		{
			_alignmentNotes = new List<string>();
		}

		public WordPairSurrogate(WordPair wp)
		{
			Meaning = wp.Meaning.Gloss;
			Word1 = wp.Word1.StrRep;
			Word2 = wp.Word2.StrRep;
			_alignmentNotes = wp.AlignmentNotes.ToList();
			PredictedCognacy = wp.PredictedCognacy;
			PhoneticSimilarityScore = wp.PhoneticSimilarityScore;
			PredictedCognacyScore = wp.PredictedCognacyScore;
		}

		[ProtoMember(1)]
		public string Meaning { get; set; }
		[ProtoMember(2)]
		public string Word1 { get; set; }
		[ProtoMember(3)]
		public string Word2 { get; set; }

		[ProtoMember(4)]
		public List<string> AlignmentNotes
		{
			get { return _alignmentNotes; }
		}
		[ProtoMember(5)]
		public bool PredictedCognacy { get; set; }
		[ProtoMember(6)]
		public double PhoneticSimilarityScore { get; set; }
		[ProtoMember(7)]
		public double PredictedCognacyScore { get; set; }

		public WordPair ToWordPair(CogProject project, VarietyPair vp)
		{
			Meaning meaning = project.Meanings[Meaning];
			Word word1 = vp.Variety1.Words[meaning].First(w => w.StrRep == Word1);
			Word word2 = vp.Variety2.Words[meaning].First(w => w.StrRep == Word2);
			var wp = new WordPair(word1, word2)
				{
					PredictedCognacy = PredictedCognacy,
					PhoneticSimilarityScore = PhoneticSimilarityScore,
					PredictedCognacyScore = PredictedCognacyScore
				};
			wp.AlignmentNotes.AddRange(_alignmentNotes);
			return wp;
		}
	}
}
