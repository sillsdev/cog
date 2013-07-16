using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Export;

namespace SIL.Cog.Presentation.Views
{
	public class UnicodeCsvClipboardExporter : ClipboardExporterBase
	{
		private string _indentationString;
		private MemoryStream _baseStream;
		private bool _isFirstColumn;

		public UnicodeCsvClipboardExporter()
		{
			IncludeColumnHeaders = true;
			FormatSettings = new CsvFormatSettings();
			_indentationString = "";
			_baseStream = new ToStringMemoryStream();
		}

		public CsvFormatSettings FormatSettings { get; private set; }

		protected override object ClipboardData
		{
			get { return _baseStream; }
		}

		protected override void Indent()
		{
			_indentationString += FormatSettings.Separator;
		}

		protected override void Unindent()
		{
			if (_indentationString == null)
			{
				Debug.Fail( "Indentation must at least be string.empty when unindenting." );
			}
			else
			{
				int indentationLength = _indentationString.Length;

				// If there are less characters in indentationString than in the empty field, just set indentation
				// as string.empty
				_indentationString = indentationLength < 1 ? "" : _indentationString.Substring(0, indentationLength - 1);
			}
		}

		protected override void ResetExporter()
		{
			_baseStream = new ToStringMemoryStream();
			_indentationString = "";
		}

		protected override void StartHeader(DataGridContext dataGridContext)
		{
			if (!string.IsNullOrEmpty(_indentationString))
				WriteToBaseStream(_indentationString);

			// The next StartDataItemField will be considered as first column
			_isFirstColumn = true;
		}

		protected override void StartHeaderField( DataGridContext dataGridContext, Column column )
		{
			// We always insert the separator before the value except for the first item
			if (!_isFirstColumn)
				WriteToBaseStream(FormatSettings.Separator);
			else
				_isFirstColumn = false;

			object columnHeader = UseFieldNamesInHeader || column.Title == null ? column.FieldName : column.Title;

			string fieldValueString = FormatCsvData(null, columnHeader);
			WriteToBaseStream(fieldValueString);
		}

		protected override void EndHeader(DataGridContext dataGridContext)
		{
			WriteToBaseStream(FormatSettings.NewLine);
		}

		protected override void StartDataItem( DataGridContext dataGridContext, object dataItem )
		{
			if (!string.IsNullOrEmpty(_indentationString))
				WriteToBaseStream(_indentationString);

			// The next StartDataItemField will be considered as first column
			_isFirstColumn = true;
		}

		protected override void StartDataItemField(DataGridContext dataGridContext, Column column, object fieldValue)
		{
			// We always insert the separator before the value except for the first item
			if( !_isFirstColumn )
				WriteToBaseStream(FormatSettings.Separator);
			else
				_isFirstColumn = false;

			string fieldValueString = FormatCsvData(null, fieldValue);
			WriteToBaseStream(fieldValueString);
		}

		protected override void EndDataItem(DataGridContext dataGridContext, object dataItem)
		{
			WriteToBaseStream(FormatSettings.NewLine);
		}

		private void WriteToBaseStream(char value)
		{
			byte[] tempBuffer = Encoding.Unicode.GetBytes(new[] {value});
			_baseStream.Write(tempBuffer, 0, tempBuffer.Length);
		}

		private void WriteToBaseStream(string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			byte[] tempBuffer = Encoding.Unicode.GetBytes(value);
			_baseStream.Write(tempBuffer, 0, tempBuffer.Length);
		}

		private string FormatCsvData(Type dataType, object dataValue)
		{
			string outputString = null;
			bool checkWhitespace = true;

			if ((dataValue != null) && (!Convert.IsDBNull(dataValue)) && (!(dataValue is Array)))
			{
				if (dataType == null)
					dataType = dataValue.GetType();

				if (dataType == typeof(string))
				{
					string textQualifier = FormatSettings.TextQualifier.ToString(CultureInfo.InvariantCulture);

					if (textQualifier == "\0")
						outputString = (string) dataValue;
					else
						outputString = FormatSettings.TextQualifier + ((string) dataValue).Replace(textQualifier, textQualifier + textQualifier) + FormatSettings.TextQualifier;

					checkWhitespace = false;
				}
				else if (dataType == typeof(DateTime))
				{
					if (!string.IsNullOrEmpty(FormatSettings.DateTimeFormat))
						outputString = ((DateTime) dataValue).ToString(FormatSettings.DateTimeFormat, FormatSettings.Culture ?? CultureInfo.InvariantCulture);
				}
				else if ((dataType == typeof(double)) ||
							(dataType == typeof(decimal)) ||
							(dataType == typeof(float)) ||
							(dataType == typeof(int)) ||
							(dataType == typeof(double)) ||
							(dataType == typeof(decimal)) ||
							(dataType == typeof(float)) ||
							(dataType == typeof(short)) ||
							(dataType == typeof(Single)) ||
							(dataType == typeof(UInt16)) ||
							(dataType == typeof(UInt32)) ||
							(dataType == typeof(UInt64)) ||
							(dataType == typeof(Int16)) ||
							(dataType == typeof(Int64)))
				{
					string format = FormatSettings.NumericFormat;

					if (((dataType == typeof (double)) ||
					     (dataType == typeof (decimal)) ||
					     (dataType == typeof (float))) &&
					    (!string.IsNullOrEmpty(FormatSettings.FloatingPointFormat)))
					{
						format = FormatSettings.FloatingPointFormat;
					}

					if (!string.IsNullOrEmpty(format))
						outputString = string.Format(FormatSettings.Culture ?? CultureInfo.InvariantCulture, "{0:" + format + "}", dataValue);
				}

				if (outputString == null)
					outputString = string.Format(FormatSettings.Culture ?? CultureInfo.InvariantCulture, "{0}", dataValue);

				// For dates and numbers, as a rule, we never use the TextQualifier. However, the
				// specified format can introduce whitespaces. To better support this case, we add
				// the TextQualifier if needed.
				if ((checkWhitespace) && (FormatSettings.TextQualifier != '\0'))
				{
					bool useTextQualifier = false;

					// If the output string contains the character used to separate the fields, we
					// don't bother checking for whitespace. TextQualifier will be used.
					if (outputString.IndexOf(FormatSettings.Separator) < 0)
					{
						for (int i = 0; i < outputString.Length; i++)
						{
							if (char.IsWhiteSpace(outputString, i))
							{
								useTextQualifier = true;
								break;
							}
						}
					}
					else
					{
						useTextQualifier = true;
					}

					if (useTextQualifier)
						outputString = FormatSettings.TextQualifier + outputString + FormatSettings.TextQualifier;
				}

			}

			return outputString;
		}

		private class ToStringMemoryStream : MemoryStream
		{
			public override string ToString()
			{
				if (Length == 0)
					return "";

				try
				{
					return Encoding.Unicode.GetString(ToArray());
				}
				catch (Exception)
				{
					return base.ToString();
				}
			}
		}
	}
}
