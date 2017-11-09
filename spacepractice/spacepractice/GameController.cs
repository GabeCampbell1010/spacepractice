using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Model;
using Network;

namespace SpaceWars
{
    public delegate void ConnectedToServerEventHandler(World w, int playerID);
    public delegate void FrameTickEventHandler();
    public delegate void NewShipEventHandler(Ship s);
    public delegate void ShipDiedEventHandler(Ship s);

    public class GameController
    {
        private bool firing;
        private int playerID;
        private Socket server;
        private bool thrusting;
        private int turning;
        private World world;

        [field: CompilerGenerated]
        private event FrameTickEventHandler FrameTick;

        [field: CompilerGenerated]
        private event ConnectedToServerEventHandler GameConnected;

        [field: CompilerGenerated]
        private event NewShipEventHandler NewShip;

        [field: CompilerGenerated]
        private event ShipDiedEventHandler ShipDied;

        public int GetPlayerID() =>
            this.playerID;

        public World GetWorld() =>
            this.world;

        public void HandleKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (this.turning != 0)
                    {
                        break;
                    }
                    this.turning = 4;
                    return;

                case Keys.Up:
                    this.thrusting = true;
                    return;

                case Keys.Right:
                    if (this.turning != 0)
                    {
                        break;
                    }
                    this.turning = 2;
                    return;

                case Keys.Space:
                    this.firing = true;
                    break;

                default:
                    return;
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (this.turning != 4)
                    {
                        break;
                    }
                    this.turning = 0;
                    return;

                case Keys.Up:
                    this.thrusting = false;
                    return;

                case Keys.Right:
                    if (this.turning != 2)
                    {
                        break;
                    }
                    this.turning = 0;
                    return;

                case Keys.Space:
                    this.firing = false;
                    break;

                default:
                    return;
            }
        }

        public void HandleMovementControlls()
        {
            if (((this.turning != 0) || this.thrusting) || this.firing)
            {
                StringBuilder builder = new StringBuilder();
                if (this.turning == 2)
                {
                    builder.Append("R");
                }
                else if (this.turning == 4)
                {
                    builder.Append("L");
                }
                if (this.thrusting)
                {
                    builder.Append("T");
                }
                if (this.firing)
                {
                    builder.Append("F");
                }
                Networking.Send(this.server, "(" + builder.ToString() + ")\n");
            }
        }

        public void ReceiveData(SocketState state)
        {
            StringBuilder sb = state.sb;
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            try
            {
                char[] separator = new char[] { '\n' };
                string[] strArray = sb.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int length = strArray.Length;
                if (sb.ToString()[sb.Length - 1] != '\n')
                {
                    length--;
                }
                List<Ship> list = new List<Ship>();
                List<Ship> list2 = new List<Ship>();
                for (int i = 0; i < length; i++)
                {
                    string json = strArray[i];
                    if ((json[0] == '{') && (json[json.Length - 1] == '}'))
                    {
                        Ship item = null;
                        Projectile projectile = null;
                        Star star = null;
                        JObject obj1 = JObject.Parse(json);
                        JToken token = obj1["ship"];
                        JToken token2 = obj1["proj"];
                        if (token != null)
                        {
                            item = JsonConvert.DeserializeObject<Ship>(json, settings);
                        }
                        if (token2 != null)
                        {
                            projectile = JsonConvert.DeserializeObject<Projectile>(json, settings);
                        }
                        if (obj1["star"] != null)
                        {
                            star = JsonConvert.DeserializeObject<Star>(json, settings);
                        }
                        World world = this.world;
                        lock (world)
                        {
                            if (item != null)
                            {
                                if (this.world.GetShips().ContainsKey(item.GetID()))
                                {
                                    if (this.world.GetShips()[item.GetID()].Alive && !item.Alive)
                                    {
                                        list2.Add(item);
                                    }
                                }
                                else
                                {
                                    list.Add(item);
                                }
                                this.world.GetShips()[item.GetID()] = item;
                            }
                            if (projectile != null)
                            {
                                if (projectile.IsAlive())
                                {
                                    this.world.GetProjectiles()[projectile.GetID()] = projectile;
                                }
                                else if (this.world.GetProjectiles().ContainsKey(projectile.GetID()))
                                {
                                    this.world.GetProjectiles().Remove(projectile.GetID());
                                }
                            }
                            if (star != null)
                            {
                                this.world.GetStars()[star.GetID()] = star;
                            }
                        }
                        sb.Remove(0, json.Length + 1);
                    }
                }
                foreach (Ship ship2 in list2)
                {
                    this.ShipDied(ship2);
                }
                foreach (Ship ship3 in list)
                {
                    this.NewShip(ship3);
                }
                this.FrameTick();
            }
            catch (JsonReaderException)
            {
            }
            catch (Exception)
            {
            }
            state.call_me = new Action<SocketState>(this.ReceiveData);
            Networking.RequestMoreData(state);
        }

        public void ReceiveStartupInfo(SocketState state)
        {
            StringBuilder sb = state.sb;
            int size = 0;
            try
            {
                char[] separator = new char[] { '\n' };
                string[] strArray = sb.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if ((strArray.Length < 2) || ((strArray.Length == 2) && (sb.ToString()[sb.Length - 1] != '\n')))
                {
                    Networking.RequestMoreData(state);
                    return;
                }
                sb.Remove(0, strArray[0].Length + 1);
                this.playerID = int.Parse(strArray[0]);
                state.uid = this.playerID;
                sb.Remove(0, strArray[1].Length + 1);
                size = int.Parse(strArray[1]);
            }
            catch (Exception)
            {
            }
            Console.WriteLine("my player id: " + this.playerID);
            this.world = new World(size);
            this.GameConnected(this.world, this.playerID);
            state.call_me = new Action<SocketState>(this.ReceiveData);
            this.ReceiveData(state);
        }

        public void RegisterFrameTickHandler(FrameTickEventHandler h)
        {
            this.FrameTick += h;
        }

        public void RegisterGameConnectedEventHandler(ConnectedToServerEventHandler h)
        {
            this.GameConnected += h;
        }

        public void RegisterNewShipEventHandler(NewShipEventHandler h)
        {
            this.NewShip += h;
        }

        public void RegisterShipDiedEventHandler(ShipDiedEventHandler h)
        {
            this.ShipDied += h;
        }

        public void SetServerSocket(Socket s)
        {
            this.server = s;
        }
    }
}

