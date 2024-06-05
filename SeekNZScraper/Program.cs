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
            string domain = "https://seek.co.nz";
            string keyword = "developer";
            string location = "auckland";
            // Load the HTML from the URL
            string url = $"{domain}/{keyword}-jobs/in-{location}"; // Replace with your URL
            WebClient webClient = new WebClient(); //Deprecated
            string mainHTMLJobQuery = webClient.DownloadString(url);

            // Parse the HTML using HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(mainHTMLJobQuery);
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
                        string fullJobLink = domain + _jobLinkPostFix?[0].GetAttributeValue("href", string.Empty);
                        Console.WriteLine(_jobTitle);
                        Console.WriteLine(fullJobLink);

                        WebClient _webClient = new WebClient(); //Deprecated
                        string htmlJobPage = webClient.DownloadString(fullJobLink);

                        // Parse the HTML using HtmlAgilityPack
                        HtmlDocument _doc = new HtmlDocument();
                        doc.LoadHtml(htmlJobPage);

                        //foreach (HtmlNode? _article in doc.DocumentNode.SelectNodes("//article"))
                        //{

                        //}
                    }
                }
                articleIndex++;
            }

            Console.ReadLine();
            /*
                Outputs:
                https://seek.co.nz/job/76371378?type=promoted&amp;ref=search-standalone&amp;origin=cardTitle
                Full Stack Developer
                https://seek.co.nz/job/76374214?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Junior Front End Developer
                https://seek.co.nz/job/76373052?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Junior Software Developer
                https://seek.co.nz/job/76335968?type=standard&amp;ref=search-standalone&amp;origin=cardTitle
                Full Stack Software Developer
                https://seek.co.nz/job/76183228?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Intermediate Swift Developer (Contract)
                https://seek.co.nz/job/76371832?type=standard&amp;ref=search-standalone&amp;origin=cardTitle
                WordPress Developer/ Designer
                https://seek.co.nz/job/76378821?type=standard&amp;ref=search-standalone&amp;origin=cardTitle
                Software Developer
                https://seek.co.nz/job/76215739?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Intermediate Full Stack Developer
                https://seek.co.nz/job/76296969?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Fullstack Developer (Node &amp; React)
                https://seek.co.nz/job/76389005?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Junior Developer
                https://seek.co.nz/job/75993807?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Front End Web Developer, Fashion &amp; Homewares Brand
                https://seek.co.nz/job/76231343?type=standard&amp;ref=search-standalone&amp;origin=cardTitle
                Senior Full Stack Developer
                https://seek.co.nz/job/76371378?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                React Full Stack Developer
                https://seek.co.nz/job/76239523?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Full Stack Web Developer
                https://seek.co.nz/job/76223506?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Seeking Savvy Backend Developer
                https://seek.co.nz/job/76207866?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Fullstack Developer (&gt;= intermediate)
                https://seek.co.nz/job/76154462?type=standard&amp;ref=search-standalone&amp;origin=cardTitle
                Software Engineer
                https://seek.co.nz/job/76279249?type=standard&amp;ref=search-standalone&amp;origin=cardTitle
                Front End Developer
                https://seek.co.nz/job/76101126?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Senior Software Developer (C++)
                https://seek.co.nz/job/76366838?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
                Senior Full Stack Developer - React
                https://seek.co.nz/job/76365641?type=standout&amp;ref=search-standalone&amp;origin=cardTitle
             */
        }


    }
}
