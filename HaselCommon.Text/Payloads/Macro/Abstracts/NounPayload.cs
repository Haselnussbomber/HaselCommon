using Dalamud.Game;
using HaselCommon.Utils;

namespace HaselCommon.Text.Payloads.Macro.Abstracts;

// <XXnoun(SheetName,Person,RowId[,Amount[,Case[,UnkInt5]]])>
// s . .
public abstract class NounPayload : MacroPayload
{
    private static readonly IntegerExpression DefaultPerson = new(5);
    private static readonly IntegerExpression DefaultAmount = new(1);
    private static readonly IntegerExpression DefaultCase = new(1);
    private static readonly IntegerExpression DefaultUnkInt5 = new(1);

    public abstract ClientLanguage Language { get; }

    public Expression? SheetName { get; set; }
    public Expression? Person { get; set; } = DefaultPerson;
    public Expression? RowId { get; set; }
    public Expression? Amount { get; set; } = DefaultAmount;
    public Expression? Case { get; set; } = DefaultCase;
    public Expression? UnkInt5 { get; set; } = DefaultUnkInt5;

    public override byte[] Encode()
    {
        if (SheetName == null || RowId == null)
            return [];

        return EncodeChunk(
            SheetName,
            Person ?? DefaultPerson,
            RowId,
            Amount ?? DefaultAmount,
            Case ?? DefaultCase,
            UnkInt5 ?? DefaultUnkInt5
        );
    }

    public override void Decode(BinaryReader reader)
    {
        if (reader.ReadByte() != START_BYTE)
            throw new Exception("Expected START_BYTE");

        if (reader.ReadByte() != (byte)Code)
            throw new Exception($"Expected MacroCode {Code} (0x{(byte)Code:X})");

        reader.ReadIntegerExpression();

        SheetName = Expression.Parse(reader.BaseStream);
        Person = Expression.Parse(reader.BaseStream);
        RowId = Expression.Parse(reader.BaseStream);

        if (!reader.IsEndOfChunk())
        {
            Amount = Expression.Parse(reader.BaseStream);

            if (!reader.IsEndOfChunk())
            {
                Case = Expression.Parse(reader.BaseStream);

                if (!reader.IsEndOfChunk())
                    UnkInt5 = Expression.Parse(reader.BaseStream);
            }
        }

        if (reader.ReadByte() != END_BYTE)
            throw new Exception("Expected END_BYTE");
    }

    public override SeString Resolve(List<Expression>? localParameters = null)
    {
        if (SheetName == null || RowId == null)
            return new SeString();

        var sheetName = SheetName.ResolveString(localParameters).ToString();
        var rowId = RowId.ResolveNumber(localParameters);

        var person = (Person ?? DefaultPerson).ResolveNumber(localParameters);
        var amount = (Amount ?? DefaultAmount).ResolveNumber(localParameters);
        var @case = (Case ?? DefaultCase).ResolveNumber(localParameters);
        var unkInt5 = (UnkInt5 ?? DefaultUnkInt5).ResolveNumber(localParameters);

        return Service.Get<TextDecoder>().ProcessNoun(Language, sheetName, person, rowId, amount, @case, unkInt5);
    }
}
