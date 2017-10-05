﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;

namespace SpeakerSelectorOfDeath
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string FILE_FILTER = "Speaker Selections OF DEATH (*.ssod)|*.ssod|Speaker Selections OF DEATH (*.json)|*.json";
		ViewModel _viewModel;

		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			PreserveReferencesHandling = PreserveReferencesHandling.Objects
		};

		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;

			//var data = WriteObject<ViewModel>(viewModel);
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			
			_viewModel = new ViewModel();

			//ISpeakerProvider speakerProvider = new IccSpeakerProvider(@"C:\Users\Jon\Downloads\icc2015.csv");

			//var speakers = speakerProvider.GetSpeakerSessions();

			//_viewModel.Speakers.AddRange(speakers);
			//_viewModel.UnselectedSessions.AddRange(speakers.SelectMany(s => s.Sessions));

			InitializeRoomsAndTimes();

			DataContext = _viewModel;
		}


		private void InitializeRoomsAndTimes()
		{
			//List<string> roomNames = new List<string> { "Auditorium", "116E", "118E", "119E", "121E", "125E", "123E", "126E" };
			List<string> roomNames = new List<string> 
			{ 
				"Room A (50)",
				"Room B (50)",
				"Room C (30)",
				"Room D (30)",
				"Room E (30)",
				"Room F (30)",
				"Room G (30)",
				"Room H (30)",
			};

			_viewModel.TimeSlots = new ObservableCollection<TimeSlot> 
			{ 
				TimeSlot.Create(9, 0, 75, 1),
				TimeSlot.Create(10, 30, 75, 2),
				TimeSlot.Create(12, 45, 75, 3),
				TimeSlot.Create(14, 15, 75, 4),
				TimeSlot.Create(15, 45, 75, 5),
			};

			foreach (var roomName in roomNames)
			{
				var room = new Room { RoomName = roomName };

				foreach (var timeSlot in _viewModel.TimeSlots)
				{
					room.Selections.Add(new Selection { Room = room, TimeSlot = timeSlot });
				}

				_viewModel.Rooms.Add(room);

			}

			

		}


		#region Session Drag and Drop

		//
		//http://blogs.gotdotnet.com/jaimer/archive/2007/07/12/drag-drop-in-wpf-explained-end-to-end.aspx
		//

		Point _startPoint = new Point();
		bool IsDragging = false;

		//we will only allow sessions to be dragged onto selections

		private void Session_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
			{
				Point position = e.GetPosition(null);

				if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					StartDrag(sender, e);
				}
			}

		}

		private void Session_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_startPoint = e.GetPosition(null);
		}

		private void StartDrag(object sender, MouseEventArgs e)
		{
			IsDragging = true;

			Panel panel = (Panel)sender;
			object dataContext = panel.DataContext;

			if (dataContext != null)
			{
				DataObject data = new DataObject(dataContext.GetType(), dataContext);
				DragDropEffects de = DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
			}
			IsDragging = false;
		}

		private void StartDragCustomCursor(object sender, MouseEventArgs e)
		{
			Panel panel = (Panel)sender;
			object dataContext = panel.DataContext;

			GiveFeedbackEventHandler handler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);

			((FrameworkElement)sender).GiveFeedback += handler;
			IsDragging = true;
			DataObject data = new DataObject(dataContext.GetType(), dataContext);

			DragDropEffects de = DragDrop.DoDragDrop(((FrameworkElement)sender), data, DragDropEffects.Move);
			((FrameworkElement)sender).GiveFeedback -= handler;
			IsDragging = false;
		}

		void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			try
			{
				Mouse.SetCursor(Cursors.Hand);

				e.UseDefaultCursors = false;
				e.Handled = true;
			}
			finally { }
		}

		private void Selection_Drop(object sender, DragEventArgs e)
		{
			
			//if we are dropping a session on a selection

			Panel panel = sender as Panel;
			if (panel != null && panel.DataContext is Selection)
			{
				var targetDropSelection = (Selection)panel.DataContext;

				if (e.Data.GetDataPresent(typeof(Session)))
				{
					Session droppingSession = (Session)e.Data.GetData(typeof(Session));

					if (droppingSession != targetDropSelection.Session)
					{
						if (droppingSession.Selection != null && targetDropSelection.Session != null)
						{
							//if we're dragging a session that is already scheduled, to a different spot, just swap them instead of 
							//adding one to the unselected sessions list

							var originalSourceSelection = droppingSession.Selection;
							var sessionDroppedOnTo = targetDropSelection.Session;

							droppingSession.Selection = targetDropSelection;
							originalSourceSelection.Session = sessionDroppedOnTo;
							
							return;
						}

						//if there was an old session, add it to unselected list
						if (targetDropSelection.Session != null)
						{
							targetDropSelection.Session.Selection = null;
							_viewModel.UnselectedSessions.Add(targetDropSelection.Session);
						}

						if (droppingSession.Selection != null)
							droppingSession.Selection.Session = null;

						//if unselected had the dropped session, remove it
						_viewModel.UnselectedSessions.Remove(droppingSession);

						targetDropSelection.Session = droppingSession;
					}

				}

			}
			else if (sender == UnselectedSessionsBox)
			{
				if (e.Data.GetDataPresent(typeof(Session)))
				{
					Session droppingSession = (Session)e.Data.GetData(typeof(Session));
					//don't do anything with a drop from unselected to unselected
					if (!_viewModel.UnselectedSessions.Contains(droppingSession))
					{
						droppingSession.Selection.Session = null;
						droppingSession.Selection = null;

						_viewModel.UnselectedSessions.Add(droppingSession);
					}
				}
			}
			//panel.Background = Brushes.Transparent;

			_viewModel.IsDirty = true;
		}

		private void Selection_DragEnter(object sender, DragEventArgs e)
		{
			//Panel potentialDropTarget = (Panel)sender;
			//if (potentialDropTarget.DataContext is Session)
			//{
			//    if (e.Data.GetDataPresent(typeof(Session)))
			//    {
			//        Session droppingSession = (Session)e.Data.GetData(typeof(Session));
			//        if (droppingSession.ContainsInTree(potentialDropTarget.DataContext as RuleCategory))
			//            potentialDropTarget.Background = Brushes.Pink;
			//        else
			//            potentialDropTarget.Background = Brushes.LightGreen;
			//    }
			//    else
			//        potentialDropTarget.Background = Brushes.LightGreen;
			//}
			//else if (potentialDropTarget.DataContext is Rule)
			//    potentialDropTarget.Background = Brushes.Pink;
		}

		private void Selection_DragLeave(object sender, DragEventArgs e)
		{
			//Panel panel = (Panel)sender;
			//panel.Background = Brushes.Transparent;
		}

		#endregion

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			SaveAs();
		}

		private void SaveAs()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Save Speaker Selections";
			sfd.DefaultExt = "ssod";
			sfd.Filter = FILE_FILTER;
			sfd.RestoreDirectory = true;

			if (sfd.ShowDialog() == true)
			{
				using (Stream stream = File.Open(sfd.FileName, FileMode.OpenOrCreate))
				{
					if (Path.GetExtension(sfd.FileName) == ".json")
					{
						JsonSerializer serializer = JsonSerializer.Create(_jsonSerializerSettings);
						// XmlSerializer serializer = new XmlSerializer(typeof(ViewModel), new[]{ typeof(Selection), typeof(Speaker), typeof(Session), typeof(Room), typeof(TimeSlot) });

						using (TextWriter writer = new StreamWriter(stream))
						{
							serializer.Serialize(writer, _viewModel);
						}
					}
					else
					{
						BinaryFormatter serializer = new BinaryFormatter();
						serializer.Serialize(stream, _viewModel);
					}
				}

				_viewModel.IsDirty = false;
			}
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Load Speaker Selections";
			ofd.DefaultExt = "ssod";
			ofd.Filter = FILE_FILTER;
			ofd.RestoreDirectory = true;

			if (ofd.ShowDialog() == true)
			{
				using (Stream stream = File.Open(ofd.FileName, FileMode.OpenOrCreate))
				{
					ViewModel viewModel;

					if (Path.GetExtension(ofd.FileName) == ".json")
					{
						JsonSerializer serializer = JsonSerializer.Create(_jsonSerializerSettings);
						using (StreamReader sr = new StreamReader(stream))
						{
							viewModel = serializer.Deserialize<ViewModel>(new JsonTextReader(sr));
						}
					}
					else
					{
						BinaryFormatter serializer = new BinaryFormatter();
						viewModel = serializer.Deserialize(stream) as ViewModel;
					}

					_viewModel = viewModel;
					_viewModel.IsDirty = false;
					this.DataContext = _viewModel;
				}
			}

			_viewModel.AssignKeys();
		}

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Greg's stuff";
			sfd.DefaultExt = "csv";
			sfd.Filter = "Gregs selections (*.csv)|*.csv";
			sfd.RestoreDirectory = true;
			HashSet<string> emails = new HashSet<string>();
			if (sfd.ShowDialog() == true)
			{
				var fileBuilder = new StringBuilder();
				int speakerKey = 1;
				int sessionKey = 1;
				foreach (Speaker speaker in _viewModel.Speakers)
				{
					foreach (Session session in speaker.Sessions)
					{
						var lineBuilder = new StringBuilder();

						if (speaker.SpeakerKey != 0)
						{
							lineBuilder.Append(speaker.SpeakerKey); lineBuilder.Append(",");
						}
						else
						{
							lineBuilder.Append(speakerKey); lineBuilder.Append(",");
						}

						if (session.SessionKey != 0)
						{
							lineBuilder.Append(sessionKey); lineBuilder.Append(",");
						}
						else
						{
							lineBuilder.Append(session.SessionKey); lineBuilder.Append(",");
						}

						lineBuilder.Append(session.Selection != null); lineBuilder.Append(",");
						lineBuilder.Append(session.Selection != null ? session.Selection.Room.RoomName : ""); lineBuilder.Append(",");
						lineBuilder.Append(session.Selection != null ? session.Selection.TimeSlot.StartDate.ToString("h:mm") : ""); lineBuilder.Append(",");
						lineBuilder.Append(speaker.Name); lineBuilder.Append(",");
						lineBuilder.Append(session.Level); lineBuilder.Append(",");
						lineBuilder.Append(session.Title); lineBuilder.Append(",");

						fileBuilder.AppendLine(lineBuilder.ToString());                        

						sessionKey++;
					}
					speakerKey++;

					emails.Add(speaker.Email);
				}

				using (StreamWriter writer = new StreamWriter(sfd.FileName))
				{
					writer.Write(fileBuilder.ToString());
				}
			}
		}

		private void ExportSplitButton_Click(object sender, RoutedEventArgs e)
		{
			ExportSpeakers();
			ExportSessions();
		}

		private void ExportSpeakers()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Greg's stuff";
			sfd.DefaultExt = "csv";
			sfd.Filter = "Speakers (*.csv)|*.csv";
			sfd.RestoreDirectory = true;
			sfd.FileName = "Speakers.csv";

			if (sfd.ShowDialog() == true)
			{
				var fileBuilder = new StringBuilder();

				AppendField(fileBuilder, "SpeakerKey");
				AppendField(fileBuilder, "SpeakerName");
				AppendField(fileBuilder, "CityState");
				AppendField(fileBuilder, "EmailAddress");
				AppendField(fileBuilder, "WebsiteBlogUrl");
				AppendField(fileBuilder, "HeadshotUrl");
				AppendField(fileBuilder, "SpeakerBio");
				AppendField(fileBuilder, "OtherNotes");
				AppendField(fileBuilder, "Twitter");
				AppendField(fileBuilder, "PhoneNumber");
				EndRow(fileBuilder);

				foreach (Speaker speaker in _viewModel.Speakers)
				{
					AppendField(fileBuilder, speaker.SpeakerKey.ToString());
					AppendField(fileBuilder, speaker.Name);
					AppendField(fileBuilder, speaker.HomeTown);
					AppendField(fileBuilder, speaker.Email);
					AppendField(fileBuilder, speaker.Website);
					AppendField(fileBuilder, speaker.HeadshotUrl);
					AppendField(fileBuilder, speaker.Bio);
					AppendField(fileBuilder, speaker.NotesToOrganizer);
					AppendField(fileBuilder, speaker.Twitter);
					AppendField(fileBuilder, speaker.PhoneNumber);

					EndRow(fileBuilder);
				}

				using (FileStream fileStream = new FileStream(sfd.FileName, FileMode.Create))
				{
					using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
					{
						writer.Write(fileBuilder.ToString());
					}
				}
			}
		}

		private void ExportSessions()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Greg's stuff";
			sfd.DefaultExt = "csv";
			sfd.Filter = "Sessions (*.csv)|*.csv";
			sfd.RestoreDirectory = true;
			sfd.FileName = "Sessions.csv";

			if (sfd.ShowDialog() == true)
			{
				var fileBuilder = new StringBuilder();

				AppendField(fileBuilder, "SpeakerKey");
				AppendField(fileBuilder, "SessionKey");
				AppendField(fileBuilder, "Selected");
				AppendField(fileBuilder, "Room");
				AppendField(fileBuilder, "Time");
				AppendField(fileBuilder, "SessionLevel");
				AppendField(fileBuilder, "SessionTitle");
				AppendField(fileBuilder, "SessionDescription");
				EndRow(fileBuilder);

				foreach (Speaker speaker in _viewModel.Speakers)
				{
					foreach (Session speakerSession in speaker.Sessions)
					{
						AppendField(fileBuilder, speaker.SpeakerKey.ToString());
						AppendField(fileBuilder, speakerSession.SessionKey.ToString());
						AppendField(fileBuilder, speakerSession.IsSelected ? "1" : "0");

						if (speakerSession.IsSelected)
						{
							AppendField(fileBuilder, speakerSession.Selection.Room.RoomName);
							AppendField(fileBuilder, speakerSession.Selection.TimeSlot.StartDate.ToString("h:mmtt"));
						}
						else
						{
							AppendField(fileBuilder, string.Empty);	
							AppendField(fileBuilder, string.Empty);	
						}

						AppendField(fileBuilder, speakerSession.Level);
						AppendField(fileBuilder, speakerSession.Title);
						AppendField(fileBuilder, speakerSession.Description);
				
						EndRow(fileBuilder);
					}
				}

				using (FileStream fileStream = new FileStream(sfd.FileName, FileMode.Create))
				{
					using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
					{
						writer.Write(fileBuilder.ToString());
					}
				}
			}
		}

		private void AppendField(StringBuilder csvContent, string fieldValue)
		{
			string contentDelimiter = string.Empty;

			if (fieldValue != null)
			{
				fieldValue = CleanMsWordCharacters(fieldValue);

				if (fieldValue.IndexOfAny(new char[] {',', '"', '\r', '\n'}) != -1)
				{
					contentDelimiter = "^";
				}
			}

			csvContent.AppendFormat("{1}{0}{1}|", fieldValue, contentDelimiter);
		}

		private void EndRow(StringBuilder csvContent)
		{
			if (csvContent[csvContent.Length - 1] == '|')
			{
				csvContent.Remove(csvContent.Length - 1, 1);
			}

			csvContent.AppendLine();
		}


		private void EmailCsv_Click(object sender, RoutedEventArgs e)
		{
			var haveSessions = new List<string>();
			var noSessions = new List<string>();

			foreach (Speaker speaker in _viewModel.Speakers)
			{
				if (speaker.Sessions.Any(s => s.Selection != null))
					haveSessions.Add(speaker.Email);
				else
					noSessions.Add(speaker.Email);
				
			}
		}

		private void ImportButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Import Speaker Submissions";
			ofd.DefaultExt = "txt";
			ofd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
			ofd.FilterIndex = 1;
			ofd.RestoreDirectory = true;

			if (ofd.ShowDialog() == true)
			{
				_viewModel = new ViewModel();

				ISpeakerProvider speakerProvider = new IccSpeakerProvider(ofd.FileName);

				var speakers = speakerProvider.GetSpeakerSessions();

				_viewModel.Speakers.AddRange(speakers);
				_viewModel.UnselectedSessions.AddRange(speakers.SelectMany(s => s.Sessions));

				InitializeRoomsAndTimes();

				_viewModel.IsDirty = false;

				DataContext = _viewModel;
			}
		}

		private void ImportExtraButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Import Speaker Submissions";
			ofd.DefaultExt = "txt";
			ofd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
			ofd.FilterIndex = 1;
			ofd.RestoreDirectory = true;

			if (ofd.ShowDialog() == true)
			{
				ISpeakerProvider speakerProvider = new IccSpeakerProvider(ofd.FileName);

				var speakers = speakerProvider.GetSpeakerSessions();

				foreach (var speaker in _viewModel.Speakers)
				{
					foreach (var speakerExtraFields in speakers)
					{
						if (speaker.Email == speakerExtraFields.Email)
						{
							speaker.Twitter = speakerExtraFields.Twitter;
							speaker.PhoneNumber = speakerExtraFields.PhoneNumber;
						}
					}
				}
			}
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			if (_viewModel.IsDirty)
			{
				MessageBoxResult result = MessageBox.Show(this, "Schedule has been modified. Save your changes?", "Warning", MessageBoxButton.YesNoCancel);

				switch (result)
				{
					case MessageBoxResult.Cancel:
						e.Cancel = true;
						return;
					case MessageBoxResult.No:
						return;
					case MessageBoxResult.Yes:
						SaveAs();
						break;
				}
			}
		}

		public string CleanMsWordCharacters(string value)
		{
			StringBuilder buffer = new StringBuilder(value);

			buffer = buffer.Replace('\u2013', '-');
			buffer = buffer.Replace('\u2014', '-');
			buffer = buffer.Replace('\u2015', '-');
			buffer = buffer.Replace('\u2017', '_');
			buffer = buffer.Replace('\u2018', '\'');
			buffer = buffer.Replace('\u2019', '\'');
			buffer = buffer.Replace('\u201a', ',');
			buffer = buffer.Replace('\u201b', '\'');
			buffer = buffer.Replace('\u201c', '\"');
			buffer = buffer.Replace('\u201d', '\"');
			buffer = buffer.Replace('\u201e', '\"');
			buffer = buffer.Replace("\u2026", "...");
			buffer = buffer.Replace('\u2032', '\'');
			buffer = buffer.Replace('\u2033', '\"');
			buffer = buffer.Replace("â€™", "'");
			buffer = buffer.Replace("â€œ", "\"");

			// Includes a non-visible extended character 
			buffer = buffer.Replace("â€", "\"");

			// Em-dash
			buffer = buffer.Replace("â€\"", "-");
			buffer = buffer.Replace("â€¦", "...");

			return buffer.ToString();
		}

	}

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

			foreach (var speaker in _speakers)
			{
				if (speaker.SpeakerKey != 0)
				{
					allSpeakerKeysZero = false;
					break;
				}
			}

			if (allSpeakerKeysZero)
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
	}

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
			foreach (var speakerSession in Speaker.Sessions)
			{
				if (speakerSession.Selection != null)
				{
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

	[Flags]
	public enum SelectionState
	{
		Default = 0,
		InSearchResult = 1,
		SameTimeConflict = 2,
		BackToBack = 4
	}

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

	public static class Extensions
	{
		public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
		{
			foreach (T t in items)
			{
				collection.Add(t);
			}
		}
		
	}

	public class SessionTemplateSelector : DataTemplateSelector
	{

		/// <summary>
		/// initializes SessionTemplateSelector
		/// </summary>
		public SessionTemplateSelector()
		{
		}
		
		public DataTemplate NonNullTemplate { get; set; }
		

		

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
				return null;

			return NonNullTemplate;

			//return base.SelectTemplate(item, container);
		}
		
			
		
	}

	public class SelectionStateToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var selectionState = (SelectionState) value;

			if (selectionState.Has(SelectionState.InSearchResult))
				return new SolidColorBrush(Colors.Pink);

			if (selectionState.Has(SelectionState.SameTimeConflict))
				return new SolidColorBrush(Colors.Coral);

			if (selectionState.Has(SelectionState.BackToBack))
				return new SolidColorBrush(Colors.BlueViolet);

			return new SolidColorBrush(Colors.LightGreen);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class BrushColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				{
					return new SolidColorBrush(Colors.Pink);
				}
			}
			return new SolidColorBrush(Colors.LightGreen);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}


}
