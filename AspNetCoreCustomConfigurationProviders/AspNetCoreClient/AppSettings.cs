public class AppSettings
{
    public string? Message{ get; set; }
    public string? BackgroundColor { get; set; }
    public string? FontSize { get; set; }
    public string? FontColor { get; set; }
    
}

public class SwaggerOptions
{
    public string? ApiName { get; set; }
    public string? ApiVersion { get; set; }
    public string? ClientId { get; set; }
    public string? Authority { get; set; }
    public bool? EnableSwagger { get; set; } 
}
