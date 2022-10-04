using UnityEngine.AddressableAssets;

public class GameSceneSO : DescriptionBaseSO
{
    public GameSceneType sceneType;
    public AssetReference sceneReference;

    public enum GameSceneType
    {
        // 특수 역할을 하는 Scene
        Initialization,
        PersistantMangers,

        // 플레이 가능한 Scene
        Menu,
        Location,
        
        // Game 전반적인 관리하는 매니저 Scene
        Gameplay,

        // Level을 구성하는 Prop들을 모듈화 해놓은 Scene
        Art,
    }
}
