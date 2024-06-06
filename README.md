## Web Scraper for Seek.co.nz

<img width="648" alt="image" src="https://github.com/Catley94/CSharp_WebScraper/assets/36040041/b391bd88-15d8-4f21-b195-282c588d7a12">
<img width="602" alt="image" src="https://github.com/Catley94/CSharp_WebScraper/assets/36040041/63e5a2d0-3afa-455d-996f-8fc33a63b596">

This is a simple web scraper specifically made for Seek.co.nz, using the role as "Developer" in "Auckland", however both the role and location are changable in code.

It was made specifically to keep track of the tech being used in NZ so I can make sure my skills are appealing to them when it's our time to move over there.

It will initally scrape the each page with the job listings, going into each job posting, scraping that page and looking specifically for keywords defined in "Keywords.cs".

If it finds any keywords, it adds the job listing URL to that keyword, and makes sure that if that same keyword has been mentioned twice in the page, that it will not count a second for the same keyword.

To save the traffic on that website (as this wasn't meant to be malicious), after initially an successfully scraping, it saves all html strings of the individual job postings, to a .json file, when you initially open the program, 
it will look for that .json file and load the data through it, rather than re-scraping the website. 

Also, as Seek.co.nz has no easy way of determining how many pages are available for this query, you have to manually set the page limit, however if the page limit is higher than what is actually available (pageLimit = 50, but there are only 27 pages),
it already checks if there are any job postings on that page, if not, the it assumes that's all the pages accounted for and will give you the summary of how many times keywords have been mentioned.

As a summary, it displays a tally graph to easily see which is more popular, and there is also a graph above with actual numbers.

As mentioned above, this is not meant to be a malicious program which is heavy on traffic, if Seek.co.nz approach me about this project and request me to make this a private repo, or take it down, I will do so. 

There is room to add more than one job site hear, though FYI from my experience of scraping, each website can be different, so the logic of scraping that page will be unqiue, perhaps something a switch/case statement would be useful for. 
