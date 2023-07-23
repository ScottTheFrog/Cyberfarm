using Godot;
using System.Collections.Generic;

public partial class autoload_1 : Node
{
    public List<Inventory> InventoriesList { get; set; } = new List<Inventory>();
    public Item OnItemOnHand { get; set; }

    public void AddToInventories(Inventory newInv)
    {
        InventoriesList.Add(newInv);
        newInv.itemInventoryDirection += MoveItem;
        newInv.onFocus += ChangeFocus;
    }

     void MoveItem(InvSignal info)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        int indexOfPrimaryInv = 0;
        for (int i = 0; i < InventoriesList.Count; i++)
        {
            if (InventoriesList[i].InventoryName == info.invName)
            {
                indexOfPrimaryInv = i;
            }
        }
        for (int i = 0; i < InventoriesList.Count; i++)
        {
            if (InventoriesList[i].InventoryName == info.invName)
            {
                continue;
            }

            foreach (TextureRect rct in InventoriesList[i].Grid.GetChildren())
            {
                bool shouldPickUp = rct.GetGlobalRect().HasPoint(mousePos);
                if (shouldPickUp)
                {
                    InventoriesList[indexOfPrimaryInv].ExchangeItems(InventoriesList[i], info.itemIndex, rct.GetIndex());
                    rct.Texture = InventoriesList[i].Stored[rct.GetIndex()].modelTextureRect.Texture;
                    InventoriesList[i].pickedNode = rct;
                    InventoriesList[indexOfPrimaryInv].pickedNode = InventoriesList[indexOfPrimaryInv].Grid.GetChild<TextureRect>(info.itemIndex);
                    InventoriesList[indexOfPrimaryInv].pickedNode.Texture = InventoriesList[indexOfPrimaryInv].Stored[info.itemIndex].modelTextureRect.Texture;
                    InventoriesList[indexOfPrimaryInv].isItemPressed = true;
                    InventoriesList[indexOfPrimaryInv].originalPosition = InventoriesList[indexOfPrimaryInv].pickedNode.Position;
                    return;
                }
            }
        }
    }

     void ChangeFocus(string invName)
    {
        foreach (Inventory inv in InventoriesList)
        {
            if (inv.InventoryName == invName)
                inv.CnvLayer.Layer = 2;
            else
                inv.CnvLayer.Layer = 1;
        }
    }

    public override void _Process(double delta)
    {
    }
}