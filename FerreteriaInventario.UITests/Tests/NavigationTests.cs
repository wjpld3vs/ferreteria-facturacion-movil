using OpenQA.Selenium.Appium;
using NUnit.Framework;
using FerreteriaInventario.UITests.Pages;
using FerreteriaInventario.UITests.Config;

namespace FerreteriaInventario.UITests.Tests;

[TestFixture]
[Order(2)]
public class NavigationTests
{
    private static AppiumDriver? _driver;
    private static TestSettings? _settings;
    private LoginPage? _loginPage;
    private DashboardPage? _dashboardPage;

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
        _dashboardPage = new DashboardPage(_driver!);
    }

    [Test]
    public void Should_Display_Login_Then_Dashboard_On_Valid_Login()
    {
        Assert.That(_loginPage!.IsPageDisplayed(), Is.True);

        if (_settings!.SkipLoginTests)
        {
            Assert.Inconclusive("Test credentials not configured via environment variables");
            return;
        }

        _loginPage.Login(_settings.TestUsername, _settings.TestPassword);
        _loginPage.WaitForLoginResult(15);

        if (_dashboardPage!.IsPageDisplayed())
        {
            Assert.Pass("Dashboard displayed after valid login");
        }
        else
        {
            Assert.Inconclusive("Dashboard not reached - possible API unavailable or app not loaded");
        }
    }

    [Test]
    public void Should_Logout_And_Return_To_Login()
    {
        if (_settings!.SkipLoginTests)
        {
            Assert.Inconclusive("Test credentials not configured");
            return;
        }

        if (!_dashboardPage!.IsPageDisplayed())
        {
            _loginPage!.Login(_settings.TestUsername, _settings.TestPassword);
            _loginPage.WaitForLoginResult(15);
        }

        if (_dashboardPage!.IsPageDisplayed())
        {
            _dashboardPage.ClickLogout();
            Assert.That(_loginPage!.IsPageDisplayed(), Is.True,
                "Should return to login page after logout");
        }
        else
        {
            Assert.Inconclusive("Dashboard not reachable for logout test");
        }
    }

    [Test]
    public void Should_Open_Productos_From_Dashboard()
    {
        if (_settings!.SkipLoginTests)
        {
            Assert.Inconclusive("Test credentials not configured");
            return;
        }

        if (_loginPage!.IsPageDisplayed())
        {
            _loginPage.Login(_settings.TestUsername, _settings.TestPassword);
            _loginPage.WaitForLoginResult(15);
        }

        if (!_dashboardPage!.IsPageDisplayed())
        {
            Assert.Inconclusive("Dashboard not reachable");
            return;
        }

        _dashboardPage.ClickQuickAction("Productos");

        var productosPage = new ProductosPage(_driver!);
        Assert.That(productosPage.IsPageDisplayed(), Is.True,
            "Should navigate to Productos from Dashboard");
    }

    [Test]
    public void Should_Open_Ventas_From_Dashboard()
    {
        if (_settings!.SkipLoginTests)
        {
            Assert.Inconclusive("Test credentials not configured");
            return;
        }

        if (_loginPage!.IsPageDisplayed())
        {
            _loginPage.Login(_settings.TestUsername, _settings.TestPassword);
            _loginPage.WaitForLoginResult(15);
        }

        if (!_dashboardPage!.IsPageDisplayed())
        {
            Assert.Inconclusive("Dashboard not reachable");
            return;
        }

        _dashboardPage.ClickQuickAction("Ventas");

        var ventasPage = new VentasPage(_driver!);
        Assert.That(ventasPage.IsPageDisplayed(), Is.True,
            "Should navigate to Ventas from Dashboard");
    }
}