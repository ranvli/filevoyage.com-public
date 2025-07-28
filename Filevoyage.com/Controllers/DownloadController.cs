// Controllers/DownloadController.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Filevoyage.com.Models;
using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Filevoyage.com.Controllers
{
    public class DownloadController : Controller
    {
        private readonly AzureStorageService _storage;
        private readonly CosmosDbService _cosmos;

        public DownloadController(
            AzureStorageService storage,
            CosmosDbService cosmosDb)
        {
            _storage = storage;
            _cosmos = cosmosDb;
        }

        // 1) Página de “pre‑descarga”: /ABC123
        [HttpGet("{shortCode}")]
        public async Task<IActionResult> DownloadPage(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                ViewBag.ErrorReason = "Código inválido o inexistente.";
                return View("DownloadExpired");
            }

            var meta = await _cosmos.GetItemByIdAsync(shortCode);
            if (meta == null)
            {
                ViewBag.ErrorReason = "Código inválido o inexistente.";
                return View("DownloadExpired");
            }

            if (meta.ExpirationDate < DateTime.UtcNow)
            {
                ViewBag.ErrorReason = $"El archivo expiró el {meta.ExpirationDate:dd/MM/yyyy HH:mm} UTC.";
                return View("DownloadExpired");
            }

            int? remaining = meta.MaxDownloads > 0
                ? meta.MaxDownloads - meta.DownloadCount
                : (int?)null;

            if (remaining.HasValue && remaining.Value <= 0)
            {
                ViewBag.ErrorReason = "Se alcanzó el número máximo de descargas.";
                return View("DownloadExpired");
            }

            ViewBag.ShortCode = shortCode;
            ViewBag.Filename = meta.Filename;
            ViewBag.ProtectWithQR = meta.ProtectWithQR;
            ViewBag.Remaining = remaining;

            var host = $"{Request.Scheme}://{Request.Host}";
            ViewBag.StreamUrl = $"{host}/{shortCode}/stream";

            return View("Download");
        }


        // 2) Endpoint para generar el QR: /ABC123/qr
        [HttpGet("{shortCode}/qr")]
        public async Task<IActionResult> QrCode(string shortCode)
        {
            // (podrías validar meta como arriba, pero asumo que /qr sólo lo ve quien sabe el código)
            var host = $"{Request.Scheme}://{Request.Host}";
            var downloadLink = $"{host}/{shortCode}";

            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(downloadLink, QRCodeGenerator.ECCLevel.Q);
            using var qrBmp = new PngByteQRCode(qrData);
            var pngBytes = qrBmp.GetGraphic(20);

            return File(pngBytes, "image/png");
        }

        // 3) Descarga real “stream”: /ABC123/stream
        [HttpGet("{shortCode}/stream")]
        public async Task<IActionResult> StreamDownload(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return NotFound();

            var meta = await _cosmos.GetItemByIdAsync(shortCode);
            if (meta == null)
                return NotFound();

            // Límite de descargas
            if (meta.MaxDownloads > 0 && meta.DownloadCount >= meta.MaxDownloads)
                return Forbid("Límite de descargas excedido.");

            // Incrementa contador
            meta.DownloadCount++;
            await _cosmos.UpdateItemAsync(meta);

            // Opción: devolver remaining en cabecera
            if (meta.MaxDownloads > 0)
            {
                var rem = meta.MaxDownloads - meta.DownloadCount;
                Response.Headers["X-Remaining-Downloads"] = rem.ToString();
            }

            // Recupera el blob
            var blobName = Path.GetFileName(meta.DownloadUrl);
            var result = await _storage.DownloadFileStreamAsync(blobName);
            if (result == null)
                return NotFound();

            // AzureStorageService devuelve (Stream Content, string ContentType)
            var stream = result.Value.Content;
            var contentType = result.Value.ContentType;

            // Devuelve el archivo al cliente
            return File(stream, contentType, meta.Filename);
        }
    }
}
