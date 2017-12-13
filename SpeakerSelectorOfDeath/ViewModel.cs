using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SpeakerSelectorOfDeath
{
	[Serializable]
	public class ViewModel : INotifyPropertyChanged
	{
		private bool _isDirty;

		public bool IsDirty
		{
			get { return _isDirty; }
			set { _isDirty = value; }
		}

		private string _search = "";
		public string Search
		{
			get { return _search; }
			set
			{
				if (_search == value)
					return;
				_search = value;
				Highlight();
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Search"));
			}
		}

		private void Highlight()
		{
			if (_search.Length > 1 && _search.StartsWith("="))
			{
				int sessionCount;

				if (Int32.TryParse(_search.Substring(1), out sessionCount))
				{
					foreach (var speaker in Speakers)
					{
						int selectedSessionCount = 0;

						foreach (var session in speaker.Sessions)
						{
							if (session.IsSelected)
							{
								selectedSessionCount++;
							}
						}
						foreach (var session in speaker.Sessions)
						{
							session.Highlight = (selectedSessionCount >= sessionCount);
						}
					}
				}
			}
			else
			{
				var searchRegex = new Regex(_search, RegexOptions.IgnoreCase);

				if (_search == "")
					searchRegex = new Regex(@"asdofiwoinfoinas;donifosianwoinwef", RegexOptions.IgnoreCase);

				foreach (var speaker in Speakers)
				{
					var speakerHighlight = searchRegex.IsMatch(speaker.Name);

					foreach (var session in speaker.Sessions)
					{
						session.Highlight = searchRegex.IsMatch(session.Title) || speakerHighlight;
					}
				}
			}
		}

		
		private ObservableCollection<Speaker> _speakers = new ObservableCollection<Speaker>();
		public ObservableCollection<Speaker> Speakers
		{
			get { return _speakers; }
			set
			{
				if (_speakers != value)
				{
					_speakers = value;
					FirePropertyChanged("Speakers");
				}
			}
		}

		private ObservableCollection<Session> _selectedSessions = new ObservableCollection<Session>();
		public ObservableCollection<Session> SelectedSessions
		{
			get { return _selectedSessions; }
			set
			{
				if (_selectedSessions != value)
				{
					_selectedSessions = value;
					FirePropertyChanged("SelectedSessions");
				}
			}
		}

		private ObservableCollection<Session> _unselectedSessions = new ObservableCollection<Session>();
		public ObservableCollection<Session> UnselectedSessions
		{
			get { return _unselectedSessions; }
			set
			{
				if (_unselectedSessions != value)
				{
					_unselectedSessions = value;
					FirePropertyChanged("UnselectedSessions");
				}
			}
		}

		private ObservableCollection<Room> _rooms = new ObservableCollection<Room>();
		public ObservableCollection<Room> Rooms
		{
			get { return _rooms; }
			set
			{
				if (_rooms != value)
				{
					_rooms = value;
					FirePropertyChanged("Rooms");
				}
			}
		}

		private ObservableCollection<TimeSlot> _timeSlots;
		public ObservableCollection<TimeSlot> TimeSlots
		{
			get { return _timeSlots; }
			set
			{
				if (_timeSlots != value)
				{
					_timeSlots = value;
					FirePropertyChanged("TimeSlots");
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

		public void AssignKeys()
		{
			bool allSpeakerKeysZero = true;
			bool allSpeakerKeysSame = true;
			int firstSpeakerKey = -1;

			foreach (var speaker in _speakers)
			{
				if (speaker.SpeakerKey != 0)
				{
					allSpeakerKeysZero = false;
				}

				if (firstSpeakerKey == -1)
				{
					firstSpeakerKey = speaker.SpeakerKey;
				}
				else
				{
					if (firstSpeakerKey != speaker.SpeakerKey)
					{
						allSpeakerKeysSame = false;
					}
				}
			}

			if (allSpeakerKeysZero || allSpeakerKeysSame)
			{
				int speakerKey = 1;
				int sessionKey = 1;

				foreach (var speaker in _speakers)
				{
					speaker.SpeakerKey = speakerKey++;

					foreach (var session in speaker.Sessions)
					{
						session.SessionKey = sessionKey++;
					}
				}
			}
		}

		public void InitializeSessionStates()
		{
			foreach (var speaker in Speakers)
			{
				bool speakerHasSelectedSessions = false;

				foreach (var speakerSession in speaker.Sessions)
				{
					if (speakerSession.Selection != null)
					{
						speakerHasSelectedSessions = true;
						break;
					}
				}

				foreach (var speakerSession in speaker.Sessions)
				{
					if (speakerHasSelectedSessions)
					{
						speakerSession.State = speakerSession.State.Remove(SelectionState.SpeakerNoSelection);
					}
					else
					{
						speakerSession.State = speakerSession.State.Include(SelectionState.SpeakerNoSelection);
					}
				}
			}
		}
	}
}