using System;
using System.Linq;

using ACE.Common;
using ACE.Database.Models.World;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity;
using ACE.Server.Entity.Mutations;
using ACE.Server.Factories.Entity;
using ACE.Server.Factories.Enum;
using ACE.Server.Factories.Tables;
using ACE.Server.Managers;
using ACE.Server.WorldObjects;
using log4net;
using Org.BouncyCastle.Bcpg;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ACE.Server.Factories
{
    public static partial class LootGenerationFactory
    {
        private static WorldObject CreateArmor(TreasureDeath profile, bool isMagical, bool isArmor, LootBias lootBias = LootBias.UnBiased, bool mutate = true)
        {
            var minType = LootTables.ArmorType.Helms;
            LootTables.ArmorType maxType;

            switch (profile.Tier)
            {
                case 1:
                default:
                    maxType = LootTables.ArmorType.ChainmailArmor;
                    break;
                case 2:
                    maxType = LootTables.ArmorType.DiforsaArmor;
                    break;
                case 3:
                case 4:
                    maxType = LootTables.ArmorType.CovenantArmor;
                    break;
                case 5:
                    maxType = LootTables.ArmorType.AlduressaArmor;
                    break;
                case 6:
                    maxType = LootTables.ArmorType.HaebreanArmor;
                    break;
                case 7:
                case 8:
                    maxType = LootTables.ArmorType.OlthoiAlduressaArmor;
                    break;
            }

            // Added for making clothing drops their own drop, and not involved in armor roll chance
            LootTables.ArmorType armorType;
            if (isArmor)
                armorType = (LootTables.ArmorType)ThreadSafeRandom.Next((int)minType, (int)maxType);
            else
                armorType = LootTables.ArmorType.MiscClothing;

            int[] table = LootTables.GetLootTable(armorType);

            int rng = ThreadSafeRandom.Next(0, table.Length - 1);

            int armorWeenie = table[rng];

            WorldObject wo = WorldObjectFactory.CreateNewWorldObject((uint)armorWeenie);

            if (wo != null && mutate)
                MutateArmor(wo, profile, isMagical, armorType);

            return wo;
        }

        private static void MutateArmor(WorldObject wo, TreasureDeath profile, bool isMagical, LootTables.ArmorType armorType, TreasureRoll roll = null)
        {
            // material type
            var materialType = GetMaterialType(wo, profile.Tier);
            if (materialType > 0)
                wo.MaterialType = materialType;

            // item color
            MutateColor(wo);

            // gem count / gem material
            if (wo.GemCode != null)
                wo.GemCount = GemCountChance.Roll(wo.GemCode.Value, profile.Tier);
            else
                wo.GemCount = ThreadSafeRandom.Next(1, 6);

            wo.GemType = RollGemType(profile.Tier);

            // workmanship
            wo.ItemWorkmanship = WorkmanshipChance.Roll(profile.Tier);

            // burden
            if (wo.HasMutateFilter(MutateFilter.EncumbranceVal))  // fixme: data
                MutateBurden(wo, profile, false);

            if (roll == null)
            {
                if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                {
                    int chance = ThreadSafeRandom.Next(1, 3);
                    var wieldSkill = chance switch
                    {
                        1 => Skill.MagicDefense,
                        2 => Skill.MissileDefense,
                        _ => Skill.MeleeDefense,
                    };

                    wo.WieldRequirements = WieldRequirement.RawSkill;
                    wo.WieldSkillType = (int)wieldSkill;
                    wo.WieldDifficulty = GetCovenantWieldReq(profile.Tier, wieldSkill);
                }
                else if (profile.Tier > 6 && profile.Tier < 10)
                {
                    wo.WieldRequirements = WieldRequirement.Level;
                    wo.WieldSkillType = (int)Skill.Axe;  // Set by examples from PCAP data

                    wo.WieldDifficulty = profile.Tier switch
                    {
                        7 => 150,  // In this instance, used for indicating player level, rather than skill level
                        8 => 180,  // In this instance, used for indicating player level, rather than skill level
                        _ => 500,  // In this instance, used for indicating player level, rather than skill level
                    };
                }
            }
            else if (profile.Tier > 6 && profile.Tier < 10 && !wo.HasArmorLevel())
            {
                // normally this is handled in the mutation script for armor
                // for clothing, just calling the generic method here
                RollWieldLevelReq_T7_T8(wo, profile);
            }

            if (roll == null)
                AssignArmorLevel(wo, profile.Tier, armorType);
            else
                AssignArmorLevel_New(wo, profile, roll, armorType);

            if (wo.HasMutateFilter(MutateFilter.ArmorModVsType))
                MutateArmorModVsType(wo, profile);

            if (isMagical)
            {
                AssignMagic(wo, profile, roll, true);
            }
            else
            {
                wo.ItemManaCost = null;
                wo.ItemMaxMana = null;
                wo.ItemCurMana = null;
                wo.ItemSpellcraft = null;
                wo.ItemDifficulty = null;
            }

            if (profile.Tier > 6 && armorType != LootTables.ArmorType.SocietyArmor)
                TryRollEquipmentSet(wo, profile, roll);

            if (roll != null && profile.Tier >= 8 && profile.Tier < 9)
                TryMutateGearRating(wo, profile, roll);

            if (roll != null && profile.Tier == 9)
                TryMutateGearRatingT9(wo, profile, roll);

            if (roll != null && profile.Tier == 10)
                TryMutateGearRatingT10(wo, profile, roll);

            // item value
            //if (wo.HasMutateFilter(MutateFilter.Value))   // fixme: data
            MutateValue(wo, profile.Tier, roll);

            if (wo.ItemMaxLevel.HasValue && wo.EquipmentSetId == null)
            {
                MutateItemLevel(wo, profile);
            }

            wo.LongDesc = GetLongDesc(wo);
        }

        private static bool AssignArmorLevel_New(WorldObject wo, TreasureDeath profile, TreasureRoll roll, LootTables.ArmorType armorType)
        {
            // retail was only divied up into a few different mutation scripts here
            // anything with ArmorLevel ran these mutation scripts
            // anything that covered extremities (head / hand / foot wear) started with a slightly higher base AL,
            // but otherwise used the same mutation as anything that covered non-extremities
            // shields also had their own mutation script

            // only exceptions found: covenant armor, olthoi armor, metal cap

            if (!roll.HasArmorLevel(wo))
                return false;

            var scriptName = GetMutationScript_ArmorLevel(wo, roll);

            if (scriptName == null)
            {
                log.Error($"AssignArmorLevel_New({wo.Name}, {profile.TreasureType}, {roll.ItemType}) - unknown item type");
                return false;
            }

            // persist original values for society armor
            var wieldRequirements = wo.WieldRequirements;
            var wieldSkillType = wo.WieldSkillType;
            var wieldDifficulty = wo.WieldDifficulty;

            //Console.WriteLine($"Mutating {wo.Name} with {scriptName}");

            var mutationFilter = MutationCache.GetMutation(scriptName);

            var success = mutationFilter.TryMutate(wo, profile.Tier);

            if (armorType == LootTables.ArmorType.SocietyArmor)
            {
                wo.WieldRequirements = wieldRequirements;
                wo.WieldSkillType = wieldSkillType;
                wo.WieldDifficulty = wieldDifficulty;
            }

            return success;
        }

        private static string GetMutationScript_ArmorLevel(WorldObject wo, TreasureRoll roll)
        {
            switch (roll.ArmorType)
            {
                case TreasureArmorType.Covenant:

                    if (wo.IsShield)
                        return "ArmorLevel.covenant_shield.txt";
                    else
                        return "ArmorLevel.covenant_armor.txt";

                case TreasureArmorType.Olthoi:

                    if (wo.IsShield)
                        return "ArmorLevel.olthoi_shield.txt";
                    else
                        return "ArmorLevel.olthoi_armor.txt";
            }

            if (wo.IsShield)
                return "ArmorLevel.shield_level.txt";

            var coverage = wo.ClothingPriority ?? 0;

            if ((coverage & (CoverageMask)CoverageMaskHelper.Extremities) != 0)
                return "ArmorLevel.armor_level_extremity.txt";
            else if ((coverage & (CoverageMask)CoverageMaskHelper.Outerwear) != 0)
                return "ArmorLevel.armor_level_non_extremity.txt";
            else
                return null;
        }

        /// <summary>
        /// Assign a final AL value based upon tier
        /// Used values given at https://asheron.fandom.com/wiki/Loot#Armor_Levels for setting the AL mod values
        /// so as to not exceed the values listed in that table
        /// </summary>
        private static void AssignArmorLevel(WorldObject wo, int tier, LootTables.ArmorType armorType)
        {
            if (wo.ArmorType == null)
            {
                log.Warn($"[LOOT] Missing PropertyInt.ArmorType on loot item {wo.WeenieClassId} - {wo.Name}");
                return;
            }

            var baseArmorLevel = wo.ArmorLevel ?? 0;

            if (((wo.ClothingPriority & (CoverageMask)CoverageMaskHelper.Underwear) == 0) || wo.IsShield)
            {
                int armorModValue = 0;

                // Account for ACE World Databases that have not yet been updated
                if (wo.ArmorType != (int)ArmorType.Cloth && (wo.GetProperty(PropertyInt.Version) ?? 0) < 3)
                    AssignArmorLevelCompat(wo, tier, armorType);

                // Sets AL variations based on weenie ArmorType field, such as cloth, leather, metal, etc.
                switch (tier)
                {
                    case 1:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(0, 25);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(0, 29);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(0, 26);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        // Does not drop at this tier level
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = 0;
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(5, 22);
                        break;
                    case 2:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(25, 50);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(29, 52);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(26, 50);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        // Does not drop at this tier level
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = 0;
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(22, 39);
                        break;
                    case 3:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(50, 75);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(52, 75);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(50, 74);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = ThreadSafeRandom.Next(190, 210);
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(39, 56);
                        break;
                    case 4:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(75, 100);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(75, 98);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(74, 98);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = ThreadSafeRandom.Next(210, 230);
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(56, 73);
                        break;
                    case 5:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(100, 125);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(98, 121);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(98, 122);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = ThreadSafeRandom.Next(230, 250);
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(73, 90);
                        break;
                    case 6:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(125, 150);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(121, 144);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(122, 146);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = ThreadSafeRandom.Next(250, 270);
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(90, 107);
                        break;
                    case 7:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(150, 175);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(144, 167);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(146, 170);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = ThreadSafeRandom.Next(270, 290);
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(107, 124);
                        break;
                    case 8:
                        if (wo.ArmorType == (int)ArmorType.Cloth)
                            armorModValue = ThreadSafeRandom.Next(175, 200);
                        if (wo.ArmorType == (int)ArmorType.Leather
                            || wo.ArmorType == (int)ArmorType.StuddedLeather)
                            armorModValue = ThreadSafeRandom.Next(167, 190);
                        if (wo.ArmorType == (int)ArmorType.Metal
                            || wo.ArmorType == (int)ArmorType.Chainmail
                            || wo.ArmorType == (int)ArmorType.Scalemail)
                            armorModValue = ThreadSafeRandom.Next(170, 194);
                        // Covenant and Olthoi Armor (not Amuli, Celdon, Koujia, or Alduressa types of Olthoi Armor)
                        if (wo.ResistMagic != null && wo.ResistMagic == 9999)
                            armorModValue = ThreadSafeRandom.Next(290, 310);
                        if (wo.IsShield)
                            armorModValue = ThreadSafeRandom.Next(124, 141);
                        break;
                }

                int adjustedArmorLevel = baseArmorLevel + armorModValue;
                wo.ArmorLevel = adjustedArmorLevel;
            }

            if ((wo.ResistMagic == null || wo.ResistMagic < 9999) && wo.ArmorLevel >= 345)
                log.Warn($"[LOOT] Standard armor item exceeding upper AL threshold {wo.WeenieClassId} - {wo.Name}");
        }

        private static void AssignArmorLevelCompat(WorldObject wo, int tier, LootTables.ArmorType armorType)
        {
            log.Debug($"[LOOT] Using AL Assignment Compatibility layer for item {wo.WeenieClassId} - {wo.Name}.");

            var baseArmorLevel = wo.ArmorLevel ?? 0;

            if (baseArmorLevel > 0)
            {
                int armorModValue = 0;

                if (armorType > LootTables.ArmorType.HaebreanArmor && armorType <= LootTables.ArmorType.OlthoiAlduressaArmor)
                {
                    // Even if most are not using T8, made a change to that outcome to ensure that Olthoi Alduressa doesn't go way out of spec
                    // Side effect is that Haebrean to Olthoi Celdon may suffer
                    armorModValue = tier switch
                    {
                        7 => ThreadSafeRandom.Next(0, 40),
                        8 => ThreadSafeRandom.Next(91, 115),
                        9 => ThreadSafeRandom.Next(115, 140),
                        _ => 0,
                    };
                }
                else
                {
                    switch (tier)
                    {
                        case 1:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(0, 27);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(0, 23);

                            else
                                armorModValue = ThreadSafeRandom.Next(0, 40);
                            break;
                        case 2:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(27, 54);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(23, 46);

                            else
                                armorModValue = ThreadSafeRandom.Next(40, 80);
                            break;
                        case 3:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(54, 81);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(46, 69);

                            else if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                                armorModValue = ThreadSafeRandom.Next(90, 130);

                            else
                                armorModValue = ThreadSafeRandom.Next(80, 120);
                            break;
                        case 4:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(81, 108);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(69, 92);

                            else if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                                armorModValue = ThreadSafeRandom.Next(130, 170);

                            else
                                armorModValue = ThreadSafeRandom.Next(120, 160);
                            break;
                        case 5:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(108, 135);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(92, 115);

                            else if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                                armorModValue = ThreadSafeRandom.Next(170, 210);

                            else
                                armorModValue = ThreadSafeRandom.Next(160, 200);
                            break;
                        case 6:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(135, 162);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(115, 138);

                            else if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                                armorModValue = ThreadSafeRandom.Next(210, 250);

                            else
                                armorModValue = ThreadSafeRandom.Next(200, 240);
                            break;
                        case 7:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(162, 189);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                  || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(138, 161);

                            else if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                                armorModValue = ThreadSafeRandom.Next(250, 290);

                            else
                                armorModValue = ThreadSafeRandom.Next(240, 280);
                            break;
                        case 8:
                            if (armorType == LootTables.ArmorType.StuddedLeatherArmor
                             || armorType == LootTables.ArmorType.Helms
                             || armorType == LootTables.ArmorType.Shields)
                                armorModValue = ThreadSafeRandom.Next(189, 216);

                            else if (armorType == LootTables.ArmorType.LeatherArmor
                                || armorType == LootTables.ArmorType.MiscClothing)
                                armorModValue = ThreadSafeRandom.Next(161, 184);

                            else if (armorType == LootTables.ArmorType.CovenantArmor || armorType == LootTables.ArmorType.OlthoiArmor)
                                armorModValue = ThreadSafeRandom.Next(290, 330);

                            else if (armorType == LootTables.ArmorType.SocietyArmor)
                                armorModValue = ThreadSafeRandom.Next(189, 216);
                            else
                                armorModValue = ThreadSafeRandom.Next(280, 320);
                            break;
                        default:
                            armorModValue = 0;
                            break;
                    }
                }

                int adjustedArmorLevel = baseArmorLevel + armorModValue;
                wo.ArmorLevel = adjustedArmorLevel;
            }
        }

        private static int GetCovenantWieldReq(int tier, Skill skill)
        {
            var index = tier switch
            {
                3 => ThreadSafeRandom.Next(1, 3),
                4 => ThreadSafeRandom.Next(1, 4),
                5 => ThreadSafeRandom.Next(1, 5),
                6 => ThreadSafeRandom.Next(1, 6),
                7 => ThreadSafeRandom.Next(1, 7),
                8 => ThreadSafeRandom.Next(1, 8),
                _ => ThreadSafeRandom.Next(1, 9),
            };

            var wield = skill switch
            {
                Skill.MagicDefense => index switch
                {
                    1 => 145,
                    2 => 185,
                    3 => 225,
                    4 => 245,
                    5 => 270,
                    6 => 290,
                    7 => 310,
                    8 => 320,
                    _ => 430,
                },
                Skill.MissileDefense => index switch
                {
                    1 => 160,
                    2 => 205,
                    3 => 245,
                    4 => 270,
                    5 => 290,
                    6 => 305,
                    7 => 330,
                    8 => 340,
                    _ => 450,
                },
                _ => index switch
                {
                    1 => 200,
                    2 => 250,
                    3 => 300,
                    4 => 325,
                    5 => 350,
                    6 => 370,
                    7 => 400,
                    8 => 410,
                    _ => 500,
                },
            };
            return wield;
        }

        private static bool TryRollEquipmentSet(WorldObject wo, TreasureDeath profile, TreasureRoll roll)
        {
            if (roll == null)
            {
                if (!PropertyManager.GetBool("equipmentsetid_enabled").Item)
                    return false;

                if (profile.Tier < 6 || !wo.HasArmorLevel())
                    return false;

                if (wo.ClothingPriority == null || (wo.ClothingPriority & (CoverageMask)CoverageMaskHelper.Outerwear) == 0)
                    return false;

                var dropRate = PropertyManager.GetDouble("equipmentsetid_drop_rate").Item;
                var dropRateMod = 1.0 / dropRate;

                var lootQualityMod = 1.0f;
                if (PropertyManager.GetBool("loot_quality_mod").Item)
                    lootQualityMod = 1.0f - profile.LootQualityMod;

                // initial base 10% chance to add a random EquipmentSet, which can be adjusted via equipmentsetid_drop_rate
                var rng = ThreadSafeRandom.Next(1, (int)(100 * dropRateMod * lootQualityMod));
                if (rng > 10) return false;

                wo.EquipmentSetId = (EquipmentSet)ThreadSafeRandom.Next((int)EquipmentSet.Soldiers, (int)EquipmentSet.Lightningproof);
            }
            else
            {
                wo.EquipmentSetId = EquipmentSetChance.Roll(wo, profile, roll);
            }

            if (wo.EquipmentSetId != null && PropertyManager.GetBool("equipmentsetid_name_decoration").Item)
            {
                var equipSetId = wo.EquipmentSetId;

                var equipSetName = equipSetId.ToString();

                if (equipSetId >= EquipmentSet.Soldiers && equipSetId <= EquipmentSet.Crafters)
                    equipSetName = equipSetName.TrimEnd('s') + "'s";

                wo.Name = $"{equipSetName} {wo.Name}";
            }
            return true;
        }

        private static WorldObject CreateSocietyArmor(TreasureDeath profile, bool mutate = true)
        {
            int society = 0;
            int armortype = 0;

            if (profile.TreasureType >= 2971 && profile.TreasureType <= 2980)
                society = 0; // CH
            else if (profile.TreasureType >= 2981 && profile.TreasureType <= 2990)
                society = 1; // EW
            else if (profile.TreasureType >= 2991 && profile.TreasureType <= 3000)
                society = 2; // RB

            switch (profile.TreasureType)
            {
                case 2971:
                case 2981:
                case 2991:
                    armortype = 0; // BP
                    break;
                case 2972:
                case 2982:
                case 2992:
                    armortype = 1; // Gauntlets
                    break;
                case 2973:
                case 2983:
                case 2993:
                    armortype = 2; // Girth
                    break;
                case 2974:
                case 2984:
                case 2994:
                    armortype = 3; // Greaves
                    break;
                case 2975:
                case 2985:
                case 2995:
                    armortype = 4; // Helm
                    break;
                case 2976:
                case 2986:
                case 2996:
                    armortype = 5; // Pauldrons
                    break;
                case 2977:
                case 2987:
                case 2997:
                    armortype = 6; // Tassets
                    break;
                case 2978:
                case 2988:
                case 2998:
                    armortype = 7; // Vambraces
                    break;
                case 2979:
                case 2989:
                case 2999:
                    armortype = 8; // Sollerets
                    break;
                default:
                    break;
            }

            int societyArmorWeenie = LootTables.SocietyArmorMatrix[armortype][society];
            WorldObject wo = WorldObjectFactory.CreateNewWorldObject((uint)societyArmorWeenie);

            if (wo != null && mutate)
                MutateSocietyArmor(wo, profile, true);

            return wo;
        }

        private static void MutateSocietyArmor(WorldObject wo, TreasureDeath profile, bool isMagical, TreasureRoll roll = null)
        {
            // why is this a separate method??

            var materialType = GetMaterialType(wo, profile.Tier);
            if (materialType > 0)
                wo.MaterialType = materialType;

            if (wo.GemCode != null)
                wo.GemCount = GemCountChance.Roll(wo.GemCode.Value, profile.Tier);
            else
                wo.GemCount = ThreadSafeRandom.Next(1, 6);

            wo.GemType = RollGemType(profile.Tier);

            wo.ItemWorkmanship = WorkmanshipChance.Roll(profile.Tier);

            wo.Value = Roll_ItemValue(wo, profile.Tier);

            if (isMagical)
            {
                // looks like society armor always had impen on it
                AssignMagic(wo, profile, roll, true);
            }
            else
            {
                wo.ItemManaCost = null;
                wo.ItemMaxMana = null;
                wo.ItemCurMana = null;
                wo.ItemSpellcraft = null;
                wo.ItemDifficulty = null;
            }
            AssignArmorLevel(wo, profile.Tier, LootTables.ArmorType.SocietyArmor);

            wo.LongDesc = GetLongDesc(wo);

            // try mutate burden, if MutateFilter exists
            if (wo.HasMutateFilter(MutateFilter.EncumbranceVal))
                MutateBurden(wo, profile, false);
        }

        private static WorldObject CreateCloak(TreasureDeath profile, bool mutate = true)
        {
            // even chance between 11 different types of cloaks
            var cloakType = ThreadSafeRandom.Next(0, LootTables.Cloaks.Length - 1);

            var cloakWeenie = LootTables.Cloaks[cloakType];

            var wo = WorldObjectFactory.CreateNewWorldObject((uint)cloakWeenie);

            if (wo != null && mutate)
                MutateCloak(wo, profile);

            return wo;
        }

        private static void MutateCloak(WorldObject wo, TreasureDeath profile, TreasureRoll roll = null)
        {
            wo.ItemMaxLevel = CloakChance.Roll_ItemMaxLevel(profile);

            // wield difficulty, based on ItemMaxLevel
            switch (wo.ItemMaxLevel)
            {
                case 1:
                    wo.WieldDifficulty = 30;
                    break;
                case 2:
                    wo.WieldDifficulty = 60;
                    break;
                case 3:
                    wo.WieldDifficulty = 90;
                    break;
                case 4:
                    wo.WieldDifficulty = 120;
                    break;
                case 5:
                    wo.WieldDifficulty = 150;
                    break;
            }

            wo.IconOverlayId = IconOverlay_ItemMaxLevel[wo.ItemMaxLevel.Value - 1];

            // equipment set
            wo.EquipmentSetId = CloakChance.RollEquipmentSet();

            // proc spell
            var surgeSpell = CloakChance.RollProcSpell();

            if (surgeSpell != SpellId.Undef)
            {
                wo.ProcSpell = (uint)surgeSpell;

                // Cloaked In Skill is the only self-targeted spell
                if (wo.ProcSpell == (uint)SpellId.CloakAllSkill)
                    wo.ProcSpellSelfTargeted = true;
                else
                    wo.ProcSpellSelfTargeted = false;

                wo.CloakWeaveProc = 1;
            }
            else
            {
                // Damage Reduction proc
                wo.CloakWeaveProc = 2;
            }

            // material type
            wo.MaterialType = GetMaterialType(wo, profile.Tier);

            // workmanship
            wo.Workmanship = WorkmanshipChance.Roll(profile.Tier);

            if (roll != null && profile.Tier <= 8)
                TryMutateGearRating(wo, profile, roll);
            else if (roll != null && profile.Tier == 9)
                TryMutateGearRatingT9(wo, profile, roll);
            else if (roll != null && profile.Tier == 10)
                TryMutateGearRatingT10(wo, profile, roll);

            // item value
            //if (wo.HasMutateFilter(MutateFilter.Value))
            MutateValue(wo, profile.Tier, roll);
        }

        private static int RollCloak_ItemMaxLevel(TreasureDeath profile)
        {
            //  These Values are just for starting off.  I haven't gotten the numbers yet to confirm these.
            int cloakLevel = 1;

            int chance = ThreadSafeRandom.Next(1, 1000);
            switch (profile.Tier)
            {
                case 1:
                case 2:
                default:
                    cloakLevel = 1;
                    break;
                case 3:
                case 4:
                    if (chance <= 440)
                        cloakLevel = 1;
                    else
                        cloakLevel = 2;
                    break;
                case 5:
                    if (chance <= 250)
                        cloakLevel = 1;
                    else if (chance <= 700)
                        cloakLevel = 2;
                    else
                        cloakLevel = 3;
                    break;
                case 6:
                    if (chance <= 36)
                        cloakLevel = 1;
                    else if (chance <= 357)
                        cloakLevel = 2;
                    else if (chance <= 990)
                        cloakLevel = 3;
                    else
                        cloakLevel = 4;
                    break;
                case 7:  // From data, no chance to get a lvl 1 cloak
                    if (chance <= 463)
                        cloakLevel = 2;
                    else if (chance <= 945)
                        cloakLevel = 3;
                    else if (chance <= 984)
                        cloakLevel = 4;
                    else
                        cloakLevel = 5;
                    break;
                case 8:  // From data, no chance to get a lvl 1 cloak
                    if (chance <= 451)
                        cloakLevel = 2;
                    else if (chance <= 920)
                        cloakLevel = 3;
                    else if (chance <= 975)
                        cloakLevel = 4;
                    else
                        cloakLevel = 5;
                    break;
                case 9:  // From data, no chance to get a lvl 1 cloak
                    if (chance <= 451)
                        cloakLevel = 3;
                    else if (chance <= 920)
                        cloakLevel = 4;
                    else if (chance <= 975)
                        cloakLevel = 5;
                    else
                        cloakLevel = 5;
                    break;
            }
            return cloakLevel;
        }

        private static bool GetMutateCloakData(uint wcid)
        {
            return LootTables.Cloaks.Contains((int)wcid);
        }

        private static void MutateValue_Armor(WorldObject wo)
        {
            var bulkMod = wo.BulkMod ?? 1.0f;
            var sizeMod = wo.SizeMod ?? 1.0f;

            var armorLevel = wo.ArmorLevel ?? 0;

            // from the py16 mutation scripts
            //wo.Value += (int)(armorLevel * armorLevel / 10.0f * bulkMod * sizeMod);

            // still probably not how retail did it
            // modified for armor values to match closer to retail pcaps
            var minRng = (float)Math.Min(bulkMod, sizeMod);
            var maxRng = (float)Math.Max(bulkMod, sizeMod);

            var rng = ThreadSafeRandom.Next(minRng, maxRng);

            wo.Value += (int)(armorLevel * armorLevel / 10.0f * rng);
        }

        private static void MutateArmorModVsType(WorldObject wo, TreasureDeath profile)
        {
            // for the PropertyInt.MutateFilters found in py16 data,
            // items either had all of these, or none of these

            // only the elemental types could mutate
            TryMutateArmorModVsType(wo, profile, PropertyFloat.ArmorModVsFire);
            TryMutateArmorModVsType(wo, profile, PropertyFloat.ArmorModVsCold);
            TryMutateArmorModVsType(wo, profile, PropertyFloat.ArmorModVsAcid);
            TryMutateArmorModVsType(wo, profile, PropertyFloat.ArmorModVsElectric);
        }

        private static bool TryMutateArmorModVsType(WorldObject wo, TreasureDeath profile, PropertyFloat prop)
        {
            var armorModVsType = wo.GetProperty(prop);

            if (armorModVsType == null)
                return false;

            // perform the initial roll to determine if this ArmorModVsType will mutate
            var mutate = ArmorModVsTypeChance.Roll(profile.Tier);

            if (!mutate)
                return false;

            // get quality level 1-5 for tier
            var qualityLevel = ArmorModVsTypeChance.RollQualityLevel(profile);

            // add in rng
            // for t6+ / max quality level 5, the highest bonus found in eor data was ~0.9
            var rng = ThreadSafeRandom.Next(-0.05f, 0.15f);

            var bonusRL = qualityLevel * 0.15f + rng;

            //Console.WriteLine($"Boosting {wo.Name}.{prop} by {bonusRL}");

            armorModVsType += bonusRL;

            // ensure between -2.0 / 2.0?
            armorModVsType = Math.Clamp(armorModVsType.Value, -2.0f, 2.0f);

            wo.SetProperty(prop, armorModVsType.Value);

            return true;
        }

        private static bool TryMutateGearRating(WorldObject wo, TreasureDeath profile, TreasureRoll roll)
        {
            if (profile.Tier < 8 && profile.Tier != 9)
                return false;

            // shields don't have gear ratings
            if (wo.IsShield) return false;

            var gearRating = GearRatingChance.Roll(wo, profile, roll);

            if (gearRating == 0)
                return false;

            //Console.WriteLine($"TryMutateGearRating({wo.Name}, {profile.TreasureType}, {roll.ItemType}): rolled gear rating {gearRating}");

            var rng = ThreadSafeRandom.Next(0, 1);

            if (roll.HasArmorLevel(wo) && profile.Tier >= 7 && profile.Tier != 9)
            {
                // clothing w/ al, and crowns would be included in this group
                if (rng == 0)
                    wo.GearCritDamage = gearRating;
                else
                    wo.GearCritDamageResist = gearRating;
            }
            else if (roll.IsClothing || roll.IsCloak)
            {
                if (rng == 0)
                    wo.GearDamage = gearRating;
                else
                    wo.GearDamageResist = gearRating;
            }
            else if (roll.IsJewelry)
            {
                if (rng == 0)
                    wo.GearHealingBoost = gearRating;
                else
                    wo.GearMaxHealth = gearRating;
            }
            else
            {
                log.Error($"TryMutateGearRating({wo.Name}, {profile.TreasureType}, {roll.ItemType}): unknown item type");
                return false;
            }

            // ensure wield requirement is level 180?
            if (roll.ArmorType != TreasureArmorType.Society)
                SetWieldLevelReq(wo, 180);

            return true;

        }

        private static bool TryMutateGearRatingT9(WorldObject wo, TreasureDeath profile, TreasureRoll roll)
        {
            if (profile.Tier != 9)
                return false;

            // shields don't have gear ratings
            if (wo.IsShield) return false;

            var gearRatingT9 = GearRatingChance.RollT9(wo, profile, roll);

            if (gearRatingT9 == 0)
                return false;

            //Console.WriteLine($"TryMutateGearRating({wo.Name}, {profile.TreasureType}, {roll.ItemType}): rolled gear rating {gearRating}");

            var rngT9 = ThreadSafeRandom.Next(0, 1);

            if (roll.HasArmorLevel(wo) && profile.Tier == 9)

            {
                // clothing w/ al, and crowns would be included in this group
                if (rngT9 == 0)
                    wo.GearCritDamage = gearRatingT9;
                else
                    wo.GearCritDamageResist = gearRatingT9;
            }
            else if (roll.IsClothing || roll.IsCloak)
            {
                if (rngT9 == 0)
                    wo.GearDamage = gearRatingT9;
                else
                    wo.GearDamageResist = gearRatingT9;
            }

            else if (roll.IsJewelry)
            {
                if (rngT9 == 0)
                    wo.GearHealingBoost = gearRatingT9;
                else
                    wo.GearMaxHealth = gearRatingT9;
            }
            else
            {
                log.Error($"TryMutateGearRating({wo.Name}, {profile.TreasureType}, {roll.ItemType}): unknown item type");
                return false;
            }
            if (roll.ArmorType != TreasureArmorType.Society)
                SetWieldLevelReq(wo, 550);

            return true;
        }

        private static bool TryMutateGearRatingT10(WorldObject wo, TreasureDeath profile, TreasureRoll roll)
        {
            if (profile.Tier != 10)
                return false;

            bool applied = false;
            bool isArmorType = roll.HasArmorLevel(wo) || roll.IsClothing || roll.IsCloak;
            bool isJewelry = roll.IsJewelry;
            bool isShield = wo.IsShield;

            // 🛡️ Shields
            if (isShield)
            {
                // Always try rolling each type of rating
                void TryAssignRating(Action<int> assign)
                {
                    var val = GearRatingChance.RollT10(wo, profile, roll);
                    if (val > 0) assign(val);
                }

                TryAssignRating(v => wo.GearCritDamageResist = v);
                TryAssignRating(v => wo.GearDamageResist = v);
                TryAssignRating(v => wo.GearNetherResistRating = v);
                TryAssignRating(v => wo.GearCritDamage = v);
                TryAssignRating(v => wo.GearDamage = v);
                TryAssignRating(v => wo.GearHealingBoost = v);

                // 🛡️ Special: GearMaxHealth uses flat range instead of RollT10
                int healthBonus = ThreadSafeRandom.Next(100, 200); // Inclusive: 100–200
                wo.GearMaxHealth = healthBonus;

                // 10% chance to roll Magic Absorb
                if (ThreadSafeRandom.Next(0, 100) < 30)
                {
                    wo.AbsorbMagicDamage = 0.2f;
                }

                applied = true;
            }

            // 🎯 Armor/Underclothes/Cloaks
            else if (isArmorType)
            {
                var netherResist = GearRatingChance.RollT10(wo, profile, roll);
                if (netherResist > 0)
                    wo.GearNetherResistRating = netherResist;

                if (ThreadSafeRandom.Next(0, 2) == 0)
                {
                    var dmg = GearRatingChance.RollT10(wo, profile, roll);
                    if (dmg > 0) wo.GearDamage = dmg;
                }
                else
                {
                    var dmgResist = GearRatingChance.RollT10(wo, profile, roll);
                    if (dmgResist > 0) wo.GearDamageResist = dmgResist;
                }

                if (ThreadSafeRandom.Next(0, 2) == 0)
                {
                    var crit = GearRatingChance.RollT10(wo, profile, roll);
                    if (crit > 0) wo.GearCritDamage = crit;
                }
                else
                {
                    var critResist = GearRatingChance.RollT10(wo, profile, roll);
                    if (critResist > 0) wo.GearCritDamageResist = critResist;
                }

                applied = true;
            }

            // 💍 Jewelry
            else if (isJewelry)
            {
                var rating = GearRatingChance.RollT10(wo, profile, roll);
                if (rating > 0)
                {
                    if (ThreadSafeRandom.Next(0, 2) == 0)
                        wo.GearHealingBoost = rating;
                    else
                        wo.GearMaxHealth = rating;

                    applied = true;
                    SetWieldT10(wo, 725, profile.Tier);
                }
            }

            else
            {
                log.Error($"TryMutateGearRating({wo.Name}, {profile.TreasureType}, {roll.ItemType}): unknown item type");
                return false;
            }

            if (applied && roll.ArmorType != TreasureArmorType.Society)
            {
                SetWieldT10(wo, 725, profile.Tier);
            }

            return applied;
        }



        private static void SetWieldLevelReq(WorldObject wo, int level)
        {
            if (wo.WieldRequirements == WieldRequirement.Invalid)
            {
                wo.WieldRequirements = WieldRequirement.Level;
                wo.WieldSkillType = (int)Skill.Axe;  // set from examples in pcap data
                wo.WieldDifficulty = level;
            }
            else if (wo.WieldRequirements == WieldRequirement.Level)
            {
                if (wo.WieldDifficulty < level)
                    wo.WieldDifficulty = level;
            }
            else
            {
                // this can either be empty, or in the case of covenant / olthoi armor,
                // it could already contain a level requirement of 180, or possibly 150 in tier 8

                // we want to set this level requirement to 180, in all cases

                // magloot logs indicated that even if covenant / olthoi armor was not upgraded to 180 in its mutation script,
                // a gear rating could still drop on it, and would "upgrade" the 150 to a 180

                wo.WieldRequirements2 = WieldRequirement.Level;
                wo.WieldSkillType2 = (int)Skill.Axe;  // set from examples in pcap data
                wo.WieldDifficulty2 = level;
            }
        }

        private static void SetWieldT10(WorldObject wo, int requiredLevel, int tier)
        {
            if (tier < 10)
                return;

            // Use a placeholder Skill enum the client can display
            const Skill MeleeD = Skill.MeleeDefense;

            // Assign to primary or secondary wield requirement
            if (wo.WieldRequirements == WieldRequirement.Invalid)
            {
                wo.WieldRequirements = WieldRequirement.RawSkill;
                wo.WieldSkillType = (int)MeleeD;
                wo.WieldDifficulty = requiredLevel;
            }
            else if (wo.WieldRequirements == WieldRequirement.RawSkill && wo.WieldSkillType == (int)MeleeD)
            {
                if (wo.WieldDifficulty < requiredLevel)
                    wo.WieldDifficulty = requiredLevel;
            }
            else
            {
                wo.WieldRequirements2 = WieldRequirement.RawSkill;
                wo.WieldSkillType2 = (int)MeleeD;
                wo.WieldDifficulty2 = requiredLevel;
            }
        }

        private static bool GetMutateArmorData(uint wcid, out LootTables.ArmorType? armorType)
        {
            foreach (var kvp in LootTables.armorTypeMap)
            {
                armorType = kvp.Key;
                var table = kvp.Value;

                if (kvp.Value.Contains((int)wcid))
                    return true;
            }
            armorType = null;
            return false;
        }
    }
}
