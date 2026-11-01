namespace ValheimRcon.Commands.Container
{
    internal class RenameContainer : ContainerRconCommand
    {
        // Vanilla ZNetView.LoadFields applies "<Component>.<field>" overrides when the object
        // and the component carry the HasFields flags, so unmodded clients pick up the name.
        private const string HasFieldsKey = "HasFields";
        private const string ContainerHasFieldsKey = "Container" + HasFieldsKey;
        private const string NameKey = "Container.m_name";

        public override string Command => "renameContainer";

        public override string Description => "Sets the name a container shows in its hover text and inventory title. " +
            "Usage: renameContainer <id:userid> <name> " +
            "-reset " +
            "-force";

        protected override string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName)
        {
            var reset = false;
            var force = false;

            foreach (var (index, argument) in args.GetOptionalArguments())
            {
                switch (argument)
                {
                    case "-reset":
                        reset = true;
                        break;
                    case "-force":
                        force = true;
                        break;
                    default:
                        return $"Unknown argument {argument}";
                }
            }

            var name = args.TryGetString(1);
            var hasName = name.Length > 0 && !name.StartsWith("-");
            if (hasName == reset)
            {
                return "Specify either a name or -reset.";
            }

            var container = ZNetScene.instance.GetPrefab(zdo.GetPrefab()).GetComponent<global::Container>();
            if (container == null)
            {
                return $"The container of {prefabName} is not on the root object, so its name cannot be overridden.";
            }

            if (!ContainerUtils.ValidateContainerModification(zdo, prefabName, force, out var validationError))
            {
                return validationError;
            }

            zdo.Set(HasFieldsKey, true);
            zdo.Set(ContainerHasFieldsKey, true);
            zdo.Set(NameKey, reset ? container.m_name : name);
            zdo.SetZdoModified();
            return $"Name of {prefabName} set to '{zdo.GetString(NameKey)}'";
        }
    }
}
