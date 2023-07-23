using Godot;
using System;
using System.Linq;

public partial class Inventory : Node
{
    [Export]
    public string InventoryName { get; set; } = "Default Inventory Name";

    [Export]
    public int InventorySize { get; set; } = 30; //Default amount of items for player, not so for storage units.

    public Item[] Stored { get; set; } //Array containing said items.
    public NinePatchRect NPR { get; set; }
    public GridContainer Grid { get; set; }
    public CanvasLayer CnvLayer { get; set; }

    [Signal]
    public delegate void itemInventoryDirectionEventHandler(InvSignal info);

    [Signal]
    public delegate void onFocusEventHandler(string invName);

    public override void _Ready()
    {
        Stored = new Item[InventorySize];
        for (int i = 0; i < InventorySize; i++)
        {
            Stored[i] = new Item(0);
        }
        PopulateGrid();
        CnvLayer = GetNode<CanvasLayer>("CanvasLayer");
        NPR = GetNode<NinePatchRect>("CanvasLayer/NinePatchRect");
        GetNode<RichTextLabel>("CanvasLayer/NinePatchRect/MarginContainer/RichTextLabel").Text = "[center]" + InventoryName + "[center]";
        autoload_1 autoloadNode = GetNode<autoload_1>("/root/Autoload1");
        autoloadNode.AddToInventories(this);
    }

    public void PopulateGrid()
    {
        GridContainer backgroundGrid = GetNode<GridContainer>("CanvasLayer/NinePatchRect/MarginContainer/GridContainerBackground");
        Grid = GetNode<GridContainer>("CanvasLayer/NinePatchRect/MarginContainer/GridContainer");
        for (int i = 0; i < 15; i++)
        { //TEST CODE
            AddItem(new Item(new RandomNumberGenerator().RandiRange(1, 4)));
        }
        for (int i = 0; i < InventorySize; i++)
        {
            TextureRect backgroundRect = new TextureRect();
            backgroundRect.Texture = ItemBase.emptyItemFrame;
            backgroundGrid.AddChild(backgroundRect);
            TextureRect foregroundRect = Stored[i].modelTextureRect.Duplicate() as TextureRect;
            Grid.AddChild(foregroundRect);
        }
        foreach (TextureRect tr in Grid.GetChildren())
        {
            tr.GuiInput += _ItemGuiInput;
        }
        pickedNode = Grid.GetChild<TextureRect>(0);
    }

    public bool IsInventoryFull()
    {
        int countedItems = 0;
        foreach (Item obj in Stored)
        {
            if (obj.id != 0)
            {
                countedItems++;
            }
        }
        if (countedItems == InventorySize)
            return true;
        else
            return false;
    }

