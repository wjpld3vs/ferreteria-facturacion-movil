# Guia de UI Tests - FerreteriaInventarioApp

## 1. Objetivo

Este proyecto contiene pruebas UI automatizadas end-to-end para la aplicacion **FerreteriaInventarioApp**, construida con .NET MAUI. Las pruebas utilizan **Appium** como herramienta de automatizacion movil y **NUnit** como framework de pruebas.

El objetivo es proporcionar un conjunto de tests mantenibles, rapidos de ejecutar y faciles de extender para validar los flujos criticos de la aplicacion en dispositivos Android (e iOS cuando este disponible).

## 2. Estructura del Proyecto

```
FerreteriaInventario.UITests/
├── Drivers/
│   └── AppiumDriverFactory.cs    # Fabrica de drivers Appium (Android/iOS)
├── Pages/
│   ├── BasePage.cs              # Clase base para todos los Page Objects
│   ├── LoginPage.cs             # Page Object para LoginPage
│   ├── DashboardPage.cs         # Page Object para DashboardPage
│   ├── ProductosPage.cs         # Page Object para ProductosPage
│   └── VentasPage.cs            # Page Object para VentasPage
├── Tests/
│   ├── LoginTests.cs            # Tests para flujo de autenticacion
│   ├── NavigationTests.cs       # Tests para navegacion entre pantallas
│   └── SmokeTests.cs            # Smoke tests generales
├── Config/
│   └── TestSettings.cs          # Configuracion centralizada
├── Utilities/
│   ├── WaitHelpers.cs           # Helpers para esperas robustas
│   └── ScreenshotHelper.cs      # Captura de screenshots en fallos
└── FerreteriaInventario.UITests.csproj
```

## 3. Requisitos Previos

### 3.1 Software Necesario

- **.NET 9 SDK**
- **Visual Studio 2022** (con workload .NET MAUI)
- **Android SDK** (API level 21+)
- **Node.js** (v18+)
- **Appium** (v2.x)
- **Android Emulator** o dispositivo fisico

### 3.2 Instalar Appium

```bash
npm install -g appium
appium driver install uiautomator2  # Para Android
```

### 3.3 Variables de Entorno

| Variable | Descripcion | Valor por defecto |
|----------|-------------|-------------------|
| `APPIUM_SERVER_URL` | URL del servidor Appium | `http://localhost:4723` |
| `APPIUM_ANDROID_DEVICE` | Nombre del dispositivo Android | `Android Emulator` |
| `APPIUM_IOS_DEVICE` | Nombre del dispositivo iOS | `iPhone 15` |
| `ANDROID_APP_PATH` | Ruta al APK | (vacio - usa app instalado) |
| `API_BASE_URL` | URL base de la API | `http://10.0.2.2:5099` |
| `TEST_USERNAME` | Usuario para tests (no hardcodear) | (vacio) |
| `TEST_PASSWORD` | Password para tests (no hardcodear) | (vacio) |

### 3.4 Configurar Credenciales de Test

Para ejecutar tests que requieren login, configure las variables de entorno con las credenciales de prueba:

```powershell
# Windows PowerShell
$env:TEST_USERNAME="admin"
$env:TEST_PASSWORD="Admin123*"
$env:ANDROID_APP_PATH="C:\ruta\al\app.apk"
```

```bash
# Linux/macOS
export TEST_USERNAME="admin"
export TEST_PASSWORD="Admin123*"
export ANDROID_APP_PATH="/ruta/al/app.apk"
```

**Nota:** No hardcodee credenciales en el codigo. Use variables de entorno o archivos de configuracion externos que no se suban al repositorio.

## 4. Como Ejecutar los Tests

### 4.1 Ejecutar desde Visual Studio 2022

**Opcion A - Ventana Test Explorer:**
1. Menu `Test` > `Test Explorer`
2. Haga clic derecho en `FerreteriaInventario.UITests`
3. Seleccione `Run All Tests` o `Run Tests`

