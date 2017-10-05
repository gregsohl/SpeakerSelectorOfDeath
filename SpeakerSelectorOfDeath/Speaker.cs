using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SpeakerSelectorOfDeath
{
	[Serializable]
	public class Speaker : INotifyPropertyChanged
	{
		private int _speakerKey;

		public int SpeakerKey
		{
			get { return _speakerKey; }
			set { _speakerKey = value; }
		}

		private string _name;
		public string Name
		{
			get { return _name; }
			set
			{
				if (_name != value)
				{
					_name = value;
					FirePropertyChanged("Name");
				}
				
			}
		}

		private string _homeTown;
		public string HomeTown
		{
			get { return _homeTown; }
			set
			{
				if (_homeTown != value)
				{
					_homeTown = value;
					FirePropertyChanged("HomeTown");
				}
			}
		}

		private string _email;
		public string Email
		{
			get { return _email; }
			set
			{
				if (_email != value)
				{
					_email = value;
					FirePropertyChanged("Email");
				}
			}
		}

		private string _website;
		public string Website
		{
			get { return _website; }
			set
			{
				if (_website != value)
				{
					_website = value;
					FirePropertyChanged("Website");
				}
			}
		}

		private string _headshotUrl;
		public string HeadshotUrl
		{
			get { return _headshotUrl; }
			set
			{
				if (_headshotUrl != value)
				{
					_headshotUrl = value;
					FirePropertyChanged("HeadshotUrl");
				}
			}
		}

		private string _bio;
		public string Bio
		{
			get { return _bio; }
			set
			{
				if (_bio != value)
				{
					_bio = value;
					FirePropertyChanged("Bio");
				}
			}
		}

		private string _notesToOrganizer;
		public string NotesToOrganizer
		{
			get { return _notesToOrganizer; }
			set
			{
				if (_notesToOrganizer != value)
				{
					_notesToOrganizer = value;
					FirePropertyChanged("NotesToOrganizer");
				}
			}
		}

		public void AddSession(Session session)
		{
			_sessions.Add(session);
			session.Speaker = this;
		}

		private ObservableCollection<Session> _sessions = new ObservableCollection<Session>();
		public ObservableCollection<Session> Sessions
		{
			get { return _sessions; }
		}

		public bool Verify()
		{
			foreach (var session in _sessions)
			{
				TimeSlot sessionTime = session.Selection.TimeSlot;

				foreach (var compareSession in _sessions)
				{
					if (!session.Equals(compareSession)) { 
						TimeSlot compareSessionTime = compareSession.Selection.TimeSlot;

						if (sessionTime.StartDate == compareSessionTime.StartDate)
						{
							return false;
						}}
				}
			}

			return true;
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

		public int CountSessionsInTime(TimeSlot timeSlot)
		{
			int count = 0;
			foreach (var session in _sessions)
			{
				if ((session.Selection != null) &&
					(session.Selection.TimeSlot == timeSlot))
				{
					count++;
				}
			}

			return count;
		}

		private Dictionary<string, string> _additionalColumns;

		public Dictionary<string, string> AdditionalColumns
		{
			get { return _additionalColumns; }
			set { _additionalColumns = value; }
		}

		private static string[] _additionalColumnNames;

		public static string[] AdditionalColumnNames
		{
			get { return _additionalColumnNames; }
			set { _additionalColumnNames = value; }
		}

		private string _phoneNumber;

		public string PhoneNumber
		{
			get { return _phoneNumber; }
			set { _phoneNumber = value; }
		}

		private string _twitter;
		public string Twitter
		{
			get { return _twitter; }
			set { _twitter = value; }
		}
	}
}