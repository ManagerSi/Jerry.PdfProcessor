using System;
using System.Collections.Generic;
using System.Text;
using Jerry.Model;

namespace Jerry.PdfProcessor.Logic.PDF
{
    public interface IPdfGenerator
    {
        void CreatePdf(BaseRequest request);
    }
}
