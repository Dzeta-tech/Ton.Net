using Ton.Adnl.Protocol;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.LiteClient.Models;

/// <summary>
///     Represents a list of transactions for an account
/// </summary>
public sealed class AccountTransactions
{
    /// <summary>
    ///     List of transactions
    /// </summary>
    public required List<Transaction> Transactions { get; init; }

    /// <summary>
    ///     Raw transaction BOC data
    /// </summary>
    public required byte[] TransactionsBoc { get; init; }

    /// <summary>
    ///     Creates AccountTransactions from ADNL protocol's LiteServerTransactionList
    /// </summary>
    public static AccountTransactions FromAdnl(LiteServerTransactionList adnlResponse)
    {
        List<Transaction> transactions = [];

        if (adnlResponse.Transactions.Length > 0)
        {
            Cell[] cells = Cell.FromBoc(adnlResponse.Transactions);
            foreach (Cell cell in cells)
            {
                Transaction tx = Transaction.Load(cell.BeginParse(), cell);
                transactions.Add(tx);
            }
        }

        return new AccountTransactions
        {
            Transactions = transactions,
            TransactionsBoc = adnlResponse.Transactions
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AccountTransactions(count:{Transactions.Count})";
    }
}