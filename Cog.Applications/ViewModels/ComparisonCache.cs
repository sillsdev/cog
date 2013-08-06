using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.ViewModels
{
	[ProtoContract]
	internal class ComparisonCache
	{
		private readonly List<VarietyPairSurrogate> _varietyPairs;
		private readonly List<GlobalSoundCorrespondenceSurrogate> _stemInitialConsCorrespondences;
		private readonly List<GlobalSoundCorrespondenceSurrogate> _stemMedialConsCorrespondences;
		private readonly List<GlobalSoundCorrespondenceSurrogate> _stemFinalConsCorrespondences;
		private readonly List<GlobalSoundCorrespondenceSurrogate> _onsetConsCorrespondences;
		private readonly List<GlobalSoundCorrespondenceSurrogate> _codaConsCorrespondences;
		private readonly List<GlobalSoundCorrespondenceSurrogate> _vowelCorrespondences;
		
		public ComparisonCache()
		{
			_varietyPairs = new List<VarietyPairSurrogate>();
			_stemInitialConsCorrespondences = new List<GlobalSoundCorrespondenceSurrogate>();
			_stemMedialConsCorrespondences = new List<GlobalSoundCorrespondenceSurrogate>();
			_stemFinalConsCorrespondences = new List<GlobalSoundCorrespondenceSurrogate>();
			_onsetConsCorrespondences = new List<GlobalSoundCorrespondenceSurrogate>();
			_codaConsCorrespondences = new List<GlobalSoundCorrespondenceSurrogate>();
			_vowelCorrespondences = new List<GlobalSoundCorrespondenceSurrogate>();
		}

		public ComparisonCache(CogProject project)
		{
			var wordPairSurrogates = new Dictionary<WordPair, WordPairSurrogate>();
			_varietyPairs = project.VarietyPairs.Select(vp => new VarietyPairSurrogate(wordPairSurrogates, vp)).ToList();
			_stemInitialConsCorrespondences = GetCorrespondenceSurrogates(wordPairSurrogates, project.StemInitialConsonantCorrespondences);
			_stemMedialConsCorrespondences = GetCorrespondenceSurrogates(wordPairSurrogates, project.StemMedialConsonantCorrespondences);
			_stemFinalConsCorrespondences = GetCorrespondenceSurrogates(wordPairSurrogates, project.StemFinalConsonantCorrespondences);
			_onsetConsCorrespondences = GetCorrespondenceSurrogates(wordPairSurrogates, project.OnsetConsonantCorrespondences);
			_codaConsCorrespondences = GetCorrespondenceSurrogates(wordPairSurrogates, project.CodaConsonantCorrespondences);
			_vowelCorrespondences = GetCorrespondenceSurrogates(wordPairSurrogates, project.VowelCorrespondences);
		}

		private List<GlobalSoundCorrespondenceSurrogate> GetCorrespondenceSurrogates(Dictionary<WordPair, WordPairSurrogate> wordPairSurrogates, IEnumerable<GlobalSoundCorrespondence> corrs)
		{
			return corrs.Select(corr => new GlobalSoundCorrespondenceSurrogate(wordPairSurrogates, corr)).ToList();
		}
			
		[ProtoMember(1)]
		public List<VarietyPairSurrogate> VarietyPairs
		{
			get { return _varietyPairs; }
		}

		[ProtoMember(2)]
		public List<GlobalSoundCorrespondenceSurrogate> StemInitialConsonantCorrespondences
		{
			get { return _stemInitialConsCorrespondences; }
		}
		[ProtoMember(3)]
		public List<GlobalSoundCorrespondenceSurrogate> StemMedialConsonantCorrespondences
		{
			get { return _stemMedialConsCorrespondences; }
		}
		[ProtoMember(4)]
		public List<GlobalSoundCorrespondenceSurrogate> StemFinalConsonantCorrespondences
		{
			get { return _stemFinalConsCorrespondences; }
		}
		[ProtoMember(5)]
		public List<GlobalSoundCorrespondenceSurrogate> OnsetConsonantCorrespondences
		{
			get { return _onsetConsCorrespondences; }
		}
		[ProtoMember(6)]
		public List<GlobalSoundCorrespondenceSurrogate> CodaConsonantCorrespondences
		{
			get { return _codaConsCorrespondences; }
		}
		[ProtoMember(7)]
		public List<GlobalSoundCorrespondenceSurrogate> VowelCorrespondences
		{
			get { return _vowelCorrespondences; }
		}

		public void Load(SegmentPool segmentPool, CogProject project)
		{
			var wordPairs = new Dictionary<WordPairSurrogate, WordPair>();
			project.VarietyPairs.AddRange(_varietyPairs.Select(vp => vp.ToVarietyPair(segmentPool, wordPairs, project)));
			LoadCorrespondences(segmentPool, wordPairs, _stemInitialConsCorrespondences, project.StemInitialConsonantCorrespondences);
			LoadCorrespondences(segmentPool, wordPairs, _stemMedialConsCorrespondences, project.StemMedialConsonantCorrespondences);
			LoadCorrespondences(segmentPool, wordPairs, _stemFinalConsCorrespondences, project.StemFinalConsonantCorrespondences);
			LoadCorrespondences(segmentPool, wordPairs, _onsetConsCorrespondences, project.OnsetConsonantCorrespondences);
			LoadCorrespondences(segmentPool, wordPairs, _codaConsCorrespondences, project.CodaConsonantCorrespondences);
			LoadCorrespondences(segmentPool, wordPairs, _vowelCorrespondences, project.VowelCorrespondences);
		}

		private void LoadCorrespondences(SegmentPool segmentPool, Dictionary<WordPairSurrogate, WordPair> wordPairs,
			IEnumerable<GlobalSoundCorrespondenceSurrogate> surrogates, GlobalSoundCorrespondenceCollection corrs)
		{
			corrs.AddRange(surrogates.Select(surrogate => surrogate.ToGlobalSoundCorrespondence(segmentPool, wordPairs)));
		}
	}
}
