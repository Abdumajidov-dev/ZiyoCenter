using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connectionString = "Server=localhost;Database=ZiyoNoorDb;User Id=postgres;Password=2001;Port=5432;";

        // BCrypt hash for "Admin@123"
        string password = "Admin@123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 12);

        Console.WriteLine("Creating admin user...");
        Console.WriteLine($"Username: admin");
        Console.WriteLine($"Phone: +998901234567");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Password Hash: {hash}");
        Console.WriteLine();

        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string sql = @"
                INSERT INTO ""Admins""
                (""FirstName"", ""LastName"", ""Username"", ""Phone"", ""PasswordHash"", ""Role"", ""IsActive"", ""CreatedAt"")
                VALUES
                (@FirstName, @LastName, @Username, @Phone, @PasswordHash, @Role, @IsActive, @CreatedAt)
                RETURNING ""Id"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("FirstName", "Test");
            cmd.Parameters.AddWithValue("LastName", "Admin");
            cmd.Parameters.AddWithValue("Username", "testadmin");
            cmd.Parameters.AddWithValue("Phone", "+998901234568");
            cmd.Parameters.AddWithValue("PasswordHash", hash);
            cmd.Parameters.AddWithValue("Role", "SuperAdmin");
            cmd.Parameters.AddWithValue("IsActive", true);
            cmd.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);

            var result = cmd.ExecuteScalar();

            if (result != null)
            {
                Console.WriteLine($"✅ Admin user created successfully with ID: {result}");
            }
            else
            {
                Console.WriteLine("ℹ️ Admin user already exists (conflict on username)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
