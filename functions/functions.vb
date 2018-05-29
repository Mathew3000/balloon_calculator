Imports System.Security.Cryptography
Imports System.Text
Imports System.Environment
Imports System.IO

Public Class Functions
    Shared base_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator"
    Shared pred_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator\preds"
    Shared py_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator\python"
    Public Class model_data
        'Time values
        Public hour As Integer = 0
        Public min As Integer = 0
        Public sec As Integer = 0
        Public month As Integer = 0
        Public day As Integer = 0
        Public year As Integer = 0
        Public time As Integer = 0
        'Geographic values
        Public lat As Double = 0.0
        Public lon As Double = 0.0
        Public delta_lat As Double = 0.0
        Public delta_lon As Double = 0.0
        Public altitude As Double = 0.0 'Start altitude
        'Balloon values
        Public ascent As Double = 0.0
        Public lift As Double = 0.0
        Public l_volume As Double = 0.0 'Launch volume
        Public drag As Double = 0.0
        Public burst As Double = 0.0 'Burst altitude
        'Others
        Public wind_error As Integer = 0
        Public software As String = ""
        Public timestamp As String = ""
        Public delta_time As Double = 0.0
        Public uuid As String = ""
    End Class

    Public Class flight_data
        Public lat As String = ""
        Public lon As String = ""
        Public height As String = ""
        Public time As String = ""
    End Class
End Class
