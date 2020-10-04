using System;
using NaiveNetworkGame.Common;
using Scenes;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Components
{
    public struct UserInterfaceSharedComponent : ISharedComponentData, IEquatable<UserInterfaceSharedComponent>
    {
        public PlayerButton spawnUnitButton;
        public FixedNumbersLabel goldLabel;
        public PlayerStatsUI playerStats;

        public bool Equals(UserInterfaceSharedComponent other)
        {
            return Equals(spawnUnitButton, other.spawnUnitButton) && Equals(goldLabel, other.goldLabel) && Equals(playerStats, other.playerStats);
        }

        public override bool Equals(object obj)
        {
            return obj is UserInterfaceSharedComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (spawnUnitButton != null ? spawnUnitButton.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (goldLabel != null ? goldLabel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (playerStats != null ? playerStats.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}