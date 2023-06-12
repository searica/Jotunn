﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Jotunn.Entities;

namespace Jotunn.Configs
{
    /// <summary>
    ///     Configuration class for adding custom recipes.
    /// </summary>
    public class RecipeConfig
    {
        /// <summary>
        ///     The unique name for your recipe.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     The name of the item prefab that this recipe should create.
        /// </summary>
        public string Item { get; set; } = string.Empty;

        /// <summary>
        ///     The amount of <see cref="Item"/> that will be created from this Recipe. Defaults to <c>1</c>.
        /// </summary>
        public int Amount { get; set; } = 1;

        /// <summary>
        ///     Whether this recipe is craftable or not. Defaults to true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     The name of the crafting station prefab where this recipe can be crafted.
        ///     <br/>
        ///     Can be set to <c>null</c> to have the recipe be craftable without a crafting station.
        /// </summary>
        public string CraftingStation
        {
            get => craftingStation;
            set => craftingStation = CraftingStations.GetInternalName(value);
        }

        /// <summary>
        ///     The name of the crafting station prefab where this item can be repaired.
        ///     <br/>
        ///     Can be set to <c>null</c> to have the recipe be repairable without a crafting station.
        /// </summary>
        public string RepairStation
        {
            get => repairStation;
            set => repairStation = CraftingStations.GetInternalName(value);
        }

        /// <summary>
        ///     The minimum required level for the crafting station. Defaults to <c>1</c>.
        /// </summary>
        public int MinStationLevel { get; set; } = 1;

        /// <summary>
        ///     Array of <see cref="RequirementConfig"/>s for all crafting materials it takes to craft the recipe.
        /// </summary>
        public RequirementConfig[] Requirements { get; set; } = Array.Empty<RequirementConfig>();

        private string craftingStation = string.Empty;
        private string repairStation = string.Empty;

        /// <summary>
        ///     Converts the RequirementConfigs to Valheim style Piece.Requirements
        /// </summary>
        /// <returns>The Valheim Piece.Requirement array</returns>
        public Piece.Requirement[] GetRequirements()
        {
            List<Piece.Requirement> reqs = new List<Piece.Requirement>();

            foreach (RequirementConfig requirement in Requirements)
            {
                if (requirement != null && requirement.IsValid())
                {
                    reqs.Add(requirement.GetRequirement());
                }
            }

            return reqs.ToArray();
        }

        /// <summary>
        ///     Converts the RecipeConfig to a Valheim style Recipe.
        /// </summary>
        /// <returns>The Valheim recipe</returns>
        public Recipe GetRecipe()
        {
            if (Item == null)
            {
                Logger.LogError($"No item set in recipe config");
                return null;
            }

            var recipe = ScriptableObject.CreateInstance<Recipe>();

            var name = Name;
            if (string.IsNullOrEmpty(name))
            {
                name = "Recipe_" + Item;
            }

            recipe.name = name;

            recipe.m_item = Mock<ItemDrop>.Create(Item);
            recipe.m_amount = Amount;
            recipe.m_enabled = Enabled;

            if (!string.IsNullOrEmpty(CraftingStation))
            {
                recipe.m_craftingStation = Mock<CraftingStation>.Create(CraftingStation);
            }

            if (!string.IsNullOrEmpty(RepairStation))
            {
                recipe.m_repairStation = Mock<CraftingStation>.Create(RepairStation);
            }

            recipe.m_minStationLevel = MinStationLevel;
            recipe.m_resources = GetRequirements();

            return recipe;
        }

        /// <summary>
        ///     Loads a single RecipeConfig from a JSON string.
        /// </summary>
        /// <param name="json">JSON text</param>
        /// <returns>Loaded RecipeConfig</returns>
        public static RecipeConfig FromJson(string json)
        {
            return SimpleJson.SimpleJson.DeserializeObject<RecipeConfig>(json);
        }

        /// <summary>
        ///     Loads a list of RecipeConfigs from a JSON string.
        /// </summary>
        /// <param name="json">JSON text</param>
        /// <returns>Loaded list of RecipeConfigs</returns>
        public static List<RecipeConfig> ListFromJson(string json)
        {
            return SimpleJson.SimpleJson.DeserializeObject<List<RecipeConfig>>(json);
        }

        /// <summary>
        ///     Appends a new <see cref="RequirementConfig"/> to the array of existing ones.<br />
        ///     If the requirement is null or is not valid (has not item name or amount set) nothing will be added.
        /// </summary>
        /// <param name="requirementConfig"></param>
        public void AddRequirement(RequirementConfig requirementConfig)
        {
            if (requirementConfig != null && requirementConfig.IsValid())
            {
                Requirements = Requirements.AddToArray(requirementConfig);
            }
        }
    }
}
