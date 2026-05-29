using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using FerreteriaInventario.UITests.Config;

namespace FerreteriaInventario.UITests.Drivers;

public class AppiumDriverFactory
{
    private readonly TestSettings _settings;

    public AppiumDriverFactory(TestSettings settings)
    {
        _settings = settings;
    }

    public AndroidDriver CreateAndroidDriver()
    {
        var options = new AppiumOptions();

        options.PlatformName = "Android";
        options.AutomationName = "UiAutomator2";
        options.DeviceName = _settings.AndroidDeviceName;
        options.AddAdditionalAppiumOption("appium:noReset", _settings.NoReset);

        if (!string.IsNullOrEmpty(_settings.AndroidAppPath))
        {
            options.AddAdditionalAppiumOption("appium:app", _settings.AndroidAppPath);
        }

        if (!string.IsNullOrEmpty(_settings.AppPackage))
        {
            options.AddAdditionalAppiumOption("appium:appPackage", _settings.AppPackage);
        }

        if (!string.IsNullOrEmpty(_settings.AppActivity))
        {
            options.AddAdditionalAppiumOption("appium:appActivity", _settings.AppActivity);
        }

        var timeout = TimeSpan.FromSeconds(Math.Max(_settings.AppiumTimeoutMs / 1000.0, 15));
        return new AndroidDriver(new Uri(_settings.AppiumServerUrl), options, timeout);
    }

    public IOSDriver CreateIosDriver()
    {
        var options = new AppiumOptions();

        options.PlatformName = "iOS";
        options.AutomationName = "XCUITest";
        options.DeviceName = _settings.IosDeviceName;
        options.AddAdditionalAppiumOption("appium:noReset", _settings.NoReset);

        if (!string.IsNullOrEmpty(_settings.IosAppPath))
        {
            options.AddAdditionalAppiumOption("appium:app", _settings.IosAppPath);
        }

        var timeout = TimeSpan.FromSeconds(Math.Max(_settings.AppiumTimeoutMs / 1000.0, 15));
        return new IOSDriver(new Uri(_settings.AppiumServerUrl), options, timeout);
    }

    public AppiumDriver CreateDriver(string platform = "Android")
    {
        return platform.ToLowerInvariant() switch
        {
            "android" => CreateAndroidDriver(),
            "ios" => CreateIosDriver(),
            _ => throw new ArgumentException($"Unsupported platform: {platform}. Use 'Android' or 'IOS'.")
        };
    }
}