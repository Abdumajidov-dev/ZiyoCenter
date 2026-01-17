using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Role va Permission'larni seed qilish uchun service
/// </summary>
public class RolePermissionSeedService
{
    private readonly IUnitOfWork _unitOfWork;

    public RolePermissionSeedService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Barcha role va permissionlarni yaratish
    /// </summary>
    public async Task SeedRolesAndPermissionsAsync()
    {
        // 1. Permission'larni yaratish
        var permissions = await CreatePermissionsAsync();

        // 2. Role'larni yaratish
        var roles = await CreateRolesAsync();

        // 3. Role'larga permission'larni biriktirish
        await AssignPermissionsToRolesAsync(roles, permissions);
    }

    private async Task<Dictionary<string, Permission>> CreatePermissionsAsync()
    {
        var permissions = new Dictionary<string, Permission>();

        var permissionList = new[]
        {
            // Product Permissions
            new { Name = "ViewProducts", Description = "Mahsulotlarni ko'rish", Group = "Product" },
            new { Name = "CreateProduct", Description = "Mahsulot yaratish", Group = "Product" },
            new { Name = "UpdateProduct", Description = "Mahsulotni tahrirlash", Group = "Product" },
            new { Name = "DeleteProduct", Description = "Mahsulotni o'chirish", Group = "Product" },
            new { Name = "ManageProductStock", Description = "Mahsulot zaxirasini boshqarish", Group = "Product" },

            // Order Permissions
            new { Name = "ViewOrders", Description = "Buyurtmalarni ko'rish", Group = "Order" },
            new { Name = "CreateOrder", Description = "Buyurtma yaratish", Group = "Order" },
            new { Name = "UpdateOrderStatus", Description = "Buyurtma statusini o'zgartirish", Group = "Order" },
            new { Name = "CancelOrder", Description = "Buyurtmani bekor qilish", Group = "Order" },
            new { Name = "ViewAllOrders", Description = "Barcha buyurtmalarni ko'rish", Group = "Order" },
            new { Name = "ApplyDiscount", Description = "Chegirma berish", Group = "Order" },

            // User Management Permissions
            new { Name = "ViewUsers", Description = "Foydalanuvchilarni ko'rish", Group = "User" },
            new { Name = "ManageUsers", Description = "Foydalanuvchilarni boshqarish", Group = "User" },
            new { Name = "AssignRoles", Description = "Role biriktirish", Group = "User" },
            new { Name = "ViewRoles", Description = "Role'larni ko'rish", Group = "User" },
            new { Name = "ManageRoles", Description = "Role'larni boshqarish", Group = "User" },

            // Customer Permissions
            new { Name = "ViewOwnOrders", Description = "O'z buyurtmalarini ko'rish", Group = "Customer" },
            new { Name = "ManageCart", Description = "Savatni boshqarish", Group = "Customer" },
            new { Name = "ViewCashback", Description = "Cashback'ni ko'rish", Group = "Customer" },
            new { Name = "UseCashback", Description = "Cashback'dan foydalanish", Group = "Customer" },
            new { Name = "LikeProducts", Description = "Mahsulotlarni yoqtirish", Group = "Customer" },

            // Report Permissions
            new { Name = "ViewReports", Description = "Hisobotlarni ko'rish", Group = "Report" },
            new { Name = "ViewSalesReports", Description = "Sotuv hisobotlarini ko'rish", Group = "Report" },
            new { Name = "ViewFinancialReports", Description = "Moliyaviy hisobotlarni ko'rish", Group = "Report" },
            new { Name = "ExportReports", Description = "Hisobotlarni eksport qilish", Group = "Report" },

            // Settings Permissions
            new { Name = "ViewSettings", Description = "Sozlamalarni ko'rish", Group = "Settings" },
            new { Name = "ManageSettings", Description = "Sozlamalarni boshqarish", Group = "Settings" },

            // Support Permissions
            new { Name = "ViewSupport", Description = "Support chatlarni ko'rish", Group = "Support" },
            new { Name = "RespondToSupport", Description = "Support chatga javob berish", Group = "Support" },

            // Delivery Permissions
            new { Name = "ViewDeliveries", Description = "Yetkazib berishlarni ko'rish", Group = "Delivery" },
            new { Name = "ManageDeliveries", Description = "Yetkazib berishlarni boshqarish", Group = "Delivery" },

            // Content Permissions
            new { Name = "ViewContent", Description = "Kontentlarni ko'rish", Group = "Content" },
            new { Name = "ManageContent", Description = "Kontentlarni boshqarish", Group = "Content" },

            // Notification Permissions
            new { Name = "SendNotifications", Description = "Xabarnoma yuborish", Group = "Notification" },
            new { Name = "ManageNotifications", Description = "Xabarnomalarni boshqarish", Group = "Notification" },
        };

        foreach (var p in permissionList)
        {
            var existing = await _unitOfWork.Permissions.SelectAsync(x => x.Name == p.Name);
            if (existing == null)
            {
                var permission = new Permission
                {
                    Name = p.Name,
                    Description = p.Description,
                    Group = p.Group,
                    IsActive = true
                };
                await _unitOfWork.Permissions.InsertAsync(permission);
                await _unitOfWork.SaveChangesAsync();
                permissions[p.Name] = permission;
            }
            else
            {
                permissions[p.Name] = existing;
            }
        }

        return permissions;
    }

