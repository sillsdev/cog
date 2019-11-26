using System;
using Newtonsoft.Json;

namespace SIL.Cog.Explorer.Models
{
	public enum ColumnAlign
	{
		Left,
		Center,
		Right
	}

	public enum Editor
	{
		Input,
		Select,
		Number
	}

	public enum Formatter
	{
		Plaintext,
		Textarea,
		Html
	}

	public class DataGridValueColumn : DataGridColumn
	{
		public DataGridValueColumn(string title)
			: base(title)
		{
		}

		[JsonConverter(typeof(FieldConverter))]
		public string Field { get; set; }
		public bool? Visible { get; set; }
		public int? MinWidth { get; set; }
		public string CssClass { get; set; }
		public ColumnAlign? Align { get; set; }
		public Editor? HeaderFilter { get; set; }
		public EditorParams HeaderFilterParams { get; set; }
		public string HeaderFilterPlaceholder { get; set; }
		public Formatter? Formatter { get; set; }
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
