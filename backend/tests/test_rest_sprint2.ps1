[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$baseUrl = "http://localhost:5050"
$connStr = "Server=tcp:sqldavidc3597.database.windows.net,1433;Initial Catalog=AZURETAJAMAR;Persist Security Info=False;User ID=adminsql;Password=Admin123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "--- Limpiando valoraciones previas de prueba en Trayecto 4 ---"
Add-Type -AssemblyName "System.Data"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "DELETE FROM dbo.VALORACIONES_APP WHERE TrayectoId = 4"
$deleted = $cmd.ExecuteNonQuery()
$conn.Close()
Write-Host "Registros eliminados: $deleted`n"

function Invoke-ApiPost([string]$url, [string]$json, [hashtable]$headers = @{}) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $webReq = [System.Net.HttpWebRequest]::Create($url)
    $webReq.Method = "POST"
    $webReq.ContentType = "application/json; charset=utf-8"
    foreach ($k in $headers.Keys) {
        $webReq.Headers.Add($k, $headers[$k])
    }
    $reqStream = $webReq.GetRequestStream()
    $reqStream.Write($bytes, 0, $bytes.Length)
    $reqStream.Close()

    try {
        $webResp = $webReq.GetResponse()
        $respStream = $webResp.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($respStream, [System.Text.Encoding]::UTF8)
        $body = $reader.ReadToEnd()
        $statusCode = [int]$webResp.StatusCode
        return [PSCustomObject]@{ StatusCode = $statusCode; Body = $body; Success = $true }
    } catch [System.Net.WebException] {
        $ex = $_.Exception
        if ($ex.Response -ne $null) {
            $respStream = $ex.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($respStream, [System.Text.Encoding]::UTF8)
            $body = $reader.ReadToEnd()
            $statusCode = [int]$ex.Response.StatusCode
            return [PSCustomObject]@{ StatusCode = $statusCode; Body = $body; Success = $false }
        }
        throw
    }
}

function Invoke-ApiGet([string]$url, [hashtable]$headers = @{}) {
    $webReq = [System.Net.HttpWebRequest]::Create($url)
    $webReq.Method = "GET"
    foreach ($k in $headers.Keys) {
        $webReq.Headers.Add($k, $headers[$k])
    }

    try {
        $webResp = $webReq.GetResponse()
        $respStream = $webResp.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($respStream, [System.Text.Encoding]::UTF8)
        $body = $reader.ReadToEnd()
        $statusCode = [int]$webResp.StatusCode
        return [PSCustomObject]@{ StatusCode = $statusCode; Body = $body; Success = $true }
    } catch [System.Net.WebException] {
        $ex = $_.Exception
        if ($ex.Response -ne $null) {
            $respStream = $ex.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($respStream, [System.Text.Encoding]::UTF8)
            $body = $reader.ReadToEnd()
            $statusCode = [int]$ex.Response.StatusCode
            return [PSCustomObject]@{ StatusCode = $statusCode; Body = $body; Success = $false }
        }
        throw
    }
}

Write-Host "============================================================"
Write-Host "PRUEBA 1: Login de Sara (ID 195 - Pasajera confirmada Trayecto 4)"
Write-Host "============================================================"
$loginSara = Invoke-ApiPost "$baseUrl/api/auth/login" '{"email":"sara.ulgazi@tajamar365.com","password":"12345"}'
if ($loginSara.StatusCode -ne 200) { throw "Login de Sara falló: $($loginSara.Body)" }
$saraJson = $loginSara.Body | ConvertFrom-Json
$saraToken = $saraJson.token
Write-Host "OK -> Sara autenticada (HTTP 200). IdUsuario: $($saraJson.usuario.idUsuario)"

Write-Host "`n============================================================"
Write-Host "PRUEBA 2: POST /api/valoraciones como Sara (5 estrellas)"
Write-Host "============================================================"
$saraReview = '{"trayectoId":4,"puntuacion":5,"comentario":"Excelente viaje con David, muy puntual y conducción impecable.","tags":"Puntual,ConducciónSegura,BuenaMúsica"}'
$resPostSara = Invoke-ApiPost "$baseUrl/api/valoraciones" $saraReview @{ Authorization = "Bearer $saraToken" }
Write-Host "Status Code recibido: $($resPostSara.StatusCode)"
Write-Host "Body: $($resPostSara.Body)"
if ($resPostSara.StatusCode -ne 201) { throw "Se esperaba 201 Created para la valoración de Sara" }
$val1 = $resPostSara.Body | ConvertFrom-Json
Write-Host "OK -> Creada valoración ID $($val1.id) con $($val1.puntuacion) estrellas por $($val1.pasajeroNombre)"

