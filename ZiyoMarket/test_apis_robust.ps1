$baseUrl = "http://localhost:8081/api"

function Test-Endpoint {
    param($Method, $Url, $Body, $Headers)
    Write-Host "-> Testing [$Method] $Url"
    try {
        if ($Body) {
            $resp = Invoke-RestMethod -Uri $Url -Method $Method -Body $Body -ContentType "application/json" -Headers $Headers
        } else {
            $resp = Invoke-RestMethod -Uri $Url -Method $Method -Headers $Headers
        }
        Write-Host "   [SUCCESS] Status: OK"
        return $resp
    } catch {
        Write-Host "   [FAILED] $_"
        return $null
    }
}

Test-Endpoint "GET" "http://localhost:8081/swagger/v1/swagger.json"

Test-Endpoint "POST" "$baseUrl/AdminSeed/create-test-admin"

$loginBody = @{ phone = "admin"; password = "Admin@123" } | ConvertTo-Json
$loginResult = Test-Endpoint "POST" "$baseUrl/auth/admin-login" $loginBody
$token = $loginResult.data.accessToken

if ($token) {
    $headers = @{ Authorization = "Bearer $token" }
    
    $cats = Test-Endpoint "GET" "$baseUrl/category" $null $headers
    if ($cats) { Write-Host "   Found $($cats.data.Count) categories." }
    
    $prods = Test-Endpoint "GET" "$baseUrl/product" $null $headers
    if ($prods) { Write-Host "   Found $($prods.data.Count) products." }

    $users = Test-Endpoint "GET" "$baseUrl/user" $null $headers
    if ($users) { Write-Host "   Found $($users.data.Count) users." }
}
