using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Structs.Internal;

// ctor "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B D9 E8 ?? ?? ?? ?? 33 ED 48 8D 05 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 48 89 03 48 89 AB"
[StructLayout(LayoutKind.Explicit, Size = 0x3EE0)]
internal unsafe partial struct AddonItemSearch
{
    [FieldOffset(0)] public AtkUnitBase AtkUnitBase;

    [FieldOffset(0x230)] public SearchMode Mode;
    [FieldOffset(0x234)] public uint SelectedFilter;

    [FieldOffset(0x2DB0)] public AtkComponentTextInput* TextInput;
    [FieldOffset(0x2DB8)] public AtkComponentButton* SearchButton;

    [FieldOffset(0x3EDB)] public bool PartialMatch;

    [MemberFunction("E8 ?? ?? ?? ?? 48 8B DE 48 8D BC 24")]
    internal partial void RunSearch(bool ignoreFilters = false);

    [MemberFunction("E8 ?? ?? ?? ?? EB 40 41 8D 40 FD")]
    internal partial void SetModeFilter(SearchMode mode, int filter);

    public enum SearchMode : uint
    {
        Normal = 0,
        ArmsFilter = 1,
        EquipmentFilter = 2,
        ItemsFilter = 3,
        HousingFilter = 4,
        Wishlist = 5,
        Favorites = 6,
    }
}
