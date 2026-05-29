using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FerreteriaInventario.UITests.Pages;

public class VentasPage : BasePage
{
    private const string NewVentaButtonId = "Ventas_NewButton";

    public VentasPage(AppiumDriver driver) : base(driver) { }

    public bool IsPageDisplayed()
    {
        return IsDisplayed(NewVentaButtonId, 10);
    }

    public void ClickNewVenta()
    {
        Click(NewVentaButtonId);
    }

    public bool IsNewVentaButtonDisplayed()
    {
        return IsDisplayed(NewVentaButtonId, 5);
    }
}