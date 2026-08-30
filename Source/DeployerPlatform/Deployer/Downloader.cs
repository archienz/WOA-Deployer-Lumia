using System;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ByteSizeLib;
using Deployer.Utils;
using Serilog;

namespace Deployer
{
    public class Downloader : IDownloader
    {
        private readonly HttpClient client;

        public Downloader(HttpClient client)
        {
            this.client = client;
        }

        public async Task Download(string url, string path, IOperationProgress progressObserver = null, int timeout = 30)
        {
            using (var fileStream = File.OpenWrite(path))
            {
                await Download(url, fileStream, progressObserver, timeout);
            }

            var hash = ComputeSha256(path);
            Log.Information("Downloaded {Url} to {Path} SHA256={Hash}", url, path, hash);

            var expectedPath = path + ".sha256";
            if (File.Exists(expectedPath))
            {
                var expected = File.ReadAllText(expectedPath).Trim().Split(' ')[0];
                if (!string.Equals(expected, hash, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"SHA256 mismatch for {path}. Expected {expected}, got {hash}.");
                }
            }
        }

        private async Task Download(string url, Stream destination, IOperationProgress progressObserver = null,
            int timeout = 30)
        {
            long? totalBytes = 0;
            long bytesWritten = 0;

            await ObservableMixin.Using(() => client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead),
                    s =>
                    {
                        totalBytes = s.Content.Headers.ContentLength;
                        if (!totalBytes.HasValue)
                        {
                            progressObserver?.Percentage.OnNext(double.PositiveInfinity);
                        }
                        return ObservableMixin.Using(() => s.Content.ReadAsStreamAsync(),
                            contentStream => contentStream.ReadToEndObservable());
                    })
                .Do(bytes =>
                {
                    bytesWritten += bytes.Length;
                    if (totalBytes.HasValue)
                    {
                        progressObserver?.Percentage.OnNext((double)bytesWritten / totalBytes.Value);                        
                    }

                    progressObserver?.Value?.OnNext(bytesWritten);
                })
                .Timeout(TimeSpan.FromSeconds(timeout))
                .Select(bytes => Observable.FromAsync(async () =>
                {
                    await destination.WriteAsync(bytes, 0, bytes.Length);
                    return Unit.Default;
                }))
                .Merge(1);
        }

        private static readonly int BufferSize = (int)ByteSize.FromKiloBytes(8).Bytes;

        public async Task<Stream> GetStream(string url, IOperationProgress progress = null, int timeout = 30)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            var stream = File.Create(tmpFile, BufferSize, FileOptions.DeleteOnClose);

            await Download(url, stream, progress, timeout);
            stream.Position = 0;
            string hash;
            using (var sha = SHA256.Create())
            {
                hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            }

            Log.Information("Downloaded stream from {Url} SHA256={Hash}", url, hash);
            stream.Position = 0;
            return stream;
        }

        private static string ComputeSha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }
    }
}