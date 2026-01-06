# 🔔 Firebase Push Notification - Professional Implementation Guide

## ✅ Yangi Yondashuv (DeviceToken Table Based)

Ushbu yondashuv professional va scalable hisoblanadi:
- ✅ Bir foydalanuvchi bir nechta qurilmaga ega bo'lishi mumkin
- ✅ Har bir qurilma alohida boshqariladi
- ✅ Token expire/cleanup avtomatik
- ✅ Device metadata saqlanadi (OS, version, etc.)

---

## Backend Implementation (ASP.NET Core) - ✅ BAJARILDI

### 1. Database Schema

**DeviceTokens** Table:
```sql
CREATE TABLE "DeviceTokens" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "UserType" INTEGER NOT NULL,  -- 1=Customer, 2=Seller, 3=Admin
    "Token" TEXT NOT NULL,         -- FCM token
    "DeviceName" TEXT,            -- "iPhone 14 Pro"
    "DeviceOs" TEXT,              -- "iOS" | "Android"
    "AppVersion" TEXT,            -- "1.0.5"
    "IsActive" BOOLEAN DEFAULT TRUE,
    "LastUsedAt" TIMESTAMP,
    "CreatedAt" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

CREATE INDEX "IX_DeviceTokens_UserId_UserType" ON "DeviceTokens"("UserId", "UserType");
CREATE INDEX "IX_DeviceTokens_Token" ON "DeviceTokens"("Token");
CREATE INDEX "IX_DeviceTokens_IsActive" ON "DeviceTokens"("IsActive");
```

### 2. Backend Endpoints

#### A. Token Management (Mobile App)

**POST** `/api/push-notification/register-token` [Authorize]
```json
// Request Body
{
  "token": "fcm_token_string_here",
  "device_name": "iPhone 14 Pro",
  "device_os": "iOS",
  "app_version": "1.0.5"
}

// Response
{
  "isSuccess": true,
  "message": "Device token registered successfully"
}
```

**GET** `/api/push-notification/my-devices` [Authorize]
```json
// Response
{
  "isSuccess": true,
  "data": [
    {
      "id": 1,
      "userId": 5,
      "userType": "Customer",
      "deviceName": "iPhone 14 Pro",
      "deviceOs": "iOS",
      "appVersion": "1.0.5",
      "isActive": true,
      "lastUsedAt": "2026-01-05T14:30:00Z",
      "createdAt": "2026-01-01 10:00:00"
    }
  ]
}
```

**POST** `/api/push-notification/logout-all-devices` [Authorize]
```json
// Response
{
  "isSuccess": true,
  "message": "3 device tokens deactivated"
}
```

#### B. Send Notifications (Admin Only)

**POST** `/api/push-notification/send` [Authorize(Roles="Admin")]
```json
// Request
{
  "user_id": 5,
  "title": "Yangi buyurtma",
  "message": "Sizning #ORD-12345 buyurtmangiz qabul qilindi",
  "data": {
    "order_id": "12345",
    "type": "order_created"
  },
  "image_url": "https://example.com/image.jpg"
}

// Response
{
  "isSuccess": true,
  "message": "Notification sent to 2/2 devices"
}
```

**POST** `/api/push-notification/send-batch` [Authorize(Roles="Admin")]
```json
// Request
{
  "user_ids": [5, 10, 15],
  "title": "Aksiya",
  "message": "50% chegirma barcha mahsulotlarga!",
  "data": {
    "promo_id": "SUMMER50",
    "type": "promotion"
  }
}

// Response
{
  "isSuccess": true,
  "message": "Notification sent to 5/6 devices across 3 users"
}
```

**POST** `/api/push-notification/send-topic` [Authorize(Roles="Admin")]
```json
// Request
{
  "topic": "all_customers",
  "title": "Yangi mahsulotlar",
  "message": "Yangi iPhone 15 mahsulotlari qo'shildi!",
  "data": {
    "category": "phones",
    "type": "new_products"
  }
}

// Response
{
  "isSuccess": true,
  "message": "Notification sent to topic 'all_customers'"
}
```

---

## Flutter Implementation

### 1. Firebase Configuration

Follow the previous `FIREBASE_INTEGRATION_GUIDE.md` for Firebase setup.

### 2. Register Device Token on Login/App Launch

