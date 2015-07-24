using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Remote;
using System;
using NUnit.Framework;
using Shouldly;
using System.Diagnostics;

namespace AppiumSample
{
    [TestFixture]
    public class UnitTest1
    {
        public OpenQA.Selenium.Appium.Android.AndroidDriver<AndroidElement> driver;
        private int sessionCounter = 3;
        private Action<int> CheckIcon;
        [SetUp]
        public void BeforeAll()
        {
            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("device", "Android");
            capabilities.SetCapability(CapabilityType.Platform, "Windows");
            capabilities.SetCapability("deviceName", "Commbank Tablet(SM-T5");
            capabilities.SetCapability("platformName", "Android");
            capabilities.SetCapability("platformVersion", "4.4");
            capabilities.SetCapability("appPackage", "com.cba.android.netbank.debug");
            capabilities.SetCapability("appActivity", "com.commbank.netbank.tablet.feature.main.view.activity.MainActivity");


             CheckIcon = (i) =>
            {
                var fbbtn = driver.FindElements(By.Id("feedbackLinkImage"));
                if (i == sessionCounter)
                {
                    fbbtn.Count.ShouldNotBe(0);
                    fbbtn[0].Click();
                    driver.FindElement(By.Id("doItNowButton")).Click();
                    AnwserQuestionaire();
                    var submit = driver.FindElement(By.Id("done_button"));
                    if (submit != null)
                    {
                        submit.Click();
                        if (TryLoadingAction("com.commbank.netbank.tablet.feature.accounts.view.activity.TransactionsActivity"))
                        {
                            fbbtn = driver.FindElements(By.Id("feedbackLinkImage"));
                            fbbtn.Count.ShouldBe(0);
                        }
                    }

                }
                else if (i == sessionCounter - 1)
                {
                    var l = driver.FindElements(By.Id("doLaterButton"));
                    l.Count.ShouldNotBe(0);
                    l[0].Click();
                    if (TryLoadingAction("com.commbank.netbank.tablet.feature.accounts.view.activity.TransactionsActivity"))
                    {
                        fbbtn = driver.FindElements(By.Id("feedbackLinkImage"));
                        fbbtn.Count.ShouldNotBe(0);
                    }

                }
                else
                    fbbtn.Count.ShouldBe(0);

            };


            driver = new AndroidDriver<AndroidElement>(new Uri("http://127.0.0.1:4888/wd/hub"), capabilities, TimeSpan.FromSeconds(180));
        }

        [TearDown]
        public void AfterAll()
        {
            driver.Quit();
        }



        private bool TryLoadingAction(string expectActivity)
        {
            string loadingActivity = "com.commbank.netbank.tablet.feature.navigator.view.activity.NavigatorActivity";
            int times = 4;
            do
            {
                Thread.Sleep(5000);
                times -= 1;
            }
            while (driver.CurrentActivity.Equals(loadingActivity) && times > 0);
            return driver.CurrentActivity.Equals(expectActivity);
        }




        public bool Login()
        {
            var one = driver.FindElement(By.Id("buttonDigit1"));
            one.Click();
            var two = driver.FindElement(By.Id("buttonDigit2"));
            two.Click();
            one.Click();
            two.Click();
            return TryLoadingAction("com.commbank.netbank.tablet.feature.accounts.view.activity.DashboardActivity");

        }

        public bool OpenAcountDetail(Action<int> AddtionalCheck=null, int index=0)
        {
            bool result = false;
            Thread.Sleep(5000);
            //pick up one account
            var accountList = driver.FindElement(By.Id("accountsList"));
            if (accountList != null && accountList.FindElementsById("imageViewAccountIcon") != null)
            {       
                accountList.FindElementsById("imageViewAccountIcon")[0].Click();
                if (TryLoadingAction("com.commbank.netbank.tablet.feature.accounts.view.activity.TransactionsActivity") || TryLoadingAction("com.commbank.netbank.tablet.feature.feedback.view.FeedbackQuestionnaireActivity"))
                {
                    if (AddtionalCheck != null)
                        AddtionalCheck(index);

                    driver.FindElement(By.Id("btn_close")).Click();
                    result = true;
                }
            }
            return result;

        }
        public bool LogOff()
        {
            //logoff
            var home = driver.FindElement(By.Id("btn_home"));
            home.Click();
            var logoff = driver.FindElement(By.Id("img_log_off"));
            logoff.Click();
            driver.FindElement(By.Id("button1")).Click();

            return TryLoadingAction("com.commbank.netbank.tablet.feature.authentication.view.activity.LoginActivity");
        }

        private void AnwserQuestionaire()
        {
            Thread.Sleep(1000);
            var ans = driver.FindElements(By.Id("answerOption"));
            var comment = driver.FindElements(By.Id("comment_text_box"));
            if (ans != null && ans.Count > 0 && comment.Count == 0)
            {
                Random r = new Random();
                var ai = r.Next(0, ans.Count - 1);
                ans[ai].Click();
                AnwserQuestionaire();
            }
            else
            {
                comment[0].Click();
                Thread.Sleep(1000);
                comment[0].SendKeys("Automation Test Questionaire is awesome " + DateTime.Now.Date.ToShortDateString());
                driver.HideKeyboard();
            }
        }

       

        [Test]
        public void TestLoadRunningTableQuestionaireBySessionCounter()
        {

            for (int i = 0; i <= sessionCounter; i++)
            {
                Login();
                OpenAcountDetail(CheckIcon, i);
                LogOff();
            }
        }
    }
}
