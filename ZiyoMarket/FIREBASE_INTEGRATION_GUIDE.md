# Firebase Push Notification Integration Guide

## Backend (ASP.NET Core) - ✅ COMPLETED

### 1. Firebase Admin SDK Installation

```bash
# Already installed
cd src/ZiyoMarket.Api
dotnet add package FirebaseAdmin

cd src/ZiyoMarket.Service
dotnet add package FirebaseAdmin
```

### 2. Firebase Service Account Setup

**File:** `src/ZiyoMarket.Api/firebase-service-account.json`

```json
{
  "type": "service_account",
  "project_id": "market-f0779",
  "private_key_id": "...",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...",
  "client_email": "firebase-adminsdk-fbsvc@market-f0779.iam.gserviceaccount.com",
  ...
}
```

**⚠️ CRITICAL:** This file is in `.gitignore` - NEVER commit to git!

### 3. Backend Services Created

#### IFirebaseService Interface
Location: `src/ZiyoMarket.Service/Interfaces/IFirebaseService.cs`

Methods:
- `SendNotificationToUserAsync(fcmToken, title, body, data)` - Single user
- `SendNotificationToMultipleUsersAsync(fcmTokens, title, body, data)` - Batch
- `SendNotificationToTopicAsync(topic, title, body, data)` - Topic broadcast
- `SubscribeToTopicAsync(fcmTokens, topic)` - Subscribe users
- `UnsubscribeFromTopicAsync(fcmTokens, topic)` - Unsubscribe users

#### FirebaseService Implementation
Location: `src/ZiyoMarket.Service/Services/FirebaseService.cs`

Automatically handles:
- Android notifications (with sound, priority)
- iOS notifications (APNS)
- Error handling and logging

#### NotificationService Integration
Location: `src/ZiyoMarket.Service/Services/NotificationService.cs`

Automatically sends push notifications when:
- New order created → Customer receives notification
- Order status changed → Customer receives notification
- Cashback earned → Customer receives notification
- Support chat messages → Both customer and admin

### 4. Service Registration
Location: `src/ZiyoMarket.Api/Extensions/ServiceExtension.cs`

```csharp
services.AddSingleton<IFirebaseService, FirebaseService>(); // Firebase (Singleton)
services.AddScoped<INotificationService, NotificationService>();
```

---

## Frontend (Flutter) - TODO

### 1. Add Firebase Dependencies

**pubspec.yaml:**
```yaml
dependencies:
  firebase_core: ^3.8.1
  firebase_messaging: ^15.1.5
  flutter_local_notifications: ^18.0.1
```

### 2. Firebase Configuration Files

Download from Firebase Console:

**Android:** `android/app/google-services.json`
```json
{
  "project_info": {
    "project_number": "...",
    "project_id": "market-f0779",
    "storage_bucket": "market-f0779.firebasestorage.app"
  },
  ...
}
```

**iOS:** `ios/Runner/GoogleService-Info.plist`

### 3. Android Configuration

**android/app/build.gradle:**
```gradle
plugins {
    id "com.android.application"
    id "kotlin-android"
    id "dev.flutter.flutter-gradle-plugin"
    id 'com.google.gms.google-services'  // ✅ Add this
}

dependencies {
    implementation platform('com.google.firebase:firebase-bom:33.7.0')  // ✅ Add this
}
```

**android/build.gradle:**
```gradle
buildscript {
    dependencies {
        classpath 'com.google.gms:google-services:4.4.2'  // ✅ Add this
    }
}
```

**android/app/src/main/AndroidManifest.xml:**
```xml
<manifest>
    <application>
        <!-- ✅ Add this for notifications -->
        <meta-data
            android:name="com.google.firebase.messaging.default_notification_channel_id"
            android:value="ziyomarket_notifications" />

        <meta-data
            android:name="com.google.firebase.messaging.default_notification_icon"
            android:resource="@drawable/ic_notification" />
    </application>

    <!-- ✅ Add permissions -->
    <uses-permission android:name="android.permission.INTERNET"/>
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
</manifest>
```

### 4. iOS Configuration

**ios/Podfile:**
```ruby
platform :ios, '13.0'  # Minimum iOS 13

# Add at the end
post_install do |installer|
  installer.pods_project.targets.each do |target|
    flutter_additional_ios_build_settings(target)
    target.build_configurations.each do |config|
      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '13.0'
    end
  end
end
```

Enable Push Notifications in Xcode:
1. Open `ios/Runner.xcworkspace` in Xcode
2. Select Runner → Signing & Capabilities
3. Click "+ Capability" → Push Notifications
4. Click "+ Capability" → Background Modes → Remote notifications

### 5. Flutter Code Implementation

**lib/services/firebase_service.dart:**

