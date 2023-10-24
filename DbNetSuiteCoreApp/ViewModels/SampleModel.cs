using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using DbNetSuiteCore.Enums;
using System.Globalization;
using System.Threading;
using DbNetSuiteCore.Utilities;
using Microsoft.AspNetCore.Html;
using DbNetSuiteCoreApp.Enums;
using System;
using Microsoft.AspNetCore.Routing;
using System.IO;
using DbNetSuiteCoreApp.Pages.Samples.DbNetGrid;
using DbNetSuiteCoreApp.Pages.Samples;

namespace DbNetSuiteCoreApp.ViewModels
{
    public class SampleModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SampleModel(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public Dictionary<Components, List<SampleApp>> Samples { get; set; } = new Dictionary<Components, List<SampleApp>>();
        public SampleApp SampleApp { get; set; }
        public SampleApp NextSampleApp { get; set; }
        public SampleApp PreviousSampleApp { get; set; }
        public Components Component { get; set; }

        public string Title { get; set; }
        public string SourceCode { get; set; }

        public string PageName { get; set; }
        public string FontFamily { get; set; }
        public string FontSize { get; set; }
        public ToolbarButtonStyle ToolbarButtonStyle { get; set; } = ToolbarButtonStyle.Image;
        public ToolbarPosition ToolbarLocation { get; set; } = ToolbarPosition.Top;
        public MultiRowSelectLocation MultiRowSelectLocation { get; set; } = MultiRowSelectLocation.Left;
        public List<string> FontSizes { get; set; } = new List<string>() { "Small", "Medium", "Large" };
        public List<string> FontFamilies { get; set; } = new List<string>() { "Verdana", "Tahoma", "Georgia", "Arial" };
        public string Db { get; set; }
        public string Table { get; set; }
        public string View { get; set; }
        public List<DbObject> Tables { get; set; } = new List<DbObject>();
        public List<DbObject> Views { get; set; } = new List<DbObject>();
        public List<string> Connections { get; set; } = new List<string>();
        public List<CultureInfo> Cultures { get; set; } = new List<CultureInfo>();
        public string Culture { get; set; } = null;
        public string CustomerId { get; set; } = null;
        public int? OrderId { get; set; } = null;
        public bool ShowMenu { get; set; } = false;

        public void OnGet(
            string fontFamily = "verdana",
            string fontSize = "small",
            string db = null,
            string table = null,
            string view = null,
            ToolbarButtonStyle? toolbarButtonStyle = null,
            ToolbarPosition? toolbarLocation = null,
            MultiRowSelectLocation? multiRowSelectLocation = null,
            string culture = null,
            string customerId = null,
            int? orderId = null

          )
        {
            if (this is BrowseDbModel)
            {
                Table = table;
                View = view;
                BrowseDbPopulate(db);
            }

            if (this is FontModel)
            {
                FontFamily = fontFamily;
                FontSize = fontSize;
            }

            if (this is LocalizationModel)
            {
                Culture = culture ?? Thread.CurrentThread.CurrentCulture.Name;
                GetCultures();
            }

            PopulateSamples();

            if (this is MenuModel)
            {
                ShowMenu = true;
            }
            else
            {
                SampleNavigation();
            }

            if (toolbarButtonStyle.HasValue)
            {
                ToolbarButtonStyle = toolbarButtonStyle.Value;
            }
            if (toolbarLocation.HasValue)
            {
                ToolbarLocation = toolbarLocation.Value;
            }
            if (multiRowSelectLocation.HasValue)
            {
                MultiRowSelectLocation = multiRowSelectLocation.Value;
            }

            CustomerId = customerId;
            OrderId = orderId;

            try
            {
                GetSourceCode();
            }
            catch (Exception)
            {
                SourceCode = string.Empty;
            }
        }

        private void SampleNavigation()
        {
            var segments = HttpContext.Request.Path.ToString().Split("/");
            Component = (Components)Enum.Parse(typeof(Components), segments[^2], true);
            PageName = $"{Component}/{segments.Last()}";

            var samples = new List<SampleApp>();

            foreach (var key in Samples.Keys)
            {
                samples.AddRange(Samples[key]);
            }

            SampleApp = samples.FirstOrDefault(s => s.Url.ToLower() == PageName.ToLower());
            SampleApp ??= samples.First();

            Title = $"{SampleApp.Component} - {SampleApp.Title}";

            int sampleIndex = samples.FindIndex(app => app.Url.Equals(SampleApp.Url, StringComparison.Ordinal));
            PreviousSampleApp = (sampleIndex > 0) ? samples[sampleIndex - 1] : samples.Last();
            NextSampleApp = (sampleIndex < samples.Count - 1) ? samples[sampleIndex + 1] : samples.First();
        }

        private void GetSourceCode()
        {
            var routeName = (HttpContext.GetEndpoint() as RouteEndpoint).RoutePattern.RawText;
            var fileInfo = _webHostEnvironment.ContentRootFileProvider.GetFileInfo($"pages/{routeName}.cshtml");
            string fileContents = string.Empty;
            using (var sr = new StreamReader(fileInfo.PhysicalPath))
            {
                fileContents = sr.ReadToEnd();
            }

            var fileLines = fileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var collect = false;
            var source = new List<string>();
            foreach (string line in fileLines)
            {
                if (collect)
                {
                    if (line.StartsWith("}"))
                    {
                        break;
                    }
                    source.Add(line);
                }
                if (line.StartsWith("@section Control"))
                {
                    collect = true;
                }
            }
            if (source.Any())
            {
                if (source.First() == "{")
                {
                    source.Remove(source.First());
                }
            }
            if (source.Any())
            {
                if (source.Last() == "}")
                {
                    source.Remove(source.Last());
                }
            }
            source.Insert(0, string.Empty);
            source = source.Select(s => s.TrimStart()).ToList();
            int indent = 0;
            var indentedSource = new List<string>();
            foreach (string line in source)
            {
                if (line.StartsWith("}"))
                {
                    indent--;
                }
                string indentedLine = string.Empty;
                for (var i = 0; i < indent; i++)
                {
                    indentedLine += '\t';
                }
                indentedLine += line;
                indentedSource.Add(indentedLine);
                if (line.StartsWith("@{") || line.StartsWith("{"))
                {
                    indent++;
                }
            }
            SourceCode = string.Join(Environment.NewLine, indentedSource);
        }
        private void PopulateSamples()
        {
            Samples.Add(Components.DbNetGrid, DbNetGridSamples());
            Samples.Add(Components.DbNetCombo, DbNetComboSamples());
            Samples.Add(Components.DbNetEdit, DbNetEditSamples());
        }
        private List<SampleApp> DbNetGridSamples()
        {
            List<SampleApp> samples = new List<SampleApp>
            {
                new SampleApp("simple", "Simple"),
                new SampleApp("columns", "Specifying Columns"),
                new SampleApp("joiningtables", "Joining Tables"),
                new SampleApp("style", "Column styling"),
                new SampleApp("edit", "Editing The Grid"),
                new SampleApp("linkededit", "Linked Edit"),
                new SampleApp("multirowselect", "Selecting Multiple Rows"),
                new SampleApp("nestedgrid", "Nesting Grids"),
                new SampleApp("linkedgrids", "Linking Grids"),
                new SampleApp("quicksearch", "Quick Search"),
                new SampleApp("columnfilters", "Column Filters"),
                new SampleApp("toolbarPosition", "Positioning/Styling Toolbar"),
                new SampleApp("frozenHeader", "Freezing the headings"),
                new SampleApp("master", "Linking to grids in another page"),
                new SampleApp("groupHeader", "Group headings"),
                new SampleApp("comparingCells", "Comparing cell values"),
                new SampleApp("cellTransformation", "Cell value transformation"),
                new SampleApp("binaryData", "Handling binary data"),
                new SampleApp("viewingRecord", "Viewing records"),
                new SampleApp("font", "Changing the font size/family"),
                new SampleApp("totals", "Adding totals"),
                new SampleApp("subtotals", "Adding sub-totals"),
                new SampleApp("groupBy", "Aggregating data"),
                new SampleApp("crosstab", "Cross tabulation"),
                new SampleApp("localisation", "Localisation"),
                new SampleApp("groupByChart", "Chart integration"),
                new SampleApp("MultiSeriesChart", "Multi-series Chart"),
                new SampleApp("Dashboard", "Dashboard"),
                new SampleApp("Css", "Customising the styling"),

                new SampleApp("browsedb", "Browse a database")
            };
            return samples;
        }

        private List<SampleApp> DbNetComboSamples()
        {
            List<SampleApp> samples = new List<SampleApp>
            {
                new DbNetComboSampleApp("simplecombo", "Simple"),
                new DbNetComboSampleApp("filtered", "Filtered"),
                new DbNetComboSampleApp("linked", "Linked"),
                new DbNetComboSampleApp("multiple", "Size/Multiple"),
                new DbNetComboSampleApp("linkedgrid", "Linked Grid"),
                new DbNetComboSampleApp("datacolumns", "Data Columns"),
                new DbNetComboSampleApp("distinct", "Distinct"),
                new DbNetComboSampleApp("linkededit", "Linked Edit")
           };

            return samples;
        }

        private List<SampleApp> DbNetEditSamples()
        {
            List<SampleApp> samples = new List<SampleApp>
            {
                new DbNetEditSampleApp("simple", "Simple"),
                new DbNetEditSampleApp("columns", "Columns"),
                new DbNetEditSampleApp("layout", "Layout"),
                new DbNetEditSampleApp("browse", "Browse"),
                new DbNetEditSampleApp("fixedfilter", "Pre-filtering the dataset"),
                new DbNetEditSampleApp("toolbarPosition", "Positioning/Styling Toolbar"),
                new DbNetEditSampleApp("quicksearch", "Quick Search"),
                new DbNetEditSampleApp("linkededits", "Linked Forms"),
                new DbNetEditSampleApp("linkedgrid", "Linked Grid"),
                new DbNetEditSampleApp("binarydata", "Uploading files"),
                new DbNetEditSampleApp("dependentlookup", "Dependent lookups"),
                new DbNetEditSampleApp("products", "Conditional configuration"),
                new DbNetEditSampleApp("controltypes", "Control Types")
         };

            return samples;
        }
        public void BrowseDbPopulate(string db = null)
        {
            var connectonStrings = _configuration.GetSection("ConnectionStrings").GetChildren();
            connectonStrings = connectonStrings.AsEnumerable().Where(c => FilterConnectionString(_configuration.GetConnectionString(c.Key))).ToList();

            var connectionAlias = db ?? connectonStrings.AsEnumerable().First().Key;
            var connectionString = _configuration.GetConnectionString(connectionAlias);

            Connections = connectonStrings.AsEnumerable().Select(c => c.Key).ToList();

            using (var connection = new DbNetDataCore(connectionString, _webHostEnvironment))
            {
                connection.Open();
                Tables = connection.InformationSchema(DbNetDataCore.MetaDataType.Tables);
                Views = connection.InformationSchema(DbNetDataCore.MetaDataType.Views);
            }

            Db = connectionAlias;

            if (string.IsNullOrEmpty(Table) && string.IsNullOrEmpty(View))
            {
                Table = Tables.First().QualifiedTableName;
            }
        }

        public HtmlString HelpLink(string url, string text)
        {
            url = $"https://dbnetsuitecore.z35.web.core.windows.net/topics/{url.Replace("_", "-")}";
            return new HtmlString($"<a target=\"_blank\" href=\"{url}\">{text}</a>");
        }

        private bool FilterConnectionString(string path)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                return true;
            }

