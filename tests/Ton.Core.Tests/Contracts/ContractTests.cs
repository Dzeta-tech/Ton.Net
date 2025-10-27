using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Tuple;
using Ton.Core.Types;

namespace Ton.Core.Tests.Contracts;

public class ContractTests
{
    [Test]
    public void Test_ContractState_Uninit()
    {
        ContractState state = new()
        {
            Balance = 0,
            ExtraCurrency = null,
            Last = null,
            State = new ContractState.AccountStateInfo.Uninit()
        };

        Assert.Multiple(() =>
        {
            Assert.That(state.Balance, Is.EqualTo(new BigInteger(0)));
            Assert.That(state.State, Is.InstanceOf<ContractState.AccountStateInfo.Uninit>());
            Assert.That(state.Last, Is.Null);
        });
    }

    [Test]
    public void Test_ContractState_Active()
    {
        byte[] code = [1, 2, 3];
        byte[] data = [4, 5, 6];

        ContractState state = new()
        {
            Balance = 1000000000,
            State = new ContractState.AccountStateInfo.Active(code, data),
            Last = new ContractState.LastTransaction(12345, [0xAA, 0xBB])
        };

        Assert.Multiple(() =>
        {
            Assert.That(state.Balance, Is.EqualTo(new BigInteger(1000000000)));
            Assert.That(state.State, Is.InstanceOf<ContractState.AccountStateInfo.Active>());
        });

        ContractState.AccountStateInfo.Active active = (ContractState.AccountStateInfo.Active)state.State;
        Assert.Multiple(() =>
        {
            Assert.That(active.Code, Is.Not.Null);
            Assert.That(active.Data, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(active.Code, Is.EquivalentTo(code));
            Assert.That(active.Data, Is.EquivalentTo(data));

            Assert.That(state.Last, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(state.Last!.Lt, Is.EqualTo(new BigInteger(12345)));
            Assert.That(state.Last.Hash, Is.EquivalentTo(new byte[] { 0xAA, 0xBB }));
        });
    }

    [Test]
    public void Test_ContractState_Frozen()
    {
        byte[] stateHash = new byte[32];
        Array.Fill(stateHash, (byte)0xFF);

        ContractState state = new()
        {
            Balance = 500000,
            State = new ContractState.AccountStateInfo.Frozen(stateHash)
        };

        Assert.That(state.State, Is.InstanceOf<ContractState.AccountStateInfo.Frozen>());

        ContractState.AccountStateInfo.Frozen frozen = (ContractState.AccountStateInfo.Frozen)state.State;
        Assert.That(frozen.StateHash, Is.EquivalentTo(stateHash));
    }

    [Test]
    public void Test_SenderArguments_Required_Fields()
    {
        SenderArguments args = new()
        {
            Value = 1000000,
            To = Address.Parse("EQCtW_zzk6n82ebaVQFq8P_04wOemYhtwqMd3NuArmPODRvD")
        };

        Assert.Multiple(() =>
        {
            Assert.That(args.Value, Is.EqualTo(new BigInteger(1000000)));
            Assert.That(args.To.ToString(), Is.EqualTo("EQCtW_zzk6n82ebaVQFq8P_04wOemYhtwqMd3NuArmPODRvD"));
            Assert.That(args.Body, Is.Null);
            Assert.That(args.Bounce, Is.Null);
            Assert.That(args.SendMode, Is.Null);
        });
    }

    [Test]
    public void Test_SenderArguments_All_Fields()
    {
        Cell body = Builder.BeginCell().StoreUint(0, 32).EndCell();
        StateInit init = new(
            Builder.BeginCell().EndCell(),
            Builder.BeginCell().EndCell()
        );

        SenderArguments args = new()
        {
            Value = 2000000,
            To = Address.Parse("EQCtW_zzk6n82ebaVQFq8P_04wOemYhtwqMd3NuArmPODRvD"),
            Body = body,
            Bounce = true,
            SendMode = SendMode.PayFeesSeparately,
            Init = init
        };

        Assert.Multiple(() =>
        {
            Assert.That(args.Value, Is.EqualTo(new BigInteger(2000000)));
            Assert.That(args.Body, Is.Not.Null);
            Assert.That(args.Bounce, Is.True);
            Assert.That(args.SendMode, Is.EqualTo(SendMode.PayFeesSeparately));
            Assert.That(args.Init, Is.Not.Null);
        });
    }

    [Test]
    public void Test_ContractGetMethodResult()
    {
        TupleBuilder builder = new();
        builder.WriteNumber(123);
        builder.WriteAddress(Address.Parse("EQCtW_zzk6n82ebaVQFq8P_04wOemYhtwqMd3NuArmPODRvD"));

        ContractGetMethodResult result = new()
        {
            Stack = new TupleReader(builder.Build()),
            GasUsed = new BigInteger(5000),
            Logs = "Test logs"
        };

        Assert.Multiple(() =>
        {
            Assert.That(result.Stack.ReadNumber(), Is.EqualTo(123));
            Assert.That(result.GasUsed, Is.EqualTo(new BigInteger(5000)));
            Assert.That(result.Logs, Is.EqualTo("Test logs"));
        });
    }

    [Test]
    public void Test_InternalMessageArgs()
    {
        InternalMessageArgs args = new()
        {
            Value = 100000,
            Bounce = false,
            SendMode = SendMode.IgnoreErrors
        };

        Assert.Multiple(() =>
        {
            Assert.That(args.Value, Is.EqualTo(new BigInteger(100000)));
            Assert.That(args.Bounce, Is.False);
            Assert.That(args.SendMode, Is.EqualTo(SendMode.IgnoreErrors));
        });
    }

    [Test]
    public void Test_ComputeError_Basic()
    {
        ComputeError error = new("Out of gas", 13, "debug logs", "execution logs");

        Assert.Multiple(() =>
        {
            Assert.That(error.Message, Is.EqualTo("Out of gas"));
            Assert.That(error.ExitCode, Is.EqualTo(13));
            Assert.That(error.DebugLogs, Is.EqualTo("debug logs"));
            Assert.That(error.Logs, Is.EqualTo("execution logs"));
        });
    }

    [Test]
    public void Test_ComputeError_FromExitCode()
    {
        ComputeError error = ComputeError.FromExitCode(13);
        Assert.Multiple(() =>
        {
            Assert.That(error.Message, Is.EqualTo("Out of gas"));
            Assert.That(error.ExitCode, Is.EqualTo(13));
        });

        error = ComputeError.FromExitCode(2);
        Assert.That(error.Message, Is.EqualTo("Stack underflow"));

        error = ComputeError.FromExitCode(36);
        Assert.That(error.Message, Is.EqualTo("Not enough TON"));

        error = ComputeError.FromExitCode(999);
        Assert.That(error.Message, Is.EqualTo("Exit code 999"));
    }

    [Test]
    public void Test_ComputeError_IsException()
    {
        ComputeError error = new("Test error", 1);
        Assert.That(error, Is.InstanceOf<Exception>());
    }

    [Test]
    public void Test_ContractABI_Simple()
    {
        ContractABI abi = new()
        {
            Name = "TestContract",
            Getters =
            [
                new ABIGetter("get_balance", 123)
            ]
        };

        Assert.Multiple(() =>
        {
            Assert.That(abi.Name, Is.EqualTo("TestContract"));
            Assert.That(abi.Getters, Has.Length.EqualTo(1));
        });
        Assert.Multiple(() =>
        {
            Assert.That(abi.Getters![0].Name, Is.EqualTo("get_balance"));
            Assert.That(abi.Getters[0].MethodId, Is.EqualTo(123));
        });
    }

    [Test]
    public void Test_ContractABI_TypeRef_Simple()
    {
        ABITypeRef.Simple typeRef = new("int", true, 257);

        Assert.Multiple(() =>
        {
            Assert.That(typeRef.Type, Is.EqualTo("int"));
            Assert.That(typeRef.Optional, Is.True);
            Assert.That(typeRef.Format, Is.EqualTo(257));
        });
    }

    [Test]
    public void Test_ContractABI_TypeRef_Dict()
    {
        ABITypeRef.Dict typeRef = new("int", "cell", KeyFormat: 32);

        Assert.Multiple(() =>
        {
            Assert.That(typeRef.Key, Is.EqualTo("int"));
            Assert.That(typeRef.Value, Is.EqualTo("cell"));
            Assert.That(typeRef.KeyFormat, Is.EqualTo(32));
        });
    }

    [Test]
    public void Test_ContractABI_Receiver()
    {
        ABIReceiver receiver = new("internal", new ABIReceiverMessage.Typed("Transfer"));

        Assert.Multiple(() =>
        {
            Assert.That(receiver.Receiver, Is.EqualTo("internal"));
            Assert.That(receiver.Message, Is.InstanceOf<ABIReceiverMessage.Typed>());
        });

        ABIReceiverMessage.Typed typed = (ABIReceiverMessage.Typed)receiver.Message;
        Assert.That(typed.Type, Is.EqualTo("Transfer"));
    }

    [Test]
    public void Test_ContractABI_ReceiverMessage_Types()
    {
        ABIReceiverMessage.Typed typed = new("MyMessage");
        Assert.That(typed, Is.InstanceOf<ABIReceiverMessage.Typed>());

        ABIReceiverMessage.Any any = new();
        Assert.That(any, Is.InstanceOf<ABIReceiverMessage.Any>());

        ABIReceiverMessage.Empty empty = new();
        Assert.That(empty, Is.InstanceOf<ABIReceiverMessage.Empty>());

        ABIReceiverMessage.Text text = new("Hello");
        Assert.That(text, Is.InstanceOf<ABIReceiverMessage.Text>());
        Assert.That(text.TextValue, Is.EqualTo("Hello"));
    }

    [Test]
    public void Test_ContractABI_Complete()
    {
        ContractABI abi = new()
        {
            Name = "CompleteContract",
            Types =
            [
                new ABIType(
                    "Transfer",
                    0x12345678,
                    [
                        new ABIField("to", new ABITypeRef.Simple("address")),
                        new ABIField("amount", new ABITypeRef.Simple("int", Format: 64))
                    ]
                )
            ],
            Errors = new Dictionary<int, ABIError>
            {
                { 100, new ABIError("Insufficient balance") },
                { 101, new ABIError("Invalid recipient") }
            },
            Getters =
            [
                new ABIGetter(
                    "get_wallet_data",
                    85143,
                    [
                        new ABIArgument("query_id", new ABITypeRef.Simple("int"))
                    ],
                    new ABITypeRef.Simple("tuple")
                )
            ],
            Receivers =
            [
                new ABIReceiver("internal", new ABIReceiverMessage.Typed("Transfer")),
                new ABIReceiver("external", new ABIReceiverMessage.Empty())
            ]
        };

        Assert.Multiple(() =>
        {
            Assert.That(abi.Name, Is.EqualTo("CompleteContract"));
            Assert.That(abi.Types, Has.Length.EqualTo(1));
            Assert.That(abi.Errors, Has.Count.EqualTo(2));
            Assert.That(abi.Getters, Has.Length.EqualTo(1));
            Assert.That(abi.Receivers, Has.Length.EqualTo(2));
        });

        Assert.Multiple(() =>
        {
            Assert.That(abi.Errors[100].Message, Is.EqualTo("Insufficient balance"));
            Assert.That(abi.Types![0].Header, Is.EqualTo(0x12345678));
            Assert.That(abi.Getters![0].Arguments![0].Name, Is.EqualTo("query_id"));
        });
    }
}