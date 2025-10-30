# Desktop Admin Panel Developer Guide - ZiyoMarket

> **To'liq qo'llanma Desktop admin panel dasturchilari uchun: API Integration, Reports, Dashboard, Management**

## 📚 Mundarija

1. [Loyiha Haqida](#loyiha-haqida)
2. [Technology Stack Options](#technology-stack-options)
3. [API Base Configuration](#api-base-configuration)
4. [Authentication va Authorization](#authentication-va-authorization)
5. [Dashboard va Analytics](#dashboard-va-analytics)
6. [Product va Category Management](#product-va-category-management)
7. [Order Management](#order-management)
8. [Customer va Seller Management](#customer-va-seller-management)
9. [Support Chat Management](#support-chat-management)
10. [Delivery Management](#delivery-management)
11. [Content Management](#content-management)
12. [Reports va Analytics](#reports-va-analytics)
13. [Notification Management](#notification-management)
14. [System Settings](#system-settings)
15. [Best Practices](#best-practices)

---

## 🖥️ Loyiha Haqida

ZiyoMarket Admin Panel - bu desktop application administratorlar uchun. To'liq tizimni boshqarish, hisobotlar va analitika imkoniyatlarini taqdim etadi.

### Admin Panel Features

✅ **Dashboard**
- Real-time statistika
- Sotuv grafiklari
- Tezkor ko'rsatkichlar (KPI)
- Bugungi ko'rsatkichlar

✅ **Product Management**
- CRUD operations
- Bulk operations
- Stock management
- Category management
- QR code generation

✅ **Order Management**
- Order list with filters
- Order details
- Status updates
- Discount management
- Order cancellation

✅ **User Management**
- Customers CRUD
- Sellers CRUD
- Admin users
- User analytics
- Status management

✅ **Support System**
- Chat management
- Assign chats to admins
- Response time tracking
- Support statistics
- Chat history

✅ **Delivery Management**
- Delivery partners CRUD
- Order delivery tracking
- Status updates
- Performance metrics

✅ **Content Management**
- Blog posts
- News/Announcements
- FAQs
- Policies
- Publish/Unpublish

✅ **Reports & Analytics**
- Sales reports
- Inventory reports
- Customer analytics
- Seller performance
- Revenue tracking
- Export to Excel/PDF

✅ **Notifications**
- Send notifications
- Bulk notifications
- Notification history
- Targeted notifications

---

## 💻 Technology Stack Options

### Option 1: Electron + React (Recommended)

**Pros:**
- Cross-platform (Windows, Mac, Linux)
- Web teknologiyalaridan foydalanish
- Katta community
- React ecosystem

**Tech Stack:**
```
- Electron
- React 18
- TypeScript
- Redux Toolkit / Zustand
- Material-UI / Ant Design
- Axios
- Chart.js / Recharts
- React Router
```

### Option 2: Flutter Desktop

**Pros:**
- Single codebase (Mobile + Desktop)
- Native performance
- Beautiful UI

**Tech Stack:**
```
- Flutter Desktop
- Provider / Riverpod
- HTTP / Dio
- fl_chart
```

### Option 3: .NET WPF (Windows Only)

**Pros:**
- Native Windows
- Best performance
- Backend bilan bir texnologiya (.NET)

**Tech Stack:**
```
- WPF (.NET 8)
- MVVM pattern
- HttpClient
- LiveCharts
- MaterialDesignInXAML
```

### Option 4: Tauri + React

**Pros:**
- Lightweight alternative to Electron
- Rust backend
- Smaller bundle size

**Tech Stack:**
```
- Tauri
- React
- TypeScript
- Same as Electron
```

---

## 🔧 API Base Configuration

### Base URL and Endpoints

```typescript
// src/config/api.config.ts

export const API_CONFIG = {
  // Base URL
  BASE_URL: process.env.REACT_APP_API_URL || 'https://localhost:5001/api',

  // Timeout
  TIMEOUT: 30000,

  // API Version
  VERSION: 'v1',
};

export const ENDPOINTS = {
  // Auth
  AUTH: {
    LOGIN: '/auth/login',
    LOGOUT: '/auth/logout',
    ME: '/auth/me',
    CHANGE_PASSWORD: '/auth/change-password',
    REFRESH_TOKEN: '/auth/refresh-token',
  },

  // Dashboard
  DASHBOARD: {
    STATISTICS: '/report/dashboard',
    TODAY_STATS: '/report/dashboard/today',
  },

  // Products
  PRODUCTS: {
    LIST: '/product',
    DETAIL: (id: number) => `/product/${id}`,
    CREATE: '/product',
    UPDATE: (id: number) => `/product/${id}`,
    DELETE: (id: number) => `/product/${id}`,
    BULK_DELETE: '/product/bulk',
    SEED: '/product/seed',
    STOCK_UPDATE: '/product/stock/update',
    LOW_STOCK: '/product/stock/low',
  },

  // Categories
  CATEGORIES: {
    LIST: '/category',
    TREE: '/category/tree',
    DETAIL: (id: number) => `/category/${id}`,
    CREATE: '/category',
    UPDATE: (id: number) => `/category/${id}`,
    DELETE: (id: number) => `/category/${id}`,
    REORDER: '/category/reorder',
    TOGGLE_STATUS: (id: number) => `/category/${id}/toggle-status`,
    BULK_DELETE: '/category/bulk',
    SEED: '/category/seed',
  },

  // Orders
  ORDERS: {
    LIST: '/order',
    DETAIL: (id: number) => `/order/${id}`,
    UPDATE_STATUS: (id: number) => `/order/${id}/status`,
    CANCEL: (id: number) => `/order/${id}/cancel`,
    SUMMARY: '/order/summary',
    BULK_DELETE: '/order/bulk',
    SEED: '/order/seed',
  },

  // Customers
  CUSTOMERS: {
    LIST: '/customer',
    DETAIL: (id: number) => `/customer/${id}`,
    CREATE: '/customer',
    UPDATE: (id: number) => `/customer/${id}`,
    DELETE: (id: number) => `/customer/${id}`,
    ANALYTICS: '/customer/analytics',
    BULK_DELETE: '/customer/bulk',
  },

  // Sellers
  SELLERS: {
    LIST: '/seller',
    DETAIL: (id: number) => `/seller/${id}`,
    CREATE: '/seller',
    UPDATE: (id: number) => `/seller/${id}`,
    DELETE: (id: number) => `/seller/${id}`,
    PERFORMANCE: (id: number) => `/seller/${id}/performance`,
    TOP_SELLERS: '/seller/top',
    TOGGLE_STATUS: (id: number) => `/seller/${id}/toggle-status`,
    BULK_DELETE: '/seller/bulk',
    SEED: '/seller/seed',
  },

  // Support
  SUPPORT: {
    LIST: '/support',
    DETAIL: (id: number) => `/support/${id}`,
    CREATE: '/support',
    CLOSE: (id: number) => `/support/${id}/close`,
    REOPEN: (id: number) => `/support/${id}/reopen`,
    ASSIGN: (id: number) => `/support/${id}/assign`,
    UNASSIGN: (id: number) => `/support/${id}/unassign`,
    MESSAGES: (chatId: number) => `/support/${chatId}/messages`,
    SEND_MESSAGE: '/support/messages',
    MY_CHATS: '/support/admin/my-chats',
    MY_STATS: '/support/admin/my-stats',
    UNASSIGNED: '/support/admin/unassigned',
    OVERDUE: '/support/admin/overdue',
    STATISTICS: '/support/statistics',
    FEEDBACK: '/support/feedback',
    BULK_DELETE: '/support/bulk/delete',
    SEED_CHATS: '/support/seed/chats',
  },

  // Delivery
  DELIVERY: {
    PARTNERS: '/delivery/partners',
    PARTNER_DETAIL: (id: number) => `/delivery/partners/${id}`,
    CREATE_PARTNER: '/delivery/partners',
    UPDATE_PARTNER: (id: number) => `/delivery/partners/${id}`,
    DELETE_PARTNER: (id: number) => `/delivery/partners/${id}`,
    ORDER_DELIVERY: (orderId: number) => `/delivery/orders/${orderId}`,
    CREATE_DELIVERY: '/delivery/orders',
    UPDATE_STATUS: (id: number) => `/delivery/${id}/status`,
    TRACK: (trackingCode: string) => `/delivery/track/${trackingCode}`,
    BULK_DELETE: '/delivery/partners/bulk',
    SEED: '/delivery/partners/seed',
  },

  // Content
  CONTENT: {
    LIST: '/content',
    PUBLISHED: '/content/published',
    BY_TYPE: (type: string) => `/content/type/${type}`,
    DETAIL: (id: number) => `/content/${id}`,
    CREATE: '/content',
    UPDATE: (id: number) => `/content/${id}`,
    DELETE: (id: number) => `/content/${id}`,
    PUBLISH: (id: number) => `/content/${id}/publish`,
    UNPUBLISH: (id: number) => `/content/${id}/unpublish`,
    BULK_DELETE: '/content/bulk',
    SEED: '/content/seed',
  },

  // Notifications
  NOTIFICATIONS: {
    LIST: '/notification',
    SEND: '/notification/send',
    SEND_BULK: '/notification/send-bulk',
    DELETE: (id: number) => `/notification/${id}`,
    BULK_DELETE: '/notification/bulk',
    SEED: '/notification/seed',
  },

  // Reports
  REPORTS: {
    SALES: '/report/sales',
    DAILY_SALES: '/report/sales/daily',
    SALES_CHART: '/report/sales/chart',
    TOP_PRODUCTS: '/report/products/top',
    INVENTORY: '/report/inventory',
    LOW_STOCK: '/report/inventory/low-stock',
    CUSTOMER_ANALYTICS: '/report/customers/analytics',
    TOP_CUSTOMERS: '/report/customers/top',
    SELLER_PERFORMANCE: '/report/sellers/performance',
  },
};
```

---

## 🔐 Authentication va Authorization

### 1. HTTP Client Setup (Axios)

```typescript
// src/services/http.service.ts

import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { API_CONFIG } from '../config/api.config';

class HttpService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_CONFIG.BASE_URL,
      timeout: API_CONFIG.TIMEOUT,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor - Add token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('access_token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - Handle errors
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        // If 401 and not already retrying
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;

          try {
            const refreshToken = localStorage.getItem('refresh_token');
            const response = await this.post('/auth/refresh-token', {
              refreshToken,
            });

            if (response.data.success) {
              const { accessToken, refreshToken: newRefreshToken } = response.data.data;
              localStorage.setItem('access_token', accessToken);
              localStorage.setItem('refresh_token', newRefreshToken);

              // Retry original request
              originalRequest.headers.Authorization = `Bearer ${accessToken}`;
              return this.api(originalRequest);
            }
          } catch (refreshError) {
            // Refresh failed, logout
            localStorage.removeItem('access_token');
            localStorage.removeItem('refresh_token');
            window.location.href = '/login';
            return Promise.reject(refreshError);
          }
        }

        return Promise.reject(error);
      }
    );
  }

  async get<T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.api.get<T>(url, config);
  }

  async post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.api.post<T>(url, data, config);
  }

  async put<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.api.put<T>(url, data, config);
  }

  async delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.api.delete<T>(url, config);
  }

  async patch<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
    return this.api.patch<T>(url, data, config);
  }
}

export const httpService = new HttpService();
```

### 2. Auth Service

```typescript
// src/services/auth.service.ts

import { httpService } from './http.service';
import { ENDPOINTS } from '../config/api.config';

export interface LoginRequest {
  phoneOrEmail: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AdminUser;
}

export interface AdminUser {
  id: number;
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  permissions: string[];
  isActive: boolean;
}

class AuthService {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await httpService.post(ENDPOINTS.AUTH.LOGIN, {
      ...credentials,
      userType: 'Admin',
    });

    if (response.data.success) {
      const { accessToken, refreshToken, user } = response.data.data;

      // Save tokens
      localStorage.setItem('access_token', accessToken);
      localStorage.setItem('refresh_token', refreshToken);
      localStorage.setItem('user', JSON.stringify(user));

      return response.data.data;
    } else {
      throw new Error(response.data.message || 'Login failed');
    }
  }

  async logout(): Promise<void> {
    try {
      await httpService.post(ENDPOINTS.AUTH.LOGOUT);
    } finally {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('user');
    }
  }

  async getProfile(): Promise<AdminUser> {
    const response = await httpService.get(ENDPOINTS.AUTH.ME);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to get profile');
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    const response = await httpService.post(ENDPOINTS.AUTH.CHANGE_PASSWORD, {
      currentPassword,
      newPassword,
    });

    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to change password');
    }
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('access_token');
  }

  getCurrentUser(): AdminUser | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
}

export const authService = new AuthService();
```

---

## 📊 Dashboard va Analytics

### Dashboard Service

```typescript
// src/services/dashboard.service.ts

import { httpService } from './http.service';
import { ENDPOINTS } from '../config/api.config';

export interface DashboardStats {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  totalProducts: number;
  todayRevenue: number;
  todayOrders: number;
  pendingOrders: number;
  lowStockProducts: number;
  revenueGrowth: number;
  ordersGrowth: number;
  topSellingProducts: TopProduct[];
  recentOrders: RecentOrder[];
}

export interface TopProduct {
  productId: number;
  productName: string;
  totalSold: number;
  revenue: number;
  imageUrl?: string;
}

export interface RecentOrder {
  id: number;
  orderNumber: string;
  customerName: string;
  totalPrice: number;
  status: string;
  orderDate: string;
}

export interface TodayStats {
  revenue: number;
  orders: number;
  newCustomers: number;
  pendingOrders: number;
}

class DashboardService {
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await httpService.get(ENDPOINTS.DASHBOARD.STATISTICS);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load dashboard statistics');
  }

  async getTodayStats(): Promise<TodayStats> {
    const response = await httpService.get(ENDPOINTS.DASHBOARD.TODAY_STATS);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load today statistics');
  }
}

export const dashboardService = new DashboardService();
```

### Dashboard Component Example (React)

```typescript
// src/components/Dashboard/Dashboard.tsx

import React, { useEffect, useState } from 'react';
import { dashboardService, DashboardStats } from '../../services/dashboard.service';
import { Card, Row, Col, Statistic, Table, Spin } from 'antd';
import {
  DollarOutlined,
  ShoppingCartOutlined,
  UserOutlined,
  InboxOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
} from '@ant-design/icons';

export const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const data = await dashboardService.getDashboardStats();
      setStats(data);
    } catch (error) {
      console.error('Error loading dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <Spin size="large" />;
  }

  return (
    <div className="dashboard">
      <h1>Dashboard</h1>

      {/* KPI Cards */}
      <Row gutter={16}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Total Revenue"
              value={stats?.totalRevenue}
              prefix={<DollarOutlined />}
              suffix="UZS"
              valueStyle={{ color: '#3f8600' }}
            />
            <div>
              {stats?.revenueGrowth && stats.revenueGrowth > 0 ? (
                <ArrowUpOutlined style={{ color: 'green' }} />
              ) : (
                <ArrowDownOutlined style={{ color: 'red' }} />
              )}
              {stats?.revenueGrowth}%
            </div>
          </Card>
        </Col>

        <Col span={6}>
          <Card>
            <Statistic
              title="Total Orders"
              value={stats?.totalOrders}
              prefix={<ShoppingCartOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
            <div>
              {stats?.ordersGrowth && stats.ordersGrowth > 0 ? (
                <ArrowUpOutlined style={{ color: 'green' }} />
              ) : (
                <ArrowDownOutlined style={{ color: 'red' }} />
              )}
              {stats?.ordersGrowth}%
            </div>
          </Card>
        </Col>

        <Col span={6}>
          <Card>
            <Statistic
              title="Total Customers"
              value={stats?.totalCustomers}
              prefix={<UserOutlined />}
            />
          </Card>
        </Col>

        <Col span={6}>
          <Card>
            <Statistic
              title="Total Products"
              value={stats?.totalProducts}
              prefix={<InboxOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Today Stats */}
      <Row gutter={16} style={{ marginTop: 16 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="Today's Revenue"
              value={stats?.todayRevenue}
              suffix="UZS"
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>

        <Col span={6}>
          <Card>
            <Statistic
              title="Today's Orders"
              value={stats?.todayOrders}
            />
          </Card>
        </Col>

        <Col span={6}>
          <Card>
            <Statistic
              title="Pending Orders"
              value={stats?.pendingOrders}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>

        <Col span={6}>
          <Card>
            <Statistic
              title="Low Stock Products"
              value={stats?.lowStockProducts}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Top Products */}
      <Card title="Top Selling Products" style={{ marginTop: 16 }}>
        <Table
          dataSource={stats?.topSellingProducts}
          columns={[
            { title: 'Product Name', dataIndex: 'productName', key: 'productName' },
            { title: 'Total Sold', dataIndex: 'totalSold', key: 'totalSold' },
            { title: 'Revenue', dataIndex: 'revenue', key: 'revenue', render: (val) => `${val} UZS` },
          ]}
          pagination={false}
        />
      </Card>

      {/* Recent Orders */}
      <Card title="Recent Orders" style={{ marginTop: 16 }}>
        <Table
          dataSource={stats?.recentOrders}
          columns={[
            { title: 'Order #', dataIndex: 'orderNumber', key: 'orderNumber' },
            { title: 'Customer', dataIndex: 'customerName', key: 'customerName' },
            { title: 'Total', dataIndex: 'totalPrice', key: 'totalPrice', render: (val) => `${val} UZS` },
            { title: 'Status', dataIndex: 'status', key: 'status' },
            { title: 'Date', dataIndex: 'orderDate', key: 'orderDate' },
          ]}
          pagination={{ pageSize: 5 }}
        />
      </Card>
    </div>
  );
};
```

---

## 🛍️ Product va Category Management

### Product Service

```typescript
// src/services/product.service.ts

import { httpService } from './http.service';
import { ENDPOINTS } from '../config/api.config';

export interface Product {
  id: number;
  name: string;
  description: string;
  qrCode: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  categoryName: string;
  status: string;
  imageUrl?: string;
  weight?: number;
  dimensions?: string;
  manufacturer?: string;
  minStockLevel: number;
  isLowStock: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  qrCode: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  imageUrl?: string;
  weight?: number;
  dimensions?: string;
  manufacturer?: string;
  minStockLevel?: number;
}

export interface ProductFilterRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

class ProductService {
  async getProducts(filter: ProductFilterRequest): Promise<PaginatedResponse<Product>> {
    const response = await httpService.get(ENDPOINTS.PRODUCTS.LIST, { params: filter });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load products');
  }

  async getProductById(id: number): Promise<Product> {
    const response = await httpService.get(ENDPOINTS.PRODUCTS.DETAIL(id));
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load product');
  }

  async createProduct(product: CreateProductDto): Promise<Product> {
    const response = await httpService.post(ENDPOINTS.PRODUCTS.CREATE, product);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error(response.data.message || 'Failed to create product');
  }

  async updateProduct(id: number, product: Partial<CreateProductDto>): Promise<Product> {
    const response = await httpService.put(ENDPOINTS.PRODUCTS.UPDATE(id), product);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error(response.data.message || 'Failed to update product');
  }

  async deleteProduct(id: number): Promise<void> {
    const response = await httpService.delete(ENDPOINTS.PRODUCTS.DELETE(id));
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to delete product');
    }
  }

  async bulkDeleteProducts(startDate?: string, endDate?: string): Promise<void> {
    const response = await httpService.delete(ENDPOINTS.PRODUCTS.BULK_DELETE, {
      params: { startDate, endDate },
    });
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to delete products');
    }
  }

  async updateStock(productId: number, quantity: number, notes?: string): Promise<void> {
    const response = await httpService.post(ENDPOINTS.PRODUCTS.STOCK_UPDATE, {
      productId,
      quantity,
      notes,
    });
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to update stock');
    }
  }

  async getLowStockProducts(): Promise<Product[]> {
    const response = await httpService.get(ENDPOINTS.PRODUCTS.LOW_STOCK);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load low stock products');
  }

  async seedProducts(): Promise<void> {
    const response = await httpService.post(ENDPOINTS.PRODUCTS.SEED);
    if (!response.data.success) {
      throw new Error('Failed to seed products');
    }
  }
}

export const productService = new ProductService();
```

---

## 📦 Order Management

### Order Service

```typescript
// src/services/order.service.ts

import { httpService } from './http.service';
import { ENDPOINTS } from '../config/api.config';

export interface Order {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  sellerId?: number;
  sellerName?: string;
  orderDate: string;
  status: string;
  totalPrice: number;
  discountApplied: number;
  cashbackUsed: number;
  finalPrice: number;
  paymentMethod: string;
  deliveryType?: string;
  deliveryAddress?: string;
  customerNotes?: string;
  sellerNotes?: string;
  adminNotes?: string;
  items: OrderItem[];
  delivery?: OrderDelivery;
}

export interface OrderItem {
  productId: number;
  productName: string;
  productImage?: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
  discountApplied: number;
  totalPrice: number;
}

export interface OrderDelivery {
  id: number;
  orderId: number;
  deliveryPartnerId: number;
  deliveryPartnerName: string;
  trackingCode: string;
  status: string;
  estimatedDeliveryDate?: string;
  actualDeliveryDate?: string;
  deliveryAddress: string;
  notes?: string;
}

export interface OrderFilterRequest {
  pageNumber?: number;
  pageSize?: number;
  status?: string;
  customerId?: number;
  sellerId?: number;
  startDate?: string;
  endDate?: string;
  searchTerm?: string;
}

export interface UpdateOrderStatusDto {
  status: string;
  notes?: string;
}

export interface OrderSummary {
  totalOrders: number;
  totalRevenue: number;
  totalItems: number;
  averageOrderValue: number;
  byStatus: { [key: string]: number };
  byPaymentMethod: { [key: string]: number };
}

class OrderService {
  async getOrders(filter: OrderFilterRequest): Promise<PaginatedResponse<Order>> {
    const response = await httpService.get(ENDPOINTS.ORDERS.LIST, { params: filter });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load orders');
  }

  async getOrderById(id: number): Promise<Order> {
    const response = await httpService.get(ENDPOINTS.ORDERS.DETAIL(id));
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load order');
  }

  async updateOrderStatus(id: number, dto: UpdateOrderStatusDto): Promise<Order> {
    const response = await httpService.put(ENDPOINTS.ORDERS.UPDATE_STATUS(id), dto);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error(response.data.message || 'Failed to update order status');
  }

  async cancelOrder(id: number): Promise<void> {
    const response = await httpService.post(ENDPOINTS.ORDERS.CANCEL(id));
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to cancel order');
    }
  }

  async getOrderSummary(startDate?: string, endDate?: string): Promise<OrderSummary> {
    const response = await httpService.get(ENDPOINTS.ORDERS.SUMMARY, {
      params: { startDate, endDate },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load order summary');
  }

  async bulkDeleteOrders(startDate?: string, endDate?: string): Promise<void> {
    const response = await httpService.delete(ENDPOINTS.ORDERS.BULK_DELETE, {
      params: { startDate, endDate },
    });
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to delete orders');
    }
  }

  async seedOrders(): Promise<void> {
    const response = await httpService.post(ENDPOINTS.ORDERS.SEED);
    if (!response.data.success) {
      throw new Error('Failed to seed orders');
    }
  }
}

export const orderService = new OrderService();
```

---

## 📈 Reports va Analytics

### Report Service

```typescript
// src/services/report.service.ts

import { httpService } from './http.service';
import { ENDPOINTS } from '../config/api.config';

export interface SalesReport {
  period: {
    startDate: string;
    endDate: string;
  };
  summary: {
    totalRevenue: number;
    totalOrders: number;
    totalItems: number;
    averageOrderValue: number;
    totalDiscounts: number;
    totalCashbackUsed: number;
  };
  chartData: SalesChartData[];
}

export interface SalesChartData {
  date: string;
  revenue: number;
  orders: number;
  items: number;
}

export interface TopProductReport {
  productId: number;
  productName: string;
  totalSold: number;
  revenue: number;
  imageUrl?: string;
}

export interface InventoryReport {
  totalProducts: number;
  totalStock: number;
  totalValue: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  byCategory: CategoryInventory[];
}

export interface CategoryInventory {
  categoryId: number;
  categoryName: string;
  productCount: number;
  totalStock: number;
  totalValue: number;
}

export interface CustomerAnalytics {
  totalCustomers: number;
  activeCustomers: number;
  newCustomers: number;
  averageOrdersPerCustomer: number;
  averageRevenuePerCustomer: number;
  topCustomers: TopCustomer[];
}

export interface TopCustomer {
  customerId: number;
  customerName: string;
  totalOrders: number;
  totalSpent: number;
}

export interface SellerPerformanceReport {
  sellerId: number;
  sellerName: string;
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  commissionEarned: number;
}

class ReportService {
  async getSalesReport(
    startDate: string,
    endDate: string,
    groupBy: 'Day' | 'Week' | 'Month' = 'Day'
  ): Promise<SalesReport> {
    const response = await httpService.get(ENDPOINTS.REPORTS.SALES, {
      params: { startDate, endDate, groupBy },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load sales report');
  }

  async getDailySales(date: string): Promise<SalesChartData> {
    const response = await httpService.get(ENDPOINTS.REPORTS.DAILY_SALES, {
      params: { date },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load daily sales');
  }

  async getSalesChart(startDate: string, endDate: string): Promise<SalesChartData[]> {
    const response = await httpService.get(ENDPOINTS.REPORTS.SALES_CHART, {
      params: { startDate, endDate },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load sales chart');
  }

  async getTopProducts(
    startDate?: string,
    endDate?: string,
    limit: number = 10
  ): Promise<TopProductReport[]> {
    const response = await httpService.get(ENDPOINTS.REPORTS.TOP_PRODUCTS, {
      params: { startDate, endDate, limit },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load top products');
  }

  async getInventoryReport(): Promise<InventoryReport> {
    const response = await httpService.get(ENDPOINTS.REPORTS.INVENTORY);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load inventory report');
  }

  async getLowStockReport(): Promise<Product[]> {
    const response = await httpService.get(ENDPOINTS.REPORTS.LOW_STOCK);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load low stock report');
  }

  async getCustomerAnalytics(): Promise<CustomerAnalytics> {
    const response = await httpService.get(ENDPOINTS.REPORTS.CUSTOMER_ANALYTICS);
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load customer analytics');
  }

  async getTopCustomers(limit: number = 10): Promise<TopCustomer[]> {
    const response = await httpService.get(ENDPOINTS.REPORTS.TOP_CUSTOMERS, {
      params: { limit },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load top customers');
  }

  async getSellerPerformance(
    startDate?: string,
    endDate?: string
  ): Promise<SellerPerformanceReport[]> {
    const response = await httpService.get(ENDPOINTS.REPORTS.SELLER_PERFORMANCE, {
      params: { startDate, endDate },
    });
    if (response.data.success) {
      return response.data.data;
    }
    throw new Error('Failed to load seller performance');
  }

  // Export to Excel/PDF (implement with libraries like xlsx, jspdf)
  async exportToExcel(data: any, filename: string): Promise<void> {
    // Implementation using xlsx library
    // import * as XLSX from 'xlsx';
    // const ws = XLSX.utils.json_to_sheet(data);
    // const wb = XLSX.utils.book_new();
    // XLSX.utils.book_append_sheet(wb, ws, 'Report');
    // XLSX.writeFile(wb, `${filename}.xlsx`);
  }

  async exportToPDF(data: any, filename: string): Promise<void> {
    // Implementation using jspdf library
    // import jsPDF from 'jspdf';
    // import 'jspdf-autotable';
  }
}

export const reportService = new ReportService();
```

---

## 🎯 Best Practices

### 1. Error Handling

```typescript
// src/utils/error-handler.ts

export interface ApiError {
  message: string;
  statusCode: number;
  errors?: string[];
}

export const handleApiError = (error: any): ApiError => {
  if (error.response) {
    // Server responded with error
    return {
      message: error.response.data?.message || 'An error occurred',
      statusCode: error.response.status,
      errors: error.response.data?.errors,
    };
  } else if (error.request) {
    // No response received
    return {
      message: 'No response from server. Please check your connection.',
      statusCode: 0,
    };
  } else {
    // Other errors
    return {
      message: error.message || 'An unexpected error occurred',
      statusCode: 0,
    };
  }
};

export const showErrorNotification = (error: ApiError) => {
  // Using ant design notification
  // notification.error({
  //   message: 'Error',
  //   description: error.message,
  // });
};
```

### 2. Loading States

```typescript
// src/hooks/useAsync.ts

import { useState, useCallback } from 'react';

interface AsyncState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

export function useAsync<T>() {
  const [state, setState] = useState<AsyncState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const execute = useCallback(async (promise: Promise<T>) => {
    setState({ data: null, loading: true, error: null });
    try {
      const data = await promise;
      setState({ data, loading: false, error: null });
      return data;
    } catch (error: any) {
      setState({ data: null, loading: false, error: error.message });
      throw error;
    }
  }, []);

  return { ...state, execute };
}
```

### 3. Data Caching

```typescript
// src/utils/cache.ts

class CacheService {
  private cache: Map<string, { data: any; timestamp: number }> = new Map();
  private ttl: number = 5 * 60 * 1000; // 5 minutes

  set(key: string, data: any, customTtl?: number): void {
    this.cache.set(key, {
      data,
      timestamp: Date.now() + (customTtl || this.ttl),
    });
  }

  get(key: string): any | null {
    const cached = this.cache.get(key);
    if (!cached) return null;

    if (Date.now() > cached.timestamp) {
      this.cache.delete(key);
      return null;
    }

    return cached.data;
  }

  clear(key?: string): void {
    if (key) {
      this.cache.delete(key);
    } else {
      this.cache.clear();
    }
  }
}

export const cacheService = new CacheService();
```

### 4. Form Validation

```typescript
// Using Formik + Yup for React

import * as Yup from 'yup';

export const productValidationSchema = Yup.object({
  name: Yup.string()
    .required('Product name is required')
    .min(2, 'Name must be at least 2 characters')
    .max(200, 'Name cannot exceed 200 characters'),
  description: Yup.string().required('Description is required'),
  qrCode: Yup.string()
    .required('QR code is required')
    .matches(/^PRD-\d{3}$/, 'QR code must be in format PRD-XXX'),
  price: Yup.number()
    .required('Price is required')
    .positive('Price must be positive'),
  stockQuantity: Yup.number()
    .required('Stock quantity is required')
    .min(0, 'Stock cannot be negative'),
  categoryId: Yup.number()
    .required('Category is required')
    .positive('Select a valid category'),
});
```

---

## 📦 Recommended Packages

### For Electron + React

```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.1",
    "axios": "^1.6.2",
    "antd": "^5.12.1",
    "@ant-design/icons": "^5.2.6",
    "recharts": "^2.10.3",
    "formik": "^2.4.5",
    "yup": "^1.3.3",
    "date-fns": "^3.0.6",
    "xlsx": "^0.18.5",
    "jspdf": "^2.5.1",
    "jspdf-autotable": "^3.8.2"
  },
  "devDependencies": {
    "electron": "^28.0.0",
    "electron-builder": "^24.9.1",
    "@types/react": "^18.2.45",
    "@types/react-dom": "^18.2.18",
    "typescript": "^5.3.3",
    "vite": "^5.0.8"
  }
}
```

---

## 🎓 Key Features Summary

### Admin Panel Must-Have Features

| Feature | Priority | Endpoints |
|---------|----------|-----------|
| **Dashboard** | High | `/report/dashboard`, `/report/dashboard/today` |
| **Product CRUD** | High | `/product/*` |
| **Category CRUD** | High | `/category/*` |
| **Order Management** | High | `/order/*` |
| **User Management** | High | `/customer/*`, `/seller/*` |
| **Support Chat** | Medium | `/support/*` |
| **Reports** | High | `/report/*` |
| **Delivery Tracking** | Medium | `/delivery/*` |
| **Content Management** | Low | `/content/*` |
| **Notifications** | Medium | `/notification/*` |

---

**Version:** 1.0.0
**Last Updated:** 2025-01-30
**Author:** ZiyoMarket Development Team

**Keyingi qadamlar:**
1. Technology stack tanlash (Electron/Flutter/WPF)
2. UI/UX design
3. API integration
4. Charts va graphs (Recharts, Chart.js)
5. Excel/PDF export
6. Real-time updates (WebSockets - optional)
7. Testing va debugging
