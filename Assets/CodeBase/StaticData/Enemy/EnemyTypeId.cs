namespace CodeBase.StaticData.Enemy
{
    public enum EnemyTypeId
    {
        Base = 0,
        Tank = 1,
        Kamikadze = 2,
        Ranger = 3,

        // Серіалізується як число (ассети, хвилі) — нові значення лише в кінець.
        BroodQueen = 4,
        Larva = 5,
    }
}