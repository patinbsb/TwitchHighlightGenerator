using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace HighlightGenerator
{
    public static class ChatLogParser
    {
        private static readonly string ChatLogPrefix = ConfigurationManager.AppSettings["ChatLogPrefix"];

        public static void GenerateChatRows(Broadcast broadcast)
        {
            DateTime date = broadcast.StartTime;

            // Use selenium to scrape required text
            var url = ChatLogPrefix + date.ToString("MMMM") + "%20" + date.ToString("yyyy") + "/" + date.ToString("yyyy-MM-dd") + ".txt";

            string chatLogRaw;
            using (WebClient client = new WebClient())
            {
                chatLogRaw = client.DownloadString(url);
            }

            // We parse the chat log into messages.
            var pattern = "\\[\\d\\d\\d\\d-\\d\\d-\\d\\d \\d\\d:\\d\\d:\\d\\d UTC\\] .*: .*";

            Regex regex = new Regex(pattern);
            var matches = regex.Matches(chatLogRaw);

            Regex timeStampRegex = new Regex("\\d\\d\\d\\d-\\d\\d-\\d\\d \\d\\d:\\d\\d:\\d\\d");
            Regex userNameRegex = new Regex("\\d\\d UTC\\] .*:");
            Regex messageContentRegex = new Regex("\\d\\d UTC\\] .*: .*");

            StringBuilder sCommand = new StringBuilder("INSERT INTO `dsp`.`chatlog`\n(`broadcastid`,\n`message`,\n`date`,\n`username`)\n" +
                                                       "VALUES ");

            List<string> rows = new List<string>();

            foreach (var match in matches)
            {
                // Get timestamp, username and message content
                var timestampMatch = timeStampRegex.Match(match.ToString());
                if (timestampMatch.Success)
                {
                    var messageHour = int.Parse(timestampMatch.Value.Substring(11, 2));

                    if (messageHour < date.Hour)
                    {
                        // Skip this message, wont be needed.
                        continue;
                    }

                    var userNameMatch = userNameRegex.Match(match.ToString());
                    var messageContentMatch = messageContentRegex.Match(match.ToString());

                    // Trim out required regex pattern characters for username
                    string userName = userNameMatch.Value.Substring(8, (userNameMatch.Value.Length - 9));
                    string messageContent =
                        messageContentMatch.Value.Substring(messageContentMatch.Value.IndexOf(':') + 2);

                    rows.Add($"({broadcast.Id}, '{MySqlHelper.EscapeString(messageContent.Replace("@", ""))}', '{timestampMatch.Value}', " +
                             $"'{MySqlHelper.EscapeString(userName.Replace("@", " "))}')");
                }
                else
                {
                    throw new Exception($"Failed to find timestamp in message. {match}");
                }
            }

            MySqlConnection mySqlConnection = new MySqlConnection(Helper.MySqlConnection);
            mySqlConnection.Open();

            sCommand.Append(string.Join(",", rows));
            sCommand.Append(";");

            MySqlCommand command = new MySqlCommand(sCommand.ToString(), mySqlConnection)
            {
                CommandType = CommandType.Text
            };
            command.ExecuteNonQuery();

            mySqlConnection.Close();
        }

        public static ChatLog GetChatInRange(DateTime videoStart, double offsetStart, double offsetEnd)
        {
            DateTime startTime = videoStart.AddSeconds(offsetStart);
            DateTime endTime = videoStart.AddSeconds(offsetEnd);
            List<Message> chatRange = new List<Message>();

            MySqlConnection mySqlConnection = new MySqlConnection(Helper.MySqlConnection);
            mySqlConnection.Open();
            MySqlCommand command = new MySqlCommand("SELECT message, date, username from chatlog where " +
                                                    $"date between '{startTime.ToUniversalTime():yyyy-MM-dd HH:mm:ss}'" +
                                                    $" and '{endTime.ToUniversalTime():yyyy-MM-dd HH:mm:ss}'", mySqlConnection);

            MySqlDataAdapter dataAdapter = new MySqlDataAdapter {SelectCommand = command};
            DataSet messageDataSet = new DataSet();
            dataAdapter.Fill(messageDataSet, "chatlog");
            mySqlConnection.Close();

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
