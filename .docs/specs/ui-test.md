# UI Test Specification - FerreteriaInventarioApp

## 1. Project Overview

**Project:** FerreteriaInventarioApp UI Test Suite
**Framework:** .NET MAUI (.NET 9)
**Test Framework:** NUnit
**Automation Tool:** Appium (UIAutomator2 for Android, XCUITest for iOS)
**Language:** C#

## 2. Context Analysis

### 2.1 Solution Structure
```
FerreteriaInventarioApp.sln
├── FerreteriaInventario.Api        (ASP.NET Core Web API)
├── FerreteriaInventario.Maui       (.NET MAUI App)
└── FerreteriaInventario.Tests      (xUnit - existing unit tests)
```

### 2.2 Platform Targets
- `net9.0-android`
- `net9.0-ios`
- `net9.0-maccatalyst`
- `net9.0-windows10.0.19041.0` (optional)

### 2.3 Architecture Pattern
- **MVVM** with XAML views
- **Shell navigation** (`AppShell.xaml`)
- **Dependency Injection** via `MauiProgram.cs`
- **JWT authentication** with `SecureStorage` for token persistence

### 2.4 Critical Screens for Testing
1. **LoginPage** - Authentication entry point
2. **DashboardPage** - Home after login, role-based menu
3. **ProductosPage** - Product listing and search
4. **VentasPage** - Sales listing and creation
5. **ComprasPage** - Purchases listing and creation

### 2.5 Existing Test Project
- Uses **xUnit** (not NUnit)
- Located at `FerreteriaInventario.Tests/`
- Contains unit tests only (no UI tests)
- UI tests will be created in a separate project

## 3. Project Setup

### 3.1 New UI Test Project
Create: `FerreteriaInventario.UITests/`

**NuGet Dependencies:**
- `NUnit` (3.x)
- `NUnit3TestAdapter`
- `Microsoft.NET.Test.Sdk`
- `Appium.WebDriver`
- `FluentAssertions`

### 3.2 Project Structure
```
FerreteriaInventario.UITests/
├── Drivers/
│   └── AppiumDriverFactory.cs
├── Pages/
│   ├── BasePage.cs
│   ├── LoginPage.cs
│   ├── DashboardPage.cs
│   ├── ProductosPage.cs
│   └── VentasPage.cs
├── Tests/
│   ├── LoginTests.cs
│   ├── NavigationTests.cs
│   └── SmokeTests.cs
├── Config/
│   └── TestSettings.cs
├── Utilities/
│   ├── WaitHelpers.cs
│   └── ScreenshotHelper.cs
├── FerreteriaInventario.UITests.csproj
└── README.md
```

## 4. Driver Configuration

### 4.1 AppiumDriverFactory
Supports Android (UIAutomator2) and iOS (XCUITest).

**Configurable Capabilities:**
- `platformName` (Android/iOS)
- `automationName` (UiAutomator2/XCUITest)
- `deviceName`
- `app` (path or bundle ID)
- `appPackage` (Android)
- `appActivity` (Android)
- `appium:server` (URL)
- `appium:timeout` (ms)
- `noReset`

### 4.2 Environment Variables
```
APPIUM_SERVER_URL=http://localhost:4723
APPIUM_ANDROID_DEVICE=Android Emulator
APPIUM_IOS_DEVICE=iPhone 15
ANDROID_APP_PATH=path/to/app.apk
```

### 4.3 Appium Installation
```bash
npm install -g appium
appium driver install uiautomator2  # Android
appium driver install xcuitest      # iOS
```

## 5. Page Object Model

### 5.1 BasePage
- Common wait methods
- Screenshot capture on failure
- Element finders
- Navigation assertions

### 5.2 Screen Pages

**LoginPage:**
- `Login_UsuarioEntry` (AutomationId)
- `Login_PasswordEntry` (AutomationId)
- `Login_SubmitButton` (AutomationId)
- `Login_ErrorLabel` (AutomationId)
- `EnterCredentials(string user, string pass)`
- `Submit()`
- `GetErrorMessage()`

**DashboardPage:**
- `Dashboard_TitleLabel` (AutomationId)
- `Dashboard_LogoutButton` (AutomationId)
- `Dashboard_QuickActions` (CollectionView)
- `IsDisplayed()`
- `ClickLogout()`

**ProductosPage:**
- `Productos_SearchEntry` (AutomationId)
- `Productos_SearchButton` (AutomationId)
- `Productos_NewButton` (AutomationId)
- `Productos_List` (CollectionView)
- `Search(string text)`
- `ClickNewProduct()`

**VentasPage:**
- `Ventas_NewButton` (AutomationId)
- `Ventas_List` (CollectionView)
- `ClickNewVenta()`

## 6. AutomationIds to Add

### LoginPage.xaml
| Element | AutomationId |
|---------|--------------|
| Usuario Entry | `Login_UsuarioEntry` |
| Password Entry | `Login_PasswordEntry` |
| Submit Button | `Login_SubmitButton` |
| Error Label | `Login_ErrorLabel` |

