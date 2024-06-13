﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HtmlAgilityPack;
using SeekNZScraper.Data;
using SeekNZScraper.Models;

namespace SeekNZScraper
{
    internal class Program
    {

        static void Main(string[] args)
        {

            string role = "developer";
            string domain = "https://seek.co.nz"; // https://uk.indeed.com

            //Offline use
            DateTime date = DateTime.Now.Date;
            string filePath = $"jobPages-{date.ToString("dd-MM-yyyy-HH-mm-ss")}.json";

            JsonSaveData? saveData;
            List<string> htmlJobPages = new List<string>();
            List<string> urls = new List<string>();

            List<Keyword> keywordsToLookOutFor = new Keywords().GetKeywords();

            int pageLimit = 100;
            int highlightKeywordsGreaterThanCount = 10;

            if (File.Exists(filePath))
            {
                // Load the list from the file
                string json = File.ReadAllText(filePath);
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    IncludeFields = true
                };
                saveData = JsonSerializer.Deserialize<JsonSaveData>(json, options);
                if(saveData != null)
                {
                    htmlJobPages = saveData.HtmlStrings;
                    urls = saveData.Urls;
                }
                ConsoleWriteWithColour("Loaded job pages from file.", ConsoleColor.Green);
            }
            else
            {
                
                ConsoleWriteWithColour("Scraping pages.", ConsoleColor.Blue);
                // Scrape the website to populate the list
                htmlJobPages = ScrapeJobPages(role, domain, keywordsToLookOutFor, highlightKeywordsGreaterThanCount, pageLimit, out urls);
                ConsoleWriteWithColour("Scraped all job pages from the website.", ConsoleColor.Blue);

                saveData = new JsonSaveData(htmlJobPages, urls);

                try 
                {
                    // Save the list to the file
                    string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Serialization failed: {ex.Message}");
                }
                
            }

            if (htmlJobPages != null)
            {
                for (short i = 0; i < htmlJobPages.Count - 1; i++)
                {
                    ScrapeIndividualJobPage(htmlJobPages[i], keywordsToLookOutFor, urls[i]);
                }
            }

            // Generate Excel file
            GenerateExcelFile(htmlJobPages, keywordsToLookOutFor);

            DisplayScrapingResults(keywordsToLookOutFor, highlightKeywordsGreaterThanCount);
        }

