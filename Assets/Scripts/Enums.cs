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
    Game,
    Pause,
    Options,
    Victory, 
    Instructions, 
    Credits,
    Library,
    DiaryEntry
}

public enum PlayerCharacter
{
    DrHives,
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
    DashCooldownUpdate, //data = (int)teamIndex, (int)remainingCooldown
}

public enum PlayerCombatEvent
{
    ReceivedDamage, // data = {(int)teamIndex, (int)damage}
    ReceivedHeal, // data = {(int)teamIndex, (int)healAmount}
    Death, // data  = (int)teamIndex
    BackToLife, // data = (int)teamIndex
    SuccessfulParry,
    StartAbilityCooldown, // data = (int)teamIndex
    AbilityEnabled, // data = (int)teamIndex
    AbilityDisabled, // data = (int)teamIndex
    AbilityCooldownUpdate, //data = (int)teamIndex, (int)remainingCooldown
    DeathCooldownUpdate, //data = (int)teamIndex, (int)remainingCooldown
}

public enum InputManagerEvent
{
    DeviceChangeP1, // data = (int) controlScheme 0 = keyboard, 1 = gamepad
    DeviceChangeP2, // data = (int) controlScheme 0 = keyboard, 1 = gamepad
}

public enum GameUIAnimEvents
{
    LightningOnStart,
    PlayersLightOn,
    GameStart,
}