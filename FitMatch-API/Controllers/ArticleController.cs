﻿using FitMatch_API.Models;
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
using System.Xml;



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
                string url = "https://newsapi.org/v2/everything?q=%E5%8F%B0%E7%81%A3&searchIn=title&language=zh&sortBy=publishedAt&apiKey=5aa14d4235a64247940a4418047a5153";
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
        public IActionResult GetNewsTitles(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest("URL 參數不能為空。");
                }

                List<string> chineseTitles = GetGoogleNewsTitles(url);
                return Ok(chineseTitles);
            }
            catch (Exception ex)
            {
                // 记录异常堆栈信息
                Console.WriteLine(ex.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, $"內部服務器錯誤: {ex.Message}");
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
        private List<string> GetGoogleNewsTitles(string url)
        {
            ChromeOptions chromeOptions = new ChromeOptions();

            // 设置 User-Agent
            chromeOptions.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36"); // 替换成你想要的用户代理字符串

            chromeOptions.AddArgument("--headless"); // 启用无头模式

            using (var driver = new ChromeDriver(chromeOptions))
            {
                driver.Navigate().GoToUrl(url);

                // 模擬滾動，以便加載更多內容
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");


                // 使用等待确保页面加载完成
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(d => d.FindElement(By.CssSelector("h1")));
                wait.Until(d => d.FindElement(By.CssSelector("img")));
                wait.Until(d => d.FindElement(By.CssSelector("p")));
                //wait.Until(d => d.FindElement(By.CssSelector("h4")));
                //wait.Until(d => d.FindElement(By.CssSelector(".JtKRv.iTin5e")));


                // 初始化一个空列表来存储所有标题
                var allTitles = new List<string>();

                // 定义多个 CSS 选择器
                string[] selectors = { "h1", "p", "img" };
                bool foundFirstImage = false; // 添加一个标志变量

                foreach (var selector in selectors)
                {
                    // 获取当前选择器对应的元素列表
                    var elements = driver.FindElements(By.CssSelector(selector));
                    // 使用 XPath 选择标签为 <h1> 的元素
                    //elements = driver.FindElements(By.XPath("//h1"));
                    var imageCount = 0;
                    //var titleCount = 0;
                    ////使用 XPath 选择仅包含纯文本内容的<p> 元素
                    //elements = driver.FindElements(By.XPath("//p[not(*)]"));


                    foreach (var element in elements)
                    {

                        if (selector == "h1")
                        {
                            // 对于<h1>元素，获取文本内容，并添加 "title:" 前缀
                                string h1Text = element.Text;
                                if (!string.IsNullOrEmpty(h1Text))
                                {
                                    allTitles.Add($"title: {h1Text}");
                                }
                            //titleCount++;

                            //// 如果计数器等于3，表示找到第三张图像
                            //if (titleCount == 5)
                            //{
                               
                            //    break; // 退出内部的 foreach 循环
                            //}
                        }
                        else if (selector == "p")
                        {

                            var classAttribute = element.GetAttribute("class");

                            // 如果 class 属性为空或只包含空格，表示没有其他类别样式
                            if (string.IsNullOrWhiteSpace(classAttribute))
                            {
                                // 获取 <p> 元素的文本内容，并添加到列表中
                                string pText = element.Text;
                                if (!string.IsNullOrEmpty(pText))
                                {
                                    allTitles.Add($"contentText: {pText}");
                                }
                            }
                        }
                        else if (selector == "img")
                        {
                            //string src = element.GetAttribute("src");
                            //allTitles.Add($"src: {src}");
                            // 如果找到<img>元素，增加计数器
                            imageCount++;

                            // 如果计数器等于3，表示找到第三张图像
                            if (imageCount == 4)
                            {
                                string src = element.GetAttribute("src");
                                if (!string.IsNullOrEmpty(src))
                                {
                                    allTitles.Add($"src: {src}");
                                }
                                break; // 退出内部的 foreach 循环
                            }




                            //// 对于<img>元素，获取src属性并添加到列表，然后停止遍历
                            //string src = element.GetAttribute("src");
                            //if (!string.IsNullOrEmpty(src))
                            //{
                            //    allTitles.Add($"src: {src}");
                            //    foundFirstImage = true; // 设置标志变量为 true，表示已经找到第一个图片
                            //    break; // 退出内部的 foreach 循环
                            //}
                        }
                        else
                        {
                            // 对于其他元素，获取文本内容
                            allTitles.Add(element.Text);
                        }
                    }
                }


                return allTitles;
            }
        }
    }
}
