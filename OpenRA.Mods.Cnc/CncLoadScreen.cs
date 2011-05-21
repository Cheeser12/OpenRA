#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Support;
using OpenRA.Widgets;

namespace OpenRA.Mods.Cnc
{
	public class CncLoadScreen : ILoadScreen
	{
		Dictionary<string,string> Info;
		Stopwatch loadTimer = new Stopwatch();
		Sprite[] ss;
		string text;
		int loadTick;
		float2 nodPos, gdiPos, evaPos, textPos;
		Sprite nodLogo, gdiLogo, evaLogo, brightBlock, dimBlock;
		Rectangle Bounds;
		Renderer r;
		NullInputHandler nih = new NullInputHandler();
		
		public void Init(Dictionary<string, string> info)
		{
			Info = info;
			// Avoid standard loading mechanisms so we
			// can display loadscreen as early as possible
			r = Game.Renderer;
			if (r == null) return;
			
			var s = new Sheet("mods/cnc/uibits/chrome.png");
			Bounds = new Rectangle(0,0,Renderer.Resolution.Width, Renderer.Resolution.Height);
			ss = new Sprite[]
			{
				new Sprite(s, new Rectangle(161,128,62,33), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(161,223,62,33), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(128,161,33,62), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(223,161,33,62), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(128,128,33,33), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(223,128,33,33), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(128,223,33,33), TextureChannel.Alpha),
				new Sprite(s, new Rectangle(223,223,33,33), TextureChannel.Alpha)
			};
			nodLogo = new Sprite(s, new Rectangle(0,256,256,256), TextureChannel.Alpha);
			gdiLogo = new Sprite(s, new Rectangle(256,256,256,256), TextureChannel.Alpha);
			evaLogo = new Sprite(s, new Rectangle(256,64,128,64), TextureChannel.Alpha);
			nodPos = new float2(Renderer.Resolution.Width/2 - 384, Renderer.Resolution.Height/2 - 128);
			gdiPos = new float2(Renderer.Resolution.Width/2 + 128, Renderer.Resolution.Height/2 - 128);
			evaPos = new float2(Renderer.Resolution.Width-43-128, 43);
			
			brightBlock = new Sprite(s, new Rectangle(320,0,16,35), TextureChannel.Alpha);
			dimBlock = new Sprite(s, new Rectangle(336,0,16,35), TextureChannel.Alpha);
		}
		
		public void Display()
		{
			if (r == null || loadTimer.ElapsedTime() < 0.25)
				return;
			loadTimer.Reset();
			
			loadTick = ++loadTick % 8;
			r.BeginFrame(float2.Zero);
			r.RgbaSpriteRenderer.DrawSprite(gdiLogo, gdiPos);
			r.RgbaSpriteRenderer.DrawSprite(nodLogo, nodPos);
			r.RgbaSpriteRenderer.DrawSprite(evaLogo, evaPos);
			DrawBorder();
			
			var barY = Renderer.Resolution.Height-78;
			text = "Loading";
			var textSize = r.Fonts["BigBold"].Measure(text);
			textPos = new float2((Renderer.Resolution.Width - textSize.X) / 2, barY);
			r.Fonts["BigBold"].DrawText(text, textPos, Color.Gray);

			for (var i = 0; i <= 8; i++)
			{
				var block = loadTick == i ? brightBlock : dimBlock;
				r.RgbaSpriteRenderer.DrawSprite(block,
					new float2(Renderer.Resolution.Width/2 - 114 - i*32, barY));
				r.RgbaSpriteRenderer.DrawSprite(block,
					new float2(Renderer.Resolution.Width/2 + 114 + i*32-16, barY));
			}
			
			r.EndFrame( nih );
		}
		
		
		void DrawBorder()
		{
			// Left border
			WidgetUtils.FillRectWithSprite(new Rectangle(Bounds.Left,
								 Bounds.Top + (int)ss[0].size.Y,
								 (int)ss[2].size.X,
								 Bounds.Bottom - (int)ss[1].size.Y - Bounds.Top - (int)ss[0].size.Y),
				   ss[2]);

			// Right border
			WidgetUtils.FillRectWithSprite(new Rectangle(Bounds.Right - (int)ss[3].size.X,
								 Bounds.Top + (int)ss[0].size.Y,
								 (int)ss[2].size.X,
								 Bounds.Bottom - (int)ss[1].size.Y - Bounds.Top - (int)ss[0].size.Y),
				   ss[3]);

			// Top border
			WidgetUtils.FillRectWithSprite(new Rectangle(Bounds.Left + (int)ss[2].size.X,
								 Bounds.Top,
								 Bounds.Right - (int)ss[3].size.X - Bounds.Left - (int)ss[2].size.X,
								 (int)ss[0].size.Y),
				   ss[0]);

			// Bottom border
			WidgetUtils.FillRectWithSprite(new Rectangle(Bounds.Left + (int)ss[2].size.X,
								Bounds.Bottom - (int)ss[1].size.Y,
								 Bounds.Right - (int)ss[3].size.X - Bounds.Left - (int)ss[2].size.X,
								 (int)ss[0].size.Y),
				   ss[1]);


			WidgetUtils.DrawRGBA(ss[4], new float2(Bounds.Left, Bounds.Top));
			WidgetUtils.DrawRGBA(ss[5], new float2(Bounds.Right - ss[5].size.X, Bounds.Top));
			WidgetUtils.DrawRGBA(ss[6], new float2(Bounds.Left, Bounds.Bottom - ss[6].size.Y));
			WidgetUtils.DrawRGBA(ss[7], new float2(Bounds.Right - ss[7].size.X, Bounds.Bottom - ss[7].size.Y));
		}
		
		public void StartGame()
		{
			TestAndContinue();
		}

		void TestAndContinue()
		{
			Widget.RootWidget.RemoveChildren();
			if (!FileSystem.Exists(Info["TestFile"]))
			{
				var args = new WidgetArgs()
				{
					{ "continueLoading", () => TestAndContinue() },
					{ "installData", Info }
				};
				Widget.LoadWidget(Info["InstallerBackgroundWidget"], Widget.RootWidget, args);
				Widget.OpenWindow(Info["InstallerMenuWidget"], args);
			}
			else
				Game.LoadShellMap();
		}
	}
}

