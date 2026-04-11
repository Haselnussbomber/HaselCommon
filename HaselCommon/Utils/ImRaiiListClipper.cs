namespace HaselCommon.Utils;

public readonly ref struct ImRaiiListClipper
{
    public readonly ImGuiListClipperPtr ClipperPtr = ImGui.ImGuiListClipper();

    public ImRaiiListClipper(int itemsCount, float itemsHeight = -1.0f)
    {
        ClipperPtr.Begin(itemsCount, itemsHeight);
    }

    public readonly ref int DisplayStart => ref ClipperPtr.DisplayStart;
    public readonly ref int DisplayEnd => ref ClipperPtr.DisplayEnd;
    public readonly ref int ItemsCount => ref ClipperPtr.ItemsCount;
    public readonly ref float ItemsHeight => ref ClipperPtr.ItemsHeight;
    public readonly ref float StartPosY => ref ClipperPtr.StartPosY;

    public void Begin(int itemsCount, float itemsHeight = -1.0f)
    {
        ClipperPtr.Begin(itemsCount, itemsHeight);
    }

    public bool Step()
    {
        return ClipperPtr.Step();
    }

    public void End()
    {
        ClipperPtr.End();
    }

    public void ForceDisplayRangeByIndices(int itemMin, int itemMax)
    {
        ClipperPtr.ForceDisplayRangeByIndices(itemMin, itemMax);
    }

    public void Dispose()
    {
        ClipperPtr.End();
    }

    public Enumerator GetEnumerator() => new Enumerator(ClipperPtr);

    public ref struct Enumerator(ImGuiListClipperPtr clipper)
    {
        public int Current { get; private set; } = -1;

        public bool MoveNext()
        {
            if (Current != -1 && Current < clipper.DisplayEnd - 1)
            {
                Current++;
                return true;
            }

            if (!clipper.Step())
                return false;

            Current = clipper.DisplayStart;

            if (Current >= clipper.DisplayEnd)
                return MoveNext();

            return true;
        }
    }
}