Write-Host "`n============================================================"
Write-Host "PRUEBA 3: Intento de re-valorar el mismo trayecto (Sara)"
Write-Host "============================================================"
$resDup = Invoke-ApiPost "$baseUrl/api/valoraciones" $saraReview @{ Authorization = "Bearer $saraToken" }
Write-Host "Status Code recibido: $($resDup.StatusCode)"
Write-Host "Body: $($resDup.Body)"
if ($resDup.StatusCode -ne 400 -and $resDup.StatusCode -ne 409) { throw "Se esperaba error 400 o 409 por duplicado" }
Write-Host "OK -> Rechazado correctamente por duplicado."

Write-Host "`n============================================================"
Write-Host "PRUEBA 4: Intento de valorar por usuario sin reserva (Bryan ID 196)"
Write-Host "============================================================"
$loginBryan = Invoke-ApiPost "$baseUrl/api/auth/login" '{"email":"bryanjefferson.rocabado@tajamar365.com","password":"12345"}'
$bryanJson = $loginBryan.Body | ConvertFrom-Json
$bryanToken = $bryanJson.token
$resNoPass = Invoke-ApiPost "$baseUrl/api/valoraciones" '{"trayectoId":4,"puntuacion":5,"comentario":"No viajé pero valoro","tags":"Test"}' @{ Authorization = "Bearer $bryanToken" }
Write-Host "Status Code recibido: $($resNoPass.StatusCode)"
Write-Host "Body: $($resNoPass.Body)"
if ($resNoPass.StatusCode -ne 400 -and $resNoPass.StatusCode -ne 403) { throw "Se esperaba error 400 o 403" }
Write-Host "OK -> Rechazado correctamente porque no tiene reserva confirmada."

Write-Host "`n============================================================"
Write-Host "PRUEBA 4b: Intento de autovaloración por el conductor (David ID 212)"
Write-Host "============================================================"
$loginDavid = Invoke-ApiPost "$baseUrl/api/auth/login" '{"email":"david.castellanos@tajamar365.com","password":"12345"}'
$davidJson = $loginDavid.Body | ConvertFrom-Json
$davidToken = $davidJson.token
$resAutoVal = Invoke-ApiPost "$baseUrl/api/valoraciones" '{"trayectoId":4,"puntuacion":5,"comentario":"Soy el conductor","tags":"Test"}' @{ Authorization = "Bearer $davidToken" }
Write-Host "Status Code recibido: $($resAutoVal.StatusCode)"
Write-Host "Body: $($resAutoVal.Body)"
if ($resAutoVal.StatusCode -ne 400 -and $resAutoVal.StatusCode -ne 403) { throw "Se esperaba error 400 o 403" }
Write-Host "OK -> Rechazado correctamente al conductor."

Write-Host "`n============================================================"
Write-Host "PRUEBA 5: Segunda valoración por Matheus (ID 194) (4 estrellas)"
Write-Host "============================================================"
$loginMatheus = Invoke-ApiPost "$baseUrl/api/auth/login" '{"email":"matheus.ferreira@tajamar365.com","password":"12345"}'
$matheusJson = $loginMatheus.Body | ConvertFrom-Json
$matheusToken = $matheusJson.token
$matheusReview = '{"trayectoId":4,"puntuacion":4,"comentario":"Muy buen trayecto, puntual y buen conductor.","tags":"Puntual,Recomendable"}'
$resMatheus = Invoke-ApiPost "$baseUrl/api/valoraciones" $matheusReview @{ Authorization = "Bearer $matheusToken" }
Write-Host "Status Code recibido: $($resMatheus.StatusCode)"
Write-Host "Body: $($resMatheus.Body)"
if ($resMatheus.StatusCode -ne 201) { throw "Se esperaba 201 Created para Matheus" }
Write-Host "OK -> Segunda valoración creada correctamente."

Write-Host "`n============================================================"
Write-Host "PRUEBA 6: GET /api/conductores/212/resenas (David Castellanos)"
Write-Host "============================================================"
$resenasGet = Invoke-ApiGet "$baseUrl/api/conductores/212/resenas"
Write-Host "Status Code recibido: $($resenasGet.StatusCode)"
Write-Host "Body: $($resenasGet.Body)"
if ($resenasGet.StatusCode -ne 200) { throw "Se esperaba 200 OK en GET resenas" }
$resenasObj = $resenasGet.Body | ConvertFrom-Json
Write-Host "Conductor: $($resenasObj.conductorNombre), Puntuación Media: $($resenasObj.puntuacionMedia), Total Reseñas: $($resenasObj.totalResenas)"
Write-Host "Desglose Estrellas: 1=$($resenasObj.desgloseEstrellas.'1'), 2=$($resenasObj.desgloseEstrellas.'2'), 3=$($resenasObj.desgloseEstrellas.'3'), 4=$($resenasObj.desgloseEstrellas.'4'), 5=$($resenasObj.desgloseEstrellas.'5')"
Write-Host "OK -> GET /api/conductores/212/resenas validado con éxito."

