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





// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {

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
                //}
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"內部服務器錯誤: {ex.Message}");
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
            public List<Article> articles { get; set; }
        }

        public class Article
        {
            public string title { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            // 添加其他你需要的属性
        }

    }
}
