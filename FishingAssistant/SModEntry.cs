﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace FishingAssistant
{
    partial class ModEntry : Mod
    {
        private bool modEnable;

        private int playerStandingX;
        private int playerStandingY;
        private int playerFacingDirection;

        private bool maxCastPower;
        private bool autoHook;
        private bool autoCatchTreasure;

        private bool inFishingMiniGame;

        private ClickableTextureComponent modEnableIcon;
        private ClickableTextureComponent maxCastIcon;
        private ClickableTextureComponent catchTreasureIcon;

        private int boxSize = 96;
        private int iconSize = 40;
        private int screenMargin = 8;
        private int spacing = 2;
        private int toolBarWidth = 0;

        private void Initialize(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Display.RenderedActiveMenu += OnRenderMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.GameLoop.TimeChanged += OnTimeChange;

            Monitor.Log("Initialized (press F5 to reload config)", LogLevel.Info);
        }

        private void ToggleMod(ButtonPressedEventArgs e)
        {
            if (e.Button == Config.EnableModButton)
            {
                modEnable = !modEnable;

                if (modEnable)
                {
                    GetPlayerData();
                    CheckCurrentTime();
                }

                Monitor.Log(string.Format("Mod enable {0}", modEnable), LogLevel.Info);
            }
        }

        private void ToggleMaxCastPower(ButtonPressedEventArgs e)
        {
            if (e.Button == Config.CastPowerButton)
            {
                maxCastPower = !maxCastPower;
                Monitor.Log(string.Format("Max cast power {0}", maxCastPower), LogLevel.Info);
            }
        }

        private void ToggleCatchTreasure(ButtonPressedEventArgs e)
        {
            if (e.Button == Config.CatchTreasureButton)
            {
                autoCatchTreasure = !autoCatchTreasure;
                Monitor.Log(string.Format("Catch treasure {0}", autoCatchTreasure), LogLevel.Info);
            }
        }

        private void ReloadConfig()
        {
            Config = Helper.ReadConfig<ModConfig>();
            Monitor.Log("Config reloaded", LogLevel.Info);

            switch (Config.FishDisplayPosition)
            {
                case "Top":
                    displayOrder = new List<string>() { "Top", "UpperRight", "UpperLeft", "LowerRight" };
                    break;

                case "UpperRight":
                    displayOrder = new List<string>() { "UpperRight", "UpperLeft", "LowerRight" };
                    break;

                case "UpperLeft":
                    displayOrder = new List<string>() { "UpperLeft", "UpperRight", "LowerLeft" };
                    break;

                case "Bottom":
                    displayOrder = new List<string>() { "Bottom", "LowerRight", "LowerLeft", "UpperRight" };
                    break;

                case "LowerRight":
                    displayOrder = new List<string>() { "LowerRight", "LowerLeft", "UpperRight" };
                    break;

                case "LowerLeft":
                    displayOrder = new List<string>() { "LowerLeft", "LowerRight", "UpperLeft" };
                    break;

                default:
                    displayOrder = new List<string>() { "UpperRight", "UpperLeft", "LowerLeft" };
                    this.Monitor.Log($"Invalid config value {Config.FishDisplayPosition} for FishDisplayPosition. Valid entries include Top, Bottom, UpperRight, UpperLeft, LowerRight and LowerLeft.", LogLevel.Warn);
                    break;
            }
        }

        private void GetPlayerData()
        {
            playerStandingX = Game1.player.getStandingX();
            playerStandingY = Game1.player.getStandingY();
            playerFacingDirection = Game1.player.getDirection() != -1 ? Game1.player.getDirection() : Game1.player.FacingDirection;
        }

        private bool IsFishingMiniGameReady()
        {
            return inFishingMiniGame && bobberBar != null;
        }

        private void CheckCurrentTime()
        {
            if (Game1.timeOfDay >= Config.PauseFishingTime && modEnable)
            {
                modEnable = false;
                maxCastPower = false;
                autoCatchTreasure = false;
                Game1.addHUDMessage(new HUDMessage("Current time is " + ConvertTime(Game1.timeOfDay / 100), 3));
            }
        }

        private string ConvertTime(int currentTime)
        {
            string sTime = currentTime.ToString();
            if (sTime.Length == 3)
                sTime = "0" + sTime;

            string hour = sTime.Substring(0, 1) + sTime.Substring(1, 1);
            string minute = sTime.Substring(2, 1) + sTime.Substring(3, 1);
            hour = hour == "24" ? "00" : hour == "25" ? "01" : hour;

            string formatedTime = string.Format("{0}:{1}", hour, minute);

            DateTime parseTime = DateTime.Parse(formatedTime);
            return parseTime.ToString("h:mm tt");
        }

        private void DrawIcon()
        {
            if (Game1.eventUp)
                return;

            bool alignTop = false;
            Point playerGlobalPos = Game1.player.GetBoundingBox().Center;
            Vector2 playerLocalVec = Game1.GlobalToLocal(globalPosition: new Vector2(playerGlobalPos.X, playerGlobalPos.Y), viewport: Game1.viewport);

            float toolBarTransparency = 1;
            for (int i = 0; i < Game1.onScreenMenus.Count; i++)
            {
                if (Game1.onScreenMenus[i] is Toolbar toolBar)
                {
                    toolBarTransparency = Helper.Reflection.GetField<float>(toolBar, "transparency", true).GetValue();
                    toolBarWidth = toolBar.width / 2;
                }
            }

            if (!Game1.options.pinToolbarToggle)
                alignTop = (playerLocalVec.Y > (float)(Game1.viewport.Height / 2 + 64));

            var viewport = Game1.graphics.GraphicsDevice.Viewport;
            int screenXPos = (int)viewport.Width / 2;
            int screenYPos;

            if (alignTop)
                screenYPos = screenMargin;
            else
                screenYPos = (int)viewport.Height - screenMargin - boxSize;

            int boxCenterY = screenYPos + boxSize / 2;
            int boxCenterX = 0;
            bool drawBox = false;

            if (modEnable || maxCastPower || autoCatchTreasure)
            {
                if (Game1.activeClickableMenu != null)
                {
                    IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), screenXPos - boxSize / 2, screenYPos, boxSize, boxSize, Color.White * toolBarTransparency, 1, drawShadow: false);
                    boxCenterX = screenXPos;
                }
                else
                {
                    if (Config.ModStatusDisplayPosition == "Left")
                    {
                        IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), screenXPos - toolBarWidth - boxSize, screenYPos, boxSize, boxSize, Color.White * toolBarTransparency, drawShadow: false);
                        boxCenterX = screenXPos - toolBarWidth - boxSize / 2;
                    }
                    else if (Config.ModStatusDisplayPosition == "Right")
                    {
                        IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), screenXPos + toolBarWidth, screenYPos, boxSize, boxSize, Color.White * toolBarTransparency, 1, drawShadow: false);
                        boxCenterX = screenXPos + toolBarWidth + boxSize / 2;
                    }
                }
                drawBox = true;
            }

            Rectangle Icon;
            float iconTransparency = 1;
            int x; int y;

            iconTransparency = modEnable ? 1 : drawBox ? 0.2f : 0;
            Icon = new Rectangle(20, 428, 10, 10);
            x = boxCenterX - (boxSize / 4) - spacing;
            y = boxCenterY - (boxSize / 4) - spacing;
            modEnableIcon = new ClickableTextureComponent(new Rectangle(x, y, iconSize, iconSize), Game1.mouseCursors, Icon, 2f);
            modEnableIcon.draw(Game1.spriteBatch, Color.White * toolBarTransparency * iconTransparency, 0);

            iconTransparency = maxCastPower ? 1 : drawBox ? 0.2f : 0;
            Icon = new Rectangle(545, 1921, 53, 19);
            x = boxCenterX - (boxSize / 4);
            y = boxCenterY + (boxSize / 4) - (iconSize / 2) + spacing;
            maxCastIcon = new ClickableTextureComponent(new Rectangle(x, y, iconSize, iconSize), Game1.mouseCursors, Icon, 1f);
            maxCastIcon.draw(Game1.spriteBatch, Color.White * toolBarTransparency * iconTransparency, 0);

            iconTransparency = autoCatchTreasure ? 1 : drawBox ? 0.2f : 0;
            Icon = new Rectangle(137, 412, 10, 11);
            x = boxCenterX + (boxSize / 4) - (iconSize / 2) + spacing;
            y = boxCenterY - (boxSize / 4) - spacing;
            catchTreasureIcon = new ClickableTextureComponent(new Rectangle(x, y, iconSize, iconSize), Game1.mouseCursors, Icon, 2f);
            catchTreasureIcon.draw(Game1.spriteBatch, Color.White * toolBarTransparency * iconTransparency, 0);
        }
    }
}