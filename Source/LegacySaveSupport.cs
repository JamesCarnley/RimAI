using Verse;

namespace RimAI
{
    // Legacy support for older saves that expect OverviewComponent to be a GameComponent
    // This allows the game to load without crashing, even though this component no longer does anything.
    public class OverviewComponent : GameComponent
    {
        public OverviewComponent(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
        }
    }
}
