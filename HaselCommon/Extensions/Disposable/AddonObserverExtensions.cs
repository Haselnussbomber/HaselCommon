using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AddonObserverExtensions
{
    extension(AddonObserver addonObserver)
    {
        public IDisposable OnShow(AddonObserver.AddonShowDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => addonObserver.Show += handler,
                handler => addonObserver.Show -= handler,
                (AddonObserver.AddonShowDelegate)handler.Invoke
            );
        }

        public IDisposable OnShow(AddonObserver.AddonShowDelegate handler, string addonName)
        {
            void wrapper(AtkUnitBase* addon)
            {
                if (addon->NameString == addonName)
                    handler.Invoke(addon);
            }

            return EventExtensions.Subscribe(
                handler => addonObserver.Show += handler,
                handler => addonObserver.Show -= handler,
                (AddonObserver.AddonShowDelegate)wrapper
            );
        }

        public IDisposable OnShow(AddonObserver.AddonShowDelegate handler, params string[] addonNames)
        {
            void wrapper(AtkUnitBase* addon)
            {
                if (addonNames.Contains(addon->NameString))
                    handler.Invoke(addon);
            }

            return EventExtensions.Subscribe(
                handler => addonObserver.Show += handler,
                handler => addonObserver.Show -= handler,
                (AddonObserver.AddonShowDelegate)wrapper
            );
        }

        public IDisposable OnHide(AddonObserver.AddonHideDelegate handler)
        {
            return EventExtensions.Subscribe(
                handler => addonObserver.Hide += handler,
                handler => addonObserver.Hide -= handler,
                (AddonObserver.AddonHideDelegate)handler.Invoke
            );
        }

        public IDisposable OnHide(AddonObserver.AddonHideDelegate handler, string addonName)
        {
            void wrapper(AtkUnitBase* addon)
            {
                if (addon->NameString == addonName)
                    handler.Invoke(addon);
            }

            return EventExtensions.Subscribe(
                handler => addonObserver.Hide += handler,
                handler => addonObserver.Hide -= handler,
                (AddonObserver.AddonHideDelegate)wrapper
            );
        }

        public IDisposable OnHide(AddonObserver.AddonHideDelegate handler, params string[] addonNames)
        {
            void wrapper(AtkUnitBase* addon)
            {
                if (addonNames.Contains(addon->NameString))
                    handler.Invoke(addon);
            }

            return EventExtensions.Subscribe(
                handler => addonObserver.Hide += handler,
                handler => addonObserver.Hide -= handler,
                (AddonObserver.AddonHideDelegate)wrapper
            );
        }
    }
}
