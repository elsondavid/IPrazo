using IPrazo.Crowler.Models;

namespace IPrazo.Crowler.Services;

public class FileStorageService
{
    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = "Outputs";
        Directory.CreateDirectory(Path.Combine(_basePath, "Html"));
        Directory.CreateDirectory(Path.Combine(_basePath, "Json"));
    }

    public async Task SaveHtmlAsync(string html, int pageNumber)
    {
        var filePath = Path.Combine(_basePath, "Html", $"page_{pageNumber}.html");
        await File.WriteAllTextAsync(filePath, html);
    }

    public async Task<string> SaveJsonAsync(List<Proxy> proxies)
    {
        var fileName = $"proxies_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_basePath, "Json", fileName);
        
        var json = System.Text.Json.JsonSerializer.Serialize(proxies, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }
}