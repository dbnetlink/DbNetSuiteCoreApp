﻿using DbNetSuiteCoreApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCoreApp.Pages.Samples
{
    public class FontModel : SampleModel
    {
        public FontModel(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        { 
        }
    }
}