            return IsSqliteConnectionString(path);
        }

        private bool IsSqliteConnectionString(string path)
        {
            return Regex.IsMatch(path, @"Data Source=(.*)\.db;", RegexOptions.IgnoreCase);
        }

        private void GetCultures()
        {
            List<string> cultures = new List<string>() { "ar-QA", "en-GB", "en-US", "es-ES", "fr-FR", "ja-JP", "it-IT" };

            if (cultures.Contains(Culture) == false)
            {
                cultures.Add(Culture);
            }
            Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => cultures.Contains(c.Name)).OrderBy(c => c.DisplayName).ToList();
        }
    }

    public class SampleApp
    {
        public Components Component { get; set; }
        public string Page { get; set; }
        public string Title { get; set; }
        public string Url => $"{Component}/{Page}";

        public SampleApp(string page, string title, Components component = Components.DbNetGrid)
        {
            Component = component;
            Page = page;
            Title = title;
        }
    
    }
    public class DbNetEditSampleApp : SampleApp
    {
        public DbNetEditSampleApp(string page, string title) : base(page, title, Components.DbNetEdit)
        {
        }
    }
    public class DbNetComboSampleApp : SampleApp
    {
        public DbNetComboSampleApp(string page, string title) : base(page, title, Components.DbNetCombo)
        {
        }
    }
}