using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RestPrint
{
    [Route("/[controller]")]
    public class PrintersController : ControllerBase
    {
        [HttpGet]
        public string[] GetAll()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
        }

        [HttpPost("{printerName}/Jobs")]
        public async Task<ActionResult> Print(string printerName, string name)
        {
            switch (Request.ContentType)
            {
                case "application/octet-stream":
                    return await PrintRawAsync(printerName, name, Request.Body);
                default:
                    return new UnsupportedMediaTypeResult();
            }
        }

        private async Task<ActionResult> PrintRawAsync(string printerName, string name, Stream stream)
        {
            var di = new WinSpool.DOCINFOA
            {
                pDocName = name ?? "RestPrint",
                pDataType = "RAW"
            };

            var printerHandle = IntPtr.Zero;
            var docStarted = false;
            var pageStarted = false;
            var buffer = new byte[1024];
            var bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                var bufferAddress = bufferPin.AddrOfPinnedObject();

                if (WinSpool.OpenPrinter(printerName.Normalize(), out printerHandle, IntPtr.Zero))
                {
                    docStarted = WinSpool.StartDocPrinter(printerHandle, 1, di);

                    if (docStarted)
                    {
                        pageStarted = WinSpool.StartPagePrinter(printerHandle);

                        if (pageStarted)
                        {
                            int readBytes;

                            while ((readBytes = await stream.ReadAsync(buffer)) > 0)
                            {
                                if (!WinSpool.WritePrinter(printerHandle, bufferAddress, readBytes, out var written))
                                {
                                    return BadGateway("Unable to write to printer");
                                }

                                if (written != readBytes)
                                {
                                    return BadGateway("Wrote wrong number of bytes to printer");
                                }
                            }

                            return Ok();
                        }
                        else
                        {
                            return BadGateway("Cannot start page");
                        }
                    }
                    else
                    {
                        return BadGateway("Cannot start document");
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            finally
            {
                bufferPin.Free();

                if (pageStarted)
                {
                    WinSpool.EndPagePrinter(printerHandle);
                }

                if (docStarted)
                {
                    WinSpool.EndDocPrinter(printerHandle);
                }

                if (printerHandle != IntPtr.Zero)
                {
                    WinSpool.ClosePrinter(printerHandle);
                }
            }
        }

        private ActionResult BadGateway(object value) => StatusCode(502, value);
    }
}