```dart
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';

class FirebaseService {
  static final FirebaseMessaging _messaging = FirebaseMessaging.instance;
  static final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();

  /// Initialize Firebase and request permissions
  static Future<void> initialize() async {
    // Initialize Firebase
    await Firebase.initializeApp();

    // Request permissions (iOS)
    NotificationSettings settings = await _messaging.requestPermission(
      alert: true,
      badge: true,
      sound: true,
      provisional: false,
    );

    if (settings.authorizationStatus == AuthorizationStatus.authorized) {
      print('✅ User granted notification permission');
    }

    // Initialize local notifications
    const AndroidInitializationSettings androidSettings =
        AndroidInitializationSettings('@mipmap/ic_launcher');
    const DarwinInitializationSettings iosSettings =
        DarwinInitializationSettings();

    const InitializationSettings initSettings = InitializationSettings(
      android: androidSettings,
      iOS: iosSettings,
    );

    await _localNotifications.initialize(
      initSettings,
      onDidReceiveNotificationResponse: (details) {
        // Handle notification tap
        print('Notification tapped: ${details.payload}');
        // Navigate to appropriate screen based on payload
      },
    );

    // Create notification channel for Android
    const AndroidNotificationChannel channel = AndroidNotificationChannel(
      'ziyomarket_notifications',
      'ZiyoMarket Notifications',
      description: 'Notifications for orders, cashback, and updates',
      importance: Importance.high,
    );

    await _localNotifications
        .resolvePlatformSpecificImplementation<
            AndroidFlutterLocalNotificationsPlugin>()
        ?.createNotificationChannel(channel);

    // Listen to foreground messages
    FirebaseMessaging.onMessage.listen(_handleForegroundMessage);

    // Listen to background messages
    FirebaseMessaging.onBackgroundMessage(_handleBackgroundMessage);

    // Listen to notification taps
    FirebaseMessaging.onMessageOpenedApp.listen(_handleNotificationTap);
  }

  /// Get FCM token (send this to backend)
  static Future<String?> getToken() async {
    try {
      String? token = await _messaging.getToken();
      print('📱 FCM Token: $token');
      return token;
    } catch (e) {
      print('❌ Error getting FCM token: $e');
      return null;
    }
  }

  /// Subscribe to topic
  static Future<void> subscribeToTopic(String topic) async {
    await _messaging.subscribeToTopic(topic);
    print('✅ Subscribed to topic: $topic');
  }

  /// Unsubscribe from topic
  static Future<void> unsubscribeFromTopic(String topic) async {
    await _messaging.unsubscribeFromTopic(topic);
    print('✅ Unsubscribed from topic: $topic');
  }

  /// Handle foreground messages
  static Future<void> _handleForegroundMessage(RemoteMessage message) async {
    print('📬 Foreground message: ${message.messageId}');

    // Show local notification
    await _localNotifications.show(
      message.messageId.hashCode,
      message.notification?.title ?? 'ZiyoMarket',
      message.notification?.body ?? '',
      const NotificationDetails(
        android: AndroidNotificationDetails(
          'ziyomarket_notifications',
          'ZiyoMarket Notifications',
          importance: Importance.high,
          priority: Priority.high,
        ),
        iOS: DarwinNotificationDetails(),
      ),
      payload: message.data['action_url'],
    );
  }

  /// Handle background messages (must be top-level function)
  static Future<void> _handleBackgroundMessage(RemoteMessage message) async {
    print('📬 Background message: ${message.messageId}');
    // Process notification data
  }

  /// Handle notification tap (app opened from notification)
  static void _handleNotificationTap(RemoteMessage message) {
    print('👆 Notification tapped: ${message.messageId}');

    // Navigate based on notification type
    final data = message.data;
    final type = data['type'];

    switch (type) {
      case 'order_created':
      case 'order_status_changed':
        final orderId = data['order_id'];
        // Navigate to OrderDetailsScreen(orderId)
        break;
      case 'cashback_earned':
        // Navigate to CashbackScreen()
        break;
      case 'support_message':
        final chatId = data['chat_id'];
        // Navigate to SupportChatScreen(chatId)
        break;
      default:
        // Navigate to home or notifications list
        break;
    }
  }
}

// ⚠️ IMPORTANT: This must be a top-level function for background handler
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  await Firebase.initializeApp();
  print('📬 Background message: ${message.messageId}');
}
```

**lib/main.dart:**

```dart
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'services/firebase_service.dart';

// ⚠️ Register background handler BEFORE runApp()
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  await Firebase.initializeApp();
  print('📬 Background message handled: ${message.messageId}');
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize Firebase
  await Firebase.initializeApp();

  // Register background message handler
  FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);

  // Initialize Firebase service
  await FirebaseService.initialize();

  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ZiyoMarket',
      home: const HomePage(),
    );
  }
}
```

### 6. Update FCM Token on Login/Registration

**lib/services/auth_service.dart:**

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';
import 'firebase_service.dart';

class AuthService {
  static const String baseUrl = 'http://localhost:8080/api';

  /// Register customer
  static Future<void> register({
    required String firstName,
    required String lastName,
    required String phone,
    required String email,
    required String password,
    required String address,
  }) async {
    // Get FCM token
    String? fcmToken = await FirebaseService.getToken();

    final response = await http.post(
      Uri.parse('$baseUrl/auth/register'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'first_name': firstName,
        'last_name': lastName,
        'phone': phone,
        'email': email,
        'password': password,
        'address': address,
        'fcm_token': fcmToken, // ✅ Send FCM token to backend
      }),
    );

