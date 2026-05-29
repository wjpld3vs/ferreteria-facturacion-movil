using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FerreteriaInventario.UITests.Pages;

public class LoginPage : BasePage
{
    private const string UsuarioEntryId = "Login_UsuarioEntry";
    private const string PasswordEntryId = "Login_PasswordEntry";
    private const string SubmitButtonId = "Login_SubmitButton";
    private const string ErrorLabelId = "Login_ErrorLabel";
    private const string LoaderId = "Login_Loader";

    public LoginPage(AppiumDriver driver) : base(driver) { }

    public void EnterCredentials(string username, string password)
    {
        SendKeys(UsuarioEntryId, username);
        SendKeys(PasswordEntryId, password);
    }

    public void ClickSubmit()
    {
        Click(SubmitButtonId);
    }

    public void Login(string username, string password)
    {
        EnterCredentials(username, password);
        ClickSubmit();
    }

    public bool IsErrorMessageDisplayed()
    {
        return !string.IsNullOrEmpty(GetText(ErrorLabelId));
    }

    public string? GetErrorMessage()
    {
        return GetText(ErrorLabelId);
    }

    public bool IsLoginButtonDisplayed()
    {
        return IsDisplayed(SubmitButtonId, 5);
    }

    public bool IsPageDisplayed()
    {
        return IsDisplayed(UsuarioEntryId, 10);
    }

    public string? GetUsernameText()
    {
        var element = TryFindElement(UsuarioEntryId, 5);
        return element?.GetAttribute("text");
    }

    public void WaitForLoginResult(int timeoutSeconds = 10)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d =>
        {
            var error = GetErrorMessage();
            return !string.IsNullOrEmpty(error) || !IsDisplayed(SubmitButtonId, 0);
        });
    }
}