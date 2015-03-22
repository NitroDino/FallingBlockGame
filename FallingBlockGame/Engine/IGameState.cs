﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace engine
{
    public interface IGameState
    {
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