Write-Host "`n============================================================"
Write-Host "PRUEBA 7: GET /api/trayectos (Comprobar conductorPuntuacionMedia)"
Write-Host "============================================================"
$trayectosGet = Invoke-ApiGet "$baseUrl/api/trayectos"
Write-Host "Status Code: $($trayectosGet.StatusCode)"
$trayectosObj = $trayectosGet.Body | ConvertFrom-Json
$tDavid = $trayectosObj.elementos | Where-Object { $_.conductorNombre -like "*David*" } | Select-Object -First 1
Write-Host "Trayecto de $($tDavid.conductorNombre): conductorPuntuacionMedia = $($tDavid.conductorPuntuacionMedia), conductorTotalResenas = $($tDavid.conductorTotalResenas)"
if ($tDavid.conductorPuntuacionMedia -ne 4.5) { throw "Se esperaba conductorPuntuacionMedia 4.5" }
Write-Host "OK -> GET /api/trayectos incluye correctamente la puntuación media en la tarjeta."

Write-Host "`n============================================================"
Write-Host "PRUEBA 8: [BE-D10] GET /api/trayectos/4 (Detalle con estimacionCoste)"
Write-Host "============================================================"
$t4Get = Invoke-ApiGet "$baseUrl/api/trayectos/4"
Write-Host "Status Code: $($t4Get.StatusCode)"
Write-Host "Body: $($t4Get.Body)"
if ($t4Get.StatusCode -ne 200) { throw "Se esperaba 200 OK en GET /api/trayectos/4" }
$t4Obj = $t4Get.Body | ConvertFrom-Json
if ($null -eq $t4Obj.estimacionCoste) { throw "Falta el bloque estimacionCoste en TrayectoDetalleDto" }
Write-Host "Estimación Coste Trayecto 4:"
Write-Host " - Distancia: $($t4Obj.estimacionCoste.distanciaKm) km"
Write-Host " - Coste Total: $($t4Obj.estimacionCoste.costeTotalTrayecto) €"
Write-Host " - Plazas Ocupadas: $($t4Obj.estimacionCoste.plazasOcupadas)"
Write-Host " - Coste Por Plaza: $($t4Obj.estimacionCoste.costePorPlaza) €"
Write-Host " - Concepto Bizum: $($t4Obj.estimacionCoste.conceptoBizum)"
Write-Host " - Teléfono Bizum: $($t4Obj.estimacionCoste.telefonoBizum)"
Write-Host " - Ahorro vs Taxi: $($t4Obj.estimacionCoste.ahorroEstimadoVsTaxi) €"
Write-Host " - CO2 Ahorrado: $($t4Obj.estimacionCoste.co2AhorradoKg) kg"
Write-Host "OK -> Bloque estimacionCoste verificado en GET /api/trayectos/4."

Write-Host "`n============================================================"
Write-Host "PRUEBA 9: [BE-D10] GET /api/trayectos/calcular-coste?origen=Vallecas&destino=Tajamar&plazas=3"
Write-Host "============================================================"
$calcGet = Invoke-ApiGet "$baseUrl/api/trayectos/calcular-coste?origen=Vallecas&destino=Tajamar&plazas=3"
Write-Host "Status Code: $($calcGet.StatusCode)"
Write-Host "Body: $($calcGet.Body)"
if ($calcGet.StatusCode -ne 200) { throw "Se esperaba 200 OK en calcular-coste" }
$calcObj = $calcGet.Body | ConvertFrom-Json
Write-Host "Cálculo obtenido:"
Write-Host " - Distancia: $($calcObj.distanciaKm) km"
Write-Host " - Coste Total: $($calcObj.costeTotalTrayecto) €"
Write-Host " - Plazas: $($calcObj.plazasOcupadas)"
Write-Host " - Coste Por Plaza: $($calcObj.costePorPlaza) €"
Write-Host " - Concepto Bizum: $($calcObj.conceptoBizum)"
Write-Host " - Ahorro vs Taxi: $($calcObj.ahorroEstimadoVsTaxi) €"
Write-Host " - CO2 Ahorrado: $($calcObj.co2AhorradoKg) kg"
Write-Host "OK -> Endpoint GET /api/trayectos/calcular-coste verificado con éxito."

Write-Host "`n============================================================"
Write-Host "¡TODAS LAS PRUEBAS REST EN VIVO DE BE-D09 Y BE-D10 HAN SIDO CERTIFICADAS AL 100%!"
Write-Host "============================================================"
