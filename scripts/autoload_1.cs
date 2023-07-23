using Godot;
using System;
using System.Collections.Generic;

public partial class autoload_1 : Node
{
    public List<Inventory> inventoriesList = new List<Inventory>();
    public Item onItemOnHand;
    public void addToInventories(Inventory newInv)
    {
        inventoriesList.Add(newInv);
        newInv.itemInventoryDirection += moveItem;
        newInv.onFocus += changeFocus;
    }
    void moveItem(InvSignal info)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        int indexOfPrimaryInv = 0;
        for (int i = 0; i < inventoriesList.Count; i++)
        {
            if (inventoriesList[i].inventoryName == info.invName)
            {
                indexOfPrimaryInv = i;
            }
        }
        for (int i = 0; i < inventoriesList.Count; i++)
        {
            if (inventoriesList[i].inventoryName == info.invName)
            {
                continue;
            }
            bool shouldPickUp = false;
            foreach (TextureRect rct in inventoriesList[i].grid.GetChildren())
            {
                shouldPickUp = rct.GetGlobalRect().HasPoint(mousePos);
                if (shouldPickUp)
                {
                    inventoriesList[indexOfPrimaryInv].exchangeItems(inventoriesList[i], info.itemIndex, rct.GetIndex());
                    rct.Texture = inventoriesList[i].stored[rct.GetIndex()].modelTextureRect.Texture;
                    inventoriesList[i].pickedNode = rct;
                    inventoriesList[indexOfPrimaryInv].pickedNode = inventoriesList[indexOfPrimaryInv].grid.GetChild<TextureRect>(info.itemIndex);
                    inventoriesList[indexOfPrimaryInv].pickedNode.Texture = inventoriesList[indexOfPrimaryInv].stored[info.itemIndex].modelTextureRect.Texture;
                    inventoriesList[indexOfPrimaryInv].isItemPressed = true;
                    inventoriesList[indexOfPrimaryInv].originalPosition = inventoriesList[indexOfPrimaryInv].pickedNode.Position;
                    return;
                }
            }
        }
    }
    void changeFocus(string invName)
    {
        foreach (Inventory inv in inventoriesList)
        {
            if (inv.inventoryName == invName)
                inv.cnvLayer.Layer = 2;
            else
                inv.cnvLayer.Layer = 1;
        }
    }
    public override void _Process(double delta)
    {
    }
}
