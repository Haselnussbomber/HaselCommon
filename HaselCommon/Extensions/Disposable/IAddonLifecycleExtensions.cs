using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace HaselCommon.Extensions;

public static partial class IAddonLifecycleExtensions
{
    public delegate void AddonReceiveEventDelegate(AddonReceiveEventArgs args);
    public delegate void AddonOpenDelegate(AddonArgs args);
    public delegate void AddonCloseDelegate(AddonCloseArgs args);
    public delegate void AddonShowDelegate(AddonShowArgs args);
    public delegate void AddonHideDelegate(AddonHideArgs args);
    public delegate void AddonSetupDelegate(AddonSetupArgs args);
    public delegate void AddonRefreshDelegate(AddonRefreshArgs args);
    public delegate void AddonRequestedUpdateDelegate(AddonRequestedUpdateArgs args);
    public delegate void AddonFocusChangedDelegate(AddonFocusChangedArgs args);
    public delegate void AddonFinalizeDelegate(AddonArgs args);
    public delegate void AddonUpdateDelegate(AddonArgs args);
    public delegate void AddonDrawDelegate(AddonArgs args);
    public delegate void AddonMoveDelegate(AddonArgs args);
    public delegate void AddonMouseOverDelegate(AddonArgs args);
    public delegate void AddonMouseOutDelegate(AddonArgs args);
    public delegate void AddonFocusDelegate(AddonArgs args);

    extension(IAddonLifecycle addonLifecycle)
    {
        // ReceiveEvent (PreReceiveEvent, PostReceiveEvent)
        public IDisposable OnPreReceiveEvent(AddonReceiveEventDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonReceiveEventArgs>(AddonEvent.PreReceiveEvent, handler.Invoke, addonName);
        }

        public IDisposable OnPreReceiveEvent(AddonReceiveEventDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonReceiveEventArgs>(AddonEvent.PreReceiveEvent, handler.Invoke, addonNames);
        }

        public IDisposable OnPostReceiveEvent(AddonReceiveEventDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonReceiveEventArgs>(AddonEvent.PostReceiveEvent, handler.Invoke, addonName);
        }

        public IDisposable OnPostReceiveEvent(AddonReceiveEventDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonReceiveEventArgs>(AddonEvent.PostReceiveEvent, handler.Invoke, addonNames);
        }

        // Open (PreOpen, PostOpen)
        public IDisposable OnPreOpen(AddonOpenDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreOpen, handler.Invoke, addonName);
        }

