# ADNL Client Sample

This is a simple console application that demonstrates how to connect to a TON lite server using the ADNL protocol.

## Usage

### Interactive Mode

```bash
dotnet run
```

The program will prompt you for the lite server details.

### Command Line Mode

```bash
dotnet run <host> <port> <publicKeyBase64>
```

Example:

```bash
dotnet run 135.181.140.212 13206 "aF91CuUHuuOv9rm2W5+O/4h38M3sRm40DtSdRxQhmtQ="
```

## Getting Lite Server Configs

You can get lite server configurations from the official TON global configs:

- **Mainnet**: https://ton.org/global-config.json
- **Testnet**: https://ton.org/testnet-global.config.json

Look for the `liteservers` array in the JSON. Each lite server has:

- `ip` (needs to be converted from int to IP address format)
- `port`
- `id.key` (base64-encoded public key)

### Example from global-config.json

```json
{
  "ip": 2280728268,
  "port": 13206,
  "id": {
    "@type": "pub.ed25519",
    "key": "aF91CuUHuuOv9rm2W5+O/4h38M3sRm40DtSdRxQhmtQ="
  }
}
```

To convert the IP:

- `2280728268` → `135.181.140.212`

So you would run:

```bash
dotnet run 135.181.140.212 13206 "aF91CuUHuuOv9rm2W5+O/4h38M3sRm40DtSdRxQhmtQ="
```

## What It Does

1. Creates an ADNL client with random keys
2. Connects to the specified lite server
3. Performs the ADNL handshake
4. Sends a `liteServer.getMasterchainInfo` query
5. Receives and deserializes the response to display:
    - Last masterchain block (workchain, shard, seqno, hashes)
    - State root hash
    - Init (zero state) info

## Example Output

```
=== TON ADNL Client Sample ===

Connecting to: 135.181.140.212:13206
Server Address: 685F750AE507BAE3AFF6B9B65B9F8EFF8877F0CDEC466E340ED49D471421DAD4

Initiating connection...
[Event] 14:23:45.123 - TCP Connected
[Event] 14:23:45.234 - ADNL Ready (handshake complete)

✅ Successfully connected and ready!

Sending liteServer.getMasterchainInfo query...
Query serialized to 4 bytes
Query hex: 80BEF5BF
Query sent! Waiting for response...

[Data] 14:23:45.456 - Received 120 bytes

✅ Received response: 120 bytes
Response hex: 373633F9FFFFFFFF8000000000000000...

Deserializing response...
Constructor ID: 0xF9333637

✅ Successfully deserialized LiteServerMasterchainInfo:
  Last block:
    Workchain: -1
    Shard: -9223372036854775808
    Seqno: 45123456
    Root Hash: 1A2B3C4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0B1C2D3E4F5A6B7C8D9E0F1A2B
    File Hash: 2B3C4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0B1C2D3E4F5A6B7C8D9E0F1A2B3C
  State Root Hash: 3C4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0B1C2D3E4F5A6B7C8D9E0F1A2B3C4D
  Init (zero state):
    Workchain: -1
    Root Hash: 4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0B1C2D3E4F5A6B7C8D9E0F1A2B3C4D5E
    File Hash: 5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0B1C2D3E4F5A6B7C8D9E0F1A2B3C4D5E6F

Closing connection...
Disconnected.
```

## Next Steps

You can extend this sample to send more queries:

- `liteServer.getTime` - Get current server time
- `liteServer.getAccountState` - Get account state
- `liteServer.getTransactions` - Get account transactions
- `liteServer.sendMessage` - Send external message
- And many more from the `Functions` class in `Schema.Generated.cs`

All function constructors are available in `Ton.Adnl.Protocol.Functions` and response types can be deserialized using
their `ReadFrom` methods.

