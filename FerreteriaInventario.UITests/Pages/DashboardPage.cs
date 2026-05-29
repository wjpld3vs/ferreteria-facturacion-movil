using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FerreteriaInventario.UITests.Pages;

public class DashboardPage : BasePage
{
    private const string WelcomeLabelId = "Dashboard_WelcomeLabel";
    private const string LogoutButtonId = "Dashboard_LogoutButton";

    public DashboardPage(AppiumDriver driver) : base(driver) { }

    public bool IsPageDisplayed()
    {
        return IsDisplayed(WelcomeLabelId, 10);
    }

    public bool IsLogoutButtonDisplayed()
    {
        return IsDisplayed(LogoutButtonId, 5);
    }

    public void ClickLogout()
    {
        Click(LogoutButtonId);
    }

    public string? GetWelcomeText()
    {
        return GetText(WelcomeLabelId);
    }

    public void ClickQuickAction(string actionName)
    {
        var by = OpenQA.Selenium.Appium.MobileBy.AndroidUIAutomator(
            $"new UiSelector().text(\"{actionName}\")");
        var element = FindElementSafe(by, 10);
        element.Click();
    }
}