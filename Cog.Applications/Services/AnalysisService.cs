using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine;

namespace SIL.Cog.Applications.Services
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
			_busyService.ShowBusyIndicatorUntilUpdated();
			var pipeline = new MultiThreadedPipeline<Variety>(GetSegmentProcessors());
			pipeline.Process(_projectService.Project.Varieties);
			pipeline.WaitForComplete();
		}

		public void Segment(Variety variety)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			var pipeline = new Pipeline<Variety>(GetSegmentProcessors());
			pipeline.Process(variety.ToEnumerable());
		}

		private IEnumerable<IProcessor<Variety>> GetSegmentProcessors()
		{
			CogProject project = _projectService.Project;
			var processors = new List<IProcessor<Variety>> {new VarietySegmenter(project.Segmenter)};
			IProcessor<Variety> syllabifier;
			if (project.VarietyProcessors.TryGetValue("syllabifier", out syllabifier))
				processors.Add(syllabifier);
			else
				processors.Add(new SimpleSyllabifier());
			processors.Add(new SegmentFrequencyDistributionCalculator(_segmentPool));
			return processors;
		}

		public void StemAll(object ownerViewModel, StemmingMethod method)
		{
			Messenger.Default.Send(new DomainModelChangingMessage());
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
				});
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			_dialogService.ShowModalDialog(ownerViewModel, progressVM);
		}

		public void Stem(StemmingMethod method, Variety variety)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			Messenger.Default.Send(new DomainModelChangingMessage());
			if (method == StemmingMethod.Automatic)
				variety.Affixes.Clear();

			var pipeline = new Pipeline<Variety>(GetStemProcessors(method));
			pipeline.Process(variety.ToEnumerable());
		}

		public IEnumerable<IProcessor<Variety>> GetStemProcessors(StemmingMethod method)
		{
			CogProject project = _projectService.Project;
			var processors = new List<IProcessor<Variety>> {new AffixStripper(project.Segmenter)};
			if (method != StemmingMethod.Manual)
				processors.Add(_projectService.Project.VarietyProcessors["affixIdentifier"]);
			processors.Add(new Stemmer(_spanFactory, project.Segmenter));
			IProcessor<Variety> syllabifier;
			if (_projectService.Project.VarietyProcessors.TryGetValue("syllabifier", out syllabifier))
				processors.Add(syllabifier);
			processors.Add(new SegmentFrequencyDistributionCalculator(_segmentPool));
			return processors;
		}

		public void CompareAll(object ownerViewModel)
		{
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
						return;
					vm.Text = "Analyzing results...";
					Messenger.Default.Send(new ComparisonPerformedMessage());
				});
			pipeline.ProgressUpdated += (sender, e) => progressVM.Value = e.PercentCompleted;

			_dialogService.ShowModalDialog(ownerViewModel, progressVM);
		}

		public void Compare(VarietyPair varietyPair)
		{
			_busyService.ShowBusyIndicatorUntilUpdated();
			var pipeline = new Pipeline<VarietyPair>(GetCompareProcessors());
			pipeline.Process(varietyPair.ToEnumerable());
		}

		private IEnumerable<IProcessor<VarietyPair>> GetCompareProcessors()
		{
			CogProject project = _projectService.Project;
			var processors = new List<IProcessor<VarietyPair>> {new WordPairGenerator(project, "primary")};
			IProcessor<VarietyPair> similarSegmentIdentifier;
			if (project.VarietyPairProcessors.TryGetValue("similarSegmentIdentifier", out similarSegmentIdentifier))
				processors.Add(similarSegmentIdentifier);
			processors.Add(project.VarietyPairProcessors["soundChangeInducer"]);
			processors.Add(new GlobalSoundCorrespondenceIdentifier(_segmentPool, project, "primary"));
			return processors;
		}
	}
}
