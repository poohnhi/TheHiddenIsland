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
using HarmonyLib;
using System.Security.Cryptography.X509Certificates;
using ContentPatcher;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using StardewValley.Delegates;
// using SpaceCore;
// using SpaceCore.Events;

namespace The_Hidden_Island
{

    public partial class ModEntry : Mod
    {
        public static IModHelper Mhelper;
        public override void Entry(IModHelper helper)
        {
            Mhelper = helper;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            //helper.Events.Player.Warped += this.OnWarped;
            //helper.Events.Display.RenderedWorld += this.OnRenderedWorld;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            // example patch, you'll need to edit this for your patch
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), "performUseAction"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.performUseAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.NPC), "startRouteBehavior"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.startRouteBehavior_Postfix))
             );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.NPC), "finishRouteBehavior"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.finishRouteBehavior_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.NPC), "checkAction"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.checkAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Tool), "DoFunction"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.DoFunction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), "isActionableTile"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.isActionableTile_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), "answerDialogueAction"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.answerDialogueAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), "cleanupBeforePlayerExit"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.cleanupBeforePlayerExit_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), "resetLocalState"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.resetLocalState_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), "UpdateWhenCurrentLocation"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.UpdateWhenCurrentLocation_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), "drawAboveAlwaysFrontLayer"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.drawAboveAlwaysFrontLayer_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Event), "addSpecificTemporarySprite"),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.addSpecificTemporarySprite_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Event), nameof(Event.endBehaviors), new Type[] { typeof(string[]), typeof(GameLocation)}),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.endBehaviors_Prefix))
            );
            GameLocation.RegisterTileAction("poohnhi.WnS.CP_HouseClock", this.HandleClock);
            GameLocation.RegisterTileAction("poohnhi.WnS.CP_Diary", this.HandleDiary);


        }
        private bool HandleClock(GameLocation location, string[] action, Farmer player, Point point)
        {
            //GameLocation location, string[] args, Farmer player, Vector2 tile
            try
            {
                string currentDialogue = "";
                currentDialogue = Game1.content.LoadString("Strings\\StringsFromMaps:WnSHouse_Hallway_Clock");
                currentDialogue = currentDialogue.Replace("%time", Game1.getTimeOfDayString(Game1.timeOfDay));
                Game1.drawDialogueNoTyping(currentDialogue);
                //Game1.activeClickableMenu = new DialogueBox("This gate is locked. I wonder where the key is?");
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool HandleDiary(GameLocation location, string[] action, Farmer player, Point point)
        {
            //GameLocation location, string[] args, Farmer player, Vector2 tile
            string error;
            try
            {
                if (!ArgUtility.TryGet(action, 1, out var npcName3, out error) || !ArgUtility.TryGetRemainder(action, 2, out var rawMessage, out error))
                {
                    return false;
                }
                string message = rawMessage.Replace("\"", "");
                NPC npc2 = Game1.getCharacterFromName(npcName3);
                NPC tempNpc2 = Game1.getCharacterFromName("poohnhi.WnS.CP_InvisibleTempActor");
                if (npc2 != null && npc2.currentLocation == player.currentLocation && Utility.tileWithinRadiusOfPlayer(npc2.TilePoint.X, npc2.TilePoint.Y, 8, player))
                {
                    try
                    {
                        string str_name = message.Split('/')[0];
                        tempNpc2.setNewDialogue(str_name, add: true);
                        Game1.drawDialogue(tempNpc2);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                try
                {
                    // Game1.drawDialogueNoTyping(Game1.content.LoadString(message.Split('/')[1]));
                    string str_name = message.Split('/')[1];
                    tempNpc2.setNewDialogue(str_name, add: true);
                    Game1.drawDialogue(tempNpc2);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
/*        public static bool HARVEST_ITEM_PRICE_handleQueryMethod(string[] query, GameLocation location, Farmer player, Item targetItem, Item inputItem, Random random)
        {
            if (!GameStateQuery.Helpers.TryGetItemArg(query, 1, targetItem, inputItem, out var item, out var error) || !ArgUtility.TryGetInt(query, 2, out var minPrice, out error) || !ArgUtility.TryGetOptionalInt(query, 3, out var maxPrice, out error, int.MaxValue))
            {
                return GameStateQuery.Helpers.ErrorResult(query, error);
            }
            int? price = item?.salePrice();
            if (price >= minPrice)
            {
                return price <= maxPrice;
            }
            return false;
        }*/
        public void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
/*            if (!GameStateQuery.Exists("poohnhi.WnS.CP_HARVEST_ITEM_PRICE"))
            {
                MethodInfo methodInfo = typeof(GameStateQuery.DefaultResolvers).GetMethod("HARVEST_ITEM_PRICE_handleQueryMethod");
                GameStateQueryDelegate queryDelegate = (GameStateQueryDelegate)Delegate.CreateDelegate(typeof(GameStateQueryDelegate), methodInfo);
                GameStateQuery.Register("poohnhi.WnS.CP_HARVEST_ITEM_PRICE", queryDelegate);
            }*/
        }
        
        public void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            try
            {
            }
            catch
            {

            }
/*            foreach (string s in Game1.player.mailReceived)
            {
                if (s.Contains("WnS"))
                    this.Monitor.Log($"New day mails: {s}.", LogLevel.Error);
            }*/
        }
        public void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            /*            foreach (string s in Game1.player.mailReceived)
            {
                if (s.Contains("WnS"))
                    this.Monitor.Log($"Old day mails: {s}.", LogLevel.Error);
            }*/
        }
    }
}
