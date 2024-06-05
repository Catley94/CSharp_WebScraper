using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace SeekNZScraper
{
    internal class Program
    {

        static void Main(string[] args)
        {
            string mainURL = "https://seek.co.nz";
            // Load the HTML from the URL
            string url = "https://www.seek.co.nz/developer-jobs/in-auckland"; // Replace with your URL
            var webClient = new WebClient();
            string html = webClient.DownloadString(url);

            // Parse the HTML using HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            int articleIndex = 0;

            // Find all article tags
            foreach (HtmlNode? article in doc.DocumentNode.SelectNodes("//article"))
            {
                // Find all h2 tags inside the article tag
                var jobTitles = article.SelectNodes(".//h3");

                
                // If there are any h3 tags, print out their text content
                if (jobTitles != null)
                {
                    foreach (HtmlNode jobTitle in jobTitles)
                    {
                        string _jobTitle = jobTitle.InnerText;
                        HtmlNodeCollection? _jobLinkPostFix = jobTitle.SelectNodes(".//a");
                        string fullJobLink = mainURL + _jobLinkPostFix?[0].GetAttributeValue("href", string.Empty);
                        Console.WriteLine(_jobTitle);
                        Console.WriteLine(fullJobLink);
                    }
                }
                articleIndex++;
            }

            Console.ReadLine();
            /*
                Outputs:
                Full Stack Developer
                Senior Full Stack Developer
                Full Stack Developer
                Junior Front End Developer
                Junior Software Developer
                Full Stack Software Developer
                Intermediate Swift Developer (Contract)
                WordPress Developer/ Designer
                Software Developer
                Intermediate Full Stack Developer
                Fullstack Developer (Node &amp; React)
                Junior Developer
                Front End Web Developer, Fashion &amp; Homewares Brand
                Senior Full Stack Developer
                React Full Stack Developer
                Full Stack Web Developer
                Seeking Savvy Backend Developer
                Fullstack Developer (&gt;= intermediate)
                Software Engineer
                Front End Developer
                Senior Software Developer (C++)
                Senior Full Stack Developer - React
             */
        }


    }
}
