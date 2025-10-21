namespace IPrazo.Crowler.Models;

public class CrawlExecution
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PagesProcessed { get; set; }
    public int TotalRowsExtracted { get; set; }
    public string JsonFilePath { get; set; } = "";
}