**Opcion B - Desde el proyecto:**
1. En Solution Explorer, clic derecho en `FerreteriaInventario.UITests`
2. Seleccione `Set as StartUp Project`
3. Presione `F5` para debug o `Ctrl+F5` para ejecutar sin debug

**Opcion C - Desde el menu Test:**
1. Menu `Test` > `Run All Tests` (Ctrl+R, A)
2. O `Test` > `Run Tests` > `{Nombre del test}`

**Configuracion requerida:**
- El emulador Android debe estar ejecutandose, O
- Un dispositivo Android fisico conectado via USB con debugging habilitado
- Appium server ejecutandose en `http://localhost:4723`

**Configurar dispositivo emulado en VS:**
Si usa emulador, setee la variable de entorno antes de ejecutar:
```powershell
$env:APPIUM_ANDROID_DEVICE="emulator-5554"
```

### 4.2 Configurar DeviceName Correctamente

**Para dispositivo fisico Android:**
```powershell
# Ver dispositivos conectados
adb devices
# Output: List of devices attached
# R5CR123456789 device

$env:APPIUM_ANDROID_DEVICE="R5CR123456789"
```

**Para emulador Android:**
```powershell
$env:APPIUM_ANDROID_DEVICE="emulator-5554"
# O el nombre del AVD que creo en Android Studio
$env:APPIUM_ANDROID_DEVICE="Pixel_6_API_33"
```

### 4.2 Ejecutar desde CLI

```bash
# Restaurar paquetes
dotnet restore FerreteriaInventario.UITests/FerreteriaInventario.UITests.csproj

# Ejecutar todos los tests
dotnet test FerreteriaInventario.UITests

# Ejecutar con verbosidad
dotnet test FerreteriaInventario.UITests --verbosity normal

# Ejecutar un test especifico
dotnet test FerreteriaInventario.UITests --filter "Name=Should_Display_Login_Screen"

# Ejecutar con output de resultados
dotnet test FerreteriaInventario.UITests --logger "trx;LogFileName=TestResults.trx"
```

### 4.3 Ejecutar desde Appium

1. Inicie Appium en una terminal:
   ```bash
   appium --address 127.0.0.1 --port 4723
   ```

2. En otra terminal, ejecute los tests:
   ```bash
   dotnet test FerreteriaInventario.UITests
   ```

## 5. Arquitectura de las Pruebas

### 5.1 Page Object Model (POM)

Cada pantalla de la aplicacion tiene su propio Page Object que encapsula:
- Los localizadores (AutomationIds) de los elementos
- Las acciones posibles sobre la pantalla
- Las validaciones basicas de estado

**Ejemplo: LoginPage.cs**
```csharp
public class LoginPage : BasePage
{
    private const string UsuarioEntryId = "Login_UsuarioEntry";
    private const string PasswordEntryId = "Login_PasswordEntry";
    private const string SubmitButtonId = "Login_SubmitButton";

    public void EnterCredentials(string username, string password)
    {
        SendKeys(UsuarioEntryId, username);
        SendKeys(PasswordEntryId, password);
    }

    public void Login(string username, string password)
    {
        EnterCredentials(username, password);
        Click(SubmitButtonId);
    }
}
```

### 5.2 BasePage

La clase `BasePage` proporciona metodos comunes:
- `FindElement(automationId)` - Encuentra un elemento por AutomationId
- `Click(automationId)` - Hace click en un elemento
- `SendKeys(automationId, text)` - Escribe texto
- `IsDisplayed(automationId)` - Verifica si un elemento es visible
- `WaitForLoaderToDisappear(automationId)` - Espera a que desaparezca un loader

### 5.3 Driver Factory

`AppiumDriverFactory` crea y configura los drivers de Appium:
- Configura las capabilities necesarias para Android/iOS
- Permite configurar timeouts, noReset, y otras opciones
- Centraliza la creacion del driver para reuse en todos los tests

## 6. AutomationIds en la App

Para que las pruebas funcionen, cada control significativo en las vistas XAML tiene un `AutomationId` unico:

