# API Response Format Guide

## Overview

Barcha API endpointlar uchun **unified response format** qo'llanilyapti. Bu Flutter developer uchun response handling ni osonlashtiradi va barcha javoblar bir xil strukturaga ega bo'ladi.

## Response Structure

### Success Response

```json
{
  "status": true,
  "message": "Success message",
  "data": {
    // actual data here
  }
}
```

### Error Response

```json
{
  "status": false,
  "message": "Error message describing what went wrong",
  "data": null
}
```

## Key Points

1. **`status`** - `true` (success) yoki `false` (error)
2. **`message`** - Success yoki error message
3. **`data`** - Success bo'lsa data, error bo'lsa `null`

## Examples

### 1. Send OTP (Success)

**Request:**
```http
POST /api/sms/send-verification-code
Content-Type: application/json

{
  "phone_number": "+998901234567"
}
```

**Response (200 OK):**
```json
{
  "status": true,
  "message": "Tasdiqlash kodi yuborildi",
  "data": {
    "code": "123456",
    "expires_at": "2026-01-27T10:35:00Z"
  }
}
```

### 2. Send OTP (Validation Error)

**Request:**
```http
POST /api/sms/send-verification-code
Content-Type: application/json

{
  "phone_number": "invalid"
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Telefon raqami formati noto'g'ri",
  "data": null
}
```

### 3. Login (Success)

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "phone": "+998901234567",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "status": true,
  "message": "Login successful",
  "data": {
    "access_token": "eyJhbGci...",
    "refresh_token": "eyJhbGci...",
    "user": {
      "id": 1,
      "phone": "+998901234567",
      "first_name": "John",
      "last_name": "Doe"
    }
  }
}
```

### 4. Login (Error)

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "phone": "+998901234567",
  "password": "wrong_password"
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Telefon yoki parol noto'g'ri",
  "data": null
}
```

## Flutter Integration

### Response Model

```dart
class ApiResponse<T> {
  final bool status;
  final String message;
  final T? data;

  ApiResponse({
    required this.status,
    required this.message,
    this.data,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Object? json)? fromJsonT,
  ) {
    return ApiResponse<T>(
      status: json['status'] as bool,
      message: json['message'] as String,
      data: json['data'] != null && fromJsonT != null
          ? fromJsonT(json['data'])
          : null,
    );
  }
}
```

### Usage Example

```dart
// Send OTP
Future<ApiResponse<OtpData>> sendOtp(String phoneNumber) async {
  try {
    final response = await dio.post(
      '/api/sms/send-verification-code',
      data: {'phone_number': phoneNumber},
    );

    return ApiResponse<OtpData>.fromJson(
      response.data,
      (json) => OtpData.fromJson(json as Map<String, dynamic>),
    );
  } on DioException catch (e) {
    // Error response ham bir xil format
    if (e.response != null) {
      return ApiResponse<OtpData>.fromJson(e.response!.data, null);
    }
    return ApiResponse(
      status: false,
      message: 'Network error',
      data: null,
    );
  }
}

// Using the response
final result = await sendOtp('+998901234567');
if (result.status) {
  print('Code: ${result.data?.code}');
} else {
  print('Error: ${result.message}');
}
```

## All HTTP Status Codes

Response status code ham muhim:

- **200 OK** - Success
- **201 Created** - Resource created successfully
- **400 Bad Request** - Validation error yoki business logic error
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Access denied
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

**IMPORTANT:** Status code `400` yoki `500` bo'lsa ham, response body bir xil formatda keladi:
```json
{
  "status": false,
  "message": "Error description",
  "data": null
}
```

## Validation Errors

Validation errorlar ham bir xil formatda:

**Request:**
```json
{
  "phone_number": ""
}
```

**Response (400):**
```json
{
  "status": false,
  "message": "Telefon raqami kiritilishi shart",
  "data": null
}
```

Multiple validation errors birlashtiriladi:
```json
{
  "status": false,
  "message": "Telefon raqami kiritilishi shart, Tasdiqlash kodi kiritilishi shart",
  "data": null
}
```

## Migration Guide for Existing Controllers

Eski controllerlarni yangilash uchun:

### Before:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    var result = await _authService.LoginAsync(dto);

    if (result.IsSuccess)
        return Ok(result);

    return BadRequest(result);
}
```

### After:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    var result = await _authService.LoginAsync(dto);
    return HandleResult(result);
}
```

`HandleResult()` metod avtomatik ravishda `ApiResponse` formatiga o'tkazadi.

## Notes

- Barcha response property nomlar **snake_case** formatda (`phone_number`, `access_token`, `first_name`)
- Validation filter avtomatik ishga tushadi
- Global exception handler barcha uncaught exceptionlarni tutadi
- Har bir controller uchun `HandleResult()` ishlatish tavsiya etiladi
