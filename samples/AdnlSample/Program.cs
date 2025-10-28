using Ton.Adnl;
using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace AdnlSample;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("=== TON ADNL Client Sample ===\n");

        // Example lite server config - you can get these from global config
        // https://ton.org/global-config.json
        string host;
        int port;
        string publicKeyBase64;

        if (args.Length >= 3)
        {
            host = args[0];
            port = int.Parse(args[1]);
            publicKeyBase64 = args[2];
        }
        else
        {
            // Default to a public testnet lite server (if available)
            Console.WriteLine("Usage: dotnet run <host> <port> <publicKeyBase64>");
            Console.WriteLine("\nExample lite server configs can be found at:");
            Console.WriteLine("  Mainnet: https://ton.org/global-config.json");
            Console.WriteLine("  Testnet: https://ton.org/testnet-global.config.json");
            Console.WriteLine("\nPlease provide lite server details:");
            
            Console.Write("Host: ");
            host = Console.ReadLine() ?? "";
            
            Console.Write("Port: ");
            port = int.Parse(Console.ReadLine() ?? "0");
            
            Console.Write("Public Key (base64): ");
            publicKeyBase64 = Console.ReadLine() ?? "";
        }

        if (string.IsNullOrEmpty(host) || port == 0 || string.IsNullOrEmpty(publicKeyBase64))
        {
            Console.WriteLine("Error: Invalid lite server configuration");
            return;
        }

        try
        {
            // Decode the server's public key
            byte[] serverPublicKey = Convert.FromBase64String(publicKeyBase64);
            var serverAddress = new AdnlAddress(serverPublicKey);

            Console.WriteLine($"Connecting to: {host}:{port}");
            Console.WriteLine($"Server Address: {Convert.ToHexString(serverAddress.Hash)}");
            Console.WriteLine();

            // Create ADNL client
            using var client = new AdnlClient(host, port, serverPublicKey);

            TaskCompletionSource<byte[]> responseReceived = new();
            TaskCompletionSource<bool> readyReceived = new();

            // Subscribe to events
            client.Connected += () =>
            {
                Console.WriteLine($"[Event] {DateTime.Now:HH:mm:ss.fff} - TCP Connected");
            };

            client.Ready += () =>
            {
                Console.WriteLine($"[Event] {DateTime.Now:HH:mm:ss.fff} - ADNL Ready (handshake complete)");
                readyReceived.TrySetResult(true);
            };

            client.Closed += () =>
            {
                Console.WriteLine($"[Event] {DateTime.Now:HH:mm:ss.fff} - Connection Closed");
            };

            client.DataReceived += (data) =>
            {
                Console.WriteLine($"[Data] {DateTime.Now:HH:mm:ss.fff} - Received {data.Length} bytes");
                responseReceived.TrySetResult(data);
            };

            client.Error += (ex) =>
            {
                Console.WriteLine($"[Error] {DateTime.Now:HH:mm:ss.fff} - {ex.Message}");
                Console.WriteLine($"[Error] Stack trace: {ex.StackTrace}");
                responseReceived.TrySetException(ex);
                readyReceived.TrySetException(ex);
            };

            // Connect to the lite server
            Console.WriteLine("Initiating connection...");
            
            try
            {
                using var connectTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await client.ConnectAsync(connectTimeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\n❌ Connection timeout after 15 seconds");
                Console.WriteLine($"Current state: {client.State}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Connection failed: {ex.Message}");
                throw;
            }

            // Wait for the Ready event with timeout
            using var readyTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await readyReceived.Task.WaitAsync(readyTimeoutCts.Token);
            }
            catch (TimeoutException)
            {
                Console.WriteLine("\n❌ Timeout waiting for handshake to complete (10 seconds)");
                Console.WriteLine($"Current state: {client.State}");
                throw;
            }

                if (client.State == AdnlClientState.Ready)
                {
                    Console.WriteLine("\n✅ Successfully connected and ready!");
                    
                    // Send a query to get masterchain info
                    Console.WriteLine("\nSending liteServer.getMasterchainInfo query...");
                    
                    // 1. Create the lite server query (just the function constructor)
                    var liteQueryWriter = new TLWriteBuffer();
                    liteQueryWriter.WriteUInt32(Functions.GetMasterchainInfo);
                    byte[] liteQuery = liteQueryWriter.Build();
                    
                    // 2. Generate a random 32-byte query ID
                    byte[] queryId = AdnlKeys.GenerateRandomBytes(32);
                    
                    // 3. Wrap in liteServer.query (CRC32 of "liteServer.query data:bytes = Object")
                    var liteServerQueryWriter = new TLWriteBuffer();
                    liteServerQueryWriter.WriteUInt32(0x798C06DF); // liteServer.query constructor
                    liteServerQueryWriter.WriteBuffer(liteQuery);
                    byte[] liteServerQuery = liteServerQueryWriter.Build();
                    
                    // 4. Wrap in adnl.message.query (CRC32 of "adnl.message.query query_id:int256 query:bytes = adnl.Message")
                    var adnlQueryWriter = new TLWriteBuffer();
                    adnlQueryWriter.WriteUInt32(0xB48BF97A); // adnl.message.query constructor
                    // Write query ID as Int256 (BigInteger from little-endian bytes)
                    var queryIdBigInt = new System.Numerics.BigInteger(queryId);
                    adnlQueryWriter.WriteInt256(queryIdBigInt);
                    adnlQueryWriter.WriteBuffer(liteServerQuery);
                    byte[] finalQuery = adnlQueryWriter.Build();
                    
                    await client.WriteAsync(finalQuery);
                    Console.WriteLine("Query sent! Waiting for response...\n");

                // Wait for response with timeout
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        var responseData = await responseReceived.Task.WaitAsync(timeoutCts.Token);
                        
                        Console.WriteLine($"\n✅ Received response: {responseData.Length} bytes");
                        
                        // Unwrap ADNL protocol layers
                        var adnlReader = new TLReadBuffer(responseData);
                        
                        // 1. Read ADNL message type
                        uint adnlMessageType = adnlReader.ReadUInt32();
                        
                if (adnlMessageType == 0xDC69FB03) // tcp.pong (CRC32 of "tcp.pong random_id:long = tcp.Pong")
                {
                    Console.WriteLine("Received pong message (heartbeat), ignoring");
                }
                else if (adnlMessageType == 0x0FAC8416) // adnl.message.answer (CRC32 of "adnl.message.answer query_id:int256 answer:bytes = adnl.Message")
                        {
                            // 2. Read query ID (should match what we sent)
                            byte[] responseQueryId = adnlReader.ReadBytes(32);
                            
                            // 3. Read lite server response (length-prefixed)
                            byte[] liteServerResponse = adnlReader.ReadBuffer();
                            
                            // 4. Parse lite server response
                            var liteReader = new TLReadBuffer(liteServerResponse);
                            uint constructorId = liteReader.ReadUInt32();
                            
                            // Check if it's an error
                            if (constructorId == LiteServerError.Constructor)
                            {
                                var error = LiteServerError.ReadFrom(liteReader);
                                Console.WriteLine($"\n❌ LiteServer error {error.Code}: {error.Message}");
                            }
                            // Check if it's LiteServerCurrentTime
                            else if (constructorId == LiteServerCurrentTime.Constructor)
                            {
                                var currentTime = LiteServerCurrentTime.ReadFrom(liteReader);
                                var dateTime = DateTimeOffset.FromUnixTimeSeconds(currentTime.Now);
                                Console.WriteLine("\n✅ Successfully deserialized LiteServerCurrentTime:");
                                Console.WriteLine($"  Unix timestamp: {currentTime.Now}");
                                Console.WriteLine($"  Date/Time: {dateTime:yyyy-MM-dd HH:mm:ss} UTC");
                            }
                            // Check if it's LiteServerMasterchainInfo
                            else if (constructorId == LiteServerMasterchainInfo.Constructor)
                            {
                                var info = LiteServerMasterchainInfo.ReadFrom(liteReader);
                                Console.WriteLine("\n✅ Successfully deserialized LiteServerMasterchainInfo:");
                                Console.WriteLine($"  Last block:");
                                Console.WriteLine($"    Workchain: {info.Last.Workchain}");
                                Console.WriteLine($"    Shard: {info.Last.Shard}");
                                Console.WriteLine($"    Seqno: {info.Last.Seqno}");
                                Console.WriteLine($"    Root Hash: {Convert.ToHexString(info.Last.RootHash)}");
                                Console.WriteLine($"    File Hash: {Convert.ToHexString(info.Last.FileHash)}");
                                Console.WriteLine($"  State Root Hash: {Convert.ToHexString(info.StateRootHash)}");
                                Console.WriteLine($"  Init (zero state):");
                                Console.WriteLine($"    Workchain: {info.Init.Workchain}");
                                Console.WriteLine($"    Root Hash: {Convert.ToHexString(info.Init.RootHash)}");
                                Console.WriteLine($"    File Hash: {Convert.ToHexString(info.Init.FileHash)}");
                            }
                            else
                            {
                                Console.WriteLine($"❌ Unknown lite server constructor: 0x{constructorId:X8}");
                            }
                        }
                        else
                        {
                        }
                    }
                catch (TimeoutException)
                {
                    Console.WriteLine("\n⏱️ Timeout waiting for response (10 seconds)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Error processing response: {ex.Message}");
                    Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"\n❌ Connection failed. Current state: {client.State}");
            }

            // Close connection
            Console.WriteLine("\nClosing connection...");
            await client.CloseAsync();
            Console.WriteLine("Disconnected.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
        }
    }

    private static async Task WaitForCancellation()
    {
        var tcs = new TaskCompletionSource<bool>();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            tcs.SetResult(true);
        };
        await tcs.Task;
    }
}