### LoginPage.xaml
| Control | AutomationId |
|---------|--------------|
| Campo usuario | `Login_UsuarioEntry` |
| Campo contrasena | `Login_PasswordEntry` |
| Boton login | `Login_SubmitButton` |
| Label error | `Login_ErrorLabel` |

### DashboardPage.xaml
| Control | AutomationId |
|---------|--------------|
| Label bienvenida | `Dashboard_WelcomeLabel` |
| Boton logout | `Dashboard_LogoutButton` |

### ProductosPage.xaml
| Control | AutomationId |
|---------|--------------|
| Campo busqueda | `Productos_SearchEntry` |
| Boton buscar | `Productos_SearchButton` |
| Boton nuevo producto | `Productos_NewButton` |

### VentasPage.xaml
| Control | AutomationId |
|---------|--------------|
| Boton nueva venta | `Ventas_NewButton` |

## 7. Como Agregar Nuevos Tests

### 7.1 Crear un Nuevo Page Object

1. Cree una nueva clase en `/Pages` que herede de `BasePage`
2. Defina constantes para los AutomationIds
3. Implemente metodos de accion (Click, SendKeys, etc.)
4. Implemente metodos de verificacion (IsDisplayed, GetText, etc.)

```csharp
// Pages/NuevaPaginaPage.cs
public class NuevaPaginaPage : BasePage
{
    private const string ElementoId = "NuevaPagina_Elemento";

    public bool IsElementoDisplayed()
    {
        return IsDisplayed(ElementoId, 5);
    }

    public void ClickElemento()
    {
        Click(ElementoId);
    }
}
```

### 7.2 Crear Nuevos Tests

1. Cree una nueva clase en `/Tests` con la nomenclatura `[Nombre]Tests.cs`
2. Use el atributo `[TestFixture]` para la clase
3. Implemente metodos con `[Test]` para cada caso de prueba

```csharp
// Tests/NuevaPaginaTests.cs
[TestFixture]
public class NuevaPaginaTests
{
    private AppiumDriver? _driver;
    private NuevaPaginaPage? _page;

    [SetUp]
    public void SetUp()
    {
        var settings = TestSettings.FromEnvironment();
        var factory = new AppiumDriverFactory(settings);
        _driver = factory.CreateAndroidDriver();
        _page = new NuevaPaginaPage(_driver);
    }

    [TearDown]
    public void TearDown()
    {
        _driver?.Quit();
    }

    [Test]
    public void Should_Display_Element_On_Page()
    {
        Assert.That(_page!.IsElementoDisplayed(), Is.True);
    }
}
```

### 7.3 Nomenclatura de Tests

Siga la convencion `Should_[Accion]_[Resultado]`:

```
Should_Display_Login_Screen_On_App_Start
Should_Show_Error_When_Credentials_Are_Invalid
Should_Navigate_To_Home_When_Login_Succeeds
Should_Display_Product_List_After_Search
```

### 7.4 Agregar AutomationIds a Nuevas Vistas

Si creo una nueva vista y necesita agregar AutomationIds:

1. Abra el archivo `.xaml` de la vista
2. Agregue el atributo `AutomationId="Nombre_Unico"` a cada control que necesite testear
3. Use convenciones descriptivas: `{Pagina}_{Elemento}`

```xml
<!-- Ejemplo en MiPagina.xaml -->
<Entry AutomationId="MiPagina_CampoNombre" ... />
<Button AutomationId="MiPagina_BotonGuardar" ... />
```

## 8. Configuracion para CI/CD

### 8.1 GitHub Actions (ejemplo)

```yaml
name: UI Tests

on: [push, pull_request]

jobs:
  ui-tests:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'

    - name: Install Appium
      run: |
        npm install -g appium
        appium driver install uiautomator2

    - name: Start Appium
      run: appium --address 0.0.0.0 --port 4723 &
      sleep 5

    - name: Run Tests
      run: dotnet test FerreteriaInventario.UITests --logger "trx;LogFileName=results.trx"
      env:
        APPIUM_SERVER_URL: http://localhost:4723
        TEST_USERNAME: admin
        TEST_PASSWORD: Admin123*

    - name: Upload Test Results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: test-results
        path: |
          TestResults/*.trx
          TestResults/Screenshots/*.png
```

