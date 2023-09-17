using DbNetSuiteCoreApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCoreApp.Pages.Samples.DbNetGrid
{
    public class LocalizationModel : SampleModel
    {
        public LocalizationModel(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
        }
    }
}
