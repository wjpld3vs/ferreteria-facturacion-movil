using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using FerreteriaInventario.UITests.Utilities;

namespace FerreteriaInventario.UITests.Pages;

public abstract class BasePage
{
    protected readonly AppiumDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly ScreenshotHelper ScreenshotHelper;

    protected BasePage(AppiumDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        ScreenshotHelper = new ScreenshotHelper(driver);
    }

    protected By GetByAutomationId(string automationId)
    {
        if (Driver is AndroidDriver)
        {
            return MobileBy.AndroidUIAutomator($"new UiSelector().descriptionContains(\"{automationId}\")");
        }
        return MobileBy.AccessibilityId(automationId);
    }

    protected IWebElement FindElementSafe(By by, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d => d.FindElement(by));
    }

    protected IWebElement FindElementSafe(string automationId, int timeoutSeconds = 10)
    {
        return FindElementSafe(GetByAutomationId(automationId), timeoutSeconds);
    }

    protected IWebElement? TryFindElement(By by, int timeoutSeconds = 10)
    {
        try
        {
            return FindElementSafe(by, timeoutSeconds);
        }
        catch
        {
            return null;
        }
    }

    protected IWebElement? TryFindElement(string automationId, int timeoutSeconds = 10)
    {
        return TryFindElement(GetByAutomationId(automationId), timeoutSeconds);
    }

    protected void Click(string automationId)
    {
        var element = FindElementSafe(automationId, 10);
        element.Click();
    }

    protected void SendKeys(string automationId, string text)
    {
        var element = FindElementSafe(automationId, 10);
        element.Clear();
        element.SendKeys(text);
    }

    protected string? GetText(string automationId)
    {
        var element = TryFindElement(automationId, 10);
        return element?.Text;
    }

    protected bool IsDisplayed(string automationId, int timeoutSeconds = 5)
    {
        return TryFindElement(automationId, timeoutSeconds)?.Displayed == true;
    }

    protected void WaitForLoaderToDisappear(string loaderAutomationId, int timeoutSeconds = 30)
    {
        var by = GetByAutomationId(loaderAutomationId);
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(d =>
            {
                var elements = d.FindElements(by);
                return elements.Count == 0 || !elements[0].Displayed;
            });
        }
        catch
        {
        }
    }

    protected void CaptureScreenshotOnFailure(string testName, Action action)
    {
        ScreenshotHelper.CaptureOnFailure(testName, action);
    }
}