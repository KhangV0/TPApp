using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using TPApp.Data.Entities;

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

        public static string TiemLucDetailUrl(
            this IHtmlHelper html,
            SearchIndexContent item)
        {
            if (item == null) return "#";

            var domain = html.MainDomain().TrimEnd('/');

            return item.TypeName switch
            {
                "Tiềm lực Chuyên gia"
                    => $"{domain}/FrontEnd/Page/TiemLucKHCN/ChuyenGia.aspx?id={item.RefId}",

                "Tiềm lực Phòng thí nghiệm"
                    => $"{domain}/FrontEnd/Page/TiemLucKHCN/PhongThiNghiem.aspx?id={item.RefId}",

                "Tiềm lực Tổ chức"
                    => $"{domain}/FrontEnd/Page/TiemLucKHCN/ToChuc.aspx?id={item.RefId}",

                "Tiềm lực Doanh nghiệp"
                    => $"{domain}/FrontEnd/Page/TiemLucKHCN/DoanhNghiep.aspx?id={item.RefId}",

                "Tài Sản Trí Tuệ"
                    => $"{domain}/FrontEnd/Page/TiemLucKHCN/TaiSantriTue.aspx?id={item.RefId}",

                _ => "#"
            };
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
        // ── Format tiền VNĐ dùng chung cho các bước ──────────────────────────

        private static readonly CultureInfo _viVN = CultureInfo.GetCultureInfo("vi-VN");

        /// <summary>1.500.000.000 VNĐ (null → "——")</summary>
        public static string FormatVnd(decimal? value)
            => value.HasValue
               ? value.Value.ToString("N0", _viVN) + " VNĐ"
               : "——";

        /// <summary>1.500.000.000 VNĐ từ string (null/empty → "——")</summary>
        public static string FormatVnd(string? value)
            => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
               ? d.ToString("N0", _viVN) + " VNĐ"
               : "——";

        /// <summary>Chỉ số thuần: 1.500.000.000 (không kèm "VNĐ")</summary>
        public static string FormatVndNumber(decimal? value)
            => value.HasValue ? value.Value.ToString("N0", _viVN) : "——";

        /// <summary>Badge HTML xanh lá với giá VNĐ, dùng @Html.Raw()</summary>
        public static string FormatVndBadge(decimal? value)
            => value.HasValue
               ? $"<span class=\"badge bg-success fs-6\"><i class=\"fas fa-coins me-1\"></i>{value.Value.ToString("N0", _viVN)} VNĐ</span>"
               : "<span class=\"text-muted\">Chưa cập nhật</span>";
    }
}
