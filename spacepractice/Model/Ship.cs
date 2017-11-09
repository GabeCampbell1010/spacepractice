using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SpaceWars;

namespace Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CommandsRequest
    {
        private int turning;
        private bool thrusting;
        private bool firing;
        public int requestTurning
        {
            get =>
                this.turning;
            set
            {
                if ((value != 4) && (value != 2))
                {
                    this.turning = 0;
                }
                else
                {
                    this.turning = value;
                }
            }
        }
        public bool requestThrusting
        {
            get =>
                this.thrusting;
            set
            {
                this.thrusting = value;
            }
        }
        public bool requestFiring
        {
            get =>
                this.firing;
            set
            {
                this.firing = value;
            }
        }
        public void Clear()
        {
            this.requestTurning = 0;
            this.requestThrusting = false;
            this.requestFiring = false;
        }
    }



    [JsonObject(MemberSerialization.OptIn)]
    public class Ship
    {
        [JsonProperty(PropertyName = "thrust")]
        private bool accelerating;
        private bool active;
        private CommandsRequest cmdReqs;
        private uint fireRate;
        [JsonProperty(PropertyName = "hp")]
        private int hitPoints;
        [JsonProperty(PropertyName = "ship")]
        private int ID;
        private uint lastDeath;
        private uint lastFired;
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;
        [JsonProperty(PropertyName = "name")]
        private string name;
        private static int nextShipID;
        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;
        [JsonProperty(PropertyName = "score")]
        private int score;
        private Vector2D thrust;
        private Vector2D velocity;

        public Ship()
        {
            this.hitPoints = 5;
            this.active = true;
            this.thrust = new Vector2D(0.0, 0.0);
            this.ID = 0;
            this.location = new Vector2D(-1.0, -1.0);
            this.velocity = new Vector2D(0.0, 0.0);
            this.lastFired = 0;
            this.fireRate = 15;
            this.hitPoints = 0;
            this.score = 0;
            this.cmdReqs.Clear();
        }

        public Ship(string _name, Vector2D p, Vector2D v, Vector2D d, uint fRate)
        {
            this.hitPoints = 5;
            this.active = true;
            this.thrust = new Vector2D(0.0, 0.0);
            this.ID = nextShipID++;
            this.name = _name;
            this.location = new Vector2D(p);
            this.velocity = new Vector2D(v);
            this.orientation = new Vector2D(d);
            this.fireRate = 15;
            this.cmdReqs.Clear();
            this.hitPoints = 5;
            this.score = 0;
            this.fireRate = fRate;
        }

        private void applyCommandRequests()
        {
            if (this.cmdReqs.requestTurning == 4)
            {
                this.orientation.Rotate(-2.0);
            }
            if (this.cmdReqs.requestTurning == 2)
            {
                this.orientation.Rotate(2.0);
            }
            if (this.cmdReqs.requestThrusting)
            {
                this.accelerating = true;
                this.thrust = (Vector2D)(this.orientation * 0.08);
            }
            else
            {
                this.accelerating = false;
            }
            this.cmdReqs.Clear();
        }

        public void Die(uint time)
        {
            this.hitPoints = 0;
            this.lastDeath = time;
        }

        public void Discontinue(uint time)
        {
            if (this.Alive)
            {
                this.Die(time);
            }
            this.active = false;
        }

        public bool Fire(uint time, out Projectile p)
        {
            p = null;
            if (!this.Alive)
            {
                return false;
            }
            if ((time - this.lastFired) < this.fireRate)
            {
                return false;
            }
            this.lastFired = time;
            p = new Projectile(this.location, this.orientation, this.ID);
            return true;
        }

        public bool GetAccelerating() =>
            this.accelerating;

        public int GetHitPoints() =>
            this.hitPoints;

        public int GetID() =>
            this.ID;

        public uint GetLastDeath() =>
            this.lastDeath;

        public Vector2D GetLocation() =>
            this.location;

        public string GetName() =>
            this.name;

        public Vector2D GetOrientation() =>
            this.orientation;

        public int GetScore() =>
            this.score;

        public void Hit(uint time)
        {
            this.hitPoints--;
            if (this.hitPoints == 0)
            {
                this.Die(time);
            }
        }

        public void IncreaseScore()
        {
            this.score++;
        }

        public bool IsActive() =>
            this.active;

        public void ProcessCommand(string cmd)
        {
            foreach (char ch in cmd)
            {
                switch (ch)
                {
                    case 'R':
                        this.cmdReqs.requestTurning = 2;
                        break;

                    case 'T':
                        this.cmdReqs.requestThrusting = true;
                        break;

                    case 'F':
                        this.cmdReqs.requestFiring = true;
                        break;

                    case 'L':
                        this.cmdReqs.requestTurning = 4;
                        break;
                }
            }
        }

        public void Respawn(Vector2D newLocation)
        {
            if (this.active)
            {
                this.hitPoints = 5;
                this.location = new Vector2D(newLocation);
                this.velocity = new Vector2D(0.0, 0.0);
                this.orientation = new Vector2D(0.0, -1.0);
            }
        }

        public override string ToString() =>
            JsonConvert.SerializeObject(this);

        public void Update(IEnumerable<Star> stars, uint time)
        {
            this.applyCommandRequests();
            Vector2D vectord = new Vector2D(this.thrust);
            this.thrust = new Vector2D(0.0, 0.0);
            foreach (Star local1 in stars)
            {
                double mass = local1.GetMass();
                Vector2D vectord2 = local1.GetLocation() - this.location;
                if (vectord2.Length() < 35.0)
                {
                    this.Die(time);
                    return;
                }
                vectord2.Normalize();
                vectord2 = (Vector2D)(vectord2 * mass);
                vectord += vectord2;
            }
            this.velocity += vectord;
            this.location += this.velocity;
        }

        public void WrapAroundX()
        {
            this.location = new Vector2D(this.location.GetX() * -1.0, this.location.GetY());
        }

        public void WrapAroundY()
        {
            this.location = new Vector2D(this.location.GetX(), this.location.GetY() * -1.0);
        }

        public bool Alive =>
            (this.hitPoints > 0);
    }

}
