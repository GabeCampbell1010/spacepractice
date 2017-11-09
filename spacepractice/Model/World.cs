using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceWars;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Drawing;


namespace Model
{


    public class World
    {
        private IEnumerator<Color> colorPicker;
        private uint framesPerShot;
        private Dictionary<int, Projectile> projectiles;
        private Random rand;
        private int respawnRate;
        private Dictionary<int, Ship> ships;
        private Dictionary<int, Star> stars;
        private uint time;
        private int universeSize;

        public World()
        {
            this.universeSize = 750;
            this.framesPerShot = 15;
            this.ships = new Dictionary<int, Ship>();
            this.projectiles = new Dictionary<int, Projectile>();
            this.stars = new Dictionary<int, Star>();
            //this.colorPicker = this.AllColors().GetEnumerator();
            this.rand = new Random();
            this.time = 0;
        }

        public World(int size) : this()
        {
            this.universeSize = size;
        }

        public World(int size, IEnumerable<Star> _stars, int respawn, uint fire) : this()
        {
            this.universeSize = size;
            this.respawnRate = respawn;
            this.framesPerShot = fire;
            foreach (Star star in _stars)
            {
                this.stars[star.GetID()] = star;
            }
        }

        public Ship addRandomShip(string name)
        {
            Vector2D position = this.RandomLocation();
            Vector2D velocity = new Vector2D(0.0, 0.0);
            Vector2D direction = new Vector2D(0.0, -1.0);
            return this.addShip(name, position, velocity, direction);
        }

        public Ship addShip(string name, Vector2D position, Vector2D velocity, Vector2D direction)
        {
            Ship ship = new Ship(name, position, velocity, direction, this.framesPerShot);
            this.ships.Add(ship.GetID(), ship);
            return ship;
        }

        //[IteratorStateMachine(typeof(< AllColors > d__23))]
        //private IEnumerable<Color> AllColors()
        //{
        //    yield return Color.FromArgb(250, 0xa2, 0x1b);
        //    yield return Color.FromArgb(0x5d, 0xbb, 0x4d);
        //    yield return Color.FromArgb(0xed, 0x1b, 0x51);
        //    yield return Color.FromArgb(0xab, 0x52, 0x36);
        //    yield return Color.FromArgb(0x51, 0xa6, 220);
        //    yield return Color.FromArgb(0x1c, 0x2b, 0x53);
        //    this.< rand > 5__1 = new Random();
        //    while (true)
        //    {
        //        yield return Color.FromArgb(this.< rand > 5__1.Next(0, 0xff), this.< rand > 5__1.Next(0, 0xff), this.< rand > 5__1.Next(0, 0xff));
        //    }
        //}

        public void Cleanup()
        {
            foreach (int num in new List<int>(this.ships.Keys))
            {
                if (!this.ships[num].IsActive())
                {
                    this.ships.Remove(num);
                }
            }
            foreach (int num2 in new List<int>(this.projectiles.Keys))
            {
                if (!this.projectiles[num2].IsAlive())
                {
                    this.projectiles.Remove(num2);
                }
            }
        }

        public Dictionary<int, Projectile> GetProjectiles() =>
            this.projectiles;

        public Dictionary<int, Ship> GetShips() =>
            this.ships;

        public int GetSize() =>
            this.universeSize;

        public Dictionary<int, Star> GetStars() =>
            this.stars;

        public uint GetTime() =>
            this.time;

        public void processCommand(int playerID, string cmd)
        {
            Projectile projectile;
            if (cmd.Contains("F") && this.GetShips()[playerID].Fire(this.time, out projectile))
            {
                this.projectiles.Add(projectile.GetID(), projectile);
            }
            this.GetShips()[playerID].ProcessCommand(cmd);
        }

