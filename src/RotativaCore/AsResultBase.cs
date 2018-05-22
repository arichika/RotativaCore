using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RotativaCore.Options;

namespace RotativaCore
{
    public abstract class AsResultBase : ActionResult
    {
        protected AsResultBase()
        {
            WkhtmlPath = string.Empty;
            FormsAuthenticationCookieName = ".AspNetCore.Identity.Application";
        }

        /// <summary>
        /// This will be send to the browser as a name of the generated PDF file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Path to wkhtmltopdf\wkhtmltoimage binary.
        /// </summary>
        public string WkhtmlPath { get; set; }

        /// <summary>
        /// Custom name of authentication cookie used by forms authentication.
        /// </summary>
        [Obsolete("Use FormsAuthenticationCookieName instead of CookieName.")]
        public string CookieName
        {
            get => FormsAuthenticationCookieName;
            set => FormsAuthenticationCookieName = value;
        }

        /// <summary>
        /// Custom name of authentication cookie used by forms authentication.
        /// </summary>
        public string FormsAuthenticationCookieName { get; set; }

        /// <summary>
        /// Sets custom headers.
        /// </summary>
        [OptionFlag("--custom-header")]
        public Dictionary<string, string> CustomHeaders { get; set; }

        /// <summary>
        /// Sets cookies.
        /// </summary>
        [OptionFlag("--cookie")]
        public Dictionary<string, string> Cookies { get; set; }

        /// <summary>
        /// Sets post values.
        /// </summary>
        [OptionFlag("--post")]
        public Dictionary<string, string> Post { get; set; }

        /// <summary>
        /// Indicates whether the page can run JavaScript.
        /// </summary>
        [OptionFlag("-n")]
        public bool IsJavaScriptDisabled { get; set; }

        /// <summary>
        /// Minimum font size.
        /// </summary>
        [OptionFlag("--minimum-font-size")]
        public int? MinimumFontSize { get; set; }

        /// <summary>
        /// Sets proxy server.
        /// </summary>
        [OptionFlag("-p")]
        public string Proxy { get; set; }

        /// <summary>
        /// HTTP Authentication username.
        /// </summary>
        [OptionFlag("--username")]
        public string UserName { get; set; }

        /// <summary>
        /// HTTP Authentication password.
        /// </summary>
        [OptionFlag("--password")]
        public string Password { get; set; }

        /// <summary>
        /// Disable the intelligent shrinking strategy used by WebKit that makes the pixel/dpi ratio none constant
        /// </summary>
        [OptionFlag("--disable-smart-shrinking")]
        public bool DisableSmartShrinking { get; set; }

        /// <summary>
        /// Set viewport size if you have custom scrollbars or css attribute overflow to emulate window size (default 800)
        /// </summary>
        [OptionFlag("--viewport-size")]
        public double? ViewportSize { get; set; }

        /// <summary>
        /// Use this if you need another switches that are not currently supported by Rotativa.
        /// </summary>
        [OptionFlag("")]
        public string CustomSwitches { get; set; }


        [Obsolete(@"Use BuildFile(ActionContext) method instead and use the resulting binary data to do what needed.")]
        public string SaveOnServerPath { get; set; }

        /// <summary>
        /// Set 'Content-Disposition' Response Header. If you set 'FileName', this value is ignored and "attachment" is forced.
        /// </summary>
        public ContentDisposition ContentDisposition { get; set; }


        /// <summary>
        /// If you want to save the generated binary file to an external source such as Azure BLOB, please use this.
        /// Please return true to continue processing, false to drop with error.
        /// </summary>
        public Func<byte[], ActionContext,string, Task<bool>> OnBuildFileSuccess { get; set; } = async (byteArray, applicationContext, fileName) => true;



        protected abstract string GetUrl(ActionContext  context);

