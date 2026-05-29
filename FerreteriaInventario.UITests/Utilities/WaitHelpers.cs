using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FerreteriaInventario.UITests.Utilities;

public class WaitHelpers
{
    private readonly AppiumDriver _driver;

    public WaitHelpers(AppiumDriver driver)
    {
        _driver = driver;
    }

    public static WaitHelpers Create(AppiumDriver driver)
    {
        return new WaitHelpers(driver);
    }

    public IWebElement WaitForElementVisible(By by, int timeoutSeconds = 30)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d => d.FindElement(by));
    }

    public IWebElement WaitForElementClickable(By by, int timeoutSeconds = 30)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var element = d.FindElement(by);
            return element.Enabled && element.Displayed ? element : null!;
        });
    }

    public bool WaitForElementHidden(By by, int timeoutSeconds = 30)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            wait.Until(d =>
            {
                var elements = d.FindElements(by);
                return elements.Count == 0 || !elements[0].Displayed;
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool WaitForElementNotPresent(By by, int timeoutSeconds = 30)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            wait.Until(d => d.FindElements(by).Count == 0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void WaitForNavigation(int timeoutSeconds = 1)
    {
        Thread.Sleep(TimeSpan.FromSeconds(timeoutSeconds));
    }

    public bool IsElementPresent(By by)
    {
        try
        {
            _driver.FindElement(by);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsElementVisible(By by)
    {
        try
        {
            var element = _driver.FindElement(by);
            return element.Displayed;
        }
        catch
        {
            return false;
        }
    }
}