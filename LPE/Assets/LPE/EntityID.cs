using System;


namespace LPE {
    public readonly struct EntityID : IEquatable<EntityID> {
        public static EntityID INVALID => new EntityID(-1);

        readonly int _id;



        public EntityID(int id) {
            _id = id;
        }


        public bool IsValid() {
            return this != INVALID;
        }



        public static bool operator ==(EntityID a, EntityID b) => a._id == b._id;
        public static bool operator !=(EntityID a, EntityID b) => a._id != b._id;

        public bool Equals(EntityID other) => _id == other._id;

        public override bool Equals(object obj) => obj is EntityID other && Equals(other);
        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => $"EntityID({_id})";
    }
}