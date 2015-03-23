using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Application.ViewModels
{
	public class WordPairViewModel : ViewModelBase
	{
		private readonly WordPair _wordPair;

		private readonly AlignedNodeViewModel _prefixNode;
		private readonly ReadOnlyCollection<AlignedNodeViewModel> _alignedNodes;
		private readonly Alignment<Word, ShapeNode> _alignment; 
		private readonly AlignedNodeViewModel _suffixNode;
		private readonly MeaningViewModel _meaning;
		private readonly VarietyViewModel _variety1;
		private readonly VarietyViewModel _variety2;
		private readonly bool _areVarietiesInOrder;
		private readonly ICommand _showInMultipleWordAlignmentCommand;

		public WordPairViewModel(IWordAligner aligner, WordPair wordPair, bool areVarietiesInOrder)
		{
			_wordPair = wordPair;
			_areVarietiesInOrder = areVarietiesInOrder;
			_meaning = new MeaningViewModel(_wordPair.Word1.Meaning);
			_variety1 = new VarietyViewModel(_wordPair.VarietyPair.Variety1);
			_variety2 = new VarietyViewModel(_wordPair.VarietyPair.Variety2);

			IWordAlignerResult results = aligner.Compute(_wordPair);
			_alignment = results.GetAlignments().First();
			_prefixNode = new AlignedNodeViewModel(_alignment.Prefixes[0], _alignment.Prefixes[1]);
			var nodes = new List<AlignedNodeViewModel>();
			int i = 0;
			for (int column = 0; column < _alignment.ColumnCount; column++)
			{
				string note = null;
				if (i < _wordPair.AlignmentNotes.Count)
					note = _wordPair.AlignmentNotes[i];
				nodes.Add(new AlignedNodeViewModel(column, _alignment[0, column], _alignment[1, column], note));
				i++;
			}
			_suffixNode = new AlignedNodeViewModel(_alignment.Suffixes[0], _alignment.Suffixes[1]);

			_alignedNodes = new ReadOnlyCollection<AlignedNodeViewModel>(nodes);

			_showInMultipleWordAlignmentCommand = new RelayCommand(ShowInMultipleWordAlignment);
		}

		private void ShowInMultipleWordAlignment()
		{
			Messenger.Default.Send(new SwitchViewMessage(typeof(MultipleWordAlignmentViewModel), _wordPair.Meaning, _wordPair));
		}

		public bool AreVarietiesInOrder
		{
			get { return _areVarietiesInOrder; }
		}

		public VarietyViewModel Variety1
		{
			get { return _variety1; }
		}

		public VarietyViewModel Variety2
		{
			get { return _variety2; }
		}

		public MeaningViewModel Meaning
		{
			get { return _meaning; }
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

		public ICommand ShowInMultipleWordAlignmentCommand
		{
			get { return _showInMultipleWordAlignmentCommand; }
		}

		internal WordPair DomainWordPair
		{
			get { return _wordPair; }
		}

		internal Alignment<Word, ShapeNode> DomainAlignment
		{
			get { return _alignment; }
		}
	}
}
