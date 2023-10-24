using DbNetSuiteCoreApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCoreApp.Pages.Samples
{
    public class MenuModel : SampleModel
    {
        public MenuModel(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
        }
    }
}
