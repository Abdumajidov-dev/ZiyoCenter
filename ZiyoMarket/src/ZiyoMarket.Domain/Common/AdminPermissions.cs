namespace ZiyoMarket.Domain.Common;

/// <summary>
/// Admin ruxsatlari katalogi. Ro'yxat kodda belgilanadi, chunki har bir ruxsat aniq endpointlarni
/// ([RequirePermission]) yopadi — DB'dan qo'shilgan ruxsat hech narsani himoya qilmas edi.
/// Adminga biriktirilganlari Admin.Permissions ustunida vergul bilan saqlanadi.
/// Id'lar admin panel uchun barqaror: yangi ruxsat faqat ro'yxat oxiriga qo'shilsin.
/// </summary>
public static class AdminPermissions
{
    public const string ProductsManage = "products.manage";
    public const string ProductsPrices = "products.prices";
    public const string CategoriesManage = "categories.manage";
    public const string OrdersView = "orders.view";
    public const string OrdersManage = "orders.manage";
    public const string OrdersPayments = "orders.payments";
    public const string CustomersView = "customers.view";
    public const string CustomersManage = "customers.manage";
    public const string SellersView = "sellers.view";
    public const string SellersManage = "sellers.manage";
    public const string DeliveryManage = "delivery.manage";
    public const string ReportsView = "reports.view";
    public const string SupportManage = "support.manage";
    public const string ContentManage = "content.manage";
    public const string NotificationsSend = "notifications.send";
    public const string FinanceExpenses = "finance.expenses";
    public const string FinancePaymentCards = "finance.payment_cards";

    public sealed record Definition(int Id, string Code, string Module, string DisplayName, string Description);

    public static readonly IReadOnlyList<Definition> All = new List<Definition>
    {
        new(1, ProductsManage, "Products", "Mahsulotlarni boshqarish", "Mahsulot yaratish, tahrirlash, o'chirish va ombor qoldig'i"),
        new(2, ProductsPrices, "Products", "Narxlarni ommaviy o'zgartirish", "Kategoriya bo'yicha narxni foizga oshirish"),
        new(3, CategoriesManage, "Categories", "Kategoriyalarni boshqarish", "Kategoriya yaratish, tahrirlash, tartiblash va o'chirish"),
        new(4, OrdersView, "Orders", "Buyurtmalarni ko'rish", "Buyurtmalar ro'yxati va tafsilotlari"),
        new(5, OrdersManage, "Orders", "Buyurtmalarni boshqarish", "Status o'zgartirish, bekor qilish va o'chirish"),
        new(6, OrdersPayments, "Orders", "To'lovlarni tasdiqlash", "Mijoz yuborgan to'lov chekini tasdiqlash yoki rad etish"),
        new(7, CustomersView, "Customers", "Mijozlarni ko'rish", "Mijozlar ro'yxati, tafsilotlari va buyurtmalari"),
        new(8, CustomersManage, "Customers", "Mijozlarni boshqarish", "Mijozni yaratish, tahrirlash, bloklash va o'chirish"),
        new(9, SellersView, "Sellers", "Sotuvchilarni ko'rish", "Sotuvchilar ro'yxati va samaradorligi"),
        new(10, SellersManage, "Sellers", "Sotuvchilarni boshqarish", "Sotuvchini tahrirlash, rolini o'zgartirish, bloklash va o'chirish"),
        new(11, DeliveryManage, "Delivery", "Yetkazib berishni boshqarish", "Hamkorlar va yetkazib berish statuslari"),
        new(12, ReportsView, "Reports", "Hisobotlarni ko'rish", "Savdo, mahsulot, mijoz va moliyaviy hisobotlar"),
        new(13, SupportManage, "Support", "Qo'llab-quvvatlash", "Chatlarga javob berish, biriktirish va yopish"),
        new(14, ContentManage, "Content", "Kontentni boshqarish", "Bannerlar, yangiliklar va FAQ"),
        new(15, NotificationsSend, "Notifications", "Bildirishnoma yuborish", "Foydalanuvchilarga bildirishnoma yuborish"),
        new(16, FinanceExpenses, "Finance", "Xarajatlar", "Do'kon xarajatlarini kiritish va ko'rish"),
        new(17, FinancePaymentCards, "Finance", "To'lov kartalari", "Mijozlar pul o'tkazadigan kartani o'zgartirish"),
    };

    public static bool IsKnown(string code) => All.Any(p => p.Code == code);

    public static string? CodeById(int id) => All.FirstOrDefault(p => p.Id == id)?.Code;
}