**lib/services/device_token_service.dart:**
```dart
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import 'dart:io' show Platform;
import 'package:device_info_plus/device_info_plus.dart';
import 'package:package_info_plus/package_info_plus.dart';

class DeviceTokenService {
  static const String baseUrl = 'http://localhost:8080/api';

  /// Register device token after login
  static Future<void> registerDeviceToken(String accessToken) async {
    try {
      // Get FCM token
      String? fcmToken = await FirebaseMessaging.instance.getToken();
      if (fcmToken == null) {
        print('⚠️ FCM token is null');
        return;
      }

      // Get device info
      DeviceInfoPlugin deviceInfo = DeviceInfoPlugin();
      String deviceName = '';
      String deviceOs = '';

      if (Platform.isAndroid) {
        AndroidDeviceInfo androidInfo = await deviceInfo.androidInfo;
        deviceName = '${androidInfo.manufacturer} ${androidInfo.model}';
        deviceOs = 'Android';
      } else if (Platform.isIOS) {
        IosDeviceInfo iosInfo = await deviceInfo.iosInfo;
        deviceName = '${iosInfo.name} ${iosInfo.model}';
        deviceOs = 'iOS';
      }

      // Get app version
      PackageInfo packageInfo = await PackageInfo.fromPlatform();
      String appVersion = packageInfo.version;

      // Register token with backend
      final response = await http.post(
        Uri.parse('$baseUrl/push-notification/register-token'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $accessToken',
        },
        body: jsonEncode({
          'token': fcmToken,
          'device_name': deviceName,
          'device_os': deviceOs,
          'app_version': appVersion,
        }),
      );

      if (response.statusCode == 200) {
        print('✅ Device token registered successfully');
      } else {
        print('❌ Failed to register device token: ${response.body}');
      }
    } catch (e) {
      print('❌ Error registering device token: $e');
    }
  }

  /// Get user's registered devices
  static Future<List<Device>> getMyDevices(String accessToken) async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/push-notification/my-devices'),
        headers: {
          'Authorization': 'Bearer $accessToken',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        List<Device> devices = (data['data'] as List)
            .map((json) => Device.fromJson(json))
            .toList();
        return devices;
      } else {
        throw Exception('Failed to load devices');
      }
    } catch (e) {
      print('❌ Error getting devices: $e');
      return [];
    }
  }

  /// Logout from all devices
  static Future<void> logoutAllDevices(String accessToken) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/push-notification/logout-all-devices'),
        headers: {
          'Authorization': 'Bearer $accessToken',
        },
      );

      if (response.statusCode == 200) {
        print('✅ Logged out from all devices');
      } else {
        print('❌ Failed to logout: ${response.body}');
      }
    } catch (e) {
      print('❌ Error logging out: $e');
    }
  }
}

class Device {
  final int id;
  final String deviceName;
  final String deviceOs;
  final String appVersion;
  final bool isActive;
  final DateTime? lastUsedAt;

  Device({
    required this.id,
    required this.deviceName,
    required this.deviceOs,
    required this.appVersion,
    required this.isActive,
    this.lastUsedAt,
  });

  factory Device.fromJson(Map<String, dynamic> json) {
    return Device(
      id: json['id'],
      deviceName: json['device_name'] ?? '',
      deviceOs: json['device_os'] ?? '',
      appVersion: json['app_version'] ?? '',
      isActive: json['is_active'] ?? true,
      lastUsedAt: json['last_used_at'] != null
          ? DateTime.parse(json['last_used_at'])
          : null,
    );
  }
}
```

**pubspec.yaml:**
```yaml
dependencies:
  firebase_core: ^3.8.1
  firebase_messaging: ^15.1.5
  flutter_local_notifications: ^18.0.1
  device_info_plus: ^11.2.0  # ✅ Add this
  package_info_plus: ^8.1.2   # ✅ Add this
  http: ^1.2.2
```

### 3. Call on Login

**lib/services/auth_service.dart:**
```dart
import 'device_token_service.dart';

class AuthService {
  static Future<void> login({
    required String phoneOrEmail,
    required String password,
  }) async {
    // ... existing login logic ...

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body)['data'];
      String accessToken = data['access_token'];

      // ✅ Register device token after login
      await DeviceTokenService.registerDeviceToken(accessToken);

      // Subscribe to topics
      await FirebaseMessaging.instance.subscribeToTopic('all_customers');

      print('✅ Login successful');
    } else {
      throw Exception('Login failed: ${response.body}');
    }
  }
}
```

### 4. Call on App Launch

**lib/main.dart:**
```dart
import 'services/device_token_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  await Firebase.initializeApp();
  FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);
  await FirebaseService.initialize();

  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  @override
  void initState() {
    super.initState();
    _checkAndRegisterToken();
  }

  /// Check if user is logged in, and register token
  Future<void> _checkAndRegisterToken() async {
    String? accessToken = await getStoredAccessToken(); // Your method to get token

    if (accessToken != null && accessToken.isNotEmpty) {
      // ✅ Re-register token on app launch
      await DeviceTokenService.registerDeviceToken(accessToken);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ZiyoMarket',
      home: const HomePage(),
    );
  }
}
```

