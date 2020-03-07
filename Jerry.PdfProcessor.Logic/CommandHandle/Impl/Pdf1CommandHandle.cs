using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.Model;
using Jerry.PdfProcessor.Logic.PDF;
using Newtonsoft.Json;

namespace Jerry.PdfProcessor.Logic.CommandHandle.Impl
{
    public class Pdf1CommandHandle:BaseCommandHandle<PdfRequest,PdfResponse>
    {
        internal override ILogManager _logManager { get; set; }
        private readonly IPdfGenerator _PdfGenerator;

        public Pdf1CommandHandle(ILogManager logManager, IPdfGenerator pdfGenerator)
        {
            _logManager = logManager;
            _PdfGenerator = pdfGenerator;
        }
        public override async Task<PdfResponse> DoBusinessLogic(PdfRequest request)
        {
            _logManager.Info($"{nameof(Pdf1CommandHandle) } DoBusinessLogic");

            await CreateDefaultPdf(request);

            var res = await Task.FromResult(new PdfResponse() {PdfFile = "new pdf file"});
            return res;
        }

        private async Task CreateDefaultPdf(PdfRequest request)
        {
            try
            {
                _PdfGenerator.CreatePdf(request);
            }
            catch (Exception e)
            {
                _logManager.Error($"{nameof(Pdf1CommandHandle) } get error ",e);
                //throw;
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
