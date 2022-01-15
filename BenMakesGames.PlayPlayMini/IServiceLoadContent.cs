using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BenMakesGames.PlayPlayMini
{
    public interface IServiceLoadContent
    {
        bool FullyLoaded { get; }
        void LoadContent(GameStateManager gsm);
    }
}
