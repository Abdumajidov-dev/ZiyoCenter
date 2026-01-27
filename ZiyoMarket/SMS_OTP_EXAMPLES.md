# SMS OTP API Examples

## 1. Send Verification Code

### Endpoint
```
POST /api/sms/send-verification-code
```

### Request Body
```json
{
  "phone_number": "+998901234567"
}
```

---

## Success Response

**HTTP Status: 200 OK**

```json
{
  "status": true,
  "message": "OTP code sent successfully",
  "data": {
    "code": "123456",
    "expires_at": "2026-01-27T20:35:00Z"
  }
}
```

---

## Error Responses

### 1. Missing Phone Number

**Request:**
```json
{
  "phone_number": ""
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number is required",
  "data": null
}
```

---

### 2. Invalid Phone Format

**Request:**
```json
{
  "phone_number": "invalid"
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number must be in format +998XXXXXXXXX",
  "data": null
}
```

---

### 3. Invalid Phone Format (Numeric but Wrong)

**Request:**
```json
{
  "phone_number": "12345"
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number must be in format +998XXXXXXXXX",
  "data": null
}
```

---

## 2. Verify Code

### Endpoint
```
POST /api/sms/verify-code
```

### Request Body
```json
{
  "phone_number": "+998901234567",
  "code": "123456"
}
```

---

## Success Response

**HTTP Status: 200 OK**

```json
{
  "status": true,
  "message": "Verification code verified successfully",
  "data": true
}
```

---

## Error Responses

### 1. Invalid Code

**Request:**
```json
{
  "phone_number": "+998901234567",
  "code": "000000"
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Invalid verification code",
  "data": null
}
```

---

### 2. Expired Code

**Request:**
```json
{
  "phone_number": "+998901234567",
  "code": "123456"
}
```

**Response (400 Bad Request - after 5 minutes):**
```json
{
  "status": false,
  "message": "Verification code expired or not found",
  "data": null
}
```

---

### 3. Missing Code

**Request:**
```json
{
  "phone_number": "+998901234567",
  "code": ""
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Verification code is required",
  "data": null
}
```

---

### 4. Wrong Code Format

**Request:**
```json
{
  "phone_number": "+998901234567",
  "code": "12"
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Verification code must be 6 digits",
  "data": null
}
```

---

### 5. Multiple Validation Errors

**Request:**
```json
{
  "phone_number": "",
  "code": ""
}
```

**Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number is required, Verification code is required",
  "data": null
}
```

---

## Important Notes

1. **Barcha responselar snake_case** formatda: `phone_number`, `expires_at`, `status`
2. **Success bo'lsa:** `status: true`, `data` ichida kerakli ma'lumot
3. **Error bo'lsa:** `status: false`, `data: null`, `message` da xato sababi
4. **Validation errorlar** bir nechta bo'lsa, vergul bilan ajratiladi
5. **HTTP status code** ham muhim: `200` (success), `400` (error)

## Test Scenarios

### Scenario 1: Happy Path
```
1. POST /api/sms/send-verification-code
   Request: {"phone_number": "+998901234567"}
   Response: {"status": true, "data": {"code": "123456", ...}}

2. POST /api/sms/verify-code
   Request: {"phone_number": "+998901234567", "code": "123456"}
   Response: {"status": true, "data": true}
```

### Scenario 2: Wrong Code
```
1. POST /api/sms/send-verification-code
   Request: {"phone_number": "+998901234567"}
   Response: {"status": true, "data": {"code": "123456", ...}}

2. POST /api/sms/verify-code
   Request: {"phone_number": "+998901234567", "code": "000000"}
   Response: {"status": false, "message": "Invalid verification code"}
```

### Scenario 3: Expired Code
```
1. POST /api/sms/send-verification-code
   Request: {"phone_number": "+998901234567"}
   Response: {"status": true, "data": {"code": "123456", ...}}

2. Wait 5+ minutes

3. POST /api/sms/verify-code
   Request: {"phone_number": "+998901234567", "code": "123456"}
   Response: {"status": false, "message": "Verification code expired or not found"}
```

### Scenario 4: Invalid Phone Format
```
1. POST /api/sms/send-verification-code
   Request: {"phone_number": "invalid"}
   Response: {"status": false, "message": "Phone number must be in format +998XXXXXXXXX"}
```
