public enum SceneName
{
    StartScene,
    MainMenuScene,
    GameScene,
    GameSandboxScene,
}

public enum ScreenName 
{ 
    MainMenu,
    PlayerSelection,
    PlayerSelectionSandbox,
    Game,
    GameSandbox,
    Pause,
    PauseSandbox, //TODO: review, this is probably going to end up being the same as 1v1
    Options,
    OptionsSandbox, //TODO: review, this is probably going to end up being the same as 1v1
    Victory, 
    Instructions, 
    Credits,
}

public enum PlayerCharacter
{
    Brawlight,
    Peggy
}

// GAME EVENTS
public enum GameEvent
{
    GameEnd, // data = (int)team1Score, (int)team2Score
    ScoreUpdate, // data = (int)team1Score, (int)team2Score 
    SuddenDeath, // no data
}

public enum PlayerMovementEvent
{
    DashConsumed, // data = (int)teamIndex
    DashEnabled, // data = (int)teamIndex
}

public enum PlayerCombatEvent
{
    ReceivedDamage, // data = {(int)teamIndex, (int)damage}
    ReceivedHeal, // data = {(int)teamIndex, (int)healAmount}
    Death, // data  = (int)teamIndex
    BackToLife, // data = (int)teamIndex
    SuccessfulParry,
}