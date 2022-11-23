using System.Collections;
using UnityEngine;

public class Init : MonoBehaviour
{
    public CreatureGeneratorSettings CGSettings;
    public BipedSettings BipedSettings;
    public WorldGeneratorSettings WGSettings;

    // Start is called before the first frame update
    void Start()
    {
        var loader = GetComponent<Loader>();
        loader.Load(SetupLevel());
    }

    private IEnumerator SetupLevel()
    {
        GameObject terrain = WorldGenerator.Generate(WGSettings);
        GameObject c1 = CreatureGenerator.ParametricBiped(CGSettings, BipedSettings, null);
        yield return null;
    }
}
