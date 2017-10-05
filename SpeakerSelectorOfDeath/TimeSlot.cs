using System;
using System.ComponentModel;

namespace SpeakerSelectorOfDeath
{
	[Serializable]
	public class TimeSlot : INotifyPropertyChanged
	{
		public TimeSlot()
		{
		}

		public static TimeSlot Create(int hour, int minute, int minuteLength, int sequence)
		{
			//we might mess with this later if we ever had a multi-day conference
			//but right now it doesn't really matter
			var baseDate = new DateTime(2012, 10, 27);

			return new TimeSlot
			{
				StartDate = baseDate.AddHours(hour).AddMinutes(minute),
				EndDate = baseDate.AddHours(hour).AddMinutes(minute + minuteLength),
				Sequence = sequence
			};
		}

		private DateTime _startDate;
		public DateTime StartDate
		{
			get { return _startDate; }
			set
			{
				if (_startDate != value)
				{
					_startDate = value;
					FirePropertyChanged("StartDate");
				}
			}
		}
		
		private DateTime _endDate;
		public DateTime EndDate
		{
			get { return _endDate; }
			set
			{
				if (_endDate != value)
				{
					_endDate = value;
					FirePropertyChanged("EndDate");
				}
			}
		}

		private int _sequence;

		public int Sequence
		{
			get { return _sequence; }
			set { _sequence = value; }
		}

		public override string ToString()
		{
			return "TimeSlot: " + StartDate.ToString("HH:MM");
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