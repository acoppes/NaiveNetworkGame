using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        
        private ServerList serverList;
        
        // Start is called before the first frame update
        private void Start()
        {
            // do stuff....
            
            serverList = new ServerList();

            var savedServersJson = PlayerPrefs.GetString("SavedServers", null);

            try
            {
                if (!string.IsNullOrEmpty(savedServersJson))
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
                serverList.severs.Add(new ServerConfig
                {
                    ip = "209.151.153.172"
                });
            }

            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));

            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.ip
            }).ToList();

            var lastSelectedServer = PlayerPrefs.GetString("LastSelectedServer", null);

            var index = serverConnectDropdown.options.FindIndex(o => o.text.Equals(lastSelectedServer));
            if (index >= 0)
                serverConnectDropdown.value = index;

            newServerButton.onClick.AddListener(OnNewServerAdded);
            serverConnectButton.onClick.AddListener(OnServerConnect);
            deleteServersButton.onClick.AddListener(OnDeleteServers);

            newServerInput.contentType = InputField.ContentType.Custom;
            newServerInput.onValidateInput += OnValidateValidIpAddress;
        }

        private char OnValidateValidIpAddress(string text, int charindex, char addedchar)
        {
            if (!Regex.IsMatch(addedchar.ToString(), "[0-9|\\.]+"))
                return '\0';

            return addedchar;
        }

        private void OnDeleteServers()
        {
            serverList.severs.Clear();
            
            serverList.severs.Add(new ServerConfig
            {
                ip = "127.0.0.1"
            });
            serverList.severs.Add(new ServerConfig
            {
                ip = "209.151.153.172"
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

            var selectedServer = serverConnectDropdown.options[serverConnectDropdown.value].text;
            
            PlayerPrefs.SetString("LastSelectedServer", selectedServer);

            // launch...

            var parametersObject = ServerConnectionParametersObject.Instance;
            parametersObject.parameters = new ServerConnectionParameters
            {
                ip = selectedServer,
                port = 9000
            };
            DontDestroyOnLoad(parametersObject.gameObject);

            SceneManager.LoadScene("ClientScene");
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