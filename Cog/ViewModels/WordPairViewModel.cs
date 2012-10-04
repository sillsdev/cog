using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using SIL.Machine;

namespace SIL.Cog.ViewModels
{
	public class WordPairViewModel : ViewModelBase
	{
		private readonly CogProject _project;
		private readonly WordPair _wordPair;

		private readonly ReadOnlyCollection<AlignedNodeViewModel> _alignedNodes;
		private readonly SenseViewModel _sense;

		public WordPairViewModel(CogProject project, WordPair wordPair)
		{
			_project = project;
			_wordPair = wordPair;
			_sense = new SenseViewModel(_wordPair.Word1.Sense);

			IAligner aligner = _project.Aligners["primary"];
			IAlignerResult results = aligner.Compute(_wordPair);
			Alignment alignment = results.GetAlignments().First();
			var nodes = new List<AlignedNodeViewModel>();
			nodes.Add(new AlignedNodeViewModel(GetString(alignment.Prefix1), GetString(alignment.Prefix2)));
			nodes.Add(new AlignedNodeViewModel("|", "|"));
			int i = 0;
			foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> a in alignment.AlignedAnnotations)
			{
				string note = null;
				if (i < _wordPair.AlignmentNotes.Count)
					note = _wordPair.AlignmentNotes[i];
				nodes.Add(new AlignedNodeViewModel((string) a.Item1.FeatureStruct.GetValue(CogFeatureSystem.StrRep), (string) a.Item2.FeatureStruct.GetValue(CogFeatureSystem.StrRep), note));
				i++;
			}
			nodes.Add(new AlignedNodeViewModel("|", "|"));
			nodes.Add(new AlignedNodeViewModel(GetString(alignment.Suffix1), GetString(alignment.Suffix2)));

			_alignedNodes = new ReadOnlyCollection<AlignedNodeViewModel>(nodes);
		}

		private static string GetString(IEnumerable<Annotation<ShapeNode>> anns)
		{
			return string.Concat(anns.Select(ann => (string) ann.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
		}

		public SenseViewModel Sense
		{
			get { return _sense; }
		}

		public ReadOnlyCollection<AlignedNodeViewModel> AlignedNodes
		{
			get { return _alignedNodes; }
		}

		public double PhoneticSimilarityScore
		{
			get { return _wordPair.PhoneticSimilarityScore; }
		}

		public bool AreCognate
		{
			get { return _wordPair.AreCognatePredicted; }
		}
	}
}
