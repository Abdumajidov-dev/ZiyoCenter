# 📁 wwwroot Static Files System - Setup Summary

## ✅ Implementation Complete

Professional static file management system has been successfully implemented for ZiyoMarket API.

---

## 🎯 What Was Created

### 1. Folder Structure ✅

```
src/ZiyoMarket.Api/wwwroot/
└── images/
    ├── products/      # Product images
    ├── categories/    # Category images
    ├── banners/       # Banner/promotional images
    ├── users/         # User avatars
    └── temp/          # Temporary uploads
```

### 2. Core Services ✅

- **`FileUploadService.cs`** - Main upload service with validation
- **`FileUploadSettings.cs`** - Configuration class
- **`IFileUploadService.cs`** - Service interface
- **`FileUploadResultDto.cs`** - Response DTO
- **`ImageCategory.cs`** - Enum for image types

### 3. API Controller ✅

**`FileUploadController.cs`** with endpoints:
- `POST /api/file_upload/product` - Upload product image
- `POST /api/file_upload/product/multiple` - Upload multiple products
- `POST /api/file_upload/category` - Upload category image
- `POST /api/file_upload/banner` - Upload banner
- `POST /api/file_upload/banner/multiple` - Upload multiple banners
- `POST /api/file_upload/user/avatar` - Upload user avatar
- `DELETE /api/file_upload?filePath=...` - Delete image
- `POST /api/file_upload/delete-multiple` - Delete multiple images
- `GET /api/file_upload/url?filePath=...` - Get full URL

### 4. Configuration ✅

**Program.cs** updated:
- `app.UseStaticFiles()` - Serves wwwroot files
- Service registration for FileUploadService
- HttpContextAccessor registration

**appsettings.json** updated:
```json
{
  "FileUploadSettings": {
    "MaxFileSizeBytes": 5242880,
    "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"],
    "ProductImagesPath": "images/products",
    "CategoryImagesPath": "images/categories",
    "BannerImagesPath": "images/banners",
    "UserImagesPath": "images/users"
  }
}
```

### 5. Security & Validation ✅

- File size validation (max 5MB)
- Extension whitelist (only image types)
- Content-type validation
- Authentication required for uploads
- GUID-based unique filenames
- .gitignore updated (uploads not committed)

### 6. Entity Integration ✅

Existing entities already have `ImageUrl` property:
- ✅ `Product.ImageUrl`
- ✅ `Category.ImageUrl`
- ✅ `Customer` (can add ImageUrl for avatar)
- ✅ `User.ImageUrl` (new unified system)

### 7. Documentation ✅

- **`FILE_UPLOAD_GUIDE.md`** - Complete usage guide
- **`WWWROOT_SETUP_SUMMARY.md`** - This file

---

## 🚀 How to Test

### 1. Run the API

```bash
cd src\ZiyoMarket.Api
dotnet run
```

### 2. Open Swagger

Navigate to: `http://localhost:8080/swagger`

### 3. Upload Test Image

1. Find **FileUpload** section in Swagger
2. Click **POST /api/file_upload/product**
3. Click **Try it out**
4. Click **Choose File** and select an image
5. Add Authorization header: `Bearer YOUR_TOKEN`
6. Click **Execute**

### 4. Verify Upload

**Response:**
```json
{
  "file_name": "abc123-def456.jpg",
  "file_path": "images/products/abc123-def456.jpg",
  "file_url": "http://localhost:8080/images/products/abc123-def456.jpg",
  "file_size": 245678,
  "uploaded_at": "2026-02-08 14:30:00"
}
```

### 5. Access Image in Browser

Copy the `file_url` and open it in browser:
```
http://localhost:8080/images/products/abc123-def456.jpg
```

---

## 📝 Usage Examples

### Example 1: Upload Product Image (JavaScript)

```javascript
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
console.log(result.file_url); // Use this URL in your UI
```

### Example 2: Save to Database (C#)

```csharp
// 1. Upload image
var uploadResult = await _fileUploadService.UploadImageAsync(file, ImageCategory.Product);

// 2. Save path to database
product.ImageUrl = uploadResult.FilePath; // "images/products/abc123.jpg"
await _unitOfWork.Products.UpdateAsync(product);
await _unitOfWork.SaveChangesAsync();

// 3. Get full URL when needed
var fullUrl = _fileUploadService.GetFileUrl(product.ImageUrl);
// Returns: "http://localhost:8080/images/products/abc123.jpg"
```

### Example 3: Delete Image

```csharp
// Delete from file system
await _fileUploadService.DeleteImageAsync(product.ImageUrl);

// Delete from database
product.ImageUrl = null;
await _unitOfWork.SaveChangesAsync();
```

---

## 🌐 Production Deployment (Railway)

### Current Setup ⚠️

The current setup works for local development and testing. However, Railway uses **ephemeral file systems**, meaning uploaded files may be deleted on redeploy.

### For Production, Choose One:

#### Option 1: Railway Volumes (Persistent Storage)

1. Go to Railway dashboard
2. Add a Volume to your service
3. Mount path: `/app/src/ZiyoMarket.Api/wwwroot`
4. Files will persist across redeploys