        public IDisposable OnPreOpen(AddonOpenDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreOpen, handler.Invoke, addonNames);
        }

        public IDisposable OnPostOpen(AddonOpenDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostOpen, handler.Invoke, addonName);
        }

        public IDisposable OnPostOpen(AddonOpenDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostOpen, handler.Invoke, addonNames);
        }

        // Close (PreClose, PostClose)
        public IDisposable OnPreClose(AddonCloseDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonCloseArgs>(AddonEvent.PreClose, handler.Invoke, addonName);
        }

        public IDisposable OnPreClose(AddonCloseDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonCloseArgs>(AddonEvent.PreClose, handler.Invoke, addonNames);
        }

        public IDisposable OnPostClose(AddonCloseDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonCloseArgs>(AddonEvent.PostClose, handler.Invoke, addonName);
        }

        public IDisposable OnPostClose(AddonCloseDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonCloseArgs>(AddonEvent.PostClose, handler.Invoke, addonNames);
        }

        // Show (PreShow, PostShow)
        public IDisposable OnPreShow(AddonShowDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonShowArgs>(AddonEvent.PreShow, handler.Invoke, addonName);
        }

        public IDisposable OnPreShow(AddonShowDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonShowArgs>(AddonEvent.PreShow, handler.Invoke, addonNames);
        }

        public IDisposable OnPostShow(AddonShowDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonShowArgs>(AddonEvent.PostShow, handler.Invoke, addonName);
        }

        public IDisposable OnPostShow(AddonShowDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonShowArgs>(AddonEvent.PostShow, handler.Invoke, addonNames);
        }

        // Hide (PreHide, PostHide)
        public IDisposable OnPreHide(AddonHideDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonHideArgs>(AddonEvent.PreHide, handler.Invoke, addonName);
        }

        public IDisposable OnPreHide(AddonHideDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonHideArgs>(AddonEvent.PreHide, handler.Invoke, addonNames);
        }

        public IDisposable OnPostHide(AddonHideDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonHideArgs>(AddonEvent.PostHide, handler.Invoke, addonName);
        }

        public IDisposable OnPostHide(AddonHideDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonHideArgs>(AddonEvent.PostHide, handler.Invoke, addonNames);
        }

        // Setup (PreSetup, PostSetup)
        public IDisposable OnPreSetup(AddonSetupDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonSetupArgs>(AddonEvent.PreSetup, handler.Invoke, addonName);
        }

        public IDisposable OnPreSetup(AddonSetupDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonSetupArgs>(AddonEvent.PreSetup, handler.Invoke, addonNames);
        }

        public IDisposable OnPostSetup(AddonSetupDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonSetupArgs>(AddonEvent.PostSetup, handler.Invoke, addonName);
        }

        public IDisposable OnPostSetup(AddonSetupDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonSetupArgs>(AddonEvent.PostSetup, handler.Invoke, addonNames);
        }

        // Refresh (PreRefresh, PostRefresh)
        public IDisposable OnPreRefresh(AddonRefreshDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonRefreshArgs>(AddonEvent.PreRefresh, handler.Invoke, addonName);
        }

        public IDisposable OnPreRefresh(AddonRefreshDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonRefreshArgs>(AddonEvent.PreRefresh, handler.Invoke, addonNames);
        }

        public IDisposable OnPostRefresh(AddonRefreshDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonRefreshArgs>(AddonEvent.PostRefresh, handler.Invoke, addonName);
        }

        public IDisposable OnPostRefresh(AddonRefreshDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonRefreshArgs>(AddonEvent.PostRefresh, handler.Invoke, addonNames);
        }

        // RequestedUpdate (PreRequestedUpdate, PostRequestedUpdate)
        public IDisposable OnPreRequestedUpdate(AddonRequestedUpdateDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonRequestedUpdateArgs>(AddonEvent.PreRequestedUpdate, handler.Invoke, addonName);
        }

        public IDisposable OnPreRequestedUpdate(AddonRequestedUpdateDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonRequestedUpdateArgs>(AddonEvent.PreRequestedUpdate, handler.Invoke, addonNames);
        }

        public IDisposable OnPostRequestedUpdate(AddonRequestedUpdateDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonRequestedUpdateArgs>(AddonEvent.PostRequestedUpdate, handler.Invoke, addonName);
        }

        public IDisposable OnPostRequestedUpdate(AddonRequestedUpdateDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonRequestedUpdateArgs>(AddonEvent.PostRequestedUpdate, handler.Invoke, addonNames);
        }

        // FocusChanged (PreFocusChanged, PostFocusChanged)
        public IDisposable OnPreFocusChanged(AddonFocusChangedDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonFocusChangedArgs>(AddonEvent.PreFocusChanged, handler.Invoke, addonName);
        }

        public IDisposable OnPreFocusChanged(AddonFocusChangedDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonFocusChangedArgs>(AddonEvent.PreFocusChanged, handler.Invoke, addonNames);
        }

        public IDisposable OnPostFocusChanged(AddonFocusChangedDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonFocusChangedArgs>(AddonEvent.PostFocusChanged, handler.Invoke, addonName);
        }

        public IDisposable OnPostFocusChanged(AddonFocusChangedDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonFocusChangedArgs>(AddonEvent.PostFocusChanged, handler.Invoke, addonNames);
        }

        // PreFinalize
        public IDisposable OnPreFinalize(AddonFinalizeDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreFinalize, handler.Invoke, addonName);
        }

        public IDisposable OnPreFinalize(AddonFinalizeDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreFinalize, handler.Invoke, addonNames);
        }

        // Update (PreUpdate, PostUpdate)
        public IDisposable OnPreUpdate(AddonUpdateDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreUpdate, handler.Invoke, addonName);
        }

        public IDisposable OnPreUpdate(AddonUpdateDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreUpdate, handler.Invoke, addonNames);
        }

        public IDisposable OnPostUpdate(AddonUpdateDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostUpdate, handler.Invoke, addonName);
        }

        public IDisposable OnPostUpdate(AddonUpdateDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostUpdate, handler.Invoke, addonNames);
        }

        // Draw (PreDraw, PostDraw)
        public IDisposable OnPreDraw(AddonDrawDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreDraw, handler.Invoke, addonName);
        }

        public IDisposable OnPreDraw(AddonDrawDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreDraw, handler.Invoke, addonNames);
        }

        public IDisposable OnPostDraw(AddonDrawDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostDraw, handler.Invoke, addonName);
        }

        public IDisposable OnPostDraw(AddonDrawDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostDraw, handler.Invoke, addonNames);
        }

        // Move (PreMove, PostMove)
        public IDisposable OnPreMove(AddonMoveDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreMove, handler.Invoke, addonName);
        }

        public IDisposable OnPreMove(AddonMoveDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreMove, handler.Invoke, addonNames);
        }

        public IDisposable OnPostMove(AddonMoveDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostMove, handler.Invoke, addonName);
        }

        public IDisposable OnPostMove(AddonMoveDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostMove, handler.Invoke, addonNames);
        }

        // MouseOver (PreMouseOver, PostMouseOver)
        public IDisposable OnPreMouseOver(AddonMouseOverDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreMouseOver, handler.Invoke, addonName);
        }

        public IDisposable OnPreMouseOver(AddonMouseOverDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreMouseOver, handler.Invoke, addonNames);
        }

        public IDisposable OnPostMouseOver(AddonMouseOverDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostMouseOver, handler.Invoke, addonName);
        }

        public IDisposable OnPostMouseOver(AddonMouseOverDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostMouseOver, handler.Invoke, addonNames);
        }

        // MouseOut (PreMouseOut, PostMouseOut)
        public IDisposable OnPreMouseOut(AddonMouseOutDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreMouseOut, handler.Invoke, addonName);
        }

        public IDisposable OnPreMouseOut(AddonMouseOutDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreMouseOut, handler.Invoke, addonNames);
        }

        public IDisposable OnPostMouseOut(AddonMouseOutDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostMouseOut, handler.Invoke, addonName);
        }

        public IDisposable OnPostMouseOut(AddonMouseOutDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostMouseOut, handler.Invoke, addonNames);
        }

        // Focus (PreFocus, PostFocus)
        public IDisposable OnPreFocus(AddonFocusDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreFocus, handler.Invoke, addonName);
        }

        public IDisposable OnPreFocus(AddonFocusDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PreFocus, handler.Invoke, addonNames);
        }

        public IDisposable OnPostFocus(AddonFocusDelegate handler, string addonName)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostFocus, handler.Invoke, addonName);
        }

        public IDisposable OnPostFocus(AddonFocusDelegate handler, IEnumerable<string> addonNames)
        {
            return addonLifecycle.RegisterAddonListener<AddonArgs>(AddonEvent.PostFocus, handler.Invoke, addonNames);
        }

        private IDisposable RegisterAddonListener<TArgs>(AddonEvent eventType, Action<TArgs> handler, string addonName) where TArgs : AddonArgs
        {
            void wrapper(AddonEvent _, AddonArgs args)
            {
                handler((TArgs)args);
            }

            return EventExtensions.Subscribe(
                handler => addonLifecycle.RegisterListener(eventType, addonName, handler),
                handler => addonLifecycle.UnregisterListener(eventType, addonName, handler),
                (IAddonLifecycle.AddonEventDelegate)wrapper
            );
        }

        private IDisposable RegisterAddonListener<TArgs>(AddonEvent eventType, Action<TArgs> handler, IEnumerable<string> addonNames) where TArgs : AddonArgs
        {
            void wrapper(AddonEvent _, AddonArgs args)
            {
                handler((TArgs)args);
            }

            return EventExtensions.Subscribe(
                handler => addonLifecycle.RegisterListener(eventType, addonNames, handler),
                handler => addonLifecycle.UnregisterListener(eventType, addonNames, handler),
                (IAddonLifecycle.AddonEventDelegate)wrapper
            );
        }
    }
}
