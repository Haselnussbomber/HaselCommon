using HaselCommon.Game.Enums;

namespace HaselCommon.Extensions;

public static class ItemActionTypeExtensions
{
    extension(ItemActionType value)
    {
        public bool IsUnlockable => value
            is ItemActionType.Companion
            or ItemActionType.BuddyEquip
            or ItemActionType.Mount
            or ItemActionType.SecretRecipeBook
            or ItemActionType.UnlockLink
            or ItemActionType.TripleTriadCard
            or ItemActionType.FolkloreTome
            or ItemActionType.OrchestrionRoll
            or ItemActionType.FramersKit
            or ItemActionType.Ornament
            or ItemActionType.Glasses
            or ItemActionType.OccultRecords
            or ItemActionType.SoulShards;
    }
}
