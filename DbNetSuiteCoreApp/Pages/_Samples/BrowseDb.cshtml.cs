﻿using DbNetSuiteCoreApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DbNetSuiteCoreApp.Pages.Samples
{
    public class BrowseDbModel : SampleModel
    {
        public BrowseDbModel(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        { 
        }
    }
}