---

## Testing

### 1. Postman/Swagger Test

1. Login as customer and get access token
2. Register device token:
```bash
POST http://localhost:8080/api/push-notification/register-token
Authorization: Bearer {customer_token}

{
  "token": "test_fcm_token_123",
  "device_name": "Test Device",
  "device_os": "Android",
  "app_version": "1.0.0"
}
```

3. Login as admin and send notification:
```bash
POST http://localhost:8080/api/push-notification/send
Authorization: Bearer {admin_token}

{
  "user_id": 1,
  "title": "Test Notification",
  "message": "This is a test",
  "data": {
    "test": "true"
  }
}
```

### 2. Flutter Test

1. Run app on physical device
2. Login with customer account
3. Check backend database for registered token:
```sql
SELECT * FROM "DeviceTokens" WHERE "UserId" = 1;
```

4. Send test notification from admin account
5. Verify notification appears on device

---

## Key Differences from Old Approach

| Feature | Old (FcmToken in Customer table) | New (DeviceToken table) |
|---------|----------------------------------|-------------------------|
| Multiple Devices | ❌ No (1 token per user) | ✅ Yes (N tokens per user) |
| Device Management | ❌ No tracking | ✅ Full device metadata |
| Token Expiry | ❌ Manual | ✅ Auto-cleanup (60 days) |
| Logout All Devices | ❌ Not supported | ✅ Supported |
| Scalability | ❌ Limited | ✅ High |
| Professional | ❌ Basic | ✅ Enterprise-grade |

---

## Migration from Old to New Approach

If you have existing `Customer.FcmToken` data:

```sql
-- Migrate existing tokens to DeviceTokens table
INSERT INTO "DeviceTokens" ("UserId", "UserType", "Token", "DeviceName", "DeviceOs", "AppVersion", "IsActive", "LastUsedAt", "CreatedAt")
SELECT
    "Id" as "UserId",
    1 as "UserType",  -- 1 = Customer
    "FcmToken" as "Token",
    'Unknown Device' as "DeviceName",
    'Unknown' as "DeviceOs",
    '1.0.0' as "AppVersion",
    TRUE as "IsActive",
    NOW() as "LastUsedAt",
    NOW()::TEXT as "CreatedAt"
FROM "Customers"
WHERE "FcmToken" IS NOT NULL AND "FcmToken" != '';

-- Optional: Remove old FcmToken column (after migration)
ALTER TABLE "Customers" DROP COLUMN "FcmToken";
```

---

## Admin API Examples (Send Notifications)

### Example 1: Order Created
```json
POST /api/push-notification/send
{
  "user_id": 5,
  "title": "Yangi buyurtma",
  "message": "Sizning #ORD-12345 buyurtmangiz qabul qilindi",
  "data": {
    "order_id": "12345",
    "type": "order_created",
    "action_url": "/orders/12345"
  }
}
```

### Example 2: Cashback Earned
```json
POST /api/push-notification/send
{
  "user_id": 5,
  "title": "Cashback qo'shildi",
  "message": "Siz 5,000 so'm cashback qo'shdingiz!",
  "data": {
    "amount": "5000",
    "type": "cashback_earned",
    "action_url": "/profile/cashback"
  }
}
```

### Example 3: Promotion to All Customers
```json
POST /api/push-notification/send-topic
{
  "topic": "all_customers",
  "title": "🎉 Chegirma!",
  "message": "50% chegirma barcha mahsulotlarga! Faqat bugun!",
  "data": {
    "promo_code": "SUMMER50",
    "type": "promotion",
    "action_url": "/promotions"
  },
  "image_url": "https://example.com/promo.jpg"
}
```

---

## Cleanup & Maintenance

Run cleanup periodically (e.g., daily cron job):

```bash
POST /api/push-notification/cleanup-expired
Authorization: Bearer {admin_token}
```

This removes device tokens older than 60 days.

---

## Security Best Practices

1. ✅ Never commit `firebase-service-account.json` to git
2. ✅ Use HTTPS in production
3. ✅ Validate all input data
4. ✅ Rate limit notification endpoints
5. ✅ Monitor Firebase quota usage
6. ✅ Implement notification preferences (user can opt-out)
7. ✅ Log all notification sends for audit

---

## Status

- ✅ Backend Implementation Complete
- ✅ Database Schema Created
- ✅ API Endpoints Ready
- ⏳ Flutter Implementation (Follow this guide)
- ⏳ Testing Required

---

**Next Steps:**
1. Close Visual Studio/running apps to release file locks
2. Run migration: `dotnet ef database update --project ../ZiyoMarket.Data` (from `src/ZiyoMarket.Api`)
3. Test endpoints in Swagger
4. Implement Flutter side using this guide
