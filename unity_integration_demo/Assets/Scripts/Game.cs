using ISB.Runtime;
using ISB.Utilities;
using UnityEngine;

// Example BASIC code that invokes Game.AddBall.
//
// For x = -3 To 3
//   For z = -3 To 3
//      Game.AddBall(x, 5, z)
//   EndFor
// EndFor

public class Game
{
    [Doc("Example lib function to access Unity objects.")]
    public void AddBall(NumberValue x, NumberValue y, NumberValue z)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Sphere");
        if (prefab != null)
        {
            Object.Instantiate(prefab,
                new Vector3((float)x.ToNumber(), (float)y.ToNumber(), (float)z.ToNumber()),
                Quaternion.identity);
        }
        else
        {
            Debug.Log("Failed to load prefab.");
        }
    }
}
