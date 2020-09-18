using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace NaiveNetworkGame.Common
{
    public struct NetworkTranslationSync : IComponentData
    {
        public ushort unitId;
        public float delta;
        public float2 translation;

        public static int GetSize()
        {
            unsafe
            {
                return sizeof(ushort) + sizeof(short) * 2 + sizeof(float);
            }
        }

        public NetworkTranslationSync Write(ref DataStreamWriter writer)
        {
            // writer.WriteByte(PacketType.ServerTranslationSync);
            writer.WriteUShort(unitId);
            writer.WriteFloat(delta);
            writer.WriteShort((short) Mathf.RoundToInt(translation.x * 100.0f));
            writer.WriteShort((short) Mathf.RoundToInt(translation.y * 100.0f));
            // writer.WriteFloat(translation.x);
            // writer.WriteFloat(translation.y);
            return this;
        }

        public NetworkTranslationSync Read(ref DataStreamReader stream)
        {;
            unitId = stream.ReadUShort();
            delta = stream.ReadFloat();
            // translation.x = stream.ReadFloat();
            // translation.y = stream.ReadFloat();
            translation.x = stream.ReadShort() / 100.0f;
            translation.y = stream.ReadShort() / 100.0f;
            return this;
        }
    }
}