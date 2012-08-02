using System;
/*
 * "The Unknown Game" by Matthew Sainsbury is licensed under a 
 * Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
 * Permissions beyond the scope of this license may be available at mailto:matthew@sainsbury.za.net.
 *
 * Game Sprites are property of Douglas Bentley and are not part of the Creative Commons Licence
 * Thanks for letting me use your sprites until I figure out what my game is going to be like!
 * Check out Doug's game at http://centureblog.blogspot.com/
 * 
 */
namespace Game
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
#endif
}

