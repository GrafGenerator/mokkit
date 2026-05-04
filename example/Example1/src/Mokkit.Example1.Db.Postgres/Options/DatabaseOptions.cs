namespace Mokkit.Example1.Db.Postgres.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string Primary { get; set; } = "Host=localhost;Port=5432;Database=example1;Username=postgres;Password=postgres;";
}