using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace HighlightGenerator
{
    internal class ChatLogParser
    {

        internal ChatLogParser()
        {
            this.ChatLogPrefix = ConfigurationManager.AppSettings["ChatLogPrefix"];
        }

        private string ChatLogPrefix { get; set; }

        internal ChatLog GenerateChatLog(DateTime date)
        {
            // Use selenium to scrape required text
            var url = ChatLogPrefix + date.ToString("MMMM") + "%20" + date.ToString("yyyy") + "/" + date.ToString("yyyy-MM-dd");
            var chromeOptions = new ChromeOptions();
            // Headless
            chromeOptions.AddArgument("--headless");

            string chatLogRaw;

            // Load chrome
            using (var driver = new ChromeDriver(chromeOptions))
            {
                driver.Navigate().GoToUrl(url);
                // We ensure the element is loaded first
                var wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));
                wait.Until(d => d.FindElement(By.XPath("//*[@class='text']/span")));
                // Capture chat log
                chatLogRaw = driver.FindElementByXPath("//*[@class='text']/span").Text;
            }

            // We parse the chat log into messages.
            var chatLogRawLines = chatLogRaw.Split('[');

            var pattern = "\\[\\d\\d\\d\\d-\\d\\d-\\d\\d \\d\\d:\\d\\d:\\d\\d UTC\\] .*: .*";

            Regex regex = new Regex(pattern);
            var matches = regex.Matches(chatLogRaw);

            List<Message> messages = new List<Message>();

            Regex timeStampRegex = new Regex("\\d\\d\\d\\d-\\d\\d-\\d\\d \\d\\d:\\d\\d:\\d\\d");
            Regex userNameRegex = new Regex("\\d\\d UTC\\] .*:");
            Regex messageContentRegex = new Regex("\\d\\d UTC\\] .*: .*");
            foreach (var match in matches)
            {
                // Get timestamp, username and message content
                var timestampMatch = timeStampRegex.Match(match.ToString());
                var userNameMatch = userNameRegex.Match(match.ToString());
                var messageContentMatch = messageContentRegex.Match(match.ToString());
                if (timestampMatch.Success)
                {
                    DateTime timeStamp;
                    if (!DateTime.TryParseExact(timestampMatch.Value, "yyyy-MM-dd HH:mm:ss", new CultureInfo("en-UK"),
                        DateTimeStyles.None, out timeStamp))
                    {
                        throw new Exception($"DateTime was not interpreted properly. {timestampMatch.Value}");
                    }

                    // Trim out required regex pattern characters for username
                    string userName = userNameMatch.Value.Substring(8, (userNameMatch.Value.Length -9));
                    string messageContent =
                        messageContentMatch.Value.Substring(messageContentMatch.Value.IndexOf(':') + 2);

                    messages.Add(new Message(timeStamp, userName, messageContent));
                }
                else
                {
                    throw new Exception($"Failed to find timestamp in message. {match}");
                }
            }

            return new ChatLog(messages);
        }
    }
}
