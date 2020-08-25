using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class ConfigureServerSceneController : MonoBehaviour
    {
        [Serializable]
        public struct ServerConfig
        {
            public string ip;
        }

        [Serializable]
        public class ServerList
        {
            public List<ServerConfig> severs = new List<ServerConfig>();
        }
        
        public Dropdown serverConnectDropdown;

        public Button serverConnectButton;
        
        public InputField newServerInput;

        public Button newServerButton;

        public Button deleteServersButton;
        
        private ServerList serverList = new ServerList();
        
        // Start is called before the first frame update
        private void Start()
        {
            // do stuff....

            var savedServersJson = PlayerPrefs.GetString("SavedServers", null);

            try
            {
                if (savedServersJson != null)
                {
                    serverList = JsonUtility.FromJson<ServerList>(savedServersJson);
                }
            }
            catch
            {
                
            }
            
            if (savedServersJson == null || serverList.severs.Count == 0)
            {
                serverList = new ServerList();
                serverList.severs.Add(new ServerConfig
                {
                    ip = "127.0.0.1"
                });
            }

            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));

            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.ip
            }).ToList();

            newServerButton.onClick.AddListener(OnNewServerAdded);
            serverConnectButton.onClick.AddListener(OnServerConnect);
            deleteServersButton.onClick.AddListener(OnDeleteServers);
        }

        private void OnDeleteServers()
        {
            serverList.severs.Clear();
            serverList.severs.Add(new ServerConfig
            {
                ip = "127.0.0.1"
            });
            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));
            
            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.ip
            }).ToList();
        }

        private void OnServerConnect()
        {
            // load the other scene with parameters...
        }

        private void OnNewServerAdded()
        {
            var newServerIp = newServerInput.text;
            serverList.severs.Add(new ServerConfig
            {
                ip = newServerIp
            });
            
            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));

            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.ip
            }).ToList();
            
            newServerInput.text = string.Empty;
        }
    }
}