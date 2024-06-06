using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using HtmlAgilityPack;
using SeekNZScraper.Data;
using SeekNZScraper.Models;

namespace SeekNZScraper
{
    internal class Program
    {

        static void Main(string[] args)
        {

            //Offline use
            string filePath = "jobPages.json";
            List<string> htmlJobPages;

            List<Keyword> keywordsToLookOutFor = new Keywords().GetKeywords();

            int pageLimit = 1;
            int highlightKeywordsGreaterThanCount = 10;

            if (File.Exists(filePath))
            {
                // Load the list from the file
                string json = File.ReadAllText(filePath);
                htmlJobPages = JsonSerializer.Deserialize<List<string>>(json);
                Console.WriteLine("Loaded job pages from file.");
            }
            else
            {
                // Scrape the website to populate the list
                htmlJobPages = ScrapeJobPages(keywordsToLookOutFor, highlightKeywordsGreaterThanCount, pageLimit);
                Console.WriteLine("Scraped all job pages from the website.");

                // Save the list to the file
                string json = JsonSerializer.Serialize(htmlJobPages);
                File.WriteAllText(filePath, json);
            }

            if (htmlJobPages != null)
            {
                foreach (var htmlJobPage in htmlJobPages)
                {
                    ScrapeIndividualJobPage(htmlJobPage, keywordsToLookOutFor);
                }
            }

            DisplayScrapingResults(keywordsToLookOutFor, highlightKeywordsGreaterThanCount);
        }

        

        

        private static List<string> ScrapeJobPages(List<Keyword> keywordsToLookOutFor, int highlightKeywordsGreaterThanCount, int pageLimit)
        {

            //Scraping the Job Query page which holds all the job listings.

            List<string> individualJobPages = new List<string>();

            string domain = "https://seek.co.nz";
            string keyword = "developer";
            string location = "auckland";
            string pageQuery = "?page=";
            

            // Load the HTML from the URL
            string url = $"{domain}/{keyword}-jobs/in-{location}{pageQuery}";

            try
            {
                //Currently guess work as to how many pages.
                //When there isn't a page available, it will not have an "<article>" but a "<section>" with value of "No matching search results"
                for (int pageNum = 1; pageNum <= pageLimit; pageNum++)
                {
                    Console.WriteLine($"Page: {pageNum}");

                    WebClient webClient = new WebClient(); //Deprecated
                                                           //+ page number: https://seek.co.nz/developer-jobs/in-auckland?page=1
                    string mainHTMLJobQuery = webClient.DownloadString(url + pageNum);

                    // Parse the HTML using HtmlAgilityPack
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(mainHTMLJobQuery);
                    //Search for articles
                    HtmlNodeCollection? articles = doc.DocumentNode.SelectNodes("//article");

                    //Check if there are any articles, if not, there are no more pages to scrape
                    if (articles == null)
                    {
                        HtmlNodeCollection sections = doc.DocumentNode.SelectNodes("//section");
                        if (sections.Count > 0)
                        {
                            HtmlNodeCollection h3Nodes = doc.DocumentNode.SelectNodes(".//h3");
                            if (h3Nodes.Count > 0)
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
                            // Find all h3 tags inside the article tag
                            var jobTitles = article.SelectNodes(".//h3");


                            // If there are any h3 tags, print out their text content
                            if (jobTitles != null)
                            {
                                //Only ever going to be one job title per article... 
                                foreach (HtmlNode jobTitle in jobTitles)
                                {
                                    string _jobTitle = jobTitle.InnerText;
                                    HtmlNodeCollection? _jobLinkPostFix = jobTitle.SelectNodes(".//a");
                                    string fullJobLink = domain + _jobLinkPostFix?[0].GetAttributeValue("href", string.Empty);
                                    Console.WriteLine(_jobTitle);
                                    Console.WriteLine($"Page Link: {fullJobLink}");

                                    WebClient _webClient = new WebClient(); //Deprecated
                                    string htmlJobPage = webClient.DownloadString(fullJobLink);
                                    individualJobPages.Add(htmlJobPage);
                                }
                            }
                        }
                    }

                }
            }
            catch (LoopBreakException)
            {
                Console.WriteLine("Stopping scraper...");
            }
            
            return individualJobPages;
        }

        static void ScrapeIndividualJobPage(string htmlJobPage, List<Keyword> keywordsToLookOutFor)
        {

            // Parse the HTML using HtmlAgilityPack
            HtmlDocument _doc = new HtmlDocument();
            _doc.LoadHtml(htmlJobPage);


            foreach (HtmlNode? _section in _doc.DocumentNode.SelectNodes("//section"))
            {
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
            }
        }

        private static void DisplayScrapingResults(List<Keyword> keywordsToLookOutFor, int highlightKeywordsGreaterThanCount)
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
}
