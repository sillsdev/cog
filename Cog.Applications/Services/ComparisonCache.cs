using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	[ProtoContract]
	internal class ComparisonCache
	{
		private readonly List<VarietyPairSurrogate> _varietyPairs;
		private readonly Dictionary<SyllablePosition, List<GlobalSoundCorrespondenceSurrogate>> _globalSoundCorrespondenceCollections;
		
		public ComparisonCache()
		{
			_varietyPairs = new List<VarietyPairSurrogate>();
			_globalSoundCorrespondenceCollections = new Dictionary<SyllablePosition, List<GlobalSoundCorrespondenceSurrogate>>();
		}

		public ComparisonCache(CogProject project)
		{
			var wordPairSurrogates = new Dictionary<WordPair, WordPairSurrogate>();
			_varietyPairs = project.VarietyPairs.Select(vp => new VarietyPairSurrogate(wordPairSurrogates, vp)).ToList();
			_globalSoundCorrespondenceCollections = new Dictionary<SyllablePosition, List<GlobalSoundCorrespondenceSurrogate>>();
			foreach (KeyValuePair<SyllablePosition, GlobalSoundCorrespondenceCollection> kvp in project.GlobalSoundCorrespondenceCollections)
				_globalSoundCorrespondenceCollections[kvp.Key] = kvp.Value.Select(corr => new GlobalSoundCorrespondenceSurrogate(wordPairSurrogates, corr)).ToList();
		}
			
		[ProtoMember(1)]
		public List<VarietyPairSurrogate> VarietyPairs
		{
			get { return _varietyPairs; }
		}

		[ProtoMember(2)]
		public Dictionary<SyllablePosition, List<GlobalSoundCorrespondenceSurrogate>> GlobalSoundCorrespondenceCollections
		{
			get { return _globalSoundCorrespondenceCollections; }
		}

		public void Load(SegmentPool segmentPool, CogProject project)
		{
			var wordPairs = new Dictionary<WordPairSurrogate, WordPair>();
			project.VarietyPairs.AddRange(_varietyPairs.Select(vp => vp.ToVarietyPair(segmentPool, wordPairs, project)));
			foreach (KeyValuePair<SyllablePosition, List<GlobalSoundCorrespondenceSurrogate>> kvp in _globalSoundCorrespondenceCollections)
			{
				if (kvp.Value != null)
					project.GlobalSoundCorrespondenceCollections[kvp.Key].AddRange(kvp.Value.Select(surrogate => surrogate.ToGlobalSoundCorrespondence(segmentPool, wordPairs)));
			}
		}
	}
}
