namespace FerreteriaInventario.Maui.Helpers;

public static class ApiSettings
{
    public const string WindowsBaseUrl = "http://localhost:5099/";
    public const string AndroidEmulatorBaseUrl = "http://10.0.2.2:5099/";
    public const string PhysicalDeviceBaseUrl = "http://192.168.1.100:5099/";

    public static string GetBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            return AndroidEmulatorBaseUrl;
        }

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            return WindowsBaseUrl;
        }

        return WindowsBaseUrl;
    }
}
