# Flutter Developer Guide - ZiyoMarket Mobile App

> **To'liq qo'llanma Flutter dasturchilari uchun: API Integration, State Management, Best Practices**

## 📚 Mundarija

1. [Loyiha Haqida](#loyiha-haqida)
2. [API Base Configuration](#api-base-configuration)
3. [Authentication va Token Management](#authentication-va-token-management)
4. [API Endpoints - Mobil App Uchun](#api-endpoints-mobil-app-uchun)
5. [HTTP Client Setup](#http-client-setup)
6. [Models va Serialization](#models-va-serialization)
7. [State Management](#state-management)
8. [Error Handling](#error-handling)
9. [Push Notifications (FCM)](#push-notifications-fcm)
10. [Image Handling](#image-handling)
11. [Best Practices](#best-practices)
12. [Example Implementations](#example-implementations)

---

## 📱 Loyiha Haqida

ZiyoMarket - bu e-commerce mobile application mijozlar uchun. Quyidagi asosiy funksiyalarni qo'llab-quvvatlaydi:

### Asosiy Features

✅ **Authentication**
- Login / Register
- Password recovery
- OTP verification

✅ **Product Catalog**
- Browse products with categories
- Search and filters
- Product details
- QR code scanner
- Like/Unlike products

✅ **Shopping Cart**
- Add/remove items
- Update quantities
- View total price

✅ **Orders**
- Create orders
- Order history
- Track order status
- Cancel orders

✅ **Cashback System**
- View cashback balance
- Cashback history
- Use cashback for payment

✅ **Support Chat**
- Create support tickets
- Real-time messaging
- View chat history

✅ **Notifications**
- Order updates
- Delivery notifications
- Promotional messages
- Push notifications

---

## 🔧 API Base Configuration

### Base URL

```dart
class ApiConstants {
  // Development
  static const String baseUrl = 'https://localhost:5001/api';

  // Production (Deploy qilinganda)
  static const String baseUrl = 'https://api.ziyomarket.uz/api';

  // Timeout settings
  static const Duration connectionTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);

  // API Version
  static const String apiVersion = 'v1';
}
```

### Endpoint Constants

```dart
class Endpoints {
  // Auth
  static const String login = '/auth/login';
  static const String register = '/auth/register';
  static const String refreshToken = '/auth/refresh-token';
  static const String logout = '/auth/logout';
  static const String changePassword = '/auth/change-password';
  static const String forgotPassword = '/auth/forgot-password';
  static const String resetPassword = '/auth/reset-password';
  static const String me = '/auth/me';

  // Products
  static const String products = '/product';
  static String productById(int id) => '/product/$id';
  static String productByQr(String qr) => '/product/qr/$qr';
  static const String searchProducts = '/product/search';
  static const String featuredProducts = '/product/featured';
  static const String newArrivals = '/product/new-arrivals';
  static String productsByCategory(int categoryId) => '/product/category/$categoryId';
  static String likeProduct(int id) => '/product/$id/like';
  static const String likedProducts = '/product/liked';

  // Categories
  static const String categories = '/category';
  static const String rootCategories = '/category/root';
  static String categoryById(int id) => '/category/$id';
  static String subcategories(int parentId) => '/category/$parentId/subcategories';
  static const String categoryTree = '/category/tree';

  // Cart
  static const String cart = '/cart';
  static String cartItem(int id) => '/cart/$id';
  static const String clearCart = '/cart/clear';
  static const String cartTotal = '/cart/total';
  static const String cartCount = '/cart/count';
  static const String validateCart = '/cart/validate';

  // Orders
  static const String orders = '/order';
  static String orderById(int id) => '/order/$id';
  static String cancelOrder(int id) => '/order/$id/cancel';

  // Cashback
  static const String cashbackSummary = '/cashback/summary';
  static const String cashbackHistory = '/cashback/history';
  static const String availableCashback = '/cashback/available';
  static const String expiringCashback = '/cashback/expiring';

  // Support
  static const String supportChats = '/support';
  static String supportChatById(int id) => '/support/$id';
  static String supportMessages(int chatId) => '/support/$chatId/messages';
  static const String sendMessage = '/support/messages';
  static const String myChats = '/support/my-chats';
  static const String myLatestChat = '/support/my-latest-chat';

  // Notifications
  static const String notifications = '/notification';
  static String markAsRead(int id) => '/notification/$id/read';
  static const String markAllAsRead = '/notification/read-all';
  static const String unreadCount = '/notification/unread-count';

  // Delivery
  static String trackDelivery(String trackingCode) => '/delivery/track/$trackingCode';
  static String orderDelivery(int orderId) => '/delivery/orders/$orderId';

  // Content
  static const String publishedContent = '/content/published';
  static String contentByType(String type) => '/content/type/$type';  // Blog, News, FAQ
  static String contentById(int id) => '/content/$id';
}
```

---

## 🔐 Authentication va Token Management

### 1. Authentication Service

```dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class AuthService {
  final String baseUrl = ApiConstants.baseUrl;

  // Login
  Future<AuthResponse> login({
    required String phoneOrEmail,
    required String password,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'phoneOrEmail': phoneOrEmail,
          'password': password,
          'userType': 'Customer',
        }),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          // Save token
          await _saveTokens(
            data['data']['accessToken'],
            data['data']['refreshToken'],
          );
          return AuthResponse.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Login failed');
      }
    } catch (e) {
      throw Exception('Login error: $e');
    }
  }

  // Register
  Future<AuthResponse> register({
    required String firstName,
    required String lastName,
    required String phone,
    required String email,
    required String password,
    String? address,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/register'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'firstName': firstName,
          'lastName': lastName,
          'phone': phone,
          'email': email,
          'password': password,
          'address': address,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          await _saveTokens(
            data['data']['accessToken'],
            data['data']['refreshToken'],
          );
          return AuthResponse.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Registration failed');
      }
    } catch (e) {
      throw Exception('Registration error: $e');
    }
  }

  // Get current user profile
  Future<UserProfile> getProfile() async {
    try {
      final token = await getAccessToken();
      final response = await http.get(
        Uri.parse('$baseUrl/auth/me'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return UserProfile.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to get profile');
      }
    } catch (e) {
      throw Exception('Profile error: $e');
    }
  }

  // Logout
  Future<void> logout() async {
    try {
      final token = await getAccessToken();
      await http.post(
        Uri.parse('$baseUrl/auth/logout'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );
    } finally {
      await _clearTokens();
    }
  }

  // Token Management
  Future<void> _saveTokens(String accessToken, String refreshToken) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('access_token', accessToken);
    await prefs.setString('refresh_token', refreshToken);
  }

  Future<String?> getAccessToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('access_token');
  }

  Future<String?> getRefreshToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('refresh_token');
  }

  Future<void> _clearTokens() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('access_token');
    await prefs.remove('refresh_token');
  }

  Future<bool> isLoggedIn() async {
    final token = await getAccessToken();
    return token != null && token.isNotEmpty;
  }

  // Refresh token
  Future<void> refreshAccessToken() async {
    final refreshToken = await getRefreshToken();
    if (refreshToken == null) throw Exception('No refresh token');

    final response = await http.post(
      Uri.parse('$baseUrl/auth/refresh-token'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'refreshToken': refreshToken}),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      if (data['success']) {
        await _saveTokens(
          data['data']['accessToken'],
          data['data']['refreshToken'],
        );
      }
    } else {
      await _clearTokens();
      throw Exception('Token refresh failed');
    }
  }
}
```

### 2. Authentication Models

```dart
class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final DateTime expiresAt;
  final UserProfile user;

  AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresAt,
    required this.user,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'],
      refreshToken: json['refreshToken'],
      expiresAt: DateTime.parse(json['expiresAt']),
      user: UserProfile.fromJson(json['user']),
    );
  }
}

class UserProfile {
  final int id;
  final String firstName;
  final String lastName;
  final String phone;
  final String email;
  final String userType;
  final double cashbackBalance;
  final bool isActive;

  UserProfile({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.phone,
    required this.email,
    required this.userType,
    required this.cashbackBalance,
    required this.isActive,
  });

  String get fullName => '$firstName $lastName';

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'],
      firstName: json['firstName'],
      lastName: json['lastName'],
      phone: json['phone'],
      email: json['email'],
      userType: json['userType'],
      cashbackBalance: (json['cashbackBalance'] ?? 0).toDouble(),
      isActive: json['isActive'] ?? true,
    );
  }
}
```

---

## 🛍️ API Endpoints - Mobil App Uchun

### 1. Product Service

```dart
class ProductService {
  final String baseUrl = ApiConstants.baseUrl;
  final AuthService _authService = AuthService();

  // Get products with filters
  Future<PaginatedResponse<Product>> getProducts({
    int pageNumber = 1,
    int pageSize = 20,
    String? searchTerm,
    int? categoryId,
    double? minPrice,
    double? maxPrice,
    String sortBy = 'Name',
    String sortOrder = 'Ascending',
  }) async {
    try {
      final token = await _authService.getAccessToken();

      // Build query parameters
      final queryParams = {
        'pageNumber': pageNumber.toString(),
        'pageSize': pageSize.toString(),
        if (searchTerm != null && searchTerm.isNotEmpty) 'searchTerm': searchTerm,
        if (categoryId != null) 'categoryId': categoryId.toString(),
        if (minPrice != null) 'minPrice': minPrice.toString(),
        if (maxPrice != null) 'maxPrice': maxPrice.toString(),
        'sortBy': sortBy,
        'sortOrder': sortOrder,
      };

      final uri = Uri.parse('$baseUrl/product').replace(queryParameters: queryParams);

      final response = await http.get(
        uri,
        headers: {
          'Content-Type': 'application/json',
          if (token != null) 'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return PaginatedResponse<Product>.fromJson(
            data['data'],
            (json) => Product.fromJson(json),
          );
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to load products');
      }
    } catch (e) {
      throw Exception('Error loading products: $e');
    }
  }

  // Get product by ID
  Future<Product> getProductById(int id) async {
    try {
      final token = await _authService.getAccessToken();
      final response = await http.get(
        Uri.parse('$baseUrl/product/$id'),
        headers: {
          'Content-Type': 'application/json',
          if (token != null) 'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return Product.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to load product');
      }
    } catch (e) {
      throw Exception('Error loading product: $e');
    }
  }

  // Search products by QR code
  Future<Product> getProductByQr(String qrCode) async {
    try {
      final token = await _authService.getAccessToken();
      final response = await http.get(
        Uri.parse('$baseUrl/product/qr/$qrCode'),
        headers: {
          'Content-Type': 'application/json',
          if (token != null) 'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return Product.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Product not found');
      }
    } catch (e) {
      throw Exception('Error scanning QR: $e');
    }
  }

  // Like/Unlike product
  Future<LikeResponse> toggleLike(int productId) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.post(
        Uri.parse('$baseUrl/product/$productId/like'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return LikeResponse.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to toggle like');
      }
    } catch (e) {
      throw Exception('Error toggling like: $e');
    }
  }

  // Get liked products
  Future<List<Product>> getLikedProducts() async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.get(
        Uri.parse('$baseUrl/product/liked'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return (data['data'] as List)
              .map((json) => Product.fromJson(json))
              .toList();
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to load liked products');
      }
    } catch (e) {
      throw Exception('Error loading liked products: $e');
    }
  }

  // Get featured products
  Future<List<Product>> getFeaturedProducts() async {
    try {
      final response = await http.get(
        Uri.parse('$baseUrl/product/featured'),
        headers: {'Content-Type': 'application/json'},
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return (data['data'] as List)
              .map((json) => Product.fromJson(json))
              .toList();
        }
      }
      return [];
    } catch (e) {
      return [];
    }
  }
}
```

### 2. Cart Service

```dart
class CartService {
  final String baseUrl = ApiConstants.baseUrl;
  final AuthService _authService = AuthService();

  // Get cart items
  Future<CartResponse> getCart() async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.get(
        Uri.parse('$baseUrl/cart'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return CartResponse.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to load cart');
      }
    } catch (e) {
      throw Exception('Error loading cart: $e');
    }
  }

  // Add to cart
  Future<void> addToCart({
    required int productId,
    required int quantity,
  }) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.post(
        Uri.parse('$baseUrl/cart'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({
          'productId': productId,
          'quantity': quantity,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = jsonDecode(response.body);
        if (!data['success']) {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to add to cart');
      }
    } catch (e) {
      throw Exception('Error adding to cart: $e');
    }
  }

  // Update cart item quantity
  Future<void> updateCartItem({
    required int cartItemId,
    required int quantity,
  }) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.put(
        Uri.parse('$baseUrl/cart/$cartItemId'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({'quantity': quantity}),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (!data['success']) {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to update cart item');
      }
    } catch (e) {
      throw Exception('Error updating cart: $e');
    }
  }

  // Remove from cart
  Future<void> removeFromCart(int cartItemId) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.delete(
        Uri.parse('$baseUrl/cart/$cartItemId'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (!data['success']) {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to remove from cart');
      }
    } catch (e) {
      throw Exception('Error removing from cart: $e');
    }
  }

  // Clear cart
  Future<void> clearCart() async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.delete(
        Uri.parse('$baseUrl/cart/clear'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (!data['success']) {
          throw Exception(data['message']);
        }
      }
    } catch (e) {
      throw Exception('Error clearing cart: $e');
    }
  }

  // Get cart total
  Future<CartTotal> getCartTotal() async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.get(
        Uri.parse('$baseUrl/cart/total'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return CartTotal.fromJson(data['data']);
        }
      }
      return CartTotal(totalItems: 0, totalQuantity: 0, totalPrice: 0);
    } catch (e) {
      return CartTotal(totalItems: 0, totalQuantity: 0, totalPrice: 0);
    }
  }
}
```

### 3. Order Service

```dart
class OrderService {
  final String baseUrl = ApiConstants.baseUrl;
  final AuthService _authService = AuthService();

  // Create order
  Future<Order> createOrder({
    required List<OrderItemRequest> orderItems,
    required String paymentMethod,
    required String deliveryType,
    String? deliveryAddress,
    double cashbackToUse = 0,
    String? customerNotes,
  }) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.post(
        Uri.parse('$baseUrl/order'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode({
          'orderItems': orderItems.map((e) => e.toJson()).toList(),
          'paymentMethod': paymentMethod,
          'deliveryType': deliveryType,
          if (deliveryAddress != null) 'deliveryAddress': deliveryAddress,
          if (cashbackToUse > 0) 'cashbackToUse': cashbackToUse,
          if (customerNotes != null) 'customerNotes': customerNotes,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return Order.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to create order');
      }
    } catch (e) {
      throw Exception('Error creating order: $e');
    }
  }

  // Get orders
  Future<PaginatedResponse<Order>> getOrders({
    int pageNumber = 1,
    int pageSize = 20,
    String? status,
  }) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final queryParams = {
        'pageNumber': pageNumber.toString(),
        'pageSize': pageSize.toString(),
        if (status != null) 'status': status,
      };

      final uri = Uri.parse('$baseUrl/order').replace(queryParameters: queryParams);

      final response = await http.get(
        uri,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return PaginatedResponse<Order>.fromJson(
            data['data'],
            (json) => Order.fromJson(json),
          );
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to load orders');
      }
    } catch (e) {
      throw Exception('Error loading orders: $e');
    }
  }

  // Get order by ID
  Future<Order> getOrderById(int id) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.get(
        Uri.parse('$baseUrl/order/$id'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success']) {
          return Order.fromJson(data['data']);
        } else {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to load order');
      }
    } catch (e) {
      throw Exception('Error loading order: $e');
    }
  }

  // Cancel order
  Future<void> cancelOrder(int orderId) async {
    try {
      final token = await _authService.getAccessToken();
      if (token == null) throw Exception('Not authenticated');

      final response = await http.post(
        Uri.parse('$baseUrl/order/$orderId/cancel'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (!data['success']) {
          throw Exception(data['message']);
        }
      } else {
        throw Exception('Failed to cancel order');
      }
    } catch (e) {
      throw Exception('Error cancelling order: $e');
    }
  }
}
```

---

## 📦 Models va Serialization

### Product Model

```dart
class Product {
  final int id;
  final String name;
  final String description;
  final String qrCode;
  final double price;
  final int stockQuantity;
  final int categoryId;
  final String categoryName;
  final String status;
  final String? imageUrl;
  final bool isLowStock;
  final bool isLiked;
  final int likesCount;
  final double? weight;
  final String? dimensions;
  final String? manufacturer;
  final DateTime createdAt;
  final DateTime? updatedAt;

  Product({
    required this.id,
    required this.name,
    required this.description,
    required this.qrCode,
    required this.price,
    required this.stockQuantity,
    required this.categoryId,
    required this.categoryName,
    required this.status,
    this.imageUrl,
    required this.isLowStock,
    required this.isLiked,
    required this.likesCount,
    this.weight,
    this.dimensions,
    this.manufacturer,
    required this.createdAt,
    this.updatedAt,
  });

  factory Product.fromJson(Map<String, dynamic> json) {
    return Product(
      id: json['id'],
      name: json['name'],
      description: json['description'] ?? '',
      qrCode: json['qrCode'],
      price: (json['price'] ?? 0).toDouble(),
      stockQuantity: json['stockQuantity'] ?? 0,
      categoryId: json['categoryId'],
      categoryName: json['categoryName'] ?? '',
      status: json['status'] ?? 'Active',
      imageUrl: json['imageUrl'],
      isLowStock: json['isLowStock'] ?? false,
      isLiked: json['isLiked'] ?? false,
      likesCount: json['likesCount'] ?? 0,
      weight: json['weight']?.toDouble(),
      dimensions: json['dimensions'],
      manufacturer: json['manufacturer'],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: json['updatedAt'] != null ? DateTime.parse(json['updatedAt']) : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'description': description,
      'qrCode': qrCode,
      'price': price,
      'stockQuantity': stockQuantity,
      'categoryId': categoryId,
      'categoryName': categoryName,
      'status': status,
      'imageUrl': imageUrl,
      'isLowStock': isLowStock,
      'isLiked': isLiked,
      'likesCount': likesCount,
      'weight': weight,
      'dimensions': dimensions,
      'manufacturer': manufacturer,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }
}
```

### Order Model

```dart
class Order {
  final int id;
  final String orderNumber;
  final DateTime orderDate;
  final String status;
  final double totalPrice;
  final double discountApplied;
  final double cashbackUsed;
  final double finalPrice;
  final String paymentMethod;
  final String? deliveryType;
  final String? deliveryAddress;
  final String? customerNotes;
  final List<OrderItem> items;
  final OrderDelivery? delivery;

  Order({
    required this.id,
    required this.orderNumber,
    required this.orderDate,
    required this.status,
    required this.totalPrice,
    required this.discountApplied,
    required this.cashbackUsed,
    required this.finalPrice,
    required this.paymentMethod,
    this.deliveryType,
    this.deliveryAddress,
    this.customerNotes,
    required this.items,
    this.delivery,
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      id: json['id'],
      orderNumber: json['orderNumber'],
      orderDate: DateTime.parse(json['orderDate']),
      status: json['status'],
      totalPrice: (json['totalPrice'] ?? 0).toDouble(),
      discountApplied: (json['discountApplied'] ?? 0).toDouble(),
      cashbackUsed: (json['cashbackUsed'] ?? 0).toDouble(),
      finalPrice: (json['finalPrice'] ?? 0).toDouble(),
      paymentMethod: json['paymentMethod'],
      deliveryType: json['deliveryType'],
      deliveryAddress: json['deliveryAddress'],
      customerNotes: json['customerNotes'],
      items: (json['items'] as List?)
              ?.map((item) => OrderItem.fromJson(item))
              .toList() ??
          [],
      delivery: json['delivery'] != null
          ? OrderDelivery.fromJson(json['delivery'])
          : null,
    );
  }

  // Status helpers
  bool get isPending => status == 'Pending';
  bool get isConfirmed => status == 'Confirmed';
  bool get isDelivered => status == 'Delivered';
  bool get isCancelled => status == 'Cancelled';
  bool get canBeCancelled => status == 'Pending' || status == 'Confirmed';
}

class OrderItem {
  final int productId;
  final String productName;
  final String? productImage;
  final int quantity;
  final double unitPrice;
  final double subTotal;
  final double discountApplied;
  final double totalPrice;

  OrderItem({
    required this.productId,
    required this.productName,
    this.productImage,
    required this.quantity,
    required this.unitPrice,
    required this.subTotal,
    required this.discountApplied,
    required this.totalPrice,
  });

  factory OrderItem.fromJson(Map<String, dynamic> json) {
    return OrderItem(
      productId: json['productId'],
      productName: json['productName'],
      productImage: json['productImage'],
      quantity: json['quantity'],
      unitPrice: (json['unitPrice'] ?? 0).toDouble(),
      subTotal: (json['subTotal'] ?? 0).toDouble(),
      discountApplied: (json['discountApplied'] ?? 0).toDouble(),
      totalPrice: (json['totalPrice'] ?? 0).toDouble(),
    );
  }
}

class OrderItemRequest {
  final int productId;
  final int quantity;
  final double unitPrice;

  OrderItemRequest({
    required this.productId,
    required this.quantity,
    required this.unitPrice,
  });

  Map<String, dynamic> toJson() {
    return {
      'productId': productId,
      'quantity': quantity,
      'unitPrice': unitPrice,
    };
  }
}
```

### Cart Models

```dart
class CartResponse {
  final List<CartItem> items;
  final int totalItems;
  final int totalQuantity;
  final double totalPrice;

  CartResponse({
    required this.items,
    required this.totalItems,
    required this.totalQuantity,
    required this.totalPrice,
  });

  factory CartResponse.fromJson(Map<String, dynamic> json) {
    return CartResponse(
      items: (json['items'] as List)
          .map((item) => CartItem.fromJson(item))
          .toList(),
      totalItems: json['totalItems'] ?? 0,
      totalQuantity: json['totalQuantity'] ?? 0,
      totalPrice: (json['totalPrice'] ?? 0).toDouble(),
    );
  }
}

class CartItem {
  final int id;
  final int productId;
  final String productName;
  final String? productImage;
  final double unitPrice;
  final int quantity;
  final double subTotal;
  final bool isAvailable;
  final int stockQuantity;
  final DateTime addedAt;

  CartItem({
    required this.id,
    required this.productId,
    required this.productName,
    this.productImage,
    required this.unitPrice,
    required this.quantity,
    required this.subTotal,
    required this.isAvailable,
    required this.stockQuantity,
    required this.addedAt,
  });

  factory CartItem.fromJson(Map<String, dynamic> json) {
    return CartItem(
      id: json['id'],
      productId: json['productId'],
      productName: json['productName'],
      productImage: json['productImage'],
      unitPrice: (json['unitPrice'] ?? 0).toDouble(),
      quantity: json['quantity'],
      subTotal: (json['subTotal'] ?? 0).toDouble(),
      isAvailable: json['isAvailable'] ?? true,
      stockQuantity: json['stockQuantity'] ?? 0,
      addedAt: DateTime.parse(json['addedAt']),
    );
  }
}

class CartTotal {
  final int totalItems;
  final int totalQuantity;
  final double totalPrice;

  CartTotal({
    required this.totalItems,
    required this.totalQuantity,
    required this.totalPrice,
  });

  factory CartTotal.fromJson(Map<String, dynamic> json) {
    return CartTotal(
      totalItems: json['totalItems'] ?? 0,
      totalQuantity: json['totalQuantity'] ?? 0,
      totalPrice: (json['totalPrice'] ?? 0).toDouble(),
    );
  }
}
```

### Pagination Model

```dart
class PaginatedResponse<T> {
  final List<T> items;
  final int pageNumber;
  final int pageSize;
  final int totalCount;
  final int totalPages;
  final bool hasPreviousPage;
  final bool hasNextPage;

  PaginatedResponse({
    required this.items,
    required this.pageNumber,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasPreviousPage,
    required this.hasNextPage,
  });

  factory PaginatedResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromJsonT,
  ) {
    return PaginatedResponse<T>(
      items: (json['items'] as List)
          .map((item) => fromJsonT(item as Map<String, dynamic>))
          .toList(),
      pageNumber: json['pageNumber'],
      pageSize: json['pageSize'],
      totalCount: json['totalCount'],
      totalPages: json['totalPages'],
      hasPreviousPage: json['hasPreviousPage'],
      hasNextPage: json['hasNextPage'],
    );
  }
}
```

---

## 🎨 State Management (Provider Pattern)

### 1. Auth Provider

```dart
import 'package:flutter/foundation.dart';

class AuthProvider with ChangeNotifier {
  final AuthService _authService = AuthService();

  UserProfile? _currentUser;
  bool _isLoading = false;
  String? _error;

  UserProfile? get currentUser => _currentUser;
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isAuthenticated => _currentUser != null;

  Future<bool> login(String phoneOrEmail, String password) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _authService.login(
        phoneOrEmail: phoneOrEmail,
        password: password,
      );
      _currentUser = response.user;
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<bool> register({
    required String firstName,
    required String lastName,
    required String phone,
    required String email,
    required String password,
    String? address,
  }) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final response = await _authService.register(
        firstName: firstName,
        lastName: lastName,
        phone: phone,
        email: email,
        password: password,
        address: address,
      );
      _currentUser = response.user;
      _isLoading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _error = e.toString();
      _isLoading = false;
      notifyListeners();
      return false;
    }
  }

  Future<void> loadProfile() async {
    try {
      _currentUser = await _authService.getProfile();
      notifyListeners();
    } catch (e) {
      _error = e.toString();
      notifyListeners();
    }
  }

  Future<void> logout() async {
    await _authService.logout();
    _currentUser = null;
    notifyListeners();
  }
}
```

### 2. Cart Provider

```dart
class CartProvider with ChangeNotifier {
  final CartService _cartService = CartService();

  CartResponse? _cart;
  bool _isLoading = false;
  String? _error;

  CartResponse? get cart => _cart;
  bool get isLoading => _isLoading;
  String? get error => _error;
  int get itemCount => _cart?.totalItems ?? 0;
  double get totalPrice => _cart?.totalPrice ?? 0;

  Future<void> loadCart() async {
    _isLoading = true;
    notifyListeners();

    try {
      _cart = await _cartService.getCart();
      _error = null;
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> addToCart(int productId, int quantity) async {
    try {
      await _cartService.addToCart(
        productId: productId,
        quantity: quantity,
      );
      await loadCart(); // Reload cart
      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }

  Future<bool> updateQuantity(int cartItemId, int quantity) async {
    try {
      await _cartService.updateCartItem(
        cartItemId: cartItemId,
        quantity: quantity,
      );
      await loadCart();
      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }

  Future<bool> removeItem(int cartItemId) async {
    try {
      await _cartService.removeFromCart(cartItemId);
      await loadCart();
      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }

  Future<bool> clearCart() async {
    try {
      await _cartService.clearCart();
      await loadCart();
      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }
}
```

### 3. Product Provider

```dart
class ProductProvider with ChangeNotifier {
  final ProductService _productService = ProductService();

  List<Product> _products = [];
  Product? _selectedProduct;
  bool _isLoading = false;
  String? _error;
  int _currentPage = 1;
  bool _hasMore = true;

  List<Product> get products => _products;
  Product? get selectedProduct => _selectedProduct;
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get hasMore => _hasMore;

  Future<void> loadProducts({
    bool refresh = false,
    String? searchTerm,
    int? categoryId,
  }) async {
    if (refresh) {
      _currentPage = 1;
      _products = [];
      _hasMore = true;
    }

    if (!_hasMore || _isLoading) return;

    _isLoading = true;
    notifyListeners();

    try {
      final response = await _productService.getProducts(
        pageNumber: _currentPage,
        pageSize: 20,
        searchTerm: searchTerm,
        categoryId: categoryId,
      );

      _products.addAll(response.items);
      _hasMore = response.hasNextPage;
      _currentPage++;
      _error = null;
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> loadProductById(int id) async {
    _isLoading = true;
    notifyListeners();

    try {
      _selectedProduct = await _productService.getProductById(id);
      _error = null;
    } catch (e) {
      _error = e.toString();
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> toggleLike(int productId) async {
    try {
      final response = await _productService.toggleLike(productId);

      // Update local data
      final index = _products.indexWhere((p) => p.id == productId);
      if (index != -1) {
        // Create updated product (immutable pattern)
        // You'll need to implement copyWith in Product model
        notifyListeners();
      }

      return true;
    } catch (e) {
      _error = e.toString();
      notifyListeners();
      return false;
    }
  }
}
```

---

## 🔔 Push Notifications (FCM)

### Firebase Cloud Messaging Setup

**pubspec.yaml:**
```yaml
dependencies:
  firebase_core: ^2.24.0
  firebase_messaging: ^14.7.6
  flutter_local_notifications: ^16.2.0
```

### FCM Service

```dart
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';

class FCMService {
  final FirebaseMessaging _firebaseMessaging = FirebaseMessaging.instance;
  final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();

  Future<void> initialize() async {
    // Request permission
    await _firebaseMessaging.requestPermission(
      alert: true,
      badge: true,
      sound: true,
    );

    // Get FCM token
    String? token = await _firebaseMessaging.getToken();
    print('FCM Token: $token');

    // Send token to backend
    if (token != null) {
      await _saveFCMToken(token);
    }

    // Initialize local notifications
    const android = AndroidInitializationSettings('@mipmap/ic_launcher');
    const ios = DarwinInitializationSettings();
    const settings = InitializationSettings(android: android, iOS: ios);
    await _localNotifications.initialize(settings);

    // Handle foreground messages
    FirebaseMessaging.onMessage.listen(_handleForegroundMessage);

    // Handle background messages
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);

    // Handle notification taps
    FirebaseMessaging.onMessageOpenedApp.listen(_handleNotificationTap);
  }

  Future<void> _saveFCMToken(String token) async {
    // TODO: Send token to backend
    // PATCH /api/customer/fcm-token
    // Body: { "fcmToken": token }
  }

  void _handleForegroundMessage(RemoteMessage message) {
    print('Foreground message: ${message.notification?.title}');

    // Show local notification
    _showLocalNotification(
      message.notification?.title ?? '',
      message.notification?.body ?? '',
      message.data,
    );
  }

  Future<void> _showLocalNotification(
    String title,
    String body,
    Map<String, dynamic> data,
  ) async {
    const androidDetails = AndroidNotificationDetails(
      'ziyomarket_channel',
      'ZiyoMarket Notifications',
      importance: Importance.high,
      priority: Priority.high,
    );

    const iosDetails = DarwinNotificationDetails();

    const details = NotificationDetails(
      android: androidDetails,
      iOS: iosDetails,
    );

    await _localNotifications.show(
      DateTime.now().millisecond,
      title,
      body,
      details,
      payload: jsonEncode(data),
    );
  }

  void _handleNotificationTap(RemoteMessage message) {
    print('Notification tapped: ${message.data}');

    // Navigate based on notification type
    final data = message.data;
    final type = data['type'];

    switch (type) {
      case 'OrderStatus':
        // Navigate to order details
        final orderId = int.parse(data['orderId']);
        // Navigator.push...
        break;
      case 'Delivery':
        // Navigate to delivery tracking
        break;
      case 'Promotion':
        // Navigate to promotions
        break;
      default:
        // Navigate to home
    }
  }
}

// Background message handler (must be top-level function)
@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  print('Background message: ${message.notification?.title}');
}
```

---

## 📄 Best Practices

### 1. Error Handling

```dart
class ApiResponse<T> {
  final bool success;
  final T? data;
  final String? message;
  final List<String>? errors;

  ApiResponse({
    required this.success,
    this.data,
    this.message,
    this.errors,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>)? fromJsonT,
  ) {
    return ApiResponse<T>(
      success: json['success'] ?? false,
      data: json['data'] != null && fromJsonT != null
          ? fromJsonT(json['data'])
          : json['data'],
      message: json['message'],
      errors: json['errors'] != null
          ? List<String>.from(json['errors'])
          : null,
    );
  }
}
```

### 2. Loading States

```dart
enum LoadingState {
  initial,
  loading,
  loaded,
  error,
}

class DataState<T> {
  final LoadingState state;
  final T? data;
  final String? error;

  DataState({
    required this.state,
    this.data,
    this.error,
  });

  bool get isLoading => state == LoadingState.loading;
  bool get isLoaded => state == LoadingState.loaded;
  bool get hasError => state == LoadingState.error;
  bool get hasData => data != null;
}
```

### 3. Caching

```dart
import 'package:shared_preferences/shared_preferences.dart';

class CacheService {
  static const String _prefix = 'cache_';

  Future<void> saveString(String key, String value) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('$_prefix$key', value);
  }

  Future<String?> getString(String key) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('$_prefix$key');
  }

  Future<void> saveJson(String key, Map<String, dynamic> json) async {
    await saveString(key, jsonEncode(json));
  }

  Future<Map<String, dynamic>?> getJson(String key) async {
    final string = await getString(key);
    return string != null ? jsonDecode(string) : null;
  }

  Future<void> remove(String key) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('$_prefix$key');
  }

  Future<void> clear() async {
    final prefs = await SharedPreferences.getInstance();
    final keys = prefs.getKeys().where((key) => key.startsWith(_prefix));
    for (final key in keys) {
      await prefs.remove(key);
    }
  }
}
```

---

## 🎯 Recommended Packages

```yaml
dependencies:
  flutter:
    sdk: flutter

  # State Management
  provider: ^6.1.1

  # HTTP Requests
  http: ^1.1.2
  dio: ^5.4.0  # Alternative to http

  # Local Storage
  shared_preferences: ^2.2.2
  hive: ^2.2.3
  hive_flutter: ^1.1.0

  # Firebase
  firebase_core: ^2.24.0
  firebase_messaging: ^14.7.6
  flutter_local_notifications: ^16.2.0

  # UI
  cached_network_image: ^3.3.1
  shimmer: ^3.0.0
  flutter_svg: ^2.0.9

  # QR Scanner
  mobile_scanner: ^3.5.5

  # Image Picker
  image_picker: ^1.0.5

  # Utilities
  intl: ^0.18.1  # Date formatting
  url_launcher: ^6.2.2
  share_plus: ^7.2.1

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^3.0.1
```

---

**Version:** 1.0.0
**Last Updated:** 2025-01-30
**Author:** ZiyoMarket Development Team

**Keyingi qadamlar:**
1. Backend bilan test qilish
2. Push notifications integratsiya
3. QR scanner qo'shish
4. Image picker va upload
5. Offline mode (caching)
6. Performance optimization
