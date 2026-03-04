using System.Collections;
using UnityEngine;

/// <summary>
/// To be implemented by any movement type that the player uses.
/// <para>NOTE: Subject to change, i just made it an interface to get a global image to start</para>
/// </summary>
interface IMovement
{
    /// <summary>
    /// Initialize movement parameters. This should be called when the movement is first assigned to the player.
    /// </summary>
    /// <param name="speed"></param>
    void Init(float speed, int teamIndex);

    /// <summary>
    /// Primary movement method. Should be called every frame in the player class, and it should move the player according to the given horizontal and vertical input.
    /// </summary>
    /// <param name="horizontal"></param>
    /// <param name="vertical"></param>
    void Move(float horizontal, float vertical);

    /// <summary>
    /// Start coroutine to stop player movement for a given amount of time.
    /// </summary>

    IEnumerator DisableMovement(float duration);

    /// <summary>
    /// Enable or disable movement on command
    /// </summary>
    /// <param name="movementDisabled"></param>
    void DisableMovement(bool movementDisabled);

    /// <summary>
    /// </summary>
    /// <param name="ignoreCooldown"></param>
    /// <returns></returns>
    IEnumerator Dash(bool ignoreCooldown);
}