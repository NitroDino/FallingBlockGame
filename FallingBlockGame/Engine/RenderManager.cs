﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace engine
{
    public class RenderManager
    {
        private Graphics graphics;
        public Graphics Graphics { get { return graphics; } }

        public RenderManager(Graphics graphics)
        {
            this.graphics = graphics;
        }

        public void Draw(List<IDrawable> drawables) 
        {
            foreach (IDrawable drawable in drawables)
            {
                drawable.Draw(graphics);
            }
        }

        public void Draw(List<GameObject> gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                IDrawable drawable = gameObject.DrawableComponent.CreateDrawable();
                drawable.Draw(graphics);
            }
        }

        public void ClearScreen(Color color)
        {
            graphics.GraphicsDevice.Clear(color);
        }

    }
}