#### Option 2: Cloud Storage (Recommended) ☁️

Integrate with cloud provider:
- **AWS S3** - Scalable object storage
- **Cloudinary** - Image CDN with transformations
- **Azure Blob Storage** - Microsoft cloud storage
- **Google Cloud Storage** - Google cloud storage

### Cloud Migration Steps

1. Install cloud provider SDK
2. Create new service implementation:
   ```csharp
   public class CloudinaryFileUploadService : IFileUploadService
   {
       // Implements upload to Cloudinary
   }
   ```
3. Register based on environment:
   ```csharp
   if (app.Environment.IsProduction())
   {
       builder.Services.AddScoped<IFileUploadService, CloudinaryFileUploadService>();
   }
   else
   {
       builder.Services.AddScoped<IFileUploadService, FileUploadService>();
   }
   ```

---

## 🔒 Security Features

1. **File Size Limit** - 5MB maximum (configurable)
2. **Extension Whitelist** - Only allowed image types
3. **Content Type Validation** - Checks MIME type
4. **Authentication Required** - All uploads need JWT token
5. **Unique Filenames** - GUID-based to prevent conflicts
6. **Git Ignore** - Uploads not committed to repository

---

## 📊 Validation Rules

| Rule | Value | Configurable |
|------|-------|--------------|
| Max File Size | 5 MB | ✅ Yes (appsettings.json) |
| Allowed Extensions | .jpg, .jpeg, .png, .gif, .webp, .svg | ✅ Yes |
| Content Types | image/jpeg, image/png, etc. | ❌ Hardcoded in service |
| Authentication | Required | ❌ Hardcoded in controller |

---

## 🧪 Testing Checklist

- [x] wwwroot folder structure created
- [x] Service and interface implemented
- [x] Controller with endpoints created
- [x] Configuration added to appsettings.json
- [x] Program.cs updated (UseStaticFiles, services)
- [x] .gitignore updated (uploads excluded)
- [x] Project builds successfully
- [ ] Test upload product image
- [ ] Test upload category image
- [ ] Test upload banner image
- [ ] Test upload user avatar
- [ ] Test delete image
- [ ] Test file size validation (upload >5MB)
- [ ] Test extension validation (upload .exe)
- [ ] Access uploaded image in browser
- [ ] Test authentication (upload without token)

---

## 📚 Related Documentation

- **`FILE_UPLOAD_GUIDE.md`** - Detailed usage guide with examples
- **`CLAUDE.md`** - Project architecture and patterns
- **`README.md`** - Project overview

---

## 🎓 Key Implementation Details

### 1. Clean Architecture

File upload feature follows the project's clean architecture:
- **Service Layer** - Business logic (FileUploadService)
- **API Layer** - Controllers and DTOs
- **Configuration** - Settings pattern

### 2. Dependency Injection

All services use DI:
```csharp
public FileUploadService(
    IWebHostEnvironment webHostEnvironment,
    IOptions<FileUploadSettings> settings,
    IHttpContextAccessor httpContextAccessor)
```

### 3. Dynamic URL Generation

Service automatically generates correct URLs based on environment:
- **Local:** `http://localhost:8080/images/...`
- **Production:** `https://your-app.up.railway.app/images/...`

### 4. Entity Integration

Entities store relative paths in database:
- **Stored:** `"images/products/abc123.jpg"`
- **Displayed:** `"http://localhost:8080/images/products/abc123.jpg"`

This allows:
- Easy migration between environments
- Changing domain without database updates
- Future cloud storage migration

---

## ✅ Success Indicators

You'll know it's working when:

1. ✅ API builds without errors
2. ✅ Swagger shows /api/file_upload endpoints
3. ✅ Upload returns file_url in response
4. ✅ Opening file_url in browser shows the image
5. ✅ wwwroot/images/ folders contain uploaded files
6. ✅ Database entities have image paths saved

---

## 🐛 Common Issues & Solutions

### Issue: Static files not accessible

**Solution:** Ensure `app.UseStaticFiles()` is called before `app.UseAuthorization()` in Program.cs

### Issue: Service not registered

**Solution:** Check Program.cs has:
```csharp
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddHttpContextAccessor();
```

### Issue: 413 Payload Too Large

**Solution:** Increase request body size limit in Program.cs

### Issue: CORS error from frontend

**Solution:** Add CORS policy in Program.cs

---

## 🔄 Next Steps

1. ✅ Test all endpoints in Swagger
2. ✅ Integrate with Product/Category creation endpoints
3. ✅ Add image thumbnails generation (optional)
4. ✅ Setup Railway Volume or cloud storage for production
5. ✅ Add image optimization/compression (optional)
6. ✅ Coordinate with Flutter team for mobile integration

---

## 📞 Support

For questions or issues:
1. Check `FILE_UPLOAD_GUIDE.md` for detailed examples
2. Review Swagger documentation at `/swagger`
3. Check logs in `Logs/ziyomarket-log.txt`

---

**Status:** ✅ **IMPLEMENTATION COMPLETE & BUILD SUCCESSFUL**
**Date:** 2026-02-08
**Version:** 1.0.0
