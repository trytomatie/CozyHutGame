using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Generic Item")]
public class Item : ScriptableObject
{
    public enum ItemType {None,Equipment,Resources,Alchemie};

    public ItemType itemType = ItemType.Resources;
    // Ids are getting Distributed from the ItemManager on Start
    [HideInInspector]public ulong itemId;
    public string itemName;
    public GameObject droppedObject;
    public GameObject handProxy;
    public GameObject serverHandProxy;
    public Sprite sprite;
    public int stackSize = 1;
    public int maxStackSize = 64;


    [Serializable]
    public struct ItemData : INetworkSerializable, IEquatable<ItemData>
    {
        public ulong itemId;
        public int stackSize;

        public ItemData(ulong itemId, int itemAmount)
        {
            this.itemId = itemId;
            this.stackSize = itemAmount;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref itemId);
            serializer.SerializeValue(ref stackSize);
        }

        public static ItemData Null
        {
            get
            {
                return new ItemData(0,0);
            }

        }
        public bool IsEmpty
        {
            get
            {
                if (itemId == 0)
                {
                    return true;
                }
                return false;
            }

        }

        public int MaxStackSize
        {
            get
            {
                return ItemManager.GetMaxStackSize(itemId);
            }
        }

        public static Item ReadItemData(ItemData data)
        {
            if(data.itemId == 0)
            {
                return null;
            }
            Item item = ItemManager.GenerateItem(data.itemId);
            item.stackSize = data.stackSize;
            return item;
        }

        public override bool Equals(object other)
        {

            if (((ItemData)other).itemId == itemId && ((ItemData)other).stackSize == stackSize) 
                return true;
            return false;
        }

        public bool Equals(ItemData other)
        {
            throw new NotImplementedException();
        }

        // Override == operator
        public static bool operator ==(ItemData obj1, ItemData obj2)
        {
            if (obj1.itemId == obj2.itemId && obj1.stackSize == obj2.stackSize)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(ItemData obj1, ItemData obj2)
        {
            return !(obj1 == obj2);
        }
    }
}