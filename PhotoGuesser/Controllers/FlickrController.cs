using System.Diagnostics;
using FlickrNet;
using Microsoft.AspNetCore.Mvc;
using PhotoGuesser.Data;
using HtmlAgilityPack; 
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System;

namespace PhotoGuesser.Controllers;



[Route("api/[controller]/[action]")]
[ApiController]
public class FlickrController : ControllerBase
{
    private readonly FlickrNet.Flickr flickr;
    
    public FlickrController()
    {
        flickr = new FlickrNet.Flickr("cb1a986b128b33fede56de77f68293b7", "4fdae3558b204791");
    }
    
    [HttpGet]
    public async Task<ActionResult<List<PhotoDesc>>> GetImages(int quantity, [FromQuery]string[] searchStrings)
    {
        List<String> defaultSearchStrings = new List<string>()
        {
           "Street view", "New York street view", "Tokyo street view", "Ukraine street View", "City",
           "Grocery store", "Historic moment", "World War II", "Parade"
        };
        using (TextWriter writer = new StreamWriter("queries.txt", true))
        {
            foreach (var str in searchStrings)
            {
                if(!defaultSearchStrings.Contains(str))
                    await writer.WriteLineAsync(str);
            }
            writer.Close();
        }
        var photoDescsList = new List<PhotoDesc>();
        for(int i = 0; i < quantity; i++)
        {
            Photo selectedPhoto = new Photo();
            int yearTaken = -1;
            Stopwatch sw;
            sw = Stopwatch.StartNew();
            while (selectedPhoto.WebUrl.Equals("https://www.flickr.com/photos///"))
            {
                selectedPhoto = await GetPh(searchStrings);
                /*if(sw.ElapsedMilliseconds > 10000)
                {
                    selectedPhoto = new Photo();
                    TODO
                }*/
                Console.WriteLine(sw.ElapsedMilliseconds);
            }
            photoDescsList.Add(new PhotoDesc(selectedPhoto.WebUrl, selectedPhoto.LargeUrl, yearTaken));
        }
        return Ok(photoDescsList);
    }
    
    [HttpGet]
    public async Task<ActionResult<int>> GetYears(String webUrl)
    {
        return Ok(GetYearFromPage(webUrl));
    }

    internal async Task<Photo> GetPh(string[] searchStrings)
    {
        Random rnd = new Random();
        DateTime datetoday = DateTime.Now;
        int rndYear = rnd.Next(1970, datetoday.Year);
        DateTime generatedDate = new DateTime(rndYear, 1, 1);
        int rndArrIndex = rnd.Next(0, searchStrings.Length);
        
        var options = new PhotoSearchOptions { Text = searchStrings[rndArrIndex], MinTakenDate = new DateTime(1875, 1, 1), MaxTakenDate = generatedDate, 
            PerPage = 30, SortOrder = PhotoSearchSortOrder.Relevance, };
        
        var photos = new PhotoCollection();
        

        photos = await flickr.PhotosSearchAsync(options);
        if (photos.Pages < 1)
            return new Photo();
        options.Page = rnd.Next(0, photos.Pages - photos.Pages / 5);
        photos = await flickr.PhotosSearchAsync(options);

        return photos[rnd.Next(0,photos.Count)];
    }
    
    internal int GetYearFromPage(String url)
    {
        var web = new HtmlWeb(); 
        web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
        string dateTaken = "";

        var webPage = web.Load(url);

        var dateTakenHtmlNode = webPage.DocumentNode.QuerySelector("span.date-taken-label");
        if (dateTakenHtmlNode != null)
            dateTaken = dateTakenHtmlNode.InnerText;
        else
        {
            dateTakenHtmlNode = webPage.DocumentNode.QuerySelector("span.date-posted-label");
            if (dateTakenHtmlNode != null)
                dateTaken = dateTakenHtmlNode.InnerText;
        }
        
        if (dateTaken.Equals(""))
            return -1;
        
        string pattern = @"\d{4}";
        Regex rg = new Regex(pattern);
        var yearTakenRegEx = rg.Matches(dateTaken);
        StringBuilder yearStrTaken = new StringBuilder();
        foreach (Match ch in yearTakenRegEx)
        {
            yearStrTaken.Append(ch.Value);
        }
        
        return Convert.ToInt32(yearStrTaken.ToString()); 
    }
}