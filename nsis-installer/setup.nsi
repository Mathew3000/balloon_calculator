Name "Landing Calculator Setup"
OutFile "LandingCalcSetup1_2_0_1.exe"

VIProductVersion                 "1.2.0.1"
VIAddVersionKey ProductName      "Landing Calculator"
VIAddVersionKey Comments         "Erstellt für Stratos"
VIAddVersionKey CompanyName      "Matthias Naumann"
VIAddVersionKey LegalCopyright   "Matthias Naumann"
VIAddVersionKey FileDescription  "Tool zum vereinfachen der Rechnung mit http://predict.habhub.org/"

  !define MUI_ICON C:\Users\Matthias\Documents\Stratos\calculator\landingcalculator\lc2.ico
  !define MUI_UNICON C:\Users\Matthias\Documents\Stratos\calculator\landingcalculator\lc2.ico

InstallDir $PROGRAMFILES\LandingCalculator
Page directory
Page instfiles
Section
	SetOutPath $INSTDIR
	WriteUninstaller $INSTDIR\uninstaller.exe
	File C:\Users\Matthias\Documents\Stratos\calculator\landingcalculator\landingcalculator\bin\Debug\landingcalculator.exe
	File C:\Users\Matthias\Documents\Stratos\calculator\landingcalculator\functions\bin\Debug\functions.dll
	File C:\Users\Matthias\Documents\Stratos\calculator\landingcalculator\lc2.ico
	CreateShortCut "$DESKTOP\Landing Calculator.lnk" "$INSTDIR\landingcalculator.exe"
	SetOutPath "$APPDATA\LandingCalculator\html"
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\main.html
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker_end.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker_start.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker1.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker1_2.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker1_3.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker1_4.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker2.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker2_2.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker2_3.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker2_4.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker3.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker3_2.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker3_3.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker3_4.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker4.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker4_2.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker4_3.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker4_4.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker5.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker5_2.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker5_3.png
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\html\marker5_4.png
	SetOutPath "$APPDATA\LandingCalculator\positions"
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\positions\Waldhausweg.pos
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\positions\Alt_Saarbruecken.pos
	File C:\Users\Matthias\AppData\Roaming\LandingCalculator\positions\DFG.pos
SectionEnd
Section "uninstall"
	Delete "$INSTDIR\landingcalculator.exe"
	Delete "$INSTDIR\functions.dll"
	Delete "$INSTDIR\lc2.ico"
    Delete "$DESKTOP\LandingCalculator.lnk"
	Delete "$INSTDIR\uninstaller.exe"
SectionEnd