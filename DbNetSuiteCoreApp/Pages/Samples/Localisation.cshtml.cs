using DbNetSuiteCoreApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCoreApp.Pages.Samples
{
    public class LocalizationModel : SampleModel
    {
        public LocalizationModel(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        { 
        }
    }
}
