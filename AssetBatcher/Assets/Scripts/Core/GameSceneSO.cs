using UnityEngine.AddressableAssets;

public class GameSceneSO : DescriptionBaseSO
{
    public GameSceneType sceneType;
    public AssetReference sceneReference;

    public enum GameSceneType
    {
        // Special scenes
        Initialization,
        PersistantMangers,

        // Playable Scenes
        Menu,
        Location,
        
        // Game 전반적인 관리하는 씬
        Gameplay,

        // Work in progress scenes that don't need to be played Art
        Art,
    }
}
