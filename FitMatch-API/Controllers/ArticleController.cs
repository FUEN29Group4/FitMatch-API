using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack; // 你可能需要安装HtmlAgilityPack库
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.DataProtection.KeyManagement;


using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IDbConnection _db;

        public ArticleController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("announcement")]
        public async Task<IActionResult> Getannouncement()
        {
            const string sql = @"SELECT * FROM Article WHERE ArticleTypeName = 'announcement'";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var announcement = multi.Read<Article>().ToList();
                // 基本驗證，確保資料存在
                if (announcement == null)
                {
                    return NotFound("No data found");
                }
                return Ok(announcement);
            }
        }
        // GET api/<announcementController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Getannouncement(int id)
        {
            const string sql = @"SELECT * FROM Article WHERE ArticleID = @ArticleID";
            var parameters = new { ArticleID = id };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var announcement = multi.Read<Article>().FirstOrDefault();
                // 基本驗證，確保資料存在
                if (announcement == null)
                {
                    return NotFound("No data found");
                }
                return Ok(announcement);
            }
        }

        [HttpGet("healthy")]
        public async Task<IActionResult> Gethealthy()
        {
            try
            {
                string url = "https://newsapi.org/v2/everything?q=%E5%81%A5%E5%BA%B7&searchIn=title&language=zh&from=2023-09-15&sortBy=publishedAt&apiKey=5aa14d4235a64247940a4418047a5153";
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("user-agent", "News-API-csharp/0.1");
                //using (HttpClient client = new HttpClient())
                //{
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    // 解析JSON数据
                    var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(jsonContent);

                    // 检查响应中是否有文章
                    if (newsResponse.articles != null)
                    {
                        return Ok(newsResponse.articles);
                    }
                    else
                    {
                        return NotFound("沒有找到新聞文章");
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "無法獲取新聞數據");
                }
             
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"內部服務器錯誤: {ex.Message}");
            }
        }

        [HttpGet("sport")]
        public async Task<IActionResult> Getsport()
        {
            try
            {
                string url = "https://newsapi.org/v2/top-headlines?category=sport&country=tw&apiKey=5aa14d4235a64247940a4418047a5153";
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("user-agent", "News-API-csharp/0.1");
                //using (HttpClient client = new HttpClient())
                //{
                HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();

                        // 解析JSON数据
                        var newsResponse = JsonConvert.DeserializeObject<NewsApiResponse>(jsonContent);

                        // 检查响应中是否有文章
                        if (newsResponse.articles != null)
                        {
                            return Ok(newsResponse.articles);
                        }
                        else
                        {
                            return NotFound("沒有找到新聞文章");
                        }
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, "無法獲取新聞數據");
                    }
                //}
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"內部服務器錯誤: {ex.Message}");
            }
        }


        [HttpGet("titles")]
        public IActionResult GetNewsTitles()
        {
            try
            {
                List<string> chineseTitles = GetGoogleNewsTitles();
                return Ok(chineseTitles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"内部服务器错误: {ex.Message}");
            }
        }



        // POST api/<ArticleController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ArticleController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ArticleController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        public class NewsApiResponse
        {
            public string status { get; set; }
            public int totalResults { get; set; }
            public List<ArticleNews> articles { get; set; }
        }

        public class ArticleNews
        {
            public string title { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            // 添加其他你需要的属性
        }
        private List<string> GetGoogleNewsTitles()
        {
                ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless"); // 启用无头模式

            using (var driver = new ChromeDriver(chromeOptions))
            {


                driver.Navigate().GoToUrl("https://news.google.com/home?hl=zh-TW&gl=TW&ceid=TW:zh-Hant");


                // 模擬滾動，以便加載更多內容
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
   


                // 使用等待确保页面加载完成
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                wait.Until(d => d.FindElement(By.CssSelector(".MCAGUe")));
                wait.Until(d => d.FindElement(By.CssSelector(".IL9Cne")));
                wait.Until(d => d.FindElement(By.CssSelector(".B6pJDd")));
                wait.Until(d => d.FindElement(By.CssSelector("h4")));
                wait.Until(d => d.FindElement(By.CssSelector(".JtKRv.iTin5e")));


                // 初始化一个空列表来存储所有标题
                var allTitles = new List<string>();

                // 定义多个 CSS 选择器
                string[] selectors = {"h4" };

                foreach (var selector in selectors)
                {
                    // 获取当前选择器对应的元素列表
                    var titleElements = driver.FindElements(By.CssSelector(selector));

                    foreach (var titleElement in titleElements)
                    {
                        allTitles.Add(titleElement.Text);
                    }
                }

                return allTitles;
            }
        }
    }
}
