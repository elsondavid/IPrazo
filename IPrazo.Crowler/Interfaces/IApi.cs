using Refit;

namespace IPrazo.Crowler.Interfaces;

public interface IApi
{
    [Get("/proxy/list/order/updated/order_dir/desc/page/{pageNumber}")]
    Task<string> GetHtmlAsync(int pageNumber);

    [Get("/proxy/list/order/updated/order_dir/desc")]
    Task<string> GetFirstPageAsync();
}