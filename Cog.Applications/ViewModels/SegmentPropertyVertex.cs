using System;
using System.ComponentModel;
using System.Reflection;

namespace SIL.Cog.Applications.ViewModels
{
    public abstract class SegmentPropertyVertex : GlobalCorrespondencesGraphVertex
    {
        public override bool IsProperty
        {
            get { return true; }
        }

		protected static string GetEnumDescription(Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());

			var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes.Length > 0)
				return attributes[0].Description;
			return value.ToString();
		}
    }
}
