namespace HaselCommon.ImGuiYoga.Style;

public interface IInheritableProperty<T>
{
    void SetInherited();
    void SetValue(T? value);
    T Resolve();
}
