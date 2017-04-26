using System;

namespace SIL.Cog.Domain
{
	public static class Platform
	{
		static Platform()
		{
			IsMono = Type.GetType("Mono.Runtime") != null;
		}

		public static bool IsMono { get; }
		public static bool IsDotNet => !IsMono;
	}
}
