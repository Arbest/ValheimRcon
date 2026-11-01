using System.Text;
using ValheimRcon.Commands.Container;

namespace ValheimRcon.ZDOInfo
{
    internal class ContainerZDOInfoProvider : ZDOInfoProviderBase<Container>
    {
        public override void AppendInfo(ZDO zdo, StringBuilder stringBuilder, bool detailed)
        {
            var prefab = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponentInChildren<Container>();
            var size = ContainerFields.GetSize(zdo, prefab);

            stringBuilder.Append("Container: ");
            var count = ContainerFields.LoadInventory(zdo, prefab).NrOfItems();
            if (count == 0)
            {
                stringBuilder.Append("<empty>");
            }
            else
            {
                stringBuilder.AppendFormat("{0} items", count);
            }
            stringBuilder.AppendFormat(", size {0}x{1}", size.x, size.y);
        }
    }
}