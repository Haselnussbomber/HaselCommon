using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using HaselCommon.Game.Enums;
using HaselCommon.Sheets.Internal;
using ImGuiNET;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using Ornament = Lumina.Excel.GeneratedSheets.Ornament;

namespace HaselCommon.Services;

public class ImGuiService(
    IDalamudPluginInterface PluginInterface,
    IDataManager DataManager,
    ITextureProvider TextureProvider,
    TextureService TextureService,
    ExcelService ExcelService,
    TextService TextService,
    ImGuiContextMenuService ImGuiContextMenuService,
    ItemService ItemService) : IDisposable
{
    private readonly Dictionary<uint, Vector2?> _iconSizeCache = [];
    private readonly Lazy<IFontHandle> _tripleTriadNumberFont = new(() => PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 208f / 10f)));

    public void Dispose()
    {
        if (_tripleTriadNumberFont.IsValueCreated)
            _tripleTriadNumberFont.Value.Dispose();

        GC.SuppressFinalize(this);
    }

    private uint GetHairstyleIconId(uint unlockItem, byte tribeId, byte sexId)
    {
        return ExcelService
            .FindRow<HairMakeTypeCustom>(t => t!.Tribe.Row == tribeId && t.Gender == sexId)?
            .HairStyles
            .FirstOrDefault(h => h.Value?.HintItem.Row == unlockItem)?
            .Value?
            .Icon ?? 0;
    }

    public unsafe bool DrawSelectableItem(Item item, ImGuiId id, bool drawIcon = true, bool isHq = false, float? iconSize = null)
    {
        var itemName = TextService.GetItemName(item.RowId);
        var isHovered = false;
        iconSize ??= ImGui.GetTextLineHeight();

        if (drawIcon)
        {
            TextureService.DrawIcon(item.Icon, isHq, (float)iconSize);
            isHovered |= ImGui.IsItemHovered();
            ImGui.SameLine();
        }
        var clicked = ImGui.Selectable(itemName);
        isHovered |= ImGui.IsItemHovered();

        if (isHovered && !ImGui.IsKeyDown(ImGuiKey.LeftAlt))
        {
            if (item.ItemAction.Value?.Type == (uint)ItemActionType.Mount)
            {
                using var tooltip = ImRaii.Tooltip();
                var mount = ExcelService.GetRow<Mount>(item.ItemAction.Value!.Data[0])!;
                TextureService.DrawIcon(64000 + mount.Icon, 192);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Companion)
            {
                using var tooltip = ImRaii.Tooltip();
                var companion = ExcelService.GetRow<Companion>(item.ItemAction.Value!.Data[0])!;
                TextureService.DrawIcon(64000 + companion.Icon, 192);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Ornament)
            {
                using var tooltip = ImRaii.Tooltip();
                var ornament = ExcelService.GetRow<Ornament>(item.ItemAction.Value!.Data[0])!;
                TextureService.DrawIcon(59000 + ornament.Icon, 192);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.UnlockLink && item.ItemAction.Value?.Data[1] == 5211) // Emotes
            {
                using var tooltip = ImRaii.Tooltip();
                var emote = ExcelService.GetRow<Emote>(item.ItemAction.Value!.Data[2])!;
                TextureService.DrawIcon(emote.Icon, 80);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.TripleTriadCard)
            {
                var cardId = item.ItemAction.Value!.Data[0];
                var cardRow = ExcelService.GetRow<TripleTriadCard>(cardId)!;
                var cardResident = ExcelService.GetRow<TripleTriadCardResident>(cardId)!;
                var cardRarity = cardResident.TripleTriadCardRarity.Value!;

                var cardSize = new Vector2(208, 256);
                var cardSizeScaled = ImGuiHelpers.ScaledVector2(cardSize.X, cardSize.Y);

                using var tooltip = ImRaii.Tooltip();
                ImGui.TextUnformatted($"{(cardResident.TripleTriadCardRarity.Row == 5 ? "Ex" : "No")}. {cardResident.Order} - {cardRow.Name}");
                var pos = ImGui.GetCursorPos();
                TextureService.DrawPart("CardTripleTriad", 1, 0, cardSizeScaled);
                ImGui.SetCursorPos(pos);
                TextureService.DrawIcon(87000 + cardRow.RowId, cardSizeScaled);

                var starSize = cardSizeScaled.Y / 10f;
                var starCenter = pos + new Vector2(starSize);
                var starRadius = starSize / 1.666f;

                static Vector2 GetPosOnCircle(float radius, int index, int numberOfPoints)
                {
                    var angleIncrement = 2 * MathF.PI / numberOfPoints;
                    var angle = index * angleIncrement - MathF.PI / 2;
                    return new Vector2(
                        radius * MathF.Cos(angle),
                        radius * MathF.Sin(angle)
                    );
                }

                if (cardRarity.Stars >= 1)
                {
                    ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 0, 5)); // top
                    TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);

                    if (cardRarity.Stars >= 2)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 4, 5)); // left
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                    if (cardRarity.Stars >= 3)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 1, 5)); // right
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                    if (cardRarity.Stars >= 4)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 3, 5)); // bottom right
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                    if (cardRarity.Stars >= 5)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 2, 5)); // bottom left
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                }

                // type
                if (cardResident.TripleTriadCardType.Row != 0)
                {
                    ImGui.SetCursorPos(pos + new Vector2(cardSize.X, 0) - new Vector2(starSize * 1.5f, -starSize / 2f));
                    TextureService.DrawPart("CardTripleTriad", 1, cardResident.TripleTriadCardType.Row + 2, starSize);
                }

                // numbers
                using var font = _tripleTriadNumberFont.Value.Push();

                var numberText = $"{cardResident.Top:X}";
                var numberTextSize = ImGui.CalcTextSize(numberText);
                var numberTextWidth = numberTextSize.X / 1.333f;
                var numberCenter = pos + new Vector2(cardSizeScaled.X / 2f - numberTextWidth, cardSizeScaled.Y - numberTextSize.Y * 2f);

                static void DrawNumberText(Vector2 numberCenter, float numberTextWidth, int posIndex, string numberText)
                {
                    // shadow
                    ImGui.SetCursorPos(numberCenter + GetPosOnCircle(numberTextWidth, posIndex, 4) + ImGuiHelpers.ScaledVector2(2));
                    using (ImRaii.PushColor(ImGuiCol.Text, 0xFF000000))
                        ImGui.TextUnformatted(numberText);

                    // text
                    ImGui.SetCursorPos(numberCenter + GetPosOnCircle(numberTextWidth, posIndex, 4));
                    ImGui.TextUnformatted(numberText);
                }

                DrawNumberText(numberCenter, numberTextWidth, 0, numberText); // top
                DrawNumberText(numberCenter, numberTextWidth, 1, $"{cardResident.Right:X}"); // right
                DrawNumberText(numberCenter, numberTextWidth, 2, $"{cardResident.Left:X}"); // left
                DrawNumberText(numberCenter, numberTextWidth, 3, $"{cardResident.Bottom:X}"); // bottom
            }
            else if (item.ItemUICategory.Row == 95) // Paintings
            {
                var pictureId = (uint)ExcelService.GetRow<Picture>(item.AdditionalData)!.Image;

                if (!_iconSizeCache.TryGetValue(pictureId, out var size))
                {
                    var iconPath = TextureProvider.GetIconPath(pictureId);
                    if (string.IsNullOrEmpty(iconPath))
                    {
                        _iconSizeCache.Add(pictureId, null);
                    }
                    else
                    {
                        var file = DataManager.GetFile<TexFile>(iconPath);
                        _iconSizeCache.Add(pictureId, size = file != null ? new(file.Header.Width, file.Header.Height) : null);
                    }
                }

                if (size != null)
                {
                    using var tooltip = ImRaii.Tooltip();
                    TextureService.DrawIcon(pictureId, (Vector2)size * 0.5f);
                }
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.UnlockLink && ExcelService.FindRow<CharaMakeCustomize>(row => row?.HintItem.Row == item.RowId) != null) // Hairstyles etc.
            {
                byte tribeId = 1;
                byte sex = 1;
                unsafe
                {
                    var character = Control.GetLocalPlayer();
                    if (character != null)
                    {
                        tribeId = character->DrawData.CustomizeData.Tribe;
                        sex = character->DrawData.CustomizeData.Sex;
                    }
                }

                var hairStyleIconId = GetHairstyleIconId(item.RowId, tribeId, sex);
                if (hairStyleIconId != 0)
                {
                    using var tooltip = ImRaii.Tooltip();
                    TextureService.DrawIcon(hairStyleIconId, 192);
                }
            }
            else
            {
                using var tooltip = ImRaii.Tooltip();
                TextureService.DrawIcon(item.Icon, 64);
            }
        }

        ImGuiContextMenuService.Draw($"##{id}_ItemContextMenu{item.RowId}_IconTooltip", builder =>
        {
            builder.AddTryOn(item);
            builder.AddItemFinder(item.RowId);
            builder.AddCopyItemName(item.RowId);
            builder.AddItemSearch(item);
            builder.AddOpenOnGarlandTools("item", item.RowId);
        });

        if (ItemService.IsUnlockable(item) && ItemService.IsUnlocked(item))
        {
            ImGui.SameLine(1, 0);

            if (TextureProvider.GetFromGame("ui/uld/RecipeNoteBook_hr1.tex").TryGetWrap(out var tex, out _))
            {
                var pos = ImGui.GetCursorScreenPos() + new Vector2((float)iconSize / 2f);
                ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, pos, pos + new Vector2((float)iconSize / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }

        return clicked;
    }
}
