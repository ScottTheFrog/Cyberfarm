using Godot;
using System;
using System.Collections.Generic;

public partial class autoload_1 : Node
{
	public List<Inventory> inventoriesList = new List<Inventory>();
	public Item onItemOnHand;
	public void addToInventories(Inventory newInv){
		inventoriesList.Add(newInv);
		newInv.itemInventoryDirection += moveItem;
		newInv.onFocus += changeFocus;
	}
	void moveItem(InvSignal info){
		Vector2 mousePos = GetViewport().GetMousePosition();
		int indexOfPrimaryInv = 0;
		foreach(Inventory inv in inventoriesList){
			if (inv.inventoryName == info.invName){
				indexOfPrimaryInv = inventoriesList.IndexOf(inv);
				continue;
			}
			bool shouldPickUp = false;
			foreach(TextureRect rct in inv.grid.GetChildren()){
				shouldPickUp = rct.GetGlobalRect().HasPoint(mousePos);
				if (shouldPickUp){
					inventoriesList[indexOfPrimaryInv].exchangeItems(inv, info.itemIndex, rct.GetIndex());
					rct.Texture  = inv.stored[rct.GetIndex()].modelTextureRect.Texture;
					inventoriesList[indexOfPrimaryInv].pickedNode = inventoriesList[indexOfPrimaryInv].grid.GetChild<TextureRect>(info.itemIndex);
					inventoriesList[indexOfPrimaryInv].pickedNode.Texture = inventoriesList[indexOfPrimaryInv].stored[info.itemIndex].modelTextureRect.Texture;
					inventoriesList[indexOfPrimaryInv].isItemPressed = true;
					return;
				}
			}
		}
	}
	void changeFocus(string invName){
		foreach(Inventory inv in inventoriesList){
			if(inv.inventoryName == invName)
				inv.cnvLayer.Layer = 2;
			else
				inv.cnvLayer.Layer = 1;
		}
	}
	public override void _Process(double delta)
	{
	}
}
