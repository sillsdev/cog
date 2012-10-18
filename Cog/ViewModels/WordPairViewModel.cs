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

		private readonly AlignedNodeViewModel _prefixNode;
		private readonly ReadOnlyCollection<AlignedNodeViewModel> _alignedNodes;
		private readonly AlignedNodeViewModel _suffixNode;
		private readonly SenseViewModel _sense;

		public WordPairViewModel(CogProject project, WordPair wordPair)
		{
			_project = project;
			_wordPair = wordPair;
			_sense = new SenseViewModel(_wordPair.Word1.Sense);

			IAligner aligner = _project.Aligners["primary"];
			IAlignerResult results = aligner.Compute(_wordPair);
			Alignment alignment = results.GetAlignments().First();
			_prefixNode = new AlignedNodeViewModel(alignment.Prefix1, alignment.Prefix2);
			var nodes = new List<AlignedNodeViewModel>();
			int i = 0;
			foreach (Tuple<Annotation<ShapeNode>, Annotation<ShapeNode>> a in alignment.AlignedAnnotations)
			{
				string note = null;
				if (i < _wordPair.AlignmentNotes.Count)
					note = _wordPair.AlignmentNotes[i];
				nodes.Add(new AlignedNodeViewModel(a.Item1, a.Item2, note));
				i++;
			}
			_suffixNode = new AlignedNodeViewModel(alignment.Suffix1, alignment.Suffix2);

			_alignedNodes = new ReadOnlyCollection<AlignedNodeViewModel>(nodes);
		}

		public SenseViewModel Sense
		{
			get { return _sense; }
		}

		public AlignedNodeViewModel PrefixNode
		{
			get { return _prefixNode; }
		}

		public AlignedNodeViewModel SuffixNode
		{
			get { return _suffixNode; }
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
