using UnityEngine.Scripting;
using ISB.Runtime;
using ISB.Utilities;

[Preserve]
public class Game
{
    public static GameManager Manager = null;

    [Doc("Left turn.")]
    [Preserve]
    public void Rotate(NumberValue stationId)
    {
        if (!(Manager is null))
        {
            Manager.Rotate((int)stationId.ToNumber());
        }
    }

    [Doc("Fire.")]
    [Preserve]
    public void Fire(NumberValue stationId)
    {
        if (!(Manager is null))
        {
            Manager.Fire((int)stationId.ToNumber());
        }
    }
}
