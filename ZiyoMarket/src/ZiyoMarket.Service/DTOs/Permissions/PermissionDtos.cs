namespace ZiyoMarket.Service.DTOs.Permissions;

/// <summary>
/// Permission response DTO - Frontend uchun
/// </summary>
public class PermissionResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Module nomi (Frontend "Module" kutadi, backend "Group" ishlatadi)
    /// </summary>
    public string Module { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Module display name (O'zbek tilida)
    /// </summary>
    public string ModuleDisplay => GetModuleDisplay(Module);

    private string GetModuleDisplay(string module)
    {
        return module switch
        {
            "Product" or "Products" => "Mahsulotlar",
            "Order" or "Orders" => "Buyurtmalar",
            "Customer" or "Customers" => "Mijozlar",
            "Seller" or "Sellers" => "Sotuvchilar",
            "Category" or "Categories" => "Kategoriyalar",
            "Delivery" => "Yetkazib berish",
            "Report" or "Reports" => "Hisobotlar",
            "Settings" => "Sozlamalar",
            "Admin" or "Admins" => "Adminlar",
            "Support" => "Qo'llab-quvvatlash",
            "User" or "Users" => "Foydalanuvchilar",
            "Content" => "Kontent",
            "Notification" or "Notifications" => "Xabarnomalar",
            "Cashback" => "Cashback",
            _ => module
        };
    }
}

/// <summary>
/// Yangi permission yaratish uchun DTO
/// </summary>
public class CreatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

/// <summary>
/// Permission yangilash uchun DTO
/// </summary>
public class UpdatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Permissionlarni biriktirish uchun DTO
/// </summary>
public class AssignPermissionsDto
{
    public int AdminId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
