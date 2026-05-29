using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace FerreteriaInventario.UITests.Pages;

public class ProductosPage : BasePage
{
    private const string SearchEntryId = "Productos_SearchEntry";
    private const string SearchButtonId = "Productos_SearchButton";
    private const string NewProductButtonId = "Productos_NewButton";

    public ProductosPage(AppiumDriver driver) : base(driver) { }

    public bool IsPageDisplayed()
    {
        return IsDisplayed(SearchEntryId, 10);
    }

    public void Search(string text)
    {
        SendKeys(SearchEntryId, text);
        Click(SearchButtonId);
    }

    public void ClickNewProduct()
    {
        Click(NewProductButtonId);
    }

    public bool IsNewProductButtonDisplayed()
    {
        return IsDisplayed(NewProductButtonId, 5);
    }

    public bool IsSearchEntryDisplayed()
    {
        return IsDisplayed(SearchEntryId, 5);
    }
}