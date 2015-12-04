using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Services
{
	public class AnalysisService : IAnalysisService
	{
		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;

		public AnalysisService(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, IProjectService projectService, IDialogService dialogService, IBusyService busyService)
		{
			_spanFactory = spanFactory;
			_segmentPool = segmentPool;
			_projectService = projectService;
			_dialogService = dialogService;
			_busyService = busyService;
		}

		public void SegmentAll()
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			var pipeline = new MultiThreadedPipeline<Variety>(GetSegmentProcessors());
			pipeline.Process(_projectService.Project.Varieties);
			pipeline.WaitForComplete();
		}

		public void Segment(Variety variety)
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			var pipeline = new Pipeline<Variety>(GetSegmentProcessors());
			pipeline.Process(variety.ToEnumerable());
		}

		private IEnumerable<IProcessor<Variety>> GetSegmentProcessors()
		{
			CogProject project = _projectService.Project;
			return new[]
				{
					new VarietySegmenter(project.Segmenter),
					project.VarietyProcessors[ComponentIdentifiers.Syllabifier],
					new SegmentFrequencyDistributionCalculator(_segmentPool)
				};
		}

		public void StemAll(object ownerViewModel, StemmingMethod method)
		{
			if (method == StemmingMethod.Automatic)
			{
				foreach (Variety variety in _projectService.Project.Varieties)
					variety.Affixes.Clear();
			}

			var pipeline = new MultiThreadedPipeline<Variety>(GetStemProcessors(method));
			var progressVM = new ProgressViewModel(pvm =>
				{
					pvm.Text = "Stemming all varieties...";
					pipeline.Process(_projectService.Project.Varieties);
					while (!pipeline.WaitForComplete(100))
					{
						if (pvm.Canceled)
						{
							pipeline.Cancel();
							pipeline.WaitForComplete();
						}
					}
				}, false, true);
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			_dialogService.ShowModalDialog(ownerViewModel, progressVM);
			Messenger.Default.Send(new DomainModelChangedMessage(true));
		}

		public void Stem(StemmingMethod method, Variety variety)
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			if (method == StemmingMethod.Automatic)
				variety.Affixes.Clear();

			var pipeline = new Pipeline<Variety>(GetStemProcessors(method));
			pipeline.Process(variety.ToEnumerable());
			Messenger.Default.Send(new DomainModelChangedMessage(true));
		}

		public IEnumerable<IProcessor<Variety>> GetStemProcessors(StemmingMethod method)
		{
			CogProject project = _projectService.Project;
			var processors = new List<IProcessor<Variety>> {new AffixStripper(project.Segmenter)};
			IProcessor<Variety> syllabifier = project.VarietyProcessors[ComponentIdentifiers.Syllabifier];
			if (method != StemmingMethod.Manual)
			{
				processors.Add(syllabifier);
				processors.Add(_projectService.Project.VarietyProcessors[ComponentIdentifiers.AffixIdentifier]);
			}
			processors.Add(new Stemmer(_spanFactory, project.Segmenter));
			processors.Add(syllabifier);
			processors.Add(new SegmentFrequencyDistributionCalculator(_segmentPool));
			return processors;
		}

		public void CompareAll(object ownerViewModel)
		{
			if (_projectService.Project.Varieties.Count == 0 || _projectService.Project.Meanings.Count == 0)
				return;

			Messenger.Default.Send(new PerformingComparisonMessage());
			var generator = new VarietyPairGenerator();
			generator.Process(_projectService.Project);

			var pipeline = new MultiThreadedPipeline<VarietyPair>(GetCompareProcessors());

			var progressVM = new ProgressViewModel(vm =>
				{
					vm.Text = "Comparing all variety pairs...";
					pipeline.Process(_projectService.Project.VarietyPairs);
					while (!pipeline.WaitForComplete(500))
					{
						if (vm.Canceled)
						{
							pipeline.Cancel();
							pipeline.WaitForComplete();
							break;
						}
					}
					if (vm.Canceled)
					{
						_projectService.Project.VarietyPairs.Clear();
					}
					else
					{
						vm.Text = "Analyzing results...";
						Messenger.Default.Send(new ComparisonPerformedMessage());
					}
				}, false, true);
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			_dialogService.ShowModalDialog(ownerViewModel, progressVM);
		}

		public void Compare(VarietyPair varietyPair)
		{
			_busyService.ShowBusyIndicatorUntilFinishDrawing();
			Messenger.Default.Send(new PerformingComparisonMessage(varietyPair));
			var pipeline = new Pipeline<VarietyPair>(GetCompareProcessors());
			pipeline.Process(varietyPair.ToEnumerable());
			Messenger.Default.Send(new ComparisonPerformedMessage(varietyPair));
		}

		private IEnumerable<IProcessor<VarietyPair>> GetCompareProcessors()
		{
			CogProject project = _projectService.Project;
			var processors = new List<IProcessor<VarietyPair>>
				{
					project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator],
					new EMSoundChangeInducer(_segmentPool, project, ComponentIdentifiers.PrimaryWordAligner, ComponentIdentifiers.PrimaryCognateIdentifier),
					new SoundCorrespondenceIdentifier(_segmentPool, project, ComponentIdentifiers.PrimaryWordAligner)
				};
			return processors;
		}
	}
}
