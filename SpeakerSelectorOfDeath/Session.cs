using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpeakerSelectorOfDeath
{
	[Serializable]
	public class Session : INotifyPropertyChanged
	{
		public Session()
		{
			_state = SelectionState.Default;
		}

		private int _sessionKey;

		public int SessionKey
		{
			get { return _sessionKey; }
			set { _sessionKey = value; }
		}


		private string _level;
		public string Level
		{
			get { return _level; }
			set
			{
				if (_level != value)
				{
					_level = value;
					FirePropertyChanged("Level");
				}
			}
		}

		private string _title;
		public string Title
		{
			get { return _title; }
			set
			{
				if (_title != value)
				{
					_title = value;
					FirePropertyChanged("Title");
				}
			}
		}

		private string _description;
		public string Description
		{
			get { return _description; }
			set
			{
				if (_description != value)
				{
					_description = value;
					FirePropertyChanged("Description");
				}
			}
		}

		private int _rating;
		public int Rating
		{
			get { return _rating; }
			set
			{
				if (_rating != value)
				{
					_rating = value;
					FirePropertyChanged("Rating");
				}
			}
		}
		

		private Speaker _speaker;
		public Speaker Speaker
		{
			get { return _speaker; }
			set
			{
				if (_speaker != value)
				{
					_speaker = value;
					FirePropertyChanged("Speaker");
				}
			}
		}

		private Selection _selection;
		public Selection Selection
		{
			get { return _selection; }
			set
			{
				if (_selection != value)
				{
					_selection = value;
					if (_selection != null)
						_selection.Session = this;

					ValidateSelectedSessions();

					FirePropertyChanged("Selection");
				}
			}
		}


		private bool _highlight;
		public bool Highlight
		{
			get { return _highlight; }
			set
			{
				if (_highlight == value)
					return;
				_highlight = value;

				if (value)
					State = State.Include(SelectionState.InSearchResult);
				else
					State = State.Remove(SelectionState.InSearchResult);

				FirePropertyChanged("Highlight");
			}
		}

		private void ValidateSelectedSessions()
		{
			bool speakerHasSelections = false;

			foreach (var speakerSession in Speaker.Sessions)
			{
				if (speakerSession.Selection != null)
				{
					speakerHasSelections = true;

					int count = Speaker.CountSessionsInTime(speakerSession.Selection.TimeSlot);

					if (count > 1)
					{
						speakerSession.State = speakerSession.State.Include(SelectionState.SameTimeConflict);
					}
					else
					{
						speakerSession.State = speakerSession.State.Remove(SelectionState.SameTimeConflict);
					}

					foreach (var compareSession in Speaker.Sessions)
					{
						if (compareSession.Selection != null && compareSession != speakerSession)
						{
							if (Math.Abs(compareSession.Selection.TimeSlot.Sequence - speakerSession.Selection.TimeSlot.Sequence) == 1)
							{
								speakerSession.State = speakerSession.State.Include(SelectionState.BackToBack);
								break;
							}
							
							speakerSession.State = speakerSession.State.Remove(SelectionState.BackToBack);
						}
					}
				}
				else
				{
					speakerSession.State = speakerSession.State.Remove(SelectionState.SameTimeConflict);
					speakerSession.State = speakerSession.State.Remove(SelectionState.BackToBack);
				}
			}

			foreach (var speakerSession in Speaker.Sessions)
			{
				if (speakerHasSelections)
				{
					speakerSession.State = speakerSession.State.Remove(SelectionState.SpeakerNoSelection);
				}
				else
				{
					speakerSession.State = speakerSession.State.Include(SelectionState.SpeakerNoSelection);
				}
			}
		}

		public SelectionState _state;

		public SelectionState State
		{
			get
			{
				return _state;
			}
			set
			{
				if (_state == value)
					return;

				_state = value;
				
				FirePropertyChanged("State");
			}
		}

		public bool IsSelected
		{
			get { return Selection != null; }
		}

		public override string ToString()
		{
			return "Session: " + Title;
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

	}
}