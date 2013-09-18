using System.Windows;
using QuickGraph;
using SIL.Cog.Applications.GraphAlgorithms;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Controls
{
	public class GlobalCorrespondencesGraphLayout : CogGraphLayout<GridVertex, GlobalCorrespondenceEdge, IBidirectionalGraph<GridVertex, GlobalCorrespondenceEdge>>
	{
		public GlobalCorrespondencesGraphLayout()
		{
			SetConsonantLayoutParameters();
		}

		public static readonly DependencyProperty SyllablePositionProperty = DependencyProperty.Register("SyllablePosition", typeof(ViewModelSyllablePosition),
			typeof(GlobalCorrespondencesGraphLayout), new UIPropertyMetadata(ViewModelSyllablePosition.Onset, SyllablePositionPropertyChanged));

		private static void SyllablePositionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (GlobalCorrespondencesGraphLayout) depObj;
			switch (graphLayout.SyllablePosition)
			{
				case ViewModelSyllablePosition.Onset:
				case ViewModelSyllablePosition.Coda:
					graphLayout.SetConsonantLayoutParameters();
					break;

				case ViewModelSyllablePosition.Nucleus:
					graphLayout.SetVowelLayoutParameters();
					break;
			}
		}

		private void SetConsonantLayoutParameters()
		{
			const int rowHeight = 35;
			const int columnWidth = 30;
			const int separatorWidth = 10;
			LayoutParameters = new GridLayoutParameters
				{
					Rows =
						{
							// header
							new GridLayoutRow {AutoHeight = true},
							// nasal
							new GridLayoutRow {Height = rowHeight},
							// stop
							new GridLayoutRow {Height = rowHeight},
							// affricate
							new GridLayoutRow {Height = rowHeight},
							// fricative
							new GridLayoutRow {Height = rowHeight},
							// approximant
							new GridLayoutRow {Height = rowHeight},
							// flap or tap
							new GridLayoutRow {Height = rowHeight},
							// trill
							new GridLayoutRow {Height = rowHeight},
							// lateral fricative
							new GridLayoutRow {Height = rowHeight},
							// lateral approximant
							new GridLayoutRow {Height = rowHeight}
						},
					Columns =
						{
							// header
							new GridLayoutColumn {AutoWidth = true},
							// bilabial
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// labiodental
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// dental
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// alveolar
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// postalveolar
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// retroflex
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// palatal
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// velar
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// uvular
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// pharyngeal
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// glottal
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
						}
				};
		}

		private void SetVowelLayoutParameters()
		{
			const int rowHeight = 35;
			const int columnWidth = 30;
			const int separatorWidth = 10;
			LayoutParameters = new GridLayoutParameters
				{
					Rows =
						{
							// header
							new GridLayoutRow {AutoHeight = true},
							// close
							new GridLayoutRow {Height = rowHeight},
							// near-close
							new GridLayoutRow {Height = rowHeight},
							// close-mid
							new GridLayoutRow {Height = rowHeight},
							// mid
							new GridLayoutRow {Height = rowHeight},
							// open-mid
							new GridLayoutRow {Height = rowHeight},
							// near-open
							new GridLayoutRow {Height = rowHeight},
							// open
							new GridLayoutRow {Height = rowHeight}
						},
					Columns =
						{
							// header
							new GridLayoutColumn {AutoWidth = true},
							// front
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// near-front
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// central
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// near-back
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth},
							// back
							new GridLayoutColumn {Width = columnWidth},
							new GridLayoutColumn {Width = separatorWidth},
							new GridLayoutColumn {Width = columnWidth}
						}
				};
		}

		public ViewModelSyllablePosition SyllablePosition
		{
			get { return (ViewModelSyllablePosition) GetValue(SyllablePositionProperty); }
			set { SetValue(SyllablePositionProperty, value); }
		}

		public override bool CanAnimate
		{
			get { return false; }
		}
	}
}