    if (response.statusCode == 200) {
      print('✅ Registration successful');

      // Subscribe to topics
      await FirebaseService.subscribeToTopic('all_customers');
    } else {
      throw Exception('Registration failed: ${response.body}');
    }
  }

  /// Login
  static Future<void> login({
    required String phoneOrEmail,
    required String password,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'phone_or_email': phoneOrEmail,
        'password': password,
        'user_type': 'Customer',
      }),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body)['data'];
      String accessToken = data['access_token'];

      // ✅ Update FCM token after login
      await updateFcmToken(accessToken);

      // Subscribe to topics
      await FirebaseService.subscribeToTopic('all_customers');

      print('✅ Login successful');
    } else {
      throw Exception('Login failed: ${response.body}');
    }
  }

  /// Update FCM token on backend
  static Future<void> updateFcmToken(String accessToken) async {
    String? fcmToken = await FirebaseService.getToken();

    if (fcmToken == null) {
      print('⚠️ FCM token is null, cannot update');
      return;
    }

    final response = await http.put(
      Uri.parse('$baseUrl/customer/update-fcm-token'),
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $accessToken',
      },
      body: jsonEncode({
        'fcm_token': fcmToken,
      }),
    );

    if (response.statusCode == 200) {
      print('✅ FCM token updated on backend');
    } else {
      print('❌ Failed to update FCM token: ${response.body}');
    }
  }
}
```

### 7. Backend Endpoint for FCM Token Update

**Backend Controller:** `src/ZiyoMarket.Api/Controllers/CustomerController.cs`

```csharp
[HttpPut("update-fcm-token")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenDto dto)
{
    var customerId = GetCurrentUserId();
    var result = await _customerService.UpdateFcmTokenAsync(customerId, dto.FcmToken);
    return Ok(result);
}
```

**Service Method:** Add to `ICustomerService` and `CustomerService`

```csharp
public async Task<Result> UpdateFcmTokenAsync(int customerId, string fcmToken)
{
    var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
    if (customer == null)
        return Result.NotFound("Customer not found");

    customer.FcmToken = fcmToken;
    await _unitOfWork.Customers.Update(customer, customerId);
    await _unitOfWork.SaveChangesAsync();

    return Result.Success("FCM token updated");
}
```

---

## Testing

### 1. Backend Test (Postman/Swagger)

**Send notification endpoint:**
```
POST http://localhost:8080/api/notification/send
Authorization: Bearer {admin_token}

{
  "user_id": 1,
  "user_type": "Customer",
  "type": "OrderCreated",
  "title": "Test Notification",
  "message": "This is a test push notification",
  "priority": "High",
  "send_push_notification": true,
  "fcm_token": "{customer_fcm_token}"
}
```

### 2. Flutter Test

1. Run the app on physical device (emulators may have issues)
2. Login with customer account
3. Check console for FCM token
4. Send test notification from backend
5. Verify notification appears

---

## Troubleshooting

### Android Issues

1. **App crashes on startup:**
   - Check `google-services.json` is in `android/app/`
   - Run `flutter clean && flutter pub get`

2. **Notifications not received:**
   - Check permissions granted
   - Verify notification channel created
   - Check Firebase Console → Cloud Messaging

### iOS Issues

1. **Build fails:**
   - Run `cd ios && pod install`
   - Check minimum iOS version is 13.0

2. **Notifications not received:**
   - Check Push Notifications capability enabled
   - Verify APNs certificate in Firebase Console
   - Test on physical device (not simulator)

### Backend Issues

1. **Firebase Admin error:**
   - Check `firebase-service-account.json` exists
   - Verify file is not corrupted
   - Check Firebase project ID matches

2. **Token invalid:**
   - FCM token expires after 7 days of inactivity
   - Refresh token on app launch
   - Handle token refresh in Flutter

---

## Security Notes

1. **NEVER commit `firebase-service-account.json` to git** ✅ Already in `.gitignore`
2. Rotate Firebase service account keys periodically
3. Use environment variables for sensitive data in production
4. Validate FCM tokens before storing in database
5. Implement rate limiting for notification endpoints

---

## Production Deployment

### Backend
1. Copy `firebase-service-account.json` to server
2. Update path in `FirebaseService.cs` if needed
3. Set correct file permissions (read-only)
4. Monitor Firebase quota (free tier: 20K messages/day)

### Flutter
1. Generate release APK/IPA with correct Firebase config
2. Test notifications on production build
3. Monitor crash reports in Firebase Crashlytics
4. Set up FCM token refresh mechanism

---

## Firebase Console URLs

- **Project Console:** https://console.firebase.google.com/project/market-f0779
- **Cloud Messaging:** https://console.firebase.google.com/project/market-f0779/messaging
- **Analytics:** https://console.firebase.google.com/project/market-f0779/analytics

---

**Status:** ✅ Backend implementation complete
**Next Steps:** Flutter integration (follow this guide)