    private async Task<Dictionary<string, Role>> CreateRolesAsync()
    {
        var roles = new Dictionary<string, Role>();

        var roleList = new[]
        {
            new { Name = "Customer", Description = "Oddiy mijoz - mahsulot sotib oladi", IsSystem = true },
            new { Name = "Seller", Description = "Sotuvchi - offline buyurtma yaratadi", IsSystem = true },
            new { Name = "Manager", Description = "Menejer - sotuvchilarni boshqaradi", IsSystem = false },
            new { Name = "Admin", Description = "Administrator - tizimni boshqaradi", IsSystem = true },
            new { Name = "SuperAdmin", Description = "Super Administrator - to'liq huquqlar", IsSystem = true },
        };

        foreach (var r in roleList)
        {
            var existing = await _unitOfWork.Roles.SelectAsync(x => x.Name == r.Name);
            if (existing == null)
            {
                var role = new Role
                {
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = true,
                    IsSystemRole = r.IsSystem
                };
                await _unitOfWork.Roles.InsertAsync(role);
                await _unitOfWork.SaveChangesAsync();
                roles[r.Name] = role;
            }
            else
            {
                roles[r.Name] = existing;
            }
        }

        return roles;
    }

    private async Task AssignPermissionsToRolesAsync(
        Dictionary<string, Role> roles,
        Dictionary<string, Permission> permissions)
    {
        // Customer Role Permissions
        var customerPermissions = new[]
        {
            "ViewProducts", "ViewOwnOrders", "CreateOrder", "CancelOrder",
            "ManageCart", "ViewCashback", "UseCashback", "LikeProducts", "ViewContent"
        };
        await AssignPermissionsToRole(roles["Customer"], customerPermissions, permissions);

        // Seller Role Permissions
        var sellerPermissions = new[]
        {
            "ViewProducts", "ManageProductStock", "ViewOrders", "CreateOrder",
            "UpdateOrderStatus", "ApplyDiscount", "ViewContent"
        };
        await AssignPermissionsToRole(roles["Seller"], sellerPermissions, permissions);

        // Manager Role Permissions (Seller + more)
        var managerPermissions = new[]
        {
            "ViewProducts", "CreateProduct", "UpdateProduct", "ManageProductStock",
            "ViewOrders", "ViewAllOrders", "CreateOrder", "UpdateOrderStatus", "CancelOrder",
            "ApplyDiscount", "ViewUsers", "ViewReports", "ViewSalesReports",
            "ViewSupport", "RespondToSupport", "ViewDeliveries", "ManageDeliveries", "ViewContent"
        };
        await AssignPermissionsToRole(roles["Manager"], managerPermissions, permissions);

        // Admin Role Permissions (Manager + more)
        var adminPermissions = new[]
        {
            "ViewProducts", "CreateProduct", "UpdateProduct", "DeleteProduct", "ManageProductStock",
            "ViewOrders", "ViewAllOrders", "CreateOrder", "UpdateOrderStatus", "CancelOrder", "ApplyDiscount",
            "ViewUsers", "ManageUsers", "ViewRoles", "AssignRoles",
            "ViewReports", "ViewSalesReports", "ViewFinancialReports", "ExportReports",
            "ViewSettings", "ManageSettings",
            "ViewSupport", "RespondToSupport",
            "ViewDeliveries", "ManageDeliveries",
            "ViewContent", "ManageContent",
            "SendNotifications", "ManageNotifications"
        };
        await AssignPermissionsToRole(roles["Admin"], adminPermissions, permissions);

        // SuperAdmin - Barcha permissionlar
        var allPermissions = permissions.Keys.ToArray();
        await AssignPermissionsToRole(roles["SuperAdmin"], allPermissions, permissions);
    }

    private async Task AssignPermissionsToRole(
        Role role,
        string[] permissionNames,
        Dictionary<string, Permission> permissions)
    {
        foreach (var permName in permissionNames)
        {
            if (!permissions.ContainsKey(permName))
                continue;

            var permission = permissions[permName];

            // Check if already assigned
            var existing = await _unitOfWork.RolePermissions.SelectAsync(rp =>
                rp.RoleId == role.Id && rp.PermissionId == permission.Id);

            if (existing == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                };
                await _unitOfWork.RolePermissions.InsertAsync(rolePermission);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
