# Lite Client Playground

This sample demonstrates how to use the `Ton.LiteClient` library to interact with the TON blockchain.

## Features

- Connect to TON lite servers
- Get masterchain info
- Get shard information
- List transactions in blocks

## Usage

```bash
dotnet run <host> <port> <publicKey>
```

### Example with Public Lite Server

You can find public lite servers in the TON global config:
- Mainnet: https://ton.org/global-config.json
- Testnet: https://ton.org/testnet-global.config.json

Example (using a public testnet server):
```bash
dotnet run 135.181.140.212 13206 "uNRRL+6jLpjLRHZfCr2f8CLWQB5vcvI1Wc4NzK8VbFQ="
```

## Output Example

```
🚀 TON LiteClient Playground

📡 Connecting to 135.181.140.212:13206...

✅ Connected and ready!

📊 Step 1: Getting masterchain info...
   Latest block: seqno 53427620
   Workchain: -1
   Shard: 8000000000000000
   Root hash: E4D751AA7BAEDA51135A458ED5050072ECD7F35E15886915748C684D7BF17FF3
   File hash: BB4D0D3856887BAB68172BDCC16EAFBA9670A6AA65B31500B8191B1B1A3F17C2

🔍 Step 2: Getting all shards info...
   Found 0 shard(s)

📝 Listing transactions in masterchain block 53427620...
   Block: wc:-1, shard:8000000000000000, seqno:53427620
   Requested: 40
   Found: 3
   Incomplete: False

   Transactions:
     • Account (32 bytes): ...3333333333333333
       LT: 63036287000001
       Hash (32 bytes): 565CF7EE5C03DA62...

     • Account (32 bytes): ...3333333333333333
       LT: 63036287000002
       Hash (32 bytes): 81B3DC19692A55DD...

     • Account (32 bytes): ...5555555555555555
       LT: 63036287000003
       Hash (32 bytes): 720CF016D33F6E01...

✅ Done! Total transactions found: 3
```

