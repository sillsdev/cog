using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;
using SIL.Collections;

namespace SIL.Cog.Applications.Services
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
			Sense = wp.Sense.Gloss;
			Word1 = wp.Word1.StrRep;
			Word2 = wp.Word2.StrRep;
			_alignmentNotes = wp.AlignmentNotes.ToList();
			AreCognatePredicted = wp.AreCognatePredicted;
			PhoneticSimilarityScore = wp.PhoneticSimilarityScore;
		}

		[ProtoMember(1)]
		public string Sense { get; set; }
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
		public bool AreCognatePredicted { get; set; }
		[ProtoMember(6)]
		public double PhoneticSimilarityScore { get; set; }

		public WordPair ToWordPair(CogProject project, VarietyPair vp)
		{
			Sense sense = project.Senses[Sense];
			Word word1 = vp.Variety1.Words[sense].First(w => w.StrRep == Word1);
			Word word2 = vp.Variety2.Words[sense].First(w => w.StrRep == Word2);
			var wp = new WordPair(word1, word2)
				{
					AreCognatePredicted = AreCognatePredicted,
					PhoneticSimilarityScore = PhoneticSimilarityScore
				};
			wp.AlignmentNotes.AddRange(_alignmentNotes);
			return wp;
		}
	}
}
