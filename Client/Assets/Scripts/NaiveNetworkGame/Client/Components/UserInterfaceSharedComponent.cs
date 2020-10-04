using System;
using Scenes;
using Unity.Entities;

namespace NaiveNetworkGame.Client.Components
{
    public struct UserInterfaceSharedComponent : ISharedComponentData, IEquatable<UserInterfaceSharedComponent>
    {
        public UserInterface userInterface;

        public bool Equals(UserInterfaceSharedComponent other)
        {
            return Equals(userInterface, other.userInterface);
        }

        public override bool Equals(object obj)
        {
            return obj is UserInterfaceSharedComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (userInterface != null ? userInterface.GetHashCode() : 0);
        }
    }
}