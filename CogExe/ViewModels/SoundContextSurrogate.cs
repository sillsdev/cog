using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace SIL.Cog.ViewModels
{
	[ProtoContract]
	public class SoundContextSurrogate
	{
		private readonly List<string> _target;

		public SoundContextSurrogate(SoundContext ctxt)
		{
			LeftEnvironment = ctxt.LeftEnvironment == null ? null : ctxt.LeftEnvironment.Name;
			_target = ctxt.Target.Select(seg => seg.StrRep).ToList();
			RightEnvironment = ctxt.RightEnvironment == null ? null : ctxt.RightEnvironment.Name;
		}

		public SoundContextSurrogate()
		{
			_target = new List<string>();
		}

		[ProtoMember(1)]
		public string LeftEnvironment { get; set; }
		[ProtoMember(2)]
		public List<string> Target
		{
			get { return _target; }
		}
		[ProtoMember(3)]
		public string RightEnvironment { get; set; }

		public SoundContext ToSoundContext(CogProject project, Variety variety)
		{
			IAligner aligner = project.Aligners["primary"];
			SoundClass leftEnv = LeftEnvironment == null ? null : aligner.ContextualSoundClasses.First(sc => sc.Name == LeftEnvironment);
			SoundClass rightEnv = RightEnvironment == null ? null : aligner.ContextualSoundClasses.First(sc => sc.Name == RightEnvironment);
			return new SoundContext(leftEnv, new Ngram(_target.Select(s => variety.Segments[s])), rightEnv);
		}
	}
}
