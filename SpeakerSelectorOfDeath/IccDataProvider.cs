using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace SpeakerSelectorOfDeath
{
    public interface ISpeakerProvider
    {
        IEnumerable<Speaker> GetSpeakerSessions();
    }

    public class IccSpeakerProvider : ISpeakerProvider
    {
        string _filePath;
        
        public IccSpeakerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public IEnumerable<Speaker> GetSpeakerSessions()
        {
            List<Speaker> speakers = new List<Speaker>();

            //var obj = ServiceStack.Text.CsvSerializer.DeserializeFromStream(typeof(object), File.Open(_filePath, FileMode.OpenOrCreate));
            var dataTable =  CsvToDataTable(_filePath);
			//var dataTable = CSVParser.ParseCSV(_filePath);// CSVReader.ReadCSVFile(_filePath, true);

			//var dataTable = new DataTable();

	        bool hasSpeakerKey = dataTable.Columns.Contains("SpeakerKey");

			foreach (DataRow row in dataTable.Rows)
            {
	            int speakerKey = 0;
	            if (hasSpeakerKey)
	            {
		            speakerKey = Int32.Parse(row["SpeakerKey"].ToString());
	            }
	            else
	            {
		            speakerKey++;
	            }

	            string speakerEmail = row["Email Address"].ToString();

	            Speaker speaker;
				
				bool existingSpeaker = TryResolveSpeaker(speakerEmail, speakers, out speaker);

				if (!existingSpeaker)
				{
					speaker = new Speaker
					{
						SpeakerKey = speakerKey,
						Name = row["Speaker Name"].ToString(),
						HomeTown = row["City, State"].ToString(),
						Email = speakerEmail,
						Website = row["Website or Blog URL"].ToString(),
						HeadshotUrl = row["400x400 Headshot Image of You"].ToString(),
						Bio = row["Speaker Bio"].ToString(),
						NotesToOrganizer = row["Other notes about yourself or your submission"].ToString(),
						PhoneNumber = row["Phone Number"].ToString(),
						Twitter = row["Twitter"].ToString(),
					};
				}

                var session1 = new Session
                {
                    Level = row["Session 1 - Level"].ToString(),
                    Title = row["Session 1 - Title"].ToString(),
                    Description = row["Session 1 - Description"].ToString(),
                };

                var session2 = new Session
                {
                    Level = row["Session 2 - Level"].ToString(),
                    Title = row["Session 2 - Title"].ToString(),
                    Description = row["Session 2 - Description"].ToString(),
                };

                var session3 = new Session
                {
                    Level = row["Session 3 - Level"].ToString(),
                    Title = row["Session 3 - Title"].ToString(),
                    Description = row["Session 3 - Description"].ToString(),
                };

                if (!string.IsNullOrWhiteSpace(session1.Title))
                    speaker.AddSession(session1);

                if (!string.IsNullOrWhiteSpace(session2.Title))
                    speaker.AddSession(session2);

                if (!string.IsNullOrWhiteSpace(session3.Title))
                    speaker.AddSession(session3);

				if (!existingSpeaker)
				{
					speakers.Add(speaker);
				}

            }

            return speakers;
        }

	    private bool TryResolveSpeaker(string speakerEmail, List<Speaker> speakers, out Speaker speaker)
	    {
		    speaker = null;

		    foreach (var compareSpeaker in speakers)
		    {
			    if (speakerEmail == compareSpeaker.Email)
			    {
				    speaker = compareSpeaker;
				    return true;
			    }
		    }

		    return false;
	    }

	    static DataTable CsvToDataTable(string strFileName)
        {
            DataTable dataTable = new DataTable("DataTable Name");

            using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + 
                System.IO.Path.GetDirectoryName(strFileName) + 
                "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\""))
            {
                conn.Open();
                string strQuery = "SELECT * FROM [" + System.IO.Path.GetFileName(strFileName) + "]";
                OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter(strQuery, conn);

                adapter.Fill(dataTable);
            }
            return dataTable;
        }
    }



}
