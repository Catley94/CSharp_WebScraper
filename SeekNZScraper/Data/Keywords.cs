using SeekNZScraper.Models;

namespace SeekNZScraper.Data
{
    public class Keywords
    {
        private List<Keyword> keywordsToLookOutFor = new List<Keyword>() {
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
                new Keyword("CI/CD")
        };

        public List<Keyword> GetKeywords()
        {
            return this.keywordsToLookOutFor;
        }
    }
}
