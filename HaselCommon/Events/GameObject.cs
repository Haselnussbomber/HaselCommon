using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using HaselCommon.Services.Events;
using Lumina.Text.ReadOnly;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace HaselCommon.Events;

internal class GameObject(IEventEmitter eventEmitter, GameObjectId objectId)
{
    private bool _initialValuesSet;

    // GameObject
    private string _name = string.Empty;
    private Vector3 _position;
    private float _rotation;
    private bool _isDead;
    private bool _isMounted;

    // Character
    private uint _health;
    private uint _maxHealth;
    private uint _mana;
    private uint _maxMana;
    private ushort _gatheringPoints;
    private ushort _maxGatheringPoints;
    private ushort _craftingPoints;
    private ushort _maxCraftingPoints;
    private ushort _titleId;
    private byte _classJob;
    private byte _level;
    private byte _shieldValue;
    private byte _onlineStatus;
    private bool _isWeaponDrawn;
    private bool _isOffhandDrawn;
    private bool _inCombat;
    private bool _isHostile;
    private bool _isCasting;
    private bool _isPartyMember;
    private bool _isAllianceMember;
    private bool _isFriend;

    internal unsafe void Update(CSGameObject* gameObject)
    {
        var ptr = (nint)gameObject;

        Update(ptr, ref _name, new ReadOnlySeStringSpan(gameObject->GetName()).ExtractText(), GameObjectEvents.NameChanged);
        Update(ptr, ref _position, gameObject->Position, GameObjectEvents.PositionChanged);
        Update(ptr, ref _rotation, gameObject->Rotation, GameObjectEvents.RotationChanged);
        Update(ptr, ref _isDead, gameObject->IsDead(), GameObjectEvents.IsDeadChanged);
        Update(ptr, ref _isMounted, !gameObject->IsNotMounted(), GameObjectEvents.IsMountedChanged);

        switch (gameObject->GetObjectKind())
        {
            case ObjectKind.Pc or ObjectKind.BattleNpc:
                var character = (CSCharacter*)gameObject;
                Update(ptr, ref _health, character->Health, GameObjectEvents.HealthChanged);
                Update(ptr, ref _maxHealth, character->MaxHealth, GameObjectEvents.MaxHealthChanged);
                Update(ptr, ref _mana, character->Mana, GameObjectEvents.ManaChanged);
                Update(ptr, ref _maxMana, character->MaxMana, GameObjectEvents.MaxManaChanged);
                Update(ptr, ref _gatheringPoints, character->GatheringPoints, GameObjectEvents.GatheringPointsChanged);
                Update(ptr, ref _maxGatheringPoints, character->MaxGatheringPoints, GameObjectEvents.MaxGatheringPointsChanged);
                Update(ptr, ref _craftingPoints, character->CraftingPoints, GameObjectEvents.CraftingPointsChanged);
                Update(ptr, ref _maxCraftingPoints, character->MaxCraftingPoints, GameObjectEvents.MaxCraftingPointsChanged);
                Update(ptr, ref _titleId, character->TitleId, GameObjectEvents.TitleIdChanged);
                Update(ptr, ref _classJob, character->ClassJob, GameObjectEvents.ClassJobChanged);
                Update(ptr, ref _level, character->Level, GameObjectEvents.LevelChanged);
                Update(ptr, ref _shieldValue, character->ShieldValue, GameObjectEvents.ShieldValueChanged);
                Update(ptr, ref _onlineStatus, character->OnlineStatus, GameObjectEvents.OnlineStatusChanged);
                Update(ptr, ref _isWeaponDrawn, character->IsWeaponDrawn, GameObjectEvents.IsWeaponDrawnChanged);
                Update(ptr, ref _isOffhandDrawn, character->IsOffhandDrawn, GameObjectEvents.IsOffhandDrawnChanged);
                Update(ptr, ref _inCombat, character->InCombat, GameObjectEvents.InCombatChanged);
                Update(ptr, ref _isHostile, character->IsHostile, GameObjectEvents.IsHostileChanged);
                Update(ptr, ref _isPartyMember, character->IsPartyMember, GameObjectEvents.IsPartyMemberChanged);
                Update(ptr, ref _isAllianceMember, character->IsAllianceMember, GameObjectEvents.IsAllianceMemberChanged);
                Update(ptr, ref _isFriend, character->IsFriend, GameObjectEvents.IsFriendChanged);

                // TODO: GetStatusManager

                var castInfo = character->GetCastInfo();
                if (castInfo != null && _isCasting != character->IsCasting)
                {
                    if (character->IsCasting)
                    {
                        eventEmitter.TriggerEvent(GameObjectEvents.StartedCasting, new GameObjectEvents.GameObjectStartedCastingEventArgs
                        {
                            ObjectId = objectId,
                            Pointer = ptr,
                            ActionType = castInfo->ActionType,
                            ActionId = castInfo->ActionId,
                            TargetId = castInfo->TargetId,
                        });

                        _isCasting = true;
                    }
                    else
                    {
                        eventEmitter.TriggerEvent(GameObjectEvents.StoppedCasting, new GameObjectEvents.GameObjectChangedEventArgs
                        {
                            ObjectId = objectId,
                            Pointer = ptr,
                        });

                        _isCasting = false;
                    }
                }

                break;
        }

        if (!_initialValuesSet)
            _initialValuesSet = true;
    }

    private void Update<T>(nint pointer, ref T oldValue, T newValue, string eventName) where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            if (_initialValuesSet)
            {
                eventEmitter.TriggerEvent(eventName, new GameObjectEvents.GameObjectChangedEventArgs<T>
                {
                    ObjectId = objectId,
                    Pointer = pointer,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }

            oldValue = newValue;
        }
    }
}