### DashboardPage.xaml
| Element | AutomationId |
|---------|--------------|
| Welcome Label | `Dashboard_WelcomeLabel` |
| Logout Button | `Dashboard_LogoutButton` |

### ProductosPage.xaml
| Element | AutomationId |
|---------|--------------|
| Search Entry | `Productos_SearchEntry` |
| Search Button | `Productos_SearchButton` |
| New Product Button | `Productos_NewButton` |

### VentasPage.xaml
| Element | AutomationId |
|---------|--------------|
| New Venta Button | `Ventas_NewButton` |

## 7. Test Cases

### 7.1 Smoke Tests
```
Should_Display_Login_Screen_On_App_Start
Should_Show_Error_When_Login_Credentials_Are_Invalid
Should_Navigate_To_Dashboard_When_Login_Is_Successful
Should_Logout_And_Return_To_Login
Should_Search_Products
Should_Navigate_To_Ventas
```

### 7.2 Login Flow Tests
```
Should_Display_Login_Page
Should_Show_Error_With_Empty_Credentials
Should_Show_Error_With_Invalid_Credentials
Should_Succeed_With_Valid_Credentials
Should_Display_Role_Based_Menu_For_Admin
Should_Display_Restricted_Menu_For_Operario
```

### 7.3 Navigation Tests
```
Should_Open_Productos_From_Dashboard
Should_Open_Ventas_From_Dashboard
Should_Open_Compras_From_Dashboard (Admin only)
Should_Open_Reportes_From_Dashboard (Admin only)
Should_Open_Usuarios_From_Dashboard (Admin only)
```

### 7.4 CRUD Tests
```
Should_Create_New_Product (Admin)
Should_Search_Products_By_Name
Should_Create_New_Venta
Should_Add_Product_To_Venta
Should_Complete_Venta_Save
```

## 8. Wait Helpers

### 8.1 Methods
- `WaitForElementVisible(By, timeout)` - Element appears
- `WaitForElementClickable(By, timeout)` - Element is clickable
- `WaitForElementHidden(By, timeout)` - Loader disappears
- `WaitForNavigation(timeout)` - Page transition completes
- `WaitForElementNotPresent(By, timeout)` - Element no longer exists

### 8.2 Screenshot on Failure
- Path: `TestResults/Screenshots/`
- Format: `{TestName}_{Timestamp}.png`

## 9. CI/CD Configuration

### 9.1 GitHub Actions (example)
```yaml
- name: Run UI Tests
  run: |
    appium --address 127.0.0.1 --port 4723 &
    sleep 5
    dotnet test FerreteriaInventario.UITests
  env:
    APPIUM_SERVER_URL: http://localhost:4723
```

### 9.2 Artifacts
- TestResults/*.trx
- TestResults/Screenshots/*.png

## 10. Test Execution

### 10.1 Prerequisites
- .NET 9 SDK
- Visual Studio 2022
- Android SDK / Emulator
- Node.js + Appium
- Android app APK built

### 10.2 Start Appium
```bash
appium
```

### 10.3 Run Tests
```bash
dotnet test FerreteriaInventario.UITests
```

## 11. Known Limitations

1. **No real credentials in tests** - Use placeholders; actual credentials provided separately
2. **iOS tests** - Require Mac with Xcode; not runnable on Windows CI
3. **Android emulator** - Must be running before tests start
4. **API dependency** - Tests require API to be running at configured BaseUrl
5. **Existing xUnit tests** - New project does not modify existing test project

## 12. Assumptions

1. Appium server runs locally on `http://localhost:4723`
2. Android emulator/device connected and accessible via `adb`
3. MAUI app already built as APK at configured path
4. API running at BaseUrl (configurable per environment)
5. Test data exists (seed data from API initialization)

## 13. Configuration Files

### appsettings.json (for UITests)
```json
{
  "Appium": {
    "Server": "http://localhost:4723",
    "Android": {
      "PlatformName": "Android",
      "AutomationName": "UiAutomator2",
      "DeviceName": "Android Emulator",
      "AppPackage": "com.ferreteria.inventario.app",
      "AppActivity": "com.ferreteria.inventario.app.MainActivity"
    }
  },
  "Api": {
    "BaseUrl": "http://10.0.2.2:5099"
  }
}
```

## 14. Next Steps (Priority Order)

1. Create `FerreteriaInventario.UITests` project and add to solution
2. Implement `AppiumDriverFactory` for Android
3. Add `AutomationId` attributes to LoginPage, DashboardPage
4. Create `BasePage` with wait helpers
5. Create `LoginPage` and `LoginTests`
6. Create `DashboardPage` and `NavigationTests`
7. Add screenshot helper for failure capture
8. Configure CI pipeline template
9. Expand to Productos and Ventas tests