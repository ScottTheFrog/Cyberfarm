using Godot;
using System;
using System.Linq;

public partial class Inventory : Node
{
	[Export]
	public string inventoryName { get; set; } = "Default Inventory Name";
	[Export]
	public int inventorySize = 30; //Default amount of items for player, not so for storage units.
	public Item[] stored; //Array containing said items.
	public NinePatchRect NPR;
	public GridContainer grid;
	public CanvasLayer cnvLayer;
	[Signal]
	public delegate void itemInventoryDirectionEventHandler(InvSignal info);
	[Signal]
	public delegate void onFocusEventHandler(string invName);
	public override void _Ready(){
		stored = new Item[inventorySize];
		for(int i = 0; i<inventorySize; i++){
			stored[i] = new Item(0);
		}
		populateGrid();
		cnvLayer = GetNode<CanvasLayer>("CanvasLayer");
		NPR = GetNode<NinePatchRect>("CanvasLayer/NinePatchRect");
		GetNode<RichTextLabel>("CanvasLayer/NinePatchRect/MarginContainer/RichTextLabel").Text = "[center]" + inventoryName + "[center]";
		autoload_1 autoloadNode = GetNode<autoload_1>("/root/Autoload1");
		autoloadNode.addToInventories(this);
	}
	public void populateGrid(){
		GridContainer backgroundGrid = GetNode<GridContainer>("CanvasLayer/NinePatchRect/MarginContainer/GridContainerBackground");
		grid = GetNode<GridContainer>("CanvasLayer/NinePatchRect/MarginContainer/GridContainer");
		for (int i = 0; i < 15; i++){ //TEST CODE
			addItem(new Item(new RandomNumberGenerator().RandiRange(1,4)));
		}
		for(int i= 0; i < inventorySize; i++){
			TextureRect backgroundRect = new TextureRect();
			backgroundRect.Texture = ItemBase.emptyItemFrame;
			backgroundGrid.AddChild(backgroundRect);
			TextureRect foregroundRect = stored[i].modelTextureRect.Duplicate() as TextureRect;
			grid.AddChild(foregroundRect);
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
	public bool transferItemTo(Inventory otherInventory, int itemIndex){ //Moves item from inventory to another.
		if(otherInventory.isInventoryFull())
			return false;		//If inventory is full, then do nothing.
		otherInventory.stored[otherInventory.nextFreeIndex()] = stored[itemIndex];
		stored[itemIndex] = new Item(0);
		return true;
	}
	public void exchangeItems(Inventory otherInventory, int itemIndex, int otherItemIndex){ 
		Item otherItem = otherInventory.stored[otherItemIndex];
		GD.Print(otherItemIndex);
		GD.Print(itemIndex);
		otherInventory.stored[otherItemIndex] = stored[itemIndex];
		stored[itemIndex] = otherItem;
	}
	public int getItemId(int itemIndex){
		return stored[itemIndex].id;
	}
	public string getItemName(int itemIndex){
		return stored[itemIndex].name;
	}
	public void consumeItem(int itemIndex){
		if (stored[itemIndex].atributes.Contains(ItemBase.typeChart[1])){ // If attributes contains consumable
			stored[itemIndex] = new Item(); //Erase item
		}
	}
	public void moveItemTo(int fromIndex, int toIndex){
		Item tempItem = stored[toIndex];
		stored[toIndex] = stored[fromIndex];
		stored[fromIndex] = tempItem;
	}
	public override void _Process(double delta){
		movePickedItem();
	}
	bool isNPRPressed;
	Vector2 pressedNPRPoint;
	public void _GuiInput(InputEvent @event){ //Dragging window, this function is connected to the NPR
		if (@event is InputEventMouseButton mb){
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed){
				pressedNPRPoint = mb.Position;
				isNPRPressed = true;
				EmitSignal(SignalName.onFocus, inventoryName);
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
	public bool isItemPressed;
	Vector2 pressedItemPoint;
	Vector2 originalPosition;
	public TextureRect pickedNode = new TextureRect();
	public void _ItemGuiInput(InputEvent @event){
		if (@event is InputEventMouseButton mb){
			if (mb.ButtonIndex == MouseButton.Left && mb.Pressed){
				if (!isItemPressed){
					pressedItemPoint = mb.GlobalPosition;
					isItemPressed = true;

					bool shouldPickUp = false;
					foreach(TextureRect nod in grid.GetChildren()){
						shouldPickUp = nod.GetGlobalRect().HasPoint(pressedItemPoint);
						if(shouldPickUp){
							pickedNode = nod;
							originalPosition = pickedNode.Position;
							pickedNode.ZIndex = 10;
							EmitSignal(SignalName.onFocus, inventoryName);
							return;
						}
					}
				}
				else{
					pressedItemPoint = mb.GlobalPosition;
					bool shouldPickUp = false;
					foreach(TextureRect nod in grid.GetChildren()){
						if(nod == pickedNode)
							continue;
						shouldPickUp = nod.GetGlobalRect().HasPoint(pressedItemPoint);
						if (shouldPickUp){
							GD.Print(pickedNode.GetIndex());
							GD.Print(nod.GetIndex());
							pickedNode.Position = nod.Position; //move old node to new one position.
							moveItemTo(pickedNode.GetIndex(), nod.GetIndex());
							nod.Texture = pickedNode.Texture;
							pickedNode.Texture = stored[pickedNode.GetIndex()].modelTextureRect.Texture;
							pickedNode.ZIndex = 10;
							EmitSignal(SignalName.onFocus, inventoryName);
							
							if (stored[pickedNode.GetIndex()].id != 0)
								return;
						}
					}
					InvSignal signalInfo = new InvSignal(inventoryName, pickedNode.GetIndex());
					EmitSignal(SignalName.itemInventoryDirection, signalInfo);
					isItemPressed = false;
					pickedNode.ZIndex = 0;
					pickedNode.Position = originalPosition;
				}
			}	
		}
	}
	public void movePickedItem(){
		if(isItemPressed)
			pickedNode.Position += GetViewport().GetMousePosition() - pickedNode.Position - NPR.Position - grid.Position - new Vector2(16,16);
	}
}
public partial class InvSignal : GodotObject{
	public string invName {get; set;}
	public int itemIndex {get; set;}
	public InvSignal(string nam, int indx){
		invName = nam;
		itemIndex = indx;
	}
}
public class Item{
	public int id;
	public string name;
	public int rarity = 0;
	public string[] atributes;
	public TextureRect modelTextureRect = new TextureRect();
	private ItemBase itmb = new ItemBase(); // every time we do this it, creates another static class I HATE DOING THIS, I CANT DO TUPLES BECAAUSE THEY "DONT EXIST"
	public Item(){}
	public Item(int _id, string _name, string[] _atributes = null){
		id = _id;
		name = _name;
		atributes = _atributes;
		setTextureFromId();
	}
	public Item(int _id){ //Create item with id.
		id = _id;
		name = itmb.itemArray[_id].name;
		atributes = itmb.itemArray[_id].atributes;
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
public class ItemBase{
	public static Texture2D emptyItemFrame = ResourceLoader.Load<Texture2D>("res://resources/sprites/items/ItemFrame.png");
	public static Texture2D selectedItemFrame = ResourceLoader.Load<Texture2D>("res://resources/sprites/items/ItemFrameSelected.png"); 
	public static AtlasTexture itemAtlas = ResourceLoader.Load<AtlasTexture>("res://resources/sprites/items/itemSheet.tres");
	public static int AtlasSize = 320;
	public (int index, string name, string[] atributes)[] itemArray = {
			(0, "Empty", new string[0]),
			(1, "Frightling Seed", new string[] {typeChart[0], typeChart[3]}),
			(2, "Rotlich Seed", new string[] {typeChart[0], typeChart[3]}),
			(3, "Soilbug Seed", new string[] {typeChart[0], typeChart[3]}),
			(4, "Shadeling Seed", new string[] {typeChart[0], typeChart[3]}),
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
		"tool",
		"summer",
		"autumn",
		"winter",
		"spring"
	};
}