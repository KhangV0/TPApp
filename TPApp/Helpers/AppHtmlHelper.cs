using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace TPApp.Web.Helpers
{
    public static class AppHtmlHelper
    {
        public static string MainDomain(this IHtmlHelper html)
        {
            var config = html.ViewContext.HttpContext
                .RequestServices
                .GetService(typeof(IConfiguration)) as IConfiguration;

            return config?["AppSettings:MainDomain"] ?? string.Empty;
        }

        public static string FormatCurrencyOto(
            this IHtmlHelper html,
            string? value,
            string currency)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "0")
            {
                return "Liên hệ";
            }

            if (!double.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return "Liên hệ";
            }

            return string.Format(
                CultureInfo.GetCultureInfo("vi-VN"),
                "{0:#,##0} {1}",
                number,
                currency);
        }
    }
}
