using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.ViewModels
{
	public class SegmentVarietyViewModel : ViewModelBase
	{
		private readonly Segment _segment;

		public SegmentVarietyViewModel(Segment segment)
		{
			_segment = segment;
		}

		public string StrRep
		{
			get { return _segment.NormalizedStrRep; }
		}

		public double Probability
		{
			get { return _segment.Probability; }
		}

		public int Frequency
		{
			get { return _segment.Frequency; }
		}

		public string Type
		{
			get
			{
				if (_segment.Type == CogFeatureSystem.ConsonantType)
					return "Consonant";
				return "Vowel";
			}
		}

		public string FeatureStructure
		{
			get
			{
				var sb = new StringBuilder();
				sb.Append("[");
				bool firstFeature = true;
				foreach (SymbolicFeature feature in _segment.FeatureStruct.Features.Where(f => !CogFeatureSystem.Instance.ContainsFeature(f)))
				{
					if (!firstFeature)
						sb.Append(",");
					sb.Append(feature.Description);
					sb.Append(":");
					SymbolicFeatureValue fv = _segment.FeatureStruct.GetValue(feature);
					FeatureSymbol[] symbols = fv.Values.ToArray();
					if (symbols.Length > 1)
						sb.Append("{");
					bool firstSymbol = true;
					foreach (FeatureSymbol symbol in symbols)
					{
						if (!firstSymbol)
							sb.Append(",");
						sb.Append(symbol.Description);
						firstSymbol = false;
					}
					if (symbols.Length > 1)
						sb.Append("}");
					firstFeature = false;
				}
				sb.Append("]");
				return sb.ToString();
			}
		}

		public Segment ModelSegment
		{
			get { return _segment; }
		}
	}
}
