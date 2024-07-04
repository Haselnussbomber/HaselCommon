using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

[Obsolete]
public class ExtendedSpecialShop : SpecialShop
{
    private const int NumItems = 60;

    [StructLayout(LayoutKind.Explicit, Size = StructSize)]
    public struct SpecialShopItem
    {
        public const int StructSize = 0x60; // to update just diff between column 2 and 1

        [FieldOffset(0x4)] public uint ReceiveCount1;
        [FieldOffset(0x8)] public uint ReceiveCount2;
        [FieldOffset(0xC)] public uint GiveCount1;
        [FieldOffset(0x10)] public uint GiveCount2;
        [FieldOffset(0x14)] public uint GiveCount3;
        [FieldOffset(0x18)] public int ReceiveItemId1;
        [FieldOffset(0x1C)] public int ReceiveItemId2;
        [FieldOffset(0x20)] public int ReceiveSpecialShopItemCategory1;
        [FieldOffset(0x24)] public int ReceiveSpecialShopItemCategory2;
        [FieldOffset(0x28)] public int GiveItemId1;
        [FieldOffset(0x2C)] public int GiveItemId2;
        [FieldOffset(0x30)] public int GiveItemId3;
        [FieldOffset(0x34)] public int UnlockQuest;
        [FieldOffset(0x38)] public int Unk38;
        [FieldOffset(0x3C)] public int Unk3C;
        [FieldOffset(0x40)] public ushort Unk40;
        [FieldOffset(0x44)] public ushort Unk44;
        [FieldOffset(0x48)] public byte Unk48;
        [FieldOffset(0x4C)] public byte Unk4C;
        [FieldOffset(0x50)] public ushort Unk50; // related to Give1?
        [FieldOffset(0x52)] public ushort Unk52; // related to Give2?
        [FieldOffset(0x54)] public ushort Unk54; // related to Give2?
        [FieldOffset(0x56)] public ushort PatchNumber;
        [FieldOffset(0x58)] public byte Unk58; // related to Give1?
        [FieldOffset(0x59)] public byte Unk59; // related to Give2?
        [FieldOffset(0x5A)] public byte Unk5A; // related to Give3?
        [FieldOffset(0x5B)] public byte Unk5B;
        [FieldOffset(0x5C)] public byte Unk5C;
        [FieldOffset(0x5D)] public byte Unk5D;
        [FieldOffset(0x5E)] public byte Unk5E;
        [FieldOffset(0x5F)] public byte Unk5F;
        [FieldOffset(0x60)] public byte SortKey;
        [FieldOffset(0x61)] public bool Unk61; // related to Receive1?
        [FieldOffset(0x62)] public bool Unk62; // related to Receive2?

        public unsafe void Read(int index, RowParser parser)
        {
            ReceiveCount1 = parser.ReadOffset<uint>((ushort)(0x4 + StructSize * index));
            ReceiveCount2 = parser.ReadOffset<uint>((ushort)(0x8 + StructSize * index));
            GiveCount1 = parser.ReadOffset<uint>((ushort)(0xC + StructSize * index));
            GiveCount2 = parser.ReadOffset<uint>((ushort)(0x10 + StructSize * index));
            GiveCount3 = parser.ReadOffset<uint>((ushort)(0x14 + StructSize * index));
            ReceiveItemId1 = parser.ReadOffset<int>((ushort)(0x18 + StructSize * index));
            ReceiveItemId2 = parser.ReadOffset<int>((ushort)(0x1C + StructSize * index));
            ReceiveSpecialShopItemCategory1 = parser.ReadOffset<int>((ushort)(0x20 + StructSize * index));
            ReceiveSpecialShopItemCategory2 = parser.ReadOffset<int>((ushort)(0x24 + StructSize * index));
            GiveItemId1 = parser.ReadOffset<int>((ushort)(0x28 + StructSize * index));
            GiveItemId2 = parser.ReadOffset<int>((ushort)(0x2C + StructSize * index));
            GiveItemId3 = parser.ReadOffset<int>((ushort)(0x30 + StructSize * index));
            UnlockQuest = parser.ReadOffset<int>((ushort)(0x34 + StructSize * index));
            Unk38 = parser.ReadOffset<int>((ushort)(0x38 + StructSize * index));
            Unk3C = parser.ReadOffset<int>((ushort)(0x3C + StructSize * index));
            Unk40 = parser.ReadOffset<ushort>((ushort)(0x40 + StructSize * index));
            Unk44 = parser.ReadOffset<ushort>((ushort)(0x44 + StructSize * index));
            Unk48 = parser.ReadOffset<byte>((ushort)(0x48 + StructSize * index));
            Unk4C = parser.ReadOffset<byte>((ushort)(0x4C + StructSize * index));
            Unk50 = parser.ReadOffset<ushort>((ushort)(0x50 + StructSize * index));
            Unk52 = parser.ReadOffset<ushort>((ushort)(0x52 + StructSize * index));
            Unk54 = parser.ReadOffset<ushort>((ushort)(0x54 + StructSize * index));
            PatchNumber = parser.ReadOffset<ushort>((ushort)(0x56 + StructSize * index));
            Unk58 = parser.ReadOffset<byte>((ushort)(0x58 + StructSize * index));
            Unk59 = parser.ReadOffset<byte>((ushort)(0x59 + StructSize * index));
            Unk5A = parser.ReadOffset<byte>((ushort)(0x5A + StructSize * index));
            Unk5B = parser.ReadOffset<byte>((ushort)(0x5B + StructSize * index));
            Unk5C = parser.ReadOffset<byte>((ushort)(0x5C + StructSize * index));
            Unk5D = parser.ReadOffset<byte>((ushort)(0x5D + StructSize * index));
            Unk5E = parser.ReadOffset<byte>((ushort)(0x5E + StructSize * index));
            Unk5F = parser.ReadOffset<byte>((ushort)(0x5F + StructSize * index));
            SortKey = parser.ReadOffset<byte>((ushort)(0x60 + StructSize * index));
            Unk61 = parser.ReadOffset<bool>((ushort)(0x61 + StructSize * index));
            Unk62 = parser.ReadOffset<bool>((ushort)(0x62 + StructSize * index));
        }
    }

    public SpecialShopItem[] Items { get; set; } = null!;

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Items = new SpecialShopItem[NumItems];
        for (var i = 0; i < NumItems; i++)
        {
            Items[i] = new SpecialShopItem();
            Items[i].Read(i, parser);
        }
    }
}
