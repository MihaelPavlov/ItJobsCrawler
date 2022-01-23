namespace ItJobsCrawler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using AngleSharp.Html.Parser;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write(Constants.StartMessage);
            string searchLanguage = Console.ReadLine();

            var listWithJobs = start(searchLanguage);

            var months = GetMonths();

            string projectDir =
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..")) + @"\RawData\";

            using (var writer = new StreamWriter(projectDir + $"{DateTime.UtcNow:yyyy-MM-dd}.txt"))
            {
                writer.WriteLine($"All junior jobs for {searchLanguage.ToLower()} ! {string.Format(Constants.JobsCountMessage, listWithJobs.Count())}");
                foreach (var item in listWithJobs.OrderBy(x => x.Мonth).ThenByDescending(x => x.Date))
                {
                    var monthName = months.FirstOrDefault(x => x.Value == item.Мonth).Key;
                    writer.WriteLine($"{item.Date} - {monthName},{item.Name},{item.Link}");
                }
            }
        }

        private static IEnumerable<Job> start(string searchLanguage)
        {
            var parser = new HtmlParser();
            var webClient = new WebClient { Encoding = Encoding.UTF8 };

            var startFrom = 1;
            var counter = 0;

            var months = GetMonths();

            var listWithJobs = new List<Job>();

            while (true)
            {
                try
                {
                    var webPageContent = webClient.DownloadString(string.Format(Constants.DevBgJobsJuniorInterLink, startFrom));
                    var document = parser.ParseDocument(webPageContent);
                    var links = document.QuerySelectorAll(".title-date-wrap").Where(x => x.FirstElementChild.TextContent.ToLower().Contains(searchLanguage.ToLower()));

                    foreach (var x in links)
                    {
                        listWithJobs.Add(new Job() { Link = x.ParentElement.FirstElementChild.GetAttribute(Constants.Href), Name = x.FirstElementChild.TextContent, Date = int.Parse(x.LastElementChild.TextContent.Split(" ")[0]), Мonth = months[x.LastElementChild.TextContent.Split(" ")[1]] });

                        counter++;
                    }

                    // increase page number
                    startFrom++;
                }
                catch (Exception ex) // when they are no more pages
                {
                    Console.WriteLine(Constants.FinalMessage + ex.Message);
                    Console.WriteLine(string.Format(Constants.JobsCountMessage, counter));
                    break;
                }
            }

            return listWithJobs;
        }

        private static IDictionary<string, int> GetMonths()
        {
            return new Dictionary<string, int>()
            {
                { "ян.",1 },
                { "февр.",2 },
                { "март",3 },
                { "апр.",4 },
                { "май",5 },
                { "юни",6 },
                { "юли",7 },
                { "авг.",8 },
                { "септ.",9 },
                { "окт.",10 },
                { "ноем.",11},
                { "дек.",12},
            };
        }
    }
}
