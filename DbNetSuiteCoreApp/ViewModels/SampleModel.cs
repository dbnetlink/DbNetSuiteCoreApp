using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Hosting;
using DbNetSuiteCoreApp.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using DbNetSuiteCoreApp.Pages;
using DbNetSuiteCore.Enums;
using DbNetSuiteCoreApp.Pages.Samples;
using System.Globalization;
using System.Reflection.Metadata;
using System.Threading;
using System.Drawing;
using DbNetSuiteCore.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Html;

namespace DbNetSuiteCoreApp.ViewModels
{
    public class SampleModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public SampleModel(IConfiguration configuration, IWebHostEnvironment env) 
        {
            _configuration = configuration;
            _env = env;
            PopulateSamples();
        }

        public List<SampleApp> Samples { get; set; } = new List<SampleApp>();
        public SampleApp SampleApp { get; set; }
        public SampleApp NextSampleApp { get; set; }
        public SampleApp PreviousSampleApp { get; set; }

        public string Title { get; set; }
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

            if (toolbarButtonStyle.HasValue )
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

            PageName = HttpContext.Request.Path.ToString().Split("/").Last();
            SampleApp = Samples.FirstOrDefault(s => s.Page.ToLower() == PageName.ToLower());

            SampleApp ??= Samples.First();

            var sampleIndex = Samples.IndexOf(SampleApp);
            PreviousSampleApp = (sampleIndex > 0) ? Samples[sampleIndex - 1] : Samples.Last();
            NextSampleApp = (sampleIndex < Samples.Count - 1) ? Samples[sampleIndex + 1] : Samples.First();
        }

        private void PopulateSamples()
        {
            Samples.Add(new SampleApp("simple", "Simple"));
            Samples.Add(new SampleApp("columns", "Specifying Columns"));
            Samples.Add(new SampleApp("joiningtables", "Joining Tables"));
            Samples.Add(new SampleApp("style", "Column styling"));
            Samples.Add(new SampleApp("multirowselect", "Selecting Multiple Rows"));
            Samples.Add(new SampleApp("nestedgrid", "Nesting Grids"));
            Samples.Add(new SampleApp("linkedgrid", "Linking Grids"));
            Samples.Add(new SampleApp("quicksearch", "Quick Search"));
            Samples.Add(new SampleApp("columnfilters", "Column Filters"));
            Samples.Add(new SampleApp("toolbarPosition", "Positioning/Styling Toolbar"));
            Samples.Add(new SampleApp("frozenHeader", "Freezing the headings"));
            Samples.Add(new SampleApp("master", "Linking to grids in another page"));
            Samples.Add(new SampleApp("groupHeader", "Group headings"));
            Samples.Add(new SampleApp("comparingCells", "Comparing cell values"));
            Samples.Add(new SampleApp("cellTransformation", "Cell value transformation"));
            Samples.Add(new SampleApp("binaryData", "Handling binary data"));
            Samples.Add(new SampleApp("viewingRecord", "Viewing records"));
            Samples.Add(new SampleApp("font", "Changing the font size/family"));
            Samples.Add(new SampleApp("totals", "Adding totals"));
            Samples.Add(new SampleApp("subtotals", "Adding sub-totals"));
            Samples.Add(new SampleApp("groupBy", "Aggregating data"));
            Samples.Add(new SampleApp("crosstab", "Cross tabulation"));
            Samples.Add(new SampleApp("localisation", "Localisation"));
            Samples.Add(new SampleApp("groupByChart", "Chart integration"));
            Samples.Add(new SampleApp("MultiSeriesChart", "Multi-series Chart"));
            Samples.Add(new SampleApp("Dashboard", "Dashboard"));
            Samples.Add(new SampleApp("Css", "Customising the styling"));

            Samples.Add(new SampleApp("browsedb", "Browse a database"));
        }

        public void BrowseDbPopulate(string db = null)
        {
            var connectonStrings = _configuration.GetSection("ConnectionStrings").GetChildren();
            connectonStrings = connectonStrings.AsEnumerable().Where(c => FilterConnectionString(_configuration.GetConnectionString(c.Key))).ToList();

            var connectionAlias = db ?? connectonStrings.AsEnumerable().First().Key;
            var connectionString = _configuration.GetConnectionString(connectionAlias);

            Connections = connectonStrings.AsEnumerable().Select(c => c.Key).ToList();

            using (var connection = new DbNetDataCore(connectionString, _env))
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
            url = $"https://dbnetsuitecore.z35.web.core.windows.net/topics/{url}";
            return new HtmlString($"<a target=\"_blank\" href=\"{url}\">{text}</a>");
        }

        private bool FilterConnectionString(string path)
        {
            if (_env.IsDevelopment())
            {
                return true;
            }

            return IsSqliteConnectionString(path);
        }

        private bool IsSqliteConnectionString(string path)
        {
            return Regex.IsMatch(path, @"Data Source=(.*)\.db;", RegexOptions.IgnoreCase);
        }

        private string MapDatabasePath(string ConnectionString)
        {
            if (!ConnectionString.EndsWith(";"))
                ConnectionString += ";";

            ConnectionString = Regex.Replace(ConnectionString, @"DataProvider=(.*?);", "", RegexOptions.IgnoreCase);
            string CurrentPath = this._env.WebRootPath;
            string DataSourcePropertyName = "data source";
            ConnectionString = Regex.Replace(ConnectionString, DataSourcePropertyName + "=~", DataSourcePropertyName + "=" + CurrentPath, RegexOptions.IgnoreCase).Replace("=//", "=/");
            return ConnectionString;
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

        private List<DbObject> InformationSchemaQuery(SqlConnection conn, string tableType)
        {
            var sql = $"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='{tableType}' order by TABLE_SCHEMA, TABLE_NAME";
            return conn.Query<InformationSchema>(sql).Select(s => new DbObject() { QualifiedTableName = s.FullQualifiedTableName, TableName = s.FullTableName }).ToList();
        }

        private string SqliteMasterQuery(string tableType)
        {
            return $"SELECT name as QualifiedTableName,name as TableName FROM sqlite_master WHERE type='{tableType}' and name not like 'sqlite_%' order by name";
        }
    }

    public class SampleApp
    {
        public string Page { get; set; }
        public string Title { get; set; }

        public SampleApp(string page, string title)
        {
            Page = page;
            Title = title;
        }
    }
}