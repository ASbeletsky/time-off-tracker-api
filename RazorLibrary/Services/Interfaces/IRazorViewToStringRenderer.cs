using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;

namespace RazorLibrary.Services.Interfaces
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
