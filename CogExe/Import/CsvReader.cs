using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SIL.Cog.Import
{
    /// <summary>
    /// Based on code found at
    /// http://knab.ws/blog/index.php?/archives/3-CSV-file-parser-and-writer-in-C-Part-1.html
    /// and
    /// http://knab.ws/blog/index.php?/archives/10-CSV-file-parser-and-writer-in-C-Part-2.html
    /// </summary>
    public class CsvReader
    {
        private readonly TextReader _instream;
        private readonly char _separatorChar;

        /// ///////////////////////////////////////////////////////////////////////
        /// CsvReader
        /// 
        public CsvReader(TextReader inStream, char separatorChar)
        {
	        _eos = false;
	        _instream = inStream;
            _separatorChar = separatorChar;
        }


        /// ///////////////////////////////////////////////////////////////////////
        /// ReadRow
        /// 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row">
        /// Contains the values in the current row, in the order in which they 
        /// appear in the file.
        /// </param>
        /// <returns>
        /// True if a row was returned in parameter "row".
        /// False if no row returned. In that case, you're at the end of the file.
        /// </returns>
        public bool ReadRow(out IList<string> row)
        {
            row = new List<string>();

            while (true)
            {
                // Number of the line where the item starts. Note that an item
                // can span multiple lines.
	            string item;
                bool moreAvailable = GetNextItem(out item);
                if (!moreAvailable)
                {
                    return (row.Count > 0);
                }
                row.Add(item);
            }
        }

        private bool _eos;
        private bool _eol;

        private bool GetNextItem(out string itemString)
        {
            itemString = "";
            if (_eol)
            {
                // previous item was last in line, start new line
                _eol = false;
                return false;
            }

            bool itemFound = false;
            bool quoted = false;
            bool predata = true;
            bool postdata = false;
            var item = new StringBuilder();

            while (true)
            {
                char c = GetNextChar(true);
                if (_eos)
                {
                    if (itemFound) { itemString = item.ToString(); }
                    return itemFound;
                }

                // ---------

                if ((postdata || !quoted) && c == _separatorChar)
                {
                    // end of item, return
                    if (itemFound) { itemString = item.ToString(); }
                    return true;
                }

                if ((predata || postdata || !quoted) && (c == '\x0A' || c == '\x0D'))
                {
                    // we are at the end of the line, eat newline characters and exit
                    _eol = true;
                    if (c == '\x0D' && GetNextChar(false) == '\x0A')
                    {
                        // new line sequence is 0D0A
                        GetNextChar(true);
                    }

                    if (itemFound) { itemString = item.ToString(); }
                    return true;
                }

                if (predata && c == ' ')
                    // whitespace preceeding data, discard
                    continue;

                if (predata && c == '"')
                {
                    // quoted data is starting
                    quoted = true;
                    predata = false;
                    itemFound = true;
                    continue;
                }

                if (predata)
                {
                    // data is starting without quotes
                    predata = false;
                    item.Append(c);
                    itemFound = true;
                    continue;
                }

                if (c == '"' && quoted)
                {
                    if (GetNextChar(false) == '"')
                    {
                        // double quotes within quoted string means add a quote       
                        item.Append(GetNextChar(true));
                    }
                    else
                    {
                        // end-quote reached
                        postdata = true;
                    }

                    continue;
                }

                // all cases covered, character must be data
                item.Append(c);
            }
        }

        private readonly char[] _buffer = new char[4096];
        private int _pos;
        private int _length;

        private char GetNextChar(bool eat)
        {
            if (_pos >= _length)
            {
                _length = _instream.ReadBlock(_buffer, 0, _buffer.Length);
                if (_length == 0)
                {
                    _eos = true;
                    return '\0';
                }
                _pos = 0;
            }
            if (eat)
                return _buffer[_pos++];

	        return _buffer[_pos];
        }
    }
}
