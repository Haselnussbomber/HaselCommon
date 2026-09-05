namespace HaselCommon.Extensions;

[GenerateEventSubscribers<IClientState>]
public static partial class IClientStateExtensions
{
    public delegate void ContentsFinderPoppedDelegate(uint cfcId);

    extension(IClientState clientState)
    {
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
