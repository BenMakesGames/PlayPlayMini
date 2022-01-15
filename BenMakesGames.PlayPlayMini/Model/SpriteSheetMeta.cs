using System;
using System.Collections.Generic;
using System.Text;

namespace BenMakesGames.PlayPlayMini.Model
{
    public struct SpriteSheetMeta
    {
        public string Key { get; set; }
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool PreLoaded { get; set; }

        /// <param name="key"></param>
        /// <param name="path">Relative path to image, excluding file extension (ex: "Characters/Nina")</param>
        /// <param name="width">Width of an individual sprite</param>
        /// <param name="height">Height of an individual sprite</param>
        /// <param name="preLoaded">Whether or not to load this resource BEFORE entering the first IGameState</param>
        public SpriteSheetMeta(string key, string path, int width, int height, bool preLoaded = false)
        {
            Key = key;
            Path = path;
            Width = width;
            Height = height;
            PreLoaded = preLoaded;
        }
    }
}
