Ejecucion de UI Tests desde Visual Studio 2022
Con Emulador Android
Requisitos:
1. Android Studio con emulador configurado (Pixel4 o superior recomendado)
2. Appium Server instalado (npm install -g appium)
3. USB debugging NO necesario para emulador
Pasos:
1. Crear y configurar emulador en Android Studio:
   - File > Settings > Android SDK > SDK Manager
   - Crear un AVD (Android Virtual Device) si no existe - Nombre sugerido: TestEmulator o Pixel_6_API_33
2. Iniciar Appium Server:
   - Abra una terminal PowerShell:
      appium --address 127.0.0.1 --port 4723
      - Mantenga esta terminal abierta
3. Iniciar el emulador Android:
   - Desde Android Studio: Tools > Device Manager > Start   - O desde linea de comandos:
      cd $env:ANDROID_HOME\emulator
   .\emulator -avd Pixel_6_API_33
      - Espere a que Android cargue completamente
4. Configurar variables de entorno en VS2022:
   - Menu: Debug > Properties > Debug Properties
   - O cree un archivo launchSettings.json en Properties del proyecto UITests:
      {
     profiles: {
       UITests: {
         commandLineArgs: --filter "Name=Should_Display_Login",
         environmentVariables: {
           APPIUM_SERVER_URL: http://localhost:4723,
           APPIUM_ANDROID_DEVICE: Pixel_6_API_33
         }
       }
     }
   }
   
5. Ejecutar desde Visual Studio:
   - Establezca FerreteriaInventario.UITests como proyecto de inicio
   - Menu Test > Run All Tests
   - O clic derecho en el proyecto > Run Tests
Con Dispositivo Android Fisico
Requisitos:
1. Dispositivo Android con开发者选项 habilitadas
2. USB debugging activado en el dispositivo
3. Drivers USB instalados (para Windows)
Pasos:
1. Habilitar USB Debugging en el dispositivo:
   - Settings > About Phone > tap7 times on Build Number
   - Settings > Developer Options > Enable USB Debugging
   - Conecte el celular via USB
2. Verificar conexion ADB:
      adb devices
      Debe mostrar algo como:
      List of devices attached
   R5CR123456789 device
   
3. Instalar Appium driver uiautomator2:
      appium driver install uiautomator2
   
4. Configurar TestSettings para dispositivo fisico:
   - Configure la variable APPIUM_ANDROID_DEVICE con el nombre del dispositivo
      $env:APPIUM_ANDROID_DEVICE="R5CR123456789"
   $env:ANDROID_APP_PATH="C:\ruta\ao\app.apk"
   
5. Ejecutar Appium Server:
      appium --address 127.0.0.1 --port 4723
   
6. Ejecutar desde Visual Studio:
   - Establezca FerreteriaInventario.UITests como proyecto de inicio
   - Presione F5 o use menu Test > Run All Tests
Diferencias Clave
Aspecto	Emulador
Velocidad	Mas lento
Consistencia	Muy consistente
Configuracion inicial	Android Studio necesario
Hardware real	No
Conexion	localhost
Configuracion de DeviceName
El deviceName en TestSettings.cs debe coincidir con:
Para emulador:
- Use el nombre del AVD: Pixel_6_API_33, TestEmulator, etc.
- Verifique con: adb devices -l
Para dispositivo fisico:
- Use el serial del dispositivo: R5CR123456789
- O configure un nombre descriptivo via adb devices -l