using System.Linq;
using ValheimRcon.Commands.Search;

namespace ValheimRcon.Commands.Container
{
    public abstract class ContainerRconCommand : RconCommand
    {
        protected override string OnHandle(CommandArgs args)
        {
            var id = args.GetObjectId(0);
            var idCriteria = new IdCriteria(id);
            var zdo = ZDOMan.instance.m_objectsByID.Values.FirstOrDefault(idCriteria.IsMatch);

            if (zdo == null)
            {
                return "No object found with the specified ID.";
            }

            var prefabHash = zdo.GetPrefab();
            var prefabName = zdo.GetPrefabName();
            var prefab = ZNetScene.instance.GetPrefab(prefabHash);
            
            if (prefab == null)
            {
                return $"Failed to load prefab: {prefabName}";
            }

            var containerComponent = prefab.GetComponentInChildren<global::Container>();
            if (containerComponent == null)
            {
                return $"Object {prefabName} is not a container.";
            }

            var inventory = ContainerFields.LoadInventory(zdo, containerComponent);
            DroppedOnLoad = ContainerFields.CountDropped(zdo, containerComponent, inventory);
            if (DroppedOnLoad > 0 && ChangesItems)
            {
                return $"{DroppedOnLoad} stored items of {prefabName} lie outside its {ContainerFields.SizeLabel(inventory)} grid, " +
                    "saving it would delete them.";
            }

            return HandleInventory(args, zdo, inventory, prefabName);
        }

        // Commands that only read the items or leave them alone override this, the rest are refused
        // when the inventory could not hold every stored item.
        protected virtual bool ChangesItems => true;

        protected int DroppedOnLoad { get; private set; }

        protected abstract string HandleInventory(CommandArgs args, ZDO zdo, Inventory inventory, string prefabName);

        protected void SaveInventory(ZDO zdo, Inventory inventory)
        {
            var package = new ZPackage();
            inventory.Save(package);
            zdo.Set(ZDOVars.s_items, package.GetArray());
            zdo.SetZdoModified();
        }
    }
}
