using HtmlAgilityPack;
using IPrazo.Crowler.Models;

namespace IPrazo.Crowler.Services;

public static class ProxyParser
{
    public static List<Proxy> ParseProxyTable(string html, int pageNumber)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-hover']");
        if (table == null)
        {
            Console.WriteLine("Tabela não encontrada!");
            return new List<Proxy>();
        }

        var rows = table.SelectNodes(".//tbody/tr");
        if (rows == null)
        {
            Console.WriteLine("Nenhuma linha encontrada na tabela!");
            return new List<Proxy>();
        }

        var result = new List<Proxy>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("./td");
            if (cells == null || cells.Count < 8)
                continue;

            try
            {
                // IP Address (célula 1)
                var ipCell = cells[1];
                var ipLink = ipCell.SelectSingleNode(".//a");
                string ip = ipLink?.InnerText?.Trim() ?? ipCell.InnerText.Trim();

                // Port (célula 2) - do data-port
                var portCell = cells[2];
                var portSpan = portCell.SelectSingleNode(".//span[@class='port']");
                string port = portSpan?.GetAttributeValue("data-port", "") ?? "80";

                // Country (célula 3) - texto após a imagem
                var countryCell = cells[3];
                string country = System.Net.WebUtility.HtmlDecode(countryCell.InnerText).Trim();

                // Protocol (célula 6)
                var protocolCell = cells[6];
                string protocol = protocolCell.InnerText.Trim();

                if (!string.IsNullOrWhiteSpace(ip))
                {
                    result.Add(new Proxy
                    {
                        IpAddress = ip,
                        Port = port,
                        Protocol = protocol,
                        Country = country,
                        PageNumber = pageNumber
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar linha: {ex.Message}");
            }
        }

        return result;
    }

    public static int GetTotalPages(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var pagination = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'pagination')]//a");
        if (pagination != null)
        {
            var pageNumbers = new List<int>();
            foreach (var pageLink in pagination)
            {
                if (int.TryParse(pageLink.InnerText.Trim(), out int page))
                {
                    pageNumbers.Add(page);
                }
            }
            return pageNumbers.Any() ? pageNumbers.Max() : 1;
        }

        return 1;
    }
}