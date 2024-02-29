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

namespace The_Hidden_Island
{
    public partial class ModEntry
    {
        private List<Vector2> baubles;
        private List<WeatherDebris> weatherDebris;
        protected Color _ambientLightColor = Color.White;
        bool todayIsdebris = false;
        public static bool tmrIsrain { get; set; }
        public static bool tmrIsstorm { get; set; }

        public void cleanupBeforePlayerExit(GameLocation gameLocation)
        {
            try
            {
                //gameLocation.cleanupBeforePlayerExit();
                if (baubles != null)
                {
                    baubles.Clear();
                }
                if (weatherDebris != null)
                {
                    weatherDebris.Clear();
                }
            }
            catch
            {

            }
        }
        protected void _updateWoodsLighting(GameLocation gameLocation)
        {
            try
            {
                if (!Game1.isRaining)
                {
                    if (Game1.timeOfDay <= 1900)
                    {
                        gameLocation.ignoreOutdoorLighting.Value = true;
                        gameLocation.forceLoadPathLayerLights = false;
                    }
                    else
                    {
                        gameLocation.ignoreOutdoorLighting.Value = false;
                        gameLocation.forceLoadPathLayerLights = true;
                    }
                    _ambientLightColor = new Color(150, 120, 50);
                    if (Game1.currentLocation == gameLocation)
                    {
                        int fade_start_time = Utility.ConvertTimeToMinutes(Game1.getStartingToGetDarkTime());
                        int fade_end_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime());
                        int light_fade_start_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime());
                        int light_fade_end_time = Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime());
                        float num = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / 7000f * 10f;
                        float lerp = Utility.Clamp((num - (float)fade_start_time) / (float)(fade_end_time - fade_start_time), 0f, 1f);
                        float light_lerp = Utility.Clamp((num - (float)light_fade_start_time) / (float)(light_fade_end_time - light_fade_start_time), 0f, 1f);
                        Game1.ambientLight.R = (byte)Utility.Lerp((int)_ambientLightColor.R, (int)Game1.outdoorLight.R, lerp);
                        Game1.ambientLight.G = (byte)Utility.Lerp((int)_ambientLightColor.G, (int)Game1.outdoorLight.G, lerp);
                        Game1.ambientLight.B = (byte)Utility.Lerp((int)_ambientLightColor.B, (int)Game1.outdoorLight.B, lerp);
                        Game1.ambientLight.A = (byte)Utility.Lerp((int)_ambientLightColor.A, (int)Game1.outdoorLight.A, lerp);
                        Color light_color = Color.Black;
                        light_color.A = (byte)Utility.Lerp(255f, 0f, light_lerp);
                        foreach (LightSource light in Game1.currentLightSources)
                        {
                            if (light.lightContext.Value == LightSource.LightContext.MapLight)
                            {
                                if (light.lightTexture != Game1.indoorWindowLight && Game1.timeOfDay < 2000)
                                    light.color.Value = new Color(0, 0, 0, 0);
                                else if (light.lightTexture != Game1.indoorWindowLight && Game1.timeOfDay >= 2000)
                                    light.color.Value = Color.Black;
                                else
                                    light.color.Value = light_color;
                            }
                        }
                    }
                }
                else
                {
                    Color light_color = Color.Black;
                    foreach (LightSource light in Game1.currentLightSources)
                    {
                        if (light.lightContext.Value == LightSource.LightContext.MapLight)
                        {
                            if (light.lightTexture != Game1.indoorWindowLight && Game1.timeOfDay < 2000)
                                light.color.Value = new Color(0, 0, 0, 0);
                            else if (light.lightTexture != Game1.indoorWindowLight && Game1.timeOfDay >= 2000)
                                light.color.Value = Color.Black;
                            else light.color.Value = new Color(0, 0, 0, 0);
                        }
                    }
                }
            }
            catch
            {

            }
        }
        protected void initCentral(GameLocation gameLocation)
        {
            resetLocalState(gameLocation);
        }
        protected void drawCentral()
        {
            try
            {
                UpdateWhenCurrentLocation(Game1.currentLocation, Game1.currentGameTime);
                drawAboveAlwaysFrontLayer(Game1.currentLocation);
            }
            catch
            {

            }
        }
        protected void resetLocalState(GameLocation gameLocation)
        {
            try
            {
                Map map = gameLocation.map;
                _updateWoodsLighting(gameLocation);
                Random random = Utility.CreateDaySaveRandom();
                int num = 25 + random.Next(0, 75);
                if (!Game1.isRaining)
                {

                    baubles = new List<Vector2>();
                    for (int i = 0; i < num; i++)
                    {
                        baubles.Add(new Vector2(Game1.random.Next(0, map.DisplayWidth), Game1.random.Next(0, map.DisplayHeight)));
                    }
                    if (!Game1.currentSeason.Equals("winter"))
                    {
                        weatherDebris = new List<WeatherDebris>();
                        int spacing = 192;
                        int leafType = 1;
                        if (Game1.currentSeason.Equals("spring"))
                        {
                            leafType = 0;
                        }
                        if (Game1.currentSeason.Equals("fall"))
                        {
                            leafType = 2;
                        }
                        for (int j = 0; j < num; j++)
                        {
                            weatherDebris.Add(new WeatherDebris(new Vector2(j * spacing % Game1.graphics.GraphicsDevice.Viewport.Width + Game1.random.Next(spacing), j * spacing / Game1.graphics.GraphicsDevice.Viewport.Width * spacing % Game1.graphics.GraphicsDevice.Viewport.Height + Game1.random.Next(spacing)), leafType, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
                        }
                    }
                }
            }
            catch
            {

            }
        }
        public void UpdateWhenCurrentLocation(GameLocation gameLocation, GameTime time)
        {
            try
            {
                Map map = gameLocation.map;
                gameLocation.UpdateWhenCurrentLocation(time);
                if (baubles != null)
                {
                    for (int i = 0; i < baubles.Count; i++)
                    {
                        Vector2 value = default(Vector2);
                        value.X = baubles[i].X - Math.Max(0.4f, Math.Min(1f, (float)i * 0.01f)) - (float)((double)((float)i * 0.01f) * Math.Sin(Math.PI * 2.0 * (double)time.TotalGameTime.Milliseconds / 8000.0));
                        value.Y = baubles[i].Y + Math.Max(0.5f, Math.Min(1.2f, (float)i * 0.02f));
                        if (value.Y > (float)map.DisplayHeight || value.X < 0f)
                        {
                            value.X = Game1.random.Next(0, map.DisplayWidth);
                            value.Y = -64f;
                        }

                        baubles[i] = value;
                    }
                }
            }
            
            catch
            {

            }
        }

        public void drawAboveAlwaysFrontLayer(GameLocation gameLocation)
        {
            try
            {
                Color overlayColor = Color.White;
                SpriteBatch b = Game1.spriteBatch;
                gameLocation.drawAboveAlwaysFrontLayer(b);
                if (baubles != null)
                {
                    for (int i = 0; i < baubles.Count; i++)
                    {
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, baubles[i]), new Microsoft.Xna.Framework.Rectangle(346 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(i * 25)) % 600.0) / 150 * 5, 1971, 5, 5), overlayColor, (float)i * ((float)Math.PI / 8f), Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    }
                }
            }
            catch
            {

            }
        }
    }
}