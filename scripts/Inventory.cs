using Godot;
using System;
using System.Linq;

public partial class Inventory : Node
{
	[Export]
	public string inventoryName { get; set; } = "Default Inventory Name";
	public int inventorySize = 30; //Default amount of items for player, not so for storage units.
	public Item[] stored; //Array containing said items.
	public NinePatchRect NPR;
	public override void _Ready(){
		stored = new Item[inventorySize];
		for(int i = 0; i<inventorySize; i++){
			stored[i] = new Item();
		}
		populateGrid();
		NPR = GetNode<NinePatchRect>("CanvasLayer/NinePatchRect");
		GetNode<RichTextLabel>("CanvasLayer/NinePatchRect/MarginContainer/RichTextLabel").Text = "[center]" + inventoryName + "[center]";
	}
	public void populateGrid(){
		GridContainer backgroundGrid = GetNode<GridContainer>("CanvasLayer/NinePatchRect/MarginContainer/GridContainerBackground");
		GridContainer grid = GetNode<GridContainer>("CanvasLayer/NinePatchRect/MarginContainer/GridContainer");
		Item newItem = new Item(1);//TEST CODE
		for (int i = 0; i < 15; i++){ //TEST CODE
			addItem(newItem);
		}
		for(int i= 0; i < 30; i++){
			TextureRect backgroundRect = new TextureRect();
			backgroundRect.Texture = ItemBase.emptyItemFrame;
			backgroundGrid.AddChild(backgroundRect);
			grid.AddChild(stored[i].modelTextureRect.Duplicate());
		}
		foreach(TextureRect tr in grid.GetChildren()){
			tr.GuiInput += _ItemGuiInput;
		}
	} 
	public bool isInventoryFull(){
		int countedItems = 0;
		foreach(Item obj in stored){
			if (obj.id != 0){
				countedItems++;
			}
		}
		if(countedItems == inventorySize)
			return true;
		else
			return false;
	}
	public int nextFreeIndex(){
		for(int i = 0; i < inventorySize; i++){
			if (stored[i].id == 0){
				return i;
			}
		}
		return -1;
	}
	public void addItem(Item newItem){ //Adds a new item.
		if(isInventoryFull()) 
			return;		//If inventory is full, then do nothing.
		stored[nextFreeIndex()] = newItem;
	}
	public void transferItemTo(Inventory otherInventory, int itemIndex){ //Moves item from inventory to another.
		if(otherInventory.isInventoryFull())
			return;		//If inventory is full, then do nothing.
		otherInventory.stored[otherInventory.nextFreeIndex()] = stored[itemIndex];
		stored[itemIndex] = new Item();
	}
	public int getItemId(int itemIndex){
		return stored[itemIndex].id;
	}
	public string getItemName(int itemIndex){
		return ItemBase.nameArray[stored[itemIndex].id];
	}
	public void consumeItem(int itemIndex){
		if (stored[itemIndex].atributes.Contains(ItemBase.typeChart[1])){ // If attributes contains consumable
			stored[itemIndex] = new Item(); //Erase item
		}
	}
	public override void _Process(double delta){

	}
	bool isNPRPressed;
	Vector2 pressedNPRPoint;
	public void _GuiInput(InputEvent @event){ //Dragging window, this function is connected to the NPR
		if (@event is InputEventMouseButton mb){
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed){
				pressedNPRPoint = mb.Position;
				isNPRPressed = true;
			}
			else
				isNPRPressed = false;
		}
		if (@event is InputEventMouseMotion mm){
			if(isNPRPressed){
				NPR.Position += mm.Position - pressedNPRPoint;
			}
		}
	}
	bool isItemPressed;
	Vector2 pressedItemPoint;
	public void _ItemGuiInput(InputEvent @event){
		if (@event is InputEventMouseButton mb){
			GD.Print(@event);
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed){
				pressedItemPoint = mb.Position;
				isItemPressed = true;
			}
			else
				isItemPressed = false;
		}
		if (@event is InputEventMouseMotion mm){
			if(isItemPressed){
				NPR.Position += mm.Position - pressedItemPoint;
			}
		}
	}
}
public partial class Item{
	public int id = 0;
	public int rarity = 0;
	public string[] atributes;
	public TextureRect modelTextureRect = new TextureRect();
	public Item(){ //Set texture, i.e nothing.
		setTextureFromId();
	}
	public Item(int _id){ //Create item with id.
		id = _id;
		setTextureFromId();
	}
	public Vector2 atlasCoordsFromID(){
		return new Vector2(id * 32 -((int)((id*32)/ItemBase.AtlasSize )*ItemBase.AtlasSize), (int)((id*32)/ItemBase.AtlasSize)*32);
	}
	void setTextureFromId(){
		AtlasTexture newAtlasInstance = (AtlasTexture)ItemBase.itemAtlas.Duplicate();
		newAtlasInstance.Region = new Rect2(atlasCoordsFromID(), new Vector2(32,32));
		modelTextureRect.Texture = newAtlasInstance;
	}
}
public static class ItemBase{
	public static Texture2D emptyItemFrame = ResourceLoader.Load<Texture2D>("res://resources/sprites/items/ItemFrame.png");
	public static Texture2D selectedItemFrame = ResourceLoader.Load<Texture2D>("res://resources/sprites/items/ItemFrameSelected.png"); 
	public static AtlasTexture itemAtlas = ResourceLoader.Load<AtlasTexture>("res://resources/sprites/items/itemSheet.tres");
	public static int AtlasSize = 320;
	public static string[] nameArray = { //Essentially the id of the Item.
		"Empty",
		"Lezabel Seed Bag"
		};
	public static string[] rarityChart = {
		"Garbage",
		"Shabby",
		"Plain",
		"Slick",
		"High-Grade",
		"Unreal"
		};	
	public static string[] typeChart = {
		"canBePlanted",
		"consumable",
		"tool"
	};
}