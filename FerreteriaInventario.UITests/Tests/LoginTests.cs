using OpenQA.Selenium.Appium;
using NUnit.Framework;
using FerreteriaInventario.UITests.Pages;
using FerreteriaInventario.UITests.Config;

namespace FerreteriaInventario.UITests.Tests;

[TestFixture]
[Order(1)]
public class LoginTests
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
    public void Should_Display_Login_Screen_On_App_Start()
    {
        Assert.That(_loginPage!.IsPageDisplayed(), Is.True, "Login page should be displayed on app start");
    }

    [Test]
    public void Should_Show_Error_When_Login_Credentials_Are_Invalid()
    {
        _loginPage!.Login("invalid_user", "invalid_password");
        _loginPage.WaitForLoginResult(10);

        Assert.That(_loginPage.IsErrorMessageDisplayed(), Is.True,
            "Error message should be displayed for invalid credentials");
    }

    [Test]
    public void Should_Show_Error_With_Empty_Credentials()
    {
        _loginPage!.ClickSubmit();
        _loginPage.WaitForLoginResult(10);

        Assert.That(_loginPage.IsErrorMessageDisplayed(), Is.True);
    }

    [Test]
    public void Should_Login_Button_Be_Displayed()
    {
        Assert.That(_loginPage!.IsLoginButtonDisplayed(), Is.True);
    }

    [Test]
    public void Should_Not_Show_Error_Before_Login_Attempt()
    {
        Assert.That(_loginPage!.IsErrorMessageDisplayed(), Is.False,
            "Error should not be shown before login attempt");
    }

    [Test]
    public void Should_Allow_Credential_Entry()
    {
        _loginPage!.EnterCredentials("testuser", "testpass");

        var userEntry = _loginPage.GetUsernameText();
        Assert.That(userEntry, Is.EqualTo("testuser"));
    }
}