using System;
using System.Windows;
using GraphSharp.Algorithms.Layout.Simple.Grid;
using QuickGraph;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Controls
{
	public class GlobalCorrespondencesGraphLayout : WeightedGraphLayout<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge,
		IBidirectionalGraph<GlobalCorrespondencesGraphVertex, GlobalCorrespondencesGraphEdge>>
	{
		public GlobalCorrespondencesGraphLayout()
		{
			SetLayoutParameters(typeof(ConsonantManner), typeof(ConsonantPlace));
		}

		public static readonly DependencyProperty SyllablePositionProperty = DependencyProperty.Register("SyllablePosition", typeof(SyllablePosition),
			typeof(GlobalCorrespondencesGraphLayout), new UIPropertyMetadata(SyllablePosition.Onset, SyllablePositionPropertyChanged));

		private static void SyllablePositionPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			var graphLayout = (GlobalCorrespondencesGraphLayout) depObj;
			switch (graphLayout.SyllablePosition)
			{
				case SyllablePosition.Onset:
				case SyllablePosition.Coda:
					graphLayout.SetLayoutParameters(typeof(ConsonantManner), typeof(ConsonantPlace));
					break;

				case SyllablePosition.Nucleus:
					graphLayout.SetLayoutParameters(typeof(VowelHeight), typeof(VowelBackness));
					break;
			}
		}

		private void SetLayoutParameters(Type rowType, Type columnType)
		{
			const int rowHeight = 35;
			const int columnWidth = 40;
			const int separatorWidth = 10;
			var parameters = new GridLayoutParameters();
			parameters.Rows.Add(new GridLayoutRow {AutoHeight = true});
			foreach (object row in Enum.GetValues(rowType))
				parameters.Rows.Add(new GridLayoutRow {Height = rowHeight});

			parameters.Columns.Add(new GridLayoutColumn {AutoWidth = true});
			foreach (object column in Enum.GetValues(columnType))
			{
				parameters.Columns.Add(new GridLayoutColumn {Width = columnWidth});
				parameters.Columns.Add(new GridLayoutColumn {Width = separatorWidth});
				parameters.Columns.Add(new GridLayoutColumn {Width = columnWidth});
			}

			parameters.VertexInfo += parameters_VertexInfo;
			LayoutParameters = parameters;
		}

		private void parameters_VertexInfo(object sender, GridVertexInfoEventArgs e)
		{
			object vertex = e.Vertex;
			if (vertex is ConsonantPlaceVertex)
			{
				var place = (ConsonantPlaceVertex) vertex;
				e.VertexInfo.Row = 0;
				e.VertexInfo.Column = (((int) place.Place) * 3) + 1;
				e.VertexInfo.ColumnSpan = 3;
			}
			else if (vertex is ConsonantMannerVertex)
			{
				var manner = (ConsonantMannerVertex) vertex;
				e.VertexInfo.Row = ((int) manner.Manner) + 1;
				e.VertexInfo.Column = 0;
				e.VertexInfo.HorizontalAlignment = GridHorizontalAlignment.Left;
			}
			else if (vertex is VowelBacknessVertex)
			{
				var backness = (VowelBacknessVertex) vertex;
				e.VertexInfo.Row = 0;
				e.VertexInfo.Column = (((int) backness.Backness) * 3) + 1;
				e.VertexInfo.ColumnSpan = 3;
			}
			else if (vertex is VowelHeightVertex)
			{
				var height = (VowelHeightVertex) vertex;
				e.VertexInfo.Row = ((int) height.Height) + 1;
				e.VertexInfo.Column = 0;
				e.VertexInfo.HorizontalAlignment = GridHorizontalAlignment.Left;
			}
			else if (vertex is GlobalConsonantVertex)
			{
				var consonant = (GlobalConsonantVertex) vertex;
				e.VertexInfo.Row = ((int) consonant.Manner) + 1;
				e.VertexInfo.Column = (((int) consonant.Place) * 3) + 1;
				if (consonant.Voiced)
				{
					e.VertexInfo.Column += 2;
					e.VertexInfo.HorizontalAlignment = GridHorizontalAlignment.Left;
				}
				else
				{
					e.VertexInfo.HorizontalAlignment = GridHorizontalAlignment.Right;
				}
			}
			else if (vertex is GlobalVowelVertex)
			{
				var vowel = (GlobalVowelVertex) vertex;
				e.VertexInfo.Row = ((int) vowel.Height) + 1;
				e.VertexInfo.Column = (((int) vowel.Backness) * 3) + 1;
				if (vowel.Round)
				{
					e.VertexInfo.Column += 2;
					e.VertexInfo.HorizontalAlignment = GridHorizontalAlignment.Left;
				}
				else
				{
					e.VertexInfo.HorizontalAlignment = GridHorizontalAlignment.Right;
				}
			}
		}

		public SyllablePosition SyllablePosition
		{
			get { return (SyllablePosition) GetValue(SyllablePositionProperty); }
			set { SetValue(SyllablePositionProperty, value); }
		}

		public override bool CanAnimate
		{
			get { return false; }
		}
	}
}