### 8.2 Recomendaciones para CI

1. **Use agentes efimeros** - Cada corrida debe partir de un estado limpio
2. **Inicie Appium en el pipeline** - Use un paso que arranque el servidor
3. **Publique artifacts** - Guarde screenshots y resultados de tests
4. **No guarde secretos en el codigo** - Use variables de entorno del pipeline
5. **Separe tests rapidos de lentos** - Use categorias de NUnit si es necesario

## 9. Solucion de Problemas Comunes

### Problema: "Unable to connect to Appium server"
**Solucion:**
- Verifique que Appium este ejecutandose: `appium`
- Verifique que la URL sea correcta (por defecto `http://localhost:4723`)
- Verifique el firewall y que el puerto 4723 este disponible

### Problema: "Element not found"
**Solucion:**
- Verifique que el `AutomationId` este correctamente agregado en el XAML
- Aumente el timeout en `TestSettings.cs`
- Use `IsDisplayed()` para verificar que el elemento esta visible antes de interactuar

### Problema: "Appium driver creation failed"
**Solucion:**
- Verifique que el emulador/dispositivo este conectado: `adb devices`
- Verifique que el `deviceName` en las settings coincida con el dispositivo
- Verifique que las capabilities esten correctamente configuradas

### Problema: "Build failed - missing NuGet packages"
**Solucion:**
```bash
dotnet restore FerreteriaInventario.UITests
dotnet build FerreteriaInventario.UITests
```

### Problema: "Tests timeout"
**Solucion:**
- Reduzca la complejidad de los tests
- Aumente el timeout en `WaitHelpers.cs`
- Verifique que la API este responding correctamente

## 10. Limitaciones Conocidas

1. **Tests de iOS requieren Mac** - No se pueden ejecutar en Windows/Linux CI
2. **No hay credenciales en el codigo** - Necesita configurar variables de entorno
3. **API debe estar ejecutandose** - Los tests de login requieren conexion a la API
4. **Android emulator debe estar ejecutandose** - Manual o via avdmanager

## 11. Proximos Pasos Sugeridos

### Flujos Prioritarios para Automatizar

1. **Login completo** - Login exitoso y fallido con validacion de errores
2. **Navegacion del menu** - Verificar que todos los menus sean accesibles
3. **CRUD de Productos** - Crear, editar y buscar productos
4. **Registro de Ventas** - Completar una venta de principio a fin
5. **Registro de Compras** - Completar una compra de principio a fin
6. **Reportes** - Verificar que los reportes se generen correctamente

### Mejoras Futuras

- Implementar tests paralelos con NUnit
- Agregar integracion con test management tools
- Implementar data-driven tests desde archivos externos
- Crear tests de rendimiento y load testing
- Agregar validacion de accesibilidad

## 12. Archivos Creados

| Archivo | Proposito |
|---------|-----------|
| `FerreteriaInventario.UITests.csproj` | Proyecto de tests con dependencias NuGet |
| `AppiumDriverFactory.cs` | Fabrica de drivers para Android/iOS |
| `TestSettings.cs` | Configuracion centralizada desde variables de entorno |
| `WaitHelpers.cs` | Helpers para esperas robustas |
| `ScreenshotHelper.cs` | Captura de screenshots en fallos |
| `BasePage.cs` | Clase base para Page Objects |
| `LoginPage.cs` | Page Object para login |
| `DashboardPage.cs` | Page Object para dashboard |
| `ProductosPage.cs` | Page Object para productos |
| `VentasPage.cs` | Page Object para ventas |
| `LoginTests.cs` | Tests de autenticacion |
| `NavigationTests.cs` | Tests de navegacion |
| `SmokeTests.cs` | Smoke tests generales |

## 13. Contacto y Soporte

Para dudas o problemas con los tests:
1. Revise este documento completo
2. Verifique la configuracion de entorno
3. Revise los logs de Appium
4. Verifique los screenshots capturados en `TestResults/Screenshots/`