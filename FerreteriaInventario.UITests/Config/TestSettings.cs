namespace FerreteriaInventario.UITests.Config;

public class TestSettings
{
    public string AppiumServerUrl { get; set; } = Environment.GetEnvironmentVariable("APPIUM_SERVER_URL") ?? "http://localhost:4723";
    public int AppiumTimeoutMs { get; set; } = 30000;
    public string AndroidDeviceName { get; set; } = Environment.GetEnvironmentVariable("APPIUM_ANDROID_DEVICE") ?? "Android Emulator";
    public string IosDeviceName { get; set; } = Environment.GetEnvironmentVariable("APPIUM_IOS_DEVICE") ?? "iPhone 15";
    public string AndroidAppPath { get; set; } = Environment.GetEnvironmentVariable("ANDROID_APP_PATH") ?? "";
    public string IosAppPath { get; set; } = Environment.GetEnvironmentVariable("IOS_APP_PATH") ?? "";
    public string AppPackage { get; set; } = Environment.GetEnvironmentVariable("ANDROID_APP_PACKAGE") ?? "com.ferreteria.inventario.app";
    public string AppActivity { get; set; } = Environment.GetEnvironmentVariable("ANDROID_APP_ACTIVITY") ?? "com.ferreteria.inventario.app.MainActivity";
    public string ApiBaseUrl { get; set; } = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://10.0.2.2:5099";
    public string TestUsername { get; set; } = Environment.GetEnvironmentVariable("TEST_USERNAME") ?? "";
    public string TestPassword { get; set; } = Environment.GetEnvironmentVariable("TEST_PASSWORD") ?? "";
    public bool NoReset { get; set; } = false;
    public bool SkipLoginTests { get; set; } = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_USERNAME"));

    public static TestSettings FromEnvironment()
    {
        return new TestSettings();
    }
}