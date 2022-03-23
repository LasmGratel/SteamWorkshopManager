using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace SteamWorkshopManager.Util.UI;

/// <summary>
/// Usage: 
/// 1) In a XAML file, declare the above namespace, e.g.:
///    xmlns:common="using:WinRT_RichTextBlock.Html2Xaml"
///     
/// 2) In RichTextBlock controls, set or databind the Html property, e.g.:
/// 
///    <RichTextBlock common:Properties.Html="{Binding ...}"/>
///    
///    or
///    
///    <RichTextBlock>
///       <common:Properties.Html>
///         <![CDATA[
///             <p>This is a list:</p>
///             <ul>
///                 <li>Item 1</li>
///                 <li>Item 2</li>
///                 <li>Item 3</li>
///             </ul>
///         ]]>
///       </common:Properties.Html>
///    </RichTextBlock>
/// </summary>
public class Properties : DependencyObject
{
    public static readonly DependencyProperty HtmlProperty =
        DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Properties), new PropertyMetadata(null, HtmlChanged));

    public static void SetHtml(DependencyObject obj, string value)
    {
        obj.SetValue(HtmlProperty, value);
    }

    public static string GetHtml(DependencyObject obj)
    {
        return (string)obj.GetValue(HtmlProperty);
    }

    private static async void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Get the target RichTextBlock
        var richText = d as RichTextBlock;
        if (richText == null) return;

        // Wrap the value of the Html property in a div and convert it to a new RichTextBlock
        var xhtml = $"<div>{e.NewValue as string}</div>";
        xhtml = xhtml.Replace("\r", "").Replace("\n", "<br />");
        RichTextBlock newRichText = null;
        if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
        {
            // In design mode we swallow all exceptions to make editing more friendly
            var xaml = "";
            try
            {
                xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
                newRichText = (RichTextBlock)XamlReader.Load(xaml);
            }
            catch (Exception ex)
            {
                var errorxaml = $@"
                        <RichTextBlock 
                         xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                        >
                            <Paragraph>An exception occurred while converting HTML to XAML: {ex.Message}</Paragraph>
                            <Paragraph />
                            <Paragraph>HTML:</Paragraph>
                            <Paragraph>{EncodeXml(xhtml)}</Paragraph>
                            <Paragraph />
                            <Paragraph>XAML:</Paragraph>
                            <Paragraph>{EncodeXml(xaml)}</Paragraph>
                        </RichTextBlock>";
                newRichText = (RichTextBlock)XamlReader.Load(errorxaml);
            } // Display a friendly error in design mode.
        }
        else
        {
            // When not in design mode, we let the application handle any exceptions
            try
            {
                var xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
                newRichText = (RichTextBlock)XamlReader.Load(xaml);
            }
            catch (Exception)
            {
                var errorxaml = $@"
                        <RichTextBlock 
                         xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                        >
                            <Paragraph>Cannot convert HTML to XAML. Please ensure that the HTML content is valid.</Paragraph>
                            <Paragraph />
                            <Paragraph>HTML:</Paragraph>
                            <Paragraph>{EncodeXml(xhtml)}</Paragraph>
                        </RichTextBlock>";
                newRichText = (RichTextBlock)XamlReader.Load(errorxaml);
            } // Display a friendly error in design mode.
        }

        // Move the blocks in the new RichTextBlock to the target RichTextBlock
        richText.Blocks.Clear();
        if (newRichText != null)
        {
            for (var i = newRichText.Blocks.Count - 1; i >= 0; i--)
            {
                var b = newRichText.Blocks[i];
                newRichText.Blocks.RemoveAt(i);
                richText.Blocks.Insert(0, b);
            }
        }
    }

    private static string EncodeXml(string xml)
    {
        var encodedXml = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        return encodedXml;
    }

    private static XsltProcessor _html2XamlProcessor;

    private static async Task<string> ConvertHtmlToXamlRichTextBlock(string xhtml)
    {
        // Load XHTML fragment as XML document
        var xhtmlDoc = new XmlDocument();
        xhtmlDoc.LoadXml(xhtml);

        if (_html2XamlProcessor == null)
        {
            // Read XSLT. In design mode we cannot access the xslt from the file system (with Build Action = Content), 
            // so we use it as an embedded resource instead:
            var assembly = typeof(Properties).GetTypeInfo().Assembly;
            await using var stream = assembly.GetManifestResourceStream("SteamWorkshopManager.RichTextBlockHtml2Xaml.xslt");
            var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            var html2XamlXslDoc = new XmlDocument();
            html2XamlXslDoc.LoadXml(content);
            _html2XamlProcessor = new XsltProcessor(html2XamlXslDoc);
        }

        // Apply XSLT to XML
        var xaml = _html2XamlProcessor.TransformToString(xhtmlDoc.FirstChild);
        return xaml;
    }

}