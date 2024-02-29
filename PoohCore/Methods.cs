using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile;
using xTile.Dimensions;
using System.Collections.Generic;
using Netcode;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Inventories;
using xTile.Tiles;
using StardewValley.Extensions;
using static System.Collections.Specialized.BitVector32;
using StardewValley.Delegates;
using StardewValley.TokenizableStrings;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;
using Microsoft.Xna.Framework.Media;
using System.Linq;
using static StardewValley.GameStateQuery;

namespace PoohCore
{
    public partial class ModEntry
    {
        /// <summary>A token which returns the player's initials, or the initials of the input name.</summary>
        /// numbers.Add(1);
        public static string GetMailFlagProgressNumberFromMailFlag(string mailFlag)
        {
            string currentNum = "0";
            foreach (string s in Game1.player.mailReceived)
            {
                if (s.Contains(mailFlag) && s.Length != mailFlag.Length)
                {
                    string getNumMailFlag = s.Replace(mailFlag, "");
                    int num = Int32.Parse(getNumMailFlag);
                    currentNum = num.ToString();
                    break;
                }
            }
            return currentNum;
        }
        public static string GetNextNumberFromString(string number)
        {
            int num = Int32.Parse(number);
            num += 1;
            return num.ToString();
        }

        internal class GetMailFlagProgressNumberToken
        {
            private string MailFlag = "";
            public bool AllowsInput()
            {
                return true;
            }
            public bool RequiresInput()
            {
                return true;
            }
            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }
            public bool UpdateContext()
            {
                string check = "";
                string oldCheck = this.MailFlag;
                return check != oldCheck;
            }
            public IEnumerable<string> GetValues(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    yield break;
                string[] tempInput = input.Split(' ');
                string thisMailFlag = tempInput[0];
                this.MailFlag = thisMailFlag;
                if (!ModEntry.ListOfMailFlag.Contains(thisMailFlag))
                {
                    ModEntry.ListOfMailFlag.Add(thisMailFlag);
                }
                if (GetNextNumberFromString(GetMailFlagProgressNumberFromMailFlag(thisMailFlag)) != null && GetNextNumberFromString(GetMailFlagProgressNumberFromMailFlag(thisMailFlag)) != "")
                    yield return GetNextNumberFromString(GetMailFlagProgressNumberFromMailFlag(thisMailFlag));
                else yield return "1";
            }
        }
        internal class GetGiftToken
        {
            private string NPCName = "";
            public bool AllowsInput()
            {
                return true;
            }
            public bool RequiresInput()
            {
                return true;
            }
            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }
            public bool UpdateContext()
            {
                string check = "";
                string oldCheck = this.NPCName;
                return check != oldCheck;
            }
            public IEnumerable<string> GetValues(string input)
            {

                if (string.IsNullOrWhiteSpace(input))
                    yield break;
                string[] tempInput = input.Split(' ');
                string NPCInternalName = tempInput[0];
                string giftTaste = tempInput[1];
                string relativeCheck = tempInput[2];

                // get initials

                if (relativeCheck == "true" || relativeCheck == "True")
                {
                    string relativeName = GetRelativeNPC(NPCInternalName);
                    this.NPCName = relativeName;
                    yield return GetIDGenerator(relativeName, giftTaste);
                }
                else
                {
                    this.NPCName = NPCInternalName;
                    yield return GetIDGenerator(NPCInternalName, giftTaste);
                }

            }
        }
        internal class GetRelativeNameToken
        {
            private string relativeName;

            public bool AllowsInput()
            {
                return true;
            }
            public bool RequiresInput()
            {
                return true;
            }
            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }

            public IEnumerable<string> GetValues(string input)
            {
                string relativeName = GetRelativeNPC(input);
                this.relativeName = relativeName;

                yield return relativeName;
            }
        }
        internal class GetItemNameToken
        {
            private string itemName = "";

            public bool AllowsInput()
            {
                return true;
            }

            public bool RequiresInput()
            {
                return true;
            }

            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }
            public bool UpdateContext()
            {
                string check = "";
                string oldCheck = this.itemName;
                return check != oldCheck;
            }
            public IEnumerable<string> GetValues(string input)
            {
                
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(input);
                string itemName = itemData.DisplayName;
                this.itemName = itemName;
                yield return itemName;
            }
        }
        internal class GetRelativeDisplayNameToken
        {
            private string NPCname;
            public bool AllowsInput()
            {
                return true;
            }
            public bool RequiresInput()
            {
                return true;
            }
            public bool CanHaveMultipleValues(string input = null)
            {
                return false;
            }
            public bool UpdateContext()
            {
                string check = "";
                string oldCheck = this.NPCname;
                return check != oldCheck;
            }
            public IEnumerable<string> GetValues(string input)
            {
                NPC character = Game1.getCharacterFromName(input);
                if (character != null)
                {
                    this.NPCname = character.displayName;
                    yield return character.displayName;
                }
                yield break;
            }
        }
        public static string GetIDGenerator(string internalNPC, string giftTaste)
        {
            // giftTaste is loved, liked, ... hated
            // internalNPC is the internal NPC name that it try to get gift taste
            NPC npc = Game1.getCharacterFromName(internalNPC);
            if (npc!=null)
            {
                Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 77);
                CharacterData npcData = npc.GetData();
                Dictionary<string, string> npcGiftTastes = DataLoader.NpcGiftTastes(Game1.content);
                if (npcGiftTastes.TryGetValue(internalNPC, out var rawGiftTasteData))
                {
                    string[] rawGiftTasteFields = rawGiftTasteData.Split('/');
                    int tempNum = 9;
                    switch (giftTaste)
                    {
                        case "loved":
                        case "love":
                            tempNum = 1;
                            break;
                        case "liked":
                        case "like":
                            tempNum = 3;
                            break;
                        case "disliked":
                        case "dislike":
                            tempNum = 5;
                            break;
                        case "hated":
                        case "hate":
                            tempNum = 7;
                            break;
                    }
                    string[] Items = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, tempNum));
                    string item;
                    item = r.Choose(Items);
                    return item;
                }
            }
            
            return "74";
        }
        public static string GetRelativeNPC(string internalNPC)
        {

            NPC npc = Game1.getCharacterFromName(internalNPC);
            if (npc!=null)
            {
                Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 77);
                CharacterData npcData = npc.GetData();
                if (npcData?.FriendsAndFamily != null && Utility.TryGetRandom(npcData.FriendsAndFamily, out var relativeName, out var relativeTitle))
                    return relativeName;
            }
               
            return "";
        }

        public static bool GetRandomItemIDForGiftTaste(string[] query, out string replacement, Random random, Farmer player)
        {
            if (!ArgUtility.TryGet(query, 1, out var NPCInternalName, out var error) || !ArgUtility.TryGet(query, 2, out var giftTaste, out error) || !ArgUtility.TryGet(query, 3, out var relativeCheck, out error))
            {
                return TokenParser.LogTokenError(query, error, out replacement);
            }
            if (relativeCheck == "true" || relativeCheck == "True")
            {
                string relativeName = GetRelativeNPC(NPCInternalName);
                replacement = GetIDGenerator(relativeName, giftTaste);
                return true;
            }
            replacement = GetIDGenerator(NPCInternalName, giftTaste);
            return true;
        }
        public static bool GetRelativeNameForGiftTaste(string[] query, out string replacement, Random random, Farmer player)
        {
            if (!ArgUtility.TryGet(query, 1, out var NPCInternalName, out var error))
            {
                return TokenParser.LogTokenError(query, error, out replacement);
            }
            string relativeName = GetRelativeNPC(NPCInternalName);
            replacement = relativeName;
            return true;

        }
    }

}