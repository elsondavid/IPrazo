using IPrazo.Crowler.Data;
using IPrazo.Crowler.Interfaces;
using IPrazo.Crowler.Models;
using IPrazo.Crowler.Services;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace IPrazo.Crowler;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Iniciando Web Crawler...");
        var startTime = DateTime.Now;

        // Configuração de serviços
        var services = new ServiceCollection();
        services.AddRefitClient<IApi>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri("https://proxyservers.pro");
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

        services.AddSingleton<FileStorageService>();

        var serviceProvider = services.BuildServiceProvider();
        var api = serviceProvider.GetRequiredService<IApi>();
        var fileStorage = serviceProvider.GetRequiredService<FileStorageService>();

        try
        {
            // Descobrir total de páginas
            Console.WriteLine("Detectando total de páginas...");
            var firstPageHtml = await api.GetFirstPageAsync();
            var totalPages = ProxyParser.GetTotalPages(firstPageHtml);
            Console.WriteLine($"Total de páginas: {totalPages}");

            var allProxies = new List<Proxy>();
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(3, 3); // 3 threads simultâneas

            for (int page = 1; page <= totalPages; page++)
            {
                await semaphore.WaitAsync();
                tasks.Add(ProcessPageAsync(api, fileStorage, page, allProxies, semaphore));
            }

            await Task.WhenAll(tasks);

            var endTime = DateTime.Now;

            // Salvar JSON
            Console.WriteLine("Salvando dados em JSON...");
            var jsonFilePath = await fileStorage.SaveJsonAsync(allProxies);

            //  Mostrar caminho completo do JSON
            Console.WriteLine($"Caminho completo do JSON: {Path.GetFullPath(jsonFilePath)}");

            // Salvar metadados no banco
            Console.WriteLine("Salvando metadados no banco...");
            await SaveExecutionMetadataAsync(startTime, endTime, totalPages, allProxies.Count, jsonFilePath);

            // Relatório final
            PrintReport(startTime, endTime, totalPages, allProxies.Count, jsonFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
        }

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }

    private static async Task ProcessPageAsync(IApi api, FileStorageService fileStorage, int pageNumber, List<Proxy> allProxies, SemaphoreSlim semaphore)
    {
        try
        {
            Console.WriteLine($"Processando página {pageNumber}...");

            var html = await api.GetHtmlAsync(pageNumber);
            await fileStorage.SaveHtmlAsync(html, pageNumber);

            var proxies = ProxyParser.ParseProxyTable(html, pageNumber);

            lock (allProxies)
            {
                allProxies.AddRange(proxies);
            }

            Console.WriteLine($"Página {pageNumber}: {proxies.Count} proxies");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na página {pageNumber}: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static async Task SaveExecutionMetadataAsync(DateTime start, DateTime end, int pages, int totalRows, string jsonPath)
    {
        using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();

        db.Executions.Add(new CrawlExecution
        {
            StartTime = start,
            EndTime = end,
            PagesProcessed = pages,
            TotalRowsExtracted = totalRows,
            JsonFilePath = jsonPath
        });

        await db.SaveChangesAsync();
    }

    private static void PrintReport(DateTime start, DateTime end, int pages, int totalProxies, string jsonPath)
    {
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("RELATÓRIO FINAL");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($"Tempo total: {(end - start).TotalSeconds:F2}s");
        Console.WriteLine($"Páginas processadas: {pages}");
        Console.WriteLine($"Proxies extraídos: {totalProxies}");
        Console.WriteLine($"Arquivo JSON: {jsonPath}");
        Console.WriteLine($"Caminho absoluto: {Path.GetFullPath(jsonPath)}");
        Console.WriteLine($"Banco: crawler.db");
        Console.WriteLine($"HTMLs: Outputs/Html/");

        if (totalProxies > 0)
        {
            Console.WriteLine($"\nTecnologias: Refit + EF + SQLite + HtmlAgilityPack");
        }
    }
}