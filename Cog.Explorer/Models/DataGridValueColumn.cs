using System;
using Newtonsoft.Json;

namespace SIL.Cog.Explorer.Models
{

	public class DataGridValueColumn : DataGridColumn
	{
		[JsonConverter(typeof(FieldConverter))]
		public string Field { get; set; }
		public bool? Visible { get; set; }
		public int? MinWidth { get; set; }
		public string CssClass { get; set; }
		public DataGridHorzAlign? Align { get; set; }
		public DataGridEditor? HeaderFilter { get; set; }
		public EditorParams HeaderFilterParams { get; set; }
		public string HeaderFilterPlaceholder { get; set; }
		public bool? HeaderSort { get; set; }
		public DataGridFormatter? Formatter { get; set; }
		public object FormatterParams { get; set; }
	}

	public abstract class EditorParams { }

	public class SelectParams : EditorParams
	{
		public object Values { get; set; }
	}

	public class FieldConverter : JsonConverter<string>
	{
		public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
		{
			if (value != null && char.IsUpper(value, 0))
				value = value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
			writer.WriteValue(value);
		}

		public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{

			var value = (string)reader.Value;
			if (value != null && char.IsLower(value, 0))
				value = value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
			return value;
		}
	}
}
