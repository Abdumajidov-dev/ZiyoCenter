# Expected Response Format - Exact Examples

## Test 1: Invalid Phone Format (String)

**Request:**
```bash
POST /api/sms/send-verification-code
Content-Type: application/json

{
  "phone_number": "invalid"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number must be in format +998XXXXXXXXX",
  "data": null
}
```

---

## Test 2: Empty Phone Number

**Request:**
```json
{
  "phone_number": ""
}
```

**Expected Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number is required",
  "data": null
}
```

---

## Test 3: Valid Phone - Success

**Request:**
```json
{
  "phone_number": "+998901234567"
}
```

**Expected Response (200 OK):**
```json
{
  "status": true,
  "message": "OTP code sent successfully",
  "data": {
    "code": "123456",
    "expires_at": "2026-01-27T20:45:00Z"
  }
}
```

---

## Test 4: Wrong Number Format (5 digits)

**Request:**
```json
{
  "phone_number": "12345"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "status": false,
  "message": "Phone number must be in format +998XXXXXXXXX",
  "data": null
}
```

---

## IMPORTANT CHECKS

✅ **status** field must be `true` or `false` (boolean)
✅ **message** field must contain error description in English
✅ **data** field must be `null` for errors
✅ HTTP status code must be `400` for validation errors
✅ NO extra fields like "type", "title", "errors", "traceId"

---

## If you see this OLD format (WRONG):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PhoneNumber": [
      "Phone number must be in format +998XXXXXXXXX"
    ]
  },
  "traceId": "00-xxx"
}
```

This means:
1. ValidationFilter is NOT working
2. Default ASP.NET validation is still active
3. Need to check Program.cs configuration
