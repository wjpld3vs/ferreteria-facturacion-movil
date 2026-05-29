using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FerreteriaInventario.UITests.Utilities;

public class ScreenshotHelper
{
    private readonly AppiumDriver _driver;
    private readonly string _screenshotDirectory;

    public ScreenshotHelper(AppiumDriver driver, string screenshotDirectory = "TestResults/Screenshots")
    {
        _driver = driver;
        _screenshotDirectory = screenshotDirectory;
        Directory.CreateDirectory(_screenshotDirectory);
    }

    public string CaptureScreenshot(string testName)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{testName}_{timestamp}.png";
        var filePath = Path.Combine(_screenshotDirectory, fileName);

        try
        {
            var screenshot = _driver as ITakesScreenshot;
            if (screenshot != null)
            {
                var ts = screenshot.GetScreenshot();
                System.IO.File.WriteAllBytes(filePath, ts.AsByteArray);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
        }

        return filePath;
    }

    public void CaptureOnFailure(string testName, Action action)
    {
        try
        {
            action();
        }
        catch
        {
            CaptureScreenshot(testName);
            throw;
        }
    }

    public static string GetTestResultsDirectory()
    {
        return "TestResults";
    }

    public static string GetScreenshotsDirectory()
    {
        var dir = Path.Combine(GetTestResultsDirectory(), "Screenshots");
        Directory.CreateDirectory(dir);
        return dir;
    }
}