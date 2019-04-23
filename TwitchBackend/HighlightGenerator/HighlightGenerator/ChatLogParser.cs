using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace HighlightGenerator
{
    /// <summary>
    /// Responsible for inserting a days worth of chat messages into the local database and retrieving a slice of messages within a time range.
    /// </summary>
    public static class ChatLogParser
    {
        // Location of the third party Twitch chat-log service.
        private static readonly string ChatLogPrefix = ConfigurationManager.AppSettings["ChatLogPrefix"];

        /// <summary>
        /// Downloads a days worth of chat-logs linked to the input Broadcast from the third party overrustlelogs service.
        /// Inserts in bulk into a local database for future use.
        /// </summary>
        /// <param name="broadcast"></param>
        public static void GenerateChatRows(Broadcast broadcast)
        {
            DateTime date = broadcast.StartTime;

            // Building the url that points to the chat-log text file we need.
            var url = ChatLogPrefix + date.ToString("MMMM") + "%20" + date.ToString("yyyy") + "/" + date.ToString("yyyy-MM-dd") + ".txt";

            // Download the text file to local memory.
            string chatLogRaw;
            using (WebClient client = new WebClient())
            {
                chatLogRaw = client.DownloadString(url);
            }

            // We parse the chat log into messages.

            // Each regex pattern corresponds to a subsection of the messages we find.
            Regex timeStampRegex = new Regex("\\d\\d\\d\\d-\\d\\d-\\d\\d \\d\\d:\\d\\d:\\d\\d");
            Regex userNameRegex = new Regex("\\d\\d UTC\\] ([^:]*): ");
            Regex messageContentRegex = new Regex("\\d\\d UTC\\] [^:]*: (.*)");

            // Extract individual messages.
            var messagePattern = "\\[\\d\\d\\d\\d-\\d\\d-\\d\\d \\d\\d:\\d\\d:\\d\\d UTC\\] .*: .*";
            Regex regex = new Regex(messagePattern);
            var messageMatches = regex.Matches(chatLogRaw);

            // We build up a large single insert command to make the insert operation fast.
            StringBuilder sCommand = new StringBuilder("INSERT INTO `dsp`.`chatlog`\n(`broadcastid`,\n`message`,\n`date`,\n`username`)\n" +
                                                       "VALUES ");

            // Build up the insert command with each messageMatch we find.
            List<string> rows = new List<string>();
            foreach (var match in messageMatches)
            {
                // Get timestamp, username and message content
                var timestampMatch = timeStampRegex.Match(match.ToString());
                if (timestampMatch.Success)
                {
                    // We get the hour of the message to restrict our row inserts to only relevant chat-log messages for the Broadcast
                    var messageHour = int.Parse(timestampMatch.Value.Substring(11, 2));
                    if (messageHour < date.Hour)
                    {
                        continue;
                    }

                    var userNameMatch = userNameRegex.Match(match.ToString()).Groups[1];
                    var messageContentMatch = messageContentRegex.Match(match.ToString()).Groups[1];
                    // Trim out required regex pattern characters for username.
                    string userName = userNameMatch.Value;
                    if (userName.Length > 200)
                    {
                        Console.Write("shit");
                    }
                    string messageContent =
                        messageContentMatch.Value;

                    // Our row insert we append to the large insert.
                    rows.Add($"({broadcast.Id}, '{MySqlHelper.EscapeString(messageContent.Replace("@", ""))}', '{timestampMatch.Value}', " +
                             $"'{MySqlHelper.EscapeString(userName.Replace("@", ""))}')");
                }
                else
                {
                    throw new Exception($"Failed to find timestamp in message. {match}");
                }
            }
            // add in each row to the large insert statement.
            sCommand.Append(string.Join(",", rows));
            sCommand.Append(";");

            // We prepare to insert into the database.
            MySqlConnection mySqlConnection = new MySqlConnection(Helper.MySqlConnection);
            mySqlConnection.Open();
            MySqlCommand command = new MySqlCommand(sCommand.ToString(), mySqlConnection)
            {
                CommandType = CommandType.Text
            };
            command.ExecuteNonQuery();
            mySqlConnection.Close();
        }

        /// <summary>
        /// Returns a local representation of the chat-log messages we require in the specified time range.
        /// </summary>
        /// <param name="videoStart"></param>
        /// <param name="offsetStart"></param>
        /// <param name="offsetEnd"></param>
        /// <returns></returns>
        public static ChatLog GetChatInRange(DateTime videoStart, double offsetStart, double offsetEnd)
        {
            // Setup info for sql select statement.
            DateTime startTime = videoStart.AddSeconds(offsetStart);
            DateTime endTime = videoStart.AddSeconds(offsetEnd);
            List<Message> chatRange = new List<Message>();

            // Configure select statement and destination object.
            MySqlConnection mySqlConnection = new MySqlConnection(Helper.MySqlConnection);
            MySqlCommand command = new MySqlCommand("SELECT message, date, username from chatlog where " +
                                                    $"date between '{startTime.ToUniversalTime():yyyy-MM-dd HH:mm:ss}'" +
                                                    $" and '{endTime.ToUniversalTime():yyyy-MM-dd HH:mm:ss}'", mySqlConnection);
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter { SelectCommand = command };

            // Load into memory the select statement.
            mySqlConnection.Open();
            DataSet messageDataSet = new DataSet();
            dataAdapter.Fill(messageDataSet, "chatlog");
            mySqlConnection.Close();

            // Build our chat-log from the gathered data.
            var messageRows = messageDataSet.Tables["chatlog"].Rows;
            foreach (DataRow row in messageRows)
            {
                chatRange.Add(new Message(Convert.ToDateTime(row.ItemArray[1]), row.ItemArray[2].ToString(),
                    row.ItemArray[0].ToString()));
            }
            return new ChatLog(chatRange);
        }
    }
}
