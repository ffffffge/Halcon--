using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.Core;

namespace Halcon查询.CommandBaseClass
{
    public static class WebView2Behavior
    {
        public static readonly DependencyProperty HtmlStringProperty =
            DependencyProperty.RegisterAttached("HtmlString", typeof(string),
                typeof(WebView2Behavior), new PropertyMetadata(null, OnHtmlStringChanged));

        public static string GetHtmlString(DependencyObject obj) =>
            (string)obj.GetValue(HtmlStringProperty);

        public static void SetHtmlString(DependencyObject obj, string value) =>
            obj.SetValue(HtmlStringProperty, value);

        private static async void OnHtmlStringChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is Microsoft.Web.WebView2.Wpf.WebView2 webView2)
            {
                await webView2.EnsureCoreWebView2Async();
                var html = (string)e.NewValue;

                if (!string.IsNullOrEmpty(html))
                {
                    // 方法1：使用 NavigateToString
                    webView2.CoreWebView2.NavigateToString(html);

                    // 方法2：使用 data URL（更好的编码处理）
                    // string dataUrl = $"data:text/html;charset=utf-8,{Uri.EscapeDataString(html)}";
                    // webView2.CoreWebView2.Navigate(dataUrl);
                }
                else
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync(
                        "document.body.innerHTML = ''");
                }
            }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string),
                typeof(WebView2Behavior), new PropertyMetadata(null, OnSourceChanged));

        public static string GetSource(DependencyObject obj) =>
            (string)obj.GetValue(SourceProperty);

        public static void SetSource(DependencyObject obj, string value) =>
            obj.SetValue(SourceProperty, value);

        private static async void OnSourceChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is Microsoft.Web.WebView2.Wpf.WebView2 webView2)
            {
                await webView2.EnsureCoreWebView2Async();
                var msource = (string)e.NewValue;

                if (!string.IsNullOrEmpty(msource))
                {
                    // 方法1：使用 NavigateToString
                    webView2.CoreWebView2.Navigate(msource);

                    //await webView2.CoreWebView2.ExecuteScriptAsync($"window.location.hash = '#page={345}';");

                    await webView2.CoreWebView2.ExecuteScriptAsync("window.location.reload();");

                    // 方法2：使用 data URL（更好的编码处理）
                    // string dataUrl = $"data:text/html;charset=utf-8,{Uri.EscapeDataString(html)}";
                    // webView2.CoreWebView2.Navigate(dataUrl);
                }
                else
                {
                    webView2.CoreWebView2.Navigate("");
                }
            }
        }
    }
}
