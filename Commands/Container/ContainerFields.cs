namespace ValheimRcon.Commands.Container
{
    // Per-object overrides of Container fields, read the way vanilla ZNetView.LoadFields applies
    // them on clients: only when the object carries "HasFields" and the component "HasFieldsContainer".
    // Mods such as ContainerSizes resize containers through these overrides.
    internal static class ContainerFields
    {
        private const string Component = "Container";
        public const string HasFieldsKey = ZNetView.CustomFieldsStr;
        public const string ContainerHasFieldsKey = ZNetView.CustomFieldsStr + Component;
        public const string NameKey = Component + ".m_name";
        private const string WidthKey = Component + ".m_width";
        private const string HeightKey = Component + ".m_height";

        // Larger than any container grid, so loading into it keeps every stored item.
        private const int ProbeSize = 64;

        public static Inventory LoadInventory(ZDO zdo, global::Container prefab)
        {
            var size = GetSize(zdo, prefab);
            return Load(zdo, prefab, size.x, size.y);
        }

        // Stored items that the inventory left out on load because their slot is outside its grid.
        // Saving that inventory would delete them.
        public static int CountDropped(ZDO zdo, global::Container prefab, Inventory inventory)
        {
            return Load(zdo, prefab, ProbeSize, ProbeSize).NrOfItems() - inventory.NrOfItems();
        }

        public static string SizeLabel(Inventory inventory)
        {
            return $"size {inventory.GetWidth()}x{inventory.GetHeight()}";
        }

        private static Vector2i GetSize(ZDO zdo, global::Container prefab)
        {
            if (!zdo.GetBool(HasFieldsKey, false) || !zdo.GetBool(ContainerHasFieldsKey, false))
            {
                return new Vector2i(prefab.m_width, prefab.m_height);
            }

            return new Vector2i(zdo.GetInt(WidthKey, prefab.m_width), zdo.GetInt(HeightKey, prefab.m_height));
        }

        private static Inventory Load(ZDO zdo, global::Container prefab, int width, int height)
        {
            var inventory = new Inventory(prefab.m_name, prefab.m_bkg, width, height);
            var itemsData = zdo.GetByteArray(ZDOVars.s_items, null);
            if (itemsData != null)
            {
                inventory.Load(new ZPackage(itemsData));
            }

            return inventory;
        }
    }
}