        public Vector2D RandomLocation()
        {
            Vector2D vectord;
            bool flag;
            do
            {
                float num = (((float)this.rand.NextDouble()) * 2f) - 1f;
                vectord = new Vector2D((double)((((float)this.rand.NextDouble()) * 2f) - 1f), (double)num);
                vectord *= this.universeSize / 2;
                flag = true;
                foreach (Star star in this.stars.Values)
                {
                    if (((vectord - star.GetLocation())).Length() < 35.0)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            while (!flag);
            return vectord;
        }

        public void Update()
        {
            int num = this.universeSize / 2;
            foreach (Ship ship in this.ships.Values)
            {
                if (!ship.Alive)
                {
                    if ((this.time - ship.GetLastDeath()) >= this.respawnRate)
                    {
                        ship.Respawn(this.RandomLocation());
                    }
                }
                else
                {
                    ship.Update(this.stars.Values, this.time);
                    if ((ship.GetLocation().GetX() > num) || (ship.GetLocation().GetX() < -num))
                    {
                        ship.WrapAroundX();
                    }
                    if ((ship.GetLocation().GetY() > num) || (ship.GetLocation().GetY() < -num))
                    {
                        ship.WrapAroundY();
                    }
                }
            }
            foreach (Projectile projectile in this.projectiles.Values)
            {
                projectile.Update((float)this.time, this.stars.Values);
                if (projectile.IsAlive())
                {
                    if (projectile.GetLocation().Length() > Math.Sqrt(2.0 * Math.Pow(((double)this.universeSize) / 2.0, 2.0)))
                    {
                        projectile.Die();
                    }
                    foreach (Ship ship2 in this.ships.Values)
                    {
                        if ((ship2.Alive && (projectile.GetOwner() != ship2.GetID())) && (((projectile.GetLocation() - ship2.GetLocation())).Length() < 20.0))
                        {
                            projectile.Die();
                            ship2.Hit(this.time);
                            if (!ship2.Alive && this.ships.ContainsKey(projectile.GetOwner()))
                            {
                                this.ships[projectile.GetOwner()].IncreaseScore();
                            }
                            break;
                        }
                    }
                }
            }
            this.time++;
        }

        ////[CompilerGenerated]
        //private sealed class AllColors : IEnumerable<Color>, IEnumerable, IEnumerator<Color>, IDisposable, IEnumerator
        //{
        //    private int state;
        //    private Color current;
        //    private int initialThreadId;
        //    private Random rando;

        //    //[DebuggerHidden]
        //public AllColors(int state)
        //{
        //    this.state = state;
        //    this.initialThreadId = Environment.CurrentManagedThreadId;
        //}

        //private bool MoveNext()
        //{
        //    switch (this.state)
        //        {
        //            case 0:
        //                this.state = -1;
        //        this.current = Color.FromArgb(250, 0xa2, 0x1b);
        //        this.state = 1;
        //        return true;

        //            case 1:
        //                this.state = -1;
        //        this.current = Color.FromArgb(0x5d, 0xbb, 0x4d);
        //        this.state = 2;
        //        return true;

        //            case 2:
        //                this.state = -1;
        //        this.current = Color.FromArgb(0xed, 0x1b, 0x51);
        //        this.state = 3;
        //        return true;

        //            case 3:
        //                this.state = -1;
        //        this.current = Color.FromArgb(0xab, 0x52, 0x36);
        //        this.state = 4;
        //        return true;

        //            case 4:
        //                this.state = -1;
        //        this.current = Color.FromArgb(0x51, 0xa6, 220);
        //        this.state = 5;
        //        return true;

        //            case 5:
        //                this.state = -1;
        //        this.current = Color.FromArgb(0x1c, 0x2b, 0x53);
        //        this.state = 6;
        //        return true;

        //            case 6:
        //                this.state = -1;
        //        this.rando = new Random();
        //        break;

        //            case 7:
        //                this.state = -1;
        //        break;

        //        default:
        //                return false;
        //    }
        //    this.current = Color.FromArgb(this.rando.Next(0, 0xff), this.rando.Next(0, 0xff), this.rando.Next(0, 0xff));
        //    this.state = 7;
        //    return true;
        //}

        ////[DebuggerHidden]
        //IEnumerator<Color> IEnumerable<Color>.GetEnumerator()
        //{
        //    if ((this.state == -2) && (this.initialThreadId == Environment.CurrentManagedThreadId))
        //        {
        //        this.state = 0;
        //        return this;
        //    }
        //    return new World.AllColors(0);
        //}

        ////[DebuggerHidden]
        //IEnumerator IEnumerable.GetEnumerator() =>
        //    this.System.Collections.Generic.IEnumerable<System.Drawing.Color>.GetEnumerator();

        //[DebuggerHidden]
        //void IEnumerator.Reset()
        //{
        //    throw new NotSupportedException();
        //}

        //[DebuggerHidden]
        //void IDisposable.Dispose()
        //{
        //}

        //    bool IEnumerator.MoveNext()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    Color IEnumerator<Color>.Current =>
        //    this.current;

        //    object IEnumerator.Current =>
        //        this.current;
        //}
}
}