    public int NextFreeIndex()
    {
        for (int i = 0; i < InventorySize; i++)
        {
            if (Stored[i].id == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public void AddItem(Item newItem)
    { //Adds a new item.
        if (IsInventoryFull())
            return;     //If inventory is full, then do nothing.
        Stored[NextFreeIndex()] = newItem;
    }

    public bool TransferItemTo(Inventory otherInventory, int itemIndex)
    { //Moves item from inventory to another.
        if (otherInventory.IsInventoryFull())
            return false;       //If inventory is full, then do nothing.
        otherInventory.Stored[otherInventory.NextFreeIndex()] = Stored[itemIndex];
        Stored[itemIndex] = new Item(0);
        return true;
    }

    public void ExchangeItems(Inventory otherInventory, int itemIndex, int otherItemIndex)
    {
        Item otherItem = otherInventory.Stored[otherItemIndex];
        GD.Print(otherItemIndex);
        GD.Print(itemIndex);
        otherInventory.Stored[otherItemIndex] = Stored[itemIndex];
        Stored[itemIndex] = otherItem;
    }

    public int GetItemId(int itemIndex) => Stored[itemIndex].id;

    public string GetItemName(int itemIndex) => Stored[itemIndex].name;

    public void ConsumeItem(int itemIndex)
    {
        if (Stored[itemIndex].atributes.Contains(ItemBase.typeChart[1]))
        { // If attributes contains consumable
            Stored[itemIndex] = new Item(); //Erase item
        }
    }

    public void MoveItemTo(int fromIndex, int toIndex)
    {
        Item tempItem = Stored[toIndex];
        Stored[toIndex] = Stored[fromIndex];
        Stored[fromIndex] = tempItem;
    }

    public override void _Process(double delta)
    {
        MovePickedItem();
    }

    bool isNPRPressed;
     Vector2 pressedNPRPoint;

    public void _GuiInput(InputEvent @event)
    { //Dragging window, this function is connected to the NPR
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                pressedNPRPoint = mb.Position;
                isNPRPressed = true;
                EmitSignal(SignalName.onFocus, InventoryName);
            }
            else
                isNPRPressed = false;
        }
        if (@event is InputEventMouseMotion mm)
        {
            if (isNPRPressed)
            {
                NPR.Position += mm.Position - pressedNPRPoint;
            }
        }
    }

    public bool isItemPressed;
     Vector2 pressedItemPoint;
    public Vector2 originalPosition;
    public TextureRect pickedNode;

    public void _ItemGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                if (!isItemPressed)
                {
                    pressedItemPoint = mb.GlobalPosition;
                    isItemPressed = true;

                    bool shouldPickUp = false;
                    foreach (TextureRect nod in Grid.GetChildren())
                    {
                        shouldPickUp = nod.GetGlobalRect().HasPoint(pressedItemPoint);
                        if (shouldPickUp)
                        {
                            pickedNode = nod;
                            originalPosition = pickedNode.Position;
                            MoveItemTo(pickedNode.GetIndex(), nod.GetIndex());
                            nod.Texture = pickedNode.Texture;
                            pickedNode.Texture = Stored[pickedNode.GetIndex()].modelTextureRect.Texture;
                            pickedNode.ZIndex = 10;
                            EmitSignal(SignalName.onFocus, InventoryName);
                            if (Stored[pickedNode.GetIndex()].id != 0)
                                return;
                        }
                    }
                }
                else
                {
                    pressedItemPoint = mb.GlobalPosition;
                    bool shouldPickUp = false;
                    foreach (TextureRect nod in Grid.GetChildren())
                    {
                        if (nod == pickedNode)
                            continue;
                        shouldPickUp = nod.GetGlobalRect().HasPoint(pressedItemPoint);
                        if (shouldPickUp)
                        {
                            pickedNode.Position = nod.Position; //move old node to new one position.
                            MoveItemTo(pickedNode.GetIndex(), nod.GetIndex());
                            nod.Texture = pickedNode.Texture;
                            pickedNode.Texture = Stored[pickedNode.GetIndex()].modelTextureRect.Texture;
                            pickedNode.ZIndex = 10;
                            EmitSignal(SignalName.onFocus, InventoryName);

                            if (Stored[pickedNode.GetIndex()].id != 0)
                                return;
                        }
                    }
                    isItemPressed = false;
                    pickedNode.ZIndex = 0;
                    pickedNode.Position = originalPosition;
                    InvSignal signalInfo = new InvSignal(InventoryName, pickedNode.GetIndex());
                    EmitSignal(SignalName.itemInventoryDirection, signalInfo);
                }
            }
        }
    }

    public void MovePickedItem()
    {
        if (isItemPressed)
            pickedNode.Position += GetViewport().GetMousePosition() - pickedNode.Position - NPR.Position - Grid.Position - new Vector2(16, 16);
    }
}

public partial class InvSignal : GodotObject
{
    public string invName { get; set; }
    public int itemIndex { get; set; }

    public InvSignal(string nam, int indx)
    {
        invName = nam;
        itemIndex = indx;
    }
}

public class Item
{
    public int id;
    public string name;
    public int rarity = 0;
    public string[] atributes;
    public TextureRect modelTextureRect = new TextureRect();
     ItemBase itmb = new ItemBase(); // every time we do this it, creates another static class I HATE DOING THIS, I CANT DO TUPLES BECAAUSE THEY "DONT EXIST"

    public Item()
    { }

    public Item(int _id, string _name, string[] _atributes = null)
    {
        id = _id;
        name = _name;
        atributes = _atributes;
        SetTextureFromId();
    }

    public Item(int _id)
    { //Create item with id.
        id = _id;
        name = itmb.itemArray[_id].name;
        atributes = itmb.itemArray[_id].atributes;
        SetTextureFromId();
    }

    public Vector2 AtlasCoordsFromID() => new Vector2(id * 32 - ((int)((id * 32) / ItemBase.AtlasSize) * ItemBase.AtlasSize), (int)((id * 32) / ItemBase.AtlasSize) * 32);

    void SetTextureFromId()
    {
        AtlasTexture newAtlasInstance = (AtlasTexture)ItemBase.itemAtlas.Duplicate();
        newAtlasInstance.Region = new Rect2(AtlasCoordsFromID(), new Vector2(32, 32));
        modelTextureRect.Texture = newAtlasInstance;
    }
}

public class ItemBase
{
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