using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    using Newtonsoft.Json;
    using SpaceWars;
    using System;
    using System.Collections.Generic;

    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "alive")]
        private bool alive;
        [JsonProperty(PropertyName = "proj")]
        private int ID;
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;
        private static int nextProjectileID;
        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;
        [JsonProperty(PropertyName = "owner")]
        private int owner;

        public Projectile()
        {
            this.ID = -1;
            this.owner = -1;
            this.location = null;
            this.orientation = null;
            this.alive = false;
        }

        public Projectile(Vector2D loc, Vector2D dir, int _owner)
        {
            this.ID = nextProjectileID++;
            this.owner = _owner;
            this.location = new Vector2D(loc);
            this.orientation = new Vector2D(dir);
            this.alive = true;
        }

        public void Die()
        {
            this.alive = false;
        }

        public int GetID() =>
            this.ID;

        public Vector2D GetLocation() =>
            this.location;

        public Vector2D GetOrientation() =>
            this.orientation;

        public int GetOwner() =>
            this.owner;

        public bool IsAlive() =>
            this.alive;

        public override string ToString() =>
            JsonConvert.SerializeObject(this);

        public void Update(float time, IEnumerable<Star> stars)
        {
            float num = 15f;
            this.location += (Vector2D)(this.orientation * num);
            foreach (Star star in stars)
            {
                if (((this.location - star.GetLocation())).Length() < 35.0)
                {
                    this.Die();
                    break;
                }
            }
        }
    }
}

