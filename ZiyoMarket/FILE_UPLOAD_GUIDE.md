# File Upload System Guide

## 📁 Overview

Professional file upload system for ZiyoMarket API with support for:
- Product images
- Category images
- Banner images
- User avatars (profile pictures)
- Local and Railway (production) deployment

## 🏗️ Architecture

### Folder Structure

```
src/ZiyoMarket.Api/
└── wwwroot/
    └── images/
        ├── products/      # Product images
        ├── categories/    # Category images
        ├── banners/       # Banner/promotional images
        ├── users/         # User avatars
        └── temp/          # Temporary uploads
```

### Components

1. **FileUploadService** - Core service for handling uploads
2. **FileUploadController** - REST API endpoints
3. **FileUploadSettings** - Configuration (appsettings.json)
4. **FileUploadResultDto** - Response format with URLs

## 🚀 Quick Start

### 1. Upload a Product Image

**Request:**
```http
POST /api/file_upload/product
Content-Type: multipart/form-data

file: [image file]
```

**Response:**
```json
{
  "file_name": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
  "file_path": "images/products/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
  "file_url": "http://localhost:8080/images/products/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
  "file_size": 245678,
  "uploaded_at": "2026-02-08 14:30:00"
}
```

### 2. Upload a Category Image

**Request:**
```http
POST /api/file_upload/category
Content-Type: multipart/form-data

file: [image file]
```

### 3. Upload a Banner Image

**Request:**
```http
POST /api/file_upload/banner
Content-Type: multipart/form-data

file: [image file]
```

### 4. Upload User Avatar

**Request:**
```http
POST /api/file_upload/user/avatar
Content-Type: multipart/form-data

file: [image file]
```

## 📋 Available Endpoints

| Endpoint | Method | Description | Auth Required |
|----------|--------|-------------|---------------|
| `/api/file_upload/product` | POST | Upload single product image | ✅ |
| `/api/file_upload/product/multiple` | POST | Upload multiple product images | ✅ |
| `/api/file_upload/category` | POST | Upload category image | ✅ |
| `/api/file_upload/banner` | POST | Upload banner image | ✅ |
| `/api/file_upload/banner/multiple` | POST | Upload multiple banners | ✅ |
| `/api/file_upload/user/avatar` | POST | Upload user avatar | ✅ |
| `/api/file_upload` | DELETE | Delete image by path | ✅ |
| `/api/file_upload/delete-multiple` | POST | Delete multiple images | ✅ |
| `/api/file_upload/url` | GET | Get full URL for file path | ❌ |

## 🔒 Validation Rules

### File Size
- **Maximum:** 5 MB (configurable in appsettings.json)
- Files larger than 5MB will be rejected

### Allowed Extensions
- `.jpg`, `.jpeg`
- `.png`
- `.gif`
- `.webp`
- `.svg`

### Content Types
- `image/jpeg`
- `image/png`
- `image/gif`
- `image/webp`
- `image/svg+xml`

## ⚙️ Configuration

Edit `src/ZiyoMarket.Api/appsettings.json`:

```json
{
  "FileUploadSettings": {
    "MaxFileSizeBytes": 5242880,
    "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"],
    "UploadPath": "images",
    "ProductImagesPath": "images/products",
    "CategoryImagesPath": "images/categories",
    "BannerImagesPath": "images/banners",
    "UserImagesPath": "images/users",
    "TempImagesPath": "images/temp"
  }
}
```

## 📝 Usage Examples

### Example 1: Upload Product Image (cURL)

```bash
curl -X POST "http://localhost:8080/api/file_upload/product" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -F "file=@product.jpg"
```

### Example 2: Upload Multiple Banners (cURL)

```bash
curl -X POST "http://localhost:8080/api/file_upload/banner/multiple" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -F "files=@banner1.jpg" \
  -F "files=@banner2.jpg" \
  -F "files=@banner3.jpg"
```

### Example 3: Delete Image (cURL)

```bash
curl -X DELETE "http://localhost:8080/api/file_upload?filePath=images/products/abc123.jpg" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Example 4: JavaScript/Fetch

```javascript
// Upload single image
const formData = new FormData();
formData.append('file', imageFile);

const response = await fetch('http://localhost:8080/api/file_upload/product', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer YOUR_ACCESS_TOKEN'
  },
  body: formData
});

const result = await response.json();
console.log(result.file_url); // Full URL to access image
```

### Example 5: Flutter/Dart

```dart
import 'package:http/http.dart' as http;
import 'dart:io';

