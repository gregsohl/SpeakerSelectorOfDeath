using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SpeakerSelectorOfDeath
{
	[Serializable]
	public class Room : INotifyPropertyChanged
	{
		private string _roomName;
		public string RoomName
		{
			get { return _roomName; }
			set
			{
				if (_roomName != value)
				{
					_roomName = value;
					FirePropertyChanged("RoomName");
				}
			}
		}

		private string _trackName;
		public string TrackName
		{
			get { return _trackName; }
			set
			{
				if (_trackName != value)
				{
					_trackName = value;
					FirePropertyChanged("TrackName");
				}
			}
		}

		private ObservableCollection<Selection> _selections = new ObservableCollection<Selection>();
		public ObservableCollection<Selection> Selections
		{
			get { return _selections; }
			set
			{
				if (_selections != value)
				{
					_selections = value;
					FirePropertyChanged("Selections");
				}
			}
		}

		public override string ToString()
		{
			return "Room: " + RoomName;
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