        private static List<string> ScrapeSeekNZJobPages(string role, string domain, List<Keyword> keywordsToLookOutFor, int highlightKeywordsGreaterThanCount, int pageLimit, out List<string> urls)
        {
            //Scraping the Job Query page which holds all the job listings.
            urls = new List<string>();
            List<string> individualJobPages = new List<string>();


            string location = "auckland";
            string pageQuery = "?page=";


            // Load the HTML from the URL
            string url = $"{domain}/{role}-jobs/in-{location}{pageQuery}";

            try
            {
                //Currently guess work as to how many pages.
                //When there isn't a page available, it will not have an "<article>" but a "<section>" with value of "No matching search results"
                for (int pageNum = 1; pageNum <= pageLimit; pageNum++)
                {
                    ConsoleWriteWithColour($"Page: {pageNum}", ConsoleColor.Yellow);

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
                                        ConsoleWriteWithColour("No more pages. Exiting...", ConsoleColor.Blue);
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

                                    ConsoleWriteWithColour(_jobTitle, ConsoleColor.Cyan);
                                    ConsoleWriteWithColour($"Page Link: {fullJobLink}", ConsoleColor.Green);

                                    WebClient _webClient = new WebClient(); //Deprecated
                                    string htmlJobPage = webClient.DownloadString(fullJobLink);
                                    individualJobPages.Add(htmlJobPage);
                                    urls.Add(fullJobLink);
                                }
                            }
                        }
                    }

                }
            }
            catch (LoopBreakException)
            {
                ConsoleWriteWithColour("Stopping scraper...", ConsoleColor.Red);
            }
            return individualJobPages;
        }

        private static List<string> ScrapeIndeedUKJobPages(string role, string domain, List<Keyword> keywordsToLookOutFor, int highlightKeywordsGreaterThanCount, int pageLimit, out List<string> urls)
        {
            //Scraping the Job Query page which holds all the job listings.
            urls = new List<string>();
            List<string> individualJobPages = new List<string>();


            string location = "London";
            string pageQuery = "&start=";

            // Load the HTML from the URL
            string url = $"{domain}/jobs?q={role}&l={location}{pageQuery}";

            //Pages: &start=0 = page 1, &start=10 = page 2, &start=20 = page 3, &start=30 = page4
            try
            {
                //Currently guess work as to how many pages.
                //When there isn't a page available, it will not have an "<article>" but a "<section>" with value of "No matching search results"
                for (int pageNum = 0; pageNum <= pageLimit * 10; pageNum += 10) //pageLimit = 50 * 10 = 500 = 51 pages
                {
                    ConsoleWriteWithColour($"Page: {pageNum}", ConsoleColor.Yellow);

                    WebClient webClient = new WebClient(); //Deprecated
                                                           //+ page number: https://seek.co.nz/developer-jobs/in-auckland?page=1
                    string mainHTMLJobQuery = webClient.DownloadString(url + pageQuery + pageNum);

                    // Parse the HTML using HtmlAgilityPack
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(mainHTMLJobQuery);

                    //Search for articles
                    HtmlNodeCollection? jobListings = doc.DocumentNode.SelectNodes("//ul");


                    //Check if there are any articles, if not, there are no more pages to scrape
                    if (jobListings == null)
                    {

                        foreach (var jobListing in jobListings)
                        {
                            if (jobListing.HasClass("eu4oa1w0"))
                            {
                                //Perhaps instead of this,see if jobListings can be filtered to return only with a HasClass("eu4oa1w0");

                                //That will allow me to loop through the correct <li> tags.
                            }
                        }


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
                                        ConsoleWriteWithColour("No more pages. Exiting...", ConsoleColor.Blue);
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

                                    ConsoleWriteWithColour(_jobTitle, ConsoleColor.Cyan);
                                    ConsoleWriteWithColour($"Page Link: {fullJobLink}", ConsoleColor.Green);

                                    WebClient _webClient = new WebClient(); //Deprecated
                                    string htmlJobPage = webClient.DownloadString(fullJobLink);
                                    individualJobPages.Add(htmlJobPage);
                                    urls.Add(fullJobLink);
                                }
                            }
                        }
                    }
                }
            }
            catch (LoopBreakException)
            {
                ConsoleWriteWithColour("Stopping scraper...", ConsoleColor.Red);
            }
            return individualJobPages;
        }

        private static List<string> ScrapeJobPages(string role, string domain, List<Keyword> keywordsToLookOutFor, int highlightKeywordsGreaterThanCount, int pageLimit, out List<string> urls)
        {
            //Scraping the Job Query page which holds all the job listings.
            urls = new List<string>();

            switch (domain)
            {
                case "https://seek.co.nz":
                    return ScrapeSeekNZJobPages(role, domain, keywordsToLookOutFor, highlightKeywordsGreaterThanCount, pageLimit, out urls);
                case "https://uk.indeed.com":
                    return ScrapeIndeedUKJobPages(role, domain, keywordsToLookOutFor, highlightKeywordsGreaterThanCount, pageLimit, out urls);
            }
            return new List<string>();
        }

        static void ScrapeIndividualJobPage(string htmlJobPage, List<Keyword> keywordsToLookOutFor, string url)
        {

            // Parse the HTML using HtmlAgilityPack
            HtmlDocument _doc = new HtmlDocument();
            _doc.LoadHtml(htmlJobPage);

            HtmlNodeCollection? h1s = _doc.DocumentNode.SelectNodes(".//h1");
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
                            if (_li != null)
                            {
                                foreach (var _keyword in keywordsToLookOutFor)
                                {
                                    if (_li.InnerText.Contains(_keyword.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!_keyword.HtmlStrings.Contains(htmlJobPage))
                                        {
                                            _keyword.HtmlStrings.Add(htmlJobPage);
                                            _keyword.Urls.Add(url);
                                            _keyword.IncrementCount();
                                            if (h1s != null)
                                            {
                                                _keyword.JobNames.Add(h1s[0].InnerText);
                                                //foreach (HtmlNode? _h1 in h1s)
                                                //{
                                                //    if (_h1 != null)
                                                //    {
                                                //        _keyword.JobNames.Add(_h1.InnerText);
                                                //    }
                                                //}
                                            }
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

            // Sort the keywords by Count in descending order
            keywordsToLookOutFor = keywordsToLookOutFor.OrderByDescending(k => k.Count).ToList();

            List<Keyword> keywords = new List<Keyword>();

            foreach (var _keyword in keywordsToLookOutFor)
            {
                if(_keyword.Count > 0) keywords.Add(_keyword);
            }

            int maxLength = keywordsToLookOutFor.Max(k => k.Name.Length);

            ConsoleWriteWithColour("---- Now displaying a summary ----", ConsoleColor.Green);
            
            foreach (var _keyword in keywords)
            {
                string compiledString = $"{_keyword.Name.PadRight(maxLength)}: ";
                for (int i = 0; i < _keyword.Count; i++)
                {
                    compiledString += i > 0 && (i + 1) % 5 == 0 ? "|" : "-";
                    if (i == _keyword.Count - 1) compiledString += $" " + _keyword.Count;
                }
                ConsoleWriteWithColour(compiledString, ConsoleColor.Cyan);
            }


        }

        static void GenerateExcelFile(List<string> jobPages, List<Keyword> keywordsToLookOutFor)
        {
            DateTime date = DateTime.Now.Date;


            string excelFilePath = $"JobPages-{date.ToString("dd-MM-yyyy-HH-mm-ss")}.xlsx";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Job Pages");

                // Add headers
                worksheet.Cell(1, 1).Value = "Keyword";
                worksheet.Cell(1, 2).Value = "Count";
                worksheet.Cell(1, 3).Value = "Job Titles";
                worksheet.Cell(1, 4).Value = "URLs";

                

                // Sort the keywords by Count in descending order
                keywordsToLookOutFor = keywordsToLookOutFor.OrderByDescending(k => k.Count).ToList();

                // Add data to worksheet
                int row = 2;
                foreach (var keyword in keywordsToLookOutFor)
                {
                    worksheet.Cell(row, 1).Value = keyword.Name;
                    worksheet.Cell(row, 2).Value = keyword.Count;

                    for (short i = 0; i < keyword.Urls.Count - 1; i++)
                    {
                        worksheet.Cell(row, 3).Value = keyword.JobNames[i];
                        worksheet.Cell(row, 4).Value = keyword.Urls[i];
                        row++;
                    }

                    row++;
                }

                // Save the workbook
                workbook.SaveAs(excelFilePath);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Excel file '{excelFilePath}' has been generated successfully.");
            Console.ResetColor();
        }

        private static void ConsoleWriteWithColour(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
}
