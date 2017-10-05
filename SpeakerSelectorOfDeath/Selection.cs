using System;
using System.ComponentModel;

namespace SpeakerSelectorOfDeath
{
	[Serializable]
	public class Selection : INotifyPropertyChanged
	{
		private Room _room;
		public Room Room
		{
			get { return _room; }
			set
			{
				if (_room != value)
				{
					_room = value;
					FirePropertyChanged("Room");
				}
			}
		}

		private TimeSlot _timeSlot;
		public TimeSlot TimeSlot
		{
			get { return _timeSlot; }
			set
			{
				if (_timeSlot != value)
				{
					_timeSlot = value;
					FirePropertyChanged("TimeSlot");
				}
			}
		}

		private Session _session;
		public Session Session
		{
			get { return _session; }
			set
			{
				if (_session != value)
				{
					_session = value;
					if (_session != null)
						_session.Selection = this;

					FirePropertyChanged("Session");
				}
			}
		}


		#region INotifyPropertyChanged Members

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		private void FirePropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}