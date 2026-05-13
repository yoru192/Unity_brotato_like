using UnityEngine;

namespace CodeBase.Common
{
    public static class PhysicsLayers
    {
        public static readonly int Hittable = LayerMask.NameToLayer("Hittable");
        public static readonly int Player = LayerMask.NameToLayer("Player");

        public static readonly int HittableMask = 1 << Hittable;
        public static readonly int PlayerMask = 1 << Player;
    }
}
