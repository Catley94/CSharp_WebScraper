using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using HtmlAgilityPack;
using SeekNZScraper.Models;

namespace SeekNZScraper
{
    internal class Program
    {

        static void Main(string[] args)
        {



            List<Keyword> keywordsToLookOutFor = new List<Keyword>() {
                new Keyword("C#"),
                new Keyword("C++"),
                new Keyword("Cpp"),
                new Keyword("Node"),
                new Keyword("NextJS"),
                new Keyword("NuxtJS"),
                new Keyword("Flutter"),
                new Keyword("Python"),
                new Keyword("HTML"),
                new Keyword("CSS"),
                new Keyword("SASS"),
                new Keyword("BootStrap"),
                new Keyword("TailwindCSS"),
                new Keyword("JavaScript"),
                new Keyword("TypeScript"),
                new Keyword("jQuery"),
                new Keyword("React"),
                new Keyword("React Native"),
                new Keyword("Vue"),
                new Keyword("Angular"),
                new Keyword("RxJS"),
                new Keyword("Ruby"),
                new Keyword("Docker"),
                new Keyword("Kubernetes"),
                new Keyword("Azure"),
                new Keyword("Google Cloud"),
                new Keyword("AWS"),
                new Keyword("Microservices"),
                new Keyword("MariaDB"),
                new Keyword("SQL"),
                new Keyword("PostgreSQL"),
                new Keyword("MySQL"),
                new Keyword("T-SQL"),
                new Keyword("Mongo"),
                new Keyword("GoLang"),
                new Keyword("Java"),
                new Keyword("Springboot"),
                new Keyword(".NET"),
                new Keyword("MVC.NET"),
                new Keyword("C3.NET"),
                new Keyword("ASP.NET"),
                new Keyword("Entity Framework"),
                new Keyword("Signal R"),
                new Keyword("PHP"),
                new Keyword("RESTFUL"),
                new Keyword("GraphQL"),
                new Keyword("Git"),
                new Keyword("Cypress"),
                new Keyword("RabbitMQ"),
                new Keyword("CI/CD"),
            };


            string domain = "https://seek.co.nz";
            string keyword = "developer";
            string location = "auckland";
            string pageQuery = "?page=";
            int pageLimit = 50;
            int highlightKeywordsGreaterThanCount = 10;
            // Load the HTML from the URL
            string url = $"{domain}/{keyword}-jobs/in-{location}{pageQuery}"; // Replace with your URL

            try 
            {
                //Currently guess work as to how many pages.
                //When there isn't a page available, it will not have an "<article>" but a "<section>" with value of "No matching search results"
                for (int i = 1; i <= pageLimit; i++)
                {
                    LoopThroughPage(url, i, domain, keywordsToLookOutFor);
                }
            }
            catch(LoopBreakException)
            {
                Console.WriteLine("Stopping scraper...");
            }
            finally
            {
                //TODO
                //Output data into PDF or Excel spread sheet?
                Console.WriteLine("---- Now displaying a summary ----");
                foreach (var _keyword in keywordsToLookOutFor)
                {
                    if (_keyword.Count > highlightKeywordsGreaterThanCount)
                    {
                        Console.WriteLine($"{_keyword.Name}: {_keyword.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"{_keyword.Name}: {_keyword.Count}");
                    }
                }

                int maxLength = keywordsToLookOutFor.Max(k => k.Name.Length);

                Console.WriteLine("---- Now displaying a summary ----");
                foreach (var _keyword in keywordsToLookOutFor)
                {
                    string compiledString = $"{_keyword.Name.PadRight(maxLength)}: ";
                    for (int i = 0; i < _keyword.Count; i++)
                    {
                        compiledString += i % 5 == 0 && i != 0 ? "|" : "-";
                    }
                    Console.WriteLine(compiledString);
                }
            }

            
        }

        static bool HasKeywordBeenTriggered(string keyword, List<string> alreadyCountedKeywords)
        {
            foreach (var alreadyCountedKeyword in alreadyCountedKeywords)
            {
                if (keyword == alreadyCountedKeyword) return true;
            }

            return false;
        }

        static void LoopThroughPage(string url, int page, string domain, List<Keyword> keywordsToLookOutFor)
        {
            WebClient webClient = new WebClient(); //Deprecated
            string mainHTMLJobQuery = webClient.DownloadString(url + page);
            Console.WriteLine($"Page: {page}");

            // Parse the HTML using HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(mainHTMLJobQuery);
            int articleIndex = 0;

            HtmlNodeCollection? articles = doc.DocumentNode.SelectNodes("//article");
            if (articles == null)
            {
                HtmlNodeCollection sections = doc.DocumentNode.SelectNodes("//section");
                if (sections.Count > 0)
                {
                    HtmlNodeCollection h3Nodes = doc.DocumentNode.SelectNodes(".//h3");
                    if(h3Nodes.Count > 0)
                    {
                        foreach (var h3 in h3Nodes)
                        {
                            if (h3.InnerText.Contains("No matching", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("No more pages. Exiting...");
                                throw new LoopBreakException();
                            }
                            else
                            {
                                Console.Error.WriteLine("There is an h3, but may have changed their wording. Exiting...");
                                throw new LoopBreakException();
                            }
                        }
                    } 
                    else
                    {
                        Console.Error.WriteLine("There are no articles, but there is a section, but no h3. Exiting...");
                        throw new LoopBreakException();
                    }
                }
                else
                {
                    Console.Error.WriteLine("There are no articles or sections found on the page. Exiting...");
                    throw new LoopBreakException();
                }
            } 
            else
            {
                // Find all article tags
                foreach (HtmlNode? article in articles)
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
                            Console.WriteLine($"Page Link: {fullJobLink}");

                            WebClient _webClient = new WebClient(); //Deprecated
                            string htmlJobPage = webClient.DownloadString(fullJobLink);

                            // Parse the HTML using HtmlAgilityPack
                            HtmlDocument _doc = new HtmlDocument();
                            _doc.LoadHtml(htmlJobPage);


                            foreach (HtmlNode? _section in _doc.DocumentNode.SelectNodes("//section"))
                            {
                                List<string> alreadyCountedKeywords = new List<string>();
                                HtmlNodeCollection? unorderedLists = _section.SelectNodes(".//ul");
                                if (unorderedLists != null)
                                {
                                    foreach (HtmlNode? _ul in unorderedLists)
                                    {
                                        HtmlNodeCollection? _lis = _ul.SelectNodes(".//li");
                                        foreach (HtmlNode? _li in _lis)
                                        {
                                            List<string> keywordsToAdd = new List<string>();
                                            if (_li != null)
                                            {
                                                foreach (var _keyword in keywordsToLookOutFor)
                                                {
                                                    if (_li.InnerText.Contains(_keyword.Name, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        if (!_keyword.urls.Contains(htmlJobPage))
                                                        {
                                                            _keyword.urls.Add(htmlJobPage);
                                                            _keyword.IncrementCount();
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                                alreadyCountedKeywords.Clear();
                            }
                        }
                    }
                    articleIndex++;
                }
            }            
        }
    }
}
