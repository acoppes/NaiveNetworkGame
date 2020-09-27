using System.IO;
using System.IO.Compression;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Scenes.Tests
{
    public class TestCompression : MonoBehaviour
    {
        private NativeList<NetworkConnection> m_Connections;
        public NetworkDriver m_Driver;

        private NetworkPipeline pipeline;

        public bool useGzipCompression = false;
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

            byte[] resultBytes;
            
            using (var resultStream = new MemoryStream())
            {
                if (useGzipCompression)
                {
                    using (var compressionStream = new GZipStream(resultStream, compressionLevel))
                    {
                        compressionStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    using (var compressionStream = new DeflateStream(resultStream, compressionLevel))
                    {
                        compressionStream.Write(bytes, 0, bytes.Length);
                    }
                }

                resultBytes = resultStream.ToArray();
            }
            
            // var compressedStream2 = new MemoryStream();
            compressedText = Encoding.UTF8.GetString(resultBytes, 0, resultBytes.Length);
            compressedSize = resultBytes.Length;
            
            // decompress...

            using (var readStream = new MemoryStream(resultBytes))
            {
                if (useGzipCompression)
                {
                    using (var uncompressionStream = new GZipStream(readStream, CompressionMode.Decompress))
                    {
                        var readBytes = new byte[bytes.Length];
                        var length = uncompressionStream.Read(readBytes, 0, bytes.Length);
                        uncompressedText = Encoding.UTF8.GetString(readBytes, 0, length);
                        targetSize = length;
                    }
                }
                else
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
}