Future<String> uploadProductImage(File imageFile, String token) async {
  var request = http.MultipartRequest(
    'POST',
    Uri.parse('http://localhost:8080/api/file_upload/product'),
  );

  request.headers['Authorization'] = 'Bearer $token';
  request.files.add(await http.MultipartFile.fromPath('file', imageFile.path));

  var response = await request.send();
  var responseData = await response.stream.bytesToString();
  var json = jsonDecode(responseData);

  return json['file_url']; // Full URL
}
```

## 🌐 Production Deployment (Railway)

### Environment Variables

Set in Railway dashboard:

```bash
APP_BASE_URL=https://your-railway-app.up.railway.app
```

### Important Notes

1. **Persistent Storage:** Railway uses ephemeral file systems. Uploaded files may be deleted on redeploy.
2. **Solution:** For production, integrate with cloud storage:
   - AWS S3
   - Cloudinary
   - Azure Blob Storage
   - Google Cloud Storage

3. **Current Setup:** Works for testing and small-scale deployments.

### Railway File Persistence Options

**Option 1: Use Railway Volumes (Persistent Storage)**
```bash
# In Railway dashboard, add a volume
# Mount path: /app/src/ZiyoMarket.Api/wwwroot
```

**Option 2: Cloud Storage Integration (Recommended)**
- Modify `FileUploadService` to use cloud provider SDK
- Store files in S3/Cloudinary/Azure
- Return cloud URLs instead of local paths

## 🔧 Integration with Entities

### Product Entity

```csharp
// Upload image
var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.Product);

// Save to database
product.ImageUrl = uploadResult.FilePath; // "images/products/abc123.jpg"
await _unitOfWork.Products.UpdateAsync(product);
await _unitOfWork.SaveChangesAsync();

// Access image URL
var fullUrl = _fileUploadService.GetFileUrl(product.ImageUrl);
// Result: "http://localhost:8080/images/products/abc123.jpg"
```

### Category Entity

```csharp
var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.Category);
category.ImageUrl = uploadResult.FilePath;
await _unitOfWork.SaveChangesAsync();
```

### User Entity (Avatar)

```csharp
var uploadResult = await _fileUploadService.UploadImageAsync(imageFile, ImageCategory.User);
customer.ImageUrl = uploadResult.FilePath; // Add ImageUrl field to Customer entity
await _unitOfWork.SaveChangesAsync();
```

## 🧪 Testing Checklist

- [ ] Upload single product image
- [ ] Upload multiple product images
- [ ] Upload category image
- [ ] Upload banner image
- [ ] Upload user avatar
- [ ] Delete image
- [ ] Delete multiple images
- [ ] Verify file size validation (upload >5MB file, should fail)
- [ ] Verify extension validation (upload .exe file, should fail)
- [ ] Access uploaded image via browser (http://localhost:8080/images/products/...)
- [ ] Test authentication (upload without token, should fail)

## 🛡️ Security Features

1. **File Size Validation** - Prevents large file uploads
2. **Extension Whitelist** - Only allowed image formats
3. **Content Type Check** - Validates MIME type
4. **Authentication Required** - All upload endpoints require JWT token
5. **Unique Filenames** - GUID-based names prevent overwriting
6. **Path Sanitization** - Prevents directory traversal attacks

## 📊 File Naming Convention

Files are renamed using GUID to ensure uniqueness:

```
Original: my-product-photo.jpg
Saved as: a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg
```

This prevents:
- Filename conflicts
- Overwriting existing files
- Special character issues
- Cross-platform compatibility problems

## 🔍 Troubleshooting

### Issue: File not accessible after upload

**Solution:** Ensure `app.UseStaticFiles()` is called in `Program.cs` before `app.UseAuthorization()`

### Issue: 413 Payload Too Large

**Solution:** Increase max request body size in `Program.cs`:

```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

### Issue: Images deleted after Railway redeploy

**Solution:** Use Railway Volumes or migrate to cloud storage (S3, Cloudinary)

### Issue: CORS error from frontend

**Solution:** Add CORS policy in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

## 📈 Performance Tips

1. **Image Optimization:** Compress images before upload (client-side)
2. **Thumbnails:** Generate thumbnails for large images
3. **CDN:** Use CDN for serving static files in production
4. **Lazy Loading:** Implement lazy loading in frontend
5. **Caching:** Set proper cache headers for static files

## 🔄 Migration to Cloud Storage

When ready for production, migrate to cloud storage:

1. Install cloud provider SDK (AWS, Azure, Cloudinary)
2. Update `FileUploadService` to use cloud APIs
3. Update configuration with cloud credentials
4. Test thoroughly
5. Migrate existing files

Example structure for cloud migration:

```csharp
public interface IFileUploadService
{
    Task<FileUploadResultDto> UploadImageAsync(IFormFile file, ImageCategory category);
}

public class S3FileUploadService : IFileUploadService
{
    // AWS S3 implementation
}

public class LocalFileUploadService : IFileUploadService
{
    // Current local implementation
}

// Register based on environment
if (app.Environment.IsProduction())
{
    builder.Services.AddScoped<IFileUploadService, S3FileUploadService>();
}
else
{
    builder.Services.AddScoped<IFileUploadService, LocalFileUploadService>();
}
```

---

**Version:** 1.0.0
**Last Updated:** 2026-02-08
**Author:** ZiyoMarket Development Team
