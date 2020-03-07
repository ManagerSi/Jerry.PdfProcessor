using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iText.Html2pdf;
using Jerry.Model;

namespace Jerry.PdfProcessor.Logic.PDF
{
    public class PdfGenerator:IPdfGenerator
    {
        public void CreatePdf(BaseRequest request)
        {
            string basePath = Environment.CurrentDirectory;
            string htmlPath = Path.Combine(basePath, "html", "input.html");
            string pdfPath = Path.Combine(basePath, "pdf", "output"+$"{DateTime.Now.ToString("yy-MM-dd_hhmmss_ffff")}"+".pdf");

            using (FileStream htmlSource = File.Open(htmlPath, FileMode.Open))
            using (FileStream pdfDest = File.Open(pdfPath, FileMode.OpenOrCreate))
            {
                ConverterProperties converterProperties = new ConverterProperties();
                string assertPath = Path.Combine(basePath, "html");
                converterProperties.SetBaseUri(assertPath);
                HtmlConverter.ConvertToPdf(htmlSource, pdfDest, converterProperties);
            }
        }
    }
}
