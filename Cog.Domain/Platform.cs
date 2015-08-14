using System;

namespace SIL.Cog.Domain
{
	static class Platform
	{
		private static bool _isMono;

		static Platform()
		{
			_isMono = Type.GetType("Mono.Runtime") != null;
		}

		public static bool IsMono
		{
			get { return _isMono; }
		}

		public static bool IsDotNet
		{
			get { return !_isMono; }
		}
	}
}
