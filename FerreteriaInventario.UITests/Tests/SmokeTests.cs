using OpenQA.Selenium.Appium;
using NUnit.Framework;
using FerreteriaInventario.UITests.Pages;
using FerreteriaInventario.UITests.Config;

namespace FerreteriaInventario.UITests.Tests;

[TestFixture]
[Order(3)]
public class SmokeTests
{
    private static AppiumDriver? _driver;
    private static TestSettings? _settings;
    private LoginPage? _loginPage;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _settings = TestSettings.FromEnvironment();
        var factory = new Drivers.AppiumDriverFactory(_settings);
        _driver = factory.CreateAndroidDriver();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _driver?.Quit();
    }

    [SetUp]
    public void SetUp()
    {
        _loginPage = new LoginPage(_driver!);
    }

    [Test]
    public void Should_App_Start_And_Show_Login()
    {
        Assert.That(_loginPage!.IsPageDisplayed(), Is.True, "App should start and show login page");
    }

    [Test]
    public void Should_Login_Elements_Be_Present()
    {
        Assert.That(_loginPage!.IsLoginButtonDisplayed(), Is.True, "Login button should be visible");
        Assert.That(_loginPage!.IsPageDisplayed(), Is.True, "Username entry should be visible");
    }

    [Test]
    public void Should_Full_Login_Flow_Work()
    {
        Assert.That(_loginPage!.IsPageDisplayed(), Is.True);

        if (_settings!.SkipLoginTests)
        {
            Assert.Inconclusive("Test credentials not configured via environment variables");
            return;
        }

        _loginPage.Login(_settings.TestUsername, _settings.TestPassword);
        _loginPage.WaitForLoginResult(15);

        var dashboardPage = new DashboardPage(_driver!);
        if (dashboardPage.IsPageDisplayed())
        {
            Assert.Pass("Full login flow works correctly");
        }
        else
        {
            Assert.Fail("Full login flow failed - dashboard not displayed after valid login");
        }
    }

    [Test]
    public void Should_Capture_Screenshot_On_Test_Failure()
    {
        var screenshotHelper = new Utilities.ScreenshotHelper(_driver!);

        try
        {
            throw new Exception("Simulated failure for screenshot capture test");
        }
        catch
        {
            var path = screenshotHelper.CaptureScreenshot("SmokeTests_ScreenshotCapture");
            Assert.That(File.Exists(path), Is.True, "Screenshot should be captured on failure");
        }
    }
}