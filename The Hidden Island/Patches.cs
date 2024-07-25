using System;
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
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using HarmonyLib;
using System.Drawing;
using System.Security.Cryptography;
using Color = Microsoft.Xna.Framework.Color;
using static StardewValley.Minigames.CraneGame;
using xTile.Layers;
using xTile.Tiles;
using StardewValley.ItemTypeDefinitions;
using StardewValley.SpecialOrders;
using System.Threading;
using Point = Microsoft.Xna.Framework.Point;
using static System.Net.WebRequestMethods;
using StardewValley.GameData;

namespace The_Hidden_Island
{
    internal class Patches
    {
        // this is a mess... why am I even do this... why can't you be normal...
        private static Multiplayer multiplayer = ModEntry.Mhelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
        private static IMonitor Monitor;
        static int randomNumber = 0;
        static Color color1 = Color.Indigo;
        static Color color2 = Color.Goldenrod;
        // call this method from your Entry class
        internal static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
        // patches need to be static!
        private static void totemWarpHiddenIsland(Farmer who)
        {
            try
            {
                GameLocation currentLocation = who.currentLocation;
                for (int j = 0; j < 12; j++)
                {
                    multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
                }
                who.playNearbySoundAll("wand");
                Game1.displayFarmer = false;
                Game1.player.temporarilyInvincible = true;
                Game1.player.temporaryInvincibilityTimer = -2000;
                Game1.player.freezePause = 1000;
                Game1.flashAlpha = 1f;
                Color glowColor = randomNumber == 0 ? color2 : color1;
                Game1.screenGlowOnce(glowColor, hold: false);
                DelayedAction.fadeAfterDelay(totemWarpForRealHiddenIsland, 1000);
                Microsoft.Xna.Framework.Rectangle rectangle = who.GetBoundingBox();
                new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, 64, 64).Inflate(192, 192);
                int num = 0;
                Microsoft.Xna.Framework.Point tilePoint = who.TilePoint;
                for (int num2 = tilePoint.X + 8; num2 >= tilePoint.X - 8; num2--)
                {
                    multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(6, new Vector2(num2, tilePoint.Y) * 64f, Color.White, 8, flipped: false, 50f)
                    {
                        layerDepth = 1f,
                        delayBeforeAnimationStart = num * 25,
                        motion = new Vector2(-0.25f, 0f)
                    });
                    num++;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in totem warp to Hidden Island:\n{ex}", LogLevel.Error);
            }
}
        private static void totemWarpForRealHiddenIsland()
        {
            try
            {
                Game1.warpFarmer("poohnhi.WnS.CP_HiddenIsland", 17, 23, flip: false);
                Game1.fadeToBlackAlpha = 0.99f;
                Game1.screenGlow = false;
                Game1.player.temporarilyInvincible = false;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.displayFarmer = true;
                randomNumber = new Random().Next(2);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in totem warp to Hidden Island:\n{ex}", LogLevel.Error);
            }
        }
        internal static bool answerDialogueAction_Prefix(StardewValley.GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (__result == false)
                {
                    switch (questionAndAnswer)
                    {
                        case null:
                            return false;
                        case "EatSleepingPill_Yes":
                            Game1.player.isEating = false;
                            Game1.player.eatHeldObject();
                            string debugCommand = "sleep";
                            Game1.game1.parseDebugInput(debugCommand);
                            __result = true;
                            break;
                        case "EatSleepingPill_No":
                            Game1.player.isEating = false;
                            Game1.player.completelyStopAnimatingOrDoingAction();
                            __result = true;
                            break;
                    }
                }
                return true; // don't run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(answerDialogueAction_Prefix)}:\n{ex}", LogLevel.Error);
                return false; // run original logic
            }
            
        }

        internal static bool performUseAction_Prefix(StardewValley.Object __instance, GameLocation location, ref bool __result)
        {
            try
            {
                if (__result == false)
                {
                    if (!Game1.player.canMove || __instance.isTemporarilyInvisible)
                    {
                        return false;
                    }
                    bool flag = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming && !Game1.player.bathingClothes && !Game1.player.onBridge.Value;
                    if ((flag) && (__instance.QualifiedItemId == "(O)poohnhi.WnS.CP_SleepingPills"))
                        {
                        Game1.player.faceDirection(2);
                        Game1.player.itemToEat = Game1.player.ActiveObject;
                        Game1.player.FarmerSprite.setCurrentSingleAnimation(304);
                        if (Game1.objectData.TryGetValue(Game1.player.ActiveObject.ItemId, out var objectData))
                        {
                            Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:poohnhi.WnS.AskIfTakePill", Game1.player.ActiveObject.DisplayName), Game1.currentLocation.createYesNoResponses(), "EatSleepingPill");
                        }
                        return false;
                        }
                    if ((flag) && (__instance.QualifiedItemId == "(O)poohnhi.WnS.CP_HiddenIslandWarpTotem"))
                    {
                        bool check_condition = false;
                        if (!Game1.player.mailReceived.Contains("WnSAllowedToVisitOnWeekends"))
                        {
                            if (Game1.dayOfMonth % 7 == 6 || Game1.dayOfMonth % 7 == 0)
                            {
                                // Warn the player not to visit if it is Saturday or Sunday
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WnSWeekendsWarning"));
                                check_condition = false;
                            }
                            else
                                check_condition = true;
                        }
                        else
                        {
                            check_condition = true;
                        }
                        if (!check_condition)
                        {
                            return false;
                        }

                        Game1.player.jitterStrength = 1f;
                        Color glowColor = randomNumber == 0 ? color1 : color2;
                        location.playSound("warrior");
                        Game1.player.faceDirection(2);
                        Game1.player.CanMove = false;
                        Game1.player.temporarilyInvincible = true;
                        Game1.player.temporaryInvincibilityTimer = -4000;
                        Game1.changeMusicTrack("silence");
                        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
                        {
                            new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
                            new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, totemWarpHiddenIsland, behaviorAtEndOfFrame: true)
                        });

                        TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                        {
                            motion = new Vector2(0f, -1f),
                            scaleChange = 0.01f,
                            alpha = 1f,
                            alphaFade = 0.0075f,
                            shakeIntensity = 1f,
                            initialPosition = Game1.player.Position + new Vector2(0f, -96f),
                            xPeriodic = true,
                            xPeriodicLoopTime = 1000f,
                            xPeriodicRange = 4f,
                            layerDepth = 1f
                        };
                        temporaryAnimatedSprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
                        multiplayer.broadcastSprites(location, temporaryAnimatedSprite);
                        temporaryAnimatedSprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                        {
                            motion = new Vector2(0f, -0.5f),
                            scaleChange = 0.005f,
                            scale = 0.5f,
                            alpha = 1f,
                            alphaFade = 0.0075f,
                            shakeIntensity = 1f,
                            delayBeforeAnimationStart = 10,
                            initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
                            xPeriodic = true,
                            xPeriodicLoopTime = 1000f,
                            xPeriodicRange = 4f,
                            layerDepth = 0.9999f
                        };
                        temporaryAnimatedSprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
                        multiplayer.broadcastSprites(location, temporaryAnimatedSprite);
                        temporaryAnimatedSprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
                        {
                            motion = new Vector2(0f, -0.5f),
                            scaleChange = 0.005f,
                            scale = 0.5f,
                            alpha = 1f,
                            alphaFade = 0.0075f,
                            delayBeforeAnimationStart = 20,
                            shakeIntensity = 1f,
                            initialPosition = Game1.player.Position + new Vector2(64f, -96f),
                            xPeriodic = true,
                            xPeriodicLoopTime = 1000f,
                            xPeriodicRange = 4f,
                            layerDepth = 0.9988f
                        };
                        temporaryAnimatedSprite.CopyAppearanceFromItemId(__instance.QualifiedItemId);
                        multiplayer.broadcastSprites(location, temporaryAnimatedSprite);
                        Game1.screenGlowOnce(glowColor, hold: false);
                        Utility.addSprinklesToLocation(location, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
                        __result = true;
                        return false;
                    }
                }
                return true; // don't run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performUseAction_Prefix)}:\n{ex}", LogLevel.Error);
                return false; // run original logic
            }
        }
        internal static void finishRouteBehavior_Prefix(StardewValley.NPC __instance, string behaviorName)
        {
            try
            {
                GameLocation gameLocation = (Utility.getGameLocationOfCharacter(__instance));
                switch (behaviorName)
                {
                    case "wns.winnie_coffee":
                        Utility.getGameLocationOfCharacter(__instance).removeTemporarySpritesWithID(18625003);
                        if (Game1.player.dialogueQuestionsAnswered.Contains("WnSWFCYes"))
                            Utility.getGameLocationOfCharacter(__instance).removeTemporarySpritesWithID(18625004);
                        break;
                    case "wns.winnie_sit_kitchen_chair":
                        Utility.getGameLocationOfCharacter(__instance).removeTemporarySpritesWithID(18625001);
                        Utility.getGameLocationOfCharacter(__instance).removeTemporarySpritesWithID(18625002);
                        break;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(finishRouteBehavior_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        internal static void startRouteBehavior_Postfix(StardewValley.NPC __instance, string behaviorName)
        {
            try
            {
                if (behaviorName.Length > 0 && behaviorName[0] == '"')
                {
                    return;
                }
                GameLocation gameLocation = (Utility.getGameLocationOfCharacter(__instance));
                switch (behaviorName)
                {
                    case "wns.winnie_coffee":
                        if (Game1.IsMasterGame)
                        {
                            string breakfastdrinkID = "";
                            gameLocation.TryGetMapProperty("poohnhi.WnS.CP.WinnieBreakfastDrink", out breakfastdrinkID);
                            try
                            {
                                Item this_drink = ItemRegistry.Create(breakfastdrinkID);
                                ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem(this_drink.QualifiedItemId);
                                multiplayer.broadcastSprites(gameLocation, new TemporaryAnimatedSprite(dataOrErrorItem2.TextureName, dataOrErrorItem2.GetSourceRect(0, this_drink.ParentSheetIndex), 100f, 1, 999999, new Vector2(26, 29f) * 64f + new Vector2(0f, 3f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 2f, 0f, 0f, 0f)
                                {
                                    drawAboveAlwaysFront = true,
                                    id = 18625003
                                });
                                multiplayer.broadcastSprites(gameLocation, new TemporaryAnimatedSprite(dataOrErrorItem2.TextureName, dataOrErrorItem2.GetSourceRect(0, this_drink.ParentSheetIndex), 100f, 1, 999999, new Vector2(26, 29f) * 64f + new Vector2(8f, 7f) * 4f, flicker: false, flipped: true, 0.0002f, 0f, Color.White, 2f, 0f, 0f, 0f)
                                {
                                    drawAboveAlwaysFront = true,
                                    id = 18625004
                                });
                                __instance.doEmote(56);
                            }
                            catch
                            {

                            }
                        }
                        break;
                    case "wns.winnie_sit_kitchen_chair":
                        if (Game1.IsMasterGame)
                        {
                            string breakfastID = "";
                            string breakfastdrinkID = "";
                            gameLocation.TryGetMapProperty("poohnhi.WnS.CP.WinnieBreakfast", out breakfastID);
                            gameLocation.TryGetMapProperty("poohnhi.WnS.CP.WinnieBreakfastDrink", out breakfastdrinkID);
                            try
                            {
                                Item this_food = ItemRegistry.Create(breakfastID);
                                ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this_food.QualifiedItemId);
                                multiplayer.broadcastSprites(gameLocation, new TemporaryAnimatedSprite(dataOrErrorItem.TextureName, dataOrErrorItem.GetSourceRect(0, this_food.ParentSheetIndex), 100f, 1, 999999, new Vector2(24f, 29f) * 64f + new Vector2(3f, 6f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 3f, 0f, 0f, 0f)
                                {
                                    drawAboveAlwaysFront = true,
                                    id = 18625001
                                });
                                Item this_drink = ItemRegistry.Create(breakfastdrinkID);
                                ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem(this_drink.QualifiedItemId);
                                multiplayer.broadcastSprites(gameLocation, new TemporaryAnimatedSprite(dataOrErrorItem2.TextureName, dataOrErrorItem2.GetSourceRect(0, this_drink.ParentSheetIndex), 100f, 1, 999999, new Vector2(24f, 30f) * 64f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 2f, 0f, 0f, 0f)
                                {
                                    drawAboveAlwaysFront = true,
                                    id = 18625002
                                });
                                __instance.doEmote(56);
                            }
                            catch
                            {

                            }
                        }
                        break;
                    // case "dick_fish":
                    //     extendSourceRect(0, 32);
                    //     Sprite.tempSpriteHeight = 64;
                    //     drawOffset = new Vector2(0f, 96f);
                    //     Sprite.ignoreSourceRectUpdates = false;
                    //     if (Utility.isOnScreen(Utility.Vector2ToPoint(base.Position), 64, base.currentLocation))
                    //     {
                    //         base.currentLocation.playSound("slosh", base.Tile);
                    //     }
                    //     break;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(startRouteBehavior_Postfix)}:\n{ex}", LogLevel.Error);

            }
        }
        internal static bool checkAction_Prefix(StardewValley.NPC __instance, Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                if (__instance.Name == "WnS.Winnie" && (Utility.getGameLocationOfCharacter(__instance).Name) == "poohnhi.WnS.CP_House" && __instance.TilePoint.X == 9 && __instance.TilePoint.Y == 15 && Game1.timeOfDay <= 1230)
                {
                    if (!__instance.isEmoting)
                    {
                        __instance.doEmote(24);
                    }
                    __instance.shake(250);
                    Game1.DrawDialogue(__instance, "Strings\\schedules\\WnS.Winnie:MorningSleep");
                    return false;
                }
                if (__instance.Name == "WnS.Winnie" && (Utility.getGameLocationOfCharacter(__instance).Name) == "poohnhi.WnS.CP_House" && __instance.TilePoint.X == 9 && __instance.TilePoint.Y == 15 && Game1.timeOfDay > 1230 && Game1.timeOfDay <= 2100)
                {
                    if (!__instance.isEmoting)
                    {
                        __instance.doEmote(24);
                    }
                    __instance.shake(250);
                    Game1.DrawDialogue(__instance, "Strings\\schedules\\WnS.Winnie:AfternoonSleep");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(checkAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
        internal static void wandWarpForReal_Postfix(StardewValley.Tools.Wand __instance)
        {
            try
            {
                if (__instance.ItemId == "poohnhi.WnS.CP_HiddenIslandScepter")
                {
                    Farmer lastUser = __instance.getLastFarmerToUse();
                    Game1.warpFarmer("poohnhi.WnS.CP_House", 9, 11, flip: false);
                    Game1.fadeToBlackAlpha = 0.99f;
                    Game1.screenGlow = false;
                    lastUser.temporarilyInvincible = false;
                    lastUser.temporaryInvincibilityTimer = 0;
                    Game1.displayFarmer = true;
                    lastUser.CanMove = true;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(wandWarpForReal_Postfix)}:\n{ex}", LogLevel.Error);
            }

        }
        internal static bool isActionableTile_Prefix(StardewValley.GameLocation __instance, int xTile, int yTile, Farmer who, ref bool __result)
        {
            try
            {
                string[] action = ArgUtility.SplitBySpace(__instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings"));
                if (action.Length != 0)
                {
                    switch (action[0])
                    {
                        case "poohnhi.WnS.CP_Diary":
                        case "poohnhi.WnS.CP_HouseClock":
                            Game1.isInspectionAtCurrentCursorTile = true;
                            return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(isActionableTile_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
        internal static void endBehaviors_Prefix(StardewValley.Event __instance, string[] args, GameLocation location)
        {
            try
            {
                if (Game1.getMusicTrackName().Contains(Game1.currentSeason) && ArgUtility.Get(__instance.eventCommands, 0) != "continue")
                {
                    Game1.stopMusicTrack(MusicContext.Default);
                }
                switch (ArgUtility.Get(args, 1))
                {
                    case "WnSIntroCGDeath":
                        {
                            Game1.playSound("death");
                            Game1.player.health = -1;                            
                            Game1.eventOver = true;
                            __instance.CurrentCommand += 2;
                            Game1.screenGlowHold = false;
                            Game1.screenGlowOnce(Color.Black, hold: true, 1f, 1f);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(endBehaviors_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        internal static void addSpecificTemporarySprite_Postfix(StardewValley.Event __instance, string key, GameLocation location, string[] args)
        {
            try
            {
                switch (key)
                {
                    case "WnSObeliskWarp":
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                __instance.farmer.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)__instance.farmer.position.X - 256, (int)__instance.farmer.position.X + 192), Game1.random.Next((int)__instance.farmer.position.Y - 256, (int)__instance.farmer.position.Y + 192)), flicker: false, Game1.random.NextBool()));
                            }
                            Microsoft.Xna.Framework.Rectangle playerBounds = __instance.farmer.GetBoundingBox();
                            new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
                            int j = 0;
                            Point playerTile = __instance.farmer.TilePoint;
                            for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
                            {
                                __instance.farmer.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
                                {
                                    layerDepth = 1f,
                                    delayBeforeAnimationStart = j * 25,
                                    motion = new Vector2(-0.25f, 0f)
                                });
                                j++;
                            }
                            break;
                        }
                    case "WnSIntroCGChange1":
                        {
                            for (int i = Game1.currentLocation.temporarySprites.Count - 1; i >= 0; i--)
                            {
                                if (Game1.currentLocation.temporarySprites[i].textureName == "Mods\\poohnhi.WnS.CP\\IntroCGSprite")
                                {
                                    Game1.currentLocation.temporarySprites[i].sourceRect.Y = 320;
                                    Game1.currentLocation.temporarySprites[i].sourceRectStartingPos.Y = 320f;
                                    break;
                                }
                            }
                            break;
                        }
                    case "WnSIntroCGChange2":
                        {
                            for (int i = Game1.currentLocation.temporarySprites.Count - 1; i >= 0; i--)
                            {
                                if (Game1.currentLocation.temporarySprites[i].textureName == "Mods\\poohnhi.WnS.CP\\IntroCGSprite")
                                {
                                    Game1.currentLocation.temporarySprites[i].sourceRect.Y = 640;
                                    Game1.currentLocation.temporarySprites[i].sourceRectStartingPos.Y = 640f;

                                    break;
                                }
                            }
                            break;
                        }
                    case "WnSIntroCGChange3":
                        {
                            for (int i = Game1.currentLocation.temporarySprites.Count - 1; i >= 0; i--)
                            {
                                if (Game1.currentLocation.temporarySprites[i].textureName == "Mods\\poohnhi.WnS.CP\\IntroCGSprite")
                                {
                                    Game1.currentLocation.temporarySprites[i].sourceRect.Y = 960;
                                    Game1.currentLocation.temporarySprites[i].sourceRectStartingPos.Y = 960f;
                                    break;
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addSpecificTemporarySprite_Postfix)}:\n{ex}", LogLevel.Error);
            }

        }
    }
}
