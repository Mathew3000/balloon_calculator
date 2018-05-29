Imports System.IO
Imports System.Net
Imports System.Security.Permissions
Imports System.Text
Imports System.Environment
Imports System.Runtime.InteropServices
Imports functions.Functions
Imports System.Drawing

<PermissionSet(SecurityAction.Demand, Name:="FullTrust")>
<ComVisibleAttribute(True)>
Public Class LandingCalculator
    Shared calc_model As New model_data
    Shared flight_path As New List(Of flight_data)
    Shared CLEAN_PATH As New List(Of flight_data)

    Private sz As Size
    Private pnt As Point

    Dim base_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator"
    Dim pred_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator\preds"
    Dim flight_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator\flights"
    Dim position_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator\positions"
    Dim html_folder As String = GetFolderPath(SpecialFolder.ApplicationData) & "\LandingCalculator\html"
    Dim image_folder As String = GetFolderPath(SpecialFolder.MyPictures) & "\LandingCalculator\captures"

    Public Const GA As Double = 9.80665
    Public Const RHO_A As Double = 1.205
    Dim RHO_G As Double = 0.1786
    Dim ADM As Double = 7238.3
    Dim CD As Double = 0.3
    Dim BD As Double = 7.86

    Dim START_HEIGHT As Double = 0.0
    Dim START_LAT As Double = 0.0
    Dim START_LON As Double = 0.0

    Dim LAUNCH_YEAR As Integer = 0
    Dim LAUNCH_MONTH As Integer = 0
    Dim LAUNCH_DAY As Integer = 0
    Dim LAUNCH_HOUR As Integer = 0
    Dim LAUNCH_MINUTE As Integer = 0
    Dim LAUNCH_SECOND As Integer = 0
    Dim DESCENT_RATE As Double = 0.0
    Dim LAUNCH_LON As Double = 0.0
    Dim LAUNCH_LAT As Double = 0.0
    Dim LAUNCH_HEIGHT As Double = 13.2
    Dim DELTA_LAT As Double = 0.0
    Dim DELTA_LON As Double = 0.0

    Dim end_height As Double = 0.0
    Dim end_lat As Double = 0.0
    Dim end_lon As Double = 0.0
    Dim pop As Double = 0.0
    Dim pop_lat As Double = 0.0
    Dim pop_lon As Double = 0.0

    Dim GAS As String = "Helium"
    Dim TYPE As String = "k200"

    Dim csv_height As New List(Of String)
    Dim csv_lat As New List(Of String)
    Dim csv_lon As New List(Of String)

    Dim calc_type As String = "single"
    Dim run_number As Integer = 1
    Dim sleeping As Boolean = True

    Dim ascent_set As Boolean = False
    Dim altitude_set As Boolean = False

    Dim altitude As Double = 0.0
    Dim ascent As Double = 0.0
    Dim payload As Double = 0.0
    Dim time As Double = 0.0
    Dim lift As Double = 0.0
    Dim free_lift As Double = 0.0
    Dim burst_volume As Double = 0.0
    Dim launch_volume As Double = 0.0
    Dim launch_radius As Double = 0.0
    Dim launch_area As Double = 0.0
    Dim dens_diff As Double = 0.0
    Dim gross_lift As Double = 0.0
    Dim tot_mass As Double = 0.0
    Dim volume_ratio As Double = 0.0

    Dim balloon As Double = 0.0
    Dim fa1 As Double = 0.0
    Dim fa2 As Double = 0.0
    Dim fa3 As Double = 0.0
    Dim fa4 As Double = 0.0

    Dim a As Double = 0.0
    Dim b As Double = 0.0
    Dim c As Double = 0.0
    Dim d As Double = 0.0
    Dim f As Double = 0.0
    Dim g As Double = 0.0
    Dim h As Double = 0.0
    Dim R As Double = 0.0
    Dim S As Double = 0.0
    Dim T As Double = 0.0
    Dim U As Double = 0.0

    Dim run_1_height = 0.0
    Dim run_2_height = 0.0
    Dim run_3_height = 0.0
    Dim run_4_height = 0.0

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        If (Not Directory.Exists(pred_folder)) Then
            Directory.CreateDirectory(pred_folder)
        End If
        If (Not Directory.Exists(position_folder)) Then
            Directory.CreateDirectory(position_folder)
        End If
        If (Not Directory.Exists(image_folder)) Then
            Directory.CreateDirectory(image_folder)
        End If
        If (Not Directory.Exists(flight_folder)) Then
            Directory.CreateDirectory(flight_folder)
        End If
        If (Not Directory.Exists(html_folder)) Then
            msg("Fehlender Ordner", "Der Ordner " & html_folder & " konnte nicht gefunden werden. Bitte erneut kopieren!")
            Application.Exit()
            End
        End If
        web_maps.Navigate(New Uri("file:///" & html_folder & "/main.html"))
        web_maps.Navigate(New Uri("file:///" & html_folder & "/main.html"))
        Me.web_maps.ObjectForScripting = Me
        save_pos.InitialDirectory = position_folder
        load_pos.InitialDirectory = position_folder
        save_flight.InitialDirectory = flight_folder
        load_flight.InitialDirectory = flight_folder
        save_image.InitialDirectory = image_folder
        set_timers(5)
    End Sub

    'Get Values from js
    Public Sub set_lat(ByVal lat As Double)
        LAUNCH_LAT = lat
        t_launch_lat.Text = lat
    End Sub
    Public Sub set_lon(ByVal lon As Double)
        LAUNCH_LON = lon
        t_launch_lon.Text = lon
    End Sub
    Public Sub set_elevation(ByVal elev As Double)
        LAUNCH_HEIGHT = elev
        t_launch_height.Text = elev
    End Sub

    Private Sub b_calc_Click(sender As Object, e As EventArgs) Handles b_calc.Click
        START_HEIGHT = t_launch_height.Text
        START_LAT = t_launch_lat.Text
        START_LON = t_launch_lon.Text
        If ck_fall.Checked = True Then
            Dim current_time = DateTime.Now
            cal_launch.SelectionStart = current_time.Date
            cal_launch.SelectionEnd = current_time.Date
            current_time = current_time.AddMinutes(2)
            t_altitude.Text = Int(t_launch_height.Text + 2)
            t_launch_h.Text = current_time.Hour
            t_launch_m.Text = current_time.Minute
            t_launch_s.Text = current_time.Second
        End If
        If calc_type = "single" Then
            If t_ascent.Text <> "" Then
                lb_sr_1.Text = t_ascent.Text & "m/s"
            Else
                lb_sr_1.Text = "5"
            End If
            lb_orange.Text = t_fact_1.Text
            calculate(1)
        End If
        If calc_type = "multi" Then
            lb_sr_1.Text = t_ascent.Text & "m/s"
            lb_sr_2.Text = t_drag2.Text & "m/s"
            lb_sr_3.Text = t_drag3.Text & "m/s"
            lb_sr_4.Text = t_drag4.Text & "m/s"
            lb_sr_5.Text = t_drag5.Text & "m/s"
            lb_orange.Text = t_fact_1.Text
            lb_pink.Text = t_fact_2.Text
            lb_blue.Text = t_fact_3.Text
            lb_red.Text = t_fact_4.Text
            b_calc.Enabled = False
            calculate(1)
            t_2.Enabled = True
            t_3.Enabled = True
            t_4.Enabled = True
            t_5.Enabled = True
            t_6.Enabled = True
            t_7.Enabled = True
            t_8.Enabled = True
            t_9.Enabled = True
            t_10.Enabled = True
            t_11.Enabled = True
            t_12.Enabled = True
            t_13.Enabled = True
            t_14.Enabled = True
            t_15.Enabled = True
            t_16.Enabled = True
            t_17.Enabled = True
            t_18.Enabled = True
            t_19.Enabled = True
            t_20.Enabled = True
        End If
    End Sub

    Public Sub calculate(run As Integer)
        run_number = run
        p_calc.Value = 0
        label_what.Text = "Berechne..."
        'Read input fields
        p_progress.Visible = True
        b_calc.Enabled = False
        If read_input() = False Then
            Exit Sub
        End If
        'First make sure everything is entered right
        If check() = False Then
            Exit Sub
        End If
        'Next set all constants to match the selected gas
        get_rho_g()
        get_bd()
        'Do some magic! (maths)
        'Convert weight to kg
        balloon = balloon / 1000
        payload = payload / 1000
        'Calculate burst volume
        burst_volume = (4.0 / 3.0) * Math.PI * Math.Pow(BD / 2.0, 3)
        p_calc.Value = 5
        If t_altitude.Text <> "" Then
            launch_volume = burst_volume * Math.Exp((-altitude) / ADM)
            launch_radius = Math.Pow((3 * launch_volume) / (4 * Math.PI), (1 / 3))
        End If
        If t_ascent.Text <> "" Then
            a = GA * (RHO_A - RHO_G) * (4.0 / 3.0) * Math.PI
            b = (-0.5) * Math.Pow(ascent, 2) * CD * RHO_A * Math.PI
            c = 0
            d = -(payload + balloon) * GA
            f = (((3 * c) / a) - (Math.Pow(b, 2) / Math.Pow(a, 2)) / 3.0)
            g = (((2 * Math.Pow(b, 3)) / Math.Pow(a, 3)) - ((9 * b * c) / (Math.Pow(a, 2))) + ((27 * d) / a)) / 27.0
            h = (Math.Pow(g, 2) / 4.0) + (Math.Pow(f, 3) / 27.0)
            If h <= 0 Then
                msg("Error", "Exakt eine reelle Wurzel erwartet!")
            End If
            R = (-0.5 * g) + Math.Sqrt(h)
            S = Math.Pow(R, (1.0 / 3.0))
            T = (-0.5 * g) - Math.Sqrt(h)
            U = Math.Pow(T, (1.0 / 3.0))
            launch_radius = (S + U) - (b / (3 * a))
        End If
        launch_area = Math.PI * Math.Pow(launch_radius, 2)
        launch_volume = (4.0 / 3.0) * Math.PI * Math.Pow(launch_radius, 3)
        dens_diff = RHO_A - RHO_G
        gross_lift = launch_volume * dens_diff
        lift = (gross_lift - balloon) * 1000
        tot_mass = balloon + payload
        free_lift = (gross_lift - tot_mass) * GA
        ascent = Math.Sqrt(free_lift / (0.5 * CD * launch_area * RHO_A))
        volume_ratio = launch_volume / burst_volume
        altitude = -(ADM) * Math.Log(volume_ratio)
        time = (altitude / ascent) / 60
        p_calc.Value = 10
        l_height.Text = altitude
        l_ascent.Text = ascent
        l_lift.Text = lift
        l_time.Text = time
        l_volume.Text = launch_volume
        l_volume_li.Text = (launch_volume * 1000)

        If run_number = 1 Then
            l_vol_1.Text = launch_volume * 1000
        End If
        If run_number = 2 Then
            l_vol_2.Text = launch_volume * 1000
        End If
        If run_number = 3 Then
            l_vol_3.Text = launch_volume * 1000
        End If
        If run_number = 4 Then
            l_vol_4.Text = launch_volume * 1000
        End If
        If run_number = 5 Then
            l_vol_5.Text = launch_volume * 1000
        End If

        If t_fact_1.Text <> "" Then
            l_height_1.Text = (altitude * (fa1))
        End If
        If t_fact_2.Text <> "" Then
            l_height_2.Text = (altitude * (fa2))
        End If
        If t_fact_3.Text <> "" Then
            l_height_3.Text = (altitude * (fa3))
        End If
        If t_fact_4.Text <> "" Then
            l_height_4.Text = (altitude * (fa4))
        End If
        p_calc.Value = 15

        calc_model.delta_time = time

        'gathering everything together
        calc_model.altitude = START_HEIGHT
        If ck_fall.Checked = True Then
            calc_model.burst = START_HEIGHT + 1
        Else
            calc_model.burst = altitude
        End If
        calc_model.ascent = ascent
        calc_model.day = LAUNCH_DAY
        calc_model.delta_lat = DELTA_LAT
        calc_model.delta_lon = DELTA_LON
        calc_model.drag = DESCENT_RATE
        calc_model.hour = LAUNCH_HOUR
        calc_model.lat = START_LAT
        calc_model.lon = START_LON
        calc_model.min = LAUNCH_MINUTE
        calc_model.month = LAUNCH_MONTH
        calc_model.sec = LAUNCH_SECOND
        'calc_model.software = >read requested software<
        calc_model.year = LAUNCH_YEAR
        'fly()
        p_calc.Value = 20
        'Run with first factor
        If run = 1 Then
            If ck_fall.Checked = True Then
                sendtoweb("5", (T_altitude.Text))
            Else
                sendtoweb(t_ascent.Text, (l_height_1.Text))
            End If
        End If
        If run = 2 Then
            sendtoweb(t_drag2.Text, (l_height_1.Text))
        End If
        If run = 3 Then
            sendtoweb(t_drag3.Text, (l_height_1.Text))
        End If
        If run = 4 Then
            sendtoweb(t_drag4.Text, (l_height_1.Text))
        End If
        If run = 5 Then
            sendtoweb(t_drag5.Text, (l_height_1.Text))
        End If
        'Run with second factor
        If run = 6 Then
            sendtoweb(t_ascent.Text, (l_height_2.Text))
        End If
        If run = 7 Then
            sendtoweb(t_drag2.Text, (l_height_2.Text))
        End If
        If run = 8 Then
            sendtoweb(t_drag3.Text, (l_height_2.Text))
        End If
        If run = 9 Then
            sendtoweb(t_drag4.Text, (l_height_2.Text))
        End If
        If run = 10 Then
            sendtoweb(t_drag5.Text, (l_height_2.Text))
        End If
        'Run with third factor
        If run = 11 Then
            sendtoweb(t_ascent.Text, (l_height_3.Text))
        End If
        If run = 12 Then
            sendtoweb(t_drag2.Text, (l_height_3.Text))
        End If
        If run = 13 Then
            sendtoweb(t_drag3.Text, (l_height_3.Text))
        End If
        If run = 14 Then
            sendtoweb(t_drag4.Text, (l_height_3.Text))
        End If
        If run = 15 Then
            sendtoweb(t_drag5.Text, (l_height_3.Text))
        End If
        'Run with fourth factor
        If run = 16 Then
            sendtoweb(t_ascent.Text, (l_height_4.Text))
        End If
        If run = 17 Then
            sendtoweb(t_drag2.Text, (l_height_4.Text))
        End If
        If run = 18 Then
            sendtoweb(t_drag3.Text, (l_height_4.Text))
        End If
        If run = 19 Then
            sendtoweb(t_drag4.Text, (l_height_4.Text))
        End If
        If run = 20 Then
            sendtoweb(t_drag5.Text, (l_height_4.Text))
        End If
    End Sub

    Public Sub sendtoweb(ascent_r As Double, w_height As Double)
        p_calc.Value = 20
        Dim burst_alt As HtmlElement = web_calc.Document.GetElementById("burst")
        Dim ascent_rate As HtmlElement = web_calc.Document.GetElementById("ascent")
        Dim time_to_burst As HtmlElement = web_calc.Document.GetElementById("ttb")
        Dim neck_lift As HtmlElement = web_calc.Document.GetElementById("nl")
        Dim launch_volume As HtmlElement = web_calc.Document.GetElementById("lv_m3")
        Dim start_lat As HtmlElement = web_calc.Document.GetElementById("lat")
        Dim start_lon As HtmlElement = web_calc.Document.GetElementById("lon")
        Dim start_height As HtmlElement = web_calc.Document.GetElementById("initial_alt")
        Dim descent_rate As HtmlElement = web_calc.Document.GetElementById("drag")
        Dim hour As HtmlElement = web_calc.Document.GetElementById("hour")
        Dim min As HtmlElement = web_calc.Document.GetElementById("min")
        Dim sec As HtmlElement = web_calc.Document.GetElementById("sec")
        Dim day As HtmlElement = web_calc.Document.GetElementById("day")
        Dim month As HtmlElement = web_calc.Document.GetElementById("month")
        Dim year As HtmlElement = web_calc.Document.GetElementById("year")

        Dim submit As HtmlElement = web_calc.Document.GetElementById("run_pred_btn")
        p_calc.Value = 25


        burst_alt.SetAttribute("value", conv(w_height))
        ascent_rate.SetAttribute("value", conv(ascent_r))
        time_to_burst.SetAttribute("value", conv(calc_model.delta_time))
        neck_lift.SetAttribute("value", conv(calc_model.lift))
        launch_volume.SetAttribute("value", conv(calc_model.l_volume))
        start_lat.SetAttribute("value", conv(calc_model.lat))
        start_lon.SetAttribute("value", conv(calc_model.lon))
        start_height.SetAttribute("value", conv(calc_model.altitude))
        descent_rate.SetAttribute("value", conv(calc_model.drag))
        hour.SetAttribute("value", conv(calc_model.hour))
        min.SetAttribute("value", conv(calc_model.min))
        sec.SetAttribute("value", conv(calc_model.sec))
        day.SetAttribute("value", conv(calc_model.day))
        month.SetAttribute("value", conv(calc_model.month))
        year.SetAttribute("value", conv(calc_model.year))

        submit.InvokeMember("click")
        p_calc.Value = 30
        t_sleep.Enabled = True

    End Sub


    Private Shared Sub msg(title, text)
        MessageBox.Show(
            text,
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button1)
    End Sub

    Private Function check()
        Dim state As Boolean = True
        'Check if boxes are empty
        If t_altitude.Text = "" Then
            altitude_set = False
        Else
            altitude_set = True
        End If
        If t_ascent.Text = "" Then
            ascent_set = False
        Else
            ascent_set = True
        End If

        'Check if everything is allowed
        If altitude_set = True And ascent_set = True Then
            msg("Fehler", "Entweder Steigrate oder Zielhöhe angeben!")
            state = False
        End If
        If altitude_set = False And ascent_set = False Then
            msg("Fehler", "Entweder Steigrate oder Zielhöhe angeben!")
            state = False
        End If
        If ascent_set = True And ascent < 0 Then
            msg("Fehler", "Die Steigrate kann nicht negativ sein!")
            state = False
        End If
        If ascent_set = True And ascent > 10 Then
            msg("Fehler", "Die Steigrate ist zu hoch! (>10m/s)")
            state = False
        End If
        If altitude_set = True And altitude < 10000 Then
            msg("Fehler", "Die Zielhöhe ist zu niedrig! (<10km)")
            state = False
        End If
        If altitude_set = True And altitude > 40000 Then
            msg("Fehler", "Die Zielhöhe ist zu hoch! (>40km)")
            state = False
        End If

        Return state
    End Function

    Private Function read_input()
        Dim state As Boolean = True
        Dim timestring As String
        Dim utc_time As System.DateTime
        'Read values
        If ck_fall.Checked Then
            altitude = t_launch_height.Text + 1
        End If
        If t_altitude.Text <> "" Then
            altitude = t_altitude.Text
        End If
        If t_ascent.Text <> "" Then
            If (run_number = 1) Or (run_number = 6) Or (run_number = 11) Or (run_number = 16) Then
                ascent = t_ascent.Text
            End If
            If (run_number = 2) Or (run_number = 7) Or (run_number = 12) Or (run_number = 17) Then
                ascent = t_drag2.Text
            End If
            If (run_number = 3) Or (run_number = 8) Or (run_number = 13) Or (run_number = 18) Then
                ascent = t_drag3.Text
            End If
            If (run_number = 4) Or (run_number = 9) Or (run_number = 14) Or (run_number = 19) Then
                ascent = t_drag4.Text
            End If
            If (run_number = 5) Or (run_number = 10) Or (run_number = 15) Or (run_number = 20) Then
                ascent = t_drag5.Text
            End If
        End If
        If (t_fact_1.Text = "") Or (t_fact_2.Text = "") Or (t_fact_3.Text = "") Or (t_fact_4.Text = "") Then
            msg("Fehlende Eingabe!", "Bitte alle Faktoren angeben!")
            state = False
        Else
            If t_fact_1.Text <> "" Then
                fa1 = Int(t_fact_1.Text) / 100
            End If
            If t_fact_2.Text <> "" Then
                fa2 = Int(t_fact_2.Text) / 100
            End If
            If t_fact_3.Text <> "" Then
                fa3 = Int(t_fact_3.Text) / 100
            End If
            If t_fact_4.Text <> "" Then
                fa4 = Int(t_fact_4.Text) / 100
            End If
        End If
        If (t_launch_h.Text = "") Or (t_launch_m.Text = "") Or (t_launch_s.Text = "") Then
            msg("Fehlende Eingabe!", "Bitte die Startzeit angeben!")
            state = False
        Else
            Try
                timestring = (t_launch_h.Text & ":" & t_launch_m.Text & ":" & t_launch_s.Text)
                utc_time = System.DateTime.Parse(timestring)
                utc_time = utc_time.ToUniversalTime()
            Catch
                msg("Fehler", "Falsches Zeitformat!")
                End
            Finally
                LAUNCH_HOUR = utc_time.Hour
                LAUNCH_MINUTE = utc_time.Minute
                LAUNCH_SECOND = utc_time.Second
            End Try
        End If
        If t_drag.Text = "" Then
            msg("Fehlende Eingabe!", "Bitte Sinkrate angeben!")
            state = False
        Else
            DESCENT_RATE = t_drag.Text
        End If
        If t_payload.Text = "" Then
            msg("Fehlende Eingabe!", "Bitte Nutzlast eingeben")
            state = False
        Else
            payload = Int(t_payload.Text)
        End If

        TYPE = c_balloon.Text
        GAS = c_gas.Text
        'Read launch values
        LAUNCH_YEAR = cal_launch.SelectionStart.Year
        LAUNCH_MONTH = cal_launch.SelectionStart.Month
        LAUNCH_DAY = cal_launch.SelectionStart.Day

        Return state
    End Function

    'Set rho_g to specify the selected gas
    Private Sub get_rho_g()
        Select Case GAS
            Case "Helium"
                RHO_G = 0.1786
            Case "Hydrogen"
                RHO_G = 0.0899
            Case "Methane"
                RHO_G = 0.6672
        End Select
    End Sub

    'Set bd according to selected balloon type
    Private Sub get_bd()
        Select Case TYPE
            'Kaymont
            Case "Kaymont-200"
                BD = 3.0
                CD = 0.25
                balloon = 200
            Case "Kaymont-300"
                BD = 3.78
                CD = 0.25
                balloon = 300
            Case "Kaymont-350"
                BD = 4.12
                CD = 0.25
                balloon = 350
            Case "Kaymont-450"
                BD = 4.72
                CD = 0.25
                balloon = 450
            Case "Kaymont-500"
                BD = 4.99
                CD = 0.25
                balloon = 500
            Case "Kaymont-600"
                BD = 6.02
                CD = 0.3
                balloon = 600
            Case "Kaymont-700"
                BD = 6.53
                CD = 0.3
                balloon = 700
            Case "Kaymont-800"
                BD = 7.0
                CD = 0.3
                balloon = 800
            Case "Kaymont-1000"
                BD = 7.86
                CD = 0.3
                balloon = 1000
            Case "Kaymont-1200"
                BD = 8.63
                CD = 0.25
                balloon = 1200
            Case "Kaymont-1500"
                BD = 9.44
                CD = 0.25
                balloon = 1500
            Case "Kaymont-2000"
                BD = 10.54
                CD = 0.25
                balloon = 2000
            Case "Kaymont-3000"
                BD = 13.0
                CD = 0.25
                balloon = 3000
                'Hwoyee
            Case "Hwoyee-100"
                BD = 2.0
                CD = 0.25
                balloon = 100
            Case "Hwoyee-200"
                BD = 3.0
                CD = 0.25
                balloon = 200
            Case "Hwoyee-300"
                BD = 3.8
                CD = 0.25
                balloon = 300
            Case "Hwoyee-350"
                BD = 4.1
                CD = 0.25
                balloon = 350
            Case "Hwoyee-400"
                BD = 4.5
                CD = 0.25
                balloon = 400
            Case "Hwoyee-500"
                BD = 5.0
                CD = 0.25
                balloon = 500
            Case "Hwoyee-600"
                BD = 5.8
                CD = 0.3
                balloon = 600
            Case "Hwoyee-750"
                BD = 6.5
                CD = 0.3
                balloon = 750
            Case "Hwoyee-800"
                BD = 6.8
                CD = 0.3
                balloon = 800
            Case "Hwoyee-950"
                BD = 7.2
                CD = 0.3
                balloon = 950
            Case "Hwoyee-1000"
                BD = 7.5
                CD = 0.3
                balloon = 1000
            Case "Hwoyee-1200"
                BD = 8.5
                CD = 0.25
                balloon = 1200
            Case "Hwoyee-1500"
                BD = 9.5
                CD = 0.25
                balloon = 1500
            Case "Hwoyee-1600"
                BD = 10.5
                CD = 0.25
                balloon = 1600
            Case "Hwoyee-2000"
                BD = 11.0
                CD = 0.25
                balloon = 2000
            Case "Hwoyee-3000"
                BD = 12.5
                CD = 0.25
                balloon = 3000
                'PAWAN
            Case "PAWAN-100"
                BD = 1.6
                CD = 0.25
                balloon = 100
            Case "PAWAN-350"
                BD = 4.0
                CD = 0.25
                balloon = 350
            Case "PAWAN-600"
                BD = 5.8
                CD = 0.3
                balloon = 600
            Case "PAWAN-800"
                BD = 6.6
                CD = 0.3
                balloon = 800
            Case "PAWAN-900"
                BD = 7.0
                CD = 0.3
                balloon = 900
            Case "PAWAN-1200"
                BD = 8.0
                CD = 0.25
                balloon = 1200
            Case "PAWAN-1600"
                BD = 9.6
                CD = 0.25
                balloon = 1600
            Case "PAWAN-2000"
                BD = 10.2
                CD = 0.25
                balloon = 2000
        End Select
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles b_reload.Click
        web_maps.Refresh()
    End Sub

    Private Sub b_save_pos_Click(sender As Object, e As EventArgs) Handles b_save_pos.Click
        Dim lines() As String = {LAUNCH_LAT, LAUNCH_LON, LAUNCH_HEIGHT}
        Dim file_name As String = ""
        If save_pos.ShowDialog() = DialogResult.OK Then
            If save_pos.FileName = "" Then
                msg("Fehlender Dateiname!", "Bitte gültigen Dateinamen angeben!")
            Else
                If (LAUNCH_LAT <> 0.0) And (LAUNCH_LON <> 0.0) Then
                    file_name = save_pos.FileName
                    Using outputFile As New StreamWriter(Convert.ToString(file_name))
                        For Each line As String In lines
                            outputFile.WriteLine(line)
                        Next
                    End Using
                Else
                    msg("Keine Position!", "Bitte Position anwählen!")
                End If
            End If
        End If
    End Sub

    Private Sub b_load_pos_Click(sender As Object, e As EventArgs) Handles b_load_pos.Click
        Dim lines() As String
        load_pos.FileName = ""
        If load_pos.ShowDialog() = DialogResult.OK Then
            lines = File.ReadAllLines(load_pos.FileName)
            LAUNCH_LAT = lines(0)
            LAUNCH_LON = lines(1)
            LAUNCH_HEIGHT = lines(2)
            t_launch_lat.Text = LAUNCH_LAT
            t_launch_lon.Text = LAUNCH_LON
            t_launch_height.Text = LAUNCH_HEIGHT
            set_marker()
        End If
    End Sub
    Private Sub set_marker()
        Me.web_maps.Document.InvokeScript("setMarker", New String() {LAUNCH_LAT * 10000000000000, LAUNCH_LON * 10000000000000})
    End Sub

    Private Sub t_sleep_Tick(sender As Object, e As EventArgs) Handles t_sleep.Tick
        t_sleep.Enabled = False
        Dim url As String = ""
        Dim hash As String = ""
        Dim uuid As String = "uuid="
        Dim index As Integer = 0
        Dim outfile As String = ""

        url = web_calc.Url.AbsoluteUri
        index = url.IndexOf(uuid)
        hash = url.Substring(index + 5)
        Try
            Directory.CreateDirectory(pred_folder & "\" & hash)
        Catch
            msg("Fehler", "Unbekannter Fehler beim erstellen des Vorhersageordners")
            Exit Sub
        End Try

        outfile = pred_folder & "\" & hash & "\flight_path.csv"
        p_calc.Value = 35
        Try
            My.Computer.Network.DownloadFile("http://predict.habhub.org/preds/" & hash & "/flight_path.csv", outfile, userName:=String.Empty, password:=String.Empty, showUI:=False, connectionTimeout:=100000, overwrite:=True)
        Catch
            msg("Fehler", "Unbekannter Fehler bein herunterladen des Flugplans")
        End Try
        p_calc.Value = 40
        read_csv(outfile)
        p_calc.Value = 55
        'create_path()
        set_markers()
    End Sub

    Private Sub read_csv(file As String)
        Dim data() As String
        Dim i As Integer = 0
        Dim csv_dat As New flight_data
        flight_path.Clear()

        Using reader As New StreamReader(file)
            While Not reader.EndOfStream()
                data = reader.ReadLine().Split(","c)
                csv_lat.Add(data(1).Trim())
                csv_lon.Add(data(2).Trim())
                csv_height.Add(data(3).Trim())
            End While
        End Using
        p_calc.Value = 45
        For i = 0 To csv_lat.Count() - 1
            csv_dat.height = csv_height(i)
            csv_dat.lat = csv_lat(i)
            csv_dat.lon = csv_lon(i)
            flight_path.Add(csv_dat)
        Next
        p_calc.Value = 55
    End Sub

    Private Sub create_path()
        msg("NO!", "NO!")
    End Sub

    Private Sub set_markers()
        Dim i As Integer = 0
        Dim length As Integer = 0

        length = flight_path.Count()
        pop = 0

        For i = 0 To length - 1
            If Convert.ToDouble(csv_height(i).Replace("."c, ","c)) > pop Then
                pop = csv_height(i).Replace("."c, ","c)
                pop_lat = csv_lat(i).Replace("."c, ","c)
                pop_lon = csv_lon(i).Replace("."c, ","c)
            End If
            end_height = csv_height(i).Replace("."c, ","c)
            end_lat = csv_lat(i).Replace("."c, ","c)
            end_lon = csv_lon(i).Replace("."c, ","c)
        Next
        p_calc.Value = 65

        t_pop.Enabled = True
        p_calc.Value = 70
    End Sub

    Private Sub t_end_Tick(sender As Object, e As EventArgs) Handles t_end.Tick
        t_end.Enabled = False
        If calc_type = "single" Then
            Me.web_maps.Document.InvokeScript("setEnd", New String() {end_lat * 100000, end_lon * 100000, 0})
        End If
        If calc_type = "multi" Then
            Me.web_maps.Document.InvokeScript("setEnd", New String() {end_lat * 100000, end_lon * 100000, run_number})
        End If
        p_calc.Value = 100
        label_what.Text = "Fertig"
        t_close.Enabled = True
    End Sub

    Private Function conv(line As String)
        Dim converted As String

        converted = line.Replace(","c, "."c)

        Return converted
    End Function

    Private Sub t_pop_Tick(sender As Object, e As EventArgs) Handles t_pop.Tick
        t_pop.Enabled = False
        If calc_type = "single" Then
            Me.web_maps.Document.InvokeScript("setPop", New String() {pop_lat * 100000, pop_lon * 100000})
        End If
        p_calc.Value = 90
        t_end.Enabled = True
    End Sub

    Private Sub t_close_Tick(sender As Object, e As EventArgs) Handles t_close.Tick
        t_close.Enabled = False
        p_progress.Visible = False
        b_calc.Enabled = True
    End Sub

    Private Sub cb_drag_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cb_drag.SelectedIndexChanged
        If cb_drag.Text = "single" Then
            calc_type = "single"
            t_drag2.Enabled = False
            t_drag3.Enabled = False
            t_drag4.Enabled = False
            t_drag5.Enabled = False
            t_fact_2.Enabled = False
            t_fact_3.Enabled = False
            t_fact_4.Enabled = False
        End If
        If cb_drag.Text = "multi" Then
            calc_type = "multi"
            t_drag2.Enabled = True
            t_drag3.Enabled = True
            t_drag4.Enabled = True
            t_drag5.Enabled = True
            t_fact_1.Enabled = True
            t_fact_2.Enabled = True
            t_fact_3.Enabled = True
            t_fact_4.Enabled = True
        End If
    End Sub

    Private Sub cb_time_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cb_time.SelectedIndexChanged
        set_timers(cb_time.Text)
    End Sub

    Private Sub set_timers(time As Integer)
        t_2.Interval = cb_time.Text * 1000
        t_3.Interval = cb_time.Text * 2000
        t_4.Interval = cb_time.Text * 3000
        t_5.Interval = cb_time.Text * 4000
        t_6.Interval = cb_time.Text * 5000
        t_7.Interval = cb_time.Text * 6000
        t_8.Interval = cb_time.Text * 7000
        t_9.Interval = cb_time.Text * 8000
        t_10.Interval = cb_time.Text * 9000
        t_11.Interval = cb_time.Text * 10000
        t_12.Interval = cb_time.Text * 11000
        t_13.Interval = cb_time.Text * 12000
        t_14.Interval = cb_time.Text * 13000
        t_15.Interval = cb_time.Text * 14000
        t_16.Interval = cb_time.Text * 15000
        t_17.Interval = cb_time.Text * 16000
        t_18.Interval = cb_time.Text * 17000
        t_19.Interval = cb_time.Text * 18000
        t_20.Interval = cb_time.Text * 19000
    End Sub

    Private Sub t_2_Tick(sender As Object, e As EventArgs) Handles t_2.Tick
        t_2.Enabled = False
        calculate(2)
    End Sub

    Private Sub t_3_Tick(sender As Object, e As EventArgs) Handles t_3.Tick
        t_3.Enabled = False
        calculate(3)
    End Sub

    Private Sub t_4_Tick(sender As Object, e As EventArgs) Handles t_4.Tick
        t_4.Enabled = False
        calculate(4)
    End Sub

    Private Sub t_5_Tick(sender As Object, e As EventArgs) Handles t_5.Tick
        t_5.Enabled = False
        calculate(5)
    End Sub

    Private Sub t_6_Tick(sender As Object, e As EventArgs) Handles t_6.Tick
        t_6.Enabled = False
        calculate(6)
    End Sub

    Private Sub t_7_Tick(sender As Object, e As EventArgs) Handles t_7.Tick
        t_7.Enabled = False
        calculate(7)
    End Sub

    Private Sub t_8_Tick(sender As Object, e As EventArgs) Handles t_8.Tick
        t_8.Enabled = False
        calculate(8)
    End Sub

    Private Sub t_9_Tick(sender As Object, e As EventArgs) Handles t_9.Tick
        t_9.Enabled = False
        calculate(9)
    End Sub

    Private Sub t_10_Tick(sender As Object, e As EventArgs) Handles t_10.Tick
        t_10.Enabled = False
        calculate(10)
    End Sub

    Private Sub t_11_Tick(sender As Object, e As EventArgs) Handles t_11.Tick
        t_11.Enabled = False
        calculate(11)
    End Sub

    Private Sub t_12_Tick(sender As Object, e As EventArgs) Handles t_12.Tick
        t_12.Enabled = False
        calculate(12)
    End Sub

    Private Sub t_13_Tick(sender As Object, e As EventArgs) Handles t_13.Tick
        t_13.Enabled = False
        calculate(13)
    End Sub

    Private Sub t_14_Tick(sender As Object, e As EventArgs) Handles t_14.Tick
        t_14.Enabled = False
        calculate(14)
    End Sub

    Private Sub t_15_Tick(sender As Object, e As EventArgs) Handles t_15.Tick
        t_15.Enabled = False
        calculate(15)
    End Sub

    Private Sub t_16_Tick(sender As Object, e As EventArgs) Handles t_16.Tick
        t_16.Enabled = False
        calculate(16)
    End Sub

    Private Sub t_17_Tick(sender As Object, e As EventArgs) Handles t_17.Tick
        t_17.Enabled = False
        calculate(17)
    End Sub

    Private Sub t_18_Tick(sender As Object, e As EventArgs) Handles t_18.Tick
        t_18.Enabled = False
        calculate(18)
    End Sub

    Private Sub t_19_Tick(sender As Object, e As EventArgs) Handles t_19.Tick
        t_19.Enabled = False
        calculate(19)
    End Sub

    Private Sub t_20_Tick(sender As Object, e As EventArgs) Handles t_20.Tick
        t_20.Enabled = False
        calculate(20)
        b_calc.Enabled = True
    End Sub

    Private Sub b_save_img_Click(sender As Object, e As EventArgs) Handles b_save_img.Click, bt_save_screen.Click
        Dim bmp As New Bitmap(pan_web.Width, pan_web.Height)
        Dim bmp2 As New Bitmap(web_maps.ClientRectangle.Width, web_maps.ClientRectangle.Height + 42)
        Dim file_n As String = ""
        Dim gBmp As Graphics = Graphics.FromImage(bmp2)

        ' Capture Fullsize Screen Shot
        gBmp.CopyFromScreen(web_maps.PointToScreen(New Point(0, 0)), New Point(0, 42), New Size(web_maps.Width, web_maps.Height))

        pan_web.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width - 5, 42))

        gBmp.DrawImage(bmp, -2, -2)


        If save_image.ShowDialog() = DialogResult.OK Then
            If save_image.FileName = "" Then
                msg("Fehlender Dateiname!", "Bitte gültigen Dateinamen angeben!")
            Else
                file_n = save_image.FileName
                bmp2.Save(file_n, _
                Imaging.ImageFormat.Bmp)
            End If
        End If
    End Sub

    Private Sub cb_web_timeout_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cb_web_timeout.SelectedIndexChanged
        t_sleep.Interval = cb_web_timeout.Text * 1000
    End Sub

    Private Sub bt_fullscreen_Click(sender As Object, e As EventArgs) Handles bt_fullscreen.Click
        bt_exit_fullscreen.Visible = True
        bt_save_screen.Visible = True
        bt_exit_fullscreen.Left = Screen.PrimaryScreen.Bounds.Width - 120
        Me.FormBorderStyle = FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized

        pnt = pan_web.Location
        sz = pan_web.Size

        pan_web.Parent = Me
        pan_web.BringToFront()
        pan_web.Location = Me.Location
        pan_web.Size = Me.Size

        web_maps.Width = Screen.PrimaryScreen.Bounds.Width - 10
        web_maps.Height = Screen.PrimaryScreen.Bounds.Height - 50
    End Sub

    Private Sub bt_exit_fullscreen_Click(sender As Object, e As EventArgs) Handles bt_exit_fullscreen.Click
        bt_exit_fullscreen.Visible = False
        bt_save_screen.Visible = False

        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.WindowState = FormWindowState.Normal

        pan_web.Location = pnt
        pan_web.Size = sz

        web_maps.Width = 716
        web_maps.Height = 551
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles bt_clear_map.Click
        Me.web_maps.Document.InvokeScript("clearMarkers")
    End Sub

    Private Sub ck_fall_CheckedChanged(sender As Object, e As EventArgs) Handles ck_fall.CheckedChanged
        If ck_fall.Checked = True Then
            cb_drag.Text = "single"
            calc_type = "single"
            cb_drag.Enabled = False
            t_fact_1.Text = "100"
            t_fact_1.Enabled = False
            t_fact_2.Enabled = False
            t_fact_3.Enabled = False
            t_fact_4.Enabled = False
            t_ascent.Text = ""
            t_ascent.Enabled = False
            t_launch_h.Enabled = False
            t_launch_m.Enabled = False
            t_launch_s.Enabled = False
            cal_launch.Enabled = False
            l_start_pos.Text = "Position:"
            l_start_height.Text = "Höhe[m]:"
        Else
            cb_drag.Enabled = True
            cal_launch.Enabled = True
            t_fact_1.Enabled = True
            t_ascent.Enabled = True
            t_launch_h.Enabled = True
            t_launch_m.Enabled = True
            t_launch_s.Enabled = True
            l_start_pos.Text = "Startosition:"
            l_start_height.Text = "Starthöhe[m]:"
        End If
    End Sub

    Private Sub b_save_flight_Click(sender As Object, e As EventArgs) Handles b_save_flight.Click
        Dim lines() As String = {LAUNCH_LAT, LAUNCH_LON, LAUNCH_HEIGHT, t_payload.Text, t_fact_1.Text, t_fact_2.Text, t_fact_3.Text, t_fact_4.Text, c_balloon.Text, t_launch_h.Text, t_launch_m.Text, t_launch_s.Text, cal_launch.SelectionStart.Year, cal_launch.SelectionStart.Month, cal_launch.SelectionStart.Day, t_drag.Text}
        Dim file_name As String = ""
        If save_flight.ShowDialog() = DialogResult.OK Then
            If save_flight.FileName = "" Then
                msg("Fehlender Dateiname!", "Bitte gültigen Dateinamen angeben!")
            Else
                If (LAUNCH_LAT <> 0.0) And (LAUNCH_LON <> 0.0) And (t_payload.Text <> "") And (t_fact_1.Text <> "") And (t_fact_2.Text <> "") And (t_fact_3.Text <> "") And (t_fact_4.Text <> "") And (c_balloon.Text <> "") And (t_launch_h.Text <> "") And (t_launch_m.Text <> "") And (t_launch_s.Text <> "") And (t_drag.Text <> "") Then
                    file_name = save_flight.FileName
                    Using outputFile As New StreamWriter(Convert.ToString(file_name))
                        For Each line As String In lines
                            outputFile.WriteLine(line)
                        Next
                    End Using
                Else
                    msg("Einstellungen Prüfen!", "Einstellungen nicht vollständig!")
                End If
            End If
        End If
    End Sub

    Private Sub b_load_flight_Click(sender As Object, e As EventArgs) Handles b_load_flight.Click
        Dim lines() As String
        load_flight.FileName = ""
        If load_flight.ShowDialog() = DialogResult.OK Then
            lines = File.ReadAllLines(load_flight.FileName)
            LAUNCH_LAT = lines(0)
            LAUNCH_LON = lines(1)
            LAUNCH_HEIGHT = lines(2)
            t_launch_lat.Text = LAUNCH_LAT
            t_launch_lon.Text = LAUNCH_LON
            t_launch_height.Text = LAUNCH_HEIGHT
            set_marker()
            t_payload.Text = lines(3)
            t_fact_1.Text = lines(4)
            t_fact_2.Text = lines(5)
            t_fact_3.Text = lines(6)
            t_fact_4.Text = lines(7)
            c_balloon.Text = lines(8)
            t_launch_h.Text = lines(9)
            t_launch_m.Text = lines(10)
            t_launch_s.Text = lines(11)
            cal_launch.SelectionStart =
                New Date(lines(12), lines(13), lines(14))
            cal_launch.SelectionEnd =
                New Date(lines(12), lines(13), lines(14))
            t_drag.Text = lines(15)
        End If
    End Sub

End Class
