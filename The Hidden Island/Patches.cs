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
                    bool flag = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming.Value && !Game1.player.bathingClothes.Value && !Game1.player.onBridge.Value;
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

        private static void Winnie_wandWarpForReal()
        {
            Tool __instance = Game1.player.CurrentTool;
            Game1.warpFarmer("poohnhi.WnS.CP_House", 9, 11, flip: false);
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            __instance.lastUser.temporarilyInvincible = false;
            __instance.lastUser.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
            __instance.lastUser.CanMove = true;
        }

        internal static void DoFunction_Prefix(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            try
            {
                //System.Console.WriteLine("test");
                //Monitor.Log($"Item ID is {__instance.ItemId}:\n", LogLevel.Error); cannot use monitor here...
                if (__instance.ItemId == "poohnhi.WnS.CP_HiddenIslandScepter" && !who.bathingClothes.Value && who.IsLocalPlayer && !who.onBridge.Value)
                    {
                        __instance.IndexOfMenuItemView = 2;
                        __instance.CurrentParentTileIndex = 2;
                        for (int i = 0; i < 12; i++)
                        {
                            multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.position.X - 256, (int)who.position.X + 192), Game1.random.Next((int)who.position.Y - 256, (int)who.position.Y + 192)), flicker: false, Game1.random.NextBool()));
                        }

                        if (__instance.PlayUseSounds)
                        {
                            who.playNearbySoundAll("wand");
                        }

                        Game1.displayFarmer = false;
                        who.temporarilyInvincible = true;
                        who.temporaryInvincibilityTimer = -2000;
                        who.Halt();
                        who.faceDirection(2);
                        who.CanMove = false;
                        who.freezePause = 2000;
                        Game1.flashAlpha = 1f;
                        DelayedAction.fadeAfterDelay(Winnie_wandWarpForReal, 1000);
                        Microsoft.Xna.Framework.Rectangle boundingBox = who.GetBoundingBox();
                        new Microsoft.Xna.Framework.Rectangle(boundingBox.X, boundingBox.Y, 64, 64).Inflate(192, 192);
                        int num = 0;
                        Point tilePoint = who.TilePoint;
                        for (int num2 = tilePoint.X + 8; num2 >= tilePoint.X - 8; num2--)
                        {
                            multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(6, new Vector2(num2, tilePoint.Y) * 64f, Color.White, 8, flipped: false, 50f)
                            {
                                layerDepth = 1f,
                                delayBeforeAnimationStart = num * 25,
                                motion = new Vector2(-0.25f, 0f)
                            });
                            num++;
                        }

                    __instance.CurrentParentTileIndex = __instance.IndexOfMenuItemView;
                    }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoFunction_Prefix)}:\n{ex}", LogLevel.Error);
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
        private static List<Vector2> baubles;

        private static List<WeatherDebris> weatherDebris;
        protected static Color _ambientLightColor = Color.White;

        internal static void resetLocalState_Postfix(StardewValley.GameLocation __instance)
        {
            try
            {
                if (__instance.Name == "poohnhi.WnS.CP_HiddenIslandCentral")
                {
                    _ambientLightColor = new Color(150, 120, 50);
                    __instance.ignoreOutdoorLighting.Value = false;
                    _updateWoodsLighting();
                    Random random = Utility.CreateDaySaveRandom();
                    int num = 25 + random.Next(0, 75);
                    if (!__instance.IsRainingHere())
                    {
                        baubles = new List<Vector2>();
                        for (int i = 0; i < num; i++)
                        {
                            baubles.Add(new Vector2(Game1.random.Next(0, __instance.map.DisplayWidth), Game1.random.Next(0, __instance.map.DisplayHeight)));
                        }

                        Season season = __instance.GetSeason();
                        if (season != Season.Winter)
                        {
                            weatherDebris = new List<WeatherDebris>();
                            int num2 = 192;
                            int which = 1;
                            if (season == Season.Fall)
                            {
                                which = 2;
                            }

                            for (int j = 0; j < num; j++)
                            {
                                weatherDebris.Add(new WeatherDebris(new Vector2(j * num2 % Game1.graphics.GraphicsDevice.Viewport.Width + Game1.random.Next(num2), j * num2 / Game1.graphics.GraphicsDevice.Viewport.Width * num2 % Game1.graphics.GraphicsDevice.Viewport.Height + Game1.random.Next(num2)), which, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
                            }
                        }
                    }

                    if (Game1.timeOfDay < 1200)
                    {
                        return;
                    }

                    Random random2 = new Random((int)(Game1.stats.DaysPlayed + 15));
                    int endTime = Utility.ModifyTime(1920, random2.Next(390));
                    int num3 = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, endTime) * Game1.realMilliSecondsPerGameMinute;
                    if (num3 <= 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
            }
            
            
        }
        protected static void _updateWoodsLighting()
        {
            try
            {
                if (Game1.currentLocation.Name != "poohnhi.WnS.CP_HiddenIslandCentral")
                {
                    return;
                }
                GameLocation thisLocation = Game1.currentLocation;
                int num = Utility.ConvertTimeToMinutes(Game1.getStartingToGetDarkTime(thisLocation));
                int num2 = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime(thisLocation));
                int num3 = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime(thisLocation));
                int num4 = Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime(thisLocation));
                float num5 = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameMinute;
                float t = Utility.Clamp((num5 - (float)num) / (float)(num2 - num), 0f, 1f);
                float t2 = Utility.Clamp((num5 - (float)num3) / (float)(num4 - num3), 0f, 1f);
                Game1.ambientLight.R = (byte)Utility.Lerp((int)_ambientLightColor.R, (int)Math.Max(_ambientLightColor.R, Game1.isRaining ? Game1.ambientLight.R : Game1.outdoorLight.R), t);
                Game1.ambientLight.G = (byte)Utility.Lerp((int)_ambientLightColor.G, (int)Math.Max(_ambientLightColor.G, Game1.isRaining ? Game1.ambientLight.G : Game1.outdoorLight.G), t);
                Game1.ambientLight.B = (byte)Utility.Lerp((int)_ambientLightColor.B, (int)Math.Max(_ambientLightColor.B, Game1.isRaining ? Game1.ambientLight.B : Game1.outdoorLight.B), t);
                Game1.ambientLight.A = (byte)Utility.Lerp((int)_ambientLightColor.A, (int)Math.Max(_ambientLightColor.A, Game1.isRaining ? Game1.ambientLight.A : Game1.outdoorLight.A), t);
                Color black = Color.Black;
                black.A = (byte)Utility.Lerp(255f, 0f, t2);
                foreach (LightSource value in Game1.currentLightSources.Values)
                {
                    if (value.lightContext.Value == LightSource.LightContext.MapLight)
                    {
                        value.color.Value = black;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in update wood lighting:\n{ex}", LogLevel.Error);
            }
            
        }
        
        internal static void UpdateWhenCurrentLocation_Postfix(StardewValley.GameLocation __instance, GameTime time)
        {
            try
            {
                if (__instance.Name == "poohnhi.WnS.CP_HiddenIslandCentral")
                {
                    _updateWoodsLighting();
                    if (baubles != null)
                    {
                        for (int i = 0; i < baubles.Count; i++)
                        {
                            Vector2 value = default(Vector2);
                            value.X = baubles[i].X - Math.Max(0.4f, Math.Min(1f, (float)i * 0.01f)) - (float)((double)((float)i * 0.01f) * Math.Sin(Math.PI * 2.0 * (double)time.TotalGameTime.Milliseconds / 8000.0));
                            value.Y = baubles[i].Y + Math.Max(0.5f, Math.Min(1.2f, (float)i * 0.02f));
                            if (value.Y > (float)__instance.map.DisplayHeight || value.X < 0f)
                            {
                                value.X = Game1.random.Next(0, __instance.map.DisplayWidth);
                                value.Y = -64f;
                            }

                            baubles[i] = value;
                        }
                    }

                    if (weatherDebris == null)
                    {
                        return;
                    }

                    foreach (WeatherDebris weatherDebri in weatherDebris)
                    {
                        weatherDebri.update();
                    }

                    Game1.updateDebrisWeatherForMovement(weatherDebris);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(UpdateWhenCurrentLocation_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        internal static void drawAboveAlwaysFrontLayer_Postfix(StardewValley.GameLocation __instance, SpriteBatch b)
        {
            try
            {
                if (__instance.Name == "poohnhi.WnS.CP_HiddenIslandCentral")
                {
                    if (baubles != null)
                    {
                        for (int i = 0; i < baubles.Count; i++)
                        {
                            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, baubles[i]), new Microsoft.Xna.Framework.Rectangle(346 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(i * 25)) % 600.0) / 150 * 5, 1971, 5, 5), Color.White, (float)i * (MathF.PI / 8f), Vector2.Zero, 4f, SpriteEffects.None, 1f);
                        }
                    }

                    if (weatherDebris == null || __instance.currentEvent != null)
                    {
                        return;
                    }

                    foreach (WeatherDebris weatherDebri in weatherDebris)
                    {
                        weatherDebri.draw(b);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(drawAboveAlwaysFrontLayer_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        internal static void cleanupBeforePlayerExit_Postfix(StardewValley.GameLocation __instance)
        {
            try
            {
                if (__instance.Name == "poohnhi.WnS.CP_HiddenIslandCentral")
                {
                    baubles?.Clear();
                    weatherDebris?.Clear();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(cleanupBeforePlayerExit_Postfix)}:\n{ex}", LogLevel.Error);
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
