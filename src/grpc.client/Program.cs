using grpc.proto;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace grpc.client {
    class Program {
        static async Task Main(string[] args) {
            var start1mb = DateTime.Now.TimeOfDay;
            await GetFile(@"test1.txt");
            await GetFile(@"test2.txt");
            await GetFile(@"test3.txt");
            await GetFile(@"test4.txt");
            await GetFile(@"test5.txt");
            await GetFile(@"test6.txt");
            await GetFile(@"test7.txt");
            await GetFile(@"test8.txt");
            await GetFile(@"test9.txt");
            await GetFile(@"test10.txt");
            var stop1mb = DateTime.Now.TimeOfDay;
            Console.WriteLine($"Podsumowanie po wysłaniu 10 plików 1MB: { stop1mb - start1mb} \n");

            var start100mb = DateTime.Now.TimeOfDay;
            await GetFile(@"test100.txt");
            await GetFile(@"test102.txt");
            await GetFile(@"test103.txt");
            await GetFile(@"test104.txt");
            await GetFile(@"test105.txt");
            await GetFile(@"test106.txt");
            await GetFile(@"test107.txt");
            await GetFile(@"test108.txt");
            await GetFile(@"test109.txt");
            await GetFile(@"test110.txt");
            var stop100mb = DateTime.Now.TimeOfDay;
            Console.WriteLine($"Podsumowanie po wysłaniu 10 plików 100MB: { stop100mb - start100mb}");
            Console.ReadKey();
        }

        public static async Task GetFile(string filePath) {
            var beginCount = DateTime.Now.TimeOfDay;

            var _channel = GrpcChannel.ForAddress("https://localhost:5001/", new GrpcChannelOptions {
                MaxReceiveMessageSize = 5 * 1024 * 1024, // 5 MB
                MaxSendMessageSize = 5 * 1024 * 1024, // 5 MB
            });

            var _client = new FileTransferService.FileTransferServiceClient(_channel);
            var _request = new FileRequest { FilePath = filePath };
            var _temp_file = $"temp_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.tmp";
            var _final_file = _temp_file;

            using (var _call = _client.DownloadFile(_request)) { 
                await using (var _fs = File.OpenWrite(_temp_file)) {
                    await foreach (var _chunk in _call.ResponseStream.ReadAllAsync().ConfigureAwait(false)) {
                        var _total_size = _chunk.FileSize;

                        if (!String.IsNullOrEmpty(_chunk.FileName)) {
                            _final_file = _chunk.FileName;
                        }

                        if (_chunk.Chunk.Length == _chunk.ChunkSize)
                            _fs.Write(_chunk.Chunk.ToByteArray());
                        else {
                            _fs.Write(_chunk.Chunk.ToByteArray(), 0, _chunk.ChunkSize);
                        }
                    }
                }
            }

            if (_final_file != _temp_file)
                File.Move(_temp_file, _final_file);

            var endCount = DateTime.Now.TimeOfDay;
            Console.WriteLine($"podsumowanie czasu dla pliku {_final_file}: {endCount - beginCount}");
        }
    }
}