        /// <summary>
        /// Returns properties with OptionFlag attribute as one line that can be passed to wkhtmltopdf binary.
        /// </summary>
        /// <returns>Command line parameter that can be directly passed to wkhtmltopdf binary.</returns>
        protected virtual string GetConvertOptions()
        {
            var result = new StringBuilder();

            var fields = GetType().GetProperties();
            foreach (var fi in fields)
            {
                if (!(fi.GetCustomAttributes(typeof(OptionFlag), true).FirstOrDefault() is OptionFlag of))
                    continue;

                object value = fi.GetValue(this, null);
                if (value == null)
                    continue;

                if (fi.PropertyType == typeof(Dictionary<string, string>))
                {
                    var dictionary = (Dictionary<string, string>)value;
                    foreach (var d in dictionary)
                    {
                        result.AppendFormat(" {0} {1} {2}", of.Name, d.Key, d.Value);
                    }
                }
                else if (fi.PropertyType == typeof(bool))
                {
                    if ((bool)value)
                        result.AppendFormat(CultureInfo.InvariantCulture, " {0}", of.Name);
                }
                else
                {
                    result.AppendFormat(CultureInfo.InvariantCulture, " {0} {1}", of.Name, value);
                }
            }

            return result.ToString().Trim();
        }

        private string GetWkParams(ActionContext  context)
        {
            var switches = string.Empty;

            var cookieOptions = context.HttpContext.RequestServices.GetService<IOptions<CookieAuthenticationOptions>>();

            if (cookieOptions.Value != null && !string.IsNullOrEmpty(cookieOptions.Value.Cookie.Name))
            {
                var cookieName = cookieOptions.Value.Cookie.Name;

                string authenticationCookie = null;
                if (context.HttpContext.Request.Cookies != null && context.HttpContext.Request.Cookies.ContainsKey(cookieName))
                {
                    authenticationCookie = context.HttpContext.Request.Cookies[cookieName];
                }
                if (authenticationCookie != null)
                {
                    var authCookieValue = authenticationCookie;
                    switches += " --cookie " + FormsAuthenticationCookieName + " " + authCookieValue;
                }
            }

            switches += " " + GetConvertOptions();

            var url = GetUrl(context);
            switches += " " + url;

            return switches;
        }

        protected virtual byte[] CallTheDriver(ActionContext  context)
        {
            var switches = GetWkParams(context);
            var fileContent = WkhtmlConvert(switches);
            return fileContent;
        }

        protected abstract byte[] WkhtmlConvert(string switches);

        public byte[] BuildFile(ActionContext  context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (WkhtmlPath == string.Empty)
            {
                var location = Assembly.GetEntryAssembly().Location;
                var directory = Path.GetDirectoryName(location);
                WkhtmlPath = Path.Combine(directory, "WkHtmlToPdf");
            }

            var fileContent = CallTheDriver(context);

            if (string.IsNullOrEmpty(SaveOnServerPath) == false)
                File.WriteAllBytes(SaveOnServerPath, fileContent);


            if (!OnBuildFileSuccess?.Invoke(fileContent, context, FileName).Result ?? true)
                throw new InvalidOperationException($"{nameof(OnBuildFileSuccess)} returned false.");

            return fileContent;
        }

        public override void ExecuteResult(ActionContext context)
        {
            var fileContent = BuildFile(context);

            var response = PrepareResponse(context.HttpContext.Response);

            response.Body.Write(fileContent, 0, fileContent.Length);
        }

        private static string SanitizeFileName(string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars()));
            var invalidCharsPattern = $@"[{invalidChars}]+";

            var result = Regex.Replace(name, invalidCharsPattern, "_");
            return result;
        }

        protected HttpResponse  PrepareResponse(HttpResponse  response)
        {
            response.ContentType = GetContentType();

            if (!string.IsNullOrEmpty(FileName))
            {
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{SanitizeFileName(FileName).Trim()}\"");
                return response;
            }

            response.Headers.Add("Content-Disposition", $"{ContentDisposition.ToString().ToLower()}");
            return response;
        }

        protected abstract string GetContentType();
    }
}