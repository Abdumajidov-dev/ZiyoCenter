using System.Text.Json;

namespace ZiyoMarket.Api.Helpers;

/// <summary>
/// Snake case naming policy for JSON serialization
/// Converts PascalCase/camelCase to snake_case (e.g., FirstName -> first_name)
/// Special case: 'Data' -> 'date' (not 'data')
/// </summary>
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Removed special case - 'Data' will now correctly convert to 'data'
        // if (name == "Data")
        //     return "date";

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(name[0]));

        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
