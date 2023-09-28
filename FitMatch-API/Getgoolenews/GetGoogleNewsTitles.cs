using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace FitMatch_API.GetGoogleNews
{
    public class GoogleNewsScraper
    {
        public string GetGoogleNewsTitles()
        {
            using (var driver = new ChromeDriver())
            {
                driver.Navigate().GoToUrl("https://news.google.com/");

                // 使用等待确保页面加载完成
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.CssSelector(".xrnccd")));

                // 获取所有标题
                var titleElements = driver.FindElements(By.CssSelector(".xrnccd"));
                var titles = new List<string>();

                foreach (var titleElement in titleElements)
                {
                    titles.Add(titleElement.Text);
                }

                return string.Join(Environment.NewLine, titles);
            }
        }

    }
}
