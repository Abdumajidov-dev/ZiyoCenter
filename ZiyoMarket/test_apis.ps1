$baseUrl = "http://localhost:8081/api"
$swaggerUrl = "http://localhost:8081/swagger/v1/swagger.json"

Write-Host "--- 1. Testing Swagger JSON ---"
try {
    $swagger = Invoke-RestMethod -Uri $swaggerUrl -Method Get
    Write-Host "Swagger JSON loaded successfully. Title: $($swagger.info.title)"
} catch {
    Write-Host "Failed to load Swagger JSON: $_"
}

Write-Host "`n--- 2. Testing Admin Seed ---"
try {
    $seedResult = Invoke-RestMethod -Uri "$baseUrl/AdminSeed/create-test-admin" -Method Post
    Write-Host "Admin Seed Result: $($seedResult | ConvertTo-Json -Depth 2)"
} catch {
    Write-Host "Failed to seed admin: $_"
}

Write-Host "`n--- 3. Testing Admin Login ---"
$token = ""
try {
    $loginBody = @{
        phone = "admin"
        password = "Admin@123"
    } | ConvertTo-Json
    
    $loginResult = Invoke-RestMethod -Uri "$baseUrl/auth/admin-login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResult.data.accessToken
    Write-Host "Login successful! Token acquired."
} catch {
    Write-Host "Failed to login: $_"
}

if ($token) {
    $headers = @{
        Authorization = "Bearer $token"
    }

    Write-Host "`n--- 4. Testing Get Categories (GET /api/category) ---"
    try {
        $categories = Invoke-RestMethod -Uri "$baseUrl/category" -Method Get -Headers $headers
        Write-Host "Categories count: $($categories.data.Count)"
        if ($categories.data.Count -gt 0) {
            Write-Host "Sample Category: $($categories.data[0] | ConvertTo-Json -Depth 1 -Compress)"
        }
    } catch {
        Write-Host "Failed to get categories: $_"
    }

    Write-Host "`n--- 5. Testing Get Products (GET /api/product) ---"
    try {
        $products = Invoke-RestMethod -Uri "$baseUrl/product" -Method Get -Headers $headers
        Write-Host "Products count: $($products.data.Count)"
        if ($products.data.Count -gt 0) {
            Write-Host "Sample Product: $($products.data[0].name)"
        }
    } catch {
        Write-Host "Failed to get products: $_"
    }
}
