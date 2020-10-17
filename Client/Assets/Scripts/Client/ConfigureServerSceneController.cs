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
            public string name;
            public string ip;
            public ushort port;
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

        // public Text ipText;
        public Dropdown localAddressesDropdown;
        
        private ServerList serverList;

        private void AddDefaultServers(ServerList list)
        {
            list.severs.Add(new ServerConfig
            {
                name = "Localhost",
                ip = "127.0.0.1", 
                port = 9000,
            });
            list.severs.Add(new ServerConfig
            {
                name = "Server1",
                ip = "209.151.153.172",
                port = 9000
            });
            list.severs.Add(new ServerConfig
            {
                name = "Server2",
                ip = "209.151.153.172",
                port = 9001
            });
            list.severs.Add(new ServerConfig
            {
                name = "Server3",
                ip = "209.151.153.172",
                port = 9002
            });
            list.severs.Add(new ServerConfig
            {
                name = "Server4",
                ip = "209.151.153.172",
                port = 9003
            });
            list.severs.Add(new ServerConfig
            {
                name = "Server5",
                ip = "209.151.153.172",
                port = 9004
            });
        }
        
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
                serverList.severs.Clear();
            } 
            
            if (savedServersJson == null || serverList.severs.Count == 0)
            {
                serverList = new ServerList();
                AddDefaultServers(serverList);
            }

            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));

            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.name
            }).ToList();

            var lastSelectedServer = PlayerPrefs.GetString("LastSelectedServer", null);

            var index = serverConnectDropdown.options.FindIndex(o => o.text.Equals(lastSelectedServer));
            if (index >= 0)
                serverConnectDropdown.value = index;

            newServerButton.onClick.AddListener(OnNewServerAdded);
            serverConnectButton.onClick.AddListener(OnServerConnect);
            startLocalServerButton.onClick.AddListener(OnLocalServer);
            deleteServersButton.onClick.AddListener(OnDeleteServers);

            newServerInput.contentType = InputField.ContentType.Custom;
            newServerInput.onValidateInput += OnValidateValidIpAddress;

            var autoOpenChangelog = PlayerPrefs.GetInt("NaiveNetworkGame.ChangelogAutoOpen", 1);
            if (autoOpenChangelog == 1)
            {
                changelogWindow.Open();
            }

            // TODO: dropdown of assigned ip addresses
            // ipText.text = GetLocalIPAddress();
            var addresses = GetLocalIPAddresses();
            localAddressesDropdown.options = addresses.Select(a => new Dropdown.OptionData(a)).ToList();
        }
        
        public List<string> GetLocalIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var list = new List<string>();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    list.Add(ip.ToString());
                    // if (ip.ToString().StartsWith("192"))
                    //     return ip.ToString();
                }
            }
            return list;
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
            AddDefaultServers(serverList);
            
            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));
            
            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.name
            }).ToList();
        }

        private void OnServerConnect()
        {
            // load the other scene with parameters...

            var selectedServer = serverList.severs[serverConnectDropdown.value];
            
            // var selectedServer = serverConnectDropdown.options[serverConnectDropdown.value].text;
            
            PlayerPrefs.SetString("LastSelectedServer", selectedServer.name);

            // launch...

            var parametersObject = ServerConnectionParametersObject.Instance;
            parametersObject.parameters = new ServerConnectionParameters
            {
                ip = selectedServer.ip,
                port = selectedServer.port
            };
            
            DontDestroyOnLoad(parametersObject.gameObject);

            SceneManager.LoadScene("ClientScene", LoadSceneMode.Single);
        }
        
        private void OnLocalServer()
        {
            var selectedAddress = localAddressesDropdown.options[localAddressesDropdown.value].text;
            
            var parametersObject = ServerConnectionParametersObject.Instance;
            parametersObject.parameters = new ServerConnectionParameters
            {
                // or we could use localhost or 127.0.0.1
                ip = selectedAddress,
                port = 9000
            };
            
            DontDestroyOnLoad(parametersObject.gameObject);

            SceneManager.LoadScene("ServerScene", LoadSceneMode.Single);
            SceneManager.LoadScene("ClientScene", LoadSceneMode.Additive);
        }

        private void OnNewServerAdded()
        {
            var newServerIp = newServerInput.text;
            serverList.severs.Add(new ServerConfig
            {
                name = newServerIp,
                ip = newServerIp,
                port = 9000
            });
            
            PlayerPrefs.SetString("SavedServers", JsonUtility.ToJson(serverList));

            serverConnectDropdown.options = serverList.severs.Select(s => new Dropdown.OptionData
            {
                text = s.name
            }).ToList();
            
            newServerInput.text = string.Empty;
        }
    }
}