using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Development;
using NaiveNetworkGame.Client.Systems;
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

        public Button startLocalServerButton;
        
        public InputField newServerInput;

        public Button newServerButton;

        public Button deleteServersButton;

        public ChangelogWindow changelogWindow;

        public Text ipText;
        
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
            serverConnectButton.onClick.AddListener(delegate { OnServerConnect(false); });
            startLocalServerButton.onClick.AddListener(delegate { OnServerConnect(true); });
            deleteServersButton.onClick.AddListener(OnDeleteServers);

            newServerInput.contentType = InputField.ContentType.Custom;
            newServerInput.onValidateInput += OnValidateValidIpAddress;

            var autoOpenChangelog = PlayerPrefs.GetInt("NaiveNetworkGame.ChangelogAutoOpen", 1);
            if (autoOpenChangelog == 1)
            {
                changelogWindow.Open();
            }

            ipText.text = GetLocalIPAddress();
        }
        
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
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

        private void OnServerConnect(bool startServer)
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

            if (startServer)
            {
                SceneManager.LoadScene("ServerScene", LoadSceneMode.Single);
                SceneManager.LoadScene("ClientScene", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene("ClientScene", LoadSceneMode.Single);
            }
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