using System.Linq;

namespace ValheimRcon.Commands.Container
{
    internal class RenameContainer : ContainerRconCommand
    {
        public override string Command => "renameContainer";

        public override string Description => "Sets the name a container shows in its hover text and inventory title. " +
            "Usage: renameContainer <id:userid> <name> " +
            "-reset " +
            "-force";

        private const int MaxNameLength = 64;

        protected override bool ChangesItems => false;

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

            if (args.Arguments.Skip(2).Any(a => !a.StartsWith("-")))
            {
                return "Put a name with spaces in double quotes.";
            }

            // Clients render the name as rich text, so tags would change how it looks.
            if (hasName && (name.Length > MaxNameLength || name.IndexOfAny(new[] { '<', '>' }) >= 0 || name.Any(char.IsControl)))
            {
                return $"The name must be at most {MaxNameLength} characters, without <, > or control characters.";
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

            // Unmodded clients pick the name up through vanilla ZNetView.LoadFields.
            zdo.Set(ContainerFields.HasFieldsKey, true);
            zdo.Set(ContainerFields.ContainerHasFieldsKey, true);
            zdo.Set(ContainerFields.NameKey, reset ? container.m_name : name);
            zdo.SetZdoModified();
            return $"Name of {prefabName} set to '{zdo.GetString(ContainerFields.NameKey)}'";
        }
    }
}
