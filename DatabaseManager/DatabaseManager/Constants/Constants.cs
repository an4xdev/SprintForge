namespace DatabaseManager.Constants;
public static class Constants
{
    public static string FilePath => "Data/data.sql";
    
    public static List<string> SeedFiles => new()
    {
        "Data/procedures.sql",
        "Data/data.sql"
    };
}