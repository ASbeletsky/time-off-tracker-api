using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;

namespace EmailTemplateRender.Services.Interfaces
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}
