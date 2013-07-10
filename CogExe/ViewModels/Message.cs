using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	internal enum MessageType
	{
		// this message might be called in a worker thread
		ComparisonPerformed,
		ComparisonInvalidated,
		SwitchView,
		ViewChanged
	}

	internal class Message : MessageBase
	{
		private readonly MessageType _type;
		private readonly object _data;

		public Message(MessageType type)
			: this (type, null)
		{
		}

		public Message(MessageType type, object data)
		{
			_type = type;
			_data = data;
		}

		public MessageType Type
		{
			get { return _type; }
		}

		public object Data
		{
			get { return _data; }
		}
	}
}
