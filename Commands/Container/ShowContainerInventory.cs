using System.Text;
using ValheimRcon.ZDOInfo;

namespace ValheimRcon.Commands.Container
{
    internal class ShowContainerInventory : ContainerRconCommand
    {
        public override string Command => "showContainer";

        public override string Description => "Shows inventory contents of a container by object ID. " +
            "Usage: showContainer <id:userid>";

        protected override bool ChangesItems => false;

        protected override string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName)
        {
            var sb = new StringBuilder();
            if (inventory.NrOfItems() == 0)
            {
                sb.AppendFormat("Container is empty, {0}.", ContainerFields.SizeLabel(inventory));
                AppendDropped(sb);
                return sb.ToString();
            }

            ZDOInfoUtil.AppendInfo(zdo, sb);
            sb.AppendLine();
            sb.AppendFormat("Items ({0}):", inventory.NrOfItems());

            var allItems = inventory.GetAllItems();
            var itemIndex = 0;
            foreach (var item in allItems)
            {
                sb.AppendLine();
                var itemName = item.m_dropPrefab?.name ?? item.m_shared.m_name;
                sb.AppendFormat("[{0}] {1} ", itemIndex, itemName);
                ZDOInfoUtil.AppendItemInfo(item, sb);
                itemIndex++;
            }

            AppendDropped(sb);
            return sb.ToString().TrimEnd();
        }

        private void AppendDropped(StringBuilder sb)
        {
            if (DroppedOnLoad > 0)
            {
                sb.AppendLine();
                sb.AppendFormat("Not listed: {0} stored items outside the grid.", DroppedOnLoad);
            }
        }
    }
}