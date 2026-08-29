using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace HaselCommon.Extensions;

public static partial class IFrameworkExtensions
{
    public delegate void UpdateDelegate(float delta);

    extension(IFramework framework)
    {
        public IDisposable OnUpdate(UpdateDelegate handler)
        {
            void wrapper(IFramework _)
            {
                unsafe
                {
                    var framework = Framework.Instance();
                    if (framework != null)
                        handler(framework->FrameDeltaTime);
                }
            }

            return EventExtensions.Subscribe(
                handler => framework.Update += handler,
                handler => framework.Update -= handler,
                (IFramework.OnUpdateDelegate)wrapper
            );
        }

        public IDisposable OnUpdate(Action handler)
        {
            void wrapper(IFramework _)
            {
                handler();
            }

            return EventExtensions.Subscribe(
                handler => framework.Update += handler,
                handler => framework.Update -= handler,
                (IFramework.OnUpdateDelegate)wrapper
            );
        }
    }
}
