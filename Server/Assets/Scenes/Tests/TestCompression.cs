using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.UI;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Scenes.Tests
{
    public class TestCompression : MonoBehaviour
    {
        private NativeList<NetworkConnection> m_Connections;
        public NetworkDriver m_Driver;

        private NetworkPipeline pipeline;

        public CompressionLevel compressionLevel;

        public int sourceSize;
        public int compressedSize;
        public int targetSize;

        [TextArea(5, 15)]
        public string textToSend;

        [TextArea(5, 15)]
        public string compressedText;
        
        [TextArea(5, 15)]
        public string uncompressedText;
        
        
        void Start()
        {
            var bytes = Encoding.UTF8.GetBytes(textToSend.ToCharArray());
            sourceSize = bytes.Length;

            // var memoryStream = new MemoryStream(bytes);
            byte[] resultBytes;
            
            using (var resultStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(resultStream, compressionLevel))
                {
                    compressionStream.Write(bytes, 0, bytes.Length);
                }
                resultBytes = resultStream.ToArray();
            }
            
            // var compressedStream2 = new MemoryStream();
            compressedText = Encoding.UTF8.GetString(resultBytes, 0, resultBytes.Length);
            compressedSize = resultBytes.Length;
            
            // decompress...

            using (var readStream = new MemoryStream(resultBytes))
            {
                using (var uncompressionStream = new DeflateStream(readStream, CompressionMode.Decompress))
                {
                    var readBytes = new byte[bytes.Length];
                    var length = uncompressionStream.Read(readBytes, 0, bytes.Length);
                    uncompressedText = Encoding.UTF8.GetString(readBytes, 0, length);
                    targetSize = length;
                }    
            }

            

        }

    }
}