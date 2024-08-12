using System;

namespace Project.Managers.Levels.Interfaces
{
    public interface ILevelLoader<LevelAssetType, LevelEnumType>
        where LevelEnumType : Enum
    {
        public LevelAssetType GetLevel(int number, LevelEnumType type);
    }

    public interface ILevelAssetManager<TLevelAsset, TLevelEnum>
        where TLevelAsset : class
        where TLevelEnum : Enum
    {
        public TLevelAsset GetLevelForType(int level, TLevelEnum type);
        public TLevelAsset GetCurrentLevelForType(TLevelEnum type);
    }

    public interface ILevelTypeManager<TLevelEnum>
        where TLevelEnum : Enum
    {
        public int GetLevelNumberForType(TLevelEnum type);
        public bool HasLevel(TLevelEnum type);
        public void SetLevelNumberForType(TLevelEnum type, int number);
    }

    public interface ILevelProgressManager
    {
        public void CompleteLevel(Enum type, int number, float time, string rewardId);
    }

    public interface ILevelProgressManager<TLevelEnum>
        where TLevelEnum : Enum
    {
        public void CompleteLevel(TLevelEnum type, int number, float time, string rewardId);
    }
}
