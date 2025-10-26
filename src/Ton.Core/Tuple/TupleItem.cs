using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Tuple;

/// <summary>
///     Base class for all tuple item types.
/// </summary>
public abstract class TupleItem
{
    /// <summary>
    ///     Gets the type name of this tuple item.
    /// </summary>
    public abstract string Type { get; }
}

/// <summary>
///     Represents a null value in a tuple.
/// </summary>
public class TupleItemNull : TupleItem
{
    public static readonly TupleItemNull Instance = new();

    TupleItemNull()
    {
    }

    public override string Type => "null";
}

/// <summary>
///     Represents an integer value in a tuple.
/// </summary>
public class TupleItemInt(BigInteger value) : TupleItem
{
    public override string Type => "int";

    /// <summary>
    ///     Gets the integer value.
    /// </summary>
    public BigInteger Value { get; } = value;
}

/// <summary>
///     Represents a NaN (Not a Number) value in a tuple.
/// </summary>
public class TupleItemNaN : TupleItem
{
    public static readonly TupleItemNaN Instance = new();

    TupleItemNaN()
    {
    }

    public override string Type => "nan";
}

/// <summary>
///     Represents a cell in a tuple.
/// </summary>
public class TupleItemCell(Cell cell) : TupleItem
{
    public override string Type => "cell";

    /// <summary>
    ///     Gets the cell.
    /// </summary>
    public Cell Cell { get; } = cell;
}

/// <summary>
///     Represents a slice in a tuple.
/// </summary>
public class TupleItemSlice(Cell cell) : TupleItem
{
    public override string Type => "slice";

    /// <summary>
    ///     Gets the cell backing this slice.
    /// </summary>
    public Cell Cell { get; } = cell;
}

/// <summary>
///     Represents a builder in a tuple.
/// </summary>
public class TupleItemBuilder(Cell cell) : TupleItem
{
    public override string Type => "builder";

    /// <summary>
    ///     Gets the cell backing this builder.
    /// </summary>
    public Cell Cell { get; } = cell;
}

/// <summary>
///     Represents a nested tuple.
/// </summary>
public class TupleItemTuple(TupleItem[] items) : TupleItem
{
    public override string Type => "tuple";

    /// <summary>
    ///     Gets the items in this tuple.
    /// </summary>
    public TupleItem[] Items { get; } = items;
}