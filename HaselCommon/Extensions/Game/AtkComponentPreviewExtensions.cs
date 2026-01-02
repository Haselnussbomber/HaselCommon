using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AtkComponentPreviewExtensions
{
    extension(ref AtkComponentPreview component)
    {
        public AtkNineGridNode* BorderNode => component.GetNineGridNodeById(3);
        public AtkImageNode* ImageNode => component.GetImageNodeById(4);

        public Vector2 Size
        {
            get
            {
                var ownerNode = component.OwnerNode;
                if (ownerNode == null)
                    return Vector2.Zero;

                return new(ownerNode->GetWidth(), ownerNode->GetHeight());
            }

            set
            {
                var ownerNode = component.OwnerNode;
                if (ownerNode == null)
                    return;

                var width = (ushort)value.X;
                var height = (ushort)value.Y;

                ownerNode->SetWidth(width);
                ownerNode->SetHeight(height);

                var border = component.BorderNode;
                if (border != null)
                {
                    border->SetWidth((ushort)(width + 8));
                    border->SetHeight((ushort)(height + 10));
                }

                var image = component.ImageNode;
                if (image != null)
                {
                    image->SetWidth(width);
                    image->SetHeight(height);
                }
            }
        }
    }
}
