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

    [JsonObject(MemberSerialization.OptIn)]
    public class Star
    {
        [JsonProperty(PropertyName = "star")]
        private int ID;
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;
        [JsonProperty(PropertyName = "mass")]
        private double mass;
        private static int nextStarID;

        public Star()
        {
            this.ID = -1;
            this.mass = 0.0;
            this.location = null;
        }

        public Star(Vector2D loc, double m)
        {
            this.ID = nextStarID++;
            this.location = new Vector2D(loc);
            this.mass = m;
        }

        public int GetID() =>
            this.ID;

        public Vector2D GetLocation() =>
            this.location;

        public double GetMass() =>
            this.mass;

        public override string ToString() =>
            JsonConvert.SerializeObject(this);
    }

}
