using Dalamud.Game.ClientState;

namespace HaselCommon.Extensions;

public static partial class IClientStateExtensions
{
    public delegate void ZoneInitDelegate(ZoneInitEventArgs args);
    public delegate void TerritoryChangeDelegate(uint territoryTypeId);
    public delegate void MapChangeDelegate(uint mapId);
    public delegate void InstanceChangeDelegate(uint instance);
    public delegate void ClassJobChangeDelegate(uint classJobId);
    public delegate void LevelChangeDelegate(uint classJobId, uint level);
    public delegate void LoginDelegate();
    public delegate void LogoutDelegate(int type, int code);
    public delegate void EnterPvPDelegate();
    public delegate void LeavePvPDelegate();
    public delegate void ContentsFinderPoppedDelegate(uint cfcId);

    extension(IClientState clientState)
    {
        public IDisposable OnZoneInit(ZoneInitDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.ZoneInit += handler,
                handler => clientState.ZoneInit -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnTerritoryChange(TerritoryChangeDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.TerritoryChanged += handler,
                handler => clientState.TerritoryChanged -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnMapChange(MapChangeDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.MapIdChanged += handler,
                handler => clientState.MapIdChanged -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnInstanceChange(InstanceChangeDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.InstanceChanged += handler,
                handler => clientState.InstanceChanged -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnClassJobChange(ClassJobChangeDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.ClassJobChanged += handler,
                handler => clientState.ClassJobChanged -= handler,
                (IClientState.ClassJobChangeDelegate)handler.Invoke
            );
        }

        public IDisposable OnLevelChange(LevelChangeDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.LevelChanged += handler,
                handler => clientState.LevelChanged -= handler,
                (IClientState.LevelChangeDelegate)handler.Invoke
            );
        }

        public IDisposable OnLogin(LoginDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.Login += handler,
                handler => clientState.Login -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnLogout(Action handler)
        {
            void wrapper(int type, int code)
            {
                handler();
            }

            return EventExtensions.Subscribe(
                handler => clientState.Logout += handler,
                handler => clientState.Logout -= handler,
                (IClientState.LogoutDelegate)wrapper
            );
        }

        public IDisposable OnLogout(LogoutDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.Logout += handler,
                handler => clientState.Logout -= handler,
                (IClientState.LogoutDelegate)handler.Invoke
            );
        }

        public IDisposable OnEnterPvP(EnterPvPDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.EnterPvP += handler,
                handler => clientState.EnterPvP -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnLeavePvP(LeavePvPDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => clientState.LeavePvP += handler,
                handler => clientState.LeavePvP -= handler,
                handler.Invoke
            );
        }

        public IDisposable OnContentsFinderPopped(ContentsFinderPoppedDelegate handler)
        {
            void wrapper(ContentFinderCondition cfc)
            {
                handler(cfc.RowId);
            }

            return EventExtensions.Subscribe(
                handler => clientState.CfPop += handler,
                handler => clientState.CfPop -= handler,
                wrapper
            );
        }
    }
}
