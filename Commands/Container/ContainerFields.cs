namespace ValheimRcon.Commands.Container
{
    // Per-object overrides of Container fields, read the way vanilla ZNetView.LoadFields applies
    // them on clients: only when the object and the component carry the HasFields flags.
    // Mods such as ContainerSizes resize containers through these overrides.
    internal static class ContainerFields
    {
        public const string HasFieldsKey = "HasFields";
        public const string ContainerHasFieldsKey = "ContainerHasFields";
        public const string NameKey = "Container.m_name";
        private const string WidthKey = "Container.m_width";
        private const string HeightKey = "Container.m_height";

        public static Vector2i GetSize(ZDO zdo, global::Container prefab)
        {
            if (!zdo.GetBool(HasFieldsKey, false) || !zdo.GetBool(ContainerHasFieldsKey, false))
            {
                return new Vector2i(prefab.m_width, prefab.m_height);
            }

            return new Vector2i(zdo.GetInt(WidthKey, prefab.m_width), zdo.GetInt(HeightKey, prefab.m_height));
        }

        public static Inventory LoadInventory(ZDO zdo, global::Container prefab)
        {
            var size = GetSize(zdo, prefab);
            var inventory = new Inventory(prefab.m_name, prefab.m_bkg, size.x, size.y);
            var itemsData = zdo.GetByteArray(ZDOVars.s_items, null);
            if (itemsData != null)
            {
                inventory.Load(new ZPackage(itemsData));
            }

            return inventory;
        }
    }
}
