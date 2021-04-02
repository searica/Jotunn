﻿using JotunnLib.Managers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using JotunnLib.Utils;

namespace JotunnLib
{
    public static class InventoryExtension
    {
        public static bool HasAnyCustomItem(this Inventory self)
        {
            foreach (var inventoryItem in self.m_inventory)
            {
                foreach (var customItem in ObjectManager.Instance.Items)
                {
                    if (inventoryItem.TokenName() == customItem.ItemDrop.TokenName())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool HasEveryItemFromSavedFile(this Inventory self)
        {
            var customItemNames = self.GetAllCustomItemNamesFromFile();
            var savedFileCount = customItemNames.Count;

            var inventoryCount = 0;
            foreach (var inventoryItem in self.m_inventory)
            {
                foreach (var customItemName in customItemNames)
                {
                    if (inventoryItem.m_dropPrefab.name == customItemName)
                    {
                        inventoryCount++;
                    }
                }
            }

            return inventoryCount == savedFileCount;
        }

        internal static List<string> GetAllCustomItemNamesFromFile(this Inventory self)
        {
            string inventoryId = self.GetInventoryUID();

            var inventoryFilePath = Path.Combine(Paths.CustomItemDataFolder, inventoryId);

            if (!File.Exists(inventoryFilePath))
            {
                return new List<string>();
            }

            var customItemNames = new List<string>();

            var data = File.ReadAllLines(inventoryFilePath);

            for (var i = 0; i < data.Length; i += ItemDataExtension.LinesToNextEntry)
            {
                customItemNames.Add(data[i]);
            }

            return customItemNames;
        }

        internal static void AddCustomItemsFromFile(this Inventory self, string inventoryId)
        {
            var data = File.ReadAllLines(Path.Combine(Paths.CustomItemDataFolder, inventoryId));

            for (var i = 0; i < data.Length; i += ItemDataExtension.LinesToNextEntry)
            {
                var itemPrefab = ObjectDB.instance.GetItemPrefab(data[i]);
                if (!itemPrefab)
                {
                    continue;
                }

                ZNetView.m_forceDisableInit = true;
                GameObject gameObject = UnityEngine.Object.Instantiate(itemPrefab);
                ZNetView.m_forceDisableInit = false;

                var itemDrop = gameObject.GetComponent<ItemDrop>();
                if (!itemDrop)
                {
                    continue;
                }

                var hash = data[i + 8];
                var foundInInventory = false;
                foreach (var itemData in self.m_inventory)
                {
                    if (itemData.GetUID() == hash)
                    {
                        foundInInventory = true;
                        break;
                    }
                }

                if (!foundInInventory)
                {
                    itemDrop.m_itemData.m_stack = Mathf.Min(int.Parse(data[i + 1], CultureInfo.InvariantCulture), itemDrop.m_itemData.m_shared.m_maxStackSize);
                    itemDrop.m_itemData.m_durability = float.Parse(data[i + 2], CultureInfo.InvariantCulture);
                    itemDrop.m_itemData.m_equiped = bool.Parse(data[i + 3]);
                    itemDrop.m_itemData.m_quality = int.Parse(data[i + 4], CultureInfo.InvariantCulture);
                    itemDrop.m_itemData.m_variant = int.Parse(data[i + 5], CultureInfo.InvariantCulture);
                    itemDrop.m_itemData.m_crafterID = long.Parse(data[i + 6], CultureInfo.InvariantCulture);
                    itemDrop.m_itemData.m_crafterName = data[i + 7];

                    self.AddItem(itemDrop.m_itemData);
                }

                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }
}
