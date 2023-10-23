using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Generic Item")]
public class Item : ScriptableObject
{

    public ulong itemId;
    public string itemName;
    public Sprite sprite;
    public int stackSize = 1;
    public int maxStackSize = 64;



    public struct ItemData : INetworkSerializable
    {
        public ulong itemId;
        public int itemAmount;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref itemId);
            serializer.SerializeValue(ref itemAmount);
        }
